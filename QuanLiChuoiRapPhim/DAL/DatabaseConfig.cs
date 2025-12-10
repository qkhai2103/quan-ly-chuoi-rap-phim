using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class DatabaseConfig
    {
        public static string GetConnectionString()
        {
                return @"Data Source=SQL8006.site4now.net;
                        Initial Catalog=db_ac1f20_khaideptrai;
                        User Id=db_ac1f20_khaideptrai_admin;
                        Password=admin123";
            
        }

        public static string ConnectionString
        {
            get { return GetConnectionString(); }
        }
    }
}