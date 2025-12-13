using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class frmMain : Form  // ĐẢM BẢO THỪA KẾ TỪ Form
    {
        private string _username;
        private string _userRole;
        private string _branch;

        public frmMain(string username, string userRole, string branch)
        {
            InitializeComponent();  // GỌI InitializeComponent TRƯỚC

            _username = username;
            _userRole = userRole;
            _branch = branch;

            SetupMainForm();
        }

        public frmMain()
        {
            InitializeComponent();
            _username = "Khách";
            _userRole = "Chưa xác định";
            _branch = "N/A";
            SetupMainForm();
        }

        private void SetupMainForm()
        {
            this.Text = $"Quản Lý Rạp Phim - {_username} ({_userRole})";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.WindowState = FormWindowState.Maximized;

            // Tạo Menu Strip
            MenuStrip mainMenu = CreateMainMenu();
            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // Panel header
            Panel headerPanel = CreateHeaderPanel();
            this.Controls.Add(headerPanel);

            // Tạo TabControl
            TabControl tabControl = CreateTabControl();
            this.Controls.Add(tabControl);

            // Đưa menu lên trên cùng
            this.Controls.SetChildIndex(mainMenu, 0);
        }

        private MenuStrip CreateMainMenu()
        {
            MenuStrip mainMenu = new MenuStrip();
            mainMenu.Font = new Font("Segoe UI", 10);

            // Menu Hệ thống
            ToolStripMenuItem systemMenu = new ToolStripMenuItem("&Hệ thống");

            ToolStripMenuItem userInfoItem = new ToolStripMenuItem("&Thông tin tài khoản", null,
                (s, e) => ShowUserInfo());

            ToolStripMenuItem changePassItem = new ToolStripMenuItem("&Đổi mật khẩu", null,
                (s, e) => ChangePassword());

            ToolStripMenuItem logoutItem = new ToolStripMenuItem("&Đăng xuất", null,
                (s, e) => Logout());

            ToolStripMenuItem exitItem = new ToolStripMenuItem("&Thoát", null,
                (s, e) => Application.Exit());

            systemMenu.DropDownItems.Add(userInfoItem);
            systemMenu.DropDownItems.Add(changePassItem);
            systemMenu.DropDownItems.Add(new ToolStripSeparator());
            systemMenu.DropDownItems.Add(logoutItem);
            systemMenu.DropDownItems.Add(new ToolStripSeparator());
            systemMenu.DropDownItems.Add(exitItem);

            // Menu Quản lý
            ToolStripMenuItem manageMenu = new ToolStripMenuItem("&Quản lý");
            ToolStripMenuItem usersItem = new ToolStripMenuItem("&Người dùng", null,
                (s, e) => ShowTab(0));
            ToolStripMenuItem moviesItem = new ToolStripMenuItem("&Phim", null,
                (s, e) => ShowTab(1));
            ToolStripMenuItem cinemaItem = new ToolStripMenuItem("&Rạp chiếu", null,
                (s, e) => ShowTab(2));

            manageMenu.DropDownItems.Add(usersItem);
            manageMenu.DropDownItems.Add(moviesItem);
            manageMenu.DropDownItems.Add(cinemaItem);

            // Menu Báo cáo
            ToolStripMenuItem reportMenu = new ToolStripMenuItem("&Báo cáo");
            ToolStripMenuItem revenueItem = new ToolStripMenuItem("&Doanh thu", null,
                (s, e) => ShowReport("Doanh thu"));
            ToolStripMenuItem ticketItem = new ToolStripMenuItem("&Vé bán", null,
                (s, e) => ShowReport("Vé bán"));

            reportMenu.DropDownItems.Add(revenueItem);
            reportMenu.DropDownItems.Add(ticketItem);

            // Menu Trợ giúp
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("&Trợ giúp");
            ToolStripMenuItem guideItem = new ToolStripMenuItem("&Hướng dẫn sử dụng", null,
                (s, e) => ShowGuide());
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("&Giới thiệu", null,
                (s, e) => ShowAbout());

            helpMenu.DropDownItems.Add(guideItem);
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(aboutItem);

            mainMenu.Items.Add(systemMenu);
            mainMenu.Items.Add(manageMenu);
            mainMenu.Items.Add(reportMenu);
            mainMenu.Items.Add(helpMenu);

            return mainMenu;
        }

        private Panel CreateHeaderPanel()
        {
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(0, 170, 255);

            Label lblWelcome = new Label();
            lblWelcome.Text = $"XIN CHÀO: {_username.ToUpper()} | VAI TRÒ: {_userRole.ToUpper()} | CHI NHÁNH: {_branch.ToUpper()}";
            lblWelcome.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Dock = DockStyle.Fill;
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;

            headerPanel.Controls.Add(lblWelcome);
            return headerPanel;
        }

        private TabControl CreateTabControl()
        {
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Margin = new Padding(10);
            tabControl.Padding = new Point(15, 10);
            tabControl.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Tab Quản lý người dùng
            TabPage tabUsers = new TabPage("👥 QUẢN LÝ NGƯỜI DÙNG");
            UC_Admin ucAdmin = new UC_Admin();
            ucAdmin.Dock = DockStyle.Fill;
            tabUsers.Controls.Add(ucAdmin);

            // Tab Quản lý phim
            TabPage tabMovies = new TabPage("🎬 QUẢN LÝ PHIM");
            Label lblMovies = new Label();
            lblMovies.Text = "Chức năng Quản lý phim đang được phát triển...\n\nSẽ có sớm!";
            lblMovies.Dock = DockStyle.Fill;
            lblMovies.TextAlign = ContentAlignment.MiddleCenter;
            lblMovies.Font = new Font("Segoe UI", 14);
            lblMovies.ForeColor = Color.Gray;
            tabMovies.Controls.Add(lblMovies);

            // Tab Quản lý rạp
            TabPage tabCinema = new TabPage("🏢 QUẢN LÝ RẠP");
            Label lblCinema = new Label();
            lblCinema.Text = "Chức năng Quản lý rạp đang được phát triển...\n\nSẽ có sớm!";
            lblCinema.Dock = DockStyle.Fill;
            lblCinema.TextAlign = ContentAlignment.MiddleCenter;
            lblCinema.Font = new Font("Segoe UI", 14);
            lblCinema.ForeColor = Color.Gray;
            tabCinema.Controls.Add(lblCinema);

            tabControl.TabPages.Add(tabUsers);
            tabControl.TabPages.Add(tabMovies);
            tabControl.TabPages.Add(tabCinema);

            // Tab Báo cáo
            TabPage tabReports = new TabPage("📊 BÁO CÁO");
            Label lblReports = new Label();
            lblReports.Text = "Chức năng Báo cáo đang được phát triển...\n\nSẽ có sớm!";
            lblReports.Dock = DockStyle.Fill;
            lblReports.TextAlign = ContentAlignment.MiddleCenter;
            lblReports.Font = new Font("Segoe UI", 14);
            lblReports.ForeColor = Color.Gray;
            tabReports.Controls.Add(lblReports);

            tabControl.TabPages.Add(tabReports);

            return tabControl;
        }

        private void ShowTab(int tabIndex)
        {
            // Tìm TabControl
            foreach (Control control in this.Controls)
            {
                if (control is TabControl tabControl)
                {
                    if (tabIndex >= 0 && tabIndex < tabControl.TabPages.Count)
                    {
                        tabControl.SelectedIndex = tabIndex;
                    }
                    break;
                }
            }
        }

        private void ShowUserInfo()
        {
            MessageBox.Show($"Thông tin tài khoản:\n\n" +
                          $"Tên đăng nhập: {_username}\n" +
                          $"Vai trò: {_userRole}\n" +
                          $"Chi nhánh: {_branch}",
                          "Thông tin tài khoản");
        }

        private void ChangePassword()
        {
            MessageBox.Show("Chức năng đổi mật khẩu đang phát triển", "Thông báo");
        }

        private void Logout()
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Tìm và hiển thị form đăng nhập
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

        private void ShowReport(string reportType)
        {
            MessageBox.Show($"Báo cáo {reportType} đang được phát triển", "Thông báo");
        }

        private void ShowGuide()
        {
            MessageBox.Show("Hướng dẫn sử dụng phần mềm:\n\n" +
                          "1. Menu Hệ thống: Quản lý tài khoản\n" +
                          "2. Menu Quản lý: Các chức năng quản lý\n" +
                          "3. Menu Báo cáo: Xem báo cáo thống kê\n" +
                          "4. Menu Trợ giúp: Hướng dẫn và giới thiệu",
                          "Hướng dẫn sử dụng");
        }

        private void ShowAbout()
        {
            MessageBox.Show("Phần mềm Quản lý Rạp Phim\n\n" +
                          "Version: 1.0\n" +
                          "Phát triển bởi: Nhóm Quản lý Rạp Phim\n" +
                          "© 2024 - All rights reserved",
                          "Giới thiệu phần mềm");
        }
    }
}