using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.BLL
{
    public class StaffBLL
    {
        private string connectionString = "Your_Connection_String";

        #region Thống kê
        public class TodayStats
        {
            public decimal TotalRevenue { get; set; }
            public int TicketsSold { get; set; }
            public int ProductsSold { get; set; }
            public int CustomersServed { get; set; }
        }

        public TodayStats GetTodayStats(int staffId)
        {
            var stats = new TodayStats();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    ISNULL(SUM(CASE WHEN LoaiVe = 'Vé phim' THEN TongTien ELSE 0 END), 0) as DoanhThuVe,
                    ISNULL(SUM(CASE WHEN LoaiVe = 'Sản phẩm' THEN TongTien ELSE 0 END), 0) as DoanhThuSP,
                    ISNULL(COUNT(CASE WHEN LoaiVe = 'Vé phim' THEN 1 END), 0) as SoVe,
                    ISNULL(COUNT(CASE WHEN LoaiVe = 'Sản phẩm' THEN 1 END), 0) as SoSP,
                    COUNT(DISTINCT MaKhachHang) as SoKhach
                FROM Ve
                WHERE MaNhanVien = @StaffId
                    AND CONVERT(date, NgayBan) = CONVERT(date, GETDATE())
                    AND TrangThai = 'Đã thanh toán'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.TotalRevenue = reader.GetDecimal(0) + reader.GetDecimal(1);
                            stats.TicketsSold = reader.GetInt32(2);
                            stats.ProductsSold = reader.GetInt32(3);
                            stats.CustomersServed = reader.GetInt32(4);
                        }
                    }
                }
            }

            return stats;
        }
        #endregion

        #region Bán vé
        public DataTable GetTodayShowtimes(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    sc.MaSuatChieu as MaSuat,
                    p.TenPhim,
                    pc.TenPhong as Phong,
                    CONVERT(varchar, sc.GioBatDau, 108) + ' - ' + 
                    CONVERT(varchar, sc.GioKetThuc, 108) as ThoiGian,
                    pc.SoGhe - ISNULL(COUNT(v.MaVe), 0) as SoGheTrong,
                    ISNULL(gv.GiaVeThuong, 0) as GiaVe,
                    CASE 
                        WHEN sc.TrangThai = 'Đang chiếu' AND GETDATE() BETWEEN sc.GioBatDau AND sc.GioKetThuc THEN 'Đang chiếu'
                        WHEN sc.TrangThai = 'Đã hủy' THEN 'Đã hủy'
                        WHEN sc.GioBatDau < GETDATE() THEN 'Đã qua'
                        ELSE 'Sắp chiếu'
                    END as TrangThai
                FROM SuatChieu sc
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                INNER JOIN ChiNhanh cn ON pc.MaChiNhanh = cn.MaChiNhanh
                LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu AND v.TrangThai IN ('Đã bán', 'Đã đặt')
                LEFT JOIN GiaVe gv ON pc.MaLoaiPhong = gv.MaLoaiPhong
                WHERE cn.MaChiNhanh = @BranchId
                    AND CONVERT(date, sc.NgayChieu) = CONVERT(date, GETDATE())
                    AND sc.TrangThai NOT IN ('Đã hủy')
                GROUP BY sc.MaSuatChieu, p.TenPhim, pc.TenPhong, sc.GioBatDau, 
                         sc.GioKetThuc, pc.SoGhe, sc.TrangThai, gv.GiaVeThuong
                HAVING pc.SoGhe - ISNULL(COUNT(v.MaVe), 0) > 0
                ORDER BY sc.GioBatDau";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public Dictionary<string, bool> GetSeatsByShowtime(int showtimeId)
        {
            var seats = new Dictionary<string, bool>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    g.MaGhe,
                    g.ViTri,
                    CASE WHEN v.MaVe IS NULL THEN 1 ELSE 0 END as ConTrong
                FROM Ghe g
                INNER JOIN PhongChieu pc ON g.MaPhong = pc.MaPhong
                INNER JOIN SuatChieu sc ON pc.MaPhong = sc.MaPhong
                LEFT JOIN Ve v ON g.MaGhe = v.MaGhe AND v.MaSuatChieu = @ShowtimeId
                WHERE sc.MaSuatChieu = @ShowtimeId
                ORDER BY g.ViTri";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ShowtimeId", showtimeId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string seatCode = reader.GetString(1);
                            bool isAvailable = reader.GetInt32(2) == 1;
                            seats[seatCode] = isAvailable;
                        }
                    }
                }
            }

            return seats;
        }

        public bool SellTickets(int showtimeId, int staffId, List<string> seats,
                               int customerId, decimal totalAmount, string paymentMethod)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Tạo hóa đơn
                    string invoiceQuery = @"INSERT INTO HoaDon 
                        (MaNhanVien, MaKhachHang, TongTien, PhuongThucThanhToan, 
                         NgayTao, TrangThai)
                        VALUES 
                        (@StaffId, @CustomerId, @TotalAmount, @PaymentMethod, 
                         GETDATE(), 'Đã thanh toán');
                        SELECT SCOPE_IDENTITY();";

                    int invoiceId;
                    using (SqlCommand cmd = new SqlCommand(invoiceQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@StaffId", staffId);
                        cmd.Parameters.AddWithValue("@CustomerId", customerId > 0 ? (object)customerId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

                        invoiceId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Tạo vé cho từng ghế
                    foreach (string seat in seats)
                    {
                        string ticketQuery = @"INSERT INTO Ve 
                            (MaSuatChieu, MaGhe, MaNhanVien, MaKhachHang, MaHoaDon,
                             LoaiVe, GiaVe, TongTien, NgayBan, TrangThai)
                            SELECT 
                                @ShowtimeId,
                                g.MaGhe,
                                @StaffId,
                                @CustomerId,
                                @InvoiceId,
                                'Vé phim',
                                gv.GiaVeThuong,
                                gv.GiaVeThuong,
                                GETDATE(),
                                'Đã bán'
                            FROM Ghe g
                            INNER JOIN PhongChieu pc ON g.MaPhong = pc.MaPhong
                            INNER JOIN SuatChieu sc ON pc.MaPhong = sc.MaPhong
                            INNER JOIN GiaVe gv ON pc.MaLoaiPhong = gv.MaLoaiPhong
                            WHERE sc.MaSuatChieu = @ShowtimeId
                                AND g.ViTri = @SeatCode";

                        using (SqlCommand cmd = new SqlCommand(ticketQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ShowtimeId", showtimeId);
                            cmd.Parameters.AddWithValue("@StaffId", staffId);
                            cmd.Parameters.AddWithValue("@CustomerId", customerId > 0 ? (object)customerId : DBNull.Value);
                            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            cmd.Parameters.AddWithValue("@SeatCode", seat);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Cập nhật điểm tích lũy nếu là thành viên
                    if (customerId > 0)
                    {
                        int pointsEarned = (int)(totalAmount / 10000); // 1 điểm / 10,000đ

                        string pointsQuery = @"UPDATE KhachHang
                            SET DiemTichLuy = DiemTichLuy + @Points,
                                TongChiTieu = TongChiTieu + @Amount
                            WHERE MaKhachHang = @CustomerId";

                        using (SqlCommand cmd = new SqlCommand(pointsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Points", pointsEarned);
                            cmd.Parameters.AddWithValue("@Amount", totalAmount);
                            cmd.Parameters.AddWithValue("@CustomerId", customerId);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Bán sản phẩm
        public DataTable GetAvailableProducts(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    sp.MaSanPham as MaSP,
                    sp.TenSanPham as TenSP,
                    sp.LoaiSanPham as Loai,
                    sp.GiaBan as DonGia,
                    sp.SoLuongTon,
                    sp.DonViTinh as DonVi
                FROM SanPham sp
                WHERE sp.MaChiNhanh = @BranchId
                    AND sp.TrangThai = 'Đang bán'
                    AND sp.SoLuongTon > 0
                ORDER BY sp.LoaiSanPham, sp.TenSanPham";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public bool SellProducts(int staffId, List<ProductSale> products,
                                int customerId, decimal totalAmount, string paymentMethod)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Tạo hóa đơn
                    string invoiceQuery = @"INSERT INTO HoaDon 
                        (MaNhanVien, MaKhachHang, TongTien, PhuongThucThanhToan, 
                         NgayTao, TrangThai)
                        VALUES 
                        (@StaffId, @CustomerId, @TotalAmount, @PaymentMethod, 
                         GETDATE(), 'Đã thanh toán');
                        SELECT SCOPE_IDENTITY();";

                    int invoiceId;
                    using (SqlCommand cmd = new SqlCommand(invoiceQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@StaffId", staffId);
                        cmd.Parameters.AddWithValue("@CustomerId", customerId > 0 ? (object)customerId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

                        invoiceId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Tạo vé/bản ghi bán sản phẩm
                    foreach (var product in products)
                    {
                        // Tạo vé sản phẩm
                        string ticketQuery = @"INSERT INTO Ve 
                            (MaNhanVien, MaKhachHang, MaHoaDon, MaSanPham,
                             LoaiVe, SoLuong, GiaVe, TongTien, NgayBan, TrangThai)
                            VALUES 
                            (@StaffId, @CustomerId, @InvoiceId, @ProductId,
                             'Sản phẩm', @Quantity, @Price, @SubTotal, GETDATE(), 'Đã bán')";

                        using (SqlCommand cmd = new SqlCommand(ticketQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@StaffId", staffId);
                            cmd.Parameters.AddWithValue("@CustomerId", customerId > 0 ? (object)customerId : DBNull.Value);
                            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                            cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                            cmd.Parameters.AddWithValue("@Price", product.Price);
                            cmd.Parameters.AddWithValue("@SubTotal", product.Price * product.Quantity);

                            cmd.ExecuteNonQuery();
                        }

                        // Cập nhật tồn kho
                        string updateStockQuery = @"UPDATE SanPham
                            SET SoLuongTon = SoLuongTon - @Quantity,
                                NgayCapNhat = GETDATE()
                            WHERE MaSanPham = @ProductId";

                        using (SqlCommand cmd = new SqlCommand(updateStockQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                            cmd.Parameters.AddWithValue("@Quantity", product.Quantity);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Cập nhật điểm tích lũy nếu là thành viên
                    if (customerId > 0)
                    {
                        int pointsEarned = (int)(totalAmount / 10000);

                        string pointsQuery = @"UPDATE KhachHang
                            SET DiemTichLuy = DiemTichLuy + @Points,
                                TongChiTieu = TongChiTieu + @Amount
                            WHERE MaKhachHang = @CustomerId";

                        using (SqlCommand cmd = new SqlCommand(pointsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Points", pointsEarned);
                            cmd.Parameters.AddWithValue("@Amount", totalAmount);
                            cmd.Parameters.AddWithValue("@CustomerId", customerId);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public class ProductSale
        {
            public string ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
        #endregion

        #region Quản lý Khách hàng
        public DataTable GetRecentCustomers(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT TOP 50
                    kh.MaKhachHang as MaKH,
                    kh.HoTen,
                    kh.SoDienThoai,
                    kh.Email,
                    kh.NgaySinh,
                    kh.DiemTichLuy,
                    CASE 
                        WHEN kh.DiemTichLuy >= 5000 THEN 'VIP'
                        WHEN kh.DiemTichLuy >= 1000 THEN 'Vàng'
                        WHEN kh.DiemTichLuy >= 100 THEN 'Bạc'
                        ELSE 'Đồng'
                    END as HangThanhVien,
                    kh.NgayDangKy
                FROM KhachHang kh
                INNER JOIN Ve v ON kh.MaKhachHang = v.MaKhachHang
                INNER JOIN NhanVien nv ON v.MaNhanVien = nv.MaNhanVien
                WHERE nv.MaChiNhanh = @BranchId
                GROUP BY kh.MaKhachHang, kh.HoTen, kh.SoDienThoai, kh.Email,
                         kh.NgaySinh, kh.DiemTichLuy, kh.NgayDangKy
                ORDER BY MAX(v.NgayBan) DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable SearchCustomers(string keyword)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    kh.MaKhachHang as MaKH,
                    kh.HoTen,
                    kh.SoDienThoai,
                    kh.Email,
                    kh.NgaySinh,
                    kh.DiemTichLuy,
                    CASE 
                        WHEN kh.DiemTichLuy >= 5000 THEN 'VIP'
                        WHEN kh.DiemTichLuy >= 1000 THEN 'Vàng'
                        WHEN kh.DiemTichLuy >= 100 THEN 'Bạc'
                        ELSE 'Đồng'
                    END as HangThanhVien,
                    kh.NgayDangKy
                FROM KhachHang kh
                WHERE kh.HoTen LIKE @Keyword 
                    OR kh.SoDienThoai LIKE @Keyword
                    OR kh.Email LIKE @Keyword
                    OR kh.MaKhachHang LIKE @Keyword
                ORDER BY kh.DiemTichLuy DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public bool RegisterCustomer(string fullName, string phone, string email,
                                    DateTime? birthday, int registeredBy)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO KhachHang 
                    (HoTen, SoDienThoai, Email, NgaySinh, DiemTichLuy, 
                     TongChiTieu, NgayDangKy, NguoiDangKy)
                    VALUES 
                    (@FullName, @Phone, @Email, @Birthday, 0, 0, GETDATE(), @RegisteredBy)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email);
                    cmd.Parameters.AddWithValue("@Birthday", birthday.HasValue ? (object)birthday.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@RegisteredBy", registeredBy);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        #endregion

        #region Phim và Lịch chiếu
        public DataTable GetMovies(int branchId, string filter)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT DISTINCT
                    p.MaPhim,
                    p.TenPhim,
                    p.TheLoai,
                    p.ThoiLuong,
                    p.DoTuoi,
                    p.NgayKhoiChieu,
                    p.NgayKetThuc,
                    p.TrangThai
                FROM Phim p
                INNER JOIN SuatChieu sc ON p.MaPhim = sc.MaPhim
                INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                WHERE pc.MaChiNhanh = @BranchId
                    AND p.TrangThai = 'Đang chiếu'
                    AND (@Filter = 'Tất cả' OR p.TheLoai = @Filter)
                    AND sc.NgayChieu >= GETDATE()
                ORDER BY p.NgayKhoiChieu DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@Filter", filter);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public DataTable GetShowtimesByDate(int branchId, DateTime date)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    sc.MaSuatChieu as MaSuat,
                    p.TenPhim,
                    pc.TenPhong as Phong,
                    CONVERT(varchar, sc.GioBatDau, 108) + ' - ' + 
                    CONVERT(varchar, sc.GioKetThuc, 108) as ThoiGian,
                    pc.SoGhe - ISNULL(COUNT(v.MaVe), 0) as SoGheTrong,
                    ISNULL(gv.GiaVeThuong, 0) as GiaVe
                FROM SuatChieu sc
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                INNER JOIN ChiNhanh cn ON pc.MaChiNhanh = cn.MaChiNhanh
                LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu AND v.TrangThai IN ('Đã bán', 'Đã đặt')
                LEFT JOIN GiaVe gv ON pc.MaLoaiPhong = gv.MaLoaiPhong
                WHERE cn.MaChiNhanh = @BranchId
                    AND CONVERT(date, sc.NgayChieu) = CONVERT(date, @Date)
                    AND sc.TrangThai NOT IN ('Đã hủy')
                    AND sc.GioBatDau > GETDATE()
                GROUP BY sc.MaSuatChieu, p.TenPhim, pc.TenPhong, sc.GioBatDau, 
                         sc.GioKetThuc, pc.SoGhe, gv.GiaVeThuong
                ORDER BY sc.GioBatDau";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@Date", date);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }
        #endregion

        #region Tồn kho
        public DataTable GetInventoryStatus(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    sp.MaSanPham as MaSP,
                    sp.TenSanPham as TenSP,
                    sp.LoaiSanPham as Loai,
                    sp.SoLuongTon,
                    sp.DonViTinh as DonVi,
                    sp.GiaBan as GiaBan,
                    CASE 
                        WHEN sp.SoLuongTon <= 0 THEN 'Hết hàng'
                        WHEN sp.SoLuongTon <= 10 THEN 'Sắp hết'
                        ELSE 'Còn hàng'
                    END as TrangThai
                FROM SanPham sp
                WHERE sp.MaChiNhanh = @BranchId
                    AND sp.TrangThai = 'Đang bán'
                ORDER BY sp.SoLuongTon, sp.TenSanPham";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }
        #endregion

        #region Lịch làm việc
        public DataTable GetPersonalSchedule(int staffId, int month, int year)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    lv.NgayLam,
                    lv.CaLam,
                    lv.GioBatDau,
                    lv.GioKetThuc,
                    lv.TrangThai,
                    lv.GhiChu
                FROM LichLamViec lv
                WHERE lv.MaNhanVien = @StaffId
                    AND MONTH(lv.NgayLam) = @Month
                    AND YEAR(lv.NgayLam) = @Year
                ORDER BY lv.NgayLam, lv.GioBatDau";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Year", year);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }
        #endregion

        #region Báo cáo cá nhân
        public class PersonalReport
        {
            public StatsData Stats { get; set; }
            public DataTable Details { get; set; }
        }

        public class StatsData
        {
            public decimal TotalRevenue { get; set; }
            public int TicketsSold { get; set; }
            public int ProductsSold { get; set; }
            public int CustomersServed { get; set; }
        }

        public PersonalReport GetPersonalReport(int staffId, int month, int year)
        {
            var report = new PersonalReport();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Lấy thống kê tổng
                string statsQuery = @"SELECT 
                    ISNULL(SUM(CASE WHEN LoaiVe = 'Vé phim' THEN TongTien ELSE 0 END), 0) as DoanhThuVe,
                    ISNULL(SUM(CASE WHEN LoaiVe = 'Sản phẩm' THEN TongTien ELSE 0 END), 0) as DoanhThuSP,
                    ISNULL(COUNT(CASE WHEN LoaiVe = 'Vé phim' THEN 1 END), 0) as SoVe,
                    ISNULL(COUNT(CASE WHEN LoaiVe = 'Sản phẩm' THEN 1 END), 0) as SoSP,
                    COUNT(DISTINCT MaKhachHang) as SoKhach
                FROM Ve
                WHERE MaNhanVien = @StaffId
                    AND MONTH(NgayBan) = @Month
                    AND YEAR(NgayBan) = @Year
                    AND TrangThai = 'Đã thanh toán'";

                using (SqlCommand cmd = new SqlCommand(statsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Year", year);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            report.Stats = new StatsData
                            {
                                TotalRevenue = reader.GetDecimal(0) + reader.GetDecimal(1),
                                TicketsSold = reader.GetInt32(2),
                                ProductsSold = reader.GetInt32(3),
                                CustomersServed = reader.GetInt32(4)
                            };
                        }
                    }
                }

                // Lấy chi tiết theo ngày
                string detailsQuery = @"SELECT 
                    CONVERT(date, v.NgayBan) as Ngay,
                    ISNULL(SUM(CASE WHEN v.LoaiVe = 'Vé phim' THEN v.TongTien ELSE 0 END), 0) as DoanhThuVe,
                    ISNULL(SUM(CASE WHEN v.LoaiVe = 'Sản phẩm' THEN v.TongTien ELSE 0 END), 0) as DoanhThuSP,
                    ISNULL(SUM(v.TongTien), 0) as TongDoanhThu,
                    COUNT(CASE WHEN v.LoaiVe = 'Vé phim' THEN 1 END) as SoVe,
                    COUNT(CASE WHEN v.LoaiVe = 'Sản phẩm' THEN 1 END) as SoSP,
                    MAX(CASE WHEN COUNT(DISTINCT v.MaKhachHang) > 10 THEN 'Xuất sắc'
                             WHEN COUNT(DISTINCT v.MaKhachHang) > 5 THEN 'Tốt'
                             ELSE 'Bình thường'
                        END) as GhiChu
                FROM Ve v
                WHERE v.MaNhanVien = @StaffId
                    AND MONTH(v.NgayBan) = @Month
                    AND YEAR(v.NgayBan) = @Year
                    AND v.TrangThai = 'Đã thanh toán'
                GROUP BY CONVERT(date, v.NgayBan)
                ORDER BY Ngay DESC";

                using (SqlCommand cmd = new SqlCommand(detailsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Year", year);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    report.Details = new DataTable();
                    adapter.Fill(report.Details);
                }
            }

            return report;
        }
        #endregion

        #region Xin nghỉ phép
        public bool SubmitLeaveRequest(int staffId, string leaveType, DateTime startDate,
                                      int days, string reason)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO DonXinNghi 
                    (MaNhanVien, LoaiNghi, TuNgay, SoNgay, LyDo, 
                     NgayGui, TrangThai)
                    VALUES 
                    (@StaffId, @LeaveType, @StartDate, @Days, @Reason,
                     GETDATE(), 'Chờ duyệt')";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@LeaveType", leaveType);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@Days", days);
                    cmd.Parameters.AddWithValue("@Reason", reason);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public DataTable GetLeaveHistory(int staffId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT
                    CONVERT(varchar, NgayGui, 103) as NgayGui,
                    LoaiNghi,
                    CONVERT(varchar, TuNgay, 103) as TuNgay,
                    SoNgay,
                    LyDo,
                    TrangThai,
                    ISNULL(GhiChuQuanLy, '') as GhiChu
                FROM DonXinNghi
                WHERE MaNhanVien = @StaffId
                ORDER BY NgayGui DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }
        #endregion

        #region CRUD Nhân viên (Staff Management)
        /// <summary>
        /// Lấy danh sách tất cả nhân viên
        /// </summary>
        public DataTable GetAllStaff()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT
                    nd.MaNguoiDung,
                    nd.TenDangNhap,
                    nd.HoTen,
                    nd.Email,
                    nd.SoDienThoai,
                    nd.VaiTro,
                    nd.MaChiNhanh,
                    cn.TenChiNhanh,
                    nd.TrangThai,
                    nd.NgayTao
                FROM NguoiDung nd
                LEFT JOIN ChiNhanh cn ON nd.MaChiNhanh = cn.MaChiNhanh
                WHERE nd.VaiTro IN (N'Nhân Viên', N'Quản Lý')
                ORDER BY nd.NgayTao DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        /// <summary>
        /// Lấy thông tin nhân viên theo ID
        /// </summary>
        public DataRow GetStaffById(int staffId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT
                    nd.MaNguoiDung,
                    nd.TenDangNhap,
                    nd.HoTen,
                    nd.Email,
                    nd.SoDienThoai,
                    nd.VaiTro,
                    nd.MaChiNhanh,
                    nd.TrangThai,
                    nd.NgayTao
                FROM NguoiDung nd
                WHERE nd.MaNguoiDung = @StaffId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Thêm nhân viên mới
        /// </summary>
        public bool CreateStaff(string username, string password, string fullName,
            string email, string phone, string role, int? branchId, bool isActive)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Check if username exists
                string checkQuery = "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @Username";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Username", username);
                    conn.Open();
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        throw new Exception("Tên đăng nhập đã tồn tại!");
                    }
                }

                string query = @"INSERT INTO NguoiDung
                    (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, VaiTro, MaChiNhanh, TrangThai, NgayTao)
                    VALUES
                    (@Username, @Password, @FullName, @Email, @Phone, @Role, @BranchId, @Status, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password); // TODO: Hash password
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@BranchId", branchId.HasValue ? (object)branchId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", isActive);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        public bool UpdateStaff(int staffId, string fullName, string email, string phone,
            string role, int? branchId, bool isActive, string newPassword = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query;
                if (!string.IsNullOrEmpty(newPassword))
                {
                    query = @"UPDATE NguoiDung SET
                        HoTen = @FullName,
                        Email = @Email,
                        SoDienThoai = @Phone,
                        VaiTro = @Role,
                        MaChiNhanh = @BranchId,
                        TrangThai = @Status,
                        MatKhau = @Password
                    WHERE MaNguoiDung = @StaffId";
                }
                else
                {
                    query = @"UPDATE NguoiDung SET
                        HoTen = @FullName,
                        Email = @Email,
                        SoDienThoai = @Phone,
                        VaiTro = @Role,
                        MaChiNhanh = @BranchId,
                        TrangThai = @Status
                    WHERE MaNguoiDung = @StaffId";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@BranchId", branchId.HasValue ? (object)branchId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", isActive);

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        cmd.Parameters.AddWithValue("@Password", newPassword); // TODO: Hash password
                    }

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Xóa nhân viên (soft delete - chuyển trạng thái)
        /// </summary>
        public bool DeleteStaff(int staffId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Soft delete - set TrangThai = 0
                string query = @"UPDATE NguoiDung SET TrangThai = 0 WHERE MaNguoiDung = @StaffId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Kiểm tra tên đăng nhập đã tồn tại chưa
        /// </summary>
        public bool CheckUsernameExists(string username, int? excludeStaffId = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = excludeStaffId.HasValue
                    ? "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @Username AND MaNguoiDung != @ExcludeId"
                    : "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @Username";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    if (excludeStaffId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@ExcludeId", excludeStaffId.Value);
                    }

                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        /// <summary>
        /// Lấy danh sách chi nhánh
        /// </summary>
        public DataTable GetBranches()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT MaChiNhanh, TenChiNhanh FROM ChiNhanh WHERE TrangThai = 1 ORDER BY TenChiNhanh";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }
        #endregion
    }
}