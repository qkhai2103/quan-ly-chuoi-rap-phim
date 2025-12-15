using QuanLiChuoiRapPhim.BLL;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_Admin : UserControl
    {
        private AdminBLL adminBLL = new AdminBLL();
        private DataTable dtUsers;

        // Thêm các control
        private DataGridView dgvUsers;
        private TextBox txtSearch;
        private ComboBox cboRoleFilter;
        private ComboBox cboStatusFilter;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnExport;

        public UC_Admin()
        {
            InitializeComponent();
            SetupUI();
            LoadUsers();
        }

        private void SetupUI()
        {
            this.BackColor = Color.White;

            // Panel tiêu ð?
            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 60;
            titlePanel.BackColor = Color.FromArgb(0, 170, 255);

            Label lblTitle = new Label();
            lblTitle.Text = "QU?N L? NGÝ?I DÙNG";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            titlePanel.Controls.Add(lblTitle);

            // Panel ch?c nãng
            Panel functionPanel = new Panel();
            functionPanel.Dock = DockStyle.Top;
            functionPanel.Height = 50;
            functionPanel.BackColor = Color.FromArgb(240, 240, 240);
            functionPanel.Padding = new Padding(20, 10, 20, 10);

            // Ô t?m ki?m
            Label lblSearch = new Label();
            lblSearch.Text = "T?M KI?M:";
            lblSearch.Font = new Font("Segoe UI", 10);
            lblSearch.Location = new Point(20, 15);
            lblSearch.AutoSize = true;

            // Nút t?m ki?m
            Button btnSearch = new Button();
            btnSearch.Text = "T?m";
            btnSearch.Font = new Font("Segoe UI", 10);
            btnSearch.Size = new Size(80, 30);
            btnSearch.Location = new Point(310, 10);
            btnSearch.BackColor = Color.FromArgb(0, 123, 255);
            btnSearch.ForeColor = Color.White;
            btnSearch.FlatStyle = FlatStyle.Flat;

            // L?c theo vai tr?
            Label lblRole = new Label();
            lblRole.Text = "Vai tr?:";
            lblRole.Font = new Font("Segoe UI", 10);
            lblRole.Location = new Point(410, 15);
            lblRole.AutoSize = true;

            cboRoleFilter = new ComboBox();
            cboRoleFilter.Font = new Font("Segoe UI", 10);
            cboRoleFilter.Size = new Size(120, 30);
            cboRoleFilter.Location = new Point(470, 10);
            cboRoleFilter.Items.AddRange(new string[] { "T?t c?", "Admin", "Qu?n l?", "Nhân viên" });
            cboRoleFilter.SelectedIndex = 0;


           

            // Button thêm m?i
            Button btnAdd = new Button();
            btnAdd.Text = "Thêm m?i";
            btnAdd.Size = new Size(100, 35);
            btnAdd.Location = new Point(20, 8);
            btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Click += BtnAdd_Click;

            // Button xóa
            Button btnDelete = new Button();
            btnDelete.Text = "Xóa";
            btnDelete.Size = new Size(100, 35);
            btnDelete.Location = new Point(130, 8);
            btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Click += BtnDelete_Click;

            // Button làm m?i
            Button btnRefresh = new Button();
            btnRefresh.Text = "Làm m?i";
            btnRefresh.Size = new Size(100, 35);
            btnRefresh.Location = new Point(240, 8);
            btnRefresh.BackColor = Color.FromArgb(0, 123, 255);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += BtnRefresh_Click;

            // Button test k?t n?i
            Button btnTest = new Button();
            btnTest.Text = "Test DB";
            btnTest.Size = new Size(100, 35);
            btnTest.Location = new Point(350, 8);
            btnTest.BackColor = Color.FromArgb(255, 193, 7);
            btnTest.ForeColor = Color.White;
            btnTest.FlatStyle = FlatStyle.Flat;
            btnTest.Click += BtnTest_Click;

            functionPanel.Controls.Add(btnAdd);
            functionPanel.Controls.Add(btnDelete);
            functionPanel.Controls.Add(btnRefresh);
            functionPanel.Controls.Add(btnTest);

            // DataGridView
            dgvUsers = new DataGridView();
            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsers.ReadOnly = true;
            dgvUsers.BackgroundColor = Color.White;
            dgvUsers.BorderStyle = BorderStyle.Fixed3D;
            dgvUsers.CellDoubleClick += dgvUsers_CellDoubleClick;

            this.Controls.Add(dgvUsers);
            this.Controls.Add(functionPanel);
            this.Controls.Add(titlePanel);
        }

        private void LoadUsers()
        {
            try
            {
                // Ki?m tra k?t n?i
                if (!adminBLL.TestDatabaseConnection())
                {
                    MessageBox.Show("Không th? k?t n?i database!", "L?i",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // L?y d? li?u t? database
                dtUsers = adminBLL.GetAllUsers();

                if (dtUsers != null && dtUsers.Rows.Count > 0)
                {
                    dgvUsers.DataSource = dtUsers;
                    //FormatDataGridView();
                }
                else
                {
                    dgvUsers.DataSource = null;
                    MessageBox.Show("Không có d? li?u ngý?i dùng trong database.", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i: {ex.Message}", "L?i",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //private void FormatDataGridView()
        //{
        //    if (dgvUsers.Columns.Count == 0) return;

        //    // Ð?nh d?ng các c?t
        //    if (dgvUsers.Columns.Contains("MaNguoiDung"))
        //    {
        //        dgvUsers.Columns["MaNguoiDung"].HeaderText = "M?";
        //        dgvUsers.Columns["MaNguoiDung"].Width = 50;
        //    }

        //    if (dgvUsers.Columns.Contains("TenDangNhap"))
        //    {
        //        dgvUsers.Columns["TenDangNhap"].HeaderText = "TÊN ÐÃNG NH?P";
        //        dgvUsers.Columns["TenDangNhap"].Width = 120;
        //    }

        //    if (dgvUsers.Columns.Contains("HoTen"))
        //    {
        //        dgvUsers.Columns["HoTen"].HeaderText = "H? TÊN";
        //        dgvUsers.Columns["HoTen"].Width = 150;
        //    }

        //    if (dgvUsers.Columns.Contains("VaiTro"))
        //    {
        //        dgvUsers.Columns["VaiTro"].HeaderText = "VAI TR?";
        //        dgvUsers.Columns["VaiTro"].Width = 100;
        //    }

        //    if (dgvUsers.Columns.Contains("TenChiNhanh"))
        //    {
        //        dgvUsers.Columns["TenChiNhanh"].HeaderText = "CHI NHÁNH";
        //        dgvUsers.Columns["TenChiNhanh"].Width = 150;
        //    }

        //    if (dgvUsers.Columns.Contains("TrangThai"))
        //    {
        //        dgvUsers.Columns["TrangThai"].HeaderText = "TR?NG THÁI";
        //        dgvUsers.Columns["TrangThai"].Width = 80;
        //    }

        //    if (dgvUsers.Columns.Contains("NgayTao"))
        //    {
        //        dgvUsers.Columns["NgayTao"].HeaderText = "NGÀY T?O";
        //        dgvUsers.Columns["NgayTao"].Width = 120;
        //        dgvUsers.Columns["NgayTao"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        //    }
        //}
        private void UpdateStatistics()
        {
            if (dtUsers == null) return;

            int total = dtUsers.Rows.Count;
            int adminCount = 0;
            int managerCount = 0;
            int staffCount = 0;
            int activeCount = 0;

            foreach (DataRow row in dtUsers.Rows)
            {
                string role = row["VaiTro"].ToString();
                string status = row["TrangThai"].ToString();

                if (role == "Admin") adminCount++;
                else if (role == "Qu?n l?") managerCount++;
                else if (role == "Nhân viên") staffCount++;

                if (status == "True" || status == "1") activeCount++;
            }

            // C?p nh?t label th?ng kê
            foreach (Control control in this.Controls)
            {
                if (control is Panel panel && panel.Name == null)
                {
                    foreach (Control ctrl in panel.Controls)
                    {
                        if (ctrl is Label lbl && lbl.Name == "lblStats")
                        {
                            lbl.Text = $"T?ng s?: {total} ngý?i dùng | Admin: {adminCount} | Qu?n l?: {managerCount} | Nhân viên: {staffCount} | Ðang ho?t ð?ng: {activeCount}";
                            break;
                        }
                    }
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // M? form thêm ngý?i dùng
            MessageBox.Show("Ch?c nãng thêm ngý?i dùng m?i", "Thông báo");
        }

       

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui l?ng ch?n ngý?i dùng c?n s?a", "C?nh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvUsers.SelectedRows[0];
            int userId = Convert.ToInt32(row.Cells["MaNguoiDung"].Value);

        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui l?ng ch?n ngý?i dùng c?n xóa", "C?nh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvUsers.SelectedRows[0];
            int userId = Convert.ToInt32(row.Cells["MaNguoiDung"].Value);
            string username = row.Cells["TenDangNhap"].Value.ToString();

            var result = MessageBox.Show($"B?n có ch?c mu?n xóa ngý?i dùng '{username}'?",
                "Xác nh?n xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // TODO: G?i BLL ð? xóa
                MessageBox.Show($"Ð? xóa ngý?i dùng {username}", "Thông báo");
                LoadUsers(); // Refresh
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            try
            {
                if (adminBLL.TestDatabaseConnection())
                {
                    MessageBox.Show("K?t n?i database THÀNH CÔNG!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("K?t n?i database TH?T B?I!", "L?i",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i: {ex.Message}", "L?i",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUsers.Rows[e.RowIndex];
                int userId = Convert.ToInt32(row.Cells["MaNguoiDung"].Value);

                // M? form ch?nh s?a
                MessageBox.Show($"Ch?nh s?a ngý?i dùng ID: {userId}", "Thông báo");
            }
        }

        private void dgvUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUsers.Rows[e.RowIndex];
                int userId = Convert.ToInt32(row.Cells["MaNguoiDung"].Value);

                frmUserDetail form = new frmUserDetail(userId);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadUsers();
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void CboRoleFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void CboStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void FilterData()
        {
            if (dtUsers == null) return;

            string searchText = txtSearch.Text.ToLower();
            string roleFilter = cboRoleFilter.SelectedItem.ToString();
            string statusFilter = cboStatusFilter.SelectedItem.ToString();

            var filteredRows = dtUsers.AsEnumerable().Where(row => {
                bool match = true;

                // T?m ki?m
                if (!string.IsNullOrEmpty(searchText))
                {
                    match = match && (
                        row.Field<string>("TenDangNhap")?.ToLower().Contains(searchText) == true ||
                        row.Field<string>("HoTen")?.ToLower().Contains(searchText) == true ||
                        row.Field<string>("Email")?.ToLower().Contains(searchText) == true
                    );
                }

                // L?c vai tr?
                if (roleFilter != "T?t c?")
                {
                    match = match && row.Field<string>("VaiTro") == roleFilter;
                }

                // L?c tr?ng thái
                if (statusFilter != "T?t c?")
                {
                    bool isActive = row.Field<bool?>("TrangThai") ??
                                   (row.Field<string>("TrangThai") == "True" ||
                                    row.Field<string>("TrangThai") == "1");

                    if (statusFilter == "Ðang ho?t ð?ng")
                        match = match && isActive;
                    else if (statusFilter == "Ð? khóa")
                        match = match && !isActive;
                }

                return match;
            });

            if (filteredRows.Any())
            {
                dgvUsers.DataSource = filteredRows.CopyToDataTable();
            }
            else
            {
                dgvUsers.DataSource = null;
            }

            UpdateStatistics();
        }
    }
}
