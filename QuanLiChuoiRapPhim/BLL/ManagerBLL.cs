using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.BLL
{
    public class ManagerBLL
    {
        private string connectionString = "Your_Connection_String";

        #region Quản lý Nhân viên
        public DataTable GetStaffByBranch(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    nv.MaNhanVien,
                    nv.HoTen,
                    nv.Email,
                    nv.SoDienThoai,
                    nv.NgaySinh,
                    nv.CMND,
                    nv.DiaChi,
                    nv.NgayVaoLam,
                    nv.ChucVu,
                    nv.LuongCoBan,
                    nv.TrangThai,
                    cn.TenChiNhanh
                FROM NhanVien nv
                INNER JOIN ChiNhanh cn ON nv.MaChiNhanh = cn.MaChiNhanh
                WHERE nv.MaChiNhanh = @BranchId
                ORDER BY nv.HoTen";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public bool AddStaff(string hoTen, string email, string sdt, DateTime ngaySinh,
                           string cmnd, string diaChi, string chucVu, decimal luong,
                           int branchId, int createdBy)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO NhanVien 
                    (HoTen, Email, SoDienThoai, NgaySinh, CMND, DiaChi, 
                     NgayVaoLam, ChucVu, LuongCoBan, TrangThai, MaChiNhanh, NguoiTao)
                    VALUES 
                    (@HoTen, @Email, @SDT, @NgaySinh, @CMND, @DiaChi,
                     GETDATE(), @ChucVu, @Luong, 'Đang làm việc', @BranchId, @CreatedBy)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HoTen", hoTen);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@SDT", sdt);
                    cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh);
                    cmd.Parameters.AddWithValue("@CMND", cmnd);
                    cmd.Parameters.AddWithValue("@DiaChi", diaChi);
                    cmd.Parameters.AddWithValue("@ChucVu", chucVu);
                    cmd.Parameters.AddWithValue("@Luong", luong);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool ResetStaffPassword(int staffId, int managerId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE NguoiDung 
                                SET MatKhau = @DefaultPassword,
                                    NgayCapNhat = GETDATE(),
                                    NguoiCapNhat = @ManagerId
                                WHERE MaNhanVien = @StaffId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@ManagerId", managerId);
                    cmd.Parameters.AddWithValue("@DefaultPassword",
                        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("123456")));

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        #endregion

        #region Quản lý Lịch chiếu
        public DataTable GetShowtimesByBranchAndDate(int branchId, DateTime date)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    sc.MaSuatChieu,
                    p.TenPhim,
                    pc.TenPhong,
                    sc.NgayChieu,
                    sc.GioBatDau,
                    sc.GioKetThuc,
                    pc.SoGhe - ISNULL(COUNT(v.MaVe), 0) as SoGheTrong,
                    ISNULL(SUM(v.TongTien), 0) as DoanhThu,
                    sc.TrangThai
                FROM SuatChieu sc
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu
                WHERE pc.MaChiNhanh = @BranchId 
                    AND CONVERT(date, sc.NgayChieu) = CONVERT(date, @Date)
                GROUP BY sc.MaSuatChieu, p.TenPhim, pc.TenPhong, 
                         sc.NgayChieu, sc.GioBatDau, sc.GioKetThuc, 
                         pc.SoGhe, sc.TrangThai
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

        public bool CancelShowtime(int showtimeId, int managerId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE SuatChieu 
                                SET TrangThai = 'Đã hủy',
                                    NgayCapNhat = GETDATE(),
                                    NguoiCapNhat = @ManagerId
                                WHERE MaSuatChieu = @ShowtimeId
                                    AND NOT EXISTS (
                                        SELECT 1 FROM Ve 
                                        WHERE MaSuatChieu = @ShowtimeId
                                            AND TrangThai IN ('Đã bán', 'Đã đặt')
                                    )";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ShowtimeId", showtimeId);
                    cmd.Parameters.AddWithValue("@ManagerId", managerId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        #endregion

        #region Quản lý Kho hàng
        public DataTable GetInventoryByBranch(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    sp.MaSanPham,
                    sp.TenSanPham,
                    sp.LoaiSanPham,
                    sp.SoLuongTon,
                    sp.DonViTinh,
                    sp.GiaNhap,
                    sp.GiaBan,
                    sp.TrangThai,
                    sp.NgayNhap
                FROM SanPham sp
                WHERE sp.MaChiNhanh = @BranchId
                    AND sp.TrangThai = 'Đang bán'
                ORDER BY sp.TenSanPham";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        public bool DeleteProduct(int productId, int managerId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE SanPham 
                                SET TrangThai = 'Ngừng bán',
                                    NgayCapNhat = GETDATE(),
                                    NguoiCapNhat = @ManagerId
                                WHERE MaSanPham = @ProductId
                                    AND SoLuongTon = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@ManagerId", managerId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        #endregion

        #region Báo cáo Doanh thu
        public DataTable GetRevenueByBranch(int branchId, string timePeriod)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = GetRevenueQuery(timePeriod);

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        private string GetRevenueQuery(string timePeriod)
        {
            return timePeriod switch
            {
                "Hôm nay" => @"SELECT 
                    CONVERT(date, v.NgayBan) as Ngay,
                    SUM(CASE WHEN v.LoaiVe = 'Vé phim' THEN v.TongTien ELSE 0 END) as DoanhThuVe,
                    SUM(CASE WHEN v.LoaiVe = 'Sản phẩm' THEN v.TongTien ELSE 0 END) as DoanhThuSP,
                    SUM(v.TongTien) as TongDoanhThu,
                    COUNT(CASE WHEN v.LoaiVe = 'Vé phim' THEN 1 END) as SoVeBan,
                    COUNT(CASE WHEN v.LoaiVe = 'Sản phẩm' THEN 1 END) as SoSPBan
                FROM Ve v
                INNER JOIN NhanVien nv ON v.MaNhanVien = nv.MaNhanVien
                WHERE nv.MaChiNhanh = @BranchId
                    AND CONVERT(date, v.NgayBan) = CONVERT(date, GETDATE())
                GROUP BY CONVERT(date, v.NgayBan)",

                "Tháng này" => @"SELECT 
                    CONVERT(date, v.NgayBan) as Ngay,
                    SUM(CASE WHEN v.LoaiVe = 'Vé phim' THEN v.TongTien ELSE 0 END) as DoanhThuVe,
                    SUM(CASE WHEN v.LoaiVe = 'Sản phẩm' THEN v.TongTien ELSE 0 END) as DoanhThuSP,
                    SUM(v.TongTien) as TongDoanhThu,
                    COUNT(CASE WHEN v.LoaiVe = 'Vé phim' THEN 1 END) as SoVeBan,
                    COUNT(CASE WHEN v.LoaiVe = 'Sản phẩm' THEN 1 END) as SoSPBan
                FROM Ve v
                INNER JOIN NhanVien nv ON v.MaNhanVien = nv.MaNhanVien
                WHERE nv.MaChiNhanh = @BranchId
                    AND MONTH(v.NgayBan) = MONTH(GETDATE())
                    AND YEAR(v.NgayBan) = YEAR(GETDATE())
                GROUP BY CONVERT(date, v.NgayBan)
                ORDER BY Ngay",

                _ => @"SELECT 
                    CONVERT(date, v.NgayBan) as Ngay,
                    SUM(CASE WHEN v.LoaiVe = 'Vé phim' THEN v.TongTien ELSE 0 END) as DoanhThuVe,
                    SUM(CASE WHEN v.LoaiVe = 'Sản phẩm' THEN v.TongTien ELSE 0 END) as DoanhThuSP,
                    SUM(v.TongTien) as TongDoanhThu,
                    COUNT(CASE WHEN v.LoaiVe = 'Vé phim' THEN 1 END) as SoVeBan,
                    COUNT(CASE WHEN v.LoaiVe = 'Sản phẩm' THEN 1 END) as SoSPBan
                FROM Ve v
                INNER JOIN NhanVien nv ON v.MaNhanVien = nv.MaNhanVien
                WHERE nv.MaChiNhanh = @BranchId
                    AND v.NgayBan >= DATEADD(day, -7, GETDATE())
                GROUP BY CONVERT(date, v.NgayBan)
                ORDER BY Ngay"
            };
        }
        #endregion

        #region Quản lý Lịch làm việc
        public DataTable GetWorkScheduleByBranch(int branchId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                    lv.MaLichLamViec,
                    nv.MaNhanVien,
                    nv.HoTen,
                    lv.NgayLam,
                    lv.CaLam,
                    lv.GioBatDau,
                    lv.GioKetThuc,
                    lv.TrangThai,
                    lv.GhiChu
                FROM LichLamViec lv
                INNER JOIN NhanVien nv ON lv.MaNhanVien = nv.MaNhanVien
                WHERE nv.MaChiNhanh = @BranchId
                    AND lv.NgayLam >= GETDATE()
                ORDER BY lv.NgayLam, lv.GioBatDau";

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
    }
}