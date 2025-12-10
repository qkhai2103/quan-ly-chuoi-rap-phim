using System;
using System.Data;
using System.Data.SqlClient;
using BCrypt.Net;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class AdminBLL
    {
        private readonly AdminDAL _adminDal;

        public AdminBLL()
        {
            _adminDal = new AdminDAL();
        }

        // 1. PHƯƠNG THỨC ĐĂNG NHẬP (BẮT BUỘC)
        public bool Login(string username, string password, out string fullName, out string errorMessage)
        {
            fullName = "";
            errorMessage = "";

            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    errorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                    return false;
                }

                // Kiểm tra với database
                DataTable userTable = _adminDal.GetUserByLogin(username, password);

                if (userTable.Rows.Count == 0)
                {
                    errorMessage = "Tài khoản hoặc mật khẩu không đúng!";
                    return false;
                }

                DataRow userRow = userTable.Rows[0];
                fullName = userRow["HoTen"].ToString();
                return true;
            }
            catch (SqlException sqlEx)
            {
                errorMessage = $"Lỗi database: {sqlEx.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi hệ thống: {ex.Message}";
                return false;
            }
        }

        // 2. PHƯƠNG THỨC GetAllUsers (BẮT BUỘC - cho UC_Admin)
        public DataTable GetAllUsers()
        {
            try
            {
                return _adminDal.GetAllUsers();
            }
            catch (Exception ex)
            {
                // Tạo DataTable rỗng nếu có lỗi
                DataTable dt = new DataTable();
                dt.Columns.Add("Error", typeof(string));
                dt.Rows.Add($"Lỗi: {ex.Message}");
                return dt;
            }
        }

        // 3. PHƯƠNG THỨC TestDatabaseConnection (BẮT BUỘC)
        public bool TestDatabaseConnection()
        {
            try
            {
                return _adminDal.TestConnection();
            }
            catch
            {
                return false;
            }
        }

        // 4. PHƯƠNG THỨC MÃ HÓA MẬT KHẨU
        public static string HashPassword(string plainPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.HashPassword(plainPassword);
            }
            catch
            {
                return plainPassword;
            }
        }
    }
}