using System;
using System.Data;
using System.Data.SqlClient;
using BCrypt.Net;
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
                return BCrypt.Net.BCrypt.HashPassword(plainPassword);
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
    }
}