using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Service.DAO;
using Service.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebRole.Controllers
{
    public class BookController : Controller
    {
        private DataAccessLayer DAL = new DataAccessLayer();
        private CloudQueue imagesQueue;
        private static CloudBlobContainer imagesBlobContainer;

        private void InitializeStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
            
            imagesBlobContainer = blobClient.GetContainerReference("images");
            
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
            
            imagesQueue = queueClient.GetQueueReference("images");
        }

        public BookController()
        {
            InitializeStorage();
        }
        // GET: Book
        public ActionResult Index()
        {
            DAL.Open();
            var result = DAL.SelectData("SELECT * FROM Book", null);
            var model = ConvertDatatableToBook(result);
            DAL.Close();
            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            Book model = new Book();
            return View();
        }

        public async Task<ActionResult> Create(Book model, HttpPostedFileBase imageFile)
        {
            //Xu ly save hinh anh
            CloudBlockBlob imageBlob = null;
            if (imageFile != null && imageFile.ContentLength != 0)
            {
                imageBlob = await UploadAndSaveBlobAsync(imageFile);
                model.ImageUrl = imageBlob.Uri.ToString();
            }


            DAL.Open();
            DAL.Excutecommand("INSERT INTO Book(Name, Description, Active) VALUES(@p1,@p2,@p3);", new SqlParameter[] {
                new SqlParameter("p1", model.Name),
                new SqlParameter("p2", model.Description),
                new SqlParameter("p3", true)});

            DAL.Close();

            if (imageBlob != null)
            {
                var queueMessage = new CloudQueueMessage(model.Id.ToString());
                await imagesQueue.AddMessageAsync(queueMessage);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int Id)
        {
            DAL.Open();
            var result = DAL.SelectData("SELECT * FROM Book WHERE Id=@p1", new SqlParameter[]{
                new SqlParameter("p1",Id),
            });
            var listModel = ConvertDatatableToBook(result);
            var model = listModel.FirstOrDefault();
            if(model == null)
            {
                return HttpNotFound();
            }
            DAL.Close();
            return View(model);
        }

        public ActionResult Edit(Book model, HttpPostedFileBase imageFile)
        {
            DAL.Open();
            DAL.Excutecommand("UPDATE Book SET Name=@p1, Description=@p2 WHERE Id=@p3", new SqlParameter[] {
                new SqlParameter("p1", model.Name),
                new SqlParameter("p2", model.Description),
                new SqlParameter("p3", model.Id)
            });

            DAL.Close();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(int Id)
        {
            DAL.Open();
            DAL.Excutecommand("DELETE FROM Book WHERE Id=@p1", new SqlParameter[] {
                new SqlParameter("p1", Id)
            });

            DAL.Close();
            Book model = new Book();
            return View();
        }

        private IEnumerable<Book> ConvertDatatableToBook(DataTable dt)
        {
            foreach (DataRow item in dt.Rows)
            {
                yield return new Book()
                {
                    Id = Convert.ToInt32(item["Id"]),
                    Name = item["Name"].ToString(),
                    Description = item["Description"].ToString(),
                    ImageUrl = item["Id"].ToString(),
                    ThumbnailUrl = item["ThumbnailUrl"].ToString(),
                    Active = Convert.ToBoolean(item["Id"]),
                };
            }
        }

        private async Task<CloudBlockBlob> UploadAndSaveBlobAsync(HttpPostedFileBase imageFile)
        {
            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            CloudBlockBlob imageBlob = imagesBlobContainer.GetBlockBlobReference(blobName);
            using (var fileStream = imageFile.InputStream)
            {
                await imageBlob.UploadFromStreamAsync(fileStream);
            }
            return imageBlob;
        }
    }
}