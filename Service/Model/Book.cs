using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Service.Model
{
    public enum Category
    {
        None,
        Advanture,
        Drama,
        Horror,
        Mystery,
        Comic,
        [Display(Name = "Science fiction")]
        Sciencefiction,
        Fantasy,
        Diaries,
        Travel,
        Journals
    }
    public class Book
    {
        public int Id { get; set; }
        [DisplayName("Tên sách")]
        public string Name { get; set; }
        [DisplayName("Giới thiệu")]
        public string Description { get; set; }
        public Category? Category { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool Active { get; set; }
        public string UserName { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public int[] SelectedCategories { get; set; }
    }
}
