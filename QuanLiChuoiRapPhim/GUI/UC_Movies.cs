using QuanLiChuoiRapPhim.BLL;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_Movies : UserControl
    {
        private PhimBLL _phimBLL = new PhimBLL();
        private DataGridView dgvPhim;
        private TextBox txtTenPhim, txtTheLoai, txtThoiLuong, txtDaoDien, txtDienVien, txtDoTuoi;
        private DateTimePicker dtpNgayKhoiChieu;
        private Button btnThem, btnSua, btnXoa, btnLamMoi, btnTim;
        private TextBox txtSearchMovie;
        private DataTable dtPhim;

        // Màu sắc CGV
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_Movies()
        {
            InitializeComponent();
            SetupUI();
            LoadPhim();
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            // Panel tiêu đề
            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 60;
            titlePanel.BackColor = _cgvRed;
            titlePanel.Padding = new Padding(20, 0, 20, 0);

            Label lblTitle = new Label();
            lblTitle.Text = "🎬 QUẢN LÝ PHIM";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            titlePanel.Controls.Add(lblTitle);

            // Panel tìm kiếm
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 50;
            searchPanel.BackColor = Color.White;
            searchPanel.Padding = new Padding(20, 10, 20, 10);

            Label lblSearch = new Label();
            lblSearch.Text = "Tìm kiếm:";
            lblSearch.Font = new Font("Segoe UI", 10);
            lblSearch.Location = new Point(0, 12);
            lblSearch.AutoSize = true;

            txtSearchMovie = new TextBox();
            txtSearchMovie.Font = new Font("Segoe UI", 10);
            txtSearchMovie.Location = new Point(80, 10);
            txtSearchMovie.Size = new Size(250, 30);
            txtSearchMovie.Text = "Nhập tên phim...";

            btnTim = new Button();
            btnTim.Text = "🔍 Tìm";
            btnTim.Location = new Point(340, 10);
            btnTim.Size = new Size(80, 30);
            btnTim.BackColor = _cgvRed;
            btnTim.ForeColor = Color.White;
            btnTim.Click += BtnTim_Click;

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearchMovie, btnTim });

            // Panel chức năng
            Panel functionPanel = new Panel();
            functionPanel.Dock = DockStyle.Top;
            functionPanel.Height = 50;
            functionPanel.BackColor = Color.White;
            functionPanel.Padding = new Padding(20, 8, 20, 8);

            btnThem = new Button();
            btnThem.Text = "➕ Thêm";
            btnThem.Size = new Size(100, 35);
            btnThem.Location = new Point(20, 8);
            btnThem.BackColor = Color.FromArgb(40, 167, 69);
            btnThem.ForeColor = Color.White;
            btnThem.Click += BtnThem_Click;

            btnSua = new Button();
            btnSua.Text = "✏️ Sửa";
            btnSua.Size = new Size(100, 35);
            btnSua.Location = new Point(130, 8);
            btnSua.BackColor = Color.FromArgb(0, 123, 255);
            btnSua.ForeColor = Color.White;
            btnSua.Click += BtnSua_Click;

            btnXoa = new Button();
            btnXoa.Text = "🗑️ Xóa";
            btnXoa.Size = new Size(100, 35);
            btnXoa.Location = new Point(240, 8);
            btnXoa.BackColor = _cgvRed;
            btnXoa.ForeColor = Color.White;
            btnXoa.Click += BtnXoa_Click;

            btnLamMoi = new Button();
            btnLamMoi.Text = "🔄 Làm mới";
            btnLamMoi.Size = new Size(100, 35);
            btnLamMoi.Location = new Point(350, 8);
            btnLamMoi.BackColor = Color.FromArgb(23, 162, 184);
            btnLamMoi.ForeColor = Color.White;
            btnLamMoi.Click += BtnLamMoi_Click;

            functionPanel.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLamMoi });

            // DataGridView
            dgvPhim = new DataGridView();
            dgvPhim.Dock = DockStyle.Fill;
            dgvPhim.BackgroundColor = Color.White;
            dgvPhim.RowHeadersVisible = false;
            dgvPhim.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPhim.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPhim.AllowUserToAddRows = false;
            dgvPhim.AllowUserToDeleteRows = false;

            // Thêm các cột
            dgvPhim.Columns.Add("MaPhim", "Mã");
            dgvPhim.Columns.Add("TenPhim", "Tên Phim");
            dgvPhim.Columns.Add("TheLoai", "Thể Loại");
            dgvPhim.Columns.Add("ThoiLuong", "Thời Lượng (phút)");
            dgvPhim.Columns.Add("DaoDien", "Đạo Diễn");
            dgvPhim.Columns.Add("DoTuoi", "Độ Tuổi");
            dgvPhim.Columns.Add("NgayKhoiChieu", "Ngày Khởi Chiếu");

            // Thêm controls
            this.Controls.Add(dgvPhim);
            this.Controls.Add(functionPanel);
            this.Controls.Add(searchPanel);
            this.Controls.Add(titlePanel);
        }

        private void LoadPhim()
        {
            try
            {
                dtPhim = _phimBLL.LayTatCaPhim();
                BindData(dtPhim);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindData(DataTable dt)
        {
            dgvPhim.Rows.Clear();
            foreach (DataRow row in dt.Rows)
            {
                dgvPhim.Rows.Add(
                    row["MaPhim"],
                    row["TenPhim"],
                    row["TheLoai"],
                    row["ThoiLuong"],
                    row["DaoDien"],
                    row["DoTuoi"],
                    Convert.ToDateTime(row["NgayKhoiChieu"]).ToString("dd/MM/yyyy")
                );
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            using (Form frmThemPhim = new Form())
            {
                frmThemPhim.Text = "Thêm Phim";
                frmThemPhim.Size = new Size(500, 400);
                frmThemPhim.StartPosition = FormStartPosition.CenterParent;

                Label lblTenPhim = new Label() { Text = "Tên Phim:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtTenPhim = new TextBox() { Location = new Point(120, 20), Size = new Size(350, 25) };

                Label lblTheLoai = new Label() { Text = "Thể Loại:", Location = new Point(20, 60), AutoSize = true };
                TextBox txtTheLoai = new TextBox() { Location = new Point(120, 60), Size = new Size(350, 25) };

                Label lblThoiLuong = new Label() { Text = "Thời Lượng (phút):", Location = new Point(20, 100), AutoSize = true };
                TextBox txtThoiLuong = new TextBox() { Location = new Point(120, 100), Size = new Size(350, 25) };

                Label lblDaoDien = new Label() { Text = "Đạo Diễn:", Location = new Point(20, 140), AutoSize = true };
                TextBox txtDaoDien = new TextBox() { Location = new Point(120, 140), Size = new Size(350, 25) };

                Label lblDienVien = new Label() { Text = "Diễn Viên:", Location = new Point(20, 180), AutoSize = true };
                TextBox txtDienVien = new TextBox() { Location = new Point(120, 180), Size = new Size(350, 25) };

                Label lblDoTuoi = new Label() { Text = "Độ Tuổi:", Location = new Point(20, 220), AutoSize = true };
                ComboBox cboDoTuoi = new ComboBox() { Location = new Point(120, 220), Size = new Size(350, 25) };
                cboDoTuoi.Items.AddRange(new[] { "P", "C13", "C16", "C18" });
                cboDoTuoi.SelectedIndex = 0;

                Label lblNgayKhoiChieu = new Label() { Text = "Ngày Khởi Chiếu:", Location = new Point(20, 260), AutoSize = true };
                DateTimePicker dtpNgayKhoiChieu = new DateTimePicker() { Location = new Point(120, 260), Size = new Size(350, 25) };

                Button btnLuu = new Button() { Text = "Lưu", Location = new Point(200, 320), Size = new Size(80, 35), BackColor = _cgvRed, ForeColor = Color.White };
                Button btnHuy = new Button() { Text = "Hủy", Location = new Point(300, 320), Size = new Size(80, 35) };

                btnLuu.Click += (s, e2) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(txtTenPhim.Text))
                        {
                            MessageBox.Show("Vui lòng nhập tên phim!", "Thông báo");
                            return;
                        }

                        if (!int.TryParse(txtThoiLuong.Text, out int thoiLuong))
                        {
                            MessageBox.Show("Thời lượng phải là số!", "Thông báo");
                            return;
                        }

                        _phimBLL.ThemPhim(txtTenPhim.Text, txtTheLoai.Text, thoiLuong, txtDaoDien.Text, 
                            txtDienVien.Text, "", cboDoTuoi.SelectedItem.ToString(), dtpNgayKhoiChieu.Value);

                        MessageBox.Show("Thêm phim thành công!", "Thông báo");
                        frmThemPhim.Close();
                        LoadPhim();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnHuy.Click += (s, e2) => frmThemPhim.Close();

                frmThemPhim.Controls.AddRange(new Control[] {
                    lblTenPhim, txtTenPhim, lblTheLoai, txtTheLoai, lblThoiLuong, txtThoiLuong,
                    lblDaoDien, txtDaoDien, lblDienVien, txtDienVien, lblDoTuoi, cboDoTuoi,
                    lblNgayKhoiChieu, dtpNgayKhoiChieu, btnLuu, btnHuy
                });

                frmThemPhim.ShowDialog();
            }
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (dgvPhim.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phim cần sửa!", "Thông báo");
                return;
            }

            int maPhim = Convert.ToInt32(dgvPhim.SelectedRows[0].Cells["MaPhim"].Value);
            DataRow row = dtPhim.Rows.Find(maPhim);

            using (Form frmSuaPhim = new Form())
            {
                frmSuaPhim.Text = "Sửa Phim";
                frmSuaPhim.Size = new Size(500, 400);
                frmSuaPhim.StartPosition = FormStartPosition.CenterParent;

                Label lblTenPhim = new Label() { Text = "Tên Phim:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtTenPhim = new TextBox() { Location = new Point(120, 20), Size = new Size(350, 25), Text = row["TenPhim"].ToString() };

                Label lblTheLoai = new Label() { Text = "Thể Loại:", Location = new Point(20, 60), AutoSize = true };
                TextBox txtTheLoai = new TextBox() { Location = new Point(120, 60), Size = new Size(350, 25), Text = row["TheLoai"].ToString() };

                Label lblThoiLuong = new Label() { Text = "Thời Lượng (phút):", Location = new Point(20, 100), AutoSize = true };
                TextBox txtThoiLuong = new TextBox() { Location = new Point(120, 100), Size = new Size(350, 25), Text = row["ThoiLuong"].ToString() };

                Label lblDaoDien = new Label() { Text = "Đạo Diễn:", Location = new Point(20, 140), AutoSize = true };
                TextBox txtDaoDien = new TextBox() { Location = new Point(120, 140), Size = new Size(350, 25), Text = row["DaoDien"].ToString() };

                Label lblDienVien = new Label() { Text = "Diễn Viên:", Location = new Point(20, 180), AutoSize = true };
                TextBox txtDienVien = new TextBox() { Location = new Point(120, 180), Size = new Size(350, 25), Text = row["DienVien"].ToString() };

                Label lblDoTuoi = new Label() { Text = "Độ Tuổi:", Location = new Point(20, 220), AutoSize = true };
                ComboBox cboDoTuoi = new ComboBox() { Location = new Point(120, 220), Size = new Size(350, 25) };
                cboDoTuoi.Items.AddRange(new[] { "P", "C13", "C16", "C18" });
                cboDoTuoi.SelectedItem = row["DoTuoi"].ToString();

                Label lblNgayKhoiChieu = new Label() { Text = "Ngày Khởi Chiếu:", Location = new Point(20, 260), AutoSize = true };
                DateTimePicker dtpNgayKhoiChieu = new DateTimePicker() { Location = new Point(120, 260), Size = new Size(350, 25), Value = Convert.ToDateTime(row["NgayKhoiChieu"]) };

                Button btnLuu = new Button() { Text = "Lưu", Location = new Point(200, 320), Size = new Size(80, 35), BackColor = _cgvRed, ForeColor = Color.White };
                Button btnHuy = new Button() { Text = "Hủy", Location = new Point(300, 320), Size = new Size(80, 35) };

                btnLuu.Click += (s, e2) =>
                {
                    try
                    {
                        if (!int.TryParse(txtThoiLuong.Text, out int thoiLuong))
                        {
                            MessageBox.Show("Thời lượng phải là số!", "Thông báo");
                            return;
                        }

                        _phimBLL.CapNhatPhim(maPhim, txtTenPhim.Text, txtTheLoai.Text, thoiLuong, 
                            txtDaoDien.Text, txtDienVien.Text, "", cboDoTuoi.SelectedItem.ToString(), dtpNgayKhoiChieu.Value);

                        MessageBox.Show("Cập nhật phim thành công!", "Thông báo");
                        frmSuaPhim.Close();
                        LoadPhim();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnHuy.Click += (s, e2) => frmSuaPhim.Close();

                frmSuaPhim.Controls.AddRange(new Control[] {
                    lblTenPhim, txtTenPhim, lblTheLoai, txtTheLoai, lblThoiLuong, txtThoiLuong,
                    lblDaoDien, txtDaoDien, lblDienVien, txtDienVien, lblDoTuoi, cboDoTuoi,
                    lblNgayKhoiChieu, dtpNgayKhoiChieu, btnLuu, btnHuy
                });

                frmSuaPhim.ShowDialog();
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvPhim.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phim cần xóa!", "Thông báo");
                return;
            }

            int maPhim = Convert.ToInt32(dgvPhim.SelectedRows[0].Cells["MaPhim"].Value);
            string tenPhim = dgvPhim.SelectedRows[0].Cells["TenPhim"].Value.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phim '{tenPhim}'?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _phimBLL.XoaPhim(maPhim);
                    MessageBox.Show("Xóa phim thành công!", "Thông báo");
                    LoadPhim();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            LoadPhim();
            txtSearchMovie.Clear();
        }

        private void BtnTim_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSearchMovie.Text))
                {
                    LoadPhim();
                    return;
                }

                DataTable dtKetQua = _phimBLL.TimKiemPhim(txtSearchMovie.Text);
                BindData(dtKetQua);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

