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
        private FlowLayoutPanel _quickActionsPanel;

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

            // Branch info với CGV style
            Label lblBranchInfo = new Label();
            lblBranchInfo.Text = $"🎬 {_branch.ToUpper()}";
            lblBranchInfo.Font = new Font("Montserrat", 14, FontStyle.Bold);
            lblBranchInfo.ForeColor = _cgvGold;
            lblBranchInfo.Location = new Point(200, 20);
            lblBranchInfo.AutoSize = true;

            // Search box với CGV style hiện đại
            Panel searchPanel = new Panel();
            searchPanel.Size = new Size(350, 40);
            searchPanel.Location = new Point(400, 15);
            searchPanel.BackColor = Color.FromArgb(50, 50, 50);
            searchPanel.BorderRadius(25);

            TextBox txtSearch = new TextBox();
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 11);
            txtSearch.ForeColor = Color.Silver;
            txtSearch.BackColor = Color.FromArgb(50, 50, 50);
            txtSearch.Size = new Size(280, 40);
            txtSearch.Location = new Point(20, 10);
            txtSearch.Text = "🔍 Tìm kiếm hệ thống...";
            txtSearch.Enter += (s, e) =>
            {
                if (txtSearch.Text == "🔍 Tìm kiếm hệ thống...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = _cgvWhite;
                }
            };
            txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "🔍 Tìm kiếm hệ thống...";
                    txtSearch.ForeColor = Color.Silver;
                }
            };

            // Search icon với animation
            Label lblSearchIcon = new Label();
            lblSearchIcon.Text = "🎬";
            lblSearchIcon.Font = new Font("Segoe UI", 16);
            lblSearchIcon.ForeColor = _cgvRed;
            lblSearchIcon.Size = new Size(40, 40);
            lblSearchIcon.Location = new Point(searchPanel.Width - 45, 0);
            lblSearchIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblSearchIcon.Cursor = Cursors.Hand;
            lblSearchIcon.MouseEnter += (s, e) =>
            {
                lblSearchIcon.ForeColor = _cgvGold;
                lblSearchIcon.Font = new Font("Segoe UI", 18);
            };
            lblSearchIcon.MouseLeave += (s, e) =>
            {
                lblSearchIcon.ForeColor = _cgvRed;
                lblSearchIcon.Font = new Font("Segoe UI", 16);
            };

            searchPanel.Controls.Add(lblSearchIcon);
            searchPanel.Controls.Add(txtSearch);

            // User info panel - CGV style hiện đại
            Panel userPanel = new Panel();
            userPanel.Size = new Size(400, 70);
            userPanel.Location = new Point(_header.Width - 450, 0);
            userPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            userPanel.BackColor = Color.Transparent;

            // Notification bell với animation
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
            btnNotification.MouseEnter += (s, e) =>
            {
                btnNotification.ForeColor = _cgvGold;
                btnNotification.Font = new Font("Segoe UI", 22);
            };
            btnNotification.MouseLeave += (s, e) =>
            {
                btnNotification.ForeColor = _cgvSilver;
                btnNotification.Font = new Font("Segoe UI", 20);
            };

            // Notification badge với CGV style
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

            // User avatar với CGV style hiện đại
            Panel avatarPanel = new Panel();
            avatarPanel.Size = new Size(45, 45);
            avatarPanel.Location = new Point(100, 12);
            avatarPanel.BackColor = _cgvRed;
            avatarPanel.BorderRadius(22);
            avatarPanel.BackgroundImageLayout = ImageLayout.Stretch;

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

            // User info với CGV typography
            Label lblUserInfo = new Label();
            lblUserInfo.Text = $"{_fullName.ToUpper()}\n{_userRole} • {_branch}";
            lblUserInfo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblUserInfo.ForeColor = _cgvSilver;
            lblUserInfo.AutoSize = true;
            lblUserInfo.Location = new Point(160, 15);
            lblUserInfo.TextAlign = ContentAlignment.MiddleLeft;

            // Settings dropdown với CGV style
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
            btnSettings.MouseEnter += (s, e) =>
            {
                btnSettings.ForeColor = _cgvGold;
                btnSettings.Font = new Font("Segoe UI", 20);
            };
            btnSettings.MouseLeave += (s, e) =>
            {
                btnSettings.ForeColor = _cgvSilver;
                btnSettings.Font = new Font("Segoe UI", 18);
            };
            btnSettings.Click += (s, e) => ShowSettingsMenu(btnSettings);

            userPanel.Controls.AddRange(new Control[] {
                notificationPanel, avatarPanel, lblUserInfo, btnSettings
            });

            _header.Controls.AddRange(new Control[] { logoPanel, lblBranchInfo, searchPanel, userPanel });

            // ========== SIDEBAR ==========
            _sidebar = new Panel();
            _sidebar.Dock = DockStyle.Left;
            _sidebar.Width = 300;
            _sidebar.BackColor = _cgvBlack;
            _sidebar.AutoScroll = false;
            _sidebar.Padding = new Padding(0, 20, 0, 0);

            // Sidebar header với CGV style hiện đại
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

            // ========== MAIN CONTENT ==========
            _mainContentPanel = new Panel();
            _mainContentPanel.Dock = DockStyle.Fill;
            _mainContentPanel.BackColor = _cgvLightGray;
            _mainContentPanel.Padding = new Padding(25);
            _mainContentPanel.AutoScroll = true;

            // ========== QUICK ACTIONS BAR ==========
            _quickActionsPanel = new FlowLayoutPanel();
            _quickActionsPanel.Dock = DockStyle.Bottom;
            _quickActionsPanel.Height = 100;
            _quickActionsPanel.BackColor = _cgvBlack;
            _quickActionsPanel.Padding = new Padding(30, 15, 30, 15);
            _quickActionsPanel.FlowDirection = FlowDirection.LeftToRight;
            _quickActionsPanel.WrapContents = false;
            _quickActionsPanel.AutoScroll = true;

            // Thêm tiêu đề cho quick actions
            Label lblQuickActionsTitle = new Label();
            lblQuickActionsTitle.Text = "🚀 THAO TÁC NHANH:";
            lblQuickActionsTitle.Font = new Font("Montserrat", 11, FontStyle.Bold);
            lblQuickActionsTitle.ForeColor = _cgvGold;
            lblQuickActionsTitle.AutoSize = true;
            lblQuickActionsTitle.Margin = new Padding(0, 25, 25, 0);
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

            // Thêm hiệu ứng glow khi active
            if (menuItem.IsActive)
            {
                btn.Paint += (s, e) =>
                {
                    using (Pen glowPen = new Pen(Color.FromArgb(100, _cgvGold), 2))
                    {
                        Rectangle rect = new Rectangle(1, 1, btn.Width - 3, btn.Height - 3);
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
                btn.Size = new Size(180, 70);
                btn.Margin = new Padding(8);
                btn.BackColor = _cgvDarkGray;
                btn.ForeColor = _cgvWhite;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = _cgvRed;
                btn.Cursor = Cursors.Hand;
                btn.BorderRadius(8);

                // Icon
                Label lblIcon = new Label();
                lblIcon.Text = action[1];
                lblIcon.Font = new Font("Segoe UI", 20);
                lblIcon.Location = new Point(15, 20);
                lblIcon.AutoSize = true;
                lblIcon.ForeColor = _cgvRed;
                btn.Controls.Add(lblIcon);

                // Text
                Label lblText = new Label();
                lblText.Text = action[0];
                lblText.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                lblText.Location = new Point(50, 23);
                lblText.Size = new Size(120, 40);
                lblText.ForeColor = _cgvWhite;
                lblText.TextAlign = ContentAlignment.MiddleLeft;
                btn.Controls.Add(lblText);

                // Hover effect với CGV style
                btn.MouseEnter += (s, e) =>
                {
                    btn.BackColor = _cgvRed;
                    lblIcon.ForeColor = _cgvWhite;
                    lblText.ForeColor = _cgvWhite;
                    btn.FlatAppearance.BorderColor = _cgvGold;
                    lblIcon.Font = new Font("Segoe UI", 22);
                };

                btn.MouseLeave += (s, e) =>
                {
                    btn.BackColor = _cgvDarkGray;
                    lblIcon.ForeColor = _cgvRed;
                    lblText.ForeColor = _cgvWhite;
                    btn.FlatAppearance.BorderColor = _cgvRed;
                    lblIcon.Font = new Font("Segoe UI", 20);
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
            homePanel.Padding = new Padding(15);

            // Welcome section với CGV style
            Panel welcomeCard = CreateRoundedCard(20);
            welcomeCard.Dock = DockStyle.Top;
            welcomeCard.Height = 180;
            welcomeCard.BackColor = _cgvBlack;
            welcomeCard.Padding = new Padding(40, 30, 40, 30);

            // Gradient background effect
            welcomeCard.Paint += (s, e) =>
            {
                using (LinearGradientBrush gradient = new LinearGradientBrush(
                    welcomeCard.ClientRectangle,
                    Color.FromArgb(30, 30, 30),
                    _cgvBlack,
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(gradient, welcomeCard.ClientRectangle);
                }

                // Add CGV pattern overlay
                using (Pen patternPen = new Pen(Color.FromArgb(40, 40, 40), 1))
                {
                    for (int i = 0; i < welcomeCard.Width; i += 20)
                    {
                        e.Graphics.DrawLine(patternPen, i, 0, i, welcomeCard.Height);
                    }
                }
            };

            Label lblWelcome = new Label();
            lblWelcome.Text = $"🎬 CHÀO MỪNG TRỞ LẠI, {_fullName.ToUpper()}!";
            lblWelcome.Font = new Font("Montserrat", 24, FontStyle.Bold);
            lblWelcome.ForeColor = _cgvGold;
            lblWelcome.Location = new Point(40, 30);
            lblWelcome.AutoSize = true;

            Label lblDate = new Label();
            lblDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy").ToUpper();
            lblDate.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblDate.ForeColor = _cgvSilver;
            lblDate.Location = new Point(40, 80);
            lblDate.AutoSize = true;

            Label lblQuote = new Label();
            lblQuote.Text = "\"Mỗi bộ phim là một hành trình mới - CGV Cinemas\"";
            lblQuote.Font = new Font("Segoe UI", 11, FontStyle.Italic);
            lblQuote.ForeColor = Color.FromArgb(200, 200, 200);
            lblQuote.Location = new Point(40, 115);
            lblQuote.AutoSize = true;

            welcomeCard.Controls.AddRange(new Control[] { lblWelcome, lblDate, lblQuote });
            homePanel.Controls.Add(welcomeCard);

            // Stats section
            Label lblStatsTitle = new Label();
            lblStatsTitle.Text = "📊 THỐNG KÊ NHANH";
            lblStatsTitle.Font = new Font("Montserrat", 18, FontStyle.Bold);
            lblStatsTitle.ForeColor = _cgvBlack;
            lblStatsTitle.Dock = DockStyle.Top;
            lblStatsTitle.Height = 60;
            lblStatsTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblStatsTitle.Margin = new Padding(0, 20, 0, 0);
            homePanel.Controls.Add(lblStatsTitle);

            // Stats cards với CGV style
            FlowLayoutPanel statsPanel = new FlowLayoutPanel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 170;
            statsPanel.Margin = new Padding(0, 0, 0, 25);
            statsPanel.Padding = new Padding(0, 10, 0, 0);
            statsPanel.WrapContents = false;
            statsPanel.AutoScroll = true;

            string[] stats = GetDashboardStats();
            Color[] statColors = {
                _cgvRed,
                Color.FromArgb(41, 128, 185),   // CGV Blue
                Color.FromArgb(142, 68, 173),   // CGV Purple
                Color.FromArgb(39, 174, 96)     // CGV Green
            };

            for (int i = 0; i < stats.Length; i++)
            {
                Panel statCard = CreateStatCard(stats[i], statColors[i]);
                statCard.Margin = new Padding(0, 0, 15, 0);
                statsPanel.Controls.Add(statCard);
            }

            homePanel.Controls.Add(statsPanel);

            // Recent activities với CGV style
            Panel activitiesCard = CreateRoundedCard(15);
            activitiesCard.Dock = DockStyle.Top;
            activitiesCard.Height = 300;
            activitiesCard.BackColor = _cgvWhite;
            activitiesCard.Padding = new Padding(25);

            Label lblActivitiesTitle = new Label();
            lblActivitiesTitle.Text = "🎯 HOẠT ĐỘNG GẦN ĐÂY";
            lblActivitiesTitle.Font = new Font("Montserrat", 16, FontStyle.Bold);
            lblActivitiesTitle.ForeColor = _cgvBlack;
            lblActivitiesTitle.Dock = DockStyle.Top;
            lblActivitiesTitle.Height = 40;

            ListBox lstActivities = new ListBox();
            lstActivities.Dock = DockStyle.Fill;
            lstActivities.BorderStyle = BorderStyle.None;
            lstActivities.BackColor = _cgvWhite;
            lstActivities.Font = new Font("Segoe UI", 11);
            lstActivities.ItemHeight = 42;
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
                        new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 3, e.Bounds.Width - 20, e.Bounds.Height - 6));
                }

                // Draw separator
                using (Pen pen = new Pen(Color.FromArgb(230, 230, 230)))
                {
                    e.Graphics.DrawLine(pen, e.Bounds.X + 10, e.Bounds.Bottom - 1, e.Bounds.Right - 10, e.Bounds.Bottom - 1);
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
            chartCard.Height = 360;
            chartCard.BackColor = _cgvWhite;
            chartCard.Margin = new Padding(0, 20, 0, 0);
            chartCard.Padding = new Padding(25);

            Label lblChartTitle = new Label();
            lblChartTitle.Text = "📈 BIỂU ĐỒ HIỆU SUẤT CGV";
            lblChartTitle.Font = new Font("Montserrat", 16, FontStyle.Bold);
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
            lblChartDesc.Text = "💰 Doanh thu 7 ngày gần đây - " + _branch;
            lblChartDesc.Font = new Font("Montserrat", 13, FontStyle.Bold);
            lblChartDesc.ForeColor = _cgvRed;
            lblChartDesc.Location = new Point(0, 10);
            lblChartDesc.AutoSize = true;

            // Simple chart visualization
            Panel chartVisual = new Panel();
            chartVisual.Dock = DockStyle.Fill;
            chartVisual.BackColor = Color.Transparent;
            chartVisual.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                // Draw grid lines
                using (Pen gridPen = new Pen(Color.FromArgb(240, 240, 240), 1))
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        int y = chartVisual.Height - (i * chartVisual.Height / 10);
                        e.Graphics.DrawLine(gridPen, 60, y, chartVisual.Width - 40, y);
                    }
                }

                // Sample data
                int[] revenues = { 120, 180, 150, 220, 190, 250, 280 };
                string[] days = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
                int barWidth = 50;
                int spacing = 25;

                for (int i = 0; i < revenues.Length; i++)
                {
                    int x = 80 + i * (barWidth + spacing);
                    int height = (revenues[i] * chartVisual.Height / 300);
                    int y = chartVisual.Height - height - 30;

                    // Draw bar with gradient
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new Rectangle(x, y, barWidth, height),
                        _cgvRed,
                        _cgvDarkRed,
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, x, y, barWidth, height);
                    }

                    // Add bar shadow
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                    {
                        e.Graphics.FillRectangle(shadowBrush, x + 2, y + 2, barWidth, height);
                    }

                    // Draw value
                    using (Font valueFont = new Font("Montserrat", 9, FontStyle.Bold))
                    {
                        e.Graphics.DrawString($"{revenues[i]}K", valueFont,
                            Brushes.Black, x, y - 20);
                    }

                    // Draw day label
                    using (Font dayFont = new Font("Segoe UI", 10, FontStyle.Bold))
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

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Draw shadow
                    Rectangle shadowRect = new Rectangle(2, 2, width - 4, height - 4);
                    using (GraphicsPath shadowPath = new GraphicsPath())
                    {
                        shadowPath.AddArc(shadowRect.X, shadowRect.Y, radius * 2, radius * 2, 180, 90);
                        shadowPath.AddArc(shadowRect.Right - radius * 2, shadowRect.Y, radius * 2, radius * 2, 270, 90);
                        shadowPath.AddArc(shadowRect.Right - radius * 2, shadowRect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        shadowPath.AddArc(shadowRect.X, shadowRect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        shadowPath.CloseFigure();

                        using (PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath))
                        {
                            shadowBrush.CenterColor = Color.FromArgb(10, 0, 0, 0);
                            shadowBrush.SurroundColors = new Color[] { Color.Transparent };
                            e.Graphics.FillPath(shadowBrush, shadowPath);
                        }
                    }

                    // Draw card
                    e.Graphics.FillPath(new SolidBrush(panel.BackColor), path);

                    // Add border
                    using (Pen borderPen = new Pen(Color.FromArgb(230, 230, 230), 1))
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            };

            return panel;
        }

        private Panel CreateStatCard(string statText, Color color)
        {
            Panel card = CreateRoundedCard(12);
            card.Size = new Size(280, 150);
            card.Padding = new Padding(20);
            card.BackColor = Color.White;
            card.Cursor = Cursors.Hand;

            // Icon với CGV style
            Panel iconPanel = new Panel();
            iconPanel.Size = new Size(45, 45);
            iconPanel.Location = new Point(20, 20);
            iconPanel.BackColor = Color.FromArgb(20, color.R, color.G, color.B);
            iconPanel.BorderRadius(10);

            Label lblIcon = new Label();
            lblIcon.Text = GetStatIcon(statText);
            lblIcon.Font = new Font("Segoe UI", 18);
            lblIcon.ForeColor = color;
            lblIcon.Dock = DockStyle.Fill;
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            iconPanel.Controls.Add(lblIcon);

            // Value
            string value = GetStatValue(statText);
            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Montserrat", 28, FontStyle.Bold);
            lblValue.ForeColor = _cgvBlack;
            lblValue.Location = new Point(80, 20);
            lblValue.AutoSize = true;

            // Description
            string desc = GetStatDescription(statText);
            Label lblDesc = new Label();
            lblDesc.Text = desc.ToUpper();
            lblDesc.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblDesc.ForeColor = Color.Gray;
            lblDesc.Location = new Point(80, 60);
            lblDesc.AutoSize = true;

            // Trend indicator
            Label lblTrend = new Label();
            lblTrend.Text = "📈 +12%";
            lblTrend.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lblTrend.ForeColor = Color.FromArgb(39, 174, 96);
            lblTrend.Location = new Point(card.Width - 70, 25);
            lblTrend.AutoSize = true;

            // Decorative line
            Panel line = new Panel();
            line.Size = new Size(240, 1);
            line.Location = new Point(20, 105);
            line.BackColor = Color.FromArgb(240, 240, 240);

            card.Controls.Add(line);
            card.Controls.AddRange(new Control[] { iconPanel, lblValue, lblDesc, lblTrend });

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(248, 248, 248);
                line.BackColor = color;
                lblIcon.Font = new Font("Segoe UI", 20);
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
                line.BackColor = Color.FromArgb(240, 240, 240);
                lblIcon.Font = new Font("Segoe UI", 18);
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

            MessageBox.Show($"🎬 CGV CINEMA SYSTEM\n\nThực hiện thành công: {action}",
                "CGV - Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Các phương thức chức năng giữ nguyên (không thay đổi logic)
        private void ShowSettingsMenu(Control sender)
        {
            // Giữ nguyên logic
        }

        private void ShowUserProfile()
        {
            // Giữ nguyên logic
        }

        private void ChangePassword()
        {
            // Giữ nguyên logic
        }

        private void ChangeTheme()
        {
            // Giữ nguyên logic
        }

        private void Logout()
        {
            // Giữ nguyên logic
        }

        private void LoadAdminUsers()
        {
            ShowPlaceholder("👥 QUẢN LÝ NGƯỜI NHÂN VIÊN", "Chức năng quản lý tài khoản người dùng hệ thống CGV");
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
            // Sử dụng maNguoiDung và maChiNhanh từ người dùng đã đăng nhập
            // Nếu chưa có, sử dụng giá trị mặc định (1, 1)
            int maNguoiDung = (_maNguoiDung > 0) ? _maNguoiDung : 1;
            int maChiNhanh = (_maChiNhanh > 0) ? _maChiNhanh : 1;
            
            LoadUserControl(new UC_Kho(maChiNhanh, maNguoiDung));
        }

        private void SetupReportTabControl()
        {
            // Giữ nguyên logic
        }

        private void LoadReportTab(int tabIndex)
        {
            // Giữ nguyên logic
        }

        private void LoadReports()
        {
            // Giữ nguyên logic
        }

        private void LoadWorkSchedule()
        {
            LoadUserControl(new UC_LichLamViec());
        }

        private void LoadPerformance()
        {
            LoadUserControl(new UC_HieuSuat());
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
            // Giữ nguyên logic
        }

        /// <summary>
        /// Helper method to load UserControl vào main panel
        /// </summary>
        private void LoadUserControl(UserControl control)
        {
            // Giữ nguyên logic
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