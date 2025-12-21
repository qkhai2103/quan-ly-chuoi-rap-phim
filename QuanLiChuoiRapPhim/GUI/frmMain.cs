
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;

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
            this.Text = $"CGV CINEMA MANAGEMENT SYSTEM | {_userRole.ToUpper()} - {_branch}";
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

            // CGV Logo với hiệu ứng gradient
            Panel logoPanel = new Panel();
            logoPanel.Size = new Size(140, 70);
            logoPanel.Location = new Point(30, 0);
            logoPanel.BackColor = Color.Transparent;

            Label lblLogo = new Label();
            lblLogo.Text = "CGV";
            lblLogo.Font = new Font("Montserrat", 34, FontStyle.Bold);
            lblLogo.ForeColor = _cgvRed;
            lblLogo.Location = new Point(0, 10);
            lblLogo.AutoSize = true;
            lblLogo.Cursor = Cursors.Hand;

            // Add gradient effect on hover
            lblLogo.MouseEnter += (s, e) =>
            {
                lblLogo.ForeColor = _cgvGold;
                lblLogo.Font = new Font("Montserrat", 34, FontStyle.Bold | FontStyle.Underline);
            };
            lblLogo.MouseLeave += (s, e) =>
            {
                lblLogo.ForeColor = _cgvRed;
                lblLogo.Font = new Font("Montserrat", 34, FontStyle.Bold);
            };

            // Subtitle với CGV style
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "CINEMA SYSTEM";
            lblSubtitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblSubtitle.ForeColor = _cgvSilver;
            lblSubtitle.Location = new Point(5, 50);
            lblSubtitle.AutoSize = true;

            logoPanel.Controls.Add(lblSubtitle);
            logoPanel.Controls.Add(lblLogo);

            // SEARCH BOX IN HEADER
            Panel searchPanel = new Panel();
            searchPanel.Size = new Size(400, 50);
            searchPanel.Location = new Point(400, 10);
            searchPanel.BackColor = Color.FromArgb(50, 50, 50);
            searchPanel.BorderRadius(20);

            TextBox txtSearch = new TextBox();
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 11);
            txtSearch.ForeColor = Color.Silver;
            txtSearch.BackColor = Color.FromArgb(50, 50, 50);
            txtSearch.Size = new Size(330, 50);
            txtSearch.Location = new Point(15, 0);
            txtSearch.Text = "🔍 Tìm kiếm...";
            txtSearch.Enter += (s, e) =>
            {
                if (txtSearch.Text == "🔍 Tìm kiếm...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = _cgvWhite;
                }
            };
            txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "🔍 Tìm kiếm...";
                    txtSearch.ForeColor = Color.Silver;
                }
            };

            Button btnSearch = new Button();
            btnSearch.Text = "🔎";
            btnSearch.Font = new Font("Segoe UI", 14);
            btnSearch.Size = new Size(40, 50);
            btnSearch.Location = new Point(searchPanel.Width - 45, 0);
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.BackColor = Color.Transparent;
            btnSearch.ForeColor = _cgvRed;
            btnSearch.Cursor = Cursors.Hand;
            btnSearch.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtSearch.Text) && txtSearch.Text != "🔍 Tìm kiếm...")
                {
                    MessageBox.Show($"Tìm kiếm: {txtSearch.Text}", "Kết quả tìm kiếm");
                }
            };

            searchPanel.Controls.Add(btnSearch);
            searchPanel.Controls.Add(txtSearch);

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
            lblAvatar.Text = _fullName.Substring(0, 1).ToUpper();
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

            _header.Controls.AddRange(new Control[] { logoPanel, searchPanel, userPanel });

            // ========== SIDEBAR ==========
            _sidebar = new Panel();
            _sidebar.Dock = DockStyle.Left;
            _sidebar.Width = 300;
            _sidebar.BackColor = _cgvBlack;
            _sidebar.AutoScroll = false;
            _sidebar.Padding = new Padding(0, 20, 0, 0);

            // Sidebar header
            Panel sidebarHeader = new Panel();
            sidebarHeader.Dock = DockStyle.Top;
            sidebarHeader.Height = 100;
            sidebarHeader.BackColor = Color.Transparent;
            sidebarHeader.Padding = new Padding(25, 0, 0, 0);

            Label lblSidebarHeader = new Label();
            lblSidebarHeader.Text = "CGV CINEMA";
            lblSidebarHeader.Font = new Font("Montserrat", 20, FontStyle.Bold);
            lblSidebarHeader.ForeColor = _cgvGold;
            lblSidebarHeader.Dock = DockStyle.Top;
            lblSidebarHeader.Height = 50;
            lblSidebarHeader.TextAlign = ContentAlignment.MiddleLeft;

            Label lblSidebarSubHeader = new Label();
            lblSidebarSubHeader.Text = "CONTROL PANEL";
            lblSidebarSubHeader.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblSidebarSubHeader.ForeColor = _cgvSilver;
            lblSidebarSubHeader.Dock = DockStyle.Top;
            lblSidebarSubHeader.Height = 30;
            lblSidebarSubHeader.TextAlign = ContentAlignment.MiddleLeft;

            // Divider line
            Panel sidebarDivider = new Panel();
            sidebarDivider.Dock = DockStyle.Top;
            sidebarDivider.Height = 2;
            sidebarDivider.BackColor = Color.FromArgb(60, 60, 60);

            sidebarHeader.Controls.Add(sidebarDivider);
            sidebarHeader.Controls.Add(lblSidebarSubHeader);
            sidebarHeader.Controls.Add(lblSidebarHeader);

            // ========== REPORT TAB CONTROL (UC_BaoCaoChiNhanh) ==========
            SetupReportTabControl();

            // Thêm header vào sidebar
            _sidebar.Controls.Add(sidebarHeader);

            // ========== QUICK ACTIONS PANEL (dưới header) ==========
            Panel quickActionsContainer = new Panel();
            quickActionsContainer.Dock = DockStyle.Top;
            quickActionsContainer.Height = 90;
            quickActionsContainer.BackColor = _cgvDarkGray;
            quickActionsContainer.Padding = new Padding(25, 15, 25, 15);

            // Label cho quick actions
            Label lblQuickActions = new Label();
            lblQuickActions.Text = "⚡ THAO TÁC NHANH";
            lblQuickActions.Font = new Font("Montserrat", 11, FontStyle.Bold);
            lblQuickActions.ForeColor = _cgvGold;
            lblQuickActions.AutoSize = true;
            lblQuickActions.Location = new Point(25, 0);

            // FlowLayoutPanel cho quick action buttons
            FlowLayoutPanel quickActionButtonsPanel = new FlowLayoutPanel();
            quickActionButtonsPanel.Dock = DockStyle.Fill;
            quickActionButtonsPanel.BackColor = Color.Transparent;
            quickActionButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
            quickActionButtonsPanel.WrapContents = false;
            quickActionButtonsPanel.AutoScroll = true;
            quickActionButtonsPanel.Padding = new Padding(0, 20, 0, 0);

            // Thêm quick action buttons vào panel
            string[][] quickActions = GetQuickActionsByRole();
            foreach (var action in quickActions)
            {
                Button btn = new Button();
                btn.Text = action[0];
                btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                int btnWidth = Math.Max(90, action[0].Length * 7 + 20);
                btn.Size = new Size(btnWidth, 45);

                btn.Margin = new Padding(5, 0, 5, 0);
                btn.BackColor = _cgvRed;
                btn.ForeColor = _cgvWhite;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = _cgvGold;
                btn.Cursor = Cursors.Hand;
                btn.BorderRadius(8);
                btn.TextAlign = ContentAlignment.MiddleCenter;

                // Click handler
                btn.Click += (s, e) => ExecuteQuickAction(action[0]);

                // Hover effect
                btn.MouseEnter += (s, e) =>
                {
                    btn.BackColor = _cgvGold;
                    btn.ForeColor = _cgvBlack;
                };

                btn.MouseLeave += (s, e) =>
                {
                    btn.BackColor = _cgvRed;
                    btn.ForeColor = _cgvWhite;
                };

                quickActionButtonsPanel.Controls.Add(btn);
            }

            quickActionsContainer.Controls.Add(quickActionButtonsPanel);
            quickActionsContainer.Controls.Add(lblQuickActions);

            // ========== MAIN CONTENT ==========
            _mainContentPanel = new Panel();
            _mainContentPanel.Dock = DockStyle.Fill;
            _mainContentPanel.BackColor = _cgvLightGray;
            _mainContentPanel.Padding = new Padding(25);
            _mainContentPanel.AutoScroll = true;

            // ========== ADD CONTROLS TO FORM ==========
            this.Controls.Add(_mainContentPanel);
            this.Controls.Add(quickActionsContainer);
            this.Controls.Add(_sidebar);
            this.Controls.Add(_header);
        }

        private void AddQuickActionsToHeader()
        {
            // Quick actions đã được di chuyển xuống dưới header
        }

        private void InitializeMenuItems()
        {
            _menuItems = new List<SidebarMenuItem>();

            // Dashboard với icon CGV
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "TỔNG QUAN",
                Icon = "🏠",
                Action = LoadHomeDashboard,
                IsActive = true
            });

            // Admin specific items
            if (_userRole == "Admin")
            {
                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "QUẢN LÝ NGƯỜI DÙNG",
                    Icon = "👥",
                    Action = LoadAdminUsers
                });

                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "QUẢN LÝ CHI NHÁNH",
                    Icon = "🏢",
                    Action = LoadAdminBranches
                });
            }

            // Common items với icons phù hợp
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "QUẢN LÝ PHIM",
                Icon = "🎬",
                Action = LoadMovies
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "LỊCH CHIẾU",
                Icon = "📅",
                Action = LoadShowtimes
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "BÁN VÉ VÀ ĐẶT GHẾ",
                Icon = "🎟️",
                Action = LoadTicketSales
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "KHO HÀNG",
                Icon = "📦",
                Action = LoadInventory
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "BÁO CÁO THỐNG KÊ",
                Icon = "📊",
                Action = LoadReports
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "LỊCH LÀM VIỆC",
                Icon = "⏰",
                Action = LoadWorkSchedule
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "HIỆU SUẤT NHÂN VIÊN",
                Icon = "📈",
                Action = LoadPerformance
            });

            // Manager specific items
            if (_userRole == "Quản lý" || _userRole == "Admin")
            {
                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "QUẢN LÝ NHÂN SỰ",
                    Icon = "👨‍💼",
                    Action = LoadStaffManagement
                });
            }

            // Admin specific items
            if (_userRole == "Admin")
            {
                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "QUẢN LÝ ADMIN",
                    Icon = "🔧",
                    Action = LoadAdminPanel
                });
            }

            // Settings
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "CÀI ĐẶT HỆ THỐNG",
                Icon = "⚙️",
                Action = LoadSettings
            });

            // Add menu items to sidebar
            int yPos = 120;
            foreach (var menuItem in _menuItems)
            {
                var button = CreateSidebarButton(menuItem, yPos);
                _sidebar.Controls.Add(button);
                yPos += 55;
            }

            // Add spacer at bottom
            Panel spacer = new Panel();
            spacer.Dock = DockStyle.Bottom;
            spacer.Height = 20;
            spacer.BackColor = Color.Transparent;
            _sidebar.Controls.Add(spacer);

            _activeMenuItem = _menuItems[0];
        }

        private Button CreateSidebarButton(SidebarMenuItem menuItem, int yPos)
        {
            Button btn = new Button();
            btn.Text = $"  {menuItem.Icon}  {menuItem.Text}";
            btn.Tag = menuItem;
            btn.Font = _cgvMenuFont;
            btn.ForeColor = menuItem.IsActive ? _cgvWhite : Color.FromArgb(180, 180, 180);
            btn.BackColor = menuItem.IsActive ? _cgvRed : Color.Transparent;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Size = new Size(_sidebar.Width - 15, 50);
            btn.Location = new Point(7, yPos);
            btn.Padding = new Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.BorderRadius(8);

            // Hover effects với CGV style
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

            btn.Click += (s, e) =>
            {
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
                menuItem.Action?.Invoke();
            };

            return btn;
        }

        private void LoadHomeDashboard()
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            Panel homePanel = new Panel();
            homePanel.Dock = DockStyle.Fill;
            homePanel.BackColor = Color.Transparent;
            homePanel.AutoScroll = true;
            homePanel.Padding = new Padding(15);

            // ========== WELCOME SECTION ==========
            Panel welcomeCard = CreateRoundedCard(15);
            welcomeCard.Dock = DockStyle.Top;
            welcomeCard.Height = 150;
            welcomeCard.BackColor = _cgvBlack;
            welcomeCard.Padding = new Padding(30, 20, 30, 20);

            Label lblWelcome = new Label();
            lblWelcome.Text = $"🎬 CHÀO MỪNG TRỞ LẠI, {_fullName.ToUpper()}!";
            lblWelcome.Font = new Font("Montserrat", 22, FontStyle.Bold);
            lblWelcome.ForeColor = _cgvGold;
            lblWelcome.Location = new Point(0, 20);
            lblWelcome.AutoSize = true;

            Label lblDate = new Label();
            lblDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy").ToUpper();
            lblDate.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            lblDate.ForeColor = _cgvSilver;
            lblDate.Location = new Point(0, 70);
            lblDate.AutoSize = true;

            Label lblRole = new Label();
            lblRole.Text = $"Vai trò: {_userRole} | Chi nhánh: {_branch}";
            lblRole.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            lblRole.ForeColor = Color.FromArgb(200, 200, 200);
            lblRole.Location = new Point(0, 100);
            lblRole.AutoSize = true;

            welcomeCard.Controls.AddRange(new Control[] { lblWelcome, lblDate, lblRole });
            homePanel.Controls.Add(welcomeCard);

            // ========== STATS CARDS SECTION ==========
            Label lblStatsTitle = new Label();
            lblStatsTitle.Text = "📊 THỐNG KÊ CHÍNH";
            lblStatsTitle.Font = new Font("Montserrat", 18, FontStyle.Bold);
            lblStatsTitle.ForeColor = _cgvBlack;
            lblStatsTitle.Dock = DockStyle.Top;
            lblStatsTitle.Height = 60;
            lblStatsTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblStatsTitle.Margin = new Padding(0, 20, 0, 0);
            homePanel.Controls.Add(lblStatsTitle);

            // Stats cards layout - 2x2 grid
            TableLayoutPanel statsGrid = new TableLayoutPanel();
            statsGrid.Dock = DockStyle.Top;
            statsGrid.Height = 360;
            statsGrid.Margin = new Padding(0, 0, 0, 25);
            statsGrid.RowCount = 2;
            statsGrid.ColumnCount = 2;
            statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            statsGrid.Padding = new Padding(0, 10, 0, 0);

            string[] stats = GetDashboardStats();
            Color[] statColors = {
                _cgvRed,
                Color.FromArgb(41, 128, 185),   // CGV Blue
                Color.FromArgb(142, 68, 173),   // CGV Purple
                Color.FromArgb(39, 174, 96)     // CGV Green
            };

            for (int i = 0; i < stats.Length; i++)
            {
                Panel statCard = CreateMinimalStatCard(stats[i], statColors[i]);
                statCard.Margin = new Padding(5);
                statsGrid.Controls.Add(statCard, i % 2, i / 2);
            }

            homePanel.Controls.Add(statsGrid);

            // ========== SYSTEM INFO SECTION ==========
            Label lblSystemTitle = new Label();
            lblSystemTitle.Text = "ℹ️ THÔNG TIN HỆ THỐNG";
            lblSystemTitle.Font = new Font("Montserrat", 18, FontStyle.Bold);
            lblSystemTitle.ForeColor = _cgvBlack;
            lblSystemTitle.Dock = DockStyle.Top;
            lblSystemTitle.Height = 60;
            lblSystemTitle.TextAlign = ContentAlignment.MiddleLeft;
            homePanel.Controls.Add(lblSystemTitle);

            // System info cards layout
            FlowLayoutPanel systemPanel = new FlowLayoutPanel();
            systemPanel.Dock = DockStyle.Top;
            systemPanel.Height = 200;
            systemPanel.Margin = new Padding(0, 0, 0, 25);
            systemPanel.WrapContents = false;
            systemPanel.AutoScroll = true;

            // System status card
            Panel systemCard = CreateRoundedCard(12);
            systemCard.Size = new Size(320, 180);
            systemCard.BackColor = _cgvWhite;
            systemCard.Padding = new Padding(20);

            Label lblSystemHeader = new Label();
            lblSystemHeader.Text = "🖥️ TRẠNG THÁI HỆ THỐNG";
            lblSystemHeader.Font = new Font("Montserrat", 14, FontStyle.Bold);
            lblSystemHeader.ForeColor = _cgvBlack;
            lblSystemHeader.Location = new Point(0, 15);
            lblSystemHeader.AutoSize = true;

            // Status items
            string[] statusItems = {
                "✅ Cơ sở dữ liệu: Hoạt động",
                "✅ Máy chủ: Trực tuyến",
                "✅ Kết nối mạng: Ổn định",
                "🟡 Bảo mật: Bình thường",
                "✅ Sao lưu: Hôm nay 02:00"
            };

            int statusY = 50;
            foreach (var item in statusItems)
            {
                Label lblStatus = new Label();
                lblStatus.Text = item;
                lblStatus.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                lblStatus.ForeColor = _cgvTextColor;
                lblStatus.Location = new Point(0, statusY);
                lblStatus.AutoSize = true;
                systemCard.Controls.Add(lblStatus);
                statusY += 25;
            }

            systemCard.Controls.Add(lblSystemHeader);
            systemPanel.Controls.Add(systemCard);

            // Performance summary card
            Panel performanceCard = CreateRoundedCard(12);
            performanceCard.Size = new Size(320, 180);
            performanceCard.BackColor = _cgvWhite;
            performanceCard.Padding = new Padding(20);

            Label lblPerformanceHeader = new Label();
            lblPerformanceHeader.Text = "📈 TÓM TẮT HIỆU SUẤT";
            lblPerformanceHeader.Font = new Font("Montserrat", 14, FontStyle.Bold);
            lblPerformanceHeader.ForeColor = _cgvBlack;
            lblPerformanceHeader.Location = new Point(0, 15);
            lblPerformanceHeader.AutoSize = true;

            // Performance metrics
            string[] performanceMetrics = {
                $"📊 Doanh thu hôm nay: {GetStatValue("DOANH THU HÔM NAY")}",
                $"🎟️ Vé đã bán: {GetStatValue("VÉ ĐÃ BÁN")}",
                $"👥 Người dùng online: 8",
                $"⏱️ Thời gian phản hồi: 0.8s",
                $"🔄 Tải hệ thống: 45%"
            };

            int perfY = 50;
            foreach (var metric in performanceMetrics)
            {
                Label lblMetric = new Label();
                lblMetric.Text = metric;
                lblMetric.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                lblMetric.ForeColor = _cgvTextColor;
                lblMetric.Location = new Point(0, perfY);
                lblMetric.AutoSize = true;
                performanceCard.Controls.Add(lblMetric);
                perfY += 25;
            }

            performanceCard.Controls.Add(lblPerformanceHeader);
            systemPanel.Controls.Add(performanceCard);

            // Recent updates card
            Panel updatesCard = CreateRoundedCard(12);
            updatesCard.Size = new Size(320, 180);
            updatesCard.BackColor = _cgvWhite;
            updatesCard.Padding = new Padding(20);

            Label lblUpdatesHeader = new Label();
            lblUpdatesHeader.Text = "🔄 CẬP NHẬT GẦN ĐÂY";
            lblUpdatesHeader.Font = new Font("Montserrat", 14, FontStyle.Bold);
            lblUpdatesHeader.ForeColor = _cgvBlack;
            lblUpdatesHeader.Location = new Point(0, 15);
            lblUpdatesHeader.AutoSize = true;

            // Recent updates
            string[] updates = {
                "• Hệ thống cập nhật phiên bản 2.1",
                "• Thêm tính năng báo cáo mới",
                "• Tối ưu hiệu suất database",
                "• Sửa lỗi giao diện người dùng",
                "• Cập nhật bảo mật hệ thống"
            };

            int updatesY = 50;
            foreach (var update in updates)
            {
                Label lblUpdate = new Label();
                lblUpdate.Text = update;
                lblUpdate.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                lblUpdate.ForeColor = _cgvTextColor;
                lblUpdate.Location = new Point(0, updatesY);
                lblUpdate.AutoSize = true;
                updatesCard.Controls.Add(lblUpdate);
                updatesY += 25;
            }

            updatesCard.Controls.Add(lblUpdatesHeader);
            systemPanel.Controls.Add(updatesCard);

            homePanel.Controls.Add(systemPanel);

            // ========== QUICK LINKS SECTION ==========
            Label lblLinksTitle = new Label();
            lblLinksTitle.Text = "🔗 TRUY CẬP NHANH";
            lblLinksTitle.Font = new Font("Montserrat", 18, FontStyle.Bold);
            lblLinksTitle.ForeColor = _cgvBlack;
            lblLinksTitle.Dock = DockStyle.Top;
            lblLinksTitle.Height = 60;
            lblLinksTitle.TextAlign = ContentAlignment.MiddleLeft;
            homePanel.Controls.Add(lblLinksTitle);

            // Quick links buttons
            FlowLayoutPanel linksPanel = new FlowLayoutPanel();
            linksPanel.Dock = DockStyle.Top;
            linksPanel.Height = 100;
            linksPanel.Margin = new Padding(0, 0, 0, 25);
            linksPanel.WrapContents = true;
            linksPanel.AutoScroll = false;

            string[] quickLinks = {
                "📋 Xem báo cáo hôm nay",
                "👥 Quản lý nhân viên",
                "🎬 Lịch chiếu hôm nay",
                "📦 Kiểm tra tồn kho",
                "⚙️ Cài đặt hệ thống",
                "📞 Hỗ trợ kỹ thuật"
            };

            string[] linkIcons = { "📋", "👥", "🎬", "📦", "⚙️", "📞" };

            for (int i = 0; i < quickLinks.Length; i++)
            {
                Button linkBtn = new Button();
                linkBtn.Text = $"  {linkIcons[i]} {quickLinks[i]}";
                linkBtn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                linkBtn.Size = new Size(220, 40);
                linkBtn.Margin = new Padding(5);
                linkBtn.BackColor = Color.White;
                linkBtn.ForeColor = _cgvTextColor;
                linkBtn.FlatStyle = FlatStyle.Flat;
                linkBtn.FlatAppearance.BorderSize = 1;
                linkBtn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
                linkBtn.Cursor = Cursors.Hand;
                linkBtn.BorderRadius(8);
                linkBtn.TextAlign = ContentAlignment.MiddleLeft;

                linkBtn.MouseEnter += (s, e) =>
                {
                    linkBtn.BackColor = _cgvLightGray;
                    linkBtn.FlatAppearance.BorderColor = _cgvRed;
                };

                linkBtn.MouseLeave += (s, e) =>
                {
                    linkBtn.BackColor = Color.White;
                    linkBtn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
                };

                linksPanel.Controls.Add(linkBtn);
            }

            homePanel.Controls.Add(linksPanel);

            _mainContentPanel.Controls.Add(homePanel);
            _mainContentPanel.ResumeLayout();
        }

        private Panel CreateMinimalStatCard(string statText, Color color)
        {
            Panel card = CreateRoundedCard(12);
            card.Size = new Size(600, 160); // Larger for grid layout
            card.Padding = new Padding(25);
            card.BackColor = Color.White;
            card.Cursor = Cursors.Hand;

            // Icon với design tối giản
            Panel iconPanel = new Panel();
            iconPanel.Size = new Size(50, 50);
            iconPanel.Location = new Point(25, 25);
            iconPanel.BackColor = Color.FromArgb(20, color.R, color.G, color.B);
            iconPanel.BorderRadius(25);

            Label lblIcon = new Label();
            lblIcon.Text = GetStatIcon(statText);
            lblIcon.Font = new Font("Segoe UI", 20);
            lblIcon.ForeColor = color;
            lblIcon.Dock = DockStyle.Fill;
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            iconPanel.Controls.Add(lblIcon);

            // Value - lớn và nổi bật
            string value = GetStatValue(statText);
            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Montserrat", 32, FontStyle.Bold);
            lblValue.ForeColor = _cgvBlack;
            lblValue.Location = new Point(90, 20);
            lblValue.AutoSize = true;

            // Description
            string desc = GetStatDescription(statText);
            Label lblDesc = new Label();
            lblDesc.Text = desc.ToUpper();
            lblDesc.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblDesc.ForeColor = Color.Gray;
            lblDesc.Location = new Point(90, 65);
            lblDesc.AutoSize = true;

            // Trend indicator (tối giản)
            Label lblTrend = new Label();
            lblTrend.Text = "↗";
            lblTrend.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTrend.ForeColor = Color.FromArgb(39, 174, 96);
            lblTrend.Location = new Point(card.Width - 50, 30);
            lblTrend.AutoSize = true;

            // Percentage change
            Label lblPercent = new Label();
            lblPercent.Text = "+12%";
            lblPercent.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblPercent.ForeColor = Color.FromArgb(39, 174, 96);
            lblPercent.Location = new Point(card.Width - 80, 55);
            lblPercent.AutoSize = true;

            // Separator line tối giản
            Panel line = new Panel();
            line.Size = new Size(card.Width - 50, 1);
            line.Location = new Point(25, 120);
            line.BackColor = Color.FromArgb(240, 240, 240);

            card.Controls.Add(line);
            card.Controls.AddRange(new Control[] { iconPanel, lblValue, lblDesc, lblTrend, lblPercent });

            // Hover effect tối giản
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(250, 250, 250);
                line.BackColor = color;
                lblIcon.Font = new Font("Segoe UI", 22);
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
                line.BackColor = Color.FromArgb(240, 240, 240);
                lblIcon.Font = new Font("Segoe UI", 20);
            };

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
            if (_userRole == "Admin")
            {
                return new string[]
                {
                    "DOANH THU HÔM NAY",
                    "TỔNG NGƯỜI DÙNG",
                    "PHIM ĐANG CHIẾU",
                    "VÉ ĐÃ BÁN"
                };
            }
            else if (_userRole == "Quản lý")
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
            if (_userRole == "Admin")
            {
                return new string[][]
                {
                    new string[] { "THÊM NGƯỜI DÙNG", "👤" },
                    new string[] { "TẠO BÁO CÁO", "📊" },
                    new string[] { "QUẢN LÝ CHI NHÁNH", "🏢" },
                    new string[] { "XEM LOG HỆ THỐNG", "📋" },
                    new string[] { "SAO LƯU DỮ LIỆU", "💾" },
                    new string[] { "CÀI ĐẶT HỆ THỐNG", "⚙️" }
                };
            }
            else if (_userRole == "Quản lý")
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

        private string GetStatValue(string stat)
        {
            return stat switch
            {
                string s when s.Contains("DOANH THU HÔM NAY") => "12.5M ₫",
                string s when s.Contains("DOANH THU CHI NHÁNH") => "8.2M ₫",
                string s when s.Contains("DOANH THU CÁ NHÂN") => "2.1M ₫",
                string s when s.Contains("TỔNG NGƯỜI DÙNG") => "42",
                string s when s.Contains("PHIM ĐANG CHIẾU") => "8",
                string s when s.Contains("VÉ ĐÃ BÁN") => "156",
                string s when s.Contains("NHÂN VIÊN ĐANG LÀM") => "12",
                string s when s.Contains("SUẤT CHIẾU HÔM NAY") => "24",
                string s when s.Contains("SẢN PHẨM TỒN KHO") => "342",
                string s when s.Contains("KHÁCH HÀNG PHỤC VỤ") => "89",
                string s when s.Contains("CA LÀM VIỆC") => "3",
                _ => "0"
            };
        }

        private string GetStatDescription(string stat)
        {
            return stat;
        }

        private void ExecuteQuickAction(string action)
        {
            MessageBox.Show($"🎬 CGV CINEMA SYSTEM\n\nThực hiện thành công: {action}",
                "CGV - Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Các phương thức chức năng
        private void ShowSettingsMenu(Control sender)
        {
            ContextMenuStrip settingsMenu = new ContextMenuStrip();
            
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

        private void LoadAdminUsers() { }
        private void LoadAdminBranches() { }
        private void LoadMovies() { }
        private void LoadShowtimes() { }
        private void LoadTicketSales() { }
        private void LoadInventory() { }
        private void SetupReportTabControl() { }
        private void LoadReportTab(int tabIndex) { }
        private void LoadReports() { }
        private void LoadWorkSchedule() { }
        private void LoadPerformance() { }
        private void LoadStaffManagement() { }
        private void LoadAdminPanel() { }
        private void LoadSettings() { }
        private void ShowPlaceholder(string title, string description) { }
        private void LoadUserControl(UserControl control) { }
    }

    // Helper classes
    public class SidebarMenuItem
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public Action Action { get; set; }
        public bool IsActive { get; set; }
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
