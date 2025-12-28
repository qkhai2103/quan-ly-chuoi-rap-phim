using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class ChiNhanhDAL
    {
        public DataTable LayTatCaChiNhanh()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    MaChiNhanh, TenChiNhanh, DiaChi, SoDienThoai, TrangThai, NgayTao
                FROM ChiNhanh
                ORDER BY TenChiNhanh";

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

        public bool ThemChiNhanh(string tenChiNhanh, string diaChi, string soDienThoai)
        {
            string query = @"
                INSERT INTO ChiNhanh (TenChiNhanh, DiaChi, SoDienThoai, TrangThai)
                VALUES (@TenChiNhanh, @DiaChi, @SoDienThoai, 1)";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenChiNhanh", tenChiNhanh);
                    cmd.Parameters.AddWithValue("@DiaChi", diaChi ?? "");
                    cmd.Parameters.AddWithValue("@SoDienThoai", soDienThoai ?? "");

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CapNhatChiNhanh(int maChiNhanh, string tenChiNhanh, string diaChi, string soDienThoai)
        {
            string query = @"
                UPDATE ChiNhanh 
                SET TenChiNhanh = @TenChiNhanh, DiaChi = @DiaChi, SoDienThoai = @SoDienThoai
                WHERE MaChiNhanh = @MaChiNhanh";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@TenChiNhanh", tenChiNhanh);
                    cmd.Parameters.AddWithValue("@DiaChi", diaChi ?? "");
                    cmd.Parameters.AddWithValue("@SoDienThoai", soDienThoai ?? "");

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool XoaChiNhanh(int maChiNhanh)
        {
            string query = "DELETE FROM ChiNhanh WHERE MaChiNhanh = @MaChiNhanh";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public DataTable TimKiemChiNhanh(string tuKhoa)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    MaChiNhanh, TenChiNhanh, DiaChi, SoDienThoai, TrangThai
                FROM ChiNhanh
                WHERE TenChiNhanh LIKE @TuKhoa OR DiaChi LIKE @TuKhoa OR SoDienThoai LIKE @TuKhoa
                ORDER BY TenChiNhanh";

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

        public DataTable LayThongKeChiNhanh()
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    cn.MaChiNhanh,
                    cn.TenChiNhanh,
                    COUNT(DISTINCT nd.MaNguoiDung) AS SoNhanVien,
                    COUNT(DISTINCT pc.MaPhong) AS SoPhongChieu,
                    ISNULL(SUM(hd.ThanhTien), 0) AS DoanhThuThang
                FROM ChiNhanh cn
                LEFT JOIN NguoiDung nd ON cn.MaChiNhanh = nd.MaChiNhanh
                LEFT JOIN PhongChieu pc ON cn.MaChiNhanh = pc.MaChiNhanh
                LEFT JOIN HoaDon hd ON nd.MaNguoiDung = hd.MaNguoiDung 
                    AND MONTH(hd.NgayLap) = MONTH(GETDATE())
                    AND YEAR(hd.NgayLap) = YEAR(GETDATE())
                GROUP BY cn.MaChiNhanh, cn.TenChiNhanh
                ORDER BY cn.TenChiNhanh";

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
