using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class DoanhThuDAL
    {
        public DataTable ThongKeDoanhThuTheoNgay(DateTime tuNgay, DateTime denNgay)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    CAST(NgayLap AS DATE) AS NgayBan,
                    COUNT(MaHoaDon) AS SoHoaDon,
                    SUM(ThanhTien) AS TongDoanhThu,
                    SUM(GiamGia) AS TongGiamGia
                FROM HoaDon
                WHERE CAST(NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND TrangThaiThanhToan = N'DaThanhToan'
                GROUP BY CAST(NgayLap AS DATE)
                ORDER BY NgayBan DESC";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                    cmd.Parameters.AddWithValue("@DenNgay", denNgay);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public DataTable ThongKeDoanhThuChiNhanh(DateTime tuNgay, DateTime denNgay)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    cn.MaChiNhanh,
                    cn.TenChiNhanh,
                    COUNT(hd.MaHoaDon) AS SoHoaDon,
                    SUM(hd.ThanhTien) AS TongDoanhThu,
                    AVG(hd.ThanhTien) AS DoanhThuTrungBinh
                FROM ChiNhanh cn
                LEFT JOIN NguoiDung nd ON cn.MaChiNhanh = nd.MaChiNhanh
                LEFT JOIN HoaDon hd ON nd.MaNguoiDung = hd.MaNguoiDung
                WHERE CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                GROUP BY cn.MaChiNhanh, cn.TenChiNhanh
                ORDER BY TongDoanhThu DESC";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                    cmd.Parameters.AddWithValue("@DenNgay", denNgay);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public DataTable ThongKePhimBanChay(int top = 10)
        {
            DataTable dt = new DataTable();
            string query = $@"
                SELECT TOP {top}
                    p.MaPhim,
                    p.TenPhim,
                    p.TheLoai,
                    COUNT(v.MaVe) AS SoVeBan,
                    SUM(v.GiaVe) AS DoanhThu
                FROM Phim p
                INNER JOIN SuatChieu sc ON p.MaPhim = sc.MaPhim
                INNER JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu
                WHERE v.TrangThai = N'DaBan'
                GROUP BY p.MaPhim, p.TenPhim, p.TheLoai
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

        public DataTable ThongKeSanPhamBanChay(int top = 10)
        {
            DataTable dt = new DataTable();
            string query = $@"
                SELECT TOP {top}
                    sp.MaSanPham,
                    sp.TenSanPham,
                    sp.LoaiSanPham,
                    SUM(ctd.SoLuong) AS SoLuongBan,
                    SUM(ctd.ThanhTien) AS DoanhThu
                FROM SanPham sp
                INNER JOIN ChiTietHoaDon ctd ON sp.MaSanPham = ctd.MaSanPham
                INNER JOIN HoaDon hd ON ctd.MaHoaDon = hd.MaHoaDon
                WHERE hd.TrangThaiThanhToan = N'DaThanhToan'
                GROUP BY sp.MaSanPham, sp.TenSanPham, sp.LoaiSanPham
                ORDER BY SoLuongBan DESC";

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

        public decimal LayDoanhThuHienTai()
        {
            string query = @"
                SELECT ISNULL(SUM(ThanhTien), 0)
                FROM HoaDon
                WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)
                  AND TrangThaiThanhToan = N'DaThanhToan'";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
        }
    }
}
