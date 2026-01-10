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
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Kiểm tra xem đã có vé được bán cho phòng này chưa
                        string checkVe = @"SELECT COUNT(*) FROM Ve v 
                                          INNER JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu
                                          WHERE sc.MaPhong = @MaPhong";
                        using (SqlCommand cmd = new SqlCommand(checkVe, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                            int veCount = (int)cmd.ExecuteScalar();
                            if (veCount > 0)
                            {
                                // Nếu đã có vé, không cho xóa mà chỉ nên ẩn đi (TrangThai = 0)
                                string softDelete = "UPDATE PhongChieu SET TrangThai = 0 WHERE MaPhong = @MaPhong";
                                using (SqlCommand cmdDelete = new SqlCommand(softDelete, conn, trans))
                                {
                                    cmdDelete.Parameters.AddWithValue("@MaPhong", maPhong);
                                    cmdDelete.ExecuteNonQuery();
                                }
                                trans.Commit();
                                return true;
                            }
                        }

                        // 2. Nếu chưa có vé, có thể xóa cứng (nhưng phải xóa ghế và suất chiếu trước)
                        // Xóa dữ liệu liên quan
                        string delGhe = "DELETE FROM GheNgoi WHERE MaPhong = @MaPhong";
                        using (SqlCommand cmd = new SqlCommand(delGhe, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                            cmd.ExecuteNonQuery();
                        }

                        string delSuat = "DELETE FROM SuatChieu WHERE MaPhong = @MaPhong";
                        using (SqlCommand cmd = new SqlCommand(delSuat, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                            cmd.ExecuteNonQuery();
                        }

                        string delPhong = "DELETE FROM PhongChieu WHERE MaPhong = @MaPhong";
                        using (SqlCommand cmd = new SqlCommand(delPhong, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                            int result = cmd.ExecuteNonQuery();
                            trans.Commit();
                            return result > 0;
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
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
