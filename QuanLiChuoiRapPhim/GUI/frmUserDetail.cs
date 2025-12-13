using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Drawing;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class frmUserDetail : Form
    {
        private AdminBLL adminBLL = new AdminBLL();
        private AdminDAL adminDAL = new AdminDAL();
        private UserBLL userBLL = new UserBLL();

        private int? userId = null; // null = thêm mới, có giá trị = sửa
        private DataTable dtBranches;

        // Controls
        private TextBox txtUsername, txtPassword, txtConfirmPassword, txtFullName, txtEmail, txtPhone;
        private ComboBox cboRole, cboBranch, cboStatus;
        private Button btnSave, btnCancel;
        private CheckBox chkShowPassword;
        private Label lblTitle;

        public frmUserDetail(int? userId = null)
        {
            this.userId = userId;
            InitializeForm();
            LoadData();
        }

        private void InitializeForm()
        {
            this.Text = userId.HasValue ? "SỬA THÔNG TIN NGƯỜI DÙNG" : "THÊM NGƯỜI DÙNG MỚI";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ========== TIÊU ĐỀ ==========
            lblTitle = new Label();
            lblTitle.Text = this.Text;
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 170, 255);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 60;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // ========== PANEL CHÍNH ==========
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(30, 10, 30, 10);
            mainPanel.BackColor = Color.White;

            int yPos = 20;
            int labelWidth = 150;
            int controlWidth = 350;

            // Tên đăng nhập
            Label lblUsername = CreateLabel("Tên đăng nhập *:", 20, yPos);
            txtUsername = CreateTextBox(180, yPos, controlWidth);
            txtUsername.MaxLength = 50;
            mainPanel.Controls.AddRange(new Control[] { lblUsername, txtUsername });
            yPos += 50;

            // Mật khẩu
            Label lblPassword = CreateLabel("Mật khẩu *:", 20, yPos);
            txtPassword = CreateTextBox(180, yPos, controlWidth);
            txtPassword.UseSystemPasswordChar = true;
            mainPanel.Controls.AddRange(new Control[] { lblPassword, txtPassword });
            yPos += 50;

            // Xác nhận mật khẩu
            Label lblConfirmPassword = CreateLabel("Xác nhận mật khẩu *:", 20, yPos);
            txtConfirmPassword = CreateTextBox(180, yPos, controlWidth);
            txtConfirmPassword.UseSystemPasswordChar = true;
            mainPanel.Controls.AddRange(new Control[] { lblConfirmPassword, txtConfirmPassword });
            yPos += 40;

            // Hiện mật khẩu
            chkShowPassword = new CheckBox();
            chkShowPassword.Text = "Hiện mật khẩu";
            chkShowPassword.Font = new Font("Segoe UI", 10);
            chkShowPassword.Location = new Point(180, yPos);
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;
            mainPanel.Controls.Add(chkShowPassword);
            yPos += 50;

            // Họ tên
            Label lblFullName = CreateLabel("Họ và tên *:", 20, yPos);
            txtFullName = CreateTextBox(180, yPos, controlWidth);
            txtFullName.MaxLength = 200;
            mainPanel.Controls.AddRange(new Control[] { lblFullName, txtFullName });
            yPos += 50;

            // Email
            Label lblEmail = CreateLabel("Email:", 20, yPos);
            txtEmail = CreateTextBox(180, yPos, controlWidth);
            txtEmail.MaxLength = 100;
            mainPanel.Controls.AddRange(new Control[] { lblEmail, txtEmail });
            yPos += 50;

            // Số điện thoại
            Label lblPhone = CreateLabel("Số điện thoại:", 20, yPos);
            txtPhone = CreateTextBox(180, yPos, controlWidth);
            txtPhone.MaxLength = 20;
            mainPanel.Controls.AddRange(new Control[] { lblPhone, txtPhone });
            yPos += 50;

            // Vai trò
            Label lblRole = CreateLabel("Vai trò *:", 20, yPos);
            cboRole = new ComboBox();
            cboRole.Font = new Font("Segoe UI", 10);
            cboRole.Size = new Size(controlWidth, 30);
            cboRole.Location = new Point(180, yPos);
            cboRole.Items.AddRange(new string[] { "Admin", "Quản lý", "Nhân viên" });
            cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            mainPanel.Controls.AddRange(new Control[] { lblRole, cboRole });
            yPos += 50;

            // Chi nhánh
            Label lblBranch = CreateLabel("Chi nhánh:", 20, yPos);
            cboBranch = new ComboBox();
            cboBranch.Font = new Font("Segoe UI", 10);
            cboBranch.Size = new Size(controlWidth, 30);
            cboBranch.Location = new Point(180, yPos);
            cboBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            mainPanel.Controls.AddRange(new Control[] { lblBranch, cboBranch });
            yPos += 50;

            // Trạng thái
            Label lblStatus = CreateLabel("Trạng thái *:", 20, yPos);
            cboStatus = new ComboBox();
            cboStatus.Font = new Font("Segoe UI", 10);
            cboStatus.Size = new Size(controlWidth, 30);
            cboStatus.Location = new Point(180, yPos);
            cboStatus.Items.AddRange(new string[] { "Đang hoạt động", "Đã khóa" });
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.SelectedIndex = 0;
            mainPanel.Controls.AddRange(new Control[] { lblStatus, cboStatus });
            yPos += 70;

            // ========== NÚT LƯU VÀ HỦY ==========
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 70;
            buttonPanel.BackColor = Color.FromArgb(248, 249, 250);

            btnSave = new Button();
            btnSave.Text = "💾 LƯU";
            btnSave.Size = new Size(120, 40);
            btnSave.Location = new Point(150, 15);
            btnSave.BackColor = Color.FromArgb(40, 167, 69);
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "❌ HỦY";
            btnCancel.Size = new Size(120, 40);
            btnCancel.Location = new Point(300, 15);
            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.ForeColor = Color.White;
            btnCancel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            // Thêm các panel vào form
            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(lblTitle);
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(x, y),
                Size = new Size(150, 30),
                TextAlign = ContentAlignment.MiddleRight
            };
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(x, y),
                Size = new Size(width, 35),
                BackColor = Color.White
            };
        }

        private void LoadData()
        {
            // Load danh sách chi nhánh
            dtBranches = userBLL.GetBranches();
            cboBranch.Items.Clear();
            cboBranch.Items.Add("-- Chọn chi nhánh --");

            foreach (DataRow row in dtBranches.Rows)
            {
                cboBranch.Items.Add(row["TenChiNhanh"]);
            }
            cboBranch.SelectedIndex = 0;

            // Nếu là sửa, load thông tin người dùng
            if (userId.HasValue)
            {
                try
                {
                    DataRow user = userBLL.GetUserById(userId.Value);

                    txtUsername.Text = user["TenDangNhap"].ToString();
                    txtUsername.Enabled = false; // Không cho sửa tên đăng nhập
                    txtFullName.Text = user["HoTen"].ToString();
                    txtEmail.Text = user["Email"].ToString();
                    txtPhone.Text = user["SoDienThoai"].ToString();

                    string role = user["VaiTro"].ToString();
                    cboRole.SelectedItem = role;

                    string branchName = user["TenChiNhanh"].ToString();
                    if (!string.IsNullOrEmpty(branchName))
                    {
                        cboBranch.SelectedItem = branchName;
                    }

                    bool status = Convert.ToBoolean(user["TrangThai"]);
                    cboStatus.SelectedIndex = status ? 0 : 1;

                    AddPasswordHintLabels();
                 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi load dữ liệu: {ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }

            }
        }

        private void AddPasswordHintLabels()
        {
            // Thêm label để thay thế PlaceholderText
            Label lblPasswordHint = new Label();
            lblPasswordHint.Text = "Để trống nếu không đổi mật khẩu";
            lblPasswordHint.Font = new Font("Segoe UI", 8);
            lblPasswordHint.ForeColor = Color.Gray;
            lblPasswordHint.Location = new Point(180, txtPassword.Location.Y + 35);
            lblPasswordHint.Size = new Size(200, 20);

            Label lblConfirmHint = new Label();
            lblConfirmHint.Text = "Để trống nếu không đổi mật khẩu";
            lblConfirmHint.Font = new Font("Segoe UI", 8);
            lblConfirmHint.ForeColor = Color.Gray;
            lblConfirmHint.Location = new Point(180, txtConfirmPassword.Location.Y + 35);
            lblConfirmHint.Size = new Size(200, 20);

            // Tìm và thêm vào panel chính
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel panel && panel.Dock == DockStyle.Fill)
                {
                    panel.Controls.Add(lblPasswordHint);
                    panel.Controls.Add(lblConfirmHint);
                    break;
                }
            }
        }

        private void ChkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            txtConfirmPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Lấy thông tin từ form
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;
                string fullname = txtFullName.Text.Trim();
                string email = txtEmail.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string role = cboRole.SelectedItem.ToString();
                string branchName = cboBranch.SelectedItem?.ToString();
                bool status = cboStatus.SelectedIndex == 0;

                // Lấy mã chi nhánh
                int? branchId = null;
                if (branchName != "-- Chọn chi nhánh --" && !string.IsNullOrEmpty(branchName))
                {
                    foreach (DataRow row in dtBranches.Rows)
                    {
                        if (row["TenChiNhanh"].ToString() == branchName)
                        {
                            branchId = Convert.ToInt32(row["MaChiNhanh"]);
                            break;
                        }
                    }
                }

                if (userId.HasValue)
                {
                    // Cập nhật người dùng
                    bool success = adminBLL.UpdateUser(userId.Value, fullname, email, phone,
                        role, branchId, status, password);

                    if (success)
                    {
                        MessageBox.Show("Cập nhật người dùng thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    // Thêm người dùng mới
                    bool success = adminBLL.AddUser(username, password, fullname,
                        email, phone, role, branchId, status);

                    if (success)
                    {
                        MessageBox.Show("Thêm người dùng mới thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            // Kiểm tra tên đăng nhập
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            // Kiểm tra mật khẩu (chỉ bắt buộc khi thêm mới)
            if (!userId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Mật khẩu xác nhận không khớp!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return false;
                }
            }

            // Kiểm tra họ tên
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            // Kiểm tra email
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                    if (addr.Address != txtEmail.Text)
                    {
                        MessageBox.Show("Email không hợp lệ!", "Cảnh báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtEmail.Focus();
                        return false;
                    }
                }
                catch
                {
                    MessageBox.Show("Email không hợp lệ!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }
    }
}