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

        // Màu sắc theme mới - Minimalist
        private Color primaryColor = Color.FromArgb(41, 128, 185);     // Xanh dương nhẹ
        private Color secondaryColor = Color.FromArgb(52, 152, 219);   // Xanh dương sáng
        private Color darkColor = Color.FromArgb(44, 62, 80);          // Xanh đậm
        private Color lightColor = Color.FromArgb(236, 240, 241);      // Xám nhạt
        private Color successColor = Color.FromArgb(46, 204, 113);     // Xanh lá
        private Color errorColor = Color.FromArgb(231, 76, 60);        // Đỏ nhạt

        public frmLogin()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Form settings
            this.Text = "Đăng Nhập - CGV Cinema";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(0);
            this.DoubleBuffered = true;

            // Main container với shadow effect
            Panel mainContainer = new Panel();
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.BackColor = Color.White;

            // Tạo bóng đổ cho container
            mainContainer.Paint += (s, e) =>
            {
                using (GraphicsPath path = CreateRoundedRectangle(mainContainer.ClientRectangle, 10))
                {
                    using (Pen shadowPen = new Pen(Color.FromArgb(30, 0, 0, 0), 2))
                    {
                        e.Graphics.DrawPath(shadowPen, path);
                    }
                }
            };

            // Panel trái (Hình ảnh minimalist)
            pnlLeft = new Panel();
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Width = 450;
            pnlLeft.BackColor = darkColor;
            pnlLeft.Paint += PnlLeft_Paint;

            // Panel phải (Login form)
            pnlLogin = new Panel();
            pnlLogin.Dock = DockStyle.Fill;
            pnlLogin.BackColor = Color.White;
            pnlLogin.Padding = new Padding(60, 50, 60, 50);

            // ========== PHẦN TRÁI ==========
            // Logo CGV minimalist
            picLogo = new PictureBox();
            picLogo.Image = CreateMinimalistLogo();
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.Location = new Point(75, 80);
            picLogo.Size = new Size(300, 100);
            pnlLeft.Controls.Add(picLogo);

            // Slogan minimalist
            Label lblSlogan = new Label();
            lblSlogan.Text = "CINEMA EXPERIENCE";
            lblSlogan.Font = new Font("Segoe UI Light", 18, FontStyle.Regular);
            lblSlogan.ForeColor = Color.White;
            lblSlogan.Location = new Point(75, 200);
            lblSlogan.Size = new Size(300, 40);
            lblSlogan.TextAlign = ContentAlignment.MiddleCenter;
            pnlLeft.Controls.Add(lblSlogan);

            // Separator line
            Panel separatorLine = new Panel();
            separatorLine.Size = new Size(100, 2);
            separatorLine.Location = new Point(175, 250);
            separatorLine.BackColor = primaryColor;
            pnlLeft.Controls.Add(separatorLine);

            // Welcome text
            Label lblWelcome = new Label();
            lblWelcome.Text = "Management System";
            lblWelcome.Font = new Font("Segoe UI", 11);
            lblWelcome.ForeColor = Color.FromArgb(180, 180, 180);
            lblWelcome.Location = new Point(75, 270);
            lblWelcome.Size = new Size(300, 25);
            lblWelcome.TextAlign = ContentAlignment.MiddleCenter;
            pnlLeft.Controls.Add(lblWelcome);

            // Decor element
            Panel decorCircle = new Panel();
            decorCircle.Size = new Size(80, 80);
            decorCircle.Location = new Point(185, 350);
            decorCircle.BackColor = Color.Transparent;
            decorCircle.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(100, 255, 255, 255), 2))
                {
                    e.Graphics.DrawEllipse(pen, 0, 0, 78, 78);
                }
                using (Pen pen = new Pen(primaryColor, 3))
                {
                    e.Graphics.DrawArc(pen, 0, 0, 78, 78, -45, 180);
                }
            };
            pnlLeft.Controls.Add(decorCircle);

            // ========== PHẦN LOGIN ==========
            int yPos = 20;

            // Title với hiệu ứng gradient
            Label lblTitle = new Label();
            lblTitle.Text = "WELCOME BACK";
            lblTitle.Font = new Font("Segoe UI", 32, FontStyle.Bold);
            lblTitle.ForeColor = darkColor;
            lblTitle.Location = new Point(0, yPos);
            lblTitle.Size = new Size(400, 70);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            pnlLogin.Controls.Add(lblTitle);
            yPos += 80;

            // Subtitle
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Please login to your account";
            lblSubtitle.Font = new Font("Segoe UI", 12);
            lblSubtitle.ForeColor = Color.FromArgb(150, 150, 150);
            lblSubtitle.Location = new Point(0, yPos);
            lblSubtitle.Size = new Size(400, 25);
            lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;
            pnlLogin.Controls.Add(lblSubtitle);
            yPos += 50;

            // Username field với icon
            (Panel userPanel, TextBox userTxtBox) = CreateModernInputField("Username", "Enter your username", yPos, false);
            txtTenDangNhap = userTxtBox;
            pnlLogin.Controls.Add(userPanel);
            yPos += 85;

            // Password field với icon
            (Panel passPanel, TextBox passTxtBox) = CreateModernInputField("Password", "Enter your password", yPos, true);
            txtMatKhau = passTxtBox;
            pnlLogin.Controls.Add(passPanel);
            yPos += 85;

            // Options panel
            Panel optionsPanel = new Panel();
            optionsPanel.Location = new Point(0, yPos);
            optionsPanel.Size = new Size(400, 30);
            optionsPanel.BackColor = Color.Transparent;

            chkHienMatKhau = new CheckBox();
            chkHienMatKhau.Text = "Show password";
            chkHienMatKhau.Font = new Font("Segoe UI", 10);
            chkHienMatKhau.ForeColor = Color.FromArgb(100, 100, 100);
            chkHienMatKhau.Location = new Point(0, 0);
            chkHienMatKhau.Size = new Size(150, 25);
            chkHienMatKhau.CheckedChanged += ChkHienMatKhau_CheckedChanged;

            lblQuenMatKhau = new Label();
            lblQuenMatKhau.Text = "Forgot password?";
            lblQuenMatKhau.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblQuenMatKhau.ForeColor = primaryColor;
            lblQuenMatKhau.Location = new Point(250, 0);
            lblQuenMatKhau.Size = new Size(150, 25);
            lblQuenMatKhau.TextAlign = ContentAlignment.MiddleRight;
            lblQuenMatKhau.Cursor = Cursors.Hand;
            lblQuenMatKhau.Click += LblQuenMatKhau_Click;
            lblQuenMatKhau.MouseEnter += (s, e) => lblQuenMatKhau.ForeColor = secondaryColor;
            lblQuenMatKhau.MouseLeave += (s, e) => lblQuenMatKhau.ForeColor = primaryColor;

            optionsPanel.Controls.Add(chkHienMatKhau);
            optionsPanel.Controls.Add(lblQuenMatKhau);
            pnlLogin.Controls.Add(optionsPanel);
            yPos += 50;

            // Login button với gradient
            btnDangNhap = CreateGradientButton("SIGN IN", primaryColor, secondaryColor, yPos);
            btnDangNhap.Click += BtnDangNhap_Click;
            pnlLogin.Controls.Add(btnDangNhap);
            yPos += 70;

            // Separator với text
            Panel separatorContainer = new Panel();
            separatorContainer.Location = new Point(0, yPos);
            separatorContainer.Size = new Size(400, 30);
            separatorContainer.BackColor = Color.Transparent;

            Panel line1 = new Panel();
            line1.Size = new Size(170, 1);
            line1.Location = new Point(0, 15);
            line1.BackColor = Color.FromArgb(230, 230, 230);

            Label lblOr = new Label();
            lblOr.Text = "or try demo accounts";
            lblOr.Font = new Font("Segoe UI", 10);
            lblOr.ForeColor = Color.FromArgb(150, 150, 150);
            lblOr.Location = new Point(170, 0);
            lblOr.Size = new Size(60, 30);
            lblOr.TextAlign = ContentAlignment.MiddleCenter;

            Panel line2 = new Panel();
            line2.Size = new Size(170, 1);
            line2.Location = new Point(230, 15);
            line2.BackColor = Color.FromArgb(230, 230, 230);

            separatorContainer.Controls.Add(line1);
            separatorContainer.Controls.Add(lblOr);
            separatorContainer.Controls.Add(line2);
            pnlLogin.Controls.Add(separatorContainer);
            yPos += 40;

            // Demo accounts buttons
            AddDemoAccountButton("Administrator", "admin", "123456", yPos, primaryColor);
            yPos += 40;
            AddDemoAccountButton("Branch Manager", "ql_cn1", "123456", yPos, successColor);
            yPos += 40;
            AddDemoAccountButton("Ticket Staff", "nv_ve01", "123456", yPos, Color.FromArgb(155, 89, 182));
            yPos += 60;

            // Exit button
            btnThoat = CreateModernButton("EXIT", Color.FromArgb(150, 150, 150), yPos);
            btnThoat.Click += BtnThoat_Click;
            pnlLogin.Controls.Add(btnThoat);

            // Add panels to form
            mainContainer.Controls.Add(pnlLogin);
            mainContainer.Controls.Add(pnlLeft);
            this.Controls.Add(mainContainer);

            // Add drag functionality for borderless form
            AddDragControl(this);
            AddDragControl(pnlLeft);

            // Add close button
            AddCloseButton();
        }

        private void PnlLeft_Paint(object sender, PaintEventArgs e)
        {
            // Vẽ gradient background tối giản
            using (LinearGradientBrush brush = new LinearGradientBrush(
                pnlLeft.ClientRectangle,
                Color.FromArgb(44, 62, 80),
                Color.FromArgb(52, 73, 94),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, pnlLeft.ClientRectangle);
            }

            // Vẽ pattern dots tinh tế
            using (Pen pen = new Pen(Color.FromArgb(30, 255, 255, 255), 1))
            {
                for (int x = 20; x < pnlLeft.Width; x += 40)
                {
                    for (int y = 20; y < pnlLeft.Height; y += 40)
                    {
                        e.Graphics.DrawEllipse(pen, x, y, 2, 2);
                    }
                }
            }
        }

        private (Panel, TextBox) CreateModernInputField(string labelText, string placeholder, int yPos, bool isPassword)
        {
            Panel panel = new Panel();
            panel.Location = new Point(0, yPos);
            panel.Size = new Size(400, 70);

            // Label
            Label label = new Label();
            label.Text = labelText;
            label.Font = new Font("Segoe UI Semibold", 11);
            label.ForeColor = darkColor;
            label.Location = new Point(0, 0);
            label.Size = new Size(400, 20);
            panel.Controls.Add(label);

            // TextBox với border và icon
            Panel inputContainer = new Panel();
            inputContainer.Location = new Point(0, 25);
            inputContainer.Size = new Size(400, 45);
            inputContainer.BackColor = Color.White;
            inputContainer.BorderStyle = BorderStyle.None;
            inputContainer.Padding = new Padding(0);

            // Vẽ border rounded
            inputContainer.Paint += (s, e) =>
            {
                using (GraphicsPath path = CreateRoundedRectangle(inputContainer.ClientRectangle, 8))
                {
                    using (Pen borderPen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            };

            // Icon
            PictureBox icon = new PictureBox();
            icon.Size = new Size(20, 20);
            icon.Location = new Point(15, 12);
            icon.Image = isPassword ? CreatePasswordIcon() : CreateUserIcon();
            icon.SizeMode = PictureBoxSizeMode.Zoom;

            TextBox textBox = new TextBox();
            textBox.Location = new Point(45, 10);
            textBox.Size = new Size(340, 25);
            textBox.Font = new Font("Segoe UI", 11);
            textBox.BorderStyle = BorderStyle.None;
            textBox.Text = placeholder;
            textBox.ForeColor = Color.FromArgb(150, 150, 150);
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
                if (textBox.Text == placeholder)
                    textBox.Text = "";
                textBox.ForeColor = darkColor;
                inputContainer.Invalidate(); // Redraw border
            };

            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrEmpty(textBox.Text))
                    textBox.Text = placeholder;
                inputContainer.Invalidate(); // Redraw border
            };

            textBox.TextChanged += (s, e) =>
            {
                // Thay đổi màu border khi có text
                inputContainer.Invalidate();
            };

            inputContainer.Controls.Add(icon);
            inputContainer.Controls.Add(textBox);
            panel.Controls.Add(inputContainer);

            return (panel, textBox);
        }

        private Button CreateGradientButton(string text, Color startColor, Color endColor, int yPos)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            button.Location = new Point(0, yPos);
            button.Size = new Size(400, 50);
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(0);

            // Rounded corners
            button.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button.Width, button.Height, 10, 10));

            // Vẽ gradient background
            button.Paint += (s, e) =>
            {
                using (GraphicsPath path = CreateRoundedRectangle(button.ClientRectangle, 10))
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    button.ClientRectangle,
                    startColor,
                    endColor,
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Draw text
                using (StringFormat sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    e.Graphics.DrawString(text, button.Font, textBrush,
                        new RectangleF(0, 0, button.Width, button.Height), sf);
                }
            };

            // Hover effects
            button.MouseEnter += (s, e) =>
            {
                button.Invalidate();
            };

            button.MouseLeave += (s, e) =>
            {
                button.Invalidate();
            };

            return button;
        }

        private Button CreateModernButton(string text, Color color, int yPos)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Segoe UI", 11);
            button.Location = new Point(0, yPos);
            button.Size = new Size(400, 45);
            button.BackColor = Color.White;
            button.ForeColor = color;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            button.FlatAppearance.BorderSize = 1;
            button.Cursor = Cursors.Hand;
            button.TextAlign = ContentAlignment.MiddleCenter;

            // Rounded corners
            button.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button.Width, button.Height, 8, 8));

            // Hover effects
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = Color.FromArgb(250, 250, 250);
                button.FlatAppearance.BorderColor = color;
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = Color.White;
                button.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            };

            return button;
        }

        private void AddDemoAccountButton(string displayName, string username, string password, int yPos, Color color)
        {
            Panel panel = new Panel();
            panel.Location = new Point(0, yPos);
            panel.Size = new Size(400, 35);
            panel.BackColor = Color.White;
            panel.Cursor = Cursors.Hand;

            // Border rounded
            panel.Paint += (s, e) =>
            {
                using (GraphicsPath path = CreateRoundedRectangle(panel.ClientRectangle, 6))
                {
                    using (Pen borderPen = new Pen(Color.FromArgb(230, 230, 230), 1))
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            };

            // Icon
            Label lblIcon = new Label();
            lblIcon.Text = "▶";
            lblIcon.Font = new Font("Segoe UI", 9);
            lblIcon.ForeColor = color;
            lblIcon.Location = new Point(15, 0);
            lblIcon.Size = new Size(20, 35);
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;

            // Text
            Label lblText = new Label();
            lblText.Text = displayName;
            lblText.Font = new Font("Segoe UI", 10);
            lblText.ForeColor = darkColor;
            lblText.Location = new Point(45, 0);
            lblText.Size = new Size(300, 35);
            lblText.TextAlign = ContentAlignment.MiddleLeft;

            // Username hint
            Label lblHint = new Label();
            lblHint.Text = $"{username} / {password}";
            lblHint.Font = new Font("Segoe UI", 8);
            lblHint.ForeColor = Color.FromArgb(150, 150, 150);
            lblHint.Location = new Point(200, 0);
            lblHint.Size = new Size(195, 35);
            lblHint.TextAlign = ContentAlignment.MiddleRight;

            // Hover effect
            panel.MouseEnter += (s, e) =>
            {
                panel.BackColor = Color.FromArgb(248, 248, 248);
                lblIcon.ForeColor = Color.FromArgb(200, 200, 200);
                panel.Invalidate();
            };

            panel.MouseLeave += (s, e) =>
            {
                panel.BackColor = Color.White;
                lblIcon.ForeColor = color;
                panel.Invalidate();
            };

            // Click event
            panel.Click += (s, e) =>
            {
                txtTenDangNhap.Text = username;
                txtMatKhau.Text = password;
                PerformLogin();
            };

            panel.Controls.Add(lblIcon);
            panel.Controls.Add(lblText);
            panel.Controls.Add(lblHint);
            pnlLogin.Controls.Add(panel);
        }

        private Image CreateMinimalistLogo()
        {
            Bitmap bmp = new Bitmap(300, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Vẽ logo CGV minimalist
                using (Font font = new Font("Segoe UI Light", 48, FontStyle.Regular))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    g.DrawString("CGV", font, brush, 0, 20);
                }

                // Line accent
                using (Pen pen = new Pen(primaryColor, 3))
                {
                    g.DrawLine(pen, 0, 85, 120, 85);
                }
            }
            return bmp;
        }

        private Image CreateUserIcon()
        {
            Bitmap bmp = new Bitmap(20, 20);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Pen pen = new Pen(Color.FromArgb(150, 150, 150), 2))
                {
                    g.DrawEllipse(pen, 2, 2, 16, 16);
                    g.DrawLine(pen, 10, 10, 10, 18);
                }
            }
            return bmp;
        }

        private Image CreatePasswordIcon()
        {
            Bitmap bmp = new Bitmap(20, 20);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Pen pen = new Pen(Color.FromArgb(150, 150, 150), 2))
                {
                    // Lock body
                    g.DrawRectangle(pen, 4, 8, 12, 10);
                    g.DrawArc(pen, 4, 4, 12, 8, 0, 180);

                    // Keyhole
                    g.FillEllipse(Brushes.White, 8, 12, 4, 4);
                }
            }
            return bmp;
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void AddCloseButton()
        {
            Button btnClose = new Button();
            btnClose.Text = "✕";
            btnClose.Font = new Font("Segoe UI", 14, FontStyle.Regular);
            btnClose.Size = new Size(40, 40);
            btnClose.Location = new Point(this.Width - 50, 10);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.Transparent;
            btnClose.ForeColor = Color.FromArgb(150, 150, 150);
            btnClose.Cursor = Cursors.Hand;

            // Rounded circle
            btnClose.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnClose.Width, btnClose.Height, 20, 20));

            btnClose.MouseEnter += (s, e) =>
            {
                btnClose.ForeColor = errorColor;
                btnClose.BackColor = Color.FromArgb(240, 240, 240);
            };

            btnClose.MouseLeave += (s, e) =>
            {
                btnClose.ForeColor = Color.FromArgb(150, 150, 150);
                btnClose.BackColor = Color.Transparent;
            };

            btnClose.Click += (s, e) => ExitApplication();

            this.Controls.Add(btnClose);
            btnClose.BringToFront();
        }

        // Giữ nguyên các phương thức khác không thay đổi về logic
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

            btnDangNhap.Text = "PROCESSING...";
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
                ShowError($"System error: {ex.Message}");
            }
            finally
            {
                btnDangNhap.Text = "SIGN IN";
                btnDangNhap.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private bool ValidateInput(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || username == "Enter your username")
            {
                ShowValidationError(txtTenDangNhap, "Please enter username!");
                return false;
            }

            if (string.IsNullOrEmpty(password) || password == "Enter your password")
            {
                ShowValidationError(txtMatKhau, "Please enter password!");
                return false;
            }

            return true;
        }

        private void ShowValidationError(Control control, string message)
        {
            control.Focus();
            MessageBox.Show(message, "Notification",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void LoginSuccessful(string fullName, string username, int maNguoiDung, int maChiNhanh)
        {
            // Hiệu ứng thành công
            pnlLogin.BackColor = Color.FromArgb(245, 255, 245);
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
                return "Administrator";
            else if (username.StartsWith("ql_"))
                return "Branch Manager";
            else if (username.StartsWith("nv_"))
            {
                if (username.Contains("_ve"))
                    return "Ticket Staff";
                else if (username.Contains("_sc"))
                    return "Showtime Staff";
                else
                    return "Staff";
            }
            else
                return "User";
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
                default: return "System-wide";
            }
        }

        private void ShowErrorMessage(string message)
        {
            // Hiệu ứng lỗi
            txtMatKhau.BackColor = Color.FromArgb(255, 245, 245);
            System.Threading.Thread.Sleep(200);
            txtMatKhau.BackColor = Color.White;

            MessageBox.Show(message, "Login Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtMatKhau.Focus();
            txtMatKhau.SelectAll();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "System Error",
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
            if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit",
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

        internal class NativeMethods
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
            this.Text = "Forgot Password";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.Padding = new Padding(30);

            // Panel container với shadow
            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.BackColor = Color.White;
            container.Padding = new Padding(20);

            // Title
            Label lblTitle = new Label();
            lblTitle.Text = "Need Help?";
            lblTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 50;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Info text
            Label lblInfo = new Label();
            lblInfo.Text = "If you forgot your password, please contact:\n\n" +
                          "📧 Email: support@cgv.vn\n" +
                          "📞 Hotline: 1900 6017\n\n" +
                          "Or visit the support desk at your nearest branch.";
            lblInfo.Font = new Font("Segoe UI", 11);
            lblInfo.ForeColor = Color.FromArgb(100, 100, 100);
            lblInfo.Dock = DockStyle.Fill;
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            lblInfo.Padding = new Padding(0, 10, 0, 10);

            // Close button
            Button btnClose = new Button();
            btnClose.Text = "CLOSE";
            btnClose.Size = new Size(120, 40);
            btnClose.BackColor = Color.FromArgb(44, 62, 80);
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Margin = new Padding(0, 20, 0, 0);
            btnClose.Click += (s, e) => this.Close();

            container.Controls.Add(lblTitle);
            container.Controls.Add(lblInfo);
            container.Controls.Add(btnClose);
            this.Controls.Add(container);

            // Close button for form
            Button btnFormClose = new Button();
            btnFormClose.Text = "×";
            btnFormClose.Font = new Font("Segoe UI", 14);
            btnFormClose.Size = new Size(40, 40);
            btnFormClose.Location = new Point(this.Width - 50, 10);
            btnFormClose.FlatStyle = FlatStyle.Flat;
            btnFormClose.FlatAppearance.BorderSize = 0;
            btnFormClose.BackColor = Color.Transparent;
            btnFormClose.ForeColor = Color.FromArgb(150, 150, 150);
            btnFormClose.Cursor = Cursors.Hand;
            btnFormClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnFormClose);
            btnFormClose.BringToFront();
        }
    }
}