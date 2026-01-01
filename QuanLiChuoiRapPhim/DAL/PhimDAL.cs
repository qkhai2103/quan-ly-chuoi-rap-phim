using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class PhimDAL
    {
        // Lấy tất cả phim
        public DataTable GetAllPhim()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    MaPhim, TenPhim, TheLoai, ThoiLuong, DaoDien, 
                    DienVien, DoTuoi, NgayKhoiChieu, TrangThai, NgayTao, HinhAnh, MoTa
                FROM Phim
                ORDER BY NgayKhoiChieu DESC";

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

        // Thêm phim
        public bool ThemPhim(string tenPhim, string theLoai, int thoiLuong, string daoDien, 
            string dienVien, string moTa, string doTuoi, DateTime ngayKhoiChieu)
        {
            string query = @"
                INSERT INTO Phim (TenPhim, TheLoai, ThoiLuong, DaoDien, DienVien, MoTa, DoTuoi, NgayKhoiChieu, TrangThai)
                VALUES (@TenPhim, @TheLoai, @ThoiLuong, @DaoDien, @DienVien, @MoTa, @DoTuoi, @NgayKhoiChieu, 1)";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenPhim", tenPhim);
                    cmd.Parameters.AddWithValue("@TheLoai", theLoai ?? "");
                    cmd.Parameters.AddWithValue("@ThoiLuong", thoiLuong);
                    cmd.Parameters.AddWithValue("@DaoDien", daoDien ?? "");
                    cmd.Parameters.AddWithValue("@DienVien", dienVien ?? "");
                    cmd.Parameters.AddWithValue("@MoTa", moTa ?? "");
                    cmd.Parameters.AddWithValue("@DoTuoi", doTuoi ?? "P");
                    cmd.Parameters.AddWithValue("@NgayKhoiChieu", ngayKhoiChieu);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Cập nhật phim
        public bool CapNhatPhim(int maPhim, string tenPhim, string theLoai, int thoiLuong, 
            string daoDien, string dienVien, string moTa, string doTuoi, DateTime ngayKhoiChieu)
        {
            string query = @"
                UPDATE Phim 
                SET TenPhim = @TenPhim, TheLoai = @TheLoai, ThoiLuong = @ThoiLuong, 
                    DaoDien = @DaoDien, DienVien = @DienVien, MoTa = @MoTa, 
                    DoTuoi = @DoTuoi, NgayKhoiChieu = @NgayKhoiChieu
                WHERE MaPhim = @MaPhim";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhim", maPhim);
                    cmd.Parameters.AddWithValue("@TenPhim", tenPhim);
                    cmd.Parameters.AddWithValue("@TheLoai", theLoai ?? "");
                    cmd.Parameters.AddWithValue("@ThoiLuong", thoiLuong);
                    cmd.Parameters.AddWithValue("@DaoDien", daoDien ?? "");
                    cmd.Parameters.AddWithValue("@DienVien", dienVien ?? "");
                    cmd.Parameters.AddWithValue("@MoTa", moTa ?? "");
                    cmd.Parameters.AddWithValue("@DoTuoi", doTuoi ?? "P");
                    cmd.Parameters.AddWithValue("@NgayKhoiChieu", ngayKhoiChieu);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Xóa phim
        public bool XoaPhim(int maPhim)
        {
            string query = "DELETE FROM Phim WHERE MaPhim = @MaPhim";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhim", maPhim);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Tìm kiếm phim
        public DataTable TimKiemPhim(string tuKhoa)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    MaPhim, TenPhim, TheLoai, ThoiLuong, DaoDien, 
                    DienVien, DoTuoi, NgayKhoiChieu, TrangThai, NgayTao, HinhAnh, MoTa
                FROM Phim
                WHERE TenPhim LIKE @TuKhoa OR TheLoai LIKE @TuKhoa OR DaoDien LIKE @TuKhoa
                ORDER BY NgayKhoiChieu DESC";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TuKhoa", "%" + tuKhoa + "%");
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
    }
}
