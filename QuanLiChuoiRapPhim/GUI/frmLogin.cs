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

        // Màu sắc theme mới - CGV Branding
        private Color primaryColor = Color.FromArgb(227, 28, 36);     // Đỏ CGV
        private Color secondaryColor = Color.FromArgb(255, 60, 60);   // Đỏ sáng (gradient)
        private Color darkColor = Color.FromArgb(20, 20, 20);          // Đen đậm
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
            // Senior Refactor: Form settings for a premium feel
            this.Controls.Clear(); // Xóa bỏ hoàn toàn các control cũ từ Designer để tránh xung đột
            
            this.Text = "CGV Cinema Management - Authentication";
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(0);
            this.DoubleBuffered = true;
            this.ShowIcon = false;

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

            // Panel trái (Premium Branding)
            pnlLeft = new Panel();
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Width = 480; 
            pnlLeft.BackColor = Color.FromArgb(20, 20, 20);
            
            pnlLeft.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // 1. Background Gradient (Deep Red to Black - CGV Style)
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    pnlLeft.ClientRectangle,
                    Color.FromArgb(180, 20, 30), // Đỏ CGV đậm
                    Color.FromArgb(10, 10, 10),  // Đen sâu
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, pnlLeft.ClientRectangle);
                }

                // 2. Vẽ Line Texture cao cấp
                using (Pen texturePen = new Pen(Color.FromArgb(15, 255, 255, 255), 1))
                {
                    for (int i = 0; i < pnlLeft.Width + pnlLeft.Height; i += 60)
                        g.DrawLine(texturePen, 0, i, i, 0);
                }

                int currentY = 160;

                // 3. Render Logo cao cấp
                try {
                    string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", "logo.png");
                    if (!System.IO.File.Exists(logoPath))
                        logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "img", "logo.png");

                    if (System.IO.File.Exists(logoPath)) {
                        using (Image img = Image.FromFile(logoPath)) {
                            int logoW = 320;
                            int logoH = (img.Height * logoW) / img.Width;
                            g.DrawImage(img, (pnlLeft.Width - logoW) / 2, currentY, logoW, logoH);
                            currentY += logoH + 40;
                        }
                    } else {
                        using (Font logoFont = new Font("Segoe UI Black", 56, FontStyle.Bold))
                        {
                            g.DrawString("CGV", logoFont, Brushes.White, (pnlLeft.Width - 180) / 2, currentY);
                            currentY += 100;
                        }
                    }
                } catch { currentY += 120; }

                // 4. Slogan tối giản
                using (Font sloganFont = new Font("Segoe UI Light", 20))
                using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
                {
                    g.DrawString("TRẢI NGHIỆM ĐIỆN ẢNH", sloganFont, Brushes.White, 
                        new Rectangle(0, currentY, pnlLeft.Width, 50), sf);
                }

                // 5. Đường trang trí tối giản (Accent line)
                using (Pen accentPen = new Pen(primaryColor, 4))
                {
                    g.DrawLine(accentPen, pnlLeft.Width / 2 - 30, currentY + 60, pnlLeft.Width / 2 + 30, currentY + 60);
                }

                // 6. Footer Text
                using (Font footerFont = new Font("Segoe UI", 10))
                using (Brush footerBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
                using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center })
                {
                    g.DrawString("CINEMA CHAIN MANAGEMENT SYSTEM v2.0", footerFont, footerBrush, 
                        new RectangleF(0, pnlLeft.Height - 60, pnlLeft.Width, 30), sf);
                }
            };

            // Panel phải (Authentication Form)
            pnlLogin = new Panel();
            pnlLogin.Dock = DockStyle.Fill;
            pnlLogin.BackColor = Color.White;
            pnlLogin.Padding = new Padding(80, 60, 80, 60); // Padding rộng hơn để sang trọng

            int yPos = 30;

            // Title section
            Label lblTitle = new Label();
            lblTitle.Text = "HỆ THỐNG QUẢN LÝ";
            lblTitle.Font = new Font("Segoe UI Black", 28);
            lblTitle.ForeColor = darkColor;
            lblTitle.Location = new Point(80, yPos); 
            lblTitle.Size = new Size(500, 60);
            pnlLogin.Controls.Add(lblTitle);
            yPos += 70;

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Vui lòng nhập định danh để bắt đầu phiên làm việc";
            lblSubtitle.Font = new Font("Segoe UI", 12);
            lblSubtitle.ForeColor = Color.Gray;
            lblSubtitle.Location = new Point(80, yPos);
            lblSubtitle.Size = new Size(500, 25);
            pnlLogin.Controls.Add(lblSubtitle);
            yPos += 60;

            // Input Fields
            (Panel userPanel, TextBox userTxtBox) = CreateModernInputField("Tài khoản", "Tên đăng nhập hệ thống", yPos + 80, false);
            userPanel.Location = new Point(80, yPos);
            txtTenDangNhap = userTxtBox;
            pnlLogin.Controls.Add(userPanel);
            yPos += 100;

            (Panel passPanel, TextBox passTxtBox) = CreateModernInputField("Mật mã", "••••••••", yPos + 80, true);
            passPanel.Location = new Point(80, yPos);
            txtMatKhau = passTxtBox;
            pnlLogin.Controls.Add(passPanel);
            yPos += 90;

            // Options Area
            Panel optionsPanel = new Panel();
            optionsPanel.Location = new Point(80, yPos);
            optionsPanel.Size = new Size(420, 30);
            
            chkHienMatKhau = new CheckBox();
            chkHienMatKhau.Text = "Duy trì đăng nhập";
            chkHienMatKhau.Font = new Font("Segoe UI", 10);
            chkHienMatKhau.ForeColor = Color.DimGray;
            chkHienMatKhau.AutoSize = true;
            chkHienMatKhau.CheckedChanged += ChkHienMatKhau_CheckedChanged;
            
            lblQuenMatKhau = new Label();
            lblQuenMatKhau.Text = "Khôi phục mật khẩu";
            lblQuenMatKhau.Font = new Font("Segoe UI Semibold", 10);
            lblQuenMatKhau.ForeColor = primaryColor;
            lblQuenMatKhau.Size = new Size(150, 25);
            lblQuenMatKhau.Location = new Point(optionsPanel.Width - 150, 0);
            lblQuenMatKhau.TextAlign = ContentAlignment.MiddleRight;
            lblQuenMatKhau.Cursor = Cursors.Hand;
            lblQuenMatKhau.Click += LblQuenMatKhau_Click;

            optionsPanel.Controls.Add(chkHienMatKhau);
            optionsPanel.Controls.Add(lblQuenMatKhau);
            pnlLogin.Controls.Add(optionsPanel);
            yPos += 60;

            // Action Button
            btnDangNhap = CreateGradientButton("XÁC THỰC NGAY", primaryColor, secondaryColor, yPos);
            btnDangNhap.Location = new Point(80, yPos);
            btnDangNhap.Size = new Size(420, 55);
            btnDangNhap.Click += BtnDangNhap_Click;
            pnlLogin.Controls.Add(btnDangNhap);
            yPos += 80;

            // Quick Access Section
            Label lblQA = new Label();
            lblQA.Text = "TRUY CẬP NHANH (DÀNH CHO KIỂM THỬ)";
            lblQA.Font = new Font("Segoe UI Bold", 8);
            lblQA.ForeColor = Color.Silver;
            lblQA.Location = new Point(80, yPos);
            lblQA.Size = new Size(420, 20);
            pnlLogin.Controls.Add(lblQA);
            yPos += 30;

            Panel pnlQAButtons = new Panel();
            pnlQAButtons.Size = new Size(420, 40);
            pnlQAButtons.Location = new Point(80, yPos);
            
            AddQuickLoginButton(pnlQAButtons, "ADMIN", "admin", "123456", 0, darkColor);
            AddQuickLoginButton(pnlQAButtons, "MANAGER", "ql_cn1", "123456", 145, darkColor);
            AddQuickLoginButton(pnlQAButtons, "STAFF", "nv_ve01", "123456", 290, darkColor);
            
            pnlLogin.Controls.Add(pnlQAButtons);
            yPos += 55;

            // Nút Thoát (Gọn lại để vừa màn hình)
            btnThoat = CreateModernButton("THOÁT ỨNG DỤNG", Color.FromArgb(180, 180, 180), yPos);
            btnThoat.Height = 35;
            btnThoat.Width = 420;
            btnThoat.Location = new Point(80, yPos);
            btnThoat.Click += BtnThoat_Click;
            pnlLogin.Controls.Add(btnThoat);

            // Close button (Top right)
            Button btnClose = new Button();
            btnClose.Text = "✕";
            btnClose.Font = new Font("Segoe UI Semibold", 14);
            btnClose.ForeColor = Color.Silver;
            btnClose.Size = new Size(40, 40);
            btnClose.Location = new Point(1100 - 480 - 50, 10);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => Application.Exit();
            pnlLogin.Controls.Add(btnClose);

            // Add to Main Form
            this.Controls.Add(pnlLogin);
            this.Controls.Add(pnlLeft);

            AddDragControl(this);
            AddDragControl(pnlLeft);
        }

        // Removed Obsolete method as we draw directly in SetupUI now

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

            TextBox textBox = new TextBox();
            textBox.Location = new Point(45, 11);
            textBox.Size = new Size(340, 25);
            textBox.Font = new Font("Segoe UI", 12);
            textBox.BorderStyle = BorderStyle.None;
            textBox.Text = placeholder;
            textBox.ForeColor = Color.FromArgb(150, 150, 150);
            textBox.BackColor = Color.White;

            // Icon
            PictureBox icon = new PictureBox();
            icon.Size = new Size(20, 20);
            icon.Location = new Point(15, 12);
            icon.Image = isPassword ? CreatePasswordIcon() : CreateUserIcon();
            icon.SizeMode = PictureBoxSizeMode.Zoom;

            // Vẽ border rounded với màu đậm hơn để dễ nhìn
            inputContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Thu nhỏ rect một chút để đường line không bị clip mất
                Rectangle rect = inputContainer.ClientRectangle;
                rect.Width -= 1; rect.Height -= 1;
                
                using (GraphicsPath path = CreateRoundedRectangle(rect, 8))
                {
                    Color borderColor = textBox.Focused ? primaryColor : Color.FromArgb(180, 180, 180);
                    using (Pen borderPen = new Pen(borderColor, 1.5f))
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            };

            if (isPassword)
            {
                textBox.UseSystemPasswordChar = false; // Tắt che khi đang hiện placeholder
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
                {
                    textBox.Text = "";
                    if (isPassword) textBox.UseSystemPasswordChar = true;
                }
                textBox.ForeColor = darkColor;
                inputContainer.Invalidate(); // Vẽ lại border khi focus
            };

            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = placeholder;
                    if (isPassword) textBox.UseSystemPasswordChar = false;
                }
                inputContainer.Invalidate(); // Vẽ lại border khi mất focus
            };

            textBox.TextChanged += (s, e) =>
            {
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

                // Draw text dynamically based on current button text
                using (StringFormat sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    e.Graphics.DrawString(button.Text, button.Font, textBrush,
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

        private void AddQuickLoginButton(Panel parent, string text, string username, string password, int xPos, Color color)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btn.Size = new Size(125, 35);
            btn.Location = new Point(xPos, 0);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = color;
            btn.FlatAppearance.BorderSize = 1;
            btn.ForeColor = color;
            btn.BackColor = Color.White;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => { btn.BackColor = color; btn.ForeColor = Color.White; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.White; btn.ForeColor = color; };
            
            btn.Click += (s, e) =>
            {
                txtTenDangNhap.Text = username;
                txtMatKhau.Text = password;
                txtMatKhau.UseSystemPasswordChar = true;
                PerformLogin();
            };

            parent.Controls.Add(btn);
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
                    g.DrawLine(pen, 0, 85, 80, 85);
                }

                using (Font smallFont = new Font("Segoe UI", 12, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    g.DrawString("CẦN THƠ", smallFont, brush, 0, 0);
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
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Pen pen = new Pen(Color.FromArgb(150, 150, 150), 2))
                {
                    // Lock handle
                    g.DrawArc(pen, 7, 4, 10, 10, 180, 180);
                    
                    // Lock body
                    g.DrawRectangle(pen, 5, 10, 14, 10);
                    
                    // Keyhole
                    g.DrawLine(pen, 12, 13, 12, 17);
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
                ShowError($"System error: {ex.Message}");
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
            if (string.IsNullOrEmpty(username) || username == "Tên đăng nhập hệ thống" || username == "Nhập tên đăng nhập")
            {
                ShowValidationError(txtTenDangNhap, "Vui lòng nhập tên đăng nhập!");
                return false;
            }

            if (string.IsNullOrEmpty(password) || password == "••••••••" || password == "Nhập mật khẩu")
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
                return "Quản trị viên";
            else if (username.StartsWith("ql_"))
                return "Quản lý chi nhánh";
            else if (username.StartsWith("nv_"))
            {
                if (username.Contains("_ve"))
                    return "Nhân viên bán vé";
                else if (username.Contains("_sc"))
                    return "Nhân viên lịch chiếu";
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
                default: return "System-wide";
            }
        }

        private void ShowErrorMessage(string message)
        {
            // Hiệu ứng lỗi
            txtMatKhau.BackColor = Color.FromArgb(255, 245, 245);
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
            this.Text = "Quên mật khẩu";
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
            lblTitle.Text = "Cần hỗ trợ?";
            lblTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 50;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
 
            // Info text
            Label lblInfo = new Label();
            lblInfo.Text = "Nếu bạn quên mật khẩu, vui lòng liên hệ:\n\n" +
                          "📧 Email: support@cgv.vn\n" +
                          "📞 Hotline: 1900 6017\n\n" +
                          "Hoặc ghé quầy hỗ trợ tại chi nhánh gần nhất.";
            lblInfo.Font = new Font("Segoe UI", 11);
            lblInfo.ForeColor = Color.FromArgb(100, 100, 100);
            lblInfo.Dock = DockStyle.Fill;
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            lblInfo.Padding = new Padding(0, 10, 0, 10);
 
            // Close button
            Button btnClose = new Button();
            btnClose.Text = "ĐÓNG";
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