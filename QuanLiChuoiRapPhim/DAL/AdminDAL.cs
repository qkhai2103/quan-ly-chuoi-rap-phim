using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class AdminDAL
    {
        // PHƯƠNG THỨC ĐĂNG NHẬP
        public DataTable GetUserByLogin(string username, string password)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    nd.MaNguoiDung, 
                    nd.TenDangNhap, 
                    nd.HoTen, 
                    nd.Email, 
                    nd.SoDienThoai, 
                    nd.VaiTro, 
                    nd.MaChiNhanh, 
                    nd.TrangThai, 
                    nd.NgayTao,
                    cn.TenChiNhanh
                FROM NguoiDung nd
                LEFT JOIN ChiNhanh cn ON nd.MaChiNhanh = cn.MaChiNhanh
                WHERE nd.TenDangNhap = @Username 
                AND nd.MatKhau = @Password
                AND nd.TrangThai = 1";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // PHƯƠNG THỨC LẤY TẤT CẢ NGƯỜI DÙNG
        public DataTable GetAllUsers()
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    nd.MaNguoiDung,
                    nd.TenDangNhap,
                    nd.HoTen,
                    nd.Email,
                    nd.SoDienThoai,
                    nd.VaiTro,
                    cn.TenChiNhanh,
                    nd.TrangThai,
                    nd.NgayTao
                FROM NguoiDung nd
                LEFT JOIN ChiNhanh cn ON nd.MaChiNhanh = cn.MaChiNhanh
                ORDER BY nd.NgayTao DESC";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // THÊM PHƯƠNG THỨC NÀY - KIỂM TRA KẾT NỐI
        public bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
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
        // Thêm phương thức cho quản lý 
        public DataTable GetAllMovies()
        {
            DataTable dt = new DataTable();
            string query = "SELECT * FROM Phim ORDER BY TenPhim";
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
    }
}