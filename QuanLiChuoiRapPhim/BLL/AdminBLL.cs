using System;
using System.Data;
using System.Data.SqlClient;
// using BCrypt.Net;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class AdminBLL
    {
        private readonly AdminDAL _adminDal;

        public AdminBLL()
        {
            _adminDal = new AdminDAL();
        }

        // 1. PHƯƠNG THỨC ĐĂNG NHẬP (BẮT BUỘC)
        public bool Login(string username, string password, out string fullName, out string errorMessage)
        {
            fullName = "";
            errorMessage = "";

            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    errorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                    return false;
                }

                // Kiểm tra với database
                DataTable userTable = _adminDal.GetUserByLogin(username, password);

                if (userTable.Rows.Count == 0)
                {
                    errorMessage = "Tài khoản hoặc mật khẩu không đúng!";
                    return false;
                }

                DataRow userRow = userTable.Rows[0];
                fullName = userRow["HoTen"].ToString();
                return true;
            }
            catch (SqlException sqlEx)
            {
                errorMessage = $"Lỗi database: {sqlEx.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi hệ thống: {ex.Message}";
                return false;
            }
        }

        // 1.5 PHƯƠNG THỨC LOGIN MỞ RỘNG - Lấy toàn bộ thông tin người dùng
        public bool LoginWithFullInfo(string username, string password, out string fullName, 
            out int maNguoiDung, out int maChiNhanh, out string errorMessage)
        {
            fullName = "";
            maNguoiDung = 0;
            maChiNhanh = 0;
            errorMessage = "";

            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    errorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                    return false;
                }

                // Kiểm tra với database
                DataTable userTable = _adminDal.GetUserByLogin(username, password);

                if (userTable.Rows.Count == 0)
                {
                    errorMessage = "Tài khoản hoặc mật khẩu không đúng!";
                    return false;
                }

                DataRow userRow = userTable.Rows[0];
                fullName = userRow["HoTen"].ToString();
                maNguoiDung = Convert.ToInt32(userRow["MaNguoiDung"]);
                maChiNhanh = userRow["MaChiNhanh"] != DBNull.Value ? Convert.ToInt32(userRow["MaChiNhanh"]) : 0;
                return true;
            }
            catch (SqlException sqlEx)
            {
                errorMessage = $"Lỗi database: {sqlEx.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi hệ thống: {ex.Message}";
                return false;
            }
        }

        // 2. PHƯƠNG THỨC GetAllUsers (BẮT BUỘC - cho UC_Admin)
        public DataTable GetAllUsers()
        {
            try
            {
                return _adminDal.GetAllUsers();
            }
            catch (Exception ex)
            {
                // Tạo DataTable rỗng nếu có lỗi
                DataTable dt = new DataTable();
                dt.Columns.Add("Error", typeof(string));
                dt.Rows.Add($"Lỗi: {ex.Message}");
                return dt;
            }
        }

        // 3. PHƯƠNG THỨC TestDatabaseConnection (BẮT BUỘC)
        public bool TestDatabaseConnection()
        {
            try
            {
                return _adminDal.TestConnection();
            }
            catch
            {
                return false;
            }
        }

        // 4. PHƯƠNG THỨC MÃ HÓA MẬT KHẨU
        public static string HashPassword(string plainPassword)
        {
            try
            {
                // TODO: Cần cài đặt BCrypt package
                // return BCrypt.Net.BCrypt.HashPassword(plainPassword);
                return plainPassword; // Tạm thời không hash mật khẩu
            }
            catch
            {
                return plainPassword;
            }
        }
        // Thêm vào class AdminBLL

        public bool AddUser(string username, string password, string fullname,
            string email, string phone, string role, int? branchId, bool status)
        {
            try
            {
                // Kiểm tra username đã tồn tại chưa
                if (CheckUsernameExists(username))
                {
                    throw new Exception("Tên đăng nhập đã tồn tại!");
                }

                // Mã hóa mật khẩu
                string hashedPassword = HashPassword(password);

                // Tạo query
                string query = @"
            INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, 
                                   VaiTro, MaChiNhanh, TrangThai, NgayTao)
            VALUES (@Username, @Password, @FullName, @Email, @Phone, 
                    @Role, @BranchId, @Status, GETDATE())";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@FullName", fullname);
                        cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@BranchId", (object)branchId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", status);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thêm người dùng: {ex.Message}");
            }
        }

        public bool UpdateUser(int userId, string fullname, string email, string phone,
            string role, int? branchId, bool status, string newPassword = null)
        {
            try
            {
                string query;
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    // Cập nhật cả mật khẩu
                    query = @"
                UPDATE NguoiDung 
                SET HoTen = @FullName,
                    Email = @Email,
                    SoDienThoai = @Phone,
                    VaiTro = @Role,
                    MaChiNhanh = @BranchId,
                    TrangThai = @Status,
                    MatKhau = @Password
                WHERE MaNguoiDung = @UserId";
                }
                else
                {
                    // Không cập nhật mật khẩu
                    query = @"
                UPDATE NguoiDung 
                SET HoTen = @FullName,
                    Email = @Email,
                    SoDienThoai = @Phone,
                    VaiTro = @Role,
                    MaChiNhanh = @BranchId,
                    TrangThai = @Status
                WHERE MaNguoiDung = @UserId";
                }

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", fullname);
                        cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@BranchId", (object)branchId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        if (!string.IsNullOrWhiteSpace(newPassword))
                        {
                            string hashedPassword = HashPassword(newPassword);
                            cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        }

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi cập nhật người dùng: {ex.Message}");
            }
        }

        public bool DeleteUser(int userId)
        {
            try
            {
                // Kiểm tra xem user có phải là admin không
                string checkQuery = "SELECT VaiTro FROM NguoiDung WHERE MaNguoiDung = @UserId";
                string role = "";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();

                    // Lấy vai trò
                    using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        role = cmd.ExecuteScalar()?.ToString();
                    }

                    // Không cho xóa admin
                    if (role == "Admin")
                    {
                        throw new Exception("Không thể xóa tài khoản Admin!");
                    }

                    // Xóa user
                    string deleteQuery = "DELETE FROM NguoiDung WHERE MaNguoiDung = @UserId";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xóa người dùng: {ex.Message}");
            }
        }

        private bool CheckUsernameExists(string username)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @Username";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // DASHBOARD STATISTICS - Real data from database
        // ============================================================

        /// <summary>
        /// Lấy thống kê tổng quan cho Dashboard Admin
        /// </summary>
        public DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();

                    // 1. Doanh thu hôm nay
                    string queryRevenue = @"
                        SELECT ISNULL(SUM(ThanhTien), 0) 
                        FROM HoaDon 
                        WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)
                        AND TrangThaiThanhToan = N'DaThanhToan'";
                    using (SqlCommand cmd = new SqlCommand(queryRevenue, conn))
                    {
                        stats.TodayRevenue = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    // 2. Tổng người dùng
                    string queryUsers = "SELECT COUNT(*) FROM NguoiDung WHERE TrangThai = 1";
                    using (SqlCommand cmd = new SqlCommand(queryUsers, conn))
                    {
                        stats.TotalUsers = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 3. Phim đang chiếu
                    string queryMovies = "SELECT COUNT(*) FROM Phim WHERE TrangThai = 1";
                    using (SqlCommand cmd = new SqlCommand(queryMovies, conn))
                    {
                        stats.ActiveMovies = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 4. Vé bán hôm nay
                    string queryTickets = @"
                        SELECT COUNT(*) FROM Ve 
                        WHERE CAST(NgayDat AS DATE) = CAST(GETDATE() AS DATE)
                        AND TrangThai = N'DaBan'";
                    using (SqlCommand cmd = new SqlCommand(queryTickets, conn))
                    {
                        stats.TodayTicketsSold = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 5. Tổng chi nhánh
                    string queryBranches = "SELECT COUNT(*) FROM ChiNhanh WHERE TrangThai = 1";
                    using (SqlCommand cmd = new SqlCommand(queryBranches, conn))
                    {
                        stats.TotalBranches = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 6. Tổng phòng chiếu
                    string queryRooms = "SELECT COUNT(*) FROM PhongChieu WHERE TrangThai = 1";
                    using (SqlCommand cmd = new SqlCommand(queryRooms, conn))
                    {
                        stats.TotalRooms = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 7. Suất chiếu hôm nay
                    string queryShowtimes = @"
                        SELECT COUNT(*) FROM SuatChieu 
                        WHERE NgayChieu = CAST(GETDATE() AS DATE)";
                    using (SqlCommand cmd = new SqlCommand(queryShowtimes, conn))
                    {
                        stats.TodayShowtimes = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 8. Tổng khách hàng
                    string queryCustomers = "SELECT COUNT(*) FROM KhachHang";
                    using (SqlCommand cmd = new SqlCommand(queryCustomers, conn))
                    {
                        stats.TotalCustomers = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 9. Doanh thu tháng này
                    string queryMonthRevenue = @"
                        SELECT ISNULL(SUM(ThanhTien), 0) 
                        FROM HoaDon 
                        WHERE MONTH(NgayLap) = MONTH(GETDATE()) 
                        AND YEAR(NgayLap) = YEAR(GETDATE())
                        AND TrangThaiThanhToan = N'DaThanhToan'";
                    using (SqlCommand cmd = new SqlCommand(queryMonthRevenue, conn))
                    {
                        stats.MonthRevenue = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    // 10. Tổng vé đã bán
                    string queryTotalTickets = "SELECT COUNT(*) FROM Ve WHERE TrangThai = N'DaBan'";
                    using (SqlCommand cmd = new SqlCommand(queryTotalTickets, conn))
                    {
                        stats.TotalTicketsSold = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting dashboard stats: {ex.Message}");
            }

            return stats;
        }

        /// <summary>
        /// Lấy danh sách thông báo hệ thống
        /// </summary>
        public DataTable GetSystemNotifications()
        {
            DataTable dt = new DataTable();
            try
            {
                string query = @"
                    SELECT TOP 10 
                        MaLichSu,
                        HanhDong,
                        BangLienQuan,
                        ThoiGian,
                        nd.HoTen AS NguoiThucHien
                    FROM LichSuHoatDong ls
                    INNER JOIN NguoiDung nd ON ls.MaNguoiDung = nd.MaNguoiDung
                    ORDER BY ThoiGian DESC";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting notifications: {ex.Message}");
            }
            return dt;
        }

        /// <summary>
        /// Lấy báo cáo sự cố chưa xử lý
        /// </summary>
        public DataTable GetPendingIncidents()
        {
            DataTable dt = new DataTable();
            try
            {
                string query = @"
                    SELECT 
                        MaBaoCao,
                        LoaiSuCo,
                        MoTa,
                        MucDoUuTien,
                        TrangThai,
                        NgayBaoCao
                    FROM BaoCaoSuCo
                    WHERE TrangThai IN (N'ChoXuLy', N'DangXuLy')
                    ORDER BY 
                        CASE MucDoUuTien 
                            WHEN N'KhanCap' THEN 1 
                            WHEN N'Cao' THEN 2 
                            WHEN N'BinhThuong' THEN 3 
                            ELSE 4 
                        END,
                        NgayBaoCao DESC";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting incidents: {ex.Message}");
            }
            return dt;
        }

        /// <summary>
        /// Backup database
        /// </summary>
        public bool BackupDatabase(string backupPath)
        {
            try
            {
                string query = $@"
                    BACKUP DATABASE [CinemaDB] 
                    TO DISK = @BackupPath
                    WITH FORMAT, INIT, NAME = 'CinemaDB Full Backup'";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes timeout
                        cmd.Parameters.AddWithValue("@BackupPath", backupPath);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi backup database: {ex.Message}");
            }
        }

        /// <summary>
        /// Xuất dữ liệu ra DataTable để export Excel
        /// </summary>
        public DataTable ExportUsersToDataTable()
        {
            return GetAllUsers();
        }
    }

    // ============================================================
    // Dashboard Statistics Model
    // ============================================================
    public class DashboardStats
    {
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveMovies { get; set; }
        public int TodayTicketsSold { get; set; }
        public int TotalTicketsSold { get; set; }
        public int TotalBranches { get; set; }
        public int TotalRooms { get; set; }
        public int TodayShowtimes { get; set; }
        public int TotalCustomers { get; set; }

        public string FormatRevenue(decimal amount)
        {
            if (amount >= 1000000000)
                return $"{amount / 1000000000:F1}B ₫";
            if (amount >= 1000000)
                return $"{amount / 1000000:F1}M ₫";
            if (amount >= 1000)
                return $"{amount / 1000:F0}K ₫";
            return $"{amount:N0} ₫";
        }
    }
}