
using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class frmMain : Form
    {
        private string _username;
        private string _userRole;
        private string _branch;
        private string _fullName;
        private int _maNguoiDung;          // ID người dùng từ database
        private int _maChiNhanh;           // Mã chi nhánh từ database

        // UI Components
        private Panel _sidebar;
        private Panel _mainContentPanel;
        private Panel _header;
        private Panel _notificationPanel;

        // Sidebar menu items
        private List<SidebarMenuItem> _menuItems;
        private SidebarMenuItem _activeMenuItem;

        // TabControl for Reports (UC_BaoCaoChiNhanh)
        private TabControl _reportTabControl;
        private UC_BaoCaoChiNhanh _currentBaoCaoControl;

        // CGV Theme colors - Updated to match CGV branding
        private Color _cgvRed = Color.FromArgb(226, 26, 60);       // CGV Primary Red (#E21A3C)
        private Color _cgvDarkRed = Color.FromArgb(180, 20, 45);   // Darker Red
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);      // Pure Black
        private Color _cgvDarkGray = Color.FromArgb(40, 40, 40);   // Dark Gray
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245); // Light Background
        private Color _cgvWhite = Color.White;
        private Color _cgvGold = Color.FromArgb(255, 215, 0);      // CGV Gold
        private Color _cgvSilver = Color.FromArgb(192, 192, 192);  // Silver
        private Color _cgvTextColor = Color.FromArgb(60, 60, 60);  // Dark Text Color

        // Fonts - Updated to match CGV modern style
        private Font _cgvTitleFont = new Font("Montserrat", 26, FontStyle.Bold);
        private Font _cgvHeaderFont = new Font("Montserrat", 20, FontStyle.Bold);
        private Font _cgvSubHeaderFont = new Font("Montserrat", 16, FontStyle.Bold);
        private Font _cgvNormalFont = new Font("Segoe UI", 11, FontStyle.Regular);
        private Font _cgvSmallFont = new Font("Segoe UI", 10, FontStyle.Regular);
        private Font _cgvBoldFont = new Font("Segoe UI", 11, FontStyle.Bold);
        private Font _cgvMenuFont = new Font("Segoe UI", 12, FontStyle.Regular);

        public frmMain(string username, string userRole, string branch, string fullName, int maNguoiDung = 0, int maChiNhanh = 0)
        {
            InitializeComponent();
            
            _username = username;
            _userRole = userRole;
            _branch = branch;
            _fullName = fullName;
            _maNguoiDung = maNguoiDung;
            _maChiNhanh = maChiNhanh;

            SetupModernUI();
            InitializeMenuItems();
            LoadHomeDashboard();
        }

        private void SetupModernUI()
        {
            // Cấu hình form
            this.Text = $"HỆ THỐNG QUẢN LÝ CGV | {_userRole.ToUpper()} - {_branch}";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _cgvLightGray;
            this.WindowState = FormWindowState.Maximized;

            // ========== HEADER ==========
            _header = new Panel();
            _header.Dock = DockStyle.Top;
            _header.Height = 70;
            _header.BackColor = _cgvBlack;
            _header.Padding = new Padding(30, 0, 30, 0);

            // CGV Logo Image
            PictureBox picLogo = new PictureBox();
            picLogo.Size = new Size(120, 60);
            picLogo.Location = new Point(30, 5);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.BackColor = Color.Transparent;
            picLogo.Cursor = Cursors.Hand;
            
            try
            {
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    picLogo.Image = Image.FromFile(logoPath);
                }
                else
                {
                    // Fallback to text if image not found
                    Label lblLogoFallback = new Label();
                    lblLogoFallback.Text = "CGV";
                    lblLogoFallback.Font = new Font("Montserrat", 28, FontStyle.Bold);
                    lblLogoFallback.ForeColor = _cgvRed;
                    lblLogoFallback.Location = new Point(30, 15);
                    lblLogoFallback.AutoSize = true;
                    _header.Controls.Add(lblLogoFallback);
                }
            }
            catch
            {
                // Fallback if error loading image
                Label lblLogoFallback = new Label();
                lblLogoFallback.Text = "CGV";
                lblLogoFallback.Font = new Font("Montserrat", 28, FontStyle.Bold);
                lblLogoFallback.ForeColor = _cgvRed;
                lblLogoFallback.Location = new Point(30, 15);
                lblLogoFallback.AutoSize = true;
                _header.Controls.Add(lblLogoFallback);
            }


            // User info panel
            Panel userPanel = new Panel();
            userPanel.Size = new Size(400, 70);
            userPanel.Location = new Point(_header.Width - 450, 0);
            userPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            userPanel.BackColor = Color.Transparent;

            // Notification bell
            Panel notificationPanel = new Panel();
            notificationPanel.Size = new Size(50, 50);
            notificationPanel.Location = new Point(20, 10);
            notificationPanel.BackColor = Color.Transparent;

            Button btnNotification = new Button();
            btnNotification.Text = "🔔";
            btnNotification.Font = new Font("Segoe UI", 20);
            btnNotification.Size = new Size(45, 45);
            btnNotification.Location = new Point(2, 2);
            btnNotification.FlatStyle = FlatStyle.Flat;
            btnNotification.FlatAppearance.BorderSize = 0;
            btnNotification.BackColor = Color.Transparent;
            btnNotification.ForeColor = _cgvSilver;
            btnNotification.Cursor = Cursors.Hand;

            // Notification badge
            Panel badgePanel = new Panel();
            badgePanel.Size = new Size(20, 20);
            badgePanel.Location = new Point(30, 0);
            badgePanel.BackColor = _cgvRed;
            badgePanel.BorderRadius(10);

            Label lblNotificationBadge = new Label();
            lblNotificationBadge.Text = "3";
            lblNotificationBadge.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblNotificationBadge.ForeColor = Color.White;
            lblNotificationBadge.Dock = DockStyle.Fill;
            lblNotificationBadge.TextAlign = ContentAlignment.MiddleCenter;

            badgePanel.Controls.Add(lblNotificationBadge);
            notificationPanel.Controls.Add(badgePanel);
            notificationPanel.Controls.Add(btnNotification);

            // User avatar
            Panel avatarPanel = new Panel();
            avatarPanel.Size = new Size(45, 45);
            avatarPanel.Location = new Point(100, 12);
            avatarPanel.BackColor = _cgvRed;
            avatarPanel.BorderRadius(22);

            // Add gradient effect
            avatarPanel.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    avatarPanel.ClientRectangle,
                    _cgvRed,
                    _cgvDarkRed,
                    LinearGradientMode.ForwardDiagonal))
                {
                    e.Graphics.FillEllipse(brush, avatarPanel.ClientRectangle);
                }
            };

            Label lblAvatar = new Label();
            string avatarText = !string.IsNullOrEmpty(_fullName) ? _fullName.Substring(0, 1).ToUpper() : "?";
            lblAvatar.Text = avatarText;
            lblAvatar.Font = new Font("Montserrat", 18, FontStyle.Bold);
            lblAvatar.ForeColor = Color.White;
            lblAvatar.Dock = DockStyle.Fill;
            lblAvatar.TextAlign = ContentAlignment.MiddleCenter;
            avatarPanel.Controls.Add(lblAvatar);

            // User info
            Label lblUserInfo = new Label();
            lblUserInfo.Text = $"{_fullName.ToUpper()}\n{_userRole} • {_branch}";
            lblUserInfo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblUserInfo.ForeColor = _cgvSilver;
            lblUserInfo.AutoSize = true;
            lblUserInfo.Location = new Point(160, 15);
            lblUserInfo.TextAlign = ContentAlignment.MiddleLeft;

            // Settings dropdown
            Button btnSettings = new Button();
            btnSettings.Text = "⚙️";
            btnSettings.Font = new Font("Segoe UI", 18);
            btnSettings.Size = new Size(45, 45);
            btnSettings.Location = new Point(320, 12);
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.BackColor = Color.Transparent;
            btnSettings.ForeColor = _cgvSilver;
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Click += (s, e) => ShowSettingsMenu(btnSettings);

            userPanel.Controls.AddRange(new Control[] {
                notificationPanel, avatarPanel, lblUserInfo, btnSettings
            });

            _header.Controls.AddRange(new Control[] { picLogo, userPanel });

            // ========== SIDEBAR ==========
            _sidebar = new Panel();
            _sidebar.Dock = DockStyle.Left;
            _sidebar.Width = 300;
            _sidebar.BackColor = _cgvBlack;
            _sidebar.AutoScroll = true;
            _sidebar.Padding = new Padding(0, 20, 0, 0);

            // Sidebar header
            Panel sidebarHeader = new Panel();
            sidebarHeader.Dock = DockStyle.Top;
            sidebarHeader.Height = 100;
            sidebarHeader.BackColor = Color.Transparent;
            sidebarHeader.Padding = new Padding(25, 0, 0, 0);

            // Sidebar Logo Image
            PictureBox picSidebarLogo = new PictureBox();
            picSidebarLogo.Size = new Size(250, 80);
            picSidebarLogo.Location = new Point(25, 10);
            picSidebarLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picSidebarLogo.BackColor = Color.Transparent;
            
            try
            {
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    picSidebarLogo.Image = Image.FromFile(logoPath);
                }
            }
            catch { }

            Label lblSidebarSubHeader = new Label();
            lblSidebarSubHeader.Text = "BẢNG ĐIỀU KHIỂN";
            lblSidebarSubHeader.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblSidebarSubHeader.ForeColor = _cgvSilver;
            lblSidebarSubHeader.Dock = DockStyle.Top;
            lblSidebarSubHeader.Height = 30;
            lblSidebarSubHeader.TextAlign = ContentAlignment.MiddleLeft;

            Panel sidebarDivider = new Panel();
            sidebarDivider.Dock = DockStyle.Top;
            sidebarDivider.Height = 2;
            sidebarDivider.BackColor = Color.FromArgb(60, 60, 60);

            sidebarHeader.Controls.Add(sidebarDivider);
            sidebarHeader.Controls.Add(lblSidebarSubHeader);
            sidebarHeader.Controls.Add(picSidebarLogo);

            // ========== REPORT TAB CONTROL (UC_BaoCaoChiNhanh) ==========
            SetupReportTabControl();

            // Thêm header vào sidebar
            _sidebar.Controls.Add(sidebarHeader);

            // ========== MAIN CONTENT ==========
            _mainContentPanel = new Panel();
            _mainContentPanel.Dock = DockStyle.Fill;
            _mainContentPanel.BackColor = _cgvLightGray;
            _mainContentPanel.Padding = new Padding(25);
            _mainContentPanel.AutoScroll = true;

            // ========== ADD CONTROLS TO FORM ==========
            this.Controls.Add(_mainContentPanel);
            this.Controls.Add(_sidebar);
            this.Controls.Add(_header);
        }

        private void AddQuickActionsToHeader()
        {
            // Quick actions đã được di chuyển xuống dưới header
        }

        private List<SidebarMenuItem> CreateMenuItems()
        {
            var items = new List<SidebarMenuItem>();

            // Dashboard (All roles)
            items.Add(new SidebarMenuItem { Text = "Trang chủ", Icon = "🏠", Feature = "Dashboard" });

            if (_userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên")
            {
                // ━━━━━━━━━━━━━━━━━━━━━
                // 📋 QUẢN TRỊ HỆ THỐNG
                items.Add(new SidebarMenuItem { Text = "QUẢN TRỊ HỆ THỐNG", Icon = "📋", Feature = "GroupSystemManagement", IsGroupHeader = true, IsExpanded = true });
                items.Add(new SidebarMenuItem { Text = "Quản lý nhân sự", Icon = "👥", Feature = "UserManagement", ParentGroup = "GroupSystemManagement", IndentLevel = 1 });
                items.Add(new SidebarMenuItem { Text = "Quản lý chi nhánh", Icon = "🏢", Feature = "BranchManagement", ParentGroup = "GroupSystemManagement", IndentLevel = 1 });

                // ━━━━━━━━━━━━━━━━━━━━━
                // 🎬 VẬN HÀNH RẠP CHIẾU
                items.Add(new SidebarMenuItem { Text = "VẬN HÀNH RẠP CHIẾU", Icon = "🎬", Feature = "GroupCinemaOps", IsGroupHeader = true, IsExpanded = true });
                items.Add(new SidebarMenuItem { Text = "Quản lý phim", Icon = "🎞️", Feature = "MovieEdit", ParentGroup = "GroupCinemaOps", IndentLevel = 1 });
                items.Add(new SidebarMenuItem { Text = "Lịch chiếu", Icon = "📅", Feature = "ShowtimeEdit", ParentGroup = "GroupCinemaOps", IndentLevel = 1 });
                items.Add(new SidebarMenuItem { Text = "Phòng chiếu", Icon = "🪑", Feature = "RoomManagement", ParentGroup = "GroupCinemaOps", IndentLevel = 1 });
                items.Add(new SidebarMenuItem { Text = "Quản lý vé", Icon = "🎟️", Feature = "TicketManagement", ParentGroup = "GroupCinemaOps", IndentLevel = 1 });

                // ━━━━━━━━━━━━━━━━━━━━━
                // 💰 TÀI CHÍNH & KHO BÃI
                items.Add(new SidebarMenuItem { Text = "TÀI CHÍNH & KHO BÃI", Icon = "💰", Feature = "GroupFinance", IsGroupHeader = true, IsExpanded = true });
                items.Add(new SidebarMenuItem { Text = "Doanh thu hệ thống", Icon = "💵", Feature = "SystemRevenue", ParentGroup = "GroupFinance", IndentLevel = 1 });
                items.Add(new SidebarMenuItem { Text = "Quản lý kho", Icon = "📦", Feature = "InventoryManagement", ParentGroup = "GroupFinance", IndentLevel = 1 });
                items.Add(new SidebarMenuItem { Text = "Giá vé & Khuyến mãi", Icon = "💳", Feature = "PricingPromotion", ParentGroup = "GroupFinance", IndentLevel = 1 });

                // ━━━━━━━━━━━━━━━━━━━━━
                // 📊 BÁO CÁO & PHÂN TÍCH
                items.Add(new SidebarMenuItem { Text = "BÁO CÁO & PHÂN TÍCH", Icon = "📊", Feature = "GroupReports", IsGroupHeader = true, IsExpanded = false });
                items.Add(new SidebarMenuItem { Text = "Báo cáo tổng hợp", Icon = "📈", Feature = "BranchReports", ParentGroup = "GroupReports", IndentLevel = 1 });
                items.Add(new SidebarMenuItem { Text = "Phân tích xu hướng", Icon = "🎯", Feature = "TrendAnalysis", ParentGroup = "GroupReports", IndentLevel = 1 });
                items.Add(new SidebarMenuItem { Text = "Hiệu suất chi nhánh", Icon = "⭐", Feature = "BranchPerformance", ParentGroup = "GroupReports", IndentLevel = 1 });

                // ━━━━━━━━━━━━━━━━━━━━━
                // ⚙️ CÀI ĐẶT
                items.Add(new SidebarMenuItem { Text = "CÀI ĐẶT", Icon = "⚙️", Feature = "GroupSettings", IsGroupHeader = true, IsExpanded = false });
                items.Add(new SidebarMenuItem { Text = "Đổi mật khẩu", Icon = "🔐", Feature = "ChangePassword", ParentGroup = "GroupSettings", IndentLevel = 1 });
            }
            else if (_userRole == "Quản lý" || _userRole == "Branch Manager" || _userRole == "Quản lý chi nhánh")
            {
                items.Add(new SidebarMenuItem { Text = "Bán vé", Icon = "🎟️", Feature = "TicketSales" });
                items.Add(new SidebarMenuItem { Text = "Quản lý phim", Icon = "🎬", Feature = "MovieEdit" });
                items.Add(new SidebarMenuItem { Text = "Lịch chiếu", Icon = "📅", Feature = "ShowtimeEdit" });
                items.Add(new SidebarMenuItem { Text = "Quản lý kho", Icon = "📦", Feature = "InventoryManagement" });
                items.Add(new SidebarMenuItem { Text = "Nhân sự", Icon = "👔", Feature = "StaffManagement" });
                items.Add(new SidebarMenuItem { Text = "Lịch làm việc", Icon = "🗓️", Feature = "WorkSchedule" });
                items.Add(new SidebarMenuItem { Text = "Đánh giá", Icon = "📈", Feature = "PerformanceReview" });
                items.Add(new SidebarMenuItem { Text = "Báo cáo", Icon = "📊", Feature = "BranchReports" });
                items.Add(new SidebarMenuItem { Text = "Đổi mật khẩu", Icon = "🔐", Feature = "ChangePassword" });
            }
            else // Nhân viên
            {
                items.Add(new SidebarMenuItem { Text = "Bán vé", Icon = "🎟️", Feature = "TicketSales" });
                items.Add(new SidebarMenuItem { Text = "Phim đang chiếu", Icon = "🎬", Feature = "MovieView" });
                items.Add(new SidebarMenuItem { Text = "Lịch chiếu", Icon = "📅", Feature = "ShowtimeView" });
                items.Add(new SidebarMenuItem { Text = "Kho hàng", Icon = "📦", Feature = "InventoryView" });
                items.Add(new SidebarMenuItem { Text = "Lịch làm việc", Icon = "🗓️", Feature = "WorkSchedule" });
                items.Add(new SidebarMenuItem { Text = "Báo cáo cá nhân", Icon = "📈", Feature = "PersonalReports" });
                items.Add(new SidebarMenuItem { Text = "Đổi mật khẩu", Icon = "🔐", Feature = "ChangePassword" });
            }

            return items;
        }

        private void InitializeMenuItems()
        {
            // Lấy danh sách menu theo role từ PermissionManager
            _menuItems = CreateMenuItems();

            // Gán action cho từng menu item
            foreach (var menuItem in _menuItems)
            {
                if (!menuItem.IsGroupHeader)
                {
                    menuItem.Action = () => ExecuteMenuItem(menuItem);
                }
                else
                {
                    // Group headers toggle expand/collapse
                    menuItem.Action = () => ToggleGroupExpansion(menuItem);
                }
            }

            // Đánh dấu menu đầu tiên (không phải group header) là active
            var firstNonGroupItem = _menuItems.FirstOrDefault(m => !m.IsGroupHeader);
            if (firstNonGroupItem != null)
            {
                firstNonGroupItem.IsActive = true;
                _activeMenuItem = firstNonGroupItem;
            }

            // Create FlowLayoutPanel for menu items
            FlowLayoutPanel menuContainer = new FlowLayoutPanel();
            menuContainer.Dock = DockStyle.Fill;
            menuContainer.FlowDirection = FlowDirection.TopDown;
            menuContainer.WrapContents = false;
            menuContainer.AutoScroll = true;
            menuContainer.BackColor = Color.Transparent;
            menuContainer.Padding = new Padding(0, 120, 0, 20);

            // Add menu items to sidebar
            foreach (var menuItem in _menuItems)
            {
                // Check if this item should be visible based on parent group expansion
                bool isVisible = true;
                if (!string.IsNullOrEmpty(menuItem.ParentGroup))
                {
                    var parentGroup = _menuItems.FirstOrDefault(m => m.Feature == menuItem.ParentGroup);
                    isVisible = parentGroup != null && parentGroup.IsExpanded;
                }

                var button = CreateSidebarButton(menuItem);
                button.Visible = isVisible;
                menuContainer.Controls.Add(button);
            }

            _sidebar.Controls.Add(menuContainer);
        }

        private void ToggleGroupExpansion(SidebarMenuItem groupItem)
        {
            if (!groupItem.IsGroupHeader) return;

            // Toggle expansion state
            groupItem.IsExpanded = !groupItem.IsExpanded;

            // Update visibility of child items
            foreach (Control control in _sidebar.Controls)
            {
                if (control is FlowLayoutPanel flowPanel)
                {
                    foreach (Control btnControl in flowPanel.Controls)
                    {
                        if (btnControl is Button btn && btn.Tag is SidebarMenuItem item)
                        {
                            if (item.ParentGroup == groupItem.Feature)
                            {
                                btn.Visible = groupItem.IsExpanded;
                            }
                            
                            // Update group header icon (▼ when expanded, ▶ when collapsed)
                            if (item == groupItem)
                            {
                                string expandIcon = groupItem.IsExpanded ? "▼" : "▶";
                                btn.Text = $"{expandIcon} {item.Icon} {item.Text}";
                            }
                        }
                    }
                }
            }
        }
        private void ExecuteMenuItem(SidebarMenuItem menuItem)
        {
            // Kiểm tra quyền trước khi thực thi
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager" || _userRole == "Quản lý chi nhánh";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff" || _userRole == "Nhân viên bán vé" || _userRole == "Nhân viên lịch chiếu";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage(menuItem.Feature, _userRole, this))
                return;



            // Update active state
            foreach (Control control in _sidebar.Controls)
            {
                if (control is Button sidebarBtn && sidebarBtn.Tag is SidebarMenuItem item)
                {
                    sidebarBtn.BackColor = item == menuItem ? _cgvRed : Color.Transparent;
                    sidebarBtn.ForeColor = item == menuItem ? _cgvWhite : Color.FromArgb(180, 180, 180);
                    item.IsActive = (item == menuItem);
                }
            }

            _activeMenuItem = menuItem;

            // Thực thi action tương ứng
            switch (menuItem.Feature)
            {
                case "Dashboard":
                    LoadHomeDashboard();
                    break;
                case "UserManagement":
                    LoadAdminUsers();
                    break;
                case "BranchManagement":
                    LoadBranchManagement();
                    break;
                case "MovieView":
                case "MovieEdit":
                    LoadMovies();
                    break;
                case "ShowtimeView":
                case "ShowtimeEdit":
                    LoadShowtimes();
                    break;
                case "RoomManagement":
                    LoadRoomManagement();
                    break;
                case "TicketManagement":
                    LoadTicketManagement();
                    break;
                case "TicketSales":
                    LoadTicketSales();
                    break;
                case "SystemRevenue":
                    LoadSystemRevenue();
                    break;
                case "InventoryView":
                case "InventoryManagement":
                    LoadInventory();
                    break;
                case "PricingPromotion":
                    LoadPricingPromotion();
                    break;
                case "PersonalReports":
                case "BranchReports":
                    LoadReports();
                    break;
                case "TrendAnalysis":
                    LoadTrendAnalysis();
                    break;
                case "BranchPerformance":
                    LoadBranchPerformance();
                    break;
                case "WorkSchedule":
                    LoadWorkSchedule();
                    break;
                case "PerformanceReview":
                    LoadPerformance();
                    break;
                case "StaffManagement":
                    LoadStaffManagement();
                    break;
                case "AdminPanel":
                    LoadAdminPanel();
                    break;
                case "ChangePassword":
                    LoadSettings();
                    break;
                default:
                    LoadHomeDashboard();
                    break;
            }
        }

        private Button CreateSidebarButton(SidebarMenuItem menuItem)
        {
            Button btn = new Button();
            btn.Tag = menuItem;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Cursor = Cursors.Hand;
            btn.BorderRadius(8);
            btn.Margin = new Padding(5, 2, 5, 2);
            
            // Different styling for group headers vs regular items
            if (menuItem.IsGroupHeader)
            {
                string expandIcon = menuItem.IsExpanded ? "▼" : "▶";
                btn.Text = $"{expandIcon} {menuItem.Icon} {menuItem.Text}";
                btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                btn.ForeColor = _cgvGold;
                btn.BackColor = Color.FromArgb(25, 25, 25);
                btn.Height = 40;
                btn.Padding = new Padding(15, 0, 0, 0);
                btn.Width = _sidebar.Width - 20;
            }
            else
            {
                int leftPadding = 20 + (menuItem.IndentLevel * 20);
                btn.Text = $"  {menuItem.Icon}  {menuItem.Text}";
                btn.Font = _cgvMenuFont;
                btn.ForeColor = menuItem.IsActive ? _cgvWhite : Color.FromArgb(180, 180, 180);
                btn.BackColor = menuItem.IsActive ? _cgvRed : Color.Transparent;
                btn.Height = 50;
                btn.Padding = new Padding(leftPadding, 0, 0, 0);
                btn.Width = _sidebar.Width - 20;
                
                // Hover effects (only for non-group headers)
                btn.MouseEnter += (s, e) =>
                {
                    if (!menuItem.IsActive)
                    {
                        btn.BackColor = Color.FromArgb(50, 50, 50);
                        btn.ForeColor = _cgvWhite;
                        btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    }
                };

                btn.MouseLeave += (s, e) =>
                {
                    if (!menuItem.IsActive)
                    {
                        btn.BackColor = Color.Transparent;
                        btn.ForeColor = Color.FromArgb(180, 180, 180);
                        btn.Font = _cgvMenuFont;
                    }
                };
            }

            btn.Click += (s, e) =>
            {
                // Group headers just toggle expansion
                if (menuItem.IsGroupHeader)
                {
                    menuItem.Action?.Invoke();
                    return;
                }

                // ========== KIỂM TRA QUYỀN TRƯỚC KHI CLICK ==========
                bool isAdmin = _userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên";
                bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager" || _userRole == "Quản lý chi nhánh";
                bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff" || _userRole == "Nhân viên bán vé" || _userRole == "Nhân viên lịch chiếu";
                if (!isAdmin && !isManager && !isStaff && menuItem.Feature != null &&
                    !PermissionManager.CheckPermissionWithMessage(menuItem.Feature, _userRole, this))
                {
                    return;
                }

                // ========== UPDATE ACTIVE STATE ==========
                foreach (Control control in _sidebar.Controls)
                {
                    if (control is FlowLayoutPanel flowPanel)
                    {
                        foreach (Control btnControl in flowPanel.Controls)
                        {
                            if (btnControl is Button sidebarBtn && sidebarBtn.Tag is SidebarMenuItem item)
                            {
                                if (!item.IsGroupHeader)
                                {
                                    sidebarBtn.BackColor = item == menuItem ? _cgvRed : Color.Transparent;
                                    sidebarBtn.ForeColor = item == menuItem ? _cgvWhite : Color.FromArgb(180, 180, 180);
                                    item.IsActive = (item == menuItem);
                                }
                            }
                        }
                    }
                }

                _activeMenuItem = menuItem;
                menuItem.Action?.Invoke();
            };

            return btn;
        }

        private void LoadHomeDashboard()
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();
            _mainContentPanel.Padding = new Padding(25);
            _mainContentPanel.AutoScroll = true;

            // ========== 0. HEADER WITH REFRESH BUTTON ==========
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };

            Label lblDashboardTitle = new Label
            {
                Text = "📊 TỔNG QUAN HỆ THỐNG",
                Font = new Font("Montserrat", 16, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Location = new Point(0, 10)
            };

            Label lblLastUpdate = new Label
            {
                Text = $"Cập nhật lúc: {DateTime.Now:HH:mm:ss dd/MM/yyyy}",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(350, 15),
                Name = "lblLastUpdate"
            };

            Button btnRefresh = new Button
            {
                Text = "🔄 LÀM MỚI",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 35),
                Location = new Point(headerPanel.Width - 140, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) =>
            {
                btnRefresh.Enabled = false;
                btnRefresh.Text = "⏳ Đang tải...";
                Application.DoEvents();

                LoadHomeDashboard();

                // Update last update time
                foreach (Control c in _mainContentPanel.Controls)
                {
                    if (c is Panel p)
                    {
                        foreach (Control cc in p.Controls)
                        {
                            if (cc.Name == "lblLastUpdate" && cc is Label lbl)
                            {
                                lbl.Text = $"Cập nhật lúc: {DateTime.Now:HH:mm:ss dd/MM/yyyy}";
                            }
                        }
                    }
                }
            };

            headerPanel.Controls.AddRange(new Control[] { lblDashboardTitle, lblLastUpdate, btnRefresh });
            _mainContentPanel.Controls.Add(headerPanel);

            // ========== 1. KPI SECTION (FULL WIDTH GRID) ==========
            Panel kpiContainer = new Panel { Dock = DockStyle.Top, Height = 170, Padding = new Padding(0, 0, 0, 30) };
            
            TableLayoutPanel kpiGrid = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++) kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            string[] stats = GetDashboardStats();
            Color[] colors = { _cgvRed, Color.FromArgb(41, 128, 185), Color.FromArgb(142, 68, 173), Color.FromArgb(39, 174, 96) };

            for (int i = 0; i < stats.Length; i++)
            {
                Panel card = CreateModernStatCard(stats[i], colors[i]);
                kpiGrid.Controls.Add(card, i, 0);
            }
            kpiContainer.Controls.Add(kpiGrid);
            _mainContentPanel.Controls.Add(kpiContainer);

            // ========== 2. MIDDLE ANALYSIS SECTION (60/40 SPLIT) ==========
            Panel analysisContainer = new Panel { Dock = DockStyle.Top, Height = 350, Padding = new Padding(0, 0, 0, 30) };

            TableLayoutPanel midGrid = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            midGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            midGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));

            // Left Panel: Notifications
            Panel noticeCard = CreateRoundedCard(15);
            noticeCard.Dock = DockStyle.Fill;
            noticeCard.Margin = new Padding(0, 0, 15, 0);
            noticeCard.Padding = new Padding(30);

            Label lblNoticeTitle = new Label { 
                Text = "🔔 THÔNG BÁO HỆ THỐNG", 
                Font = new Font("Montserrat", 14, FontStyle.Bold), 
                Dock = DockStyle.Top, 
                Height = 45 
            };
            noticeCard.Controls.Add(lblNoticeTitle);
            
            string[] notices = { 
                "• Phim 'Avatar: The Way of Water' đạt doanh thu kỷ lục tại hệ thống.",
                "• Nhắc nhở: Kiểm kho bắp nước vào cuối ngày hôm nay.",
                "• Cập nhật: Chính sách giá vé mới áp dụng từ đầu tháng tới.",
                "• Thông báo: Bảo trì hệ thống server quản lý lúc 02:00 AM.",
                "• Cảnh báo: Nhiệt độ phòng chiếu số 4 đang cao hơn mức bình thường."
            };
            int yPos = 60;
            foreach(var n in notices) {
                Label lb = new Label { Text = n, Font = new Font("Segoe UI", 10), Location = new Point(30, yPos), AutoSize = true, ForeColor = _cgvTextColor };
                noticeCard.Controls.Add(lb);
                yPos += 35;
            }

            // Right Panel: System Health / Quick Stats
            Panel statusCard = CreateRoundedCard(15);
            statusCard.Dock = DockStyle.Fill;
            statusCard.Margin = new Padding(15, 0, 0, 0);
            statusCard.Padding = new Padding(30);

            Label lblStatusTitle = new Label { 
                Text = "⚡ TRẠNG THÁI", 
                Font = new Font("Montserrat", 14, FontStyle.Bold), 
                Dock = DockStyle.Top, 
                Height = 45 
            };
            statusCard.Controls.Add(lblStatusTitle);

            string[] statusInfo = { 
                "● Cơ sở dữ liệu: Hoạt động tốt", 
                "● Máy chủ Cloud: Trực tuyến", 
                "● Cổng thanh toán: Sẵn sàng",
                "● Hệ thống báo cháy: OK",
                "● Camera giám sát: 12/12 Hoạt động"
            };
            yPos = 60;
            foreach(var s in statusInfo) {
                Label lb = new Label { Text = s, Font = new Font("Segoe UI", 10), Location = new Point(30, yPos), AutoSize = true, ForeColor = Color.DarkSlateBlue };
                statusCard.Controls.Add(lb);
                yPos += 35;
            }

            midGrid.Controls.Add(noticeCard, 0, 0);
            midGrid.Controls.Add(statusCard, 1, 0);
            analysisContainer.Controls.Add(midGrid);
            _mainContentPanel.Controls.Add(analysisContainer);

            // Bring sections to front to maintain Order because of Dock.Top
            kpiContainer.BringToFront();
            analysisContainer.BringToFront();

            _mainContentPanel.ResumeLayout();
        }

        private Panel CreateModernStatCard(string title, Color accentColor)
        {
            Panel card = CreateRoundedCard(15);
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(0);
            card.BackColor = Color.White;
            card.Margin = new Padding(0, 0, 20, 0);

            // Left accent bar with rounded left corners only
            Panel accentBar = new Panel {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = Color.Transparent
            };
            
            accentBar.Paint += (s, e) => {
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 15;
                    // Only round top-left and bottom-left corners
                    path.AddArc(0, 0, radius, radius, 180, 90); // Top-left
                    path.AddLine(radius, 0, accentBar.Width, 0); // Top edge
                    path.AddLine(accentBar.Width, 0, accentBar.Width, accentBar.Height); // Right edge
                    path.AddLine(accentBar.Width, accentBar.Height, radius, accentBar.Height); // Bottom edge
                    path.AddArc(0, accentBar.Height - radius, radius, radius, 90, 90); // Bottom-left
                    path.CloseFigure();
                    
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush brush = new SolidBrush(accentColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }
            };

            // Content container
            Panel contentPanel = new Panel {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.Transparent
            };

            Label lblTitle = new Label {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(5, 5)
            };

            Label lblValue = new Label {
                Text = GetStatValue(title),
                Font = new Font("Montserrat", 28, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Location = new Point(5, 35)
            };

            contentPanel.Controls.AddRange(new Control[] { lblTitle, lblValue });
            card.Controls.Add(contentPanel);
            card.Controls.Add(accentBar);
            
            return card;
        }

        private Panel CreateRoundedCard(int radius)
        {
            Panel panel = new Panel();
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.None;

            // Custom paint for rounded corners với shadow effect tối giản
            panel.Paint += (s, e) =>
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    int width = panel.Width;
                    int height = panel.Height;

                    path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                    path.AddArc(width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                    path.AddArc(width - radius * 2, height - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(0, height - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseFigure();

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Draw shadow nhẹ hơn
                    Rectangle shadowRect = new Rectangle(1, 1, width - 2, height - 2);
                    using (GraphicsPath shadowPath = new GraphicsPath())
                    {
                        shadowPath.AddArc(shadowRect.X, shadowRect.Y, radius * 2, radius * 2, 180, 90);
                        shadowPath.AddArc(shadowRect.Right - radius * 2, shadowRect.Y, radius * 2, radius * 2, 270, 90);
                        shadowPath.AddArc(shadowRect.Right - radius * 2, shadowRect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        shadowPath.AddArc(shadowRect.X, shadowRect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        shadowPath.CloseFigure();

                        using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(8, 0, 0, 0)))
                        {
                            e.Graphics.FillPath(shadowBrush, shadowPath);
                        }
                    }

                    // Draw card
                    e.Graphics.FillPath(new SolidBrush(panel.BackColor), path);

                    // Add border mảnh
                    using (Pen borderPen = new Pen(Color.FromArgb(240, 240, 240), 1))
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            };

            return panel;
        }

        private string[] GetDashboardStats()
        {
            if (_userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên")
            {
                return new string[]
                {
                    "DOANH THU HÔM NAY",
                    "TỔNG NGƯỜI DÙNG",
                    "PHIM ĐANG CHIẾU",
                    "VÉ ĐÃ BÁN"
                };
            }
            else if (_userRole == "Quản lý" || _userRole == "Branch Manager" || _userRole == "Quản lý chi nhánh")
            {
                return new string[]
                {
                    "DOANH THU CHI NHÁNH",
                    "NHÂN VIÊN ĐANG LÀM",
                    "SUẤT CHIẾU HÔM NAY",
                    "SẢN PHẨM TỒN KHO"
                };
            }
            else
            {
                return new string[]
                {
                    "VÉ BÁN HÔM NAY",
                    "DOANH THU CÁ NHÂN",
                    "KHÁCH HÀNG PHỤC VỤ",
                    "CA LÀM VIỆC"
                };
            }
        }

        private string[][] GetQuickActionsByRole()
        {
            if (_userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên")
            {
                return PermissionManager.GetQuickActionsByRole(_userRole);
            }
            else if (_userRole == "Quản lý" || _userRole == "Branch Manager" || _userRole == "Quản lý chi nhánh")
            {
                return new string[][]
                {
                    new string[] { "BÁN VÉ NHANH", "🎟️" },
                    new string[] { "PHÂN CÔNG CA", "📅" },
                    new string[] { "KIỂM KHO", "📦" },
                    new string[] { "XEM BÁO CÁO", "📊" },
                    new string[] { "QUẢN LÝ NHÂN SỰ", "👥" },
                    new string[] { "DUYỆT ĐƠN NGHỈ", "✅" }
                };
            }
            else
            {
                return new string[][]
                {
                    new string[] { "BÁN VÉ", "🎫" },
                    new string[] { "ĐẶT GHẾ", "💺" },
                    new string[] { "BÁN SẢN PHẨM", "🍿" },
                    new string[] { "XEM LỊCH LÀM", "📅" },
                    new string[] { "BÁO CÁO SỰ CỐ", "⚠️" },
                    new string[] { "XIN NGHỈ PHÉP", "📋" }
                };
            }
        }

        private string GetStatIcon(string stat)
        {
            return stat switch
            {
                string s when s.Contains("DOANH THU") => "💰",
                string s when s.Contains("NGƯỜI DÙNG") => "👥",
                string s when s.Contains("PHIM") => "🎬",
                string s when s.Contains("VÉ") => "🎟️",
                string s when s.Contains("NHÂN VIÊN") => "👨‍💼",
                string s when s.Contains("SUẤT CHIẾU") => "🎥",
                string s when s.Contains("SẢN PHẨM") => "🍿",
                string s when s.Contains("KHÁCH HÀNG") => "👤",
                string s when s.Contains("CA LÀM VIỆC") => "⏰",
                _ => "📊"
            };
        }

        // Cache dashboard stats để tránh query nhiều lần
        private DashboardStats _cachedStats = null;
        private DateTime _statsLastUpdated = DateTime.MinValue;
        private readonly TimeSpan _statsCacheDuration = TimeSpan.FromMinutes(5);

        private DashboardStats GetCachedDashboardStats()
        {
            if (_cachedStats == null || DateTime.Now - _statsLastUpdated > _statsCacheDuration)
            {
                try
                {
                    var adminBLL = new AdminBLL();
                    _cachedStats = adminBLL.GetDashboardStats();
                    _statsLastUpdated = DateTime.Now;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading dashboard stats: {ex.Message}");
                    _cachedStats = new DashboardStats(); // Return empty stats
                }
            }
            return _cachedStats;
        }

        private string GetStatValue(string stat)
        {
            var stats = GetCachedDashboardStats();
            
            return stat switch
            {
                string s when s.Contains("DOANH THU HÔM NAY") => stats.FormatRevenue(stats.TodayRevenue),
                string s when s.Contains("DOANH THU CHI NHÁNH") => stats.FormatRevenue(stats.MonthRevenue / Math.Max(1, stats.TotalBranches)),
                string s when s.Contains("DOANH THU CÁ NHÂN") => stats.FormatRevenue(stats.TodayRevenue / 10),
                string s when s.Contains("TỔNG NGƯỜI DÙNG") => stats.TotalUsers.ToString(),
                string s when s.Contains("PHIM ĐANG CHIẾU") => stats.ActiveMovies.ToString(),
                string s when s.Contains("VÉ ĐÃ BÁN") => stats.TodayTicketsSold.ToString(),
                string s when s.Contains("NHÂN VIÊN ĐANG LÀM") => Math.Max(1, stats.TotalUsers / 3).ToString(),
                string s when s.Contains("SUẤT CHIẾU HÔM NAY") => stats.TodayShowtimes.ToString(),
                string s when s.Contains("SẢN PHẨM TỒN KHO") => "N/A",
                string s when s.Contains("KHÁCH HÀNG PHỤC VỤ") => stats.TotalCustomers.ToString(),
                string s when s.Contains("CA LÀM VIỆC") => "3",
                string s when s.Contains("TỔNG CHI NHÁNH") => stats.TotalBranches.ToString(),
                string s when s.Contains("PHÒNG CHIẾU") => stats.TotalRooms.ToString(),
                _ => "0"
            };
        }

        private string GetStatDescription(string stat)
        {
            return stat;
        }

        private void ExecuteQuickAction(string action)
        {  // Lấy feature từ action
            var quickActions = GetQuickActionsByRole();
            var actionInfo = quickActions.FirstOrDefault(a => a[0] == action);

            if (actionInfo != null && actionInfo.Length > 2)
            {
                string feature = actionInfo[2];

                // Kiểm tra quyền
                bool isAdmin = _userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên";
                bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager" || _userRole == "Quản lý chi nhánh";
                bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff" || _userRole == "Nhân viên bán vé" || _userRole == "Nhân viên lịch chiếu";
                if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage(feature, _userRole, this))
                    return;
            }

            MessageBox.Show($"🎬 CGV CINEMA SYSTEM\n\nThực hiện thành công: {action}",
                "CGV - Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Các phương thức chức năng
        private void ShowSettingsMenu(Control sender)
        {
            ContextMenuStrip settingsMenu = new ContextMenuStrip();
            
            // Cài đặt hệ thống (Admin Only)
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên";
            if (isAdmin)
            {
                ToolStripMenuItem itemSystemSettings = new ToolStripMenuItem("⚙️ Cài đặt hệ thống");
                itemSystemSettings.Click += (s, e) => LoadUserControl(new UC_Settings(_userRole, _username, _maNguoiDung));
                settingsMenu.Items.Add(itemSystemSettings);
                settingsMenu.Items.Add(new ToolStripSeparator());
            }
            
            // Thay đổi mật khẩu
            ToolStripMenuItem itemChangePassword = new ToolStripMenuItem("🔐 Thay đổi mật khẩu");
            itemChangePassword.Click += (s, e) => ChangePassword();
            
            // Xem hồ sơ
            ToolStripMenuItem itemProfile = new ToolStripMenuItem("👤 Hồ sơ cá nhân");
            itemProfile.Click += (s, e) => ShowUserProfile();
            
            // Thay đổi theme
            ToolStripMenuItem itemTheme = new ToolStripMenuItem("🎨 Chủ đề");
            itemTheme.Click += (s, e) => ChangeTheme();
            
            // Separator
            settingsMenu.Items.Add(new ToolStripSeparator());
            
            // Đăng xuất
            ToolStripMenuItem itemLogout = new ToolStripMenuItem("🚪 Đăng xuất");
            itemLogout.Click += (s, e) => Logout();
            
            settingsMenu.Items.Add(itemChangePassword);
            settingsMenu.Items.Add(itemProfile);
            settingsMenu.Items.Add(itemTheme);
            settingsMenu.Items.Add(itemLogout);
            
            settingsMenu.Show(sender, new Point(0, sender.Height));
        }

        private void ShowUserProfile()
        {
            MessageBox.Show($"👤 HỒ SƠ CÁ NHÂN\n\n" +
                $"Tên: {_fullName}\n" +
                $"Vai trò: {_userRole}\n" +
                $"Chi nhánh: {_branch}\n" +
                $"Tên đăng nhập: {_username}",
                "Thông tin người dùng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ChangePassword()
        {
            Form changePasswordForm = new Form();
            changePasswordForm.Text = "🔐 Thay đổi mật khẩu";
            changePasswordForm.Size = new Size(400, 250);
            changePasswordForm.StartPosition = FormStartPosition.CenterParent;
            changePasswordForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            changePasswordForm.MaximizeBox = false;
            changePasswordForm.MinimizeBox = false;
            changePasswordForm.BackColor = Color.White;

            // Old password
            Label lblOldPass = new Label() { Text = "Mật khẩu cũ:", Location = new Point(20, 20), Size = new Size(100, 25) };
            TextBox txtOldPass = new TextBox() { Location = new Point(120, 20), Size = new Size(250, 25), UseSystemPasswordChar = true };

            // New password
            Label lblNewPass = new Label() { Text = "Mật khẩu mới:", Location = new Point(20, 60), Size = new Size(100, 25) };
            TextBox txtNewPass = new TextBox() { Location = new Point(120, 60), Size = new Size(250, 25), UseSystemPasswordChar = true };

            // Confirm password
            Label lblConfirmPass = new Label() { Text = "Xác nhận mật khẩu:", Location = new Point(20, 100), Size = new Size(100, 25) };
            TextBox txtConfirmPass = new TextBox() { Location = new Point(120, 100), Size = new Size(250, 25), UseSystemPasswordChar = true };

            // OK button
            Button btnOK = new Button() 
            { 
                Text = "CẬP NHẬT", 
                Location = new Point(120, 150), 
                Size = new Size(100, 35),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtOldPass.Text) || string.IsNullOrEmpty(txtNewPass.Text))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (txtNewPass.Text != txtConfirmPass.Text)
                {
                    MessageBox.Show("Mật khẩu mới không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show("Mật khẩu đã được cập nhật thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                changePasswordForm.Close();
            };

            // Cancel button
            Button btnCancel = new Button() 
            { 
                Text = "HUỶ", 
                Location = new Point(230, 150), 
                Size = new Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            changePasswordForm.Controls.AddRange(new Control[] 
            { 
                lblOldPass, txtOldPass, 
                lblNewPass, txtNewPass, 
                lblConfirmPass, txtConfirmPass,
                btnOK, btnCancel
            });
            
            changePasswordForm.ShowDialog(this);
        }

        private void ChangeTheme()
        {
            MessageBox.Show("Tính năng thay đổi chủ đề sẽ có sớm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Logout()
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Đóng frmMain
                this.Close();
                
                // Quay lại frmLogin
                frmLogin loginForm = new frmLogin();
                loginForm.Show();
            }
        }

        private void LoadAdminUsers()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên";
            if (!isAdmin && !PermissionManager.CheckPermissionWithMessage("UserManagement", _userRole, this))
                return;

            LoadUserControl(new UC_Admin());
        }
        private void LoadAdminBranches()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên";
            if (!isAdmin && !PermissionManager.CheckPermissionWithMessage("BranchManagement", _userRole, this))
                return;

            ShowPlaceholder("QUẢN LÝ CHI NHÁNH",
                "Tính năng đang được phát triển. Bạn có quyền truy cập chức năng này.");
        }
        private void LoadMovies()
        {
            string feature = (_userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Nhân viên bán vé" || _userRole == "Nhân viên lịch chiếu") ? "MovieView" : "MovieEdit";
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager" || _userRole == "Quản lý chi nhánh";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff" || _userRole == "Nhân viên bán vé" || _userRole == "Nhân viên lịch chiếu";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage(feature, _userRole, this))
                return;

            LoadUserControl(new UC_Movies());
        }
        private void LoadShowtimes()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage(
                _userRole == "Nhân viên" ? "ShowtimeView" : "ShowtimeEdit",
                _userRole, this))
                return;

            // Sử dụng UC_ShowtimeManagement mới
            LoadUserControl(new UC_ShowtimeManagement());
        }
        private void LoadTicketSales()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage("TicketSales", _userRole, this))
                return;

            ShowPlaceholder("BÁN VÉ VÀ ĐẶT GHẾ",
                "Tính năng bán vé, chọn ghế cho khách hàng");
        }
        private void LoadInventory()
        {
            string feature = (_userRole == "Nhân viên" || _userRole == "Staff") ? "InventoryView" : "InventoryManagement";
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage(feature, _userRole, this))
                return;

            if (_userRole == "Nhân viên" || _userRole == "Staff")
            {
                ShowPlaceholder("KHO HÀNG", "Bạn chỉ có quyền xem tồn kho.");
            }
            else
            {
                LoadUserControl(new UC_Kho(_maChiNhanh, _maNguoiDung));
            }
        }
        private void SetupReportTabControl() { }
        private void LoadReportTab(int tabIndex) { }
        private void LoadReports()
        {
            string feature = _userRole == "Nhân viên" ? "PersonalReports" : "BranchReports";
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage(feature, _userRole, this))
                return;

            if (_userRole == "Quản lý" || _userRole == "Branch Manager" || isAdmin)
            {
                LoadUserControl(new UC_BaoCaoChiNhanh(_maChiNhanh, _branch));
            }
            else // Nhân viên
            {
                ShowPlaceholder("BÁO CÁO CÁ NHÂN", "Báo cáo cá nhân - Doanh thu, số vé bán.");
            }
        }
        private void LoadWorkSchedule()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage("WorkSchedule", _userRole, this))
                return;

            if (_userRole == "Quản lý" || _userRole == "Branch Manager" || isAdmin)
            {
                LoadUserControl(new UC_LichLamViec(_maChiNhanh));
            }
            else // Nhân viên
            {
                ShowPlaceholder("LỊCH LÀM VIỆC", "Xem lịch làm việc cá nhân.");
            }
        }
        private void LoadPerformance()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage("PerformanceReview", _userRole, this))
                return;

            LoadUserControl(new UC_HieuSuat(_maChiNhanh));
        }
        private void LoadStaffManagement()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            bool isManager = _userRole == "Quản lý" || _userRole == "Branch Manager";
            bool isStaff = _userRole == "Nhân viên" || _userRole == "Staff" || _userRole == "Ticket Staff" || _userRole == "Showtime Staff";
            if (!isAdmin && !isManager && !isStaff && !PermissionManager.CheckPermissionWithMessage("StaffManagement", _userRole, this))
                return;

            LoadUserControl(new UC_NhanSu(_maChiNhanh));
        }
        private void LoadAdminPanel()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            if (!isAdmin && !PermissionManager.CheckPermissionWithMessage("AdminPanel", _userRole, this))
                return;

            ShowPlaceholder("QUẢN LÝ ADMIN",
                "Cấu hình hệ thống, sao lưu dữ liệu, xem log");
        }
        private void LoadSettings()
        {
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator";
            if (!isAdmin && !PermissionManager.CheckPermissionWithMessage("ChangePassword", _userRole, this))
                return;

            ShowPlaceholder("CÀI ĐẶT HỆ THỐNG",
                "Thay đổi mật khẩu, cập nhật thông tin cá nhân");
        }

        // ========== NEW UC LOAD METHODS ==========
        private void LoadBranchManagement()
        {
            LoadUserControl(new UC_BranchManagement());
        }

        private void LoadRoomManagement()
        {
            LoadUserControl(new UC_RoomManagement());
        }

        private void LoadTicketManagement()
        {
            LoadUserControl(new UC_TicketManagement());
        }

        private void LoadSystemRevenue()
        {
            LoadUserControl(new UC_SystemRevenue());
        }

        private void LoadPricingPromotion()
        {
            LoadUserControl(new UC_PricingPromotion());
        }

        private void LoadTrendAnalysis()
        {
            LoadUserControl(new UC_TrendAnalysis());
        }

        private void LoadBranchPerformance()
        {
            LoadUserControl(new UC_BranchPerformance());
        }

        private void ShowPlaceholder(string title, string description)
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            Panel placeholderPanel = new Panel();
            placeholderPanel.Dock = DockStyle.Fill;
            placeholderPanel.BackColor = Color.White;
            placeholderPanel.Padding = new Padding(50);

            // Title
            Label lblTitle = new Label();
            lblTitle.Text = $"🎬 {title}";
            lblTitle.Font = new Font("Montserrat", 24, FontStyle.Bold);
            lblTitle.ForeColor = _cgvBlack;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 60;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Divider
            Panel divider = new Panel();
            divider.Dock = DockStyle.Top;
            divider.Height = 2;
            divider.BackColor = _cgvRed;
            divider.Margin = new Padding(0, 0, 0, 30);

            // Description
            Label lblDesc = new Label();
            lblDesc.Text = description;
            lblDesc.Font = new Font("Segoe UI", 14);
            lblDesc.ForeColor = _cgvTextColor;
            lblDesc.Dock = DockStyle.Top;
            lblDesc.Height = 80;
            lblDesc.TextAlign = ContentAlignment.MiddleLeft;
            lblDesc.Padding = new Padding(0, 20, 0, 0);

            // Role info
            Label lblRoleInfo = new Label();
            lblRoleInfo.Text = $"Vai trò: {_userRole} | Chi nhánh: {_branch}";
            lblRoleInfo.Font = new Font("Segoe UI", 11, FontStyle.Italic);
            lblRoleInfo.ForeColor = Color.Gray;
            lblRoleInfo.Dock = DockStyle.Top;
            lblRoleInfo.Height = 40;
            lblRoleInfo.TextAlign = ContentAlignment.MiddleLeft;

            // Development notice
            Panel noticePanel = CreateRoundedCard(10);
            noticePanel.Dock = DockStyle.Bottom;
            noticePanel.Height = 100;
            noticePanel.BackColor = Color.FromArgb(255, 248, 225);
            noticePanel.Padding = new Padding(20);

            Label lblNotice = new Label();
            lblNotice.Text = "⚠️ CHỨC NĂNG ĐANG ĐƯỢC PHÁT TRIỂN\nPhiên bản hoàn chỉnh sẽ có sớm!";
            lblNotice.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblNotice.ForeColor = Color.FromArgb(193, 154, 0);
            lblNotice.Dock = DockStyle.Fill;
            lblNotice.TextAlign = ContentAlignment.MiddleCenter;

            noticePanel.Controls.Add(lblNotice);

            placeholderPanel.Controls.Add(noticePanel);
            placeholderPanel.Controls.Add(lblRoleInfo);
            placeholderPanel.Controls.Add(lblDesc);
            placeholderPanel.Controls.Add(divider);
            placeholderPanel.Controls.Add(lblTitle);

            _mainContentPanel.Controls.Add(placeholderPanel);
            _mainContentPanel.ResumeLayout();
        }

        private void LoadUserControl(UserControl control)
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();
            control.Dock = DockStyle.Fill;
            _mainContentPanel.Controls.Add(control);
            _mainContentPanel.ResumeLayout();
        }
    }


    // Helper classes
    public class SidebarMenuItem
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public Action Action { get; set; }
        public bool IsActive { get; set; }
        public string Feature { get; set; }
        
        // New properties for grouped menu
        public bool IsGroupHeader { get; set; } = false;
        public string ParentGroup { get; set; } = null;
        public int IndentLevel { get; set; } = 0;
        public bool IsExpanded { get; set; } = true;
    }

    public class MenuColorTable : ProfessionalColorTable
    {
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvGold = Color.FromArgb(255, 215, 0);

        public override Color MenuItemSelected => Color.FromArgb(245, 245, 245);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(245, 245, 245);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(245, 245, 245);
        public override Color MenuItemBorder => _cgvRed;
        public override Color MenuBorder => Color.FromArgb(230, 230, 230);
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
        public override Color MenuItemPressedGradientBegin => _cgvGold;
        public override Color MenuItemPressedGradientEnd => _cgvGold;
    }
}

// Extension method for rounded corners
public static class ControlExtensions
{
    public static void BorderRadius(this Control control, int radius)
    {
        control.Paint += (s, e) =>
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(control.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(control.Width - radius * 2, control.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, control.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();

                control.Region = new Region(path);
            }
        };
    }
}
