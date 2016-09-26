using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Service.Model;

namespace Service
{
    public class BookManagementContext : DbContext
    {
        public BookManagementContext() : base("name=BookManagementEntites")
        {

        }

        public BookManagementContext(string connectionString) : base(connectionString)
        {

        }
    }
}
