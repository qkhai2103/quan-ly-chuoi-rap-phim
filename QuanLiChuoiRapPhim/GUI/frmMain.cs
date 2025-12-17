
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

        // UI Components
        private Panel _sidebar;
        private Panel _mainContentPanel;
        private Panel _header;
        private Panel _notificationPanel;
        private FlowLayoutPanel _quickActionsPanel;

        // Sidebar menu items
        private List<SidebarMenuItem> _menuItems;
        private SidebarMenuItem _activeMenuItem;

        // TabControl for Reports (UC_BaoCaoChiNhanh)
        private TabControl _reportTabControl;
        private UC_BaoCaoChiNhanh _currentBaoCaoControl;

        // CGV Theme colors
        private Color _cgvRed = Color.FromArgb(220, 53, 69);       // CGV Primary Red
        private Color _cgvDarkRed = Color.FromArgb(180, 40, 55);   // Darker Red
        private Color _cgvBlack = Color.FromArgb(20, 20, 20);      // CGV Black
        private Color _cgvDarkGray = Color.FromArgb(45, 45, 45);   // Dark Gray
        private Color _cgvLightGray = Color.FromArgb(248, 249, 252); // Light Background
        private Color _cgvWhite = Color.White;
        private Color _cgvGold = Color.FromArgb(255, 193, 7);      // Accent Gold
        private Color _cgvTextColor = Color.FromArgb(51, 51, 51);  // Text Color

        // Fonts
        private Font _cgvTitleFont = new Font("Arial", 24, FontStyle.Bold);
        private Font _cgvHeaderFont = new Font("Arial", 18, FontStyle.Bold);
        private Font _cgvSubHeaderFont = new Font("Arial", 14, FontStyle.Bold);
        private Font _cgvNormalFont = new Font("Segoe UI", 11);
        private Font _cgvSmallFont = new Font("Segoe UI", 10);

        public frmMain(string username, string userRole, string branch, string fullName)
        {
            InitializeComponent();

            _username = username;
            _userRole = userRole;
            _branch = branch;
            _fullName = fullName;

            SetupModernUI();
            InitializeMenuItems();
            LoadHomeDashboard();
        }

        private void SetupModernUI()
        {
            // Cấu hình form
            this.Text = $"CGV CINEMA MANAGEMENT | {_userRole.ToUpper()} - {_branch}";
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _cgvLightGray;
            this.WindowState = FormWindowState.Maximized;

            // ========== HEADER ==========
            _header = new Panel();
            _header.Dock = DockStyle.Top;
            _header.Height = 80;
            _header.BackColor = _cgvBlack;
            _header.Padding = new Padding(25, 0, 25, 0);

            // CGV Logo với hiệu ứng
            Panel logoPanel = new Panel();
            logoPanel.Size = new Size(120, 80);
            logoPanel.Location = new Point(25, 0);
            logoPanel.BackColor = Color.Transparent;

            Label lblLogo = new Label();
            lblLogo.Text = "CGV";
            lblLogo.Font = new Font("Arial", 32, FontStyle.Bold);
            lblLogo.ForeColor = _cgvRed;
            lblLogo.Location = new Point(0, 15);
            lblLogo.AutoSize = true;
            lblLogo.MouseEnter += (s, e) => lblLogo.ForeColor = Color.White;
            lblLogo.MouseLeave += (s, e) => lblLogo.ForeColor = _cgvRed;

            // Subtitle
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "CINEMA";
            lblSubtitle.Font = new Font("Arial", 10, FontStyle.Bold);
            lblSubtitle.ForeColor = _cgvGold;
            lblSubtitle.Location = new Point(70, 45);
            lblSubtitle.AutoSize = true;

            logoPanel.Controls.Add(lblSubtitle);
            logoPanel.Controls.Add(lblLogo);

            // Branch info
            Label lblBranchInfo = new Label();
            lblBranchInfo.Text = $"{_branch.ToUpper()}";
            lblBranchInfo.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblBranchInfo.ForeColor = _cgvWhite;
            lblBranchInfo.Location = new Point(200, 25);
            lblBranchInfo.AutoSize = true;

            // Search box with CGV style
            Panel searchPanel = new Panel();
            searchPanel.Size = new Size(350, 40);
            searchPanel.Location = new Point(400, 20);
            searchPanel.BackColor = _cgvDarkGray;
            searchPanel.BorderRadius(20);

            TextBox txtSearch = new TextBox();
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 11);
            txtSearch.ForeColor = Color.White;
            txtSearch.BackColor = Color.Red;
            txtSearch.Size = new Size(300, 40);
            txtSearch.Location = new Point(20, 10);
            txtSearch.Text = "Tìm kiếm...";
            txtSearch.Enter += (s, e) => { if (txtSearch.Text == "Tìm kiếm...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.White; } };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Tìm kiếm..."; txtSearch.ForeColor = Color.Gray; } };

            Label lblSearchIcon = new Label();
            lblSearchIcon.Text = "🔍";
            lblSearchIcon.Font = new Font("Segoe UI", 14);
            lblSearchIcon.ForeColor = _cgvRed;
            lblSearchIcon.Size = new Size(30, 40);
            lblSearchIcon.Location = new Point(searchPanel.Width - 40, 10);
            lblSearchIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblSearchIcon.Cursor = Cursors.Hand;

            searchPanel.Controls.Add(lblSearchIcon);
            searchPanel.Controls.Add(txtSearch);

            // User info panel - CGV style
            Panel userPanel = new Panel();
            userPanel.Size = new Size(400, 80);
            userPanel.Location = new Point(_header.Width - 450, 0);
            userPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            userPanel.BackColor = Color.Transparent;

            // Notification bell với badge CGV style
            Panel notificationPanel = new Panel();
            notificationPanel.Size = new Size(60, 60);
            notificationPanel.Location = new Point(20, 10);
            notificationPanel.BackColor = Color.Transparent;

            Button btnNotification = new Button();
            btnNotification.Text = "🔔";
            btnNotification.Font = new Font("Segoe UI", 18);
            btnNotification.Size = new Size(50, 50);
            btnNotification.Location = new Point(5, 5);
            btnNotification.FlatStyle = FlatStyle.Flat;
            btnNotification.FlatAppearance.BorderSize = 0;
            btnNotification.BackColor = Color.Transparent;
            btnNotification.ForeColor = _cgvWhite;
            btnNotification.Cursor = Cursors.Hand;
            btnNotification.MouseEnter += (s, e) => btnNotification.ForeColor = _cgvGold;
            btnNotification.MouseLeave += (s, e) => btnNotification.ForeColor = _cgvWhite;

            // Notification badge
            Panel badgePanel = new Panel();
            badgePanel.Size = new Size(22, 22);
            badgePanel.Location = new Point(35, 0);
            badgePanel.BackColor = _cgvRed;
            badgePanel.BorderRadius(11);

            Label lblNotificationBadge = new Label();
            lblNotificationBadge.Text = "3";
            lblNotificationBadge.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblNotificationBadge.ForeColor = Color.White;
            lblNotificationBadge.Dock = DockStyle.Fill;
            lblNotificationBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            badgePanel.Controls.Add(lblNotificationBadge);
            notificationPanel.Controls.Add(badgePanel);
            notificationPanel.Controls.Add(btnNotification);

            // User avatar với CGV style
            Panel avatarPanel = new Panel();
            avatarPanel.Size = new Size(50, 50);
            avatarPanel.Location = new Point(100, 15);
            avatarPanel.BackColor = _cgvRed;
            avatarPanel.BorderRadius(25);
            avatarPanel.BackgroundImageLayout = ImageLayout.Stretch;

            Label lblAvatar = new Label();
            lblAvatar.Text = _fullName.Substring(0, 1).ToUpper();
            lblAvatar.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblAvatar.ForeColor = Color.White;
            lblAvatar.Dock = DockStyle.Fill;
            lblAvatar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            avatarPanel.Controls.Add(lblAvatar);

            // User info
            Label lblUserInfo = new Label();
            lblUserInfo.Text = $"{_fullName.ToUpper()}\n{_userRole} • {_branch}";
            lblUserInfo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblUserInfo.ForeColor = _cgvWhite;
            lblUserInfo.AutoSize = true;
            lblUserInfo.Location = new Point(160, 20);
            lblUserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Settings dropdown với CGV style
            Button btnSettings = new Button();
            btnSettings.Text = "⚙️";
            btnSettings.Font = new Font("Segoe UI", 16);
            btnSettings.Size = new Size(50, 50);
            btnSettings.Location = new Point(320, 15);
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.BackColor = Color.Transparent;
            btnSettings.ForeColor = _cgvWhite;
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.MouseEnter += (s, e) => btnSettings.ForeColor = _cgvGold;
            btnSettings.MouseLeave += (s, e) => btnSettings.ForeColor = _cgvWhite;
            btnSettings.Click += (s, e) => ShowSettingsMenu(btnSettings);

            userPanel.Controls.AddRange(new Control[] {
                notificationPanel, avatarPanel, lblUserInfo, btnSettings
            });

            _header.Controls.AddRange(new Control[] { logoPanel, lblBranchInfo, searchPanel, userPanel });

            // ========== SIDEBAR ==========
            _sidebar = new Panel();
            _sidebar.Dock = DockStyle.Left;
            _sidebar.Width = 320;
            _sidebar.BackColor = _cgvBlack;
            _sidebar.AutoScroll = true;
            _sidebar.Padding = new Padding(0, 20, 0, 0);

            // Sidebar header với CGV style
            Panel sidebarHeader = new Panel();
            sidebarHeader.Dock = DockStyle.Top;
            sidebarHeader.Height = 80;
            sidebarHeader.BackColor = Color.Transparent;
            sidebarHeader.Padding = new Padding(20, 0, 0, 0);

            Label lblSidebarHeader = new Label();
            lblSidebarHeader.Text = "CINEMA MANAGER";
            lblSidebarHeader.Font = new Font("Arial", 16, FontStyle.Bold);
            lblSidebarHeader.ForeColor = _cgvGold;
            lblSidebarHeader.Dock = DockStyle.Top;
            lblSidebarHeader.Height = 40;
            lblSidebarHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            Label lblSidebarSubHeader = new Label();
            lblSidebarSubHeader.Text = "MENU ĐIỀU KHIỂN";
            lblSidebarSubHeader.Font = new Font("Segoe UI", 10);
            lblSidebarSubHeader.ForeColor = Color.FromArgb(180, 180, 180);
            lblSidebarSubHeader.Dock = DockStyle.Top;
            lblSidebarSubHeader.Height = 30;
            lblSidebarSubHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            sidebarHeader.Controls.Add(lblSidebarSubHeader);
            sidebarHeader.Controls.Add(lblSidebarHeader);

            // ========== REPORT TAB CONTROL (UC_BaoCaoChiNhanh) ==========
            SetupReportTabControl();

            // Thêm header vào sidebar
            _sidebar.Controls.Add(sidebarHeader);

            // ========== MAIN CONTENT ==========
            _mainContentPanel = new Panel();
            _mainContentPanel.Dock = DockStyle.Fill;
            _mainContentPanel.BackColor = _cgvLightGray;
            _mainContentPanel.Padding = new Padding(20);

            // ========== QUICK ACTIONS BAR ==========
            _quickActionsPanel = new FlowLayoutPanel();
            _quickActionsPanel.Dock = DockStyle.Bottom;
            _quickActionsPanel.Height = 90;
            _quickActionsPanel.BackColor = _cgvBlack;
            _quickActionsPanel.Padding = new Padding(30, 15, 30, 15);
            _quickActionsPanel.FlowDirection = FlowDirection.LeftToRight;

            // Thêm tiêu đề cho quick actions
            Label lblQuickActionsTitle = new Label();
            lblQuickActionsTitle.Text = "THAO TÁC NHANH:";
            lblQuickActionsTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblQuickActionsTitle.ForeColor = _cgvGold;
            lblQuickActionsTitle.AutoSize = true;
            lblQuickActionsTitle.Margin = new Padding(0, 20, 20, 0);
            _quickActionsPanel.Controls.Add(lblQuickActionsTitle);

            // Add quick action buttons
            AddQuickActions();

            // ========== ADD CONTROLS TO FORM ==========
            this.Controls.Add(_mainContentPanel);
            this.Controls.Add(_quickActionsPanel);
            this.Controls.Add(_sidebar);
            this.Controls.Add(_header);
        }

        private void InitializeMenuItems()
        {
            _menuItems = new List<SidebarMenuItem>();

            // Dashboard với icon CGV
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "🎬 TỔNG QUAN",
                Icon = "",
                Action = LoadHomeDashboard,
                IsActive = true
            });

            // Admin specific items
            if (_userRole == "Admin")
            {
                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "👥 QUẢN LÝ NGƯỜI DÙNG",
                    Icon = "👥",
                    Action = LoadAdminUsers
                });

                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "🏢 QUẢN LÝ CHI NHÁNH",
                    Icon = "🏢",
                    Action = LoadAdminBranches
                });
            }

            // Common items với icons phù hợp
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "📽️ QUẢN LÝ PHIM",
                Icon = "📽️",
                Action = LoadMovies
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "🎟️ LỊCH CHIẾU",
                Icon = "🎟️",
                Action = LoadShowtimes
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "💺 BÁN VÉ VÀ ĐẶT GHẾ",
                Icon = "💺",
                Action = LoadTicketSales
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "📦 KHO HÀNG",
                Icon = "📦",
                Action = LoadInventory
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "📊 BÁO CÁO THỐNG KÊ",
                Icon = "📊",
                Action = LoadReports
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "📅 LỊCH LÀM VIỆC",
                Icon = "📅",
                Action = LoadWorkSchedule
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "📈 HIỆU SUẤT NHÂN VIÊN",
                Icon = "📈",
                Action = LoadPerformance
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "📋 ĐƠN XIN NGHỈ",
                Icon = "📋",
                Action = LoadLeaveRequests
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "✅ DUYỆT ĐƠN XIN NGHỈ",
                Icon = "✅",
                Action = LoadApproveLeave
            });

            // Manager specific items
            if (_userRole == "Quản lý" || _userRole == "Admin")
            {
                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "👨‍💼 QUẢN LÝ NHÂN SỰ",
                    Icon = "👨‍💼",
                    Action = LoadStaffManagement
                });
            }

            // Admin specific items
            if (_userRole == "Admin")
            {
                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "🔧 QUẢN LÝ ADMIN",
                    Icon = "🔧",
                    Action = LoadAdminPanel
                });
            }

            // Settings
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "⚙️ CÀI ĐẶT HỆ THỐNG",
                Icon = "⚙️",
                Action = LoadSettings
            });

            // Add menu items to sidebar
            int yPos = 100;
            foreach (var menuItem in _menuItems)
            {
                var button = CreateSidebarButton(menuItem, yPos);
                _sidebar.Controls.Add(button);
                yPos += 60;
            }

            _activeMenuItem = _menuItems[0];
        }

        private Button CreateSidebarButton(SidebarMenuItem menuItem, int yPos)
        {
            Button btn = new Button();
            btn.Text = $"  {menuItem.Icon}  {menuItem.Text}";
            btn.Tag = menuItem;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            btn.ForeColor = menuItem.IsActive ? _cgvWhite : Color.FromArgb(180, 180, 180);
            btn.BackColor = menuItem.IsActive ? _cgvRed : Color.Transparent;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btn.Size = new Size(_sidebar.Width - 10, 55);
            btn.Location = new Point(5, yPos);
            btn.Padding = new Padding(25, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.BorderRadius(5);

            // Hover effects với CGV style
            btn.MouseEnter += (s, e) =>
            {
                if (!menuItem.IsActive)
                {
                    btn.BackColor = Color.FromArgb(50, 50, 50);
                    btn.ForeColor = _cgvWhite;
                }
            };

            btn.MouseLeave += (s, e) =>
            {
                if (!menuItem.IsActive)
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Color.FromArgb(180, 180, 180);
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

            // Thêm hiệu ứng glow khi active
            if (menuItem.IsActive)
            {
                btn.Paint += (s, e) =>
                {
                    using (Pen glowPen = new Pen(Color.FromArgb(100, _cgvRed), 3))
                    {
                        Rectangle rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                        e.Graphics.DrawRectangle(glowPen, rect);
                    }
                };
            }

            return btn;
        }

        private void AddQuickActions()
        {
            string[][] quickActions = GetQuickActionsByRole();

            foreach (var action in quickActions)
            {
                Button btn = new Button();
                btn.Text = action[0];
                btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btn.Size = new Size(200, 60);
                btn.Margin = new Padding(8);
                btn.BackColor = _cgvDarkGray;
                btn.ForeColor = _cgvWhite;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = _cgvRed;
                btn.Cursor = Cursors.Hand;
                btn.BorderRadius(10);

                // Icon
                Label lblIcon = new Label();
                lblIcon.Text = action[1];
                lblIcon.Font = new Font("Segoe UI", 16);
                lblIcon.Location = new Point(15, 18);
                lblIcon.AutoSize = true;
                lblIcon.ForeColor = _cgvRed;
                btn.Controls.Add(lblIcon);

                // Text
                Label lblText = new Label();
                lblText.Text = action[0];
                lblText.Font = new Font("Segoe UI", 9);
                lblText.Location = new Point(50, 20);
                lblText.AutoSize = true;
                lblText.ForeColor = _cgvWhite;
                btn.Controls.Add(lblText);

                // Hover effect với CGV style
                btn.MouseEnter += (s, e) =>
                {
                    btn.BackColor = _cgvRed;
                    lblIcon.ForeColor = _cgvWhite;
                    lblText.ForeColor = _cgvWhite;
                    btn.FlatAppearance.BorderColor = _cgvGold;
                };

                btn.MouseLeave += (s, e) =>
                {
                    btn.BackColor = _cgvDarkGray;
                    lblIcon.ForeColor = _cgvRed;
                    lblText.ForeColor = _cgvWhite;
                    btn.FlatAppearance.BorderColor = _cgvRed;
                };

                // Assign action
                btn.Click += (s, e) => ExecuteQuickAction(action[0]);

                _quickActionsPanel.Controls.Add(btn);
            }
        }

        private void LoadHomeDashboard()
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            Panel homePanel = new Panel();
            homePanel.Dock = DockStyle.Fill;
            homePanel.BackColor = Color.Transparent;
            homePanel.AutoScroll = true;
            homePanel.Padding = new Padding(20);

            // Welcome section với CGV style
            Panel welcomeCard = CreateRoundedCard(20);
            welcomeCard.Dock = DockStyle.Top;
            welcomeCard.Height = 200;
            welcomeCard.BackColor = _cgvBlack;
            welcomeCard.Padding = new Padding(40);

            // Gradient background effect
            welcomeCard.Paint += (s, e) =>
            {
                using (LinearGradientBrush gradient = new LinearGradientBrush(
                    welcomeCard.ClientRectangle,
                    Color.FromArgb(50, 50, 50),
                    _cgvBlack,
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(gradient, welcomeCard.ClientRectangle);
                }
            };

            Label lblWelcome = new Label();
            lblWelcome.Text = $"CHÀO MỪNG TRỞ LẠI, {_fullName.ToUpper()}! 🎬";
            lblWelcome.Font = new Font("Arial", 28, FontStyle.Bold);
            lblWelcome.ForeColor = _cgvWhite;
            lblWelcome.Location = new Point(40, 40);
            lblWelcome.AutoSize = true;

            Label lblDate = new Label();
            lblDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy").ToUpper();
            lblDate.Font = new Font("Segoe UI", 14);
            lblDate.ForeColor = _cgvGold;
            lblDate.Location = new Point(40, 90);
            lblDate.AutoSize = true;

            Label lblQuote = new Label();
            lblQuote.Text = "\"Mỗi bộ phim là một hành trình mới - CGV Cinemas\"";
            lblQuote.Font = new Font("Segoe UI", 12, FontStyle.Italic);
            lblQuote.ForeColor = Color.FromArgb(200, 200, 200);
            lblQuote.Location = new Point(40, 130);
            lblQuote.AutoSize = true;

            welcomeCard.Controls.AddRange(new Control[] { lblWelcome, lblDate, lblQuote });
            homePanel.Controls.Add(welcomeCard);

            // Stats section
            Label lblStatsTitle = new Label();
            lblStatsTitle.Text = "THỐNG KÊ NHANH";
            lblStatsTitle.Font = new Font("Arial", 18, FontStyle.Bold);
            lblStatsTitle.ForeColor = _cgvBlack;
            lblStatsTitle.Dock = DockStyle.Top;
            lblStatsTitle.Height = 70;
            lblStatsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblStatsTitle.Margin = new Padding(0, 30, 0, 0);
            homePanel.Controls.Add(lblStatsTitle);

            // Stats cards với CGV style
            FlowLayoutPanel statsPanel = new FlowLayoutPanel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 180;
            statsPanel.Margin = new Padding(0, 0, 0, 30);
            statsPanel.Padding = new Padding(0, 10, 0, 0);

            string[] stats = GetDashboardStats();
            Color[] statColors = {
                _cgvRed,
                Color.FromArgb(52, 152, 219), // Blue
                Color.FromArgb(155, 89, 182), // Purple
                Color.FromArgb(46, 204, 113)  // Green
            };

            for (int i = 0; i < stats.Length; i++)
            {
                Panel statCard = CreateStatCard(stats[i], statColors[i]);
                statCard.Margin = new Padding(0, 0, 20, 0);
                statsPanel.Controls.Add(statCard);
            }

            homePanel.Controls.Add(statsPanel);

            // Recent activities với CGV style
            Panel activitiesCard = CreateRoundedCard(15);
            activitiesCard.Dock = DockStyle.Top;
            activitiesCard.Height = 320;
            activitiesCard.BackColor = _cgvWhite;
            activitiesCard.Padding = new Padding(25);

            Label lblActivitiesTitle = new Label();
            lblActivitiesTitle.Text = "🎯 HOẠT ĐỘNG GẦN ĐÂY";
            lblActivitiesTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblActivitiesTitle.ForeColor = _cgvBlack;
            lblActivitiesTitle.Dock = DockStyle.Top;
            lblActivitiesTitle.Height = 40;

            ListBox lstActivities = new ListBox();
            lstActivities.Dock = DockStyle.Fill;
            lstActivities.BorderStyle = BorderStyle.None;
            lstActivities.BackColor = _cgvWhite;
            lstActivities.Font = new Font("Segoe UI", 11);
            lstActivities.ItemHeight = 45;
            lstActivities.ForeColor = _cgvTextColor;

            // Style cho ListBox items
            lstActivities.DrawMode = DrawMode.OwnerDrawVariable;
            lstActivities.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;

                e.DrawBackground();

                // Alternating row colors
                Color backColor = e.Index % 2 == 0 ? Color.FromArgb(250, 250, 250) : _cgvWhite;
                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                // Draw activity text
                string activity = lstActivities.Items[e.Index].ToString();
                using (SolidBrush textBrush = new SolidBrush(_cgvTextColor))
                {
                    e.Graphics.DrawString(activity, new Font("Segoe UI", 10), textBrush,
                        new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 5, e.Bounds.Width - 20, e.Bounds.Height - 10));
                }

                // Draw separator
                using (Pen pen = new Pen(Color.FromArgb(230, 230, 230)))
                {
                    e.Graphics.DrawLine(pen, e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                }

                e.DrawFocusRectangle();
            };

            // Add activities
            string[] activities = {
                $"[{DateTime.Now:HH:mm}] 🔐 Đăng nhập thành công vào hệ thống CGV",
                $"[{DateTime.Now.AddMinutes(-15):HH:mm}] 🎬 Cập nhật thông tin phim 'Mai'",
                $"[{DateTime.Now.AddMinutes(-30):HH:mm}] 🎟️ Bán 5 vé cho suất chiếu 18:00",
                $"[{DateTime.Now.AddHours(-1):HH:mm}] 👥 Thêm nhân viên mới vào hệ thống",
                $"[{DateTime.Now.AddHours(-2):HH:mm}] 📊 Tạo báo cáo doanh thu tháng",
                $"[{DateTime.Now.AddHours(-3):HH:mm}] 🔧 Xử lí sự cố máy chiếu phòng 5"
            };

            foreach (var activity in activities)
            {
                lstActivities.Items.Add(activity);
            }

            activitiesCard.Controls.Add(lstActivities);
            activitiesCard.Controls.Add(lblActivitiesTitle);
            homePanel.Controls.Add(activitiesCard);

            // Performance chart với CGV style
            Panel chartCard = CreateRoundedCard(15);
            chartCard.Dock = DockStyle.Top;
            chartCard.Height = 380;
            chartCard.BackColor = _cgvWhite;
            chartCard.Margin = new Padding(0, 30, 0, 0);
            chartCard.Padding = new Padding(25);

            Label lblChartTitle = new Label();
            lblChartTitle.Text = "📈 BIỂU ĐỒ HIỆU SUẤT CGV";
            lblChartTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblChartTitle.ForeColor = _cgvBlack;
            lblChartTitle.Dock = DockStyle.Top;
            lblChartTitle.Height = 40;

            // Chart panel với CGV theme
            Panel chartPanel = new Panel();
            chartPanel.Dock = DockStyle.Fill;
            chartPanel.BackColor = Color.Transparent;
            chartPanel.Padding = new Padding(20);

            // Chart header
            Panel chartHeader = new Panel();
            chartHeader.Dock = DockStyle.Top;
            chartHeader.Height = 60;
            chartHeader.BackColor = Color.Transparent;

            Label lblChartDesc = new Label();
            lblChartDesc.Text = "Doanh thu 7 ngày gần đây - " + _branch;
            lblChartDesc.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblChartDesc.ForeColor = _cgvRed;
            lblChartDesc.Location = new Point(0, 10);
            lblChartDesc.AutoSize = true;

            // Simple chart visualization
            Panel chartVisual = new Panel();
            chartVisual.Dock = DockStyle.Fill;
            chartVisual.BackColor = Color.Transparent;
            chartVisual.Paint += (s, e) =>
            {
                // Draw chart background
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw grid lines
                using (Pen gridPen = new Pen(Color.FromArgb(240, 240, 240), 1))
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        int y = chartVisual.Height - (i * chartVisual.Height / 10);
                        e.Graphics.DrawLine(gridPen, 50, y, chartVisual.Width - 50, y);
                    }
                }

                // Sample data
                int[] revenues = { 120, 180, 150, 220, 190, 250, 280 };
                string[] days = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
                int barWidth = 60;
                int spacing = 30;

                for (int i = 0; i < revenues.Length; i++)
                {
                    int x = 80 + i * (barWidth + spacing);
                    int height = (revenues[i] * chartVisual.Height / 300);
                    int y = chartVisual.Height - height - 30;

                    // Draw bar
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new Rectangle(x, y, barWidth, height),
                        _cgvRed,
                        _cgvDarkRed,
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, x, y, barWidth, height);
                    }

                    // Draw value
                    using (Font valueFont = new Font("Segoe UI", 10, FontStyle.Bold))
                    {
                        e.Graphics.DrawString($"{revenues[i]}K", valueFont,
                            Brushes.Black, x, y - 20);
                    }

                    // Draw day label
                    using (Font dayFont = new Font("Segoe UI", 10))
                    {
                        e.Graphics.DrawString(days[i], dayFont,
                            Brushes.Gray, x + barWidth / 2 - 10, chartVisual.Height - 20);
                    }
                }
            };

            chartHeader.Controls.Add(lblChartDesc);
            chartPanel.Controls.Add(chartVisual);
            chartPanel.Controls.Add(chartHeader);
            chartCard.Controls.Add(chartPanel);
            chartCard.Controls.Add(lblChartTitle);
            homePanel.Controls.Add(chartCard);

            _mainContentPanel.Controls.Add(homePanel);
            _mainContentPanel.ResumeLayout();
        }

        private Panel CreateRoundedCard(int radius)
        {
            Panel panel = new Panel();
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.None;

            // Custom paint for rounded corners với shadow effect
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

                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(panel.BackColor), path);

                    // Add subtle shadow với CGV style
                    ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                        Color.FromArgb(240, 240, 240), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(240, 240, 240), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(240, 240, 240), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(240, 240, 240), 1, ButtonBorderStyle.Solid);
                }
            };

            return panel;
        }

        private Panel CreateStatCard(string statText, Color color)
        {
            Panel card = CreateRoundedCard(15);
            card.Size = new Size(300, 160);
            card.Padding = new Padding(25);
            card.BackColor = Color.White;

            // Icon với CGV style
            Panel iconPanel = new Panel();
            iconPanel.Size = new Size(50, 50);
            iconPanel.Location = new Point(25, 25);
            iconPanel.BackColor = Color.FromArgb(color.R, color.G, color.B, 20); // Màu nhạt
            iconPanel.BorderRadius(10);

            Label lblIcon = new Label();
            lblIcon.Text = GetStatIcon(statText);
            lblIcon.Font = new Font("Segoe UI", 20);
            lblIcon.ForeColor = color;
            lblIcon.Dock = DockStyle.Fill;
            lblIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            iconPanel.Controls.Add(lblIcon);

            // Value
            string value = GetStatValue(statText);
            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Arial", 32, FontStyle.Bold);
            lblValue.ForeColor = _cgvBlack;
            lblValue.Location = new Point(90, 25);
            lblValue.AutoSize = true;

            // Description
            string desc = GetStatDescription(statText);
            Label lblDesc = new Label();
            lblDesc.Text = desc.ToUpper();
            lblDesc.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblDesc.ForeColor = Color.Gray;
            lblDesc.Location = new Point(90, 70);
            lblDesc.AutoSize = true;

            // Trend indicator
            Label lblTrend = new Label();
            lblTrend.Text = "📈 +12%";
            lblTrend.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblTrend.ForeColor = Color.FromArgb(46, 204, 113);
            lblTrend.Location = new Point(card.Width - 80, 25);
            lblTrend.AutoSize = true;

            // Decorative line
            Panel line = new Panel();
            line.Size = new Size(250, 2);
            line.Location = new Point(25, 110);
            line.BackColor = Color.FromArgb(240, 240, 240);

            card.Controls.Add(line);
            card.Controls.AddRange(new Control[] { iconPanel, lblValue, lblDesc, lblTrend });

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(250, 250, 250);
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
            };

            return card;
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
                    new string[] { "THÊM NGƯỜI DÙNG", "" },
                    new string[] { "TẠO BÁO CÁO", "" },
                    new string[] { "QUẢN LÝ CHI NHÁNH", "" },
                    new string[] { "XEM LOG HỆ THỐNG", "" },
                    new string[] { "SAO LƯU DỮ LIỆU", "" }
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
                    new string[] { "QUẢN LÝ NHÂN SỰ", "👥" }
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
                    new string[] { "BÁO CÁO SỰ CỐ", "⚠️" }
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
            Random rnd = new Random();
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
            // Hiệu ứng visual khi nhấn
            foreach (Control ctrl in _quickActionsPanel.Controls)
            {
                if (ctrl is Button btn && btn.Text.Contains(action))
                {
                    btn.BackColor = _cgvGold;
                    btn.ForeColor = _cgvBlack;

                    Timer timer = new Timer();
                    timer.Interval = 300;
                    timer.Tick += (s, e) =>
                    {
                        btn.BackColor = _cgvDarkGray;
                        btn.ForeColor = _cgvWhite;
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                    break;
                }
            }

            MessageBox.Show($"🎬 CGV ACTION\n\nThực hiện: {action}", "CGV Cinema Management",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowSettingsMenu(Control sender)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = _cgvWhite;
            menu.ForeColor = _cgvBlack;
            menu.Font = new Font("Segoe UI", 11);
            menu.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());

            ToolStripMenuItem profileItem = new ToolStripMenuItem("🎫 Thông tin cá nhân", null, (s, e) => ShowUserProfile());
            ToolStripMenuItem passItem = new ToolStripMenuItem("🔐 Đổi mật khẩu", null, (s, e) => ChangePassword());
            ToolStripMenuItem themeItem = new ToolStripMenuItem("🎨 Thay đổi giao diện", null, (s, e) => ChangeTheme());
            ToolStripMenuItem logoutItem = new ToolStripMenuItem("🚪 Đăng xuất", null, (s, e) => Logout());
            ToolStripMenuItem exitItem = new ToolStripMenuItem("❌ Thoát", null, (s, e) => Application.Exit());

            profileItem.ForeColor = _cgvBlack;
            passItem.ForeColor = _cgvBlack;
            themeItem.ForeColor = _cgvBlack;
            logoutItem.ForeColor = _cgvRed;
            exitItem.ForeColor = Color.DarkRed;

            menu.Items.Add(profileItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(passItem);
            menu.Items.Add(themeItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(logoutItem);
            menu.Items.Add(exitItem);

            menu.Show(sender, new Point(0, sender.Height));
        }

        private void ShowUserProfile()
        {
            Form profileForm = new Form();
            profileForm.Text = "🎬 THÔNG TIN CÁ NHÂN - CGV";
            profileForm.Size = new Size(400, 350);
            profileForm.StartPosition = FormStartPosition.CenterParent;
            profileForm.BackColor = _cgvLightGray;
            profileForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            profileForm.MaximizeBox = false;
            profileForm.MinimizeBox = false;

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(30);
            mainPanel.BackColor = _cgvWhite;
            mainPanel.BorderRadius(10);

            Label lblTitle = new Label();
            lblTitle.Text = "THÔNG TIN NGƯỜI DÙNG";
            lblTitle.Font = new Font("Arial", 18, FontStyle.Bold);
            lblTitle.ForeColor = _cgvRed;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 60;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            Panel infoPanel = new Panel();
            infoPanel.Dock = DockStyle.Fill;
            infoPanel.Padding = new Padding(20);

            string[] labels = { "Họ tên:", "Vai trò:", "Chi nhánh:", "Tên đăng nhập:", "Ngày tham gia:" };
            string[] values = {
                _fullName,
                _userRole,
                _branch,
                _username,
                DateTime.Now.ToString("dd/MM/yyyy")
            };

            int yPos = 20;
            for (int i = 0; i < labels.Length; i++)
            {
                Label lbl = new Label();
                lbl.Text = labels[i];
                lbl.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                lbl.ForeColor = _cgvDarkGray;
                lbl.Location = new Point(20, yPos);
                lbl.AutoSize = true;

                Label val = new Label();
                val.Text = values[i];
                val.Font = new Font("Segoe UI", 11);
                val.ForeColor = _cgvBlack;
                val.Location = new Point(150, yPos);
                val.AutoSize = true;

                infoPanel.Controls.Add(lbl);
                infoPanel.Controls.Add(val);
                yPos += 40;
            }

            Button btnClose = new Button();
            btnClose.Text = "ĐÓNG";
            btnClose.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnClose.Size = new Size(120, 40);
            btnClose.Location = new Point(140, yPos + 20);
            btnClose.BackColor = _cgvRed;
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Cursor = Cursors.Hand;
            btnClose.BorderRadius(5);
            btnClose.Click += (s, e) => profileForm.Close();

            infoPanel.Controls.Add(btnClose);
            mainPanel.Controls.Add(infoPanel);
            mainPanel.Controls.Add(lblTitle);
            profileForm.Controls.Add(mainPanel);

            profileForm.ShowDialog();
        }

        private void ChangePassword()
        {
            MessageBox.Show("🔐 Chức năng đổi mật khẩu đang được phát triển\n\nSẽ có sẵn trong phiên bản tiếp theo!",
                "CGV - Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChangeTheme()
        {
            MessageBox.Show("🎨 Chức năng thay đổi giao diện đang được phát triển\n\nCho phép tùy chỉnh màu sắc CGV!",
                "CGV - Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show("🎬 Bạn có chắc chắn muốn đăng xuất khỏi hệ thống CGV?",
                "CGV - Xác nhận đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                // Animation effect
                this.Opacity = 0.9;
                Timer fadeTimer = new Timer();
                fadeTimer.Interval = 50;
                int fadeCount = 0;

                fadeTimer.Tick += (s, e) =>
                {
                    fadeCount++;
                    this.Opacity = 0.9 - (fadeCount * 0.1);

                    if (fadeCount >= 9)
                    {
                        fadeTimer.Stop();

                        // Đóng form hiện tại
                        this.Close();

                        // Mở lại form đăng nhập
                        frmLogin loginForm = new frmLogin();
                        loginForm.StartPosition = FormStartPosition.CenterScreen;

                        if (loginForm.ShowDialog() == DialogResult.OK)
                        {
                            // Nếu đăng nhập thành công, mở form chính mới
                            frmMain mainForm = new frmMain(loginForm.LoggedInUsername, loginForm.LoggedInRole,
                                loginForm.LoggedInBranch, loginForm.LoggedInFullName);
                            mainForm.Show();
                        }
                    }
                };

                fadeTimer.Start();
            }
        }

        // Các phương thức chức năng giữ nguyên (không thay đổi logic)
        private void LoadAdminUsers()
        {
            ShowPlaceholder("👥 QUẢN LÝ NGƯỜI DÙNG", "Chức năng quản lý tài khoản người dùng hệ thống CGV");
        }

        private void LoadAdminBranches()
        {
            ShowPlaceholder("🏢 QUẢN LÝ CHI NHÁNH", "Chức năng quản lý các chi nhánh CGV trên toàn quốc");
        }

        private void LoadMovies()
        {
            LoadUserControl(new UC_Movies());
        }

        private void LoadShowtimes()
        {
            ShowPlaceholder("🎟️ LỊCH CHIẾU", "Chức năng quản lý lịch chiếu phim CGV");
        }

        private void LoadTicketSales()
        {
            ShowPlaceholder("💺 BÁN VÉ VÀ ĐẶT GHẾ", "Chức năng bán vé và đặt ghế cho khách hàng CGV");
        }

        private void LoadInventory()
        {
            LoadUserControl(new UC_Kho(1, 1)); // Pass maChiNhanh and maNguoiDung
        }

        private void SetupReportTabControl()
        {
            // Create TabControl for Reports sidebar
            _reportTabControl = new TabControl
            {
                Dock = DockStyle.Top,
                Height = 240,
                BackColor = _cgvDarkGray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Create tabs for each report với CGV style
            TabPage tabTongQuan = new TabPage("🏠 TỔNG QUAN");
            TabPage tabDoanhThu = new TabPage("💰 DOANH THU");
            TabPage tabSanPham = new TabPage("📦 SẢN PHẨM");
            TabPage tabPhim = new TabPage("🎬 PHIM");
            TabPage tabNhanVien = new TabPage("👥 NHÂN VIÊN");

            _reportTabControl.TabPages.AddRange(new TabPage[] {
                tabTongQuan, tabDoanhThu, tabSanPham, tabPhim, tabNhanVien
            });

            // Style tabs
            foreach (TabPage tab in _reportTabControl.TabPages)
            {
                tab.BackColor = _cgvDarkGray;
                tab.ForeColor = Color.White;
                tab.Font = new Font("Segoe UI", 10);
            }

            // Setup tab selection event
            _reportTabControl.Selected += (s, e) =>
            {
                int tabIndex = _reportTabControl.SelectedIndex;
                LoadReportTab(tabIndex);
            };

            // Thêm vào sidebar sau menu items
            _sidebar.Controls.Add(_reportTabControl);
            _sidebar.Controls.SetChildIndex(_reportTabControl, 1);
        }

        private void LoadReportTab(int tabIndex)
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            try
            {
                int maChiNhanh = 1;
                string tenChiNhanh = _branch ?? "CGV " + _branch;

                // Create UC_BaoCaoChiNhanh
                if (_currentBaoCaoControl == null || _currentBaoCaoControl.IsDisposed)
                {
                    _currentBaoCaoControl = new UC_BaoCaoChiNhanh(maChiNhanh, tenChiNhanh);
                }

                // Clear and add control
                _mainContentPanel.Controls.Clear();
                _mainContentPanel.Controls.Add(_currentBaoCaoControl);

                // Switch to selected tab in UC_BaoCaoChiNhanh's internal TabControl
                foreach (Control ctrl in _currentBaoCaoControl.Controls)
                {
                    if (ctrl is TabControl internalTabControl)
                    {
                        internalTabControl.SelectedIndex = tabIndex;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"🎬 Lỗi khi tải báo cáo: {ex.Message}", "CGV - Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _mainContentPanel.ResumeLayout();
            }
        }

        private void LoadReports()
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            try
            {
                // Get manager's branch info
                int maChiNhanh = 1; // Default, should be from user context
                string tenChiNhanh = "CGV " + (_branch ?? "Chi Nhánh Mặc Định");

                // Create and load UC_BaoCaoChiNhanh
                _currentBaoCaoControl = new UC_BaoCaoChiNhanh(maChiNhanh, tenChiNhanh);
                _mainContentPanel.Controls.Add(_currentBaoCaoControl);

                // Select first tab
                if (_reportTabControl != null)
                {
                    _reportTabControl.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"🎬 Lỗi khi tải báo cáo: {ex.Message}", "CGV - Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _mainContentPanel.ResumeLayout();
            }
        }

        private void LoadWorkSchedule()
        {
            LoadUserControl(new UC_LichLamViec());
        }

        private void LoadPerformance()
        {
            LoadUserControl(new UC_HieuSuat());
        }

        private void LoadLeaveRequests()
        {
            LoadUserControl(new UC_DonXinNghi(1, 1)); // Pass maChiNhanh and maNguoiDung
        }

        private void LoadApproveLeave()
        {
            LoadUserControl(new UC_DuyetDonXinNghi(1, 1)); // Pass maChiNhanh and maNguoiDung
        }

        private void LoadStaffManagement()
        {
            LoadUserControl(new UC_NhanSu(1)); // Pass maChiNhanh
        }

        private void LoadAdminPanel()
        {
            LoadUserControl(new UC_Admin());
        }

        private void LoadSettings()
        {
            ShowPlaceholder("⚙️ CÀI ĐẶT HỆ THỐNG", "Cấu hình và tùy chỉnh hệ thống quản lý CGV");
        }

        /// <summary>
        /// Helper method để hiển thị placeholder cho chức năng chưa được implement
        /// </summary>
        private void ShowPlaceholder(string title, string description)
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            Panel placeholderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _cgvLightGray
            };

            // Center panel
            Panel centerPanel = new Panel();
            centerPanel.Size = new Size(600, 400);
            centerPanel.Location = new Point(
                (_mainContentPanel.Width - 600) / 2,
                (_mainContentPanel.Height - 400) / 2);
            centerPanel.BackColor = _cgvWhite;
            centerPanel.BorderRadius(20);
            centerPanel.Padding = new Padding(40);

            // Icon
            Label lblIcon = new Label
            {
                Text = "🎬",
                Font = new Font("Segoe UI", 72),
                ForeColor = _cgvRed,
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Title
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = _cgvBlack,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20)
            };

            // Description
            Label lblDescription = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(20)
            };

            // Coming Soon Message
            Label lblComingSoon = new Label
            {
                Text = "🔨 Chức năng này đang được phát triển bởi CGV IT Team",
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                ForeColor = _cgvRed,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(20)
            };

            centerPanel.Controls.Add(lblComingSoon);
            centerPanel.Controls.Add(lblDescription);
            centerPanel.Controls.Add(lblTitle);
            centerPanel.Controls.Add(lblIcon);

            placeholderPanel.Controls.Add(centerPanel);
            _mainContentPanel.Controls.Add(placeholderPanel);
            _mainContentPanel.ResumeLayout();
        }

        /// <summary>
        /// Helper method to load UserControl into main panel
        /// </summary>
        private void LoadUserControl(UserControl control)
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            try
            {
                control.Dock = DockStyle.Fill;
                _mainContentPanel.Controls.Add(control);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"🎬 Lỗi khi tải control: {ex.Message}", "CGV - Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _mainContentPanel.ResumeLayout();
            }
        }
    }

    // Helper classes với CGV style
    public class SidebarMenuItem
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public Action Action { get; set; }
        public bool IsActive { get; set; }
    }

    public class MenuColorTable : ProfessionalColorTable
    {
        private Color _cgvRed = Color.FromArgb(220, 53, 69);
        private Color _cgvBlack = Color.FromArgb(20, 20, 20);

        public override Color MenuItemSelected => Color.FromArgb(240, 240, 240);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(240, 240, 240);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(240, 240, 240);
        public override Color MenuItemBorder => _cgvRed;
        public override Color MenuBorder => Color.FromArgb(230, 230, 230);
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
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
