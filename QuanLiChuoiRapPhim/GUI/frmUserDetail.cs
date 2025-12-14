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

        private int? userId = null; // null = thêm m?i, có giá tr? = s?a
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
            this.Text = userId.HasValue ? "S?A THÔNG TIN NGÝ?I DÙNG" : "THÊM NGÝ?I DÙNG M?I";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ========== TIÊU Ð? ==========
            lblTitle = new Label();
            lblTitle.Text = this.Text;
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 170, 255);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 60;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ========== PANEL CHÍNH ==========
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(30, 10, 30, 10);
            mainPanel.BackColor = Color.White;

            int yPos = 20;
            int labelWidth = 150;
            int controlWidth = 350;

            // Tên ðãng nh?p
            Label lblUsername = CreateLabel("Tên ðãng nh?p *:", 20, yPos);
            txtUsername = CreateTextBox(180, yPos, controlWidth);
            txtUsername.MaxLength = 50;
            mainPanel.Controls.AddRange(new Control[] { lblUsername, txtUsername });
            yPos += 50;

            // M?t kh?u
            Label lblPassword = CreateLabel("M?t kh?u *:", 20, yPos);
            txtPassword = CreateTextBox(180, yPos, controlWidth);
            txtPassword.UseSystemPasswordChar = true;
            mainPanel.Controls.AddRange(new Control[] { lblPassword, txtPassword });
            yPos += 50;

            // Xác nh?n m?t kh?u
            Label lblConfirmPassword = CreateLabel("Xác nh?n m?t kh?u *:", 20, yPos);
            txtConfirmPassword = CreateTextBox(180, yPos, controlWidth);
            txtConfirmPassword.UseSystemPasswordChar = true;
            mainPanel.Controls.AddRange(new Control[] { lblConfirmPassword, txtConfirmPassword });
            yPos += 40;

            // Hi?n m?t kh?u
            chkShowPassword = new CheckBox();
            chkShowPassword.Text = "Hi?n m?t kh?u";
            chkShowPassword.Font = new Font("Segoe UI", 10);
            chkShowPassword.Location = new Point(180, yPos);
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;
            mainPanel.Controls.Add(chkShowPassword);
            yPos += 50;

            // H? tên
            Label lblFullName = CreateLabel("H? và tên *:", 20, yPos);
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

            // S? ði?n tho?i
            Label lblPhone = CreateLabel("S? ði?n tho?i:", 20, yPos);
            txtPhone = CreateTextBox(180, yPos, controlWidth);
            txtPhone.MaxLength = 20;
            mainPanel.Controls.AddRange(new Control[] { lblPhone, txtPhone });
            yPos += 50;

            // Vai tr?
            Label lblRole = CreateLabel("Vai tr? *:", 20, yPos);
            cboRole = new ComboBox();
            cboRole.Font = new Font("Segoe UI", 10);
            cboRole.Size = new Size(controlWidth, 30);
            cboRole.Location = new Point(180, yPos);
            cboRole.Items.AddRange(new string[] { "Admin", "Qu?n l?", "Nhân viên" });
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

            // Tr?ng thái
            Label lblStatus = CreateLabel("Tr?ng thái *:", 20, yPos);
            cboStatus = new ComboBox();
            cboStatus.Font = new Font("Segoe UI", 10);
            cboStatus.Size = new Size(controlWidth, 30);
            cboStatus.Location = new Point(180, yPos);
            cboStatus.Items.AddRange(new string[] { "Ðang ho?t ð?ng", "Ð? khóa" });
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.SelectedIndex = 0;
            mainPanel.Controls.AddRange(new Control[] { lblStatus, cboStatus });
            yPos += 70;

            // ========== NÚT LÝU VÀ H?Y ==========
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 70;
            buttonPanel.BackColor = Color.FromArgb(248, 249, 250);

            btnSave = new Button();
            btnSave.Text = "?? LÝU";
            btnSave.Size = new Size(120, 40);
            btnSave.Location = new Point(150, 15);
            btnSave.BackColor = Color.FromArgb(40, 167, 69);
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "? H?Y";
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
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
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
            cboBranch.Items.Add("-- Ch?n chi nhánh --");

            foreach (DataRow row in dtBranches.Rows)
            {
                cboBranch.Items.Add(row["TenChiNhanh"]);
            }
            cboBranch.SelectedIndex = 0;

            // N?u là s?a, load thông tin ngý?i dùng
            if (userId.HasValue)
            {
                try
                {
                    DataRow user = userBLL.GetUserById(userId.Value);

                    txtUsername.Text = user["TenDangNhap"].ToString();
                    txtUsername.Enabled = false; // Không cho s?a tên ðãng nh?p
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
                    MessageBox.Show($"L?i load d? li?u: {ex.Message}", "L?i",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }

            }
        }

        private void AddPasswordHintLabels()
        {
            // Thêm label ð? thay th? PlaceholderText
            Label lblPasswordHint = new Label();
            lblPasswordHint.Text = "Ð? tr?ng n?u không ð?i m?t kh?u";
            lblPasswordHint.Font = new Font("Segoe UI", 8);
            lblPasswordHint.ForeColor = Color.Gray;
            lblPasswordHint.Location = new Point(180, txtPassword.Location.Y + 35);
            lblPasswordHint.Size = new Size(200, 20);

            Label lblConfirmHint = new Label();
            lblConfirmHint.Text = "Ð? tr?ng n?u không ð?i m?t kh?u";
            lblConfirmHint.Font = new Font("Segoe UI", 8);
            lblConfirmHint.ForeColor = Color.Gray;
            lblConfirmHint.Location = new Point(180, txtConfirmPassword.Location.Y + 35);
            lblConfirmHint.Size = new Size(200, 20);

            // T?m và thêm vào panel chính
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
                // L?y thông tin t? form
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;
                string fullname = txtFullName.Text.Trim();
                string email = txtEmail.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string role = cboRole.SelectedItem.ToString();
                string branchName = cboBranch.SelectedItem?.ToString();
                bool status = cboStatus.SelectedIndex == 0;

                // L?y m? chi nhánh
                int? branchId = null;
                if (branchName != "-- Ch?n chi nhánh --" && !string.IsNullOrEmpty(branchName))
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
                    // C?p nh?t ngý?i dùng
                    bool success = adminBLL.UpdateUser(userId.Value, fullname, email, phone,
                        role, branchId, status, password);

                    if (success)
                    {
                        MessageBox.Show("C?p nh?t ngý?i dùng thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    // Thêm ngý?i dùng m?i
                    bool success = adminBLL.AddUser(username, password, fullname,
                        email, phone, role, branchId, status);

                    if (success)
                    {
                        MessageBox.Show("Thêm ngý?i dùng m?i thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i: {ex.Message}", "L?i",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            // Ki?m tra tên ðãng nh?p
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui l?ng nh?p tên ðãng nh?p!", "C?nh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            // Ki?m tra m?t kh?u (ch? b?t bu?c khi thêm m?i)
            if (!userId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Vui l?ng nh?p m?t kh?u!", "C?nh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("M?t kh?u ph?i có ít nh?t 6 k? t?!", "C?nh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("M?t kh?u xác nh?n không kh?p!", "C?nh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return false;
                }
            }

            // Ki?m tra h? tên
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui l?ng nh?p h? và tên!", "C?nh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            // Ki?m tra email
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                    if (addr.Address != txtEmail.Text)
                    {
                        MessageBox.Show("Email không h?p l?!", "C?nh báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtEmail.Focus();
                        return false;
                    }
                }
                catch
                {
                    MessageBox.Show("Email không h?p l?!", "C?nh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }
    }
}
