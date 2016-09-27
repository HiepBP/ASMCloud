using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class BookCategoryMapping
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int BookCategoryId { get; set; }
        public bool Active { get; set; }
    }
}
