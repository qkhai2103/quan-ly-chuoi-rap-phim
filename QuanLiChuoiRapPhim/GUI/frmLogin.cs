using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.GUI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim
{
    public partial class frmLogin : Form
    {
        // Properties để lưu thông tin người dùng đã đăng nhập
        public string LoggedInUsername { get; set; }
        public string LoggedInRole { get; set; }
        public string LoggedInBranch { get; set; }
        public string LoggedInFullName { get; set; }
        public int LoggedInMaNguoiDung { get; set; }     // ID người dùng từ database
        public int LoggedInMaChiNhanh { get; set; }      // Mã chi nhánh từ database

        private TextBox txtTenDangNhap;
        private TextBox txtMatKhau;
        private Button btnDangNhap;
        private Button btnThoat;
        private CheckBox chkHienMatKhau;

        public frmLogin()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Form settings
            this.Text = "Đăng Nhập - Quản Lý Rạp Phim";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Left Panel (Login Form)
            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = 450;
            leftPanel.BackColor = Color.White;

            // Right Panel (Info/Decoration)
            Panel rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.FromArgb(0, 170, 255);

            // Title on left
            Label lblTitle = new Label();
            lblTitle.Text = "ĐĂNG NHẬP";
            lblTitle.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 170, 255);
            lblTitle.Location = new Point(40, 30);
            lblTitle.Size = new Size(370, 70);
            lblTitle.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            leftPanel.Controls.Add(lblTitle);

            int yPos = 110;

            // Username label
            Label lblUser = new Label();
            lblUser.Text = "Tên đăng nhập";
            lblUser.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblUser.ForeColor = Color.FromArgb(80, 80, 80);
            lblUser.Location = new Point(40, yPos);
            lblUser.Size = new Size(370, 20);
            leftPanel.Controls.Add(lblUser);
            yPos += 25;

            // Username textbox
            txtTenDangNhap = new TextBox();
            txtTenDangNhap.Font = new Font("Segoe UI", 11);
            txtTenDangNhap.Location = new Point(40, yPos);
            txtTenDangNhap.Size = new Size(370, 35);
            txtTenDangNhap.Text = "admin";
            txtTenDangNhap.BorderStyle = BorderStyle.FixedSingle;
            txtTenDangNhap.KeyPress += TxtTenDangNhap_KeyPress;
            leftPanel.Controls.Add(txtTenDangNhap);
            yPos += 45;

            // Password label
            Label lblPass = new Label();
            lblPass.Text = "Mật khẩu";
            lblPass.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblPass.ForeColor = Color.FromArgb(80, 80, 80);
            lblPass.Location = new Point(40, yPos);
            lblPass.Size = new Size(370, 20);
            leftPanel.Controls.Add(lblPass);
            yPos += 25;

            // Password textbox
            txtMatKhau = new TextBox();
            txtMatKhau.Font = new Font("Segoe UI", 11);
            txtMatKhau.Location = new Point(40, yPos);
            txtMatKhau.Size = new Size(370, 35);
            txtMatKhau.Text = "123456";
            txtMatKhau.UseSystemPasswordChar = true;
            txtMatKhau.BorderStyle = BorderStyle.FixedSingle;
            txtMatKhau.KeyPress += TxtMatKhau_KeyPress;
            leftPanel.Controls.Add(txtMatKhau);
            yPos += 45;

            // Show password checkbox
            chkHienMatKhau = new CheckBox();
            chkHienMatKhau.Text = "Hiển thị mật khẩu";
            chkHienMatKhau.Font = new Font("Segoe UI", 10);
            chkHienMatKhau.ForeColor = Color.FromArgb(100, 100, 100);
            chkHienMatKhau.Location = new Point(40, yPos);
            chkHienMatKhau.Size = new Size(370, 22);
            chkHienMatKhau.CheckedChanged += ChkHienMatKhau_CheckedChanged;
            leftPanel.Controls.Add(chkHienMatKhau);
            yPos += 35;

            // Login button
            btnDangNhap = new Button();
            btnDangNhap.Text = "ĐĂNG NHẬP";
            btnDangNhap.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnDangNhap.Location = new Point(40, yPos);
            btnDangNhap.Size = new Size(370, 45);
            btnDangNhap.BackColor = Color.FromArgb(0, 170, 255);
            btnDangNhap.ForeColor = Color.White;
            btnDangNhap.FlatStyle = FlatStyle.Flat;
            btnDangNhap.FlatAppearance.BorderSize = 0;
            btnDangNhap.Cursor = Cursors.Hand;
            btnDangNhap.Click += BtnDangNhap_Click;
            leftPanel.Controls.Add(btnDangNhap);
            yPos += 55;

            // Exit button
            btnThoat = new Button();
            btnThoat.Text = "THOÁT";
            btnThoat.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnThoat.Location = new Point(40, yPos);
            btnThoat.Size = new Size(370, 40);
            btnThoat.BackColor = Color.FromArgb(220, 53, 69);
            btnThoat.ForeColor = Color.White;
            btnThoat.FlatStyle = FlatStyle.Flat;
            btnThoat.FlatAppearance.BorderSize = 0;
            btnThoat.Cursor = Cursors.Hand;
            btnThoat.Click += BtnThoat_Click;
            leftPanel.Controls.Add(btnThoat);

            // Right panel content
            Label lblInfo = new Label();
            lblInfo.Text = "QUẢN LÝ RẠP PHIM";
            lblInfo.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblInfo.ForeColor = Color.White;
            lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblInfo.Dock = DockStyle.Top;
            lblInfo.Height = 70;
            rightPanel.Controls.Add(lblInfo);

            // Description panel
            Panel descPanel = new Panel();
            descPanel.Dock = DockStyle.Top;
            descPanel.Height = 280;
            descPanel.BackColor = Color.FromArgb(0, 170, 255);
            descPanel.Padding = new Padding(30, 20, 30, 20);

            Label lblDescription = new Label();
            lblDescription.Text = "Hệ thống quản lý rạp phim hiện đại\n\n" +
                                 "• Quản lý người dùng\n" +
                                 "• Quản lý phim\n" +
                                 "• Quản lý suất chiếu\n" +
                                 "• Báo cáo doanh thu\n\n" +
                                 "Tài khoản test:\n" +
                                 "admin / 123456\n" +
                                 "ql_cn1 / 123456\n" +
                                 "nv_ve01 / 123456";
            lblDescription.Font = new Font("Segoe UI", 10);
            lblDescription.ForeColor = Color.White;
            lblDescription.Dock = DockStyle.Fill;
            lblDescription.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            descPanel.Controls.Add(lblDescription);
            rightPanel.Controls.Add(descPanel);

            // Test buttons panel
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.BackColor = Color.FromArgb(0, 170, 255);
            buttonPanel.Padding = new Padding(20);

            AddTestButton(buttonPanel, "ADMIN", "admin", "Admin", 10, Color.FromArgb(120, 20, 150));
            AddTestButton(buttonPanel, "QUAN LY", "ql_cn1", "Quản lý", 120, Color.FromArgb(220, 120, 20));
            AddTestButton(buttonPanel, "NHAN VIEN", "nv_ve01", "Nhân viên", 230, Color.FromArgb(20, 120, 120));

            rightPanel.Controls.Add(buttonPanel);

            // Add panels to form
            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);
        }

        private void AddTestButton(Panel panel, string text, string username, string role, int topOffset, Color bgColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.Location = new Point(topOffset, 20);
            btn.Size = new Size(90, 45);
            btn.BackColor = bgColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Click += (s, e) =>
            {
                LoggedInUsername = username;
                LoggedInRole = role;
                LoggedInBranch = "CGV Vincom Xuân Khánh";
                LoggedInFullName = text; // Use the button text as full name for test buttons
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            panel.Controls.Add(btn);
        }

        private void BtnDangNhap_Click(object sender, EventArgs e)
        {
            PerformLogin();
        }

        private void PerformLogin()
        {
            string username = txtTenDangNhap.Text.Trim();
            string password = txtMatKhau.Text;

            // Kiểm tra dữ liệu đầu vào
            if (!ValidateInput(username, password))
                return;

            try
            {
                AdminBLL adminBLL = new AdminBLL();
                string fullName, errorMessage;
                int maNguoiDung, maChiNhanh;

                if (adminBLL.LoginWithFullInfo(username, password, out fullName, out maNguoiDung, 
                    out maChiNhanh, out errorMessage))
                {
                    LoginSuccessful(fullName, username, maNguoiDung, maChiNhanh);
                }
                else
                {
                    ShowErrorMessage(errorMessage);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private bool ValidateInput(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void LoginSuccessful(string fullName, string username, int maNguoiDung, int maChiNhanh)
        {
            // Xác định vai trò dựa trên tên đăng nhập
            string userRole = DetermineUserRole(username);
            string branch = DetermineUserBranch(username);

            MessageBox.Show($"Đăng nhập thành công!\nChào mừng {fullName}",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Lưu thông tin đăng nhập
            LoggedInUsername = username;
            LoggedInRole = userRole;
            LoggedInBranch = branch;
            LoggedInFullName = fullName;
            LoggedInMaNguoiDung = maNguoiDung;
            LoggedInMaChiNhanh = maChiNhanh;

            // Đặt DialogResult.OK để báo rằng đăng nhập thành công
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string DetermineUserRole(string username)
        {
            // Phân loại vai trò dựa trên tên đăng nhập
            if (username.StartsWith("admin"))
                return "Admin";
            else if (username.StartsWith("ql_"))
                return "Quản lý";
            else if (username.StartsWith("nv_"))
                return "Nhân viên";
            else
                return "Người dùng";
        }

        private string DetermineUserBranch(string username)
        {
            // Xác định chi nhánh dựa trên tên đăng nhập
            if (username.EndsWith("_cn1") || username.Contains("cn1"))
                return "CGV Vincom Xuân Khánh";
            else if (username.EndsWith("_cn2") || username.Contains("cn2"))
                return "CGV Sense City Cần Thơ";
            else if (username.EndsWith("_cn3") || username.Contains("cn3"))
                return "CGV Vincom Hùng Vương";
            else
                return "Toàn hệ thống";
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Đăng nhập thất bại",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Lỗi hệ thống",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnThoat_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát chương trình?", "Xác nhận thoát",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void ChkHienMatKhau_CheckedChanged(object sender, EventArgs e)
        {
            txtMatKhau.UseSystemPasswordChar = !chkHienMatKhau.Checked;
        }

        private void TxtMatKhau_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                PerformLogin();
                e.Handled = true;
            }
        }

        private void TxtTenDangNhap_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtMatKhau.Focus();
                e.Handled = true;
            }
        }
        private TabControl CreateTabControl()
        {
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Margin = new Padding(10);
            tabControl.Padding = new Point(15, 10);
            tabControl.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // ========== TAB QUẢN LÝ NGƯỜI DÙNG (ADMIN) ==========
            TabPage tabUsers = new TabPage("👥 QUẢN LÝ NGƯỜI DÙNG");

            // CÁCH AN TOÀN: Không tạo UC_Admin ngay
            tabUsers.Enter += (sender, e) => {
                if (tabUsers.Controls.Count == 0)
                {
                    LoadAdminTab(tabUsers);
                }
            };

            // ========== TAB QUẢN LÝ PHIM ==========
            TabPage tabMovies = new TabPage("🎬 QUẢN LÝ PHIM");

            // Dùng UC_Movies đơn giản
            UC_Movies ucMovies = null;
            try
            {
                ucMovies = new UC_Movies();
                ucMovies.Dock = DockStyle.Fill;
                tabMovies.Controls.Add(ucMovies);
            }
            catch
            {
                // Fallback nếu lỗi
                Label lblMovies = new Label();
                lblMovies.Text = "Chức năng Quản lý phim\n\nSẽ có sớm!";
                lblMovies.Dock = DockStyle.Fill;
                lblMovies.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                lblMovies.Font = new Font("Segoe UI", 14);
                lblMovies.ForeColor = Color.Gray;
                tabMovies.Controls.Add(lblMovies);
            }

            // ========== TAB BÁO CÁO ==========
            TabPage tabReports = new TabPage("📊 BÁO CÁO");

            // Dùng UC đơn giản hoặc Label
            Label lblReports = new Label();
            lblReports.Text = "Chức năng Báo cáo\n\nSẽ có sớm!";
            lblReports.Dock = DockStyle.Fill;
            lblReports.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblReports.Font = new Font("Segoe UI", 14);
            lblReports.ForeColor = Color.Gray;
            tabReports.Controls.Add(lblReports);

            // Thêm các tab vào TabControl
            tabControl.TabPages.Add(tabUsers);    // Tab 0 - Quản lý người dùng
            tabControl.TabPages.Add(tabMovies);   // Tab 1 - Quản lý phim
            tabControl.TabPages.Add(tabReports);  // Tab 2 - Báo cáo

            // Chọn tab đầu tiên (Admin)
            tabControl.SelectedIndex = 0;

            return tabControl;
        }

        // Method tải UC_Admin khi cần
        private void LoadAdminTab(TabPage tabPage)
        {
            try
            {
                // Tạo UC_Admin trên UI thread
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<TabPage>(LoadAdminTab), tabPage);
                    return;
                }

                // Đảm bảo tab chưa bị dispose
                if (tabPage.IsDisposed || !tabPage.IsHandleCreated)
                    return;

                // Tạo loading indicator
                Label lblLoading = new Label();
                lblLoading.Text = "Đang tải quản lý người dùng...";
                lblLoading.Dock = DockStyle.Fill;
                lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                lblLoading.Font = new Font("Segoe UI", 12);
                tabPage.Controls.Add(lblLoading);

                // Sử dụng BackgroundWorker để tạo UC_Admin
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, e) => {
                    // Chờ một chút
                    System.Threading.Thread.Sleep(100);
                };

                worker.RunWorkerCompleted += (s, e) => {
                    if (!tabPage.IsDisposed)
                    {
                        tabPage.SuspendLayout();
                        tabPage.Controls.Clear();

                        try
                        {
                            // Tạo UC_Admin
                            UC_Admin ucAdmin = new UC_Admin();
                            ucAdmin.Dock = DockStyle.Fill;
                            tabPage.Controls.Add(ucAdmin);
                        }
                        catch (Exception ex)
                        {
                            // Fallback nếu lỗi
                            Label lblError = new Label();
                            lblError.Text = $"Không thể tải quản lý người dùng:\n{ex.Message}";
                            lblError.Dock = DockStyle.Fill;
                            lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                            lblError.Font = new Font("Segoe UI", 12);
                            lblError.ForeColor = Color.Red;
                            tabPage.Controls.Add(lblError);
                        }

                        tabPage.ResumeLayout(true);
                    }
                };

                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải tab Admin: {ex.Message}");
            }
        }
    }
}
