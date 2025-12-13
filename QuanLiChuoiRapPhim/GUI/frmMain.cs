using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class frmMain : Form
    {
        private string _username;
        private string _userRole;
        private string _branch;
        private int _branchId = 1;
        private int _userId = 1;

        private Panel _sidebarPanel;
        private Panel _mainContentPanel;
        private Panel _headerPanel;
        private MenuStrip _mainMenu;
        private Panel _containerPanel;
        private SplitContainer _splitContainer;

        public frmMain(string username, string userRole, string branch)
            : this(username, userRole, branch, 1, 1)
        {
        }

        public frmMain(string username, string userRole, string branch, int branchId, int userId)
        {
            InitializeComponent();
            _username = username;
            _userRole = userRole;
            _branch = branch;
            _branchId = branchId;
            _userId = userId;
            SetupMainForm();
        }

        private void SetupMainForm()
        {
            this.Text = $"Quản Lý Rạp Phim - {_username} ({_userRole})";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;

            // Tạo layout chính với TableLayoutPanel
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.ColumnCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Menu
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Nội dung
            mainLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            // Menu strip
            _mainMenu = CreateMainMenu();
            mainLayout.Controls.Add(_mainMenu, 0, 0);

            // Header panel
            _headerPanel = CreateHeaderPanel();
            mainLayout.Controls.Add(_headerPanel, 0, 1);

            // Container panel
            _containerPanel = new Panel();
            _containerPanel.Dock = DockStyle.Fill;
            _containerPanel.BackColor = Color.White;
            mainLayout.Controls.Add(_containerPanel, 0, 2);

            this.Controls.Add(mainLayout);
            SetupContentContainer();
            LoadHome();
        }

        private void SetupContentContainer()
        {
            _containerPanel.Controls.Clear();

            // Tạo SplitContainer
            _splitContainer = new SplitContainer();
            _splitContainer.Dock = DockStyle.Fill;
            _splitContainer.Orientation = Orientation.Horizontal;
            _splitContainer.FixedPanel = FixedPanel.Panel1;
            _splitContainer.SplitterWidth = 1;
            _splitContainer.Panel1MinSize = 220;
            _splitContainer.Panel1MaxSize = 220;
            _splitContainer.SplitterDistance = 220;
            _splitContainer.Panel2MinSize = 400;

            // Sidebar
            _sidebarPanel = CreateSidebar();
            _sidebarPanel.Dock = DockStyle.Fill;
            _splitContainer.Panel1.Controls.Add(_sidebarPanel);

            // Main content
            _mainContentPanel = new Panel();
            _mainContentPanel.Dock = DockStyle.Fill;
            _mainContentPanel.BackColor = Color.White;
            _mainContentPanel.AutoScroll = true;
            _mainContentPanel.Padding = new Padding(20);
            _splitContainer.Panel2.Controls.Add(_mainContentPanel);

            _containerPanel.Controls.Add(_splitContainer);
        }

        private MenuStrip CreateMainMenu()
        {
            MenuStrip mainMenu = new MenuStrip();
            mainMenu.Font = new Font("Segoe UI", 9);
            mainMenu.Dock = DockStyle.Fill;
            mainMenu.BackColor = Color.FromArgb(240, 240, 240);

            ToolStripMenuItem systemMenu = new ToolStripMenuItem("Hệ thống");
            systemMenu.DropDownItems.Add("Thông tin tài khoản", null, (s, e) => ShowUserInfo());
            systemMenu.DropDownItems.Add("Đổi mật khẩu", null, (s, e) => MessageBox.Show("Đang phát triển"));
            systemMenu.DropDownItems.Add(new ToolStripSeparator());
            systemMenu.DropDownItems.Add("Đăng xuất", null, (s, e) => Logout());
            systemMenu.DropDownItems.Add(new ToolStripSeparator());
            systemMenu.DropDownItems.Add("Thoát", null, (s, e) => Application.Exit());

            // Thêm menu Trợ giúp
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Trợ giúp");
            helpMenu.DropDownItems.Add("Hướng dẫn sử dụng", null, (s, e) => MessageBox.Show("Đang phát triển"));
            helpMenu.DropDownItems.Add("Giới thiệu phần mềm", null, (s, e) => MessageBox.Show("Phần mềm Quản lý Rạp Phim v1.0"));

            mainMenu.Items.Add(systemMenu);
            mainMenu.Items.Add(helpMenu);
            return mainMenu;
        }

        private Panel CreateHeaderPanel()
        {
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(0, 170, 255);
            headerPanel.Padding = new Padding(20, 0, 20, 0);

            Label lblWelcome = new Label();
            lblWelcome.Text = $"CHÀO MỪNG: {_username.ToUpper()} | VAI TRÒ: {_userRole.ToUpper()} | CHI NHÁNH: {_branch.ToUpper()}";
            lblWelcome.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Dock = DockStyle.Fill;
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;

            // Thêm ngày giờ hiện tại
            Label lblDateTime = new Label();
            lblDateTime.Font = new Font("Segoe UI", 10);
            lblDateTime.ForeColor = Color.White;
            lblDateTime.Dock = DockStyle.Right;
            lblDateTime.TextAlign = ContentAlignment.MiddleRight;
            lblDateTime.AutoSize = true;
            lblDateTime.Padding = new Padding(0, 0, 10, 0);

            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => lblDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            timer.Start();

            headerPanel.Controls.Add(lblDateTime);
            headerPanel.Controls.Add(lblWelcome);
            return headerPanel;
        }

        private Panel CreateSidebar()
        {
            Panel sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Fill;
            sidebarPanel.BackColor = Color.FromArgb(45, 45, 48);

            // Panel chứa nội dung sidebar có thể cuộn
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(45, 45, 48);
            contentPanel.AutoScroll = true;
            contentPanel.Padding = new Padding(10, 10, 10, 10);

            FlowLayoutPanel flowLayout = new FlowLayoutPanel();
            flowLayout.Dock = DockStyle.Fill;
            flowLayout.FlowDirection = FlowDirection.TopDown;
            flowLayout.WrapContents = false;
            flowLayout.AutoSize = true;
            flowLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Menu title
            Label lblTitle = new Label();
            lblTitle.Text = "MENU CHÍNH";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Size = new Size(200, 40);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            flowLayout.Controls.Add(lblTitle);

            // Home button
            Button btnHome = CreateSidebarButton("🏠 TRANG CHỦ");
            btnHome.Click += (s, e) => LoadHome();
            flowLayout.Controls.Add(btnHome);

            // Thêm các nút theo role
            string normalizedRole = _userRole.ToLower().Trim();
            if (normalizedRole.Contains("admin"))
            {
                AddRoleButtons(flowLayout, new string[]
                {
                    "👥 QUẢN LÝ NGƯỜI DÙNG",
                    "🏢 QUẢN LÝ CHI NHÁNH",
                    "🎬 QUẢN LÝ PHIM",
                    "📊 BÁO CÁO DOANH THU"
                });
            }
            else if (normalizedRole.Contains("quan") || normalizedRole.Contains("quản"))
            {
                AddRoleButtons(flowLayout, new string[]
                {
                    "👨‍💼 QUẢN LÝ NHÂN SỰ",
                    "📅 QUẢN LÝ LỊCH LÀM",
                    "📋 DUYỆT ĐƠN XIN NGHỈ",
                    "⭐ ĐÁNH GIÁ HIỆU SUẤT",
                    "📦 QUẢN LÝ KHO",
                    "📈 BÁO CÁO CHI NHÁNH"
                });
            }
            else if (normalizedRole.Contains("nhan") || normalizedRole.Contains("nhân"))
            {
                AddRoleButtons(flowLayout, new string[]
                {
                    "🎟️ BÁN VÉ",
                    "🍿 BÁN BẮP NƯỚC",
                    "👥 QUẢN LÝ KHÁCH HÀNG",
                    "📋 BÁO CÁO GIAO CA"
                });
            }

            contentPanel.Controls.Add(flowLayout);
            sidebarPanel.Controls.Add(contentPanel);

            // Panel chứa nút logout ở dưới cùng
            Panel logoutPanel = new Panel();
            logoutPanel.Dock = DockStyle.Bottom;
            logoutPanel.Height = 60;
            logoutPanel.BackColor = Color.FromArgb(35, 35, 38);
            logoutPanel.Padding = new Padding(10);

            Button btnLogout = new Button();
            btnLogout.Text = "🚪 ĐĂNG XUẤT";
            btnLogout.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogout.Dock = DockStyle.Fill;
            btnLogout.BackColor = Color.FromArgb(220, 53, 69);
            btnLogout.ForeColor = Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Click += (s, e) => Logout();

            logoutPanel.Controls.Add(btnLogout);
            sidebarPanel.Controls.Add(logoutPanel);

            return sidebarPanel;
        }

        private void AddRoleButtons(FlowLayoutPanel flowLayout, string[] buttonTexts)
        {
            foreach (string text in buttonTexts)
            {
                Button btn = CreateSidebarButton(text);

                // Gán event handler
                if (text.Contains("NGƯỜI DÙNG")) btn.Click += (s, e) => LoadAdminUsers();
                else if (text.Contains("CHI NHÁNH")) btn.Click += (s, e) => LoadAdminBranch();
                else if (text.Contains("PHIM")) btn.Click += (s, e) => LoadAdminMovie();
                else if (text.Contains("DOANH THU")) btn.Click += (s, e) => LoadAdminReport();
                else if (text.Contains("NHÂN SỰ")) btn.Click += (s, e) => LoadManagerStaff();
                else if (text.Contains("LỊCH LÀM")) btn.Click += (s, e) => LoadManagerSchedule();
                else if (text.Contains("ĐƠN XIN NGHỈ")) btn.Click += (s, e) => LoadManagerLeaveRequest();
                else if (text.Contains("HIỆU SUẤT")) btn.Click += (s, e) => LoadManagerPerformance();
                else if (text.Contains("KHO")) btn.Click += (s, e) => LoadManagerWarehouse();
                else if (text.Contains("BÁO CÁO CHI NHÁNH")) btn.Click += (s, e) => LoadManagerReport();
                else if (text.Contains("BÁN VÉ")) btn.Click += (s, e) => LoadStaffTicket();
                else if (text.Contains("BẮP NƯỚC")) btn.Click += (s, e) => LoadStaffCanteen();
                else if (text.Contains("KHÁCH HÀNG")) btn.Click += (s, e) => LoadStaffCustomer();
                else if (text.Contains("GIAO CA")) btn.Click += (s, e) => LoadStaffReport();

                flowLayout.Controls.Add(btn);
            }
        }

        private Button CreateSidebarButton(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            btn.Size = new Size(200, 45);
            btn.Margin = new Padding(0, 5, 0, 5);
            btn.BackColor = Color.FromArgb(80, 80, 85);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 100, 105);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(120, 120, 125);
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(10, 0, 0, 0);

            return btn;
        }

        // ==================== ADMIN MENU ====================
        private void LoadAdminUsers()
        {
            LoadUserControl(new UC_Admin());
        }

        private void LoadAdminBranch()
        {
            LoadComingSoon("QUẢN LÝ CHI NHÁNH");
        }

        private void LoadAdminMovie()
        {
            LoadUserControl(new UC_Movies());
        }

        private void LoadAdminReport()
        {
            LoadUserControl(new UC_Reports());
        }

        // ==================== MANAGER MENU ====================
        private void LoadManagerStaff()
        {
            LoadUserControl(new UC_NhanSu());
        }

        private void LoadManagerSchedule()
        {
            LoadUserControl(new UC_LichLamViec());
        }

        private void LoadManagerLeaveRequest()
        {
            LoadUserControl(new UC_DonXinNghi(_branchId, _userId));
        }

        private void LoadManagerPerformance()
        {
            LoadUserControl(new UC_HieuSuat());
        }

        private void LoadManagerWarehouse()
        {
            LoadUserControl(new UC_Kho(_branchId, _userId));
        }

        private void LoadManagerReport()
        {
            LoadComingSoon("BÁO CÁO CHI NHÁNH");
        }

        // ==================== STAFF MENU ====================
        private void LoadStaffTicket()
        {
            LoadComingSoon("BÁN VÉ");
        }

        private void LoadStaffCanteen()
        {
            LoadComingSoon("BÁN BẮP NƯỚC");
        }

        private void LoadStaffCustomer()
        {
            LoadComingSoon("QUẢN LÝ KHÁCH HÀNG");
        }

        private void LoadStaffReport()
        {
            LoadComingSoon("BÁO CÁO GIAO CA");
        }

        // ==================== COMMON METHODS ====================
        private void LoadUserControl(UserControl userControl)
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();
            userControl.Dock = DockStyle.Fill;
            _mainContentPanel.Controls.Add(userControl);
            _mainContentPanel.ResumeLayout();
        }

        private void LoadHome()
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            Panel homePanel = new Panel();
            homePanel.Dock = DockStyle.Fill;
            homePanel.BackColor = Color.White;
            homePanel.AutoScroll = true;
            homePanel.Padding = new Padding(20);

            // Welcome message
            Label lblWelcome = new Label();
            lblWelcome.Text = $"Chào mừng {_username.ToUpper()} đến hệ thống Quản lý Rạp Phim!";
            lblWelcome.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(0, 170, 255);
            lblWelcome.Size = new Size(800, 60);
            lblWelcome.Location = new Point(20, 20);
            lblWelcome.TextAlign = ContentAlignment.MiddleLeft;
            homePanel.Controls.Add(lblWelcome);

            // Role and branch info
            Label lblRole = new Label();
            lblRole.Text = $"👤 Vai trò: {_userRole} | 🏢 Chi nhánh: {_branch}";
            lblRole.Font = new Font("Segoe UI", 14);
            lblRole.ForeColor = Color.Gray;
            lblRole.Size = new Size(800, 30);
            lblRole.Location = new Point(20, 100);
            lblRole.TextAlign = ContentAlignment.MiddleLeft;
            homePanel.Controls.Add(lblRole);

            // Stats panel
            Panel statsPanel = new Panel();
            statsPanel.Size = new Size(homePanel.Width - 40, 200);
            statsPanel.Location = new Point(20, 150);
            statsPanel.BackColor = Color.FromArgb(248, 249, 250);
            statsPanel.BorderStyle = BorderStyle.FixedSingle;

            Label lblStatsTitle = new Label();
            lblStatsTitle.Text = "📊 THỐNG KÊ NHANH";
            lblStatsTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblStatsTitle.ForeColor = Color.FromArgb(0, 170, 255);
            lblStatsTitle.Size = new Size(300, 40);
            lblStatsTitle.Location = new Point(20, 20);
            statsPanel.Controls.Add(lblStatsTitle);

            int yPos = 80;
            string[] statsItems = GetRoleSpecificStats();

            foreach (string stat in statsItems)
            {
                Label lblStat = new Label();
                lblStat.Text = $"• {stat}";
                lblStat.Font = new Font("Segoe UI", 12);
                lblStat.ForeColor = Color.FromArgb(80, 80, 80);
                lblStat.Size = new Size(statsPanel.Width - 40, 25);
                lblStat.Location = new Point(20, yPos);
                statsPanel.Controls.Add(lblStat);
                yPos += 30;
            }

            homePanel.Controls.Add(statsPanel);

            // Quick actions
            Label lblActions = new Label();
            lblActions.Text = "🚀 HÀNH ĐỘNG NHANH";
            lblActions.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblActions.ForeColor = Color.FromArgb(0, 170, 255);
            lblActions.Size = new Size(300, 40);
            lblActions.Location = new Point(20, 370);
            homePanel.Controls.Add(lblActions);

            // Quick action buttons
            FlowLayoutPanel actionPanel = new FlowLayoutPanel();
            actionPanel.Size = new Size(homePanel.Width - 40, 100);
            actionPanel.Location = new Point(20, 420);
            actionPanel.FlowDirection = FlowDirection.LeftToRight;
            actionPanel.WrapContents = true;

            string[] quickActions = GetRoleSpecificActions();
            foreach (string action in quickActions)
            {
                Button btnAction = new Button();
                btnAction.Text = action;
                btnAction.Font = new Font("Segoe UI", 10);
                btnAction.Size = new Size(150, 40);
                btnAction.Margin = new Padding(5);
                btnAction.BackColor = Color.FromArgb(0, 170, 255);
                btnAction.ForeColor = Color.White;
                btnAction.FlatStyle = FlatStyle.Flat;
                btnAction.FlatAppearance.BorderSize = 0;
                btnAction.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 150, 235);
                btnAction.Cursor = Cursors.Hand;

                // Gán sự kiện
                if (action.Contains("Người dùng")) btnAction.Click += (s, e) => LoadAdminUsers();
                else if (action.Contains("Phim")) btnAction.Click += (s, e) => LoadAdminMovie();
                else if (action.Contains("Nhân sự")) btnAction.Click += (s, e) => LoadManagerStaff();
                else if (action.Contains("Báo cáo")) btnAction.Click += (s, e) => LoadAdminReport();
                else if (action.Contains("Kho")) btnAction.Click += (s, e) => LoadManagerWarehouse();
                else if (action.Contains("Lịch làm")) btnAction.Click += (s, e) => LoadManagerSchedule();

                actionPanel.Controls.Add(btnAction);
            }

            homePanel.Controls.Add(actionPanel);
            _mainContentPanel.Controls.Add(homePanel);
            _mainContentPanel.ResumeLayout();
        }

        private string[] GetRoleSpecificStats()
        {
            string normalizedRole = _userRole.ToLower().Trim();

            if (normalizedRole.Contains("admin"))
            {
                return new string[]
                {
                    "Tổng chi nhánh: 3",
                    "Tổng người dùng: 10",
                    "Tổng phim đang chiếu: 8",
                    "Doanh thu hôm nay: 15,000,000đ"
                };
            }
            else if (normalizedRole.Contains("quan") || normalizedRole.Contains("quản"))
            {
                return new string[]
                {
                    "Nhân viên đang làm: 6",
                    "Đơn xin nghỉ chờ: 2",
                    "Tồn kho báo động: 3 sản phẩm",
                    "Doanh thu tháng: 250,000,000đ"
                };
            }
            else if (normalizedRole.Contains("nhan") || normalizedRole.Contains("nhân"))
            {
                return new string[]
                {
                    "Vé đã bán hôm nay: 45",
                    "Doanh thu ca: 3,500,000đ",
                    "Khách hàng mới: 5",
                    "Sản phẩm bán chạy: Combo Đôi"
                };
            }

            return new string[] { "Không có thống kê" };
        }

        private string[] GetRoleSpecificActions()
        {
            string normalizedRole = _userRole.ToLower().Trim();

            if (normalizedRole.Contains("admin"))
            {
                return new string[]
                {
                    "Quản lý người dùng",
                    "Quản lý phim",
                    "Xem báo cáo",
                    "Cấu hình hệ thống"
                };
            }
            else if (normalizedRole.Contains("quan") || normalizedRole.Contains("quản"))
            {
                return new string[]
                {
                    "Quản lý nhân sự",
                    "Phân công lịch làm",
                    "Duyệt đơn nghỉ",
                    "Quản lý kho"
                };
            }
            else if (normalizedRole.Contains("nhan") || normalizedRole.Contains("nhân"))
            {
                return new string[]
                {
                    "Bán vé",
                    "Bán bắp nước",
                    "Quản lý khách",
                    "Báo cáo giao ca"
                };
            }

            return new string[] { "Về trang chủ" };
        }

        private void LoadComingSoon(string featureName)
        {
            _mainContentPanel.SuspendLayout();
            _mainContentPanel.Controls.Clear();

            Panel comingSoonPanel = new Panel();
            comingSoonPanel.Dock = DockStyle.Fill;
            comingSoonPanel.BackColor = Color.White;
            comingSoonPanel.Padding = new Padding(20);

            Label lblIcon = new Label();
            lblIcon.Text = "🚧";
            lblIcon.Font = new Font("Segoe UI", 72);
            lblIcon.Size = new Size(200, 100);
            lblIcon.Location = new Point(
                (comingSoonPanel.ClientSize.Width - lblIcon.Width) / 2,
                100
            );
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            comingSoonPanel.Controls.Add(lblIcon);

            Label lblTitle = new Label();
            lblTitle.Text = featureName;
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 170, 255);
            lblTitle.Size = new Size(600, 50);
            lblTitle.Location = new Point(
                (comingSoonPanel.ClientSize.Width - lblTitle.Width) / 2,
                220
            );
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            comingSoonPanel.Controls.Add(lblTitle);

            Label lblMessage = new Label();
            lblMessage.Text = "Chức năng đang được phát triển. Vui lòng quay lại sau!";
            lblMessage.Font = new Font("Segoe UI", 14);
            lblMessage.ForeColor = Color.Gray;
            lblMessage.Size = new Size(600, 30);
            lblMessage.Location = new Point(
                (comingSoonPanel.ClientSize.Width - lblMessage.Width) / 2,
                290
            );
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            comingSoonPanel.Controls.Add(lblMessage);

            Button btnBack = new Button();
            btnBack.Text = "← Quay lại trang chủ";
            btnBack.Font = new Font("Segoe UI", 12);
            btnBack.Size = new Size(200, 40);
            btnBack.Location = new Point(
                (comingSoonPanel.ClientSize.Width - btnBack.Width) / 2,
                350
            );
            btnBack.BackColor = Color.FromArgb(0, 170, 255);
            btnBack.ForeColor = Color.White;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 150, 235);
            btnBack.Cursor = Cursors.Hand;
            btnBack.Click += (s, e) => LoadHome();
            comingSoonPanel.Controls.Add(btnBack);

            _mainContentPanel.Controls.Add(comingSoonPanel);
            _mainContentPanel.ResumeLayout();
        }

        private void ShowUserInfo()
        {
            MessageBox.Show($"Thông tin tài khoản:\n\n" +
                          $"👤 Tên đăng nhập: {_username}\n" +
                          $"🎯 Vai trò: {_userRole}\n" +
                          $"🏢 Chi nhánh: {_branch}\n" +
                          $"🆔 Mã chi nhánh: {_branchId}\n" +
                          $"🆔 Mã người dùng: {_userId}",
                          "Thông tin tài khoản",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
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
    }
}