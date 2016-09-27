using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class Comment
    {
        public int CommentId { get; set; }
        public String UserID { get; set; }
        public int BookID { get; set; }
        public string CommentContent { get; set; }
        public String Username { get; set; }
    }
}
