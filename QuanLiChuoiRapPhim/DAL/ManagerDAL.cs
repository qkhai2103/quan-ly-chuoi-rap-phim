using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiChuoiRapPhim.DAL
{
    /// <summary>
    /// ManagerDAL: Data Access Layer cho vai trò Quản Lý chi nhánh
    /// Xử lý truy vấn: Nhân viên, Lịch làm, Đơn xin nghỉ, Hiệu suất
    /// </summary>
    public class ManagerDAL
    {
        // ==================== NHÂN VIÊN ====================

        /// <summary>
        /// Lấy danh sách nhân viên của chi nhánh
        /// </summary>
        public DataTable GetNhanVienByBranch(int branchId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    nv.MaNhanVien,
                    nd.MaNguoiDung,
                    nd.HoTen,
                    nd.Email,
                    nd.SoDienThoai,
                    nv.ViTri,
                    nv.TrangThai,
                    nd.NgayTao
                FROM NhanVienChiTiet nv
                INNER JOIN NguoiDung nd ON nv.MaNguoiDung = nd.MaNguoiDung
                WHERE nv.MaChiNhanh = @BranchId
                ORDER BY nd.HoTen";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetNhanVienByBranch: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Lấy chi tiết nhân viên (bao gồm thông tin từ multiple tables)
        /// </summary>
        public DataTable GetNhanVienDetail(int nhanVienId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    nv.MaNhanVien,
                    nd.MaNguoiDung,
                    nd.HoTen,
                    nd.Email,
                    nd.SoDienThoai,
                    nv.MaChiNhanh,
                    cn.TenChiNhanh,
                    nv.NgaySinh,
                    nv.GioiTinh,
                    nv.QueQuan,
                    nv.NoiCap,
                    nv.NoiCapCMND,
                    nv.SoTaiKhoanNH,
                    nv.NganHang,
                    nv.HoTenThuHuong,
                    nv.NgayKyHopDong,
                    nv.NgayHetHopDong,
                    nv.ViTri,
                    nv.TrangThai,
                    nv.GhiChu,
                    nv.NgayTao,
                    nd.NgayTao as NgayTaoTK
                FROM NhanVienChiTiet nv
                INNER JOIN NguoiDung nd ON nv.MaNguoiDung = nd.MaNguoiDung
                LEFT JOIN ChiNhanh cn ON nv.MaChiNhanh = cn.MaChiNhanh
                WHERE nv.MaNhanVien = @NhanVienId";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NhanVienId", nhanVienId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetNhanVienDetail: {ex.Message}");
            }

            return dt;
        }

        // TODO: Thêm Update, Delete methods sau

        // ==================== LỊCH LÀM VIỆC ====================

        /// <summary>
        /// Lấy lịch làm của nhân viên trong khoảng thời gian
        /// </summary>
        public DataTable GetLichLamByNhanVien(int nhanVienId, DateTime fromDate, DateTime toDate)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    ll.MaLichLam,
                    ll.MaNhanVien,
                    ll.NgayLam,
                    ll.Ca,
                    ll.GioVao,
                    ll.GioRa,
                    ll.ViTri,
                    ll.TrangThai,
                    ll.GhiChu
                FROM LichLamViec ll
                WHERE ll.MaNhanVien = @NhanVienId
                    AND ll.NgayLam BETWEEN @FromDate AND @ToDate
                ORDER BY ll.NgayLam";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NhanVienId", nhanVienId);
                        cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetLichLamByNhanVien: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Lấy lịch làm của chi nhánh trong khoảng thời gian
        /// </summary>
        public DataTable GetLichLamByBranch(int branchId, DateTime fromDate, DateTime toDate)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    ll.MaLichLam,
                    ll.MaNhanVien,
                    nd.HoTen,
                    ll.NgayLam,
                    ll.Ca,
                    ll.GioVao,
                    ll.GioRa,
                    ll.ViTri,
                    ll.TrangThai,
                    ll.GhiChu
                FROM LichLamViec ll
                INNER JOIN NhanVienChiTiet nv ON ll.MaNhanVien = nv.MaNhanVien
                INNER JOIN NguoiDung nd ON nv.MaNguoiDung = nd.MaNguoiDung
                WHERE nv.MaChiNhanh = @BranchId
                    AND ll.NgayLam BETWEEN @FromDate AND @ToDate
                ORDER BY ll.NgayLam, nd.HoTen";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetLichLamByBranch: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Kiểm tra nhân viên đã có ca làm ngày đó chưa
        /// </summary>
        public bool LichLamExists(int nhanVienId, DateTime ngayLam)
        {
            string query = @"
                SELECT COUNT(*) FROM LichLamViec
                WHERE MaNhanVien = @NhanVienId 
                    AND NgayLam = @NgayLam
                    AND TrangThai != 'HuyCa'";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NhanVienId", nhanVienId);
                        cmd.Parameters.AddWithValue("@NgayLam", ngayLam.Date);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL LichLamExists: {ex.Message}");
            }
        }

        // TODO: Thêm Insert, Update, Delete LichLamViec methods sau

        // ==================== ĐƠN XIN NGHỈ ====================

        /// <summary>
        /// Lấy danh sách đơn xin nghỉ chờ duyệt của chi nhánh
        /// </summary>
        public DataTable GetDonXinNghiPending(int branchId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    dxn.MaDonXinNghi,
                    dxn.MaNhanVien,
                    nd.HoTen,
                    dxn.LoaiNghi,
                    dxn.NgayBatDau,
                    dxn.NgayKetThuc,
                    dxn.SoNgay,
                    dxn.LyDo,
                    dxn.TrangThai,
                    dxn.NgayGui
                FROM DonXinNghi dxn
                INNER JOIN NhanVienChiTiet nv ON dxn.MaNhanVien = nv.MaNhanVien
                INNER JOIN NguoiDung nd ON nv.MaNguoiDung = nd.MaNguoiDung
                WHERE dxn.MaChiNhanh = @BranchId
                    AND dxn.TrangThai = N'Cho'
                ORDER BY dxn.NgayGui DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetDonXinNghiPending: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Lấy danh sách đơn xin nghỉ của nhân viên
        /// </summary>
        public DataTable GetDonXinNghiByNhanVien(int nhanVienId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    dxn.MaDonXinNghi,
                    dxn.LoaiNghi,
                    dxn.NgayBatDau,
                    dxn.NgayKetThuc,
                    dxn.SoNgay,
                    dxn.LyDo,
                    dxn.DiaDiem,
                    dxn.TrangThai,
                    dxn.NgayGui,
                    dxn.NgayPD,
                    nd_pd.HoTen as NguoiPheDuyet
                FROM DonXinNghi dxn
                LEFT JOIN NguoiDung nd_pd ON dxn.NguoiPD = nd_pd.MaNguoiDung
                WHERE dxn.MaNhanVien = @NhanVienId
                ORDER BY dxn.NgayGui DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NhanVienId", nhanVienId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetDonXinNghiByNhanVien: {ex.Message}");
            }

            return dt;
        }

        // TODO: Thêm Approve, Reject methods sau

        // ==================== HIỆU SUẤT ====================

        /// <summary>
        /// Lấy danh sách hiệu suất của chi nhánh theo tháng
        /// </summary>
        public DataTable GetHieuSuatByMonth(int branchId, string thangNam)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    hs.MaHieuSuat,
                    hs.MaNhanVien,
                    nd.HoTen,
                    hs.ThangNam,
                    hs.DiemThaiDo,
                    hs.DiemNangSuat,
                    hs.DiemKyLuat,
                    hs.DiemTrungBinh,
                    hs.DanhGia,
                    hs.HanhDong,
                    hs.TrangThai
                FROM HieuSuat hs
                INNER JOIN NhanVienChiTiet nv ON hs.MaNhanVien = nv.MaNhanVien
                INNER JOIN NguoiDung nd ON nv.MaNguoiDung = nd.MaNguoiDung
                WHERE hs.MaChiNhanh = @BranchId
                    AND hs.ThangNam = @ThangNam
                ORDER BY nd.HoTen";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@ThangNam", thangNam);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetHieuSuatByMonth: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Lấy lịch sử hiệu suất của nhân viên
        /// </summary>
        public DataTable GetHieuSuatByNhanVien(int nhanVienId, int year)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    MaHieuSuat,
                    ThangNam,
                    DiemThaiDo,
                    DiemNangSuat,
                    DiemKyLuat,
                    DiemTrungBinh,
                    DanhGia,
                    HanhDong,
                    TrangThai
                FROM HieuSuat
                WHERE MaNhanVien = @NhanVienId
                    AND YEAR(NgayDanhGia) = @Year
                ORDER BY ThangNam DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NhanVienId", nhanVienId);
                        cmd.Parameters.AddWithValue("@Year", year);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetHieuSuatByNhanVien: {ex.Message}");
            }

            return dt;
        }

        // TODO: Thêm Save, Submit methods sau

        // ==================== LỊCH SỬ CÔNG TÁC ====================

        /// <summary>
        /// Lấy lịch sử công tác của nhân viên
        /// </summary>
        public DataTable GetLichSuLamViec(int nhanVienId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT 
                    MaLichSu,
                    NgayHanhDong,
                    HanhDong,
                    GhiChu
                FROM LichSuLamViec
                WHERE MaNhanVien = @NhanVienId
                ORDER BY NgayHanhDong DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NhanVienId", nhanVienId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi SQL GetLichSuLamViec: {ex.Message}");
            }

            return dt;
        }

        // TODO: Thêm Insert method sau
    }
}
