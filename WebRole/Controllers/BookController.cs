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
            //imagesBlobContainer.CreateIfNotExists();

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
            
            imagesQueue = queueClient.GetQueueReference("images");
            //imagesQueue.CreateIfNotExists();
        }

        public BookController()
        {
            InitializeStorage();
        }
        // GET: Book
        public ActionResult Index()
        {
            DAL.Open();
            var result = DAL.SelectData("SELECT Book.Id, Book.Name, Book.Description, Book.ImageUrl, Book.ThumbnailUrl, Book.Active, AspNetUsers.UserName FROM Book LEFT JOIN AspNetUsers ON Book.AccountId = AspNetUsers.Id", null);
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
            var Username = System.Web.HttpContext.Current.User.Identity.Name;
            var users = DAL.SelectData("SELECT * FROM AspNetUsers WHERE Username=@p1", new SqlParameter[]
            {
                new SqlParameter("p1", Username),
            });
            var currentUser = ConvertDatatableToAspNetUsers(users).FirstOrDefault();

            DAL.Excutecommand("INSERT INTO Book(Name, Description, Active,AccountId, ImageUrl) VALUES(@p1,@p2,@p3,@p4,@p5);", new SqlParameter[] {
                new SqlParameter("p1", model.Name),
                new SqlParameter("p2", model.Description),
                new SqlParameter("p3", true),
                new SqlParameter("p4", currentUser.Id),
                new SqlParameter("p5", model.ImageUrl)
            });

            var result = DAL.SelectData("SELECT * FROM Book", null);
            var newBook = result.Rows[result.Rows.Count - 1];

            DAL.Close();

            if (imageBlob != null)
            {
                var queueMessage = new CloudQueueMessage(newBook["Id"].ToString());
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

        private IEnumerable<Book> ConvertDatatableToBook(DataTable dt)
        {
            foreach (DataRow item in dt.Rows)
            {
                yield return new Book()
                {
                    Id = Convert.ToInt32(item["Id"]),
                    Name = item["Name"].ToString(),
                    Description = item["Description"].ToString(),
                    ImageUrl = item["ImageUrl"].ToString(),
                    ThumbnailUrl = item["ThumbnailUrl"].ToString(),
                    Active = Convert.ToBoolean(item["Id"]),
                    UserName = item["UserName"].ToString(),
                };
            }
        }

        private IEnumerable<AspNetUser> ConvertDatatableToAspNetUsers(DataTable dt)
        {
            foreach (DataRow item in dt.Rows)
            {
                yield return new AspNetUser()
                {
                    Id = item["Id"].ToString(),
                    Email = item["Email"].ToString(),
                    EmailConfirmed = Convert.ToBoolean(item["EmailConfirmed"]),
                    PasswordHash = item["PasswordHash"].ToString(),
                    SecurityStamp = item["SecurityStamp"].ToString(),
                    PhoneNumber = item["PhoneNumber"].ToString(),
                    PhoneNumberConfirmed = Convert.ToBoolean(item["PhoneNumberConfirmed"]),
                    TwoFactorEnabled = Convert.ToBoolean(item["TwoFactorEnabled"]),
                    //LockoutEndDateUtc = Convert.ToDateTime(item["LockoutEndDateUtc"]),
                    LockoutEnabled = Convert.ToBoolean(item["LockoutEnabled"]),
                    UserName = item["UserName"].ToString(),
                    AccessFailedCount = Convert.ToInt32(item["AccessFailedCount"]),
                };
            }
        }
    }
}