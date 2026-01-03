using QuanLiChuoiRapPhim.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class UserBLL
    {
        private UserDAL userDAL = new UserDAL();

        public DataRow GetUserById(int userId)
        {
            DataTable dt = userDAL.GetUserById(userId);

            if (dt.Rows.Count == 0)
            {
                throw new ArgumentException("Không tìm thấy người dùng");
            }

            return dt.Rows[0];
        }

        public DataTable GetBranches()
        {
            return userDAL.GetBranches();
        }

        public string GetUserRole(int userId)
        {
            DataRow user = GetUserById(userId);
            return user["VaiTro"].ToString();
        }

        public bool IsAdmin(int userId)
        {
            return GetUserRole(userId) == "Admin";
        }

        public bool IsManager(int userId)
        {
            return GetUserRole(userId) == "QuanLy";
        }

        public bool IsStaff(int userId)
        {
            return GetUserRole(userId) == "NhanVien";
        }

        public DataTable GetShowsByDate(DateTime date)
        {
            return userDAL.GetShowsByDate(date);
        }

        public DataTable GetTodayShows()
        {
            return GetShowsByDate(DateTime.Today);
        }

        /// <summary>
        /// Đổi mật khẩu người dùng
        /// </summary>
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    
                    // Kiểm tra mật khẩu cũ
                    string checkQuery = "SELECT MatKhau FROM NguoiDung WHERE MaNguoiDung = @UserId";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        object result = checkCmd.ExecuteScalar();
                        
                        if (result == null)
                            return false;

                        string storedPassword = result.ToString();
                        
                        // So sánh mật khẩu (có thể là hash hoặc plain text tùy hệ thống)
                        string hashedOld = HashPassword(oldPassword);
                        
                        // Kiểm tra cả 2 trường hợp: mật khẩu đã hash hoặc plain text
                        if (storedPassword != oldPassword && storedPassword != hashedOld)
                            return false;
                    }

                    // Cập nhật mật khẩu mới
                    string updateQuery = "UPDATE NguoiDung SET MatKhau = @NewPassword WHERE MaNguoiDung = @UserId";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        // Lưu mật khẩu mới (có thể hash tùy chính sách)
                        updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                        updateCmd.Parameters.AddWithValue("@UserId", userId);
                        
                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Hash mật khẩu bằng SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
