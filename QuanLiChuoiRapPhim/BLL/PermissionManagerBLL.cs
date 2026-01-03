using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public static class PermissionManager
    {
        private static Dictionary<string, Dictionary<string, bool>> permissionMatrix = new Dictionary<string, Dictionary<string, bool>>
        {
            // Admin: Toàn quyền hệ thống
            {
                "Admin", new Dictionary<string, bool>
                {
                    // Quản lý người dùng
                    {"UserManagement", true},
                    {"UserView", true},
                    {"UserAdd", true},
                    {"UserEdit", true},
                    {"UserDelete", true},
                    {"UserResetPassword", true},
                    
                    // Quản lý chi nhánh
                    {"BranchManagement", true},
                    {"BranchAdd", true},
                    {"BranchEdit", true},
                    {"BranchDelete", true},
                    
                    // Quản lý phòng chiếu
                    {"RoomManagement", true},
                    {"RoomAdd", true},
                    {"RoomEdit", true},
                    {"RoomDelete", true},
                    {"SeatManagement", true},
                    
                    // Quản lý phim
                    {"MovieManagement", true},
                    {"MovieView", true},
                    {"MovieAdd", true},
                    {"MovieEdit", true},
                    {"MovieDelete", true},
                    
                    // Quản lý lịch chiếu
                    {"ShowtimeManagement", true},
                    {"ShowtimeView", true},
                    {"ShowtimeAdd", true},
                    {"ShowtimeEdit", true},
                    {"ShowtimeDelete", true},
                    
                    // Quản lý giá vé
                    {"PriceManagement", true},
                    {"PriceView", true},
                    {"PriceEdit", true},
                    
                    // Tài chính
                    {"FinanceManagement", true},
                    {"RevenueViewAll", true},
                    {"RevenueExport", true},
                    
                    // Báo cáo
                    {"ReportManagement", true},
                    {"SystemReport", true},
                    {"TrendAnalysis", true},
                    
                    // Khuyến mãi
                    {"PromotionManagement", true},
                    {"PromotionAdd", true},
                    {"PromotionEdit", true},
                    {"PromotionDelete", true},
                    
                    // Quản lý nhân sự
                    {"StaffManagement", true},
                    {"StaffAdd", true},
                    {"StaffEdit", true},
                    {"StaffDelete", true},
                    {"PerformanceReview", true},
                    
                    // Hệ thống
                    {"SystemSettings", true},
                    {"DatabaseBackup", true},
                    {"SystemLog", true},
                    
                    // Bán vé (chỉ xem)
                    {"TicketSalesView", true},
                    {"TicketSell", false},
                    {"TicketCancel", false},
                    
                    // Kho hàng
                    {"InventoryView", true},
                    {"InventoryManagement", true},
                    
                    // Lịch làm việc
                    {"WorkScheduleView", true},
                    {"WorkScheduleManagement", true}
                }
            },
            
            // Quản lý: Quyền trong chi nhánh
            {
                "Quản lý", new Dictionary<string, bool>
                {
                    // Quản lý người dùng (chỉ trong chi nhánh)
                    {"UserManagement", false},
                    {"UserView", true},
                    {"UserAdd", true},    // Chỉ thêm Nhân viên
                    {"UserEdit", true},   // Chỉ sửa Nhân viên
                    {"UserDelete", true}, // Chỉ xóa Nhân viên
                    {"UserResetPassword", true},
                    
                    // Quản lý chi nhánh (chỉ xem)
                    {"BranchManagement", false},
                    {"BranchAdd", false},
                    {"BranchEdit", false},
                    {"BranchDelete", false},
                    
                    // Quản lý phòng chiếu (chỉ trong chi nhánh)
                    {"RoomManagement", true},
                    {"RoomAdd", false},
                    {"RoomEdit", true},   // Sửa thông tin phòng
                    {"RoomDelete", false},
                    {"SeatManagement", true}, // Quản lý ghế hỏng
                    
                    // Quản lý phim (chỉ xem)
                    {"MovieManagement", false},
                    {"MovieView", true},
                    {"MovieAdd", false},
                    {"MovieEdit", false},
                    {"MovieDelete", false},
                    
                    // Quản lý lịch chiếu (trong chi nhánh)
                    {"ShowtimeManagement", true},
                    {"ShowtimeView", true},
                    {"ShowtimeAdd", true},
                    {"ShowtimeEdit", true},
                    {"ShowtimeDelete", true},
                    
                    // Quản lý giá vé (chỉ xem)
                    {"PriceManagement", false},
                    {"PriceView", true},
                    {"PriceEdit", false},
                    
                    // Tài chính (chỉ trong chi nhánh)
                    {"FinanceManagement", true},
                    {"RevenueViewAll", false},
                    {"RevenueViewBranch", true},
                    {"RevenueExport", true},
                    
                    // Báo cáo chi nhánh
                    {"ReportManagement", true},
                    {"BranchReport", true},
                    {"BranchReports", true},    // Thêm quyền xem báo cáo doanh thu chi nhánh
                    {"TrendAnalysis", false},
                    
                    // Khuyến mãi (chỉ áp dụng)
                    {"PromotionManagement", false},
                    {"PromotionView", true},
                    {"PromotionApply", true},
                    
                    // Quản lý nhân sự (trong chi nhánh)
                    {"StaffManagement", true},
                    {"StaffAdd", true},
                    {"StaffEdit", true},
                    {"StaffDelete", true},
                    {"PerformanceReview", true},
                    
                    // Hệ thống (không có quyền)
                    {"SystemSettings", false},
                    {"DatabaseBackup", false},
                    {"SystemLog", false},
                    
                    // Bán vé (có thể bán)
                    {"TicketSalesView", true},
                    {"TicketSell", true},
                    {"TicketCancel", true},
                    {"TicketRefund", true},
                    
                    // Kho hàng (toàn quyền trong chi nhánh)
                    {"InventoryView", true},
                    {"InventoryManagement", true},
                    {"InventoryAdd", true},
                    {"InventoryEdit", true},
                    {"InventoryDelete", true},
                    
                    // Lịch làm việc
                    {"WorkScheduleView", true},
                    {"WorkSchedule", true},            // Thêm quyền WorkSchedule cho Quản lý
                    {"WorkScheduleManagement", true},  // Thêm quyền quản lý lịch làm việc
                    {"RoomIssueReport", true},         // Thêm quyền báo lỗi phòng
                    {"ShowtimeProposal", true}         // Thêm quyền đề xuất lịch chiếu
                }
            },
            
            // Nhân viên: Quyền hạn chế
            {
                "Nhân viên", new Dictionary<string, bool>
                {
                    // Quản lý người dùng - không có quyền
                    {"UserManagement", false}, {"UserView", false}, {"UserAdd", false}, {"UserEdit", false}, {"UserDelete", false}, {"UserResetPassword", false},
                    {"BranchManagement", false}, {"BranchAdd", false}, {"BranchEdit", false}, {"BranchDelete", false},
                    {"RoomManagement", false}, {"RoomView", true}, {"RoomAdd", false}, {"RoomEdit", false}, {"RoomDelete", false}, {"SeatView", true},
                    {"MovieManagement", false}, {"MovieView", true}, {"MovieAdd", false}, {"MovieEdit", false}, {"MovieDelete", false},
                    {"ShowtimeManagement", false}, {"ShowtimeView", true}, {"ShowtimeAdd", false}, {"ShowtimeEdit", false}, {"ShowtimeDelete", false},
                    {"PriceManagement", false}, {"PriceView", true}, {"PriceEdit", false},
                    {"FinanceManagement", false}, {"RevenueViewAll", false}, {"RevenueViewPersonal", true},
                    {"ReportManagement", false}, {"PersonalReport", true}, {"PersonalReports", true}, {"IssueReport", true},
                    {"PromotionManagement", false}, {"PromotionView", true}, {"PromotionApply", true},
                    {"StaffManagement", false}, {"StaffView", false}, {"StaffAdd", false}, {"StaffEdit", false}, {"StaffDelete", false}, {"PerformanceReview", false},
                    {"SystemSettings", false}, {"DatabaseBackup", false}, {"SystemLog", false},
                    // Bán vé - quyền chính của nhân viên
                    {"TicketSales", true}, {"TicketSalesView", true}, {"TicketSell", true}, {"TicketCancel", true}, {"TicketRefund", false},
                    {"InventoryView", true}, {"InventoryManagement", false}, {"InventorySell", true}, {"InventoryAdd", false}, {"InventoryEdit", false}, {"InventoryDelete", false},
                    // Lịch làm việc - chỉ xem của mình
                    {"WorkSchedule", true}, {"WorkScheduleView", true}, {"WorkScheduleManagement", false}, {"LeaveRequest", true},
                    {"CustomerView", true}, {"CustomerAdd", true}, {"CustomerEdit", false},
                    {"PaymentProcess", true}, {"InvoicePrint", true}, {"ShiftReport", true},
                    // Đổi mật khẩu - cho phép nhân viên đổi mật khẩu của mình
                    {"ChangePassword", true},
                    // Dashboard
                    {"Dashboard", true}
                }
            }
        };

        // Static constructor to initialize Vietnamese aliases
        static PermissionManager()
        {
            // Điểm danh các quyền tương đương cho tiếng Việt
            permissionMatrix["Quản trị viên"] = permissionMatrix["Admin"];
            permissionMatrix["Quản lý chi nhánh"] = permissionMatrix["Quản lý"];
            permissionMatrix["Nhân viên bán vé"] = permissionMatrix["Nhân viên"];
            permissionMatrix["Nhân viên lịch chiếu"] = permissionMatrix["Nhân viên"];
        }

        public static bool CheckPermission(string feature, string userRole)
        {
            if (permissionMatrix.ContainsKey(userRole) &&
                permissionMatrix[userRole].ContainsKey(feature))
            {
                return permissionMatrix[userRole][feature];
            }
            return false; // Mặc định không có quyền nếu không tìm thấy
        }

        public static bool CheckPermissionWithMessage(string feature, string userRole, Form parentForm)
        {
            if (!CheckPermission(feature, userRole))
            {
                MessageBox.Show($"Bạn không có quyền truy cập chức năng này!\n\n" +
                    $"Vai trò: {userRole}\n" +
                    $"Yêu cầu quyền: {feature}",
                    "Truy cập bị từ chối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public static List<SidebarMenuItem> GetMenusByRole(string userRole, int branchId = 0, int userId = 0)
        {
            var menus = new List<SidebarMenuItem>();

            if (userRole == "Admin" || userRole == "Quản trị viên")
            {
                menus.AddRange(new[]
                {
                    new SidebarMenuItem { Text = "Dashboard", Icon = "📊", Feature = "Dashboard" },
                    new SidebarMenuItem { Text = "Quản lý Người dùng", Icon = "👥", Feature = "UserManagement" },
                    new SidebarMenuItem { Text = "Quản lý Chi nhánh", Icon = "🏢", Feature = "BranchManagement" },
                    new SidebarMenuItem { Text = "Quản lý Phim", Icon = "🎬", Feature = "MovieManagement" },
                    new SidebarMenuItem { Text = "Lịch chiếu", Icon = "📅", Feature = "ShowtimeManagement" },
                    new SidebarMenuItem { Text = "Quản lý Giá vé", Icon = "💰", Feature = "PriceManagement" },
                    new SidebarMenuItem { Text = "Tài chính Toàn hệ thống", Icon = "💳", Feature = "FinanceManagement" },
                    new SidebarMenuItem { Text = "Báo cáo & Phân tích", Icon = "📈", Feature = "ReportManagement" },
                    new SidebarMenuItem { Text = "Khuyến mãi", Icon = "🎁", Feature = "PromotionManagement" },
                    new SidebarMenuItem { Text = "Kho hàng", Icon = "📦", Feature = "InventoryManagement" },
                    new SidebarMenuItem { Text = "Cài đặt Hệ thống", Icon = "⚙️", Feature = "SystemSettings" }
                });
            }
            else if (userRole == "Quản lý" || userRole == "Quản lý chi nhánh")
            {
                menus.AddRange(new[]
                {
                    new SidebarMenuItem { Text = "Dashboard", Icon = "📊", Feature = "Dashboard" },
                    new SidebarMenuItem { Text = "Nhân viên", Icon = "👨‍💼", Feature = "StaffManagement" },
                    new SidebarMenuItem { Text = "Danh sách Phim", Icon = "🎬", Feature = "MovieView" },
                    new SidebarMenuItem { Text = "Lịch chiếu", Icon = "📅", Feature = "ShowtimeManagement" },
                    new SidebarMenuItem { Text = "Bán vé", Icon = "🎫", Feature = "TicketSalesView" },
                    new SidebarMenuItem { Text = "Kho hàng", Icon = "📦", Feature = "InventoryManagement" },
                    new SidebarMenuItem { Text = "Doanh thu Chi nhánh", Icon = "💳", Feature = "FinanceManagement" },
                    new SidebarMenuItem { Text = "Báo cáo Chi nhánh", Icon = "📈", Feature = "ReportManagement" },
                    new SidebarMenuItem { Text = "Lịch làm việc", Icon = "⏰", Feature = "WorkScheduleManagement" },
                    new SidebarMenuItem { Text = "Đánh giá Hiệu suất", Icon = "⭐", Feature = "PerformanceReview" }
                });
            }
            else if (userRole == "Nhân viên" || userRole.StartsWith("Nhân viên"))
            {
                menus.AddRange(new[]
                {
                    new SidebarMenuItem { Text = "Dashboard", Icon = "📊", Feature = "Dashboard" },
                    new SidebarMenuItem { Text = "Bán vé", Icon = "🎫", Feature = "TicketSalesView" },
                    new SidebarMenuItem { Text = "Bán Bắp nước", Icon = "🍿", Feature = "InventorySell" },
                    new SidebarMenuItem { Text = "Danh sách Phim", Icon = "🎬", Feature = "MovieView" },
                    new SidebarMenuItem { Text = "Lịch chiếu", Icon = "📅", Feature = "ShowtimeView" },
                    new SidebarMenuItem { Text = "Khách hàng", Icon = "👤", Feature = "CustomerView" },
                    new SidebarMenuItem { Text = "Tồn kho", Icon = "📦", Feature = "InventoryView" },
                    new SidebarMenuItem { Text = "Báo cáo Cá nhân", Icon = "📊", Feature = "PersonalReport" },
                    new SidebarMenuItem { Text = "Lịch làm việc", Icon = "⏰", Feature = "WorkScheduleView" },
                    new SidebarMenuItem { Text = "Xin nghỉ phép", Icon = "📋", Feature = "LeaveRequest" },
                    new SidebarMenuItem { Text = "Báo cáo sự cố", Icon = "⚠️", Feature = "IssueReport" }
                });
            }

            // Lọc menu theo quyền thực tế
            return menus.Where(menu => CheckPermission(menu.Feature, userRole)).ToList();
        }

        public static string[][] GetQuickActionsByRole(string userRole)
        {
            if (userRole == "Admin" || userRole == "Quản trị viên")
            {
                return new string[][]
                {
                    new string[] { "THÊM NHÂN VIÊN", "👥", "UserAdd" },
                    new string[] { "THÊM CHI NHÁNH", "🏢", "BranchAdd" },
                    new string[] { "THÊM PHIM MỚI", "🎬", "MovieAdd" },
                    new string[] { "XEM BÁO CÁO", "📈", "ReportManagement" },
                    new string[] { "CẤU HÌNH GIÁ", "💰", "PriceManagement" },
                    new string[] { "SAO LƯU DB", "💾", "DatabaseBackup" }
                };
            }
            else if (userRole == "Quản lý" || userRole == "Quản lý chi nhánh")
            {
                return new string[][]
                {
                    new string[] { "BÁN VÉ NHANH", "🎟️", "TicketSell" },
                    new string[] { "PHÂN CÔNG CA", "📅", "WorkScheduleManagement" },
                    new string[] { "KIỂM KHO", "📦", "InventoryManagement" },
                    new string[] { "XEM BÁO CÁO", "📊", "ReportManagement" },
                    new string[] { "THÊM NHÂN VIÊN", "👥", "StaffAdd" },
                    new string[] { "DUYỆT ĐƠN NGHỈ", "✅", "LeaveApprove" }
                };
            }
            else
            {
                return new string[][]
                {
                    new string[] { "BÁN VÉ", "🎫", "TicketSell" },
                    new string[] { "ĐẶT GHẾ", "💺", "SeatSelect" },
                    new string[] { "BÁN SẢN PHẨM", "🍿", "InventorySell" },
                    new string[] { "XEM LỊCH LÀM", "📅", "WorkScheduleView" },
                    new string[] { "BÁO CÁO SỰ CỐ", "⚠️", "IssueReport" },
                    new string[] { "XIN NGHỈ PHÉP", "📋", "LeaveRequest" }
                };
            }
        }

        public static bool CanModifyUser(string currentUserRole, string targetUserRole)
        {
            // Admin có thể sửa tất cả
            if (currentUserRole == "Admin" || currentUserRole == "Quản trị viên") return true;

            // Quản lý chỉ được sửa Nhân viên
            if ((currentUserRole == "Quản lý" || currentUserRole == "Quản lý chi nhánh") && 
                (targetUserRole == "Nhân viên" || targetUserRole.StartsWith("Nhân viên"))) return true;

            // Nhân viên không được sửa ai
            return false;
        }

        public static bool CanDeleteUser(string currentUserRole, string targetUserRole)
        {
            // Admin có thể xóa tất cả (trừ chính mình)
            if ((currentUserRole == "Admin" || currentUserRole == "Quản trị viên") && 
                targetUserRole != "Admin" && targetUserRole != "Quản trị viên") return true;

            // Quản lý chỉ được xóa Nhân viên
            if ((currentUserRole == "Quản lý" || currentUserRole == "Quản lý chi nhánh") && 
                (targetUserRole == "Nhân viên" || targetUserRole.StartsWith("Nhân viên"))) return true;

            return false;
        }

        public static bool CanViewBranchData(string currentUserRole, int currentBranchId, int targetBranchId)
        {
            // Admin xem được tất cả
            if (currentUserRole == "Admin" || currentUserRole == "Quản trị viên") return true;

            // Quản lý và Nhân viên chỉ xem được chi nhánh của mình
            return currentBranchId == targetBranchId;
        }
    }
}