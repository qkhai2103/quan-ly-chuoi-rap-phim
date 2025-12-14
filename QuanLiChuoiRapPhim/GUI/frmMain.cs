using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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

        // Theme colors
        private Color _primaryColor = Color.FromArgb(106, 90, 205); // Purple
        private Color _secondaryColor = Color.FromArgb(65, 105, 225); // Royal Blue
        private Color _accentColor = Color.FromArgb(123, 104, 238); // Light Purple
        private Color _darkColor = Color.FromArgb(30, 33, 57); // Dark Blue
        private Color _lightColor = Color.FromArgb(248, 249, 252); // Light Gray
        private Color _textColor = Color.FromArgb(51, 51, 51); // Dark Gray

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
            this.Text = "CGV | {_userRole.ToUpper()}";
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _lightColor;
            this.WindowState = FormWindowState.Maximized;

            // ========== HEADER ==========
            _header = new Panel();
            _header.Dock = DockStyle.Top;
            _header.Height = 70;
            _header.BackColor = Color.White;
            _header.Padding = new Padding(25, 0, 25, 0);

            // Logo
            Label lblLogo = new Label();
            lblLogo.Text = "CGV";
            lblLogo.Font = new Font("Poppins", 20, FontStyle.Bold);
            lblLogo.ForeColor = _primaryColor;
            lblLogo.Location = new Point(25, 20);
            lblLogo.AutoSize = true;

            // Search box
            Panel searchPanel = new Panel();
            searchPanel.Size = new Size(300, 36);
            searchPanel.Location = new Point(250, 17);
            searchPanel.BackColor = Color.FromArgb(245, 245, 245);
            searchPanel.BorderRadius(18);

            TextBox txtSearch = new TextBox();
            txtSearch.BorderStyle = BorderStyle.None;
          
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.Size = new Size(260, 36);
            txtSearch.Location = new Point(15, 8);

            Label lblSearchIcon = new Label();
            lblSearchIcon.Text = "";
            lblSearchIcon.Font = new Font("Segoe UI", 12);
            lblSearchIcon.ForeColor = Color.Gray;
            lblSearchIcon.Size = new Size(30, 36);
            lblSearchIcon.Location = new Point(searchPanel.Width - 40, 8);
            lblSearchIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            searchPanel.Controls.Add(lblSearchIcon);
            searchPanel.Controls.Add(txtSearch);

            // User info and notifications
            Panel userPanel = new Panel();
            userPanel.Size = new Size(350, 70);
            userPanel.Location = new Point(_header.Width - 400, 0);
            userPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Notification bell
            Button btnNotification = new Button();
            btnNotification.Text = "";
            btnNotification.Font = new Font("Segoe UI", 16);
            btnNotification.Size = new Size(50, 50);
            btnNotification.Location = new Point(10, 10);
            btnNotification.FlatStyle = FlatStyle.Flat;
            btnNotification.FlatAppearance.BorderSize = 0;
            btnNotification.BackColor = Color.Transparent;
            btnNotification.ForeColor = _darkColor;
            btnNotification.Cursor = Cursors.Hand;

            // Notification badge
            Label lblNotificationBadge = new Label();
            lblNotificationBadge.Text = "3";
            lblNotificationBadge.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lblNotificationBadge.ForeColor = Color.White;
            lblNotificationBadge.BackColor = Color.FromArgb(255, 87, 87);
            lblNotificationBadge.Size = new Size(18, 18);
            lblNotificationBadge.Location = new Point(35, 5);
            lblNotificationBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblNotificationBadge.BorderRadius(9);

            // User avatar and info
            Panel avatarPanel = new Panel();
            avatarPanel.Size = new Size(40, 40);
            avatarPanel.Location = new Point(70, 15);
            avatarPanel.BackColor = _primaryColor;
            avatarPanel.BorderRadius(20);

            Label lblAvatar = new Label();
            lblAvatar.Text = _fullName.Substring(0, 1).ToUpper();
            lblAvatar.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblAvatar.ForeColor = Color.White;
            lblAvatar.Dock = DockStyle.Fill;
            lblAvatar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            avatarPanel.Controls.Add(lblAvatar);

            Label lblUserInfo = new Label();
            lblUserInfo.Text = $"{_fullName}\n{_userRole}";
            lblUserInfo.Font = new Font("Segoe UI", 9);
            lblUserInfo.ForeColor = _textColor;
            lblUserInfo.AutoSize = true;
            lblUserInfo.Location = new Point(120, 20);

            // Settings dropdown
            Button btnSettings = new Button();
            btnSettings.Text = "??";
            btnSettings.Font = new Font("Segoe UI", 16);
            btnSettings.Size = new Size(50, 50);
            btnSettings.Location = new Point(280, 10);
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.BackColor = Color.Transparent;
            btnSettings.ForeColor = _darkColor;
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Click += (s, e) => ShowSettingsMenu(btnSettings);

            userPanel.Controls.AddRange(new Control[] {
                btnNotification, lblNotificationBadge,
                avatarPanel, lblUserInfo, btnSettings
            });

            _header.Controls.AddRange(new Control[] { lblLogo, searchPanel, userPanel });

            // ========== SIDEBAR ==========
            _sidebar = new Panel();
            _sidebar.Dock = DockStyle.Left;
            _sidebar.Width = 280;
            _sidebar.BackColor = _darkColor;

            // Sidebar header
            Label lblSidebarHeader = new Label();
            lblSidebarHeader.Text = "MENU CHÍNH";
            lblSidebarHeader.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblSidebarHeader.ForeColor = Color.FromArgb(180, 180, 220);
            lblSidebarHeader.Dock = DockStyle.Top;
            lblSidebarHeader.Height = 60;
            lblSidebarHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblSidebarHeader.Padding = new Padding(30, 0, 0, 0);

            _sidebar.Controls.Add(lblSidebarHeader);

            // ========== MAIN CONTENT ==========
            _mainContentPanel = new Panel();
            _mainContentPanel.Dock = DockStyle.Fill;
            _mainContentPanel.BackColor = _lightColor;

            // ========== QUICK ACTIONS BAR ==========
            _quickActionsPanel = new FlowLayoutPanel();
            _quickActionsPanel.Dock = DockStyle.Bottom;
            _quickActionsPanel.Height = 80;
            _quickActionsPanel.BackColor = Color.White;
            _quickActionsPanel.Padding = new Padding(20, 10, 20, 10);
            _quickActionsPanel.FlowDirection = FlowDirection.LeftToRight;

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

            // Dashboard
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "TỔNG QUAN",
                Icon = "",
                Action = LoadHomeDashboard,
                IsActive = true
            });

            // Admin specific items
            if (_userRole == "Admin")
            {
                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "QUẢN LÝ NGƯỜI DÙNG",
                    Icon = "",
                    Action = LoadAdminUsers
                });

                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "QUẢN LÝ CHI NHÁNH",
                    Icon = "",
                    Action = LoadAdminBranches
                });
            }

            // Common items
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "QUẢN LÝ PHIM",
                Icon = "",
                Action = LoadMovies
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "LICHJ CHIẾU",
                Icon = "",
                Action = LoadShowtimes
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "BÁN VÉ VÀ ĐẶT GHẾ",
                Icon = "",
                Action = LoadTicketSales
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "KHO",
                Icon = "",
                Action = LoadInventory
            });

            _menuItems.Add(new SidebarMenuItem
            {
                Text = "BÁO CÁO THỐNG KÊ",
                Icon = "",
                Action = LoadReports
            });

            // Manager specific items
            if (_userRole == "Quản lý" || _userRole == "Admin")
            {
                _menuItems.Add(new SidebarMenuItem
                {
                    Text = "QUẢN LÝ NHÂN SỰ",
                    Icon = "",
                    Action = LoadStaffManagement
                });
            }

            // Settings
            _menuItems.Add(new SidebarMenuItem
            {
                Text = "CÀI ĐẶT HỆ THỐNG",
                Icon = "",
                Action = LoadSettings
            });

            // Add menu items to sidebar
            int yPos = 70;
            foreach (var menuItem in _menuItems)
            {
                var button = CreateSidebarButton(menuItem, yPos);
                _sidebar.Controls.Add(button);
                yPos += 55;
            }

            _activeMenuItem = _menuItems[0];
        }

        private Button CreateSidebarButton(SidebarMenuItem menuItem, int yPos)
        {
            Button btn = new Button();
            btn.Text = $"  {menuItem.Icon}  {menuItem.Text}";
            btn.Tag = menuItem;
            btn.Font = new Font("Segoe UI", 11);
            btn.ForeColor = menuItem.IsActive ? Color.White : Color.FromArgb(180, 180, 220);
            btn.BackColor = menuItem.IsActive ? _primaryColor : Color.Transparent;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btn.Size = new Size(_sidebar.Width, 50);
            btn.Location = new Point(0, yPos);
            btn.Padding = new Padding(25, 0, 0, 0);
            btn.Cursor = Cursors.Hand;

            // Hover effects
            btn.MouseEnter += (s, e) =>
            {
                if (!menuItem.IsActive)
                {
                    btn.BackColor = Color.FromArgb(40, 43, 67);
                    btn.ForeColor = Color.White;
                }
            };

            btn.MouseLeave += (s, e) =>
            {
                if (!menuItem.IsActive)
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Color.FromArgb(180, 180, 220);
                }
            };

            btn.Click += (s, e) =>
            {
                // Update active state
                foreach (Control control in _sidebar.Controls)
                {
                    if (control is Button sidebarBtn && sidebarBtn.Tag is SidebarMenuItem item)
                    {
                        sidebarBtn.BackColor = item == menuItem ? _primaryColor : Color.Transparent;
                        sidebarBtn.ForeColor = item == menuItem ? Color.White : Color.FromArgb(180, 180, 220);
                        item.IsActive = (item == menuItem);
                    }
                }

                _activeMenuItem = menuItem;
                menuItem.Action?.Invoke();
            };

            return btn;
        }

        private void AddQuickActions()
        {
            string[][] quickActions = GetQuickActionsByRole();

            foreach (var action in quickActions)
            {
                Button btn = new Button();
                btn.Text = action[0];
                btn.Font = new Font("Segoe UI", 10);
                btn.Size = new Size(180, 50);
                btn.Margin = new Padding(5);
                btn.BackColor = Color.White;
                btn.ForeColor = _textColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
                btn.Cursor = Cursors.Hand;

                // Icon
                Label lblIcon = new Label();
                lblIcon.Text = action[1];
                lblIcon.Font = new Font("Segoe UI", 14);
                lblIcon.Location = new Point(15, 15);
                lblIcon.AutoSize = true;
                btn.Controls.Add(lblIcon);

                // Text
                Label lblText = new Label();
                lblText.Text = action[0];
                lblText.Font = new Font("Segoe UI", 9);
                lblText.Location = new Point(50, 18);
                lblText.AutoSize = true;
                btn.Controls.Add(lblText);

                // Hover effect
                btn.MouseEnter += (s, e) =>
                {
                    btn.BackColor = _lightColor;
                    btn.FlatAppearance.BorderColor = _primaryColor;
                };

                btn.MouseLeave += (s, e) =>
                {
                    btn.BackColor = Color.White;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
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
            homePanel.Padding = new Padding(30);

            // Welcome section
            Panel welcomeCard = CreateRoundedCard(30);
            welcomeCard.Dock = DockStyle.Top;
            welcomeCard.Height = 180;
            welcomeCard.BackColor = Color.White;
            welcomeCard.Padding = new Padding(30);

            Label lblWelcome = new Label();
            lblWelcome.Text = $"CHÀO MỪNG TRỞ LẠI, {_fullName}! ??";
            lblWelcome.Font = new Font("Poppins", 24, FontStyle.Bold);
            lblWelcome.ForeColor = _darkColor;
            lblWelcome.Location = new Point(30, 30);
            lblWelcome.AutoSize = true;

            Label lblDate = new Label();
            lblDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            lblDate.Font = new Font("Segoe UI", 12);
            lblDate.ForeColor = Color.Gray;
            lblDate.Location = new Point(30, 80);
            lblDate.AutoSize = true;

            Label lblQuote = new Label();
            lblQuote.Text = "\"Mỗi bộ phim là một hành trình mới\"";
            lblQuote.Font = new Font("Segoe UI", 11, FontStyle.Italic);
            lblQuote.ForeColor = _primaryColor;
            lblQuote.Location = new Point(30, 110);
            lblQuote.AutoSize = true;

            welcomeCard.Controls.AddRange(new Control[] { lblWelcome, lblDate, lblQuote });
            homePanel.Controls.Add(welcomeCard);

            // Stats section
            Label lblStatsTitle = new Label();
            lblStatsTitle.Text = "THỐNG KÊ NHANH";
            lblStatsTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblStatsTitle.ForeColor = _darkColor;
            lblStatsTitle.Dock = DockStyle.Top;
            lblStatsTitle.Height = 60;
            lblStatsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblStatsTitle.Margin = new Padding(0, 20, 0, 0);
            homePanel.Controls.Add(lblStatsTitle);

            // Stats cards
            FlowLayoutPanel statsPanel = new FlowLayoutPanel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 150;
            statsPanel.Margin = new Padding(0, 0, 0, 20);

            string[] stats = GetDashboardStats();
            Color[] statColors = { _primaryColor, _secondaryColor, _accentColor, Color.FromArgb(46, 204, 113) };

            for (int i = 0; i < stats.Length; i++)
            {
                Panel statCard = CreateStatCard(stats[i], statColors[i]);
                statCard.Margin = new Padding(0, 0, 20, 0);
                statsPanel.Controls.Add(statCard);
            }

            homePanel.Controls.Add(statsPanel);

            // Recent activities
            Panel activitiesCard = CreateRoundedCard(20);
            activitiesCard.Dock = DockStyle.Top;
            activitiesCard.Height = 300;
            activitiesCard.BackColor = Color.White;
            activitiesCard.Padding = new Padding(25);

            Label lblActivitiesTitle = new Label();
            lblActivitiesTitle.Text = "HOẠT ĐỘNG GẦN ĐÂY";
            lblActivitiesTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblActivitiesTitle.ForeColor = _darkColor;
            lblActivitiesTitle.Dock = DockStyle.Top;
            lblActivitiesTitle.Height = 40;

            ListBox lstActivities = new ListBox();
            lstActivities.Dock = DockStyle.Fill;
            lstActivities.BorderStyle = BorderStyle.None;
            lstActivities.BackColor = Color.White;
            lstActivities.Font = new Font("Segoe UI", 10);
            lstActivities.ItemHeight = 40;

            // Add activities
            string[] activities = {
                $"[{DateTime.Now:HH:mm}] Đăng nhập thành công",
                $"[{DateTime.Now.AddMinutes(-15):HH:mm}] Cập nhật phim 'Mai'",
                $"[{DateTime.Now.AddMinutes(-30):HH:mm}] Bán 5 vé suất 18:00",
                $"[{DateTime.Now.AddHours(-1):HH:mm}] Thêm nhân viên mới",
                $"[{DateTime.Now.AddHours(-2):HH:mm}] Tạo báo cáo doanh thu",
                $"[{DateTime.Now.AddHours(-3):HH:mm}] Xử lí sự cố máy chiếu"
            };

            foreach (var activity in activities)
            {
                lstActivities.Items.Add(activity);
            }

            activitiesCard.Controls.Add(lstActivities);
            activitiesCard.Controls.Add(lblActivitiesTitle);
            homePanel.Controls.Add(activitiesCard);

            // Performance chart
            Panel chartCard = CreateRoundedCard(20);
            chartCard.Dock = DockStyle.Top;
            chartCard.Height = 350;
            chartCard.BackColor = Color.White;
            chartCard.Margin = new Padding(0, 20, 0, 0);
            chartCard.Padding = new Padding(25);

            Label lblChartTitle = new Label();
            lblChartTitle.Text = "Biểu đồ hiệu suất";
            lblChartTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblChartTitle.ForeColor = _darkColor;
            lblChartTitle.Dock = DockStyle.Top;
            lblChartTitle.Height = 40;

            // Simple chart using labels
            Panel chartPanel = new Panel();
            chartPanel.Dock = DockStyle.Fill;
            chartPanel.BackColor = Color.Transparent;

            // Add chart here (you can use Chart control)
            Label lblChartPlaceholder = new Label();
            lblChartPlaceholder.Text = "Biểu đồ doanh thu sẽ hiển thị tại đây";
            lblChartPlaceholder.Font = new Font("Segoe UI", 12);
            lblChartPlaceholder.ForeColor = Color.Gray;
            lblChartPlaceholder.Dock = DockStyle.Fill;
            lblChartPlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            chartPanel.Controls.Add(lblChartPlaceholder);
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

            // Custom paint for rounded corners
            panel.Paint += (s, e) =>
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                    path.AddArc(panel.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                    path.AddArc(panel.Width - radius * 2, panel.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(0, panel.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseFigure();

                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(new SolidBrush(panel.BackColor), path);

                    // Add subtle shadow
                    ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                        Color.FromArgb(230, 230, 230), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(230, 230, 230), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(230, 230, 230), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(230, 230, 230), 1, ButtonBorderStyle.Solid);
                }
            };

            return panel;
        }

        private Panel CreateStatCard(string statText, Color color)
        {
            Panel card = CreateRoundedCard(15);
            card.Size = new Size(280, 130);
            card.Padding = new Padding(20);

            // Icon
            Label lblIcon = new Label();
            lblIcon.Text = GetStatIcon(statText);
            lblIcon.Font = new Font("Segoe UI", 24);
            lblIcon.ForeColor = color;
            lblIcon.Location = new Point(20, 20);
            lblIcon.AutoSize = true;

            // Value
            string value = GetStatValue(statText);
            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Poppins", 28, FontStyle.Bold);
            lblValue.ForeColor = _darkColor;
            lblValue.Location = new Point(20, 50);
            lblValue.AutoSize = true;

            // Description
            string desc = GetStatDescription(statText);
            Label lblDesc = new Label();
            lblDesc.Text = desc;
            lblDesc.Font = new Font("Segoe UI", 10);
            lblDesc.ForeColor = Color.Gray;
            lblDesc.Location = new Point(20, 90);
            lblDesc.AutoSize = true;

            // Trend indicator
            Label lblTrend = new Label();
            lblTrend.Text = "? 12%";
            lblTrend.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblTrend.ForeColor = Color.FromArgb(46, 204, 113);
            lblTrend.Location = new Point(card.Width - 70, 20);
            lblTrend.AutoSize = true;

            card.Controls.AddRange(new Control[] { lblIcon, lblValue, lblDesc, lblTrend });

            return card;
        }

        private string[] GetDashboardStats()
        {
            if (_userRole == "Admin")
            {
                return new string[]
                {
                    "Doanh thu hôm nay",
                    "Tổng người dùng",
                    "Phim đang chiếu",
                    "Vé đã bán"
                };
            }
            else if (_userRole == "Quản lý")
            {
                return new string[]
                {
                    "Doanh thu chi nhánh",
                    "Nhân viên đang làm",
                    "Suất chiếu hôm nay",
                    "Sản phẩm tồn kho"
                };
            }
            else
            {
                return new string[]
                {
                    "Vé bán hôm nay",
                    "Doanh thu cá nhân",
                    "Khách hàng phục vụ",
                    "Ca làm việc"
                };
            }
        }

        private string[][] GetQuickActionsByRole()
        {
            if (_userRole == "Admin")
            {
                return new string[][]
                {
                    new string[] { "Thêm người dùng", "" },
                    new string[] { "Tạo báo cáo", "" },
                    new string[] { "Quản lí chi nhánh", "" },
                    new string[] { "Xem log hệ thống", "" },
                    new string[] { "Sao lưu dữ liệu", "" }
                };
            }
            else if (_userRole == "Quản lý")
            {
                return new string[][]
                {
                    new string[] { "Bán vé nhanh", "" },
                    new string[] { "Phân công ca", "" },
                    new string[] { "Kiểm kho", "" },
                    new string[] { "Xem báo cáo", "" },
                    new string[] { "Quản lý nhân sự", "" }
                };
            }
            else
            {
                return new string[][]
                {
                    new string[] { "Bán vé", "" },
                    new string[] { "Đặt ghế", "" },
                    new string[] { "Bán sản phẩm", "" },
                    new string[] { "Xem lịch làm", "" },
                    new string[] { "Báo cáo sự cố", "" }
                };
            }
        }

        private string GetStatIcon(string stat)
        {
            return stat switch
            {
                string s when s.Contains("Doanh thu") => "??",
                string s when s.Contains("Nguoi dung") => "??",
                string s when s.Contains("Phim") => "??",
                string s when s.Contains("Vé") => "??",
                string s when s.Contains("Nhân viên") => "?????",
                string s when s.Contains("Suất chiếu") => "??",
                string s when s.Contains("Sản phẩm") => "??",
                string s when s.Contains("Khách hàng") => "??",
                string s when s.Contains("Ca làm việc") => "?",
                _ => "??"
            };
        }

        private string GetStatValue(string stat)
        {
            Random rnd = new Random();
            return stat switch
            {
                string s when s.Contains("Doanh thu hôm nay") => "12.5M �",
                string s when s.Contains("Doanh thu chi nhánh") => "8.2M �",
                string s when s.Contains("Doanh thu cá nhân") => "2.1M �",
                string s when s.Contains("Tổng người dùng") => "42",
                string s when s.Contains("Phim đang chiếu") => "8",
                string s when s.Contains("Vé đã bán") => "156",
                string s when s.Contains("Nhân viên đang làm") => "12",
                string s when s.Contains("Suất chiếu hôm nay") => "24",
                string s when s.Contains("Sản phẩm tồn kho") => "342",
                string s when s.Contains("Khách hàng phục vụ") => "89",
                string s when s.Contains("Ca làm việc") => "3",
                _ => "0"
            };
        }

        private string GetStatDescription(string stat)
        {
            return stat;
        }

        private void ExecuteQuickAction(string action)
        {
            MessageBox.Show($"Thực hiện: {action}", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowSettingsMenu(Control sender)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Color.White;
            menu.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());

            menu.Items.Add("Thông tin cá nhân", null, (s, e) => ShowUserProfile());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Đổi mật khẩu", null, (s, e) => ChangePassword());
            menu.Items.Add("Thay đổi giao diện", null, (s, e) => ChangeTheme());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Đăng xuất", null, (s, e) => Logout());
            menu.Items.Add("Thoát", null, (s, e) => Application.Exit());

            menu.Show(sender, new Point(0, sender.Height));
        }

        private void ShowUserProfile()
        {
            MessageBox.Show($"Thông tin người dùng:\n\nTên: {_fullName}\nVai trò: {_userRole}\nChi nhánh: {_branch}",
                "Thông tin cá nhân");
        }

        private void ChangePassword()
        {
            MessageBox.Show("Chức năng đổi mật khẩu đang phát triển", "Thông báo");
        }

        private void ChangeTheme()
        {
            MessageBox.Show("Chức năng thay đổi giao diện đang phát triển", "Thông báo");
        }

        private void Logout()
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is frmLogin loginForm)
                    {
                        loginForm.Show();
                        break;
                    }
                }
                this.Close();
            }
        }

        // Placeholder methods for other sections
        private void LoadAdminUsers() { /* Implementation */ }
        private void LoadAdminBranches() { /* Implementation */ }
        private void LoadMovies() { /* Implementation */ }
        private void LoadShowtimes() { /* Implementation */ }
        private void LoadTicketSales() { /* Implementation */ }
        private void LoadInventory() { /* Implementation */ }
        private void LoadReports() { /* Implementation */ }
        private void LoadStaffManagement() { /* Implementation */ }
        private void LoadSettings() { /* Implementation */ }
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
        public override Color MenuItemSelected => Color.FromArgb(240, 240, 240);
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuBorder => Color.FromArgb(230, 230, 230);
        public override Color ToolStripDropDownBackground => Color.White;
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
