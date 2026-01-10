using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    /// <summary>
    /// NhanVienDAL - Data Access Layer cho Nhân viên
    /// Tách từ NhanVienBLL để đảm bảo kiến trúc 3-tier
    /// </summary>
    public class NhanVienDAL
    {
        private readonly string _connectionString;

        public NhanVienDAL()
        {
            _connectionString = DatabaseConfig.ConnectionString;
        }

        #region Staff CRUD Operations

        /// <summary>
        /// Lấy danh sách tất cả nhân viên
        /// </summary>
        public DataTable GetAllStaff()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                WHERE nd.VaiTro IN (N'NhanVien', N'QuanLy')
                ORDER BY nd.NgayTao DESC";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        /// <summary>
        /// Lấy nhân viên theo chi nhánh
        /// </summary>
        public DataTable GetStaffByBranch(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                WHERE nd.MaChiNhanh = @BranchId
                    AND nd.VaiTro IN (N'NhanVien', N'QuanLy')
                ORDER BY nd.NgayTao DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
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

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Thêm nhân viên mới
        /// </summary>
        public int CreateStaff(string username, string passwordHash, string fullName,
            string email, string phone, string role, int? branchId, bool isActive)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO NguoiDung
                    (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, VaiTro, MaChiNhanh, TrangThai, NgayTao)
                    VALUES
                    (@Username, @Password, @FullName, @Email, @Phone, @Role, @BranchId, @Status, GETDATE());
                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", passwordHash);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@BranchId", branchId.HasValue ? (object)branchId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", isActive ? 1 : 0);

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        public bool UpdateStaff(int staffId, string fullName, string email, string phone,
            string role, int? branchId, bool isActive, string newPasswordHash = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query;
                if (!string.IsNullOrEmpty(newPasswordHash))
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
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@BranchId", branchId.HasValue ? (object)branchId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", isActive ? 1 : 0);

                    if (!string.IsNullOrEmpty(newPasswordHash))
                    {
                        cmd.Parameters.AddWithValue("@Password", newPasswordHash);
                    }

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Xóa nhân viên (soft delete)
        /// </summary>
        public bool DeleteStaff(int staffId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
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
        /// Kiểm tra username đã tồn tại
        /// </summary>
        public bool CheckUsernameExists(string username, int? excludeStaffId = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
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

        #endregion

        #region Statistics

        /// <summary>
        /// Lấy thống kê hôm nay của nhân viên
        /// </summary>
        public (decimal TotalRevenue, int TicketsSold, int ProductsSold, int CustomersServed) GetTodayStats(int staffId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT 
                    ISNULL(SUM(CASE WHEN LoaiVe = N'Vé phim' THEN TongTien ELSE 0 END), 0) as DoanhThuVe,
                    ISNULL(SUM(CASE WHEN LoaiVe = N'Sản phẩm' THEN TongTien ELSE 0 END), 0) as DoanhThuSP,
                    ISNULL(COUNT(CASE WHEN LoaiVe = N'Vé phim' THEN 1 END), 0) as SoVe,
                    ISNULL(COUNT(CASE WHEN LoaiVe = N'Sản phẩm' THEN 1 END), 0) as SoSP,
                    COUNT(DISTINCT MaKhachHang) as SoKhach
                FROM Ve
                WHERE MaNhanVien = @StaffId
                    AND CONVERT(date, NgayBan) = CONVERT(date, GETDATE())
                    AND TrangThai = N'Đã thanh toán'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (
                                reader.GetDecimal(0) + reader.GetDecimal(1),
                                reader.GetInt32(2),
                                reader.GetInt32(3),
                                reader.GetInt32(4)
                            );
                        }
                    }
                }
            }

            return (0, 0, 0, 0);
        }

        /// <summary>
        /// Lấy thống kê theo tháng của nhân viên
        /// </summary>
        public DataTable GetMonthlyStats(int staffId, int month, int year)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT 
                    CONVERT(date, v.NgayBan) as Ngay,
                    ISNULL(SUM(CASE WHEN v.LoaiVe = N'Vé phim' THEN v.TongTien ELSE 0 END), 0) as DoanhThuVe,
                    ISNULL(SUM(CASE WHEN v.LoaiVe = N'Sản phẩm' THEN v.TongTien ELSE 0 END), 0) as DoanhThuSP,
                    ISNULL(SUM(v.TongTien), 0) as TongDoanhThu,
                    COUNT(CASE WHEN v.LoaiVe = N'Vé phim' THEN 1 END) as SoVe,
                    COUNT(CASE WHEN v.LoaiVe = N'Sản phẩm' THEN 1 END) as SoSP,
                    COUNT(DISTINCT v.MaKhachHang) as SoKhach
                FROM Ve v
                WHERE v.MaNhanVien = @StaffId
                    AND MONTH(v.NgayBan) = @Month
                    AND YEAR(v.NgayBan) = @Year
                    AND v.TrangThai = N'Đã thanh toán'
                GROUP BY CONVERT(date, v.NgayBan)
                ORDER BY Ngay DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Year", year);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        #endregion

        #region Customer Management

        /// <summary>
        /// Lấy khách hàng gần đây của chi nhánh
        /// </summary>
        public DataTable GetRecentCustomers(int branchId, int topCount = 50)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = $@"SELECT TOP {topCount}
                    kh.MaKhachHang as MaKH,
                    kh.HoTen,
                    kh.SoDienThoai,
                    kh.Email,
                    kh.NgaySinh,
                    kh.DiemTichLuy,
                    CASE 
                        WHEN kh.DiemTichLuy >= 5000 THEN 'VIP'
                        WHEN kh.DiemTichLuy >= 1000 THEN N'Vàng'
                        WHEN kh.DiemTichLuy >= 100 THEN N'Bạc'
                        ELSE N'Đồng'
                    END as HangThanhVien,
                    kh.NgayDangKy
                FROM KhachHang kh
                INNER JOIN Ve v ON kh.MaKhachHang = v.MaKhachHang
                INNER JOIN NguoiDung nd ON v.MaNhanVien = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @BranchId
                GROUP BY kh.MaKhachHang, kh.HoTen, kh.SoDienThoai, kh.Email,
                         kh.NgaySinh, kh.DiemTichLuy, kh.NgayDangKy
                ORDER BY MAX(v.NgayBan) DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Tìm kiếm khách hàng
        /// </summary>
        public DataTable SearchCustomers(string keyword)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                        WHEN kh.DiemTichLuy >= 1000 THEN N'Vàng'
                        WHEN kh.DiemTichLuy >= 100 THEN N'Bạc'
                        ELSE N'Đồng'
                    END as HangThanhVien,
                    kh.NgayDangKy
                FROM KhachHang kh
                WHERE kh.HoTen LIKE @Keyword 
                    OR kh.SoDienThoai LIKE @Keyword
                    OR kh.Email LIKE @Keyword
                    OR CAST(kh.MaKhachHang as NVARCHAR) LIKE @Keyword
                ORDER BY kh.DiemTichLuy DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Đăng ký khách hàng mới
        /// </summary>
        public int RegisterCustomer(string fullName, string phone, string email,
                                    DateTime? birthday, int registeredBy)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO KhachHang 
                    (HoTen, SoDienThoai, Email, NgaySinh, DiemTichLuy, 
                     TongChiTieu, NgayDangKy, NguoiDangKy)
                    VALUES 
                    (@FullName, @Phone, @Email, @Birthday, 0, 0, GETDATE(), @RegisteredBy);
                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@Birthday", birthday.HasValue ? (object)birthday.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@RegisteredBy", registeredBy);

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        /// <summary>
        /// Cập nhật điểm tích lũy khách hàng
        /// </summary>
        public bool UpdateCustomerPoints(int customerId, int pointsEarned, decimal amountSpent)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE KhachHang
                    SET DiemTichLuy = DiemTichLuy + @Points,
                        TongChiTieu = TongChiTieu + @Amount
                    WHERE MaKhachHang = @CustomerId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Points", pointsEarned);
                    cmd.Parameters.AddWithValue("@Amount", amountSpent);
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        #endregion

        #region Showtime and Movies

        /// <summary>
        /// Lấy suất chiếu hôm nay theo chi nhánh
        /// </summary>
        public DataTable GetTodayShowtimes(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                        WHEN sc.TrangThai = N'Đang chiếu' AND GETDATE() BETWEEN sc.GioBatDau AND sc.GioKetThuc THEN N'Đang chiếu'
                        WHEN sc.TrangThai = N'Đã hủy' THEN N'Đã hủy'
                        WHEN sc.GioBatDau < GETDATE() THEN N'Đã qua'
                        ELSE N'Sắp chiếu'
                    END as TrangThai
                FROM SuatChieu sc
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                INNER JOIN ChiNhanh cn ON pc.MaChiNhanh = cn.MaChiNhanh
                LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu AND v.TrangThai IN (N'Đã bán', N'Đã đặt')
                LEFT JOIN GiaVe gv ON pc.MaLoaiPhong = gv.MaLoaiPhong
                WHERE cn.MaChiNhanh = @BranchId
                    AND CONVERT(date, sc.NgayChieu) = CONVERT(date, GETDATE())
                    AND sc.TrangThai NOT IN (N'Đã hủy')
                GROUP BY sc.MaSuatChieu, p.TenPhim, pc.TenPhong, sc.GioBatDau, 
                         sc.GioKetThuc, pc.SoGhe, sc.TrangThai, gv.GiaVeThuong
                HAVING pc.SoGhe - ISNULL(COUNT(v.MaVe), 0) > 0
                ORDER BY sc.GioBatDau";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Lấy suất chiếu theo ngày
        /// </summary>
        public DataTable GetShowtimesByDate(int branchId, DateTime date)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu AND v.TrangThai IN (N'Đã bán', N'Đã đặt')
                LEFT JOIN GiaVe gv ON pc.MaLoaiPhong = gv.MaLoaiPhong
                WHERE cn.MaChiNhanh = @BranchId
                    AND CONVERT(date, sc.NgayChieu) = CONVERT(date, @Date)
                    AND sc.TrangThai NOT IN (N'Đã hủy')
                    AND sc.GioBatDau > GETDATE()
                GROUP BY sc.MaSuatChieu, p.TenPhim, pc.TenPhong, sc.GioBatDau, 
                         sc.GioKetThuc, pc.SoGhe, gv.GiaVeThuong
                ORDER BY sc.GioBatDau";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@Date", date);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Lấy ghế theo suất chiếu
        /// </summary>
        public Dictionary<string, bool> GetSeatsByShowtime(int showtimeId)
        {
            var seats = new Dictionary<string, bool>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ShowtimeId", showtimeId);

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

        /// <summary>
        /// Lấy phim theo chi nhánh
        /// </summary>
        public DataTable GetMovies(int branchId, string filter = "Tất cả")
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                    AND p.TrangThai = N'Đang chiếu'
                    AND (@Filter = N'Tất cả' OR p.TheLoai = @Filter)
                    AND sc.NgayChieu >= GETDATE()
                ORDER BY p.NgayKhoiChieu DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@Filter", filter);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        #endregion

        #region Ticket Sales

        /// <summary>
        /// Tạo hóa đơn mới
        /// </summary>
        public int CreateInvoice(int staffId, int? customerId, decimal totalAmount, string paymentMethod)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO HoaDon 
                    (MaNhanVien, MaKhachHang, TongTien, PhuongThucThanhToan, 
                     NgayTao, TrangThai)
                    VALUES 
                    (@StaffId, @CustomerId, @TotalAmount, @PaymentMethod, 
                     GETDATE(), N'Đã thanh toán');
                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@CustomerId", customerId.HasValue ? (object)customerId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                    cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Tạo vé cho ghế
        /// </summary>
        public bool CreateTicketForSeat(SqlConnection conn, SqlTransaction transaction,
            int showtimeId, string seatCode, int staffId, int? customerId, int invoiceId)
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
                    N'Vé phim',
                    gv.GiaVeThuong,
                    gv.GiaVeThuong,
                    GETDATE(),
                    N'Đã bán'
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
                cmd.Parameters.AddWithValue("@CustomerId", customerId.HasValue ? (object)customerId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                cmd.Parameters.AddWithValue("@SeatCode", seatCode);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Bán vé (transaction)
        /// </summary>
        public bool SellTickets(int showtimeId, int staffId, List<string> seats,
                               int? customerId, decimal totalAmount, string paymentMethod)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Tạo hóa đơn
                        string invoiceQuery = @"INSERT INTO HoaDon 
                            (MaNhanVien, MaKhachHang, TongTien, PhuongThucThanhToan, 
                             NgayTao, TrangThai)
                            VALUES 
                            (@StaffId, @CustomerId, @TotalAmount, @PaymentMethod, 
                             GETDATE(), N'Đã thanh toán');
                            SELECT SCOPE_IDENTITY();";

                        int invoiceId;
                        using (SqlCommand cmd = new SqlCommand(invoiceQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@StaffId", staffId);
                            cmd.Parameters.AddWithValue("@CustomerId", customerId.HasValue ? (object)customerId.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

                            invoiceId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Tạo vé cho từng ghế
                        foreach (string seat in seats)
                        {
                            CreateTicketForSeat(conn, transaction, showtimeId, seat, staffId, customerId, invoiceId);
                        }

                        // Cập nhật điểm tích lũy
                        if (customerId.HasValue && customerId.Value > 0)
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
                                cmd.Parameters.AddWithValue("@CustomerId", customerId.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region Products & Inventory

        /// <summary>
        /// Lấy sản phẩm khả dụng
        /// </summary>
        public DataTable GetAvailableProducts(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                    AND sp.TrangThai = N'Đang bán'
                    AND sp.SoLuongTon > 0
                ORDER BY sp.LoaiSanPham, sp.TenSanPham";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Lấy tình trạng tồn kho
        /// </summary>
        public DataTable GetInventoryStatus(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT 
                    sp.MaSanPham as MaSP,
                    sp.TenSanPham as TenSP,
                    sp.LoaiSanPham as Loai,
                    sp.SoLuongTon,
                    sp.DonViTinh as DonVi,
                    sp.GiaBan as GiaBan,
                    CASE 
                        WHEN sp.SoLuongTon <= 0 THEN N'Hết hàng'
                        WHEN sp.SoLuongTon <= 10 THEN N'Sắp hết'
                        ELSE N'Còn hàng'
                    END as TrangThai
                FROM SanPham sp
                WHERE sp.MaChiNhanh = @BranchId
                    AND sp.TrangThai = N'Đang bán'
                ORDER BY sp.SoLuongTon, sp.TenSanPham";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Cập nhật tồn kho sản phẩm
        /// </summary>
        public bool UpdateProductStock(SqlConnection conn, SqlTransaction transaction, int productId, int quantity)
        {
            string query = @"UPDATE SanPham
                SET SoLuongTon = SoLuongTon - @Quantity,
                    NgayCapNhat = GETDATE()
                WHERE MaSanPham = @ProductId";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        #endregion

        #region Schedule and Leave

        /// <summary>
        /// Lấy lịch làm việc cá nhân
        /// </summary>
        public DataTable GetPersonalSchedule(int staffId, int month, int year)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Gửi đơn xin nghỉ phép
        /// </summary>
        public bool SubmitLeaveRequest(int staffId, string leaveType, DateTime startDate,
                                      int days, string reason)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO DonXinNghi 
                    (MaNhanVien, LoaiNghi, TuNgay, SoNgay, LyDo, 
                     NgayGui, TrangThai)
                    VALUES 
                    (@StaffId, @LeaveType, @StartDate, @Days, @Reason,
                     GETDATE(), N'Chờ duyệt')";

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

        /// <summary>
        /// Lấy lịch sử nghỉ phép
        /// </summary>
        public DataTable GetLeaveHistory(int staffId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        #endregion

        #region Branch Utilities

        /// <summary>
        /// Lấy danh sách chi nhánh
        /// </summary>
        public DataTable GetBranches()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT MaChiNhanh, TenChiNhanh FROM ChiNhanh WHERE TrangThai = 1 ORDER BY TenChiNhanh";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        /// <summary>
        /// Lấy chi nhánh của nhân viên
        /// </summary>
        public int? GetStaffBranchId(int staffId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT MaChiNhanh FROM NguoiDung WHERE MaNguoiDung = @StaffId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? (int?)Convert.ToInt32(result) : null;
                }
            }
        }

        #endregion
    }
}
