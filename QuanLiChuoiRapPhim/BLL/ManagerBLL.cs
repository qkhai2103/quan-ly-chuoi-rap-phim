using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiChuoiRapPhim.BLL
{
    /// <summary>
    /// ManagerBLL: Business Logic Layer cho vai trò Quản Lý chi nhánh
    /// Quản lý: Nhân sự, Lịch làm, Đơn xin nghỉ, Hiệu suất
    /// </summary>
    internal class ManagerBLL
    {
        // TODO: Tạo ManagerDAL sau khi hoàn thành SQL Tables
        // private ManagerDAL _managerDAL = new ManagerDAL();

        /// <summary>
        /// Lấy danh sách nhân viên của chi nhánh (chỉ Quản Lý của chi nhánh đó)
        /// </summary>
        public DataTable GetBranchEmployees(int managerId, int branchId)
        {
            try
            {
                // TODO: Gọi ManagerDAL.GetNhanVienByBranch(branchId)
                // 1. Kiểm tra Manager có quản lý chi nhánh này không
                // 2. Lấy dữ liệu từ NhanVienChiTiet + NguoiDung
                // 3. Trả về DataTable

                // Tạm thời trả về DataTable rỗng
                DataTable dt = new DataTable();
                dt.Columns.Add("MaNhanVien", typeof(int));
                dt.Columns.Add("MaNguoiDung", typeof(int));
                dt.Columns.Add("HoTen", typeof(string));
                dt.Columns.Add("Email", typeof(string));
                dt.Columns.Add("SoDienThoai", typeof(string));
                dt.Columns.Add("ViTri", typeof(string));
                dt.Columns.Add("TrangThai", typeof(string));
                dt.Columns.Add("NgayTao", typeof(DateTime));

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách nhân viên: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy chi tiết nhân viên
        /// </summary>
        public DataRow GetNhanVienDetail(int nhanVienId)
        {
            try
            {
                // TODO: Gọi ManagerDAL.GetNhanVienDetail(nhanVienId)
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy chi tiết nhân viên: {ex.Message}");
            }
        }

        /// <summary>
        /// Thêm nhân viên mới (chỉ Admin)
        /// </summary>
        public bool AddNhanVien(object nhanVienInfo, out string errorMessage)
        {
            errorMessage = "Chức năng này dành cho Admin";
            return false;
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        public bool UpdateNhanVien(object nhanVienInfo, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // TODO: Kiểm tra quyền Manager xem chi nhánh
                // TODO: Gọi ManagerDAL.UpdateNhanVien()
                // TODO: Ghi lại LichSuLamViec

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Xóa nhân viên (chỉ Admin)
        /// </summary>
        public bool DeleteNhanVien(int nhanVienId, out string errorMessage)
        {
            errorMessage = "Chức năng này dành cho Admin";
            return false;
        }

        /// <summary>
        /// Phân công ca làm việc
        /// </summary>
        public bool AssignShift(int nhanVienId, DateTime ngayLam, string ca, string viTri, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // TODO: Kiểm tra xung đột ca làm
                // TODO: Kiểm tra nhân viên có trong chi nhánh không
                // TODO: Gọi ManagerDAL.AddLichLam()

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xung đột ca làm
        /// </summary>
        public bool CheckShiftConflict(int nhanVienId, DateTime ngayLam)
        {
            try
            {
                // TODO: Kiểm tra xem nhân viên đã có ca làm ngày đó chưa
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy lịch làm việc của nhân viên (30 ngày gần nhất)
        /// </summary>
        public DataTable GetNhanVienSchedule(int nhanVienId, int days = 30)
        {
            try
            {
                // TODO: Gọi ManagerDAL.GetLichLamByNhanVien()
                // TODO: Lọc từ Today - 30 days
                return new DataTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy lịch làm: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy lịch làm của chi nhánh trong khoảng thời gian
        /// </summary>
        public DataTable GetBranchSchedule(int branchId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                // TODO: Gọi ManagerDAL.GetLichLamByBranch()
                return new DataTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy lịch chi nhánh: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách đơn xin nghỉ chờ duyệt của chi nhánh
        /// </summary>
        public DataTable GetPendingLeaveRequests(int branchId)
        {
            try
            {
                // TODO: Gọi ManagerDAL.GetDonXinNghiPending()
                // WHERE MaChiNhanh = branchId AND TrangThai = 'Cho'
                return new DataTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy đơn xin nghỉ: {ex.Message}");
            }
        }

        /// <summary>
        /// Duyệt đơn xin nghỉ
        /// </summary>
        public bool ApproveLeaveRequest(int donXinNghiId, int managerId, string ghiChu, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // TODO: Cập nhật DonXinNghi: TrangThai = 'DuocPheDuyet', NguoiPD = managerId, GhiChuPD = ghiChu
                // TODO: Tự động cập nhật LichLamViec: Thêm OffDay hoặc xóa ca dự kiến
                // TODO: Ghi lại LichSuLamViec

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Từ chối đơn xin nghỉ
        /// </summary>
        public bool RejectLeaveRequest(int donXinNghiId, int managerId, string lyDo, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // TODO: Cập nhật DonXinNghi: TrangThai = 'TuChoi', NguoiPD = managerId, GhiChuPD = lyDo
                // TODO: Ghi lại LichSuLamViec

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra đơn xin nghỉ có hợp lệ không
        /// </summary>
        public bool ValidateLeaveRequest(int nhanVienId, DateTime ngayBatDau, DateTime ngayKetThuc, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // Kiểm tra ngày
                if (ngayBatDau > ngayKetThuc)
                {
                    errorMessage = "Ngày bắt đầu không được sau ngày kết thúc";
                    return false;
                }

                if (ngayBatDau < DateTime.Today)
                {
                    errorMessage = "Không thể xin nghỉ những ngày đã qua";
                    return false;
                }

                // TODO: Kiểm tra số ngày phép còn lại
                // TODO: Kiểm tra có xung đột với lịch đã duyệt không

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên có thể thay thế (cùng vị trí, cùng chi nhánh, ngày đó rảnh)
        /// </summary>
        public DataTable GetAvailableReplacements(int branchId, DateTime ngayLam)
        {
            try
            {
                // TODO: Gọi ManagerDAL query
                // Lọc: Cùng chi nhánh, cùng vị trí, ngày đó không có lịch làm

                return new DataTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách thay thế: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy hiệu suất của chi nhánh theo tháng
        /// </summary>
        public DataTable GetPerformanceByMonth(int branchId, string thangNam)
        {
            try
            {
                // TODO: Gọi ManagerDAL.GetHieuSuatByMonth()
                // thangNam: 'YYYY-MM' ví dụ '2024-12'

                return new DataTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy hiệu suất: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu đánh giá hiệu suất nhân viên
        /// </summary>
        public bool SavePerformanceScore(object hieuSuatInfo, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // TODO: Kiểm tra dữ liệu hợp lệ (0-10, không để trống)
                // TODO: Tính điểm trung bình
                // TODO: Gọi ManagerDAL.SaveHieuSuat()

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Tính điểm trung bình hiệu suất
        /// </summary>
        public float CalculateAverageScore(float thaiDo, float nangSuat, float kyLuat)
        {
            try
            {
                if (thaiDo < 0 || thaiDo > 10 || nangSuat < 0 || nangSuat > 10 || kyLuat < 0 || kyLuat > 10)
                {
                    throw new ArgumentException("Điểm phải từ 0 đến 10");
                }

                float average = (thaiDo + nangSuat + kyLuat) / 3f;
                return (float)Math.Round(average, 2);
            }
            catch
            {
                return 0f;
            }
        }

        /// <summary>
        /// Gửi đánh giá hiệu suất lên phê duyệt (cho Admin duyệt)
        /// </summary>
        public bool SubmitPerformanceForApproval(int hieuSuatId, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // TODO: Cập nhật HieuSuat: TrangThai = 'DaGui'
                // TODO: Ghi lại thời gian gửi

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Lấy thông tin chi nhánh
        /// </summary>
        public DataRow GetBranchInfo(int branchId)
        {
            try
            {
                // TODO: Query bảng ChiNhanh
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy thông tin chi nhánh: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra Quản Lý có quyền quản lý chi nhánh này không
        /// </summary>
        public bool HasPermission(int managerId, int branchId)
        {
            try
            {
                // TODO: Query NguoiDung.MaChiNhanh = branchId WHERE MaNguoiDung = managerId
                // TODO: Kiểm tra VaiTro = 'QuanLy'

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy lịch sử công tác của nhân viên
        /// </summary>
        public DataTable GetWorkHistory(int nhanVienId)
        {
            try
            {
                // TODO: Gọi ManagerDAL.GetLichSuLamViec()
                return new DataTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy lịch sử: {ex.Message}");
            }
        }

        /// <summary>
        /// Thêm sự kiện vào lịch sử công tác
        /// </summary>
        public bool AddWorkHistory(int nhanVienId, string hanhDong, string ghiChu, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // TODO: Gọi ManagerDAL.AddLichSuLamViec()
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
