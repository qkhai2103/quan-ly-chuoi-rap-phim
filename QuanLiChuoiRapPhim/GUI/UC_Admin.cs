using QuanLiChuoiRapPhim.BLL;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_Admin : UserControl
    {
        private AdminBLL adminBLL = new AdminBLL();
        private DataTable dtUsers;

        public UC_Admin()
        {
            InitializeComponent();
            SetupUI();
            LoadUsers();
        }

        private void SetupUI()
        {
            this.BackColor = Color.White;

            // Panel tiêu đề
            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 60;
            titlePanel.BackColor = Color.FromArgb(0, 170, 255);

            Label lblTitle = new Label();
            lblTitle.Text = "QUẢN LÝ NGƯỜI DÙNG";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            titlePanel.Controls.Add(lblTitle);

            // Panel chức năng
            Panel functionPanel = new Panel();
            functionPanel.Dock = DockStyle.Top;
            functionPanel.Height = 50;
            functionPanel.BackColor = Color.FromArgb(240, 240, 240);

            // Button thêm mới
            Button btnAdd = new Button();
            btnAdd.Text = "Thêm mới";
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

            // Button làm mới
            Button btnRefresh = new Button();
            btnRefresh.Text = "Làm mới";
            btnRefresh.Size = new Size(100, 35);
            btnRefresh.Location = new Point(240, 8);
            btnRefresh.BackColor = Color.FromArgb(0, 123, 255);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += BtnRefresh_Click;

            // Button test kết nối
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
            dgvUsers.CellDoubleClick += DgvUsers_CellDoubleClick;

            this.Controls.Add(dgvUsers);
            this.Controls.Add(functionPanel);
            this.Controls.Add(titlePanel);
        }

        private void LoadUsers()
        {
            try
            {
                // Kiểm tra kết nối
                if (!adminBLL.TestDatabaseConnection())
                {
                    MessageBox.Show("Không thể kết nối database!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Lấy dữ liệu từ database
                dtUsers = adminBLL.GetAllUsers();

                if (dtUsers != null && dtUsers.Rows.Count > 0)
                {
                    dgvUsers.DataSource = dtUsers;
                    //FormatDataGridView();
                }
                else
                {
                    dgvUsers.DataSource = null;
                    MessageBox.Show("Không có dữ liệu người dùng trong database.", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //private void FormatDataGridView()
        //{
        //    if (dgvUsers.Columns.Count == 0) return;

        //    // Định dạng các cột
        //    if (dgvUsers.Columns.Contains("MaNguoiDung"))
        //    {
        //        dgvUsers.Columns["MaNguoiDung"].HeaderText = "MÃ";
        //        dgvUsers.Columns["MaNguoiDung"].Width = 50;
        //    }

        //    if (dgvUsers.Columns.Contains("TenDangNhap"))
        //    {
        //        dgvUsers.Columns["TenDangNhap"].HeaderText = "TÊN ĐĂNG NHẬP";
        //        dgvUsers.Columns["TenDangNhap"].Width = 120;
        //    }

        //    if (dgvUsers.Columns.Contains("HoTen"))
        //    {
        //        dgvUsers.Columns["HoTen"].HeaderText = "HỌ TÊN";
        //        dgvUsers.Columns["HoTen"].Width = 150;
        //    }

        //    if (dgvUsers.Columns.Contains("VaiTro"))
        //    {
        //        dgvUsers.Columns["VaiTro"].HeaderText = "VAI TRÒ";
        //        dgvUsers.Columns["VaiTro"].Width = 100;
        //    }

        //    if (dgvUsers.Columns.Contains("TenChiNhanh"))
        //    {
        //        dgvUsers.Columns["TenChiNhanh"].HeaderText = "CHI NHÁNH";
        //        dgvUsers.Columns["TenChiNhanh"].Width = 150;
        //    }

        //    if (dgvUsers.Columns.Contains("TrangThai"))
        //    {
        //        dgvUsers.Columns["TrangThai"].HeaderText = "TRẠNG THÁI";
        //        dgvUsers.Columns["TrangThai"].Width = 80;
        //    }

        //    if (dgvUsers.Columns.Contains("NgayTao"))
        //    {
        //        dgvUsers.Columns["NgayTao"].HeaderText = "NGÀY TẠO";
        //        dgvUsers.Columns["NgayTao"].Width = 120;
        //        dgvUsers.Columns["NgayTao"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        //    }
        //}

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Mở form thêm người dùng
            MessageBox.Show("Chức năng thêm người dùng mới", "Thông báo");
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn người dùng cần xóa", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvUsers.SelectedRows[0];
            int userId = Convert.ToInt32(row.Cells["MaNguoiDung"].Value);
            string username = row.Cells["TenDangNhap"].Value.ToString();

            var result = MessageBox.Show($"Bạn có chắc muốn xóa người dùng '{username}'?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // TODO: Gọi BLL để xóa
                MessageBox.Show($"Đã xóa người dùng {username}", "Thông báo");
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
                    MessageBox.Show("Kết nối database THÀNH CÔNG!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Kết nối database THẤT BẠI!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUsers.Rows[e.RowIndex];
                int userId = Convert.ToInt32(row.Cells["MaNguoiDung"].Value);

                // Mở form chỉnh sửa
                MessageBox.Show($"Chỉnh sửa người dùng ID: {userId}", "Thông báo");
            }
        }

        // Controls
        private DataGridView dgvUsers;
    }
}