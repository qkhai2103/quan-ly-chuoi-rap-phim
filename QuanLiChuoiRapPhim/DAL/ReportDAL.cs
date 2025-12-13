using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    public class ReportDAL
    {
        // 1. Doanh thu theo ngày (Sử dụng stored procedure có sẵn)
        public DataTable GetRevenueByDate(DateTime fromDate, DateTime toDate)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_ThongKeDoanhThuTheoNgay", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TuNgay", fromDate);
                    cmd.Parameters.AddWithValue("@DenNgay", toDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // 2. Phim bán chạy (Sử dụng stored procedure có sẵn)
        public DataTable GetTopMovies(DateTime fromDate, DateTime toDate, int top = 10)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_ThongKePhimBanChay", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Top", top);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // 3. Doanh thu theo chi nhánh (Sử dụng stored procedure có sẵn)
        public DataTable GetRevenueByBranch(DateTime fromDate, DateTime toDate)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_BaoCaoDoanhThuChiNhanh", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TuNgay", fromDate);
                    cmd.Parameters.AddWithValue("@DenNgay", toDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // 4. Vé bán theo suất chiếu
        public DataTable GetTicketSales(DateTime fromDate, DateTime toDate, string branchName = null)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    p.TenPhim,
                    COUNT(v.MaVe) AS SoVeBan,
                    SUM(v.GiaVe) AS DoanhThuVe,
                    sc.NgayChieu,
                    sc.GioChieu,
                    cn.TenChiNhanh,
                    ph.TenPhong
                FROM Ve v
                INNER JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                INNER JOIN PhongChieu ph ON sc.MaPhong = ph.MaPhong
                INNER JOIN ChiNhanh cn ON ph.MaChiNhanh = cn.MaChiNhanh
                WHERE v.TrangThai = N'DaBan'
                  AND v.NgayDat BETWEEN @FromDate AND @ToDate
                  AND (@BranchName IS NULL OR cn.TenChiNhanh = @BranchName)
                GROUP BY p.TenPhim, sc.NgayChieu, sc.GioChieu, cn.TenChiNhanh, ph.TenPhong
                ORDER BY SoVeBan DESC";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@BranchName",
                        string.IsNullOrEmpty(branchName) || branchName == "Tất cả chi nhánh" ?
                        (object)DBNull.Value : branchName);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // 5. Sản phẩm bán chạy
        public DataTable GetTopProducts(DateTime fromDate, DateTime toDate, string branchName = null)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    sp.TenSanPham,
                    sp.LoaiSanPham,
                    SUM(ct.SoLuong) AS SoLuongBan,
                    SUM(ct.ThanhTien) AS DoanhThu,
                    cn.TenChiNhanh
                FROM ChiTietHoaDon ct
                INNER JOIN SanPham sp ON ct.MaSanPham = sp.MaSanPham
                INNER JOIN HoaDon hd ON ct.MaHoaDon = hd.MaHoaDon
                INNER JOIN ChiNhanh cn ON sp.MaChiNhanh = cn.MaChiNhanh
                WHERE hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND hd.NgayLap BETWEEN @FromDate AND @ToDate
                  AND (@BranchName IS NULL OR cn.TenChiNhanh = @BranchName)
                GROUP BY sp.TenSanPham, sp.LoaiSanPham, cn.TenChiNhanh
                ORDER BY SoLuongBan DESC";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@BranchName",
                        string.IsNullOrEmpty(branchName) || branchName == "Tất cả chi nhánh" ?
                        (object)DBNull.Value : branchName);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // 6. Khách hàng thành viên
        public DataTable GetMemberCustomers()
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    HoTen,
                    SoDienThoai,
                    Email,
                    DiemTichLuy,
                    HangThanhVien,
                    NgayDangKy
                FROM KhachHang
                WHERE HangThanhVien IN (N'Bac', N'Vang')
                ORDER BY DiemTichLuy DESC";

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

        // 7. Thống kê nhanh tổng hợp
        public DataTable GetQuickStats(DateTime fromDate, DateTime toDate)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    -- Tổng doanh thu
                    (SELECT SUM(ThanhTien) FROM HoaDon 
                     WHERE TrangThaiThanhToan = N'DaThanhToan'
                     AND NgayLap BETWEEN @FromDate AND @ToDate) AS TongDoanhThu,
                    
                    -- Tổng số hóa đơn
                    (SELECT COUNT(*) FROM HoaDon 
                     WHERE TrangThaiThanhToan = N'DaThanhToan'
                     AND NgayLap BETWEEN @FromDate AND @ToDate) AS TongHoaDon,
                    
                    -- Tổng số vé bán
                    (SELECT COUNT(*) FROM Ve 
                     WHERE TrangThai = N'DaBan'
                     AND NgayDat BETWEEN @FromDate AND @ToDate) AS TongVeBan,
                    
                    -- Tổng số khách hàng
                    (SELECT COUNT(DISTINCT MaKhachHang) FROM HoaDon 
                     WHERE TrangThaiThanhToan = N'DaThanhToan'
                     AND NgayLap BETWEEN @FromDate AND @ToDate
                     AND MaKhachHang IS NOT NULL) AS TongKhachHang,
                    
                    -- Doanh thu trung bình/hóa đơn
                    (SELECT AVG(ThanhTien) FROM HoaDon 
                     WHERE TrangThaiThanhToan = N'DaThanhToan'
                     AND NgayLap BETWEEN @FromDate AND @ToDate) AS DonGiaTrungBinh";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // 8. Doanh thu theo giờ trong ngày
        public DataTable GetRevenueByHour(DateTime date)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    DATEPART(HOUR, hd.NgayLap) AS Gio,
                    COUNT(*) AS SoHoaDon,
                    SUM(hd.ThanhTien) AS DoanhThu
                FROM HoaDon hd
                WHERE hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND CAST(hd.NgayLap AS DATE) = @Date
                GROUP BY DATEPART(HOUR, hd.NgayLap)
                ORDER BY Gio";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
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

        // 9. Báo cáo tồn kho sản phẩm
        public DataTable GetInventoryReport()
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    sp.TenSanPham,
                    sp.LoaiSanPham,
                    sp.SoLuongTon,
                    sp.GiaBan,
                    cn.TenChiNhanh,
                    CASE 
                        WHEN sp.SoLuongTon <= 10 THEN N'CẦN NHẬP'
                        WHEN sp.SoLuongTon <= 30 THEN N'CẢNH BÁO'
                        ELSE N'ĐỦ'
                    END AS TinhTrang
                FROM SanPham sp
                INNER JOIN ChiNhanh cn ON sp.MaChiNhanh = cn.MaChiNhanh
                WHERE sp.TrangThai = 1
                ORDER BY sp.SoLuongTon ASC, cn.TenChiNhanh";

            using (var conn = new System.Data.SqlClient.SqlConnection(DatabaseConfig.ConnectionString))
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

        // 10. Báo cáo nhân viên bán hàng
        public DataTable GetStaffSalesReport(DateTime fromDate, DateTime toDate)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    nd.HoTen AS TenNhanVien,
                    nd.VaiTro,
                    cn.TenChiNhanh,
                    COUNT(hd.MaHoaDon) AS SoHoaDon,
                    SUM(hd.ThanhTien) AS TongDoanhThu,
                    AVG(hd.ThanhTien) AS DoanhThuTrungBinh
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                LEFT JOIN ChiNhanh cn ON nd.MaChiNhanh = cn.MaChiNhanh
                WHERE hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND hd.NgayLap BETWEEN @FromDate AND @ToDate
                  AND nd.VaiTro = N'NhanVien'
                GROUP BY nd.HoTen, nd.VaiTro, cn.TenChiNhanh
                ORDER BY TongDoanhThu DESC";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }
    }
}
