
using QuanLiChuoiRapPhim.BLL;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        public int LoggedInMaNguoiDung { get; set; }
        public int LoggedInMaChiNhanh { get; set; }

        private TextBox txtTenDangNhap;
        private TextBox txtMatKhau;
        private Button btnDangNhap;
        private Button btnThoat;
        private CheckBox chkHienMatKhau;
        private Label lblQuenMatKhau;
        private PictureBox picLogo;
        private Panel pnlLogin;
        private Panel pnlDecoration;

        // Màu sắc CGV theme
        private Color cgvRed = Color.FromArgb(220, 0, 0);
        private Color cgvDarkGray = Color.FromArgb(40, 40, 40);
        private Color cgvLightGray = Color.FromArgb(240, 240, 240);
        private Color accentBlue = Color.FromArgb(0, 120, 215);
        private Color accentGreen = Color.FromArgb(40, 167, 69);

        public frmLogin()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Form settings - Thiết kế hiện đại, responsive
            this.Text = "Đăng Nhập - Hệ Thống Quản Lý CGV Cinema";
            this.Size = new Size(1400, 800);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.Icon = null;

            // Main container
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.White;

            // ========== LEFT PANEL - Gradient background with artwork ==========
            pnlDecoration = new Panel();
            pnlDecoration.Dock = DockStyle.Left;
            pnlDecoration.Width = (int)(this.Width * 0.4);
            pnlDecoration.BackColor = cgvDarkGray;
            pnlDecoration.DoubleBuffered = true;

            // Vẽ gradient background
            pnlDecoration.Paint += (s, e) =>
            {
                LinearGradientBrush brush = new LinearGradientBrush(
                    pnlDecoration.Bounds,
                    Color.FromArgb(20, 20, 25),
                    Color.FromArgb(80, 15, 15),
                    LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, pnlDecoration.Bounds);

                // Vẽ các hình tòn trang trí
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(cgvRed, 2) { DashStyle = DashStyle.Dot })
                {
                    e.Graphics.DrawEllipse(pen, 30, 150, 220, 220);
                    e.Graphics.DrawEllipse(pen, 80, 380, 180, 180);
                }
            };

            // Logo
            picLogo = new PictureBox();
            picLogo.Image = CreateCGVLogo();
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.Location = new Point((pnlDecoration.Width - 280) / 2, 60);
            picLogo.Size = new Size(280, 100);
            picLogo.BackColor = Color.Transparent;
            pnlDecoration.Controls.Add(picLogo);
            AddDragControl(picLogo);

            // Main title
            Label lblCGVTitle = new Label();
            lblCGVTitle.Text = "CGV CINEMA";
            lblCGVTitle.Font = new Font("Arial Black", 36, FontStyle.Bold);
            lblCGVTitle.ForeColor = cgvRed;
            lblCGVTitle.AutoSize = false;
            lblCGVTitle.Location = new Point(20, 160);
            lblCGVTitle.Size = new Size(pnlDecoration.Width - 40, 60);
            lblCGVTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblCGVTitle.BackColor = Color.Transparent;
            pnlDecoration.Controls.Add(lblCGVTitle);
            AddDragControl(lblCGVTitle);

            // Slogan
            Label lblSlogan = new Label();
            lblSlogan.Text = "Hệ Thống Quản Lý\nChuỗi Rạp Phim Chuyên Nghiệp";
            lblSlogan.Font = new Font("Segoe UI", 14, FontStyle.Regular);
            lblSlogan.ForeColor = Color.FromArgb(220, 220, 220);
            lblSlogan.AutoSize = false;
            lblSlogan.Location = new Point(20, 240);
            lblSlogan.Size = new Size(pnlDecoration.Width - 40, 80);
            lblSlogan.TextAlign = ContentAlignment.MiddleCenter;
            lblSlogan.BackColor = Color.Transparent;
            pnlDecoration.Controls.Add(lblSlogan);

            // Theater illustration
            PictureBox picTheater = new PictureBox();
            picTheater.Image = CreateTheaterImage();
            picTheater.SizeMode = PictureBoxSizeMode.Zoom;
            picTheater.Location = new Point(20, 330);
            picTheater.Size = new Size(pnlDecoration.Width - 40, 320);
            picTheater.BackColor = Color.Transparent;
            pnlDecoration.Controls.Add(picTheater);

            // Copyright at bottom
            Label lblCopyright = new Label();
            lblCopyright.Text = "© 2024 CGV Cinemas. All rights reserved.";
            lblCopyright.Font = new Font("Segoe UI", 9);
            lblCopyright.ForeColor = Color.FromArgb(120, 120, 120);
            lblCopyright.Dock = DockStyle.Bottom;
            lblCopyright.Height = 35;
            lblCopyright.TextAlign = ContentAlignment.MiddleCenter;
            lblCopyright.BackColor = Color.Transparent;
            pnlDecoration.Controls.Add(lblCopyright);

            // ========== RIGHT PANEL - Login Form ==========
            pnlLogin = new Panel();
            pnlLogin.Dock = DockStyle.Fill;
            pnlLogin.BackColor = Color.White;
            pnlLogin.AutoScroll = true;
            pnlLogin.Padding = new Padding(60, 40, 60, 40);

            int yPos = 0;

            // Welcome section
            Label lblWelcome = new Label();
            lblWelcome.Text = "Chào mừng bạn";
            lblWelcome.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblWelcome.ForeColor = cgvDarkGray;
            lblWelcome.Location = new Point(60, yPos + 20);
            lblWelcome.Size = new Size(600, 45);
            lblWelcome.AutoSize = false;
            pnlLogin.Controls.Add(lblWelcome);
            yPos += 70;

            // Subtitle
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Đăng nhập vào hệ thống quản lý";
            lblSubtitle.Font = new Font("Segoe UI", 13, FontStyle.Regular);
            lblSubtitle.ForeColor = Color.FromArgb(150, 150, 150);
            lblSubtitle.Location = new Point(60, yPos);
            lblSubtitle.Size = new Size(600, 30);
            pnlLogin.Controls.Add(lblSubtitle);
            yPos += 50;

            // Username field
            Label lblUsername = new Label();
            lblUsername.Text = "Tên đăng nhập";
            lblUsername.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblUsername.ForeColor = cgvDarkGray;
            lblUsername.Location = new Point(60, yPos);
            lblUsername.Size = new Size(600, 25);
            pnlLogin.Controls.Add(lblUsername);
            yPos += 30;

            txtTenDangNhap = new TextBox();
            txtTenDangNhap.Font = new Font("Segoe UI", 12);
            txtTenDangNhap.Location = new Point(60, yPos);
            txtTenDangNhap.Size = new Size(600, 45);
            txtTenDangNhap.BorderStyle = BorderStyle.None;
            txtTenDangNhap.Padding = new Padding(15, 12, 15, 12);
            txtTenDangNhap.BackColor = Color.FromArgb(248, 248, 248);
            txtTenDangNhap.KeyPress += TxtTenDangNhap_KeyPress;

            // Username field styling
            Panel usernamePanel = new Panel();
            usernamePanel.Location = new Point(60, yPos);
            usernamePanel.Size = new Size(600, 45);
            // Vẽ viền thủ công
            usernamePanel.Paint += (s, e) => 
            {
                Color borderColor = txtTenDangNhap.Focused ? cgvRed : Color.FromArgb(220, 220, 220);
                ControlPaint.DrawBorder(e.Graphics, usernamePanel.ClientRectangle, borderColor, ButtonBorderStyle.Solid);
            };
            usernamePanel.Controls.Add(txtTenDangNhap);
            pnlLogin.Controls.Add(usernamePanel);

            // Redraw panel khi focus thay đổi
            txtTenDangNhap.Enter += (s, e) => usernamePanel.Invalidate();
            txtTenDangNhap.Leave += (s, e) => usernamePanel.Invalidate();

            yPos += 60;

            // Password field
            Label lblPassword = new Label();
            lblPassword.Text = "Mật khẩu";
            lblPassword.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblPassword.ForeColor = cgvDarkGray;
            lblPassword.Location = new Point(60, yPos);
            lblPassword.Size = new Size(600, 25);
            pnlLogin.Controls.Add(lblPassword);
            yPos += 30;

            txtMatKhau = new TextBox();
            txtMatKhau.Font = new Font("Segoe UI", 12);
            txtMatKhau.Location = new Point(60, yPos);
            txtMatKhau.Size = new Size(600, 45);
            txtMatKhau.BorderStyle = BorderStyle.None;
            txtMatKhau.UseSystemPasswordChar = true;
            txtMatKhau.Padding = new Padding(15, 12, 15, 12);
            txtMatKhau.BackColor = Color.FromArgb(248, 248, 248);
            txtMatKhau.KeyPress += TxtMatKhau_KeyPress;

            // Password field styling
            Panel passwordPanel = new Panel();
            passwordPanel.Location = new Point(60, yPos);
            passwordPanel.Size = new Size(600, 45);
            // Vẽ viền thủ công
            passwordPanel.Paint += (s, e) =>
            {
                Color borderColor = txtMatKhau.Focused ? cgvRed : Color.FromArgb(220, 220, 220);
                ControlPaint.DrawBorder(e.Graphics, passwordPanel.ClientRectangle, borderColor, ButtonBorderStyle.Solid);
            };
            passwordPanel.Controls.Add(txtMatKhau);
            pnlLogin.Controls.Add(passwordPanel);

            // Redraw panel khi focus thay đổi
            txtMatKhau.Enter += (s, e) => passwordPanel.Invalidate();
            txtMatKhau.Leave += (s, e) => passwordPanel.Invalidate();

            yPos += 60;

            // Checkbox and forgot password row
            Panel optionsPanel = new Panel();
            optionsPanel.Location = new Point(60, yPos);
            optionsPanel.Size = new Size(600, 30);
            optionsPanel.BackColor = Color.White;

            chkHienMatKhau = new CheckBox();
            chkHienMatKhau.Text = "Hiển thị mật khẩu";
            chkHienMatKhau.Font = new Font("Segoe UI", 10);
            chkHienMatKhau.ForeColor = cgvDarkGray;
            chkHienMatKhau.Location = new Point(0, 5);
            chkHienMatKhau.Size = new Size(200, 20);
            chkHienMatKhau.CheckedChanged += ChkHienMatKhau_CheckedChanged;
            optionsPanel.Controls.Add(chkHienMatKhau);

            lblQuenMatKhau = new Label();
            lblQuenMatKhau.Text = "Quên mật khẩu?";
            lblQuenMatKhau.Font = new Font("Segoe UI", 10, FontStyle.Underline);
            lblQuenMatKhau.ForeColor = cgvRed;
            lblQuenMatKhau.Location = new Point(450, 5);
            lblQuenMatKhau.Size = new Size(150, 20);
            lblQuenMatKhau.TextAlign = ContentAlignment.MiddleRight;
            lblQuenMatKhau.Cursor = Cursors.Hand;
            lblQuenMatKhau.Click += LblQuenMatKhau_Click;
            lblQuenMatKhau.MouseEnter += (s, e) => lblQuenMatKhau.ForeColor = Color.FromArgb(180, 0, 0);
            lblQuenMatKhau.MouseLeave += (s, e) => lblQuenMatKhau.ForeColor = cgvRed;
            optionsPanel.Controls.Add(lblQuenMatKhau);

            pnlLogin.Controls.Add(optionsPanel);
            yPos += 50;

            // Login button
            btnDangNhap = new Button();
            btnDangNhap.Text = "ĐĂNG NHẬP";
            btnDangNhap.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            btnDangNhap.Location = new Point(60, yPos);
            btnDangNhap.Size = new Size(600, 50);
            btnDangNhap.BackColor = cgvRed;
            btnDangNhap.ForeColor = Color.White;
            btnDangNhap.FlatStyle = FlatStyle.Flat;
            btnDangNhap.FlatAppearance.BorderSize = 0;
            btnDangNhap.Cursor = Cursors.Hand;
            btnDangNhap.Click += BtnDangNhap_Click;

            btnDangNhap.MouseEnter += (s, e) =>
            {
                btnDangNhap.BackColor = Color.FromArgb(180, 0, 0);
            };
            btnDangNhap.MouseLeave += (s, e) =>
            {
                btnDangNhap.BackColor = cgvRed;
            };

            pnlLogin.Controls.Add(btnDangNhap);
            yPos += 70;

            // Test accounts section title
            Label lblTestTitle = new Label();
            lblTestTitle.Text = "Tài khoản kiểm tra:";
            lblTestTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTestTitle.ForeColor = cgvDarkGray;
            lblTestTitle.Location = new Point(60, yPos);
            lblTestTitle.Size = new Size(600, 25);
            pnlLogin.Controls.Add(lblTestTitle);
            yPos += 35;

            // Test accounts - 3 buttons in a row
            string[][] testAccounts = new[]
            {
                new[] { "admin", "123456", "👨‍💼 Quản trị", cgvRed.ToString() },
                new[] { "ql_cn1", "123456", "👔 Quản lý CN", accentBlue.ToString() },
                new[] { "nv_ve01", "123456", "🎫 Nhân viên", accentGreen.ToString() }
            };

            int btnWidth = 180;
            int spacing = 20;
            int xPos = 60;

            for (int i = 0; i < testAccounts.Length; i++)
            {
                string username = testAccounts[i][0];
                string password = testAccounts[i][1];
                string displayName = testAccounts[i][2];
                Color btnColor = i == 0 ? cgvRed : (i == 1 ? accentBlue : accentGreen);

                Button testBtn = new Button();
                testBtn.Text = displayName;
                testBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                testBtn.Location = new Point(xPos, yPos);
                testBtn.Size = new Size(btnWidth, 40);
                testBtn.BackColor = Color.White;
                testBtn.ForeColor = btnColor;
                testBtn.FlatStyle = FlatStyle.Flat;
                testBtn.FlatAppearance.BorderColor = btnColor;
                testBtn.FlatAppearance.BorderSize = 2;
                testBtn.Cursor = Cursors.Hand;

                testBtn.MouseEnter += (s, e) =>
                {
                    testBtn.BackColor = btnColor;
                    testBtn.ForeColor = Color.White;
                };
                testBtn.MouseLeave += (s, e) =>
                {
                    testBtn.BackColor = Color.White;
                    testBtn.ForeColor = btnColor;
                };

                testBtn.Click += (s, e) =>
                {
                    txtTenDangNhap.Text = username;
                    txtMatKhau.Text = password;
                    PerformLogin();
                };

                pnlLogin.Controls.Add(testBtn);
                xPos += btnWidth + spacing;
            }

            yPos += 60;

            // Exit button
            btnThoat = new Button();
            btnThoat.Text = "THOÁT ỨNG DỤNG";
            btnThoat.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnThoat.Location = new Point(60, yPos);
            btnThoat.Size = new Size(600, 45);
            btnThoat.BackColor = Color.FromArgb(220, 220, 220);
            btnThoat.ForeColor = cgvDarkGray;
            btnThoat.FlatStyle = FlatStyle.Flat;
            btnThoat.FlatAppearance.BorderSize = 0;
            btnThoat.Cursor = Cursors.Hand;
            btnThoat.Click += BtnThoat_Click;

            btnThoat.MouseEnter += (s, e) => btnThoat.BackColor = Color.FromArgb(200, 200, 200);
            btnThoat.MouseLeave += (s, e) => btnThoat.BackColor = Color.FromArgb(220, 220, 220);

            pnlLogin.Controls.Add(btnThoat);

            // Add panels to main
            mainPanel.Controls.Add(pnlLogin);
            mainPanel.Controls.Add(pnlDecoration);
            this.Controls.Add(mainPanel);

            // Add drag functionality for borderless form
            AddDragControl(this);
            AddDragControl(pnlDecoration);
        }

        private Image CreateCGVLogo()
        {
            Bitmap bmp = new Bitmap(280, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw CGV text
                using (Font font = new Font("Arial Black", 48, FontStyle.Bold))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("CGV", font, new SolidBrush(cgvRed), new RectangleF(0, 0, 280, 100), sf);
                }
            }
            return bmp;
        }

        private Image CreateTheaterImage()
        {
            Bitmap bmp = new Bitmap(400, 280);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Screen background
                LinearGradientBrush screenBrush = new LinearGradientBrush(
                    new Rectangle(50, 15, 300, 50),
                    Color.FromArgb(30, 30, 30),
                    Color.FromArgb(10, 10, 10),
                    LinearGradientMode.Vertical);
                g.FillRectangle(screenBrush, 50, 15, 300, 50);

                // Screen border
                g.DrawRectangle(new Pen(cgvRed, 2), 50, 15, 300, 50);

                // NOW SHOWING text
                using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString("NOW SHOWING", font, Brushes.White, new RectangleF(50, 25, 300, 30), sf);
                }

                // Draw seats
                int seatSize = 12;
                int startX = 50;
                int startY = 80;
                int rows = 8;
                int cols = 18;

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int x = startX + col * (seatSize + 4);
                        int y = startY + row * (seatSize + 4);

                        Color seatColor;
                        if ((row + col) % 4 == 0)
                            seatColor = cgvRed;
                        else if ((row + col) % 3 == 0)
                            seatColor = Color.FromArgb(100, 100, 100);
                        else
                            seatColor = Color.FromArgb(80, 80, 80);

                        g.FillRectangle(new SolidBrush(seatColor), x, y, seatSize, seatSize);
                    }
                }
            }
            return bmp;
        }

        private void AddDragControl(Control control)
        {
            control.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
        }

        private void BtnDangNhap_Click(object sender, EventArgs e)
        {
            PerformLogin();
        }

        private void PerformLogin()
        {
            string username = txtTenDangNhap.Text.Trim();
            string password = txtMatKhau.Text;

            if (!ValidateInput(username, password))
                return;

            btnDangNhap.Text = "ĐANG XỬ LÝ...";
            btnDangNhap.Enabled = false;
            Application.DoEvents();

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
            finally
            {
                btnDangNhap.Text = "ĐĂNG NHẬP";
                btnDangNhap.Enabled = true;
            }
        }

        private bool ValidateInput(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenDangNhap.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhau.Focus();
                return false;
            }

            return true;
        }

        private void LoginSuccessful(string fullName, string username, int maNguoiDung, int maChiNhanh)
        {
            pnlLogin.BackColor = Color.FromArgb(240, 255, 240);
            Application.DoEvents();
            System.Threading.Thread.Sleep(300);
            pnlLogin.BackColor = Color.White;

            string userRole = DetermineUserRole(username);
            string branch = DetermineUserBranch(username, maChiNhanh);

            LoggedInUsername = username;
            LoggedInRole = userRole;
            LoggedInBranch = branch;
            LoggedInFullName = fullName;
            LoggedInMaNguoiDung = maNguoiDung;
            LoggedInMaChiNhanh = maChiNhanh;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string DetermineUserRole(string username)
        {
            if (username.StartsWith("admin"))
                return "Quản trị viên";
            else if (username.StartsWith("ql_"))
                return "Quản lý chi nhánh";
            else if (username.StartsWith("nv_"))
            {
                if (username.Contains("_ve"))
                    return "Nhân viên bán vé";
                else if (username.Contains("_sc"))
                    return "Nhân viên suất chiếu";
                else
                    return "Nhân viên";
            }
            else
                return "Người dùng";
        }

        private string DetermineUserBranch(string username, int maChiNhanh)
        {
            switch (maChiNhanh)
            {
                case 1: return "CGV Vincom Xuân Khánh - Cần Thơ";
                case 2: return "CGV Sense City - Cần Thơ";
                case 3: return "CGV Vincom Hùng Vương - Cần Thơ";
                case 4: return "CGV Vincom Đồng Khởi - TP.HCM";
                case 5: return "CGV Crescent Mall - TP.HCM";
                default: return "Toàn hệ thống";
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Đăng nhập thất bại",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtMatKhau.Focus();
            txtMatKhau.SelectAll();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Lỗi hệ thống",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void LblQuenMatKhau_Click(object sender, EventArgs e)
        {
            using (frmForgotPassword frm = new frmForgotPassword())
            {
                frm.ShowDialog();
            }
        }

        private void BtnThoat_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát ứng dụng?", "Xác nhận thoát",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                for (double opacity = 1.0; opacity > 0; opacity -= 0.1)
                {
                    this.Opacity = opacity;
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(30);
                }
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

        private class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
        }
    }

    public class frmForgotPassword : Form
    {
        public frmForgotPassword()
        {
            this.Text = "Quên mật khẩu";
            this.Size = new Size(450, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.ShowIcon = false;

            Label lblTitle = new Label();
            lblTitle.Text = "Hỗ trợ khôi phục mật khẩu";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(40, 40, 40);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(400, 30);
            this.Controls.Add(lblTitle);

            Label lblInfo = new Label();
            lblInfo.Text = "Vui lòng liên hệ quản trị viên để lấy lại mật khẩu:\n\n" +
                          "📧 Email: admin@cgv.vn\n" +
                          "☎️ Hotline: 1900 6017\n\n" +
                          "Hoặc đến quầy hỗ trợ khách hàng\n" +
                          "tại bất kỳ chi nhánh CGV nào.";
            lblInfo.Font = new Font("Segoe UI", 11);
            lblInfo.Location = new Point(20, 60);
            lblInfo.Size = new Size(400, 180);
            lblInfo.TextAlign = ContentAlignment.TopLeft;
            this.Controls.Add(lblInfo);

            Button btnClose = new Button();
            btnClose.Text = "ĐÓNG";
            btnClose.Location = new Point(175, 255);
            btnClose.Size = new Size(100, 40);
            btnClose.BackColor = Color.FromArgb(220, 0, 0);
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }
    }
}
