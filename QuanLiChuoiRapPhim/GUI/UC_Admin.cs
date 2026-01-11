using QuanLiChuoiRapPhim.BLL;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_Admin : UserControl
    {
        private AdminBLL adminBLL = new AdminBLL();
        private DataTable dtUsers;

        // UI Components
        private DataGridView dgvUsers;
        private TextBox txtSearch;
        private ComboBox cboRoleFilter;
        private Label lblTotalStaff, lblActiveStaff, lblInactiveStaff;

        // Colors
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvTextColor = Color.FromArgb(33, 33, 33);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_Admin()
        {
            InitializeComponent();
            SetupModernUI();
            LoadUsers();
        }

        private void SetupModernUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);

            // 1. Header Section
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 0, 0, 15)
            };

            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ NHÂN SỰ",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            // Search Box (Modern Style)
            Panel searchContainer = new Panel
            {
                Size = new Size(350, 40),
                BackColor = Color.White,
                Location = new Point(this.Width - 380, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            searchContainer.BorderRadius(20);

            txtSearch = new TextBox
            {
                Text = "Tìm kiếm nhân viên...",
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Size = new Size(280, 25),
                Location = new Point(15, 10)
            };
            txtSearch.Enter += (s, e) => { if (txtSearch.Text == "Tìm kiếm nhân viên...") { txtSearch.Text = ""; txtSearch.ForeColor = _cgvTextColor; } };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrEmpty(txtSearch.Text)) { txtSearch.Text = "Tìm kiếm nhân viên..."; txtSearch.ForeColor = Color.Gray; } };
            txtSearch.TextChanged += (s, e) => FilterData();

            Label lblSearchIcon = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 12),
                Location = new Point(310, 8),
                Size = new Size(30, 30),
                Cursor = Cursors.Hand
            };

            searchContainer.Controls.AddRange(new Control[] { txtSearch, lblSearchIcon });
            headerPanel.Controls.AddRange(new Control[] { lblTitle, searchContainer });

            // 2. Stats Section
            Panel statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                Padding = new Padding(0, 10, 0, 25)
            };

            TableLayoutPanel statsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var card1 = CreateStatCard("TỔNG NHÂN SỰ", "0", "", Color.FromArgb(226, 26, 60), out lblTotalStaff);
            var card2 = CreateStatCard("ĐANG HOẠT ĐỘNG", "0", "", Color.FromArgb(39, 174, 96), out lblActiveStaff);
            var card3 = CreateStatCard("NGỪNG HOẠT ĐỘNG", "0", "", Color.FromArgb(142, 68, 173), out lblInactiveStaff);

            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsGrid.Controls.Add(card3, 2, 0);
            statsPanel.Controls.Add(statsGrid);

            // 3. Middle Bar (Filter + Actions)
            Panel toolBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10),
                Margin = new Padding(0, 0, 0, 20)
            };
            toolBar.BorderRadius(12);

            Label lblFilter = new Label { Text = "VAI TRÒ:", Font = new Font("Segoe UI Semibold", 10), Location = new Point(20, 18), AutoSize = true };

            cboRoleFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Size = new Size(150, 30),
                Location = new Point(90, 15)
            };
            cboRoleFilter.Items.AddRange(new string[] { "Tất cả", "Admin", "Quản lý", "Nhân viên" });
            cboRoleFilter.SelectedIndex = 0;
            cboRoleFilter.SelectedIndexChanged += (s, e) => FilterData();

            Button btnAddNew = CreateStyledButton("➕ THÊM", _cgvRed, Color.White);
            btnAddNew.Location = new Point(toolBar.Width - 160, 10);
            btnAddNew.Size = new Size(140, 40);
            btnAddNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddNew.Click += BtnAdd_Click;

            Button btnEdit = CreateStyledButton("✏️ SỬA", Color.FromArgb(52, 152, 219), Color.White);
            btnEdit.Location = new Point(toolBar.Width - 310, 10);
            btnEdit.Size = new Size(140, 40);
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.Click += BtnEdit_Click;

            Button btnDelete = CreateStyledButton("🗑️ XÓA", Color.FromArgb(231, 76, 60), Color.White);
            btnDelete.Location = new Point(toolBar.Width - 460, 10);
            btnDelete.Size = new Size(140, 40);
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Click += BtnDelete_Click;

            Button btnReload = CreateStyledButton("🔄 LÀM MỚI", Color.FromArgb(70, 70, 70), Color.White);
            btnReload.Location = new Point(toolBar.Width - 610, 10);
            btnReload.Size = new Size(140, 40);
            btnReload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnReload.Click += BtnRefresh_Click;

            toolBar.Controls.AddRange(new Control[] { lblFilter, cboRoleFilter, btnReload, btnDelete, btnEdit, btnAddNew });

            // 4. Data Section (The Grid)
            Panel gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(1)
            };
            gridPanel.BorderRadius(12);

            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 50 },
                GridColor = Color.FromArgb(240, 240, 240),
                EnableHeadersVisualStyles = false
            };

            // Custom Grid Header Style
            dgvUsers.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI Semibold", 10),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            dgvUsers.ColumnHeadersHeight = 50;

            dgvUsers.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 10),
                ForeColor = _cgvTextColor,
                SelectionBackColor = Color.FromArgb(255, 235, 238),
                SelectionForeColor = _cgvRed,
                Padding = new Padding(10, 0, 0, 0)
            };

            dgvUsers.CellDoubleClick += dgvUsers_CellDoubleClick;

            gridPanel.Controls.Add(dgvUsers);

            // Assemble everything
            this.Controls.Add(gridPanel);

            // Spacer
            Panel spacer = new Panel { Dock = DockStyle.Top, Height = 20 };
            this.Controls.Add(spacer);

            this.Controls.Add(toolBar);
            this.Controls.Add(statsPanel);
            this.Controls.Add(headerPanel);
        }

        private Panel CreateStatCard(string title, string value, string icon, Color accentColor, out Label valueLabel)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 20, 0)
            };
            card.BorderRadius(15);

            Panel accent = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accentColor };
            accent.BorderRadius(3);
            card.Controls.Add(accent);

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 9),
                ForeColor = Color.Gray,
                Location = new Point(25, 20),
                AutoSize = true
            };

            valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(22, 45),
                AutoSize = true
            };

            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 28),
                ForeColor = Color.FromArgb(40, accentColor.R, accentColor.G, accentColor.B),
                Location = new Point(140, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { lblTitle, valueLabel, lblIcon });
            return card;
        }

        private Button CreateStyledButton(string text, Color backColor, Color foreColor)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 40),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.BorderRadius(8);
            return btn;
        }

        private void LoadUsers()
        {
            try
            {
                dtUsers = adminBLL.GetAllUsers();
                if (dtUsers != null)
                {
                    dgvUsers.DataSource = dtUsers;
                    FormatGrid();
                    UpdateStats();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid()
        {
            if (dgvUsers.Columns.Count == 0) return;

            string[] names = { "MaNguoiDung", "TenDangNhap", "HoTen", "Email", "SoDienThoai", "VaiTro", "TenChiNhanh", "TrangThai" };
            string[] headers = { "ID", "TÀI KHOẢN", "HỌ VÀ TÊN", "EMAIL", "SỐ ĐIỆN THOẠI", "VAI TRÒ", "CHI NHÁNH", "TRẠNG THÁI" };

            foreach (DataGridViewColumn col in dgvUsers.Columns)
            {
                bool found = false;
                for (int i = 0; i < names.Length; i++)
                {
                    if (col.Name == names[i])
                    {
                        col.HeaderText = headers[i];
                        found = true;
                        break;
                    }
                }
                if (!found || col.Name == "Password") col.Visible = false;
                if (col.Name == "NgayTao") col.Visible = false;
            }
        }

        private void UpdateStats()
        {
            if (dtUsers == null) return;
            int total = dtUsers.Rows.Count;
            int active = 0;
            foreach (DataRow r in dtUsers.Rows)
            {
                if (r["TrangThai"].ToString() == "True" || r["TrangThai"].ToString() == "1") active++;
            }

            lblTotalStaff.Text = total.ToString();
            lblActiveStaff.Text = active.ToString();
            lblInactiveStaff.Text = (total - active).ToString();
        }

        private void FilterData()
        {
            if (dtUsers == null) return;
            string search = txtSearch.Text == "Tìm kiếm nhân viên..." ? "" : txtSearch.Text.ToLower();
            string role = cboRoleFilter.SelectedItem.ToString();

            DataTable filteredDt = dtUsers.Clone();
            foreach (DataRow row in dtUsers.Rows)
            {
                bool mSearch = string.IsNullOrEmpty(search) ||
                              row["HoTen"].ToString().ToLower().Contains(search) ||
                              row["TenDangNhap"].ToString().ToLower().Contains(search);
                bool mRole = role == "Tất cả" || row["VaiTro"].ToString() == role;

                if (mSearch && mRole) filteredDt.ImportRow(row);
            }
            dgvUsers.DataSource = filteredDt;
        }

        private void BtnRefresh_Click(object sender, EventArgs e) => LoadUsers();

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Mở form thêm nhân viên mới (Không truyền ID để hiểu là thêm mới)
            frmUserDetail form = new frmUserDetail();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
                MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần chỉnh sửa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["MaNguoiDung"].Value);
            frmUserDetail form = new frmUserDetail(userId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
                MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hoTen = dgvUsers.SelectedRows[0].Cells["HoTen"].Value.ToString();
            var confirmResult = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nhân viên '{hoTen}'?\n\nLưu ý: Hành động này không thể hoàn tác!",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    int userId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["MaNguoiDung"].Value);

                    if (adminBLL.DeleteUser(userId))
                    {
                        LoadUsers();
                        MessageBox.Show("Xóa nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa nhân viên này! Có thể nhân viên đang có dữ liệu liên quan.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa nhân viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int userId = Convert.ToInt32(dgvUsers.Rows[e.RowIndex].Cells["MaNguoiDung"].Value);
                frmUserDetail form = new frmUserDetail(userId);
                if (form.ShowDialog() == DialogResult.OK) LoadUsers();
            }
        }
    }
}
