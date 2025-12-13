using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class UserDAL
    {
        public DataTable GetUserById(int userId)
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
                WHERE nd.MaNguoiDung = @UserId";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable GetBranches()
        {
            DataTable dt = new DataTable();

            string query = "SELECT MaChiNhanh, TenChiNhanh FROM ChiNhanh WHERE TrangThai = 1 ORDER BY TenChiNhanh";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.GetConnectionString()))
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

        public DataTable GetShowsByDate(DateTime date)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    sc.MaSuatChieu,
                    p.TenPhim,
                    ph.TenPhong,
                    cn.TenChiNhanh,
                    sc.NgayChieu,
                    sc.GioChieu,
                    sc.GiaVe,
                    sc.TrangThai
                FROM SuatChieu sc
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                INNER JOIN PhongChieu ph ON sc.MaPhong = ph.MaPhong
                INNER JOIN ChiNhanh cn ON ph.MaChiNhanh = cn.MaChiNhanh
                WHERE sc.NgayChieu = @Date
                AND sc.TrangThai = N'DangChieu'
                ORDER BY sc.GioChieu";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date.Date);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }
    }
}
