using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class GiaVeDAL
    {
        public DataTable LayTatCaCauHinhGiaVe()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    MaCauHinh, LoaiGhe, LoaiNgay, DoiTuongKhachHang, GiaGoc, PhuThu, GhiChu
                FROM CauHinhGiaVe
                WHERE TrangThai = 1
                ORDER BY LoaiGhe, LoaiNgay, DoiTuongKhachHang";

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

        public bool CapNhatGiaVe(int maCauHinh, decimal giaGoc, decimal phuThu)
        {
            string query = @"
                UPDATE CauHinhGiaVe 
                SET GiaGoc = @GiaGoc, PhuThu = @PhuThu
                WHERE MaCauHinh = @MaCauHinh";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaCauHinh", maCauHinh);
                    cmd.Parameters.AddWithValue("@GiaGoc", giaGoc);
                    cmd.Parameters.AddWithValue("@PhuThu", phuThu);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public decimal TinhGiaVe(string loaiGhe, string loaiNgay, string doiTuongKhachHang)
        {
            string query = @"
                SELECT ISNULL(GiaGoc + PhuThu, 0)
                FROM CauHinhGiaVe
                WHERE LoaiGhe = @LoaiGhe 
                  AND LoaiNgay = @LoaiNgay 
                  AND DoiTuongKhachHang = @DoiTuongKhachHang
                  AND TrangThai = 1";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LoaiGhe", loaiGhe);
                    cmd.Parameters.AddWithValue("@LoaiNgay", loaiNgay);
                    cmd.Parameters.AddWithValue("@DoiTuongKhachHang", doiTuongKhachHang);

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
        }
    }
}
