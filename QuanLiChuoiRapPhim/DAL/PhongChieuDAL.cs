using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class PhongChieuDAL
    {
        public DataTable LayTatCaPhongChieu()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    pc.MaPhong, pc.TenPhong, cn.TenChiNhanh, pc.TongSoGhe, pc.TrangThai, pc.NgayTao
                FROM PhongChieu pc
                INNER JOIN ChiNhanh cn ON pc.MaChiNhanh = cn.MaChiNhanh
                ORDER BY cn.TenChiNhanh, pc.TenPhong";

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

        public bool ThemPhongChieu(string tenPhong, int maChiNhanh, int tongSoGhe)
        {
            string query = @"
                INSERT INTO PhongChieu (TenPhong, MaChiNhanh, TongSoGhe, TrangThai)
                VALUES (@TenPhong, @MaChiNhanh, @TongSoGhe, 1)";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenPhong", tenPhong);
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@TongSoGhe", tongSoGhe);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CapNhatPhongChieu(int maPhong, string tenPhong, int tongSoGhe)
        {
            string query = @"
                UPDATE PhongChieu 
                SET TenPhong = @TenPhong, TongSoGhe = @TongSoGhe
                WHERE MaPhong = @MaPhong";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                    cmd.Parameters.AddWithValue("@TenPhong", tenPhong);
                    cmd.Parameters.AddWithValue("@TongSoGhe", tongSoGhe);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool XoaPhongChieu(int maPhong)
        {
            string query = "DELETE FROM PhongChieu WHERE MaPhong = @MaPhong";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public DataTable LayGheTrong(int maSuatChieu)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT g.MaGhe, g.SoGhe, g.SoHang, g.LoaiGhe
                FROM GheNgoi g
                INNER JOIN SuatChieu sc ON g.MaPhong = sc.MaPhong
                WHERE sc.MaSuatChieu = @MaSuatChieu
                  AND g.MaGhe NOT IN (
                      SELECT MaGhe FROM Ve 
                      WHERE MaSuatChieu = @MaSuatChieu AND TrangThai = N'DaBan'
                  )
                  AND g.TrangThai = 1
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
    }
}
