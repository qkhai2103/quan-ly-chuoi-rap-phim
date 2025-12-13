using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class frmMain : Form
    {
        private string _username;
        private string _userRole;
        private string _branch;
        private Panel _mainContentPanel;

        public frmMain(string username, string userRole, string branch)
        {
            InitializeComponent();

            _username = username;
            _userRole = userRole;
            _branch = branch;

            SetupMainForm();
        }

        private void SetupMainForm()
        {
            this.Text = $"Quản Lý Rạp Phim - {_username} ({_userRole})";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;

            // Menu strip
            MenuStrip mainMenu = CreateMainMenu();
            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // Header panel
            Panel headerPanel = CreateHeaderPanel();
            this.Controls.Add(headerPanel);

            // Container panel
            Panel containerPanel = new Panel();
            containerPanel.Dock = DockStyle.Fill;
            containerPanel.BackColor = Color.White;

            // Sidebar
            Panel sidebarPanel = CreateSidebar();
            sidebarPanel.Dock = DockStyle.Left;
            containerPanel.Controls.Add(sidebarPanel);

            // Main content
            _mainContentPanel = new Panel();
            _mainContentPanel.Dock = DockStyle.Fill;
            _mainContentPanel.BackColor = Color.White;
            containerPanel.Controls.Add(_mainContentPanel);

            this.Controls.Add(containerPanel);
            this.Controls.SetChildIndex(mainMenu, 0);

            LoadHome();
        }

        private MenuStrip CreateMainMenu()
        {
            MenuStrip mainMenu = new MenuStrip();
            mainMenu.Font = new Font("Segoe UI", 10);

            // System menu
            ToolStripMenuItem systemMenu = new ToolStripMenuItem("Hệ thống");
            systemMenu.DropDownItems.Add("Thông tin tài khoản", null, (s, e) => ShowUserInfo());
            systemMenu.DropDownItems.Add("Đổi mật khẩu", null, (s, e) => MessageBox.Show("Đang phát triển"));
            systemMenu.DropDownItems.Add(new ToolStripSeparator());
            systemMenu.DropDownItems.Add("Đăng xuất", null, (s, e) => Logout());
            systemMenu.DropDownItems.Add(new ToolStripSeparator());
            systemMenu.DropDownItems.Add("Thoát", null, (s, e) => Application.Exit());

            mainMenu.Items.Add(systemMenu);
            return mainMenu;
        }

        private Panel CreateHeaderPanel()
        {
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(0, 170, 255);

            Label lblWelcome = new Label();
            lblWelcome.Text = $"CHÀO MỪNG: {_username.ToUpper()} | VAI TRÒ: {_userRole.ToUpper()} | CHI NHÁNH: {_branch.ToUpper()}";
            lblWelcome.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Dock = DockStyle.Fill;
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;

            headerPanel.Controls.Add(lblWelcome);
            return headerPanel;
        }

        private Panel CreateSidebar()
        {
            Panel sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 220;
            sidebarPanel.BackColor = Color.FromArgb(45, 45, 48);
            sidebarPanel.Padding = new Padding(0, 0, 0, 0);  // Remove padding, handle manually
            sidebarPanel.AutoScroll = true;

            int yPos = 10;

            // Menu title
            Label lblTitle = new Label();
            lblTitle.Text = "MENU";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(10, yPos);
            lblTitle.Size = new Size(200, 40);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            sidebarPanel.Controls.Add(lblTitle);
            yPos += 50;

            // Home button
            AddSidebarButton(sidebarPanel, "TRANG CHỦ", yPos, (s, e) => LoadHome());
            yPos += 55;

            // Build menu based on role
            // Normalize role for comparison (remove diacritics)
            string normalizedRole = _userRole.ToLower().Trim();

            // Debug - show what role we got
            MessageBox.Show($"Role nhận được: '{_userRole}'\nNormalized: '{normalizedRole}'", "Debug");

            if (normalizedRole.Contains("admin"))
            {
                AddSidebarButton(sidebarPanel, "QUẢN LÝ NGƯỜI DÙNG", yPos, (s, e) => LoadAdminUsers());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "QUẢN LÝ CHI NHÁNH", yPos, (s, e) => LoadAdminBranch());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "QUẢN LÝ PHIM", yPos, (s, e) => LoadAdminMovie());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "BÁO CÁO DOANH THU", yPos, (s, e) => LoadAdminReport());
                yPos += 55;
            }
            else if (normalizedRole.Contains("quan") || normalizedRole.Contains("quản"))
            {
                AddSidebarButton(sidebarPanel, "QUẢN LÝ NHÂN SỰ", yPos, (s, e) => LoadManagerStaff());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "QUẢN LÝ KHO", yPos, (s, e) => LoadManagerWarehouse());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "QUẢN LÝ LỊCH CHIẾU", yPos, (s, e) => LoadManagerSchedule());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "BÁO CÁO CHI NHÁNH", yPos, (s, e) => LoadManagerReport());
                yPos += 55;
            }
            else if (normalizedRole.Contains("nhan") || normalizedRole.Contains("nhân"))
            {
                AddSidebarButton(sidebarPanel, "BÁN VÉ", yPos, (s, e) => LoadStaffTicket());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "BÁN BẮP NƯỚC", yPos, (s, e) => LoadStaffCanteen());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "QUẢN LÝ KHÁCH HÀNG", yPos, (s, e) => LoadStaffCustomer());
                yPos += 55;
                AddSidebarButton(sidebarPanel, "BÁO CÁO GIAO CA", yPos, (s, e) => LoadStaffReport());
                yPos += 55;
            }

            // Separator
            Panel separator = new Panel();
            separator.BackColor = Color.FromArgb(100, 100, 100);
            separator.Height = 1;
            separator.Width = 200;
            separator.Location = new Point(10, yPos);
            sidebarPanel.Controls.Add(separator);

            // Logout button (fixed at bottom)
            Button btnLogout = new Button();
            btnLogout.Text = "ĐĂNG XUẤT";
            btnLogout.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogout.Location = new Point(10, sidebarPanel.Height - 60);
            btnLogout.Size = new Size(200, 45);
            btnLogout.BackColor = Color.FromArgb(220, 53, 69);
            btnLogout.ForeColor = Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Click += (s, e) => Logout();
            sidebarPanel.Controls.Add(btnLogout);

            return sidebarPanel;
        }

        private void AddSidebarButton(Panel sidebar, string text, int yPos, EventHandler handler)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            btn.Location = new Point(10, yPos);
            btn.Size = new Size(200, 45);
            btn.BackColor = Color.FromArgb(80, 80, 85);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Click += handler;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(100, 100, 105);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(80, 80, 85);
            sidebar.Controls.Add(btn);
        }

        // ==================== ADMIN MENU ====================
        private void LoadAdminUsers()
        {
            _mainContentPanel.Controls.Clear();
            UC_Admin ucAdmin = new UC_Admin();
            ucAdmin.Dock = DockStyle.Fill;
            _mainContentPanel.Controls.Add(ucAdmin);
        }

        private void LoadAdminBranch()
        {
            LoadComingSoon("QUẢN LÝ CHI NHÁNH");
        }

        private void LoadAdminMovie()
        {
            LoadComingSoon("QUẢN LÝ PHIM");
        }

        private void LoadAdminReport()
        {
            LoadComingSoon("BÁO CÁO DOANH THU");
        }

        // ==================== MANAGER MENU ====================
        private void LoadManagerStaff()
        {
            LoadComingSoon("QUẢN LÝ NHÂN SỰ");
        }

        private void LoadManagerWarehouse()
        {
            LoadComingSoon("QUẢN LÝ KHO BẮP NƯỚC");
        }

        private void LoadManagerSchedule()
        {
            LoadComingSoon("QUẢN LÝ LỊCH CHIẾU");
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

        // ==================== COMMON ====================
        private void LoadHome()
        {
            _mainContentPanel.Controls.Clear();

            Panel homePanel = new Panel();
            homePanel.Dock = DockStyle.Fill;
            homePanel.BackColor = Color.White;

            Label lblWelcome = new Label();
            lblWelcome.Text = $"Chào mừng {_username.ToUpper()}!";
            lblWelcome.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(0, 170, 255);
            lblWelcome.Location = new Point(50, 50);
            lblWelcome.Size = new Size(800, 60);
            lblWelcome.TextAlign = ContentAlignment.MiddleLeft;
            homePanel.Controls.Add(lblWelcome);

            Label lblRole = new Label();
            lblRole.Text = $"Vai trò: {_userRole} | Chi nhánh: {_branch}";
            lblRole.Font = new Font("Segoe UI", 12);
            lblRole.ForeColor = Color.Gray;
            lblRole.Location = new Point(50, 120);
            lblRole.Size = new Size(800, 30);
            lblRole.TextAlign = ContentAlignment.MiddleLeft;
            homePanel.Controls.Add(lblRole);

            _mainContentPanel.Controls.Add(homePanel);
        }

        private void LoadComingSoon(string featureName)
        {
            _mainContentPanel.Controls.Clear();

            Label lblMessage = new Label();
            lblMessage.Text = $"{featureName}\n\nDang phat trien...";
            lblMessage.Font = new Font("Segoe UI", 16);
            lblMessage.ForeColor = Color.Gray;
            lblMessage.Dock = DockStyle.Fill;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            _mainContentPanel.Controls.Add(lblMessage);
        }

        private void ShowUserInfo()
        {
            MessageBox.Show($"Thông tin tài khoản:\n\n" +
                          $"Tên đăng nhập: {_username}\n" +
                          $"Vai trò: {_userRole}\n" +
                          $"Chi nhánh: {_branch}",
                          "Thông tin tài khoản");
        }

        private void Logout()
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận",
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
