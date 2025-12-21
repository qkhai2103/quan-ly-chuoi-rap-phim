
﻿
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
        private Panel pnlLeft;

        // Màu sắc CGV theme chuẩn
        private Color cgvRed = Color.FromArgb(220, 0, 0);      // Đỏ CGV chính
        private Color cgvDark = Color.FromArgb(20, 20, 20);    // Nền tối
        private Color cgvGray = Color.FromArgb(245, 245, 245); // Nền sáng
        private Color cgvTextDark = Color.FromArgb(50, 50, 50);// Chữ đậm
        private Color cgvTextLight = Color.FromArgb(120, 120, 120); // Chữ nhạt

        // Gradient colors
        private Color gradientStart = Color.FromArgb(220, 0, 0);
        private Color gradientEnd = Color.FromArgb(180, 0, 0);

        public frmLogin()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Form settings
            this.Text = "Đăng Nhập - CGV Cinema Management";
            this.Size = new Size(900, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(0);
            this.DoubleBuffered = true;

            // Main container với shadow effect
            Panel mainContainer = new Panel();
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.BackColor = Color.White;
            mainContainer.Padding = new Padding(0);

            // Panel trái (Background gradient)
            pnlLeft = new Panel();
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Width = 400;
            pnlLeft.BackColor = cgvDark;
            pnlLeft.Paint += PnlLeft_Paint;

            // Panel phải (Login form)
            pnlLogin = new Panel();
            pnlLogin.Dock = DockStyle.Fill;
            pnlLogin.BackColor = Color.White;
            pnlLogin.Padding = new Padding(50, 40, 50, 40);

            // ========== PHẦN TRÁI ==========
            // Logo CGV
            picLogo = new PictureBox();
            picLogo.Image = CreateModernLogo();
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.Location = new Point(50, 60);
            picLogo.Size = new Size(300, 80);
            pnlLeft.Controls.Add(picLogo);

            // Slogan
            Label lblSlogan = new Label();
            lblSlogan.Text = "CINEMA FOR EVERYONE";
            lblSlogan.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblSlogan.ForeColor = Color.White;
            lblSlogan.Location = new Point(50, 150);
            lblSlogan.Size = new Size(300, 30);
            lblSlogan.TextAlign = ContentAlignment.MiddleCenter;
            pnlLeft.Controls.Add(lblSlogan);

            // Welcome text
            Label lblWelcome = new Label();
            lblWelcome.Text = "Welcome to CGV Management System";
            lblWelcome.Font = new Font("Segoe UI", 11);
            lblWelcome.ForeColor = Color.FromArgb(180, 180, 180);
            lblWelcome.Location = new Point(50, 190);
            lblWelcome.Size = new Size(300, 25);
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;
            pnlLeft.Controls.Add(lblWelcome);

            // Version info
            Label lblVersion = new Label();
            lblVersion.Text = "Version 2.0.24";
            lblVersion.Font = new Font("Segoe UI", 9);
            lblVersion.ForeColor = Color.FromArgb(120, 120, 120);
            lblVersion.Location = new Point(50, 400);
            lblVersion.Size = new Size(300, 20);
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            pnlLeft.Controls.Add(lblVersion);

            // ========== PHẦN LOGIN ==========
            int yPos = 20;

            // Title
            Label lblTitle = new Label();
            lblTitle.Text = "ĐĂNG NHẬP";
            lblTitle.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblTitle.ForeColor = cgvTextDark;
            lblTitle.Location = new Point(0, yPos);
            lblTitle.Size = new Size(400, 60);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            pnlLogin.Controls.Add(lblTitle);
            yPos += 70;

            // Subtitle
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Vui lòng đăng nhập để tiếp tục";
            lblSubtitle.Font = new Font("Segoe UI", 11);
            lblSubtitle.ForeColor = cgvTextLight;
            lblSubtitle.Location = new Point(0, yPos);
            lblSubtitle.Size = new Size(400, 25);
            lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;
            pnlLogin.Controls.Add(lblSubtitle);
            yPos += 40;

            // Username field
            Panel userPanel = CreateModernInputField("Tên đăng nhập", "admin", yPos, false);
            txtTenDangNhap = userPanel.Controls[1] as TextBox;
            pnlLogin.Controls.Add(userPanel);
            yPos += 75;

            // Password field
            Panel passPanel = CreateModernInputField("Mật khẩu", "••••••", yPos, true);
            txtMatKhau = passPanel.Controls[1] as TextBox;
            pnlLogin.Controls.Add(passPanel);
            yPos += 75;

            // Remember me & Show password
            Panel optionsPanel = new Panel();
            optionsPanel.Location = new Point(0, yPos);
            optionsPanel.Size = new Size(400, 30);
            optionsPanel.BackColor = Color.Transparent;

            chkHienMatKhau = new CheckBox();
            chkHienMatKhau.Text = "Hiển thị mật khẩu";
            chkHienMatKhau.Font = new Font("Segoe UI", 10);
            chkHienMatKhau.ForeColor = cgvTextLight;
            chkHienMatKhau.Location = new Point(0, 0);
            chkHienMatKhau.Size = new Size(150, 25);
            chkHienMatKhau.CheckedChanged += ChkHienMatKhau_CheckedChanged;

            lblQuenMatKhau = new Label();
            lblQuenMatKhau.Text = "Quên mật khẩu?";
            lblQuenMatKhau.Font = new Font("Segoe UI", 10, FontStyle.Underline);
            lblQuenMatKhau.ForeColor = cgvRed;
            lblQuenMatKhau.Location = new Point(250, 0);
            lblQuenMatKhau.Size = new Size(150, 25);
            lblQuenMatKhau.TextAlign = ContentAlignment.MiddleRight;
            lblQuenMatKhau.Cursor = Cursors.Hand;
            lblQuenMatKhau.Click += LblQuenMatKhau_Click;

            optionsPanel.Controls.Add(chkHienMatKhau);
            optionsPanel.Controls.Add(lblQuenMatKhau);
            pnlLogin.Controls.Add(optionsPanel);
            yPos += 40;

            // Login button
            btnDangNhap = CreateModernButton("ĐĂNG NHẬP", cgvRed, yPos);
            btnDangNhap.Click += BtnDangNhap_Click;
            pnlLogin.Controls.Add(btnDangNhap);
            yPos += 60;

            // Separator
            Panel separator = new Panel();
            separator.Location = new Point(0, yPos);
            separator.Size = new Size(400, 1);
            separator.BackColor = Color.FromArgb(230, 230, 230);
            pnlLogin.Controls.Add(separator);
            yPos += 20;

            // Demo accounts label
            Label lblDemo = new Label();
            lblDemo.Text = "Tài khoản demo:";
            lblDemo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblDemo.ForeColor = cgvTextLight;
            lblDemo.Location = new Point(0, yPos);
            lblDemo.Size = new Size(400, 25);
            lblDemo.TextAlign = ContentAlignment.MiddleLeft;
            pnlLogin.Controls.Add(lblDemo);
            yPos += 30;

            // Demo accounts buttons
            AddDemoAccountButton("Quản trị viên", "admin", "123456", yPos, cgvRed);
            yPos += 35;
            AddDemoAccountButton("Quản lý chi nhánh", "ql_cn1", "123456", yPos, Color.FromArgb(0, 120, 215));
            yPos += 35;
            AddDemoAccountButton("Nhân viên bán vé", "nv_ve01", "123456", yPos, Color.FromArgb(40, 167, 69));
            yPos += 50;

            // Exit button
            btnThoat = CreateModernButton("THOÁT", Color.FromArgb(108, 117, 125), yPos);
            btnThoat.Click += BtnThoat_Click;
            pnlLogin.Controls.Add(btnThoat);

            // Add panels to form
            mainContainer.Controls.Add(pnlLogin);
            mainContainer.Controls.Add(pnlLeft);
            this.Controls.Add(mainContainer);

            // Add drag functionality for borderless form
            AddDragControl(this);
            AddDragControl(pnlLeft);
            AddDragControl(picLogo);
            AddDragControl(lblTitle);

            // Add close button
            AddCloseButton();
        }

        private void PnlLeft_Paint(object sender, PaintEventArgs e)
        {
            // Vẽ gradient background
            using (LinearGradientBrush brush = new LinearGradientBrush(
                pnlLeft.ClientRectangle,
                gradientStart,
                gradientEnd,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, pnlLeft.ClientRectangle);
            }

            // Vẽ pattern overlay
            using (TextureBrush patternBrush = CreatePatternBrush())
            {
                patternBrush.TranslateTransform(0, 0);
                e.Graphics.FillRectangle(patternBrush, pnlLeft.ClientRectangle);
            }
        }

        private TextureBrush CreatePatternBrush()
        {
            Bitmap pattern = new Bitmap(20, 20);
            using (Graphics g = Graphics.FromImage(pattern))
            {
                g.Clear(Color.Transparent);
                using (Pen pen = new Pen(Color.FromArgb(20, 255, 255, 255), 1))
                {
                    for (int i = 0; i < 20; i += 4)
                    {
                        g.DrawLine(pen, i, 0, i, 20);
                        g.DrawLine(pen, 0, i, 20, i);
                    }
                }
            }
            return new TextureBrush(pattern);
        }

        private Panel CreateModernInputField(string labelText, string placeholder, int yPos, bool isPassword)
        {
            Panel panel = new Panel();
            panel.Location = new Point(0, yPos);
            panel.Size = new Size(400, 70);

            // Label
            Label label = new Label();
            label.Text = labelText.ToUpper();
            label.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            label.ForeColor = cgvTextDark;
            label.Location = new Point(0, 0);
            label.Size = new Size(400, 20);
            panel.Controls.Add(label);

            // TextBox container
            Panel inputContainer = new Panel();
            inputContainer.Location = new Point(0, 22);
            inputContainer.Size = new Size(400, 45);
            inputContainer.BackColor = cgvGray;
            inputContainer.BorderStyle = BorderStyle.None;
            inputContainer.Padding = new Padding(1);

            // Inner panel for border effect
            Panel innerPanel = new Panel();
            innerPanel.Dock = DockStyle.Fill;
            innerPanel.BackColor = Color.White;
            innerPanel.Padding = new Padding(15, 0, 15, 0);

            TextBox textBox = new TextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.Font = new Font("Segoe UI", 11);
            textBox.BorderStyle = BorderStyle.None;
            textBox.Text = placeholder;
            textBox.ForeColor = cgvTextDark;
            textBox.BackColor = Color.White;

            if (isPassword)
            {
                textBox.UseSystemPasswordChar = true;
                textBox.KeyPress += TxtMatKhau_KeyPress;
            }
            else
            {
                textBox.KeyPress += TxtTenDangNhap_KeyPress;
            }

            // Focus effects
            textBox.Enter += (s, e) =>
            {
                inputContainer.BackColor = cgvRed;
                textBox.ForeColor = cgvTextDark;
                if (textBox.Text == placeholder)
                    textBox.Text = "";
            };

            textBox.Leave += (s, e) =>
            {
                inputContainer.BackColor = cgvGray;
                if (string.IsNullOrEmpty(textBox.Text))
                    textBox.Text = placeholder;
            };

            innerPanel.Controls.Add(textBox);
            inputContainer.Controls.Add(innerPanel);
            panel.Controls.Add(inputContainer);

            return panel;
        }

        private Button CreateModernButton(string text, Color backColor, int yPos)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            button.Location = new Point(0, yPos);
            button.Size = new Size(400, 45);
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(0);

            // Rounded corners effect
            button.Paint += (s, e) =>
            {
                using (GraphicsPath path = GetRoundedRectangle(button.ClientRectangle, 5))
                {
                    button.Region = new Region(path);
                }
            };

            // Hover effects
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = ControlPaint.Dark(backColor, 0.1f);
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = backColor;
            };

            button.MouseDown += (s, e) =>
            {
                button.BackColor = ControlPaint.Dark(backColor, 0.2f);
            };

            button.MouseUp += (s, e) =>
            {
                button.BackColor = ControlPaint.Dark(backColor, 0.1f);
            };

            return button;
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void AddDemoAccountButton(string displayName, string username, string password, int yPos, Color color)
        {
            Button btn = new Button();
            btn.Text = $"   {displayName}";
            btn.Font = new Font("Segoe UI", 10);
            btn.Location = new Point(0, yPos);
            btn.Size = new Size(400, 32);
            btn.BackColor = Color.White;
            btn.ForeColor = color;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
            btn.FlatAppearance.BorderSize = 1;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Image = CreateArrowIcon(color);
            btn.ImageAlign = ContentAlignment.MiddleRight;
            btn.Padding = new Padding(15, 0, 15, 0);

            btn.Click += (s, e) =>
            {
                txtTenDangNhap.Text = username;
                txtMatKhau.Text = password;
                PerformLogin();
            };

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(250, 250, 250);
                btn.FlatAppearance.BorderColor = color;
            };

            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.White;
                btn.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
            };

            pnlLogin.Controls.Add(btn);
        }

        private Image CreateArrowIcon(Color color)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(color, 2))
                {
                    g.DrawLines(pen, new Point[] {
                        new Point(4, 4),
                        new Point(12, 8),
                        new Point(4, 12)
                    });
                }
            }
            return bmp;
        }

        private Image CreateModernLogo()
        {
            Bitmap bmp = new Bitmap(300, 80);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Vẽ logo CGV đơn giản
                using (Font font = new Font("Arial", 36, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    g.DrawString("CGV", font, brush, 0, 20);
                }

                // Thêm hiệu ứng bóng
                using (Font font = new Font("Arial", 36, FontStyle.Bold))
                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    g.DrawString("CGV", font, shadowBrush, 2, 22);
                }
            }
            return bmp;
        }

        private void AddCloseButton()
        {
            Button btnClose = new Button();
            btnClose.Text = "×";
            btnClose.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnClose.Size = new Size(40, 40);
            btnClose.Location = new Point(this.Width - 45, 5);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.Transparent;
            btnClose.ForeColor = cgvTextLight;
            btnClose.Cursor = Cursors.Hand;

            btnClose.MouseEnter += (s, e) =>
            {
                btnClose.ForeColor = cgvRed;
                btnClose.BackColor = Color.FromArgb(240, 240, 240);
            };

            btnClose.MouseLeave += (s, e) =>
            {
                btnClose.ForeColor = cgvTextLight;
                btnClose.BackColor = Color.Transparent;
            };

            btnClose.Click += (s, e) => ExitApplication();

            this.Controls.Add(btnClose);
            btnClose.BringToFront();
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

            // Kiểm tra dữ liệu đầu vào
            if (!ValidateInput(username, password))
                return;

            // Hiệu ứng loading
            btnDangNhap.Text = "ĐANG XỬ LÝ...";
            btnDangNhap.Enabled = false;
            Cursor = Cursors.WaitCursor;
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
                Cursor = Cursors.Default;
            }
        }

        private bool ValidateInput(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || username == "Tên đăng nhập")
            {
                ShowValidationError(txtTenDangNhap, "Vui lòng nhập tên đăng nhập!");
                return false;
            }

            if (string.IsNullOrEmpty(password) || password == "••••••")
            {
                ShowValidationError(txtMatKhau, "Vui lòng nhập mật khẩu!");
                return false;
            }

            return true;
        }

        private void ShowValidationError(Control control, string message)
        {
            control.Focus();
            MessageBox.Show(message, "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void LoginSuccessful(string fullName, string username, int maNguoiDung, int maChiNhanh)
        {
            // Hiệu ứng thành công
            pnlLogin.BackColor = Color.FromArgb(240, 255, 240);
            Application.DoEvents();
            System.Threading.Thread.Sleep(300);
            pnlLogin.BackColor = Color.White;

            // Xác định vai trò và chi nhánh
            string userRole = DetermineUserRole(username);
            string branch = DetermineUserBranch(username, maChiNhanh);

            // Lưu thông tin đăng nhập
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
            // Hiệu ứng lỗi
            txtMatKhau.BackColor = Color.FromArgb(255, 240, 240);
            System.Threading.Thread.Sleep(200);
            txtMatKhau.BackColor = Color.White;

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
                txtMatKhau.SelectAll();
                e.Handled = true;
            }
        }

        // Native methods for form dragging
        internal class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
        }
    }

    // Form quên mật khẩu với thiết kế mới
    public class frmForgotPassword : Form
    {
        public frmForgotPassword()
        {
            this.Text = "Quên mật khẩu";
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(30);

            // Title
            Label lblTitle = new Label();
            lblTitle.Text = "QUÊN MẬT KHẨU";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(220, 0, 0);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 50;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Info
            Label lblInfo = new Label();
            lblInfo.Text = "Nếu bạn quên mật khẩu, vui lòng liên hệ:\n\n" +
                          "📧 Email: admin@cgv.vn\n" +
                          "📞 Hotline: 1900 6017\n\n" +
                          "Hoặc đến quầy hỗ trợ tại chi nhánh gần nhất.";
            lblInfo.Font = new Font("Segoe UI", 11);
            lblInfo.ForeColor = Color.FromArgb(80, 80, 80);
            lblInfo.Dock = DockStyle.Fill;
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            lblInfo.Padding = new Padding(0, 10, 0, 10);

            // Button panel
            Panel pnlButtons = new Panel();
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Height = 50;
            pnlButtons.BackColor = Color.Transparent;

            Button btnClose = new Button();
            btnClose.Text = "ĐÓNG";
            btnClose.Size = new Size(120, 35);
            btnClose.Location = new Point(270, 10);
            btnClose.BackColor = Color.FromArgb(220, 0, 0);
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => this.Close();

            pnlButtons.Controls.Add(btnClose);

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblInfo);
            this.Controls.Add(pnlButtons);
        }
    }
}
