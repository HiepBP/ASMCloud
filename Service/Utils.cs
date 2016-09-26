using Service.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class Utils
    {
        public static IEnumerable<Book> ConvertDatatableToBook(DataTable dt)
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
                };
            }
        }

        public static IEnumerable<AspNetUser> ConvertDatatableToAspNetUsers(DataTable dt)
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
