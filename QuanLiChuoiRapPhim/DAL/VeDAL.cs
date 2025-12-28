using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class VeDAL
    {
        public DataTable LayTatCaVe()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    v.MaVe, p.TenPhim, g.SoGhe, v.GiaVe, v.TrangThai, v.NgayDat, v.MaVeCode
                FROM Ve v
                INNER JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                INNER JOIN GheNgoi g ON v.MaGhe = g.MaGhe
                ORDER BY v.NgayDat DESC";

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

        public DataTable LayVeTheoSuatChieu(int maSuatChieu)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    v.MaVe, g.SoGhe, v.GiaVe, v.DoiTuongKhachHang, v.TrangThai, v.MaVeCode
                FROM Ve v
                INNER JOIN GheNgoi g ON v.MaGhe = g.MaGhe
                WHERE v.MaSuatChieu = @MaSuatChieu
                ORDER BY g.SoHang, g.SoGhe";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSuatChieu", maSuatChieu);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public bool BanVe(int maSuatChieu, int maGhe, int? maKhachHang, string doiTuongKhachHang, decimal giaVe)
        {
            string query = @"
                INSERT INTO Ve (MaSuatChieu, MaGhe, MaKhachHang, DoiTuongKhachHang, GiaVe, TrangThai, MaVeCode)
                VALUES (@MaSuatChieu, @MaGhe, @MaKhachHang, @DoiTuongKhachHang, @GiaVe, N'DaBan', @MaVeCode)";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    string maVeCode = $"VE{DateTime.Now:yyyyMMddHHmmss}{maSuatChieu}{maGhe}";
                    cmd.Parameters.AddWithValue("@MaSuatChieu", maSuatChieu);
                    cmd.Parameters.AddWithValue("@MaGhe", maGhe);
                    cmd.Parameters.AddWithValue("@MaKhachHang", (object)maKhachHang ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DoiTuongKhachHang", doiTuongKhachHang);
                    cmd.Parameters.AddWithValue("@GiaVe", giaVe);
                    cmd.Parameters.AddWithValue("@MaVeCode", maVeCode);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool HuyVe(int maVe)
        {
            string query = @"
                UPDATE Ve 
                SET TrangThai = N'DaHuy'
                WHERE MaVe = @MaVe";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaVe", maVe);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public DataTable ThongKeVeTheoPhim()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT TOP 10
                    p.MaPhim,
                    p.TenPhim,
                    COUNT(v.MaVe) AS SoVeBan,
                    SUM(v.GiaVe) AS DoanhThu
                FROM Phim p
                INNER JOIN SuatChieu sc ON p.MaPhim = sc.MaPhim
                INNER JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu
                WHERE v.TrangThai = N'DaBan'
                GROUP BY p.MaPhim, p.TenPhim
                ORDER BY SoVeBan DESC";

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
