using QuanLiChuoiRapPhim.BLL;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class frmStaffDetail : Form
    {
        private StaffBLL staffBLL = new StaffBLL();
        private int? staffId = null;
        private DataTable dtBranches;

        // Controls
        private TextBox txtUsername, txtPassword, txtConfirmPassword, txtFullName, txtEmail, txtPhone;
        private ComboBox cboRole, cboBranch, cboStatus;
        private Button btnSave, btnCancel;
        private CheckBox chkShowPassword;
        private Label lblTitle;

        public frmStaffDetail(int? staffId = null)
        {
            this.staffId = staffId;
            InitializeForm();
            LoadData();
        }

        private void InitializeForm()
        {
            this.Text = staffId.HasValue ? "SỬA THÔNG TIN NHÂN VIÊN" : "THÊM NHÂN VIÊN MỚI";
            this.Size = new Size(600, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = this.Text,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Main Panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 10, 30, 10),
                BackColor = Color.White
            };

            int yPos = 20;
            int controlWidth = 350;

            // Username
            Label lblUsername = CreateLabel("Tên đăng nhập *:", 20, yPos);
            txtUsername = CreateTextBox(180, yPos, controlWidth);
            txtUsername.MaxLength = 50;
            mainPanel.Controls.AddRange(new Control[] { lblUsername, txtUsername });
            yPos += 50;

            // Password
            Label lblPassword = CreateLabel("Mật khẩu *:", 20, yPos);
            txtPassword = CreateTextBox(180, yPos, controlWidth);
            txtPassword.UseSystemPasswordChar = true;
            mainPanel.Controls.AddRange(new Control[] { lblPassword, txtPassword });
            yPos += 50;

            // Confirm Password
            Label lblConfirmPassword = CreateLabel("Xác nhận MK *:", 20, yPos);
            txtConfirmPassword = CreateTextBox(180, yPos, controlWidth);
            txtConfirmPassword.UseSystemPasswordChar = true;
            mainPanel.Controls.AddRange(new Control[] { lblConfirmPassword, txtConfirmPassword });
            yPos += 40;

            // Show password checkbox
            chkShowPassword = new CheckBox
            {
                Text = "Hiện mật khẩu",
                Font = new Font("Segoe UI", 10),
                Location = new Point(180, yPos)
            };
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
                txtConfirmPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };
            mainPanel.Controls.Add(chkShowPassword);
            yPos += 50;

            // Full Name
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

            // Phone
            Label lblPhone = CreateLabel("Số điện thoại:", 20, yPos);
            txtPhone = CreateTextBox(180, yPos, controlWidth);
            txtPhone.MaxLength = 20;
            mainPanel.Controls.AddRange(new Control[] { lblPhone, txtPhone });
            yPos += 50;

            // Role
            Label lblRole = CreateLabel("Vai trò *:", 20, yPos);
            cboRole = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(controlWidth, 30),
                Location = new Point(180, yPos),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRole.Items.AddRange(new string[] { "Quản Lý", "Nhân Viên" });
            cboRole.SelectedIndex = 1;
            mainPanel.Controls.AddRange(new Control[] { lblRole, cboRole });
            yPos += 50;

            // Branch
            Label lblBranch = CreateLabel("Chi nhánh:", 20, yPos);
            cboBranch = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(controlWidth, 30),
                Location = new Point(180, yPos),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.AddRange(new Control[] { lblBranch, cboBranch });
            yPos += 50;

            // Status
            Label lblStatus = CreateLabel("Trạng thái *:", 20, yPos);
            cboStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(controlWidth, 30),
                Location = new Point(180, yPos),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.AddRange(new string[] { "Đang hoạt động", "Đã khóa" });
            cboStatus.SelectedIndex = 0;
            mainPanel.Controls.AddRange(new Control[] { lblStatus, cboStatus });

            // Button Panel
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            btnSave = new Button
            {
                Text = "💾 LƯU",
                Size = new Size(120, 40),
                Location = new Point(150, 15),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "❌ HỦY",
                Size = new Size(120, 40),
                Location = new Point(300, 15),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

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
            // Load branches
            try
            {
                dtBranches = staffBLL.GetBranches();
                cboBranch.Items.Clear();
                cboBranch.Items.Add("-- Chọn chi nhánh --");

                foreach (DataRow row in dtBranches.Rows)
                {
                    cboBranch.Items.Add(row["TenChiNhanh"]);
                }
                cboBranch.SelectedIndex = 0;
            }
            catch
            {
                cboBranch.Items.Add("-- Không có chi nhánh --");
                cboBranch.SelectedIndex = 0;
            }

            // If editing, load staff info
            if (staffId.HasValue)
            {
                try
                {
                    DataRow staff = staffBLL.GetStaffById(staffId.Value);
                    if (staff != null)
                    {
                        txtUsername.Text = staff["TenDangNhap"].ToString();
                        txtUsername.Enabled = false;
                        txtFullName.Text = staff["HoTen"].ToString();
                        txtEmail.Text = staff["Email"]?.ToString() ?? "";
                        txtPhone.Text = staff["SoDienThoai"]?.ToString() ?? "";

                        string role = staff["VaiTro"].ToString();
                        for (int i = 0; i < cboRole.Items.Count; i++)
                        {
                            if (cboRole.Items[i].ToString() == role)
                            {
                                cboRole.SelectedIndex = i;
                                break;
                            }
                        }

                        if (staff["MaChiNhanh"] != DBNull.Value)
                        {
                            int branchId = Convert.ToInt32(staff["MaChiNhanh"]);
                            for (int i = 0; i < dtBranches.Rows.Count; i++)
                            {
                                if (Convert.ToInt32(dtBranches.Rows[i]["MaChiNhanh"]) == branchId)
                                {
                                    cboBranch.SelectedIndex = i + 1;
                                    break;
                                }
                            }
                        }

                        bool status = Convert.ToBoolean(staff["TrangThai"]);
                        cboStatus.SelectedIndex = status ? 0 : 1;

                        // Password hint for edit mode
                        Label lblHint = new Label
                        {
                            Text = "(Để trống nếu không đổi mật khẩu)",
                            Font = new Font("Segoe UI", 8, FontStyle.Italic),
                            ForeColor = Color.Gray,
                            Location = new Point(180, 115),
                            AutoSize = true
                        };
                        this.Controls[0].Controls.Add(lblHint);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi load dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            // Password validation for new staff
            if (!staffId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Mật khẩu xác nhận không khớp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(txtPassword.Text) && txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            try
            {
                int? branchId = null;
                if (cboBranch.SelectedIndex > 0 && dtBranches.Rows.Count > 0)
                {
                    branchId = Convert.ToInt32(dtBranches.Rows[cboBranch.SelectedIndex - 1]["MaChiNhanh"]);
                }

                bool isActive = cboStatus.SelectedIndex == 0;
                string role = cboRole.SelectedItem.ToString();

                if (staffId.HasValue)
                {
                    // Update
                    string newPassword = string.IsNullOrEmpty(txtPassword.Text) ? null : txtPassword.Text;
                    bool result = staffBLL.UpdateStaff(
                        staffId.Value,
                        txtFullName.Text.Trim(),
                        txtEmail.Text.Trim(),
                        txtPhone.Text.Trim(),
                        role,
                        branchId,
                        isActive,
                        newPassword
                    );

                    if (result)
                    {
                        MessageBox.Show("Cập nhật nhân viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    // Create
                    bool result = staffBLL.CreateStaff(
                        txtUsername.Text.Trim(),
                        txtPassword.Text,
                        txtFullName.Text.Trim(),
                        txtEmail.Text.Trim(),
                        txtPhone.Text.Trim(),
                        role,
                        branchId,
                        isActive
                    );

                    if (result)
                    {
                        MessageBox.Show("Thêm nhân viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

