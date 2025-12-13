using System;
using System.Collections.Generic;
using System.Data;
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

        public static bool TestConnection()
        {
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(ConnectionString))
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kết nối database: {ex.Message}");
                return false;
            }
        }
    }
}