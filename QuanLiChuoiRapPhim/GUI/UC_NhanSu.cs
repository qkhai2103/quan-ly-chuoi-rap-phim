// File: UC_NhanSu.cs (Phiên bản cập nhật - Thêm chi tiết ca làm việc)
// Vị trí: QuanLiChuoiRapPhim.GUI
// Mô tả: UserControl quản lý nhân sự chi nhánh cho Quản lý
// Tính năng mới: Khi double-click hoặc chọn "Sửa", hiển thị popup chi tiết nhân viên bao gồm lịch ca làm việc gần đây

using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_NhanSu : UserControl
    {
        private readonly int _maChiNhanh;
        private DataTable _dtNhanVien;
        private ManagerBLL _managerBLL;
        private ManagerDAL _managerDAL;

        // UI Components
        private Panel pnlHeader, pnlFilter, pnlFooter;
        private DataGridView dgvNhanVien;
        private TextBox txtTimKiem;
        private ComboBox cboTrangThai;
        private Button btnLamMoi, btnThem, btnSua, btnXoa;
        private Label lblThongKe;

        public UC_NhanSu(int maChiNhanh)
        {
            _maChiNhanh = maChiNhanh;
            _managerDAL = new ManagerDAL();
            _managerBLL = new ManagerBLL();

            ThietLapGiaoDien();
            TaiDanhSachNhanVien();
        }

        private void ThietLapGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // HEADER
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0, 170, 255)
            };
            Label lblTieuDe = new Label
            {
                Text = "QUẢN LÝ NHÂN VIÊN CHI NHÁNH",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            pnlHeader.Controls.Add(lblTieuDe);

            // FILTER PANEL
            pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Label lblTimKiem = new Label { Text = "🔍 Tìm kiếm:", Font = new Font("Segoe UI", 10), AutoSize = true, Location = new Point(20, 18) };
            txtTimKiem = new TextBox { Width = 250, Location = new Point(120, 15), Font = new Font("Segoe UI", 10) };
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;

            Label lblTrangThai = new Label { Text = "📊 Trạng thái:", Font = new Font("Segoe UI", 10), AutoSize = true, Location = new Point(400, 18) };
            cboTrangThai = new ComboBox { Width = 150, Location = new Point(500, 15), DropDownStyle = ComboBoxStyle.DropDownList };
            cboTrangThai.Items.AddRange(new string[] { "Tất cả", "CoHieuLuc", "HetHieuLuc", "TamDung" });
            cboTrangThai.SelectedIndex = 0;
            cboTrangThai.SelectedIndexChanged += CboTrangThai_SelectedIndexChanged;

            btnLamMoi = CreateButton("🔄 Làm mới", 670, Color.FromArgb(0, 123, 255));
            btnLamMoi.Click += BtnLamMoi_Click;

            btnThem = CreateButton("➕ Thêm", 780, Color.FromArgb(40, 167, 69));
            btnThem.Click += BtnThem_Click;

            btnSua = CreateButton("✏️ Sửa / Chi tiết", 890, Color.FromArgb(255, 193, 7));
            btnSua.Click += BtnSua_Click;

            btnXoa = CreateButton("🗑️ Xóa", 1000, Color.FromArgb(220, 53, 69));
            btnXoa.Click += BtnXoa_Click;

            pnlFilter.Controls.AddRange(new Control[] { lblTimKiem, txtTimKiem, lblTrangThai, cboTrangThai, btnLamMoi, btnThem, btnSua, btnXoa });

            // DATAGRIDVIEW
            dgvNhanVien = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvNhanVien.CellDoubleClick += DgvNhanVien_CellDoubleClick;
            dgvNhanVien.SelectionChanged += DgvNhanVien_SelectionChanged;

            // FOOTER
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            lblThongKe = new Label
            {
                Text = "Tổng nhân viên: 0 người",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(20, 10),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            pnlFooter.Controls.Add(lblThongKe);

            this.Controls.Add(dgvNhanVien);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private Button CreateButton(string text, int x, Color backColor)
        {
            return new Button
            {
                Text = text,
                Width = 140,
                Height = 30,
                Location = new Point(x, 15),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
        }

        private void TaiDanhSachNhanVien()
        {
            try
            {
                _dtNhanVien = _managerBLL.GetBranchEmployees(0, _maChiNhanh); // TODO: truyền đúng MaQuanLy nếu cần

                if (_dtNhanVien == null || _dtNhanVien.Rows.Count == 0)
                {
                    MessageBox.Show("Không có nhân viên nào trong chi nhánh này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblThongKe.Text = "Tổng nhân viên: 0 người";
                    dgvNhanVien.DataSource = null;
                    return;
                }

                dgvNhanVien.DataSource = _dtNhanVien;

                // Định nghĩa cột hiển thị
                if (dgvNhanVien.Columns["MaNhanVien"] != null) dgvNhanVien.Columns["MaNhanVien"].HeaderText = "Mã NV";
                if (dgvNhanVien.Columns["MaNguoiDung"] != null) dgvNhanVien.Columns["MaNguoiDung"].Visible = false;
                dgvNhanVien.Columns["HoTen"].HeaderText = "Họ và Tên";
                dgvNhanVien.Columns["Email"].HeaderText = "Email";
                dgvNhanVien.Columns["SoDienThoai"].HeaderText = "Số ĐT";
                dgvNhanVien.Columns["ViTri"].HeaderText = "Vị Trí";
                dgvNhanVien.Columns["TrangThai"].HeaderText = "Trạng Thái";
                dgvNhanVien.Columns["NgayTao"].HeaderText = "Ngày Tạo";
                dgvNhanVien.Columns["NgayTao"].DefaultCellStyle.Format = "dd/MM/yyyy";

                int activeCount = _dtNhanVien.Select("TrangThai = 'CoHieuLuc'").Length;
                lblThongKe.Text = $"Tổng nhân viên: {_dtNhanVien.Rows.Count} người (Đang hoạt động: {activeCount})";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách nhân viên:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblThongKe.Text = "Lỗi tải dữ liệu";
            }
        }

        private void TxtTimKiem_TextChanged(object sender, EventArgs e)
        {
            if (_dtNhanVien == null) return;

            string filter = txtTimKiem.Text.Trim().Replace("'", "''");
            string statusFilter = cboTrangThai.SelectedItem.ToString() == "Tất cả" ? "" : $"AND TrangThai = '{cboTrangThai.SelectedItem}'";

            string rowFilter = string.IsNullOrEmpty(filter)
                ? statusFilter.TrimStart(" AND ".ToCharArray())
                : $"(HoTen LIKE '%{filter}%' OR Email LIKE '%{filter}%' OR SoDienThoai LIKE '%{filter}%') {statusFilter}";

            _dtNhanVien.DefaultView.RowFilter = rowFilter.Trim();
        }

        private void CboTrangThai_SelectedIndexChanged(object sender, EventArgs e) => TxtTimKiem_TextChanged(sender, e);

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            txtTimKiem.Clear();
            cboTrangThai.SelectedIndex = 0;
            TaiDanhSachNhanVien();
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng thêm nhân viên mới chỉ thực hiện được bởi Admin.\nVui lòng liên hệ Admin hệ thống.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hoTen = dgvNhanVien.SelectedRows[0].Cells["HoTen"].Value.ToString();
            MessageBox.Show($"Chức năng xóa nhân viên '{hoTen}' chỉ thực hiện được bởi Admin.\nVui lòng liên hệ Admin hệ thống.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSua_Click(object sender, EventArgs e) => HienThiChiTietNhanVien();

        private void DgvNhanVien_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) HienThiChiTietNhanVien();
        }

        private void DgvNhanVien_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvNhanVien.SelectedRows.Count > 0;
            btnSua.Enabled = hasSelection;
            btnXoa.Enabled = hasSelection;
        }

        // ==================== CHI TIẾT NHÂN VIÊN + CA LÀM VIỆC ====================
        private void HienThiChiTietNhanVien()
        {
            if (dgvNhanVien.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên để xem chi tiết!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvNhanVien.SelectedRows[0];
            int maNguoiDung = Convert.ToInt32(row.Cells["MaNguoiDung"].Value);
            string hoTen = row.Cells["HoTen"].Value.ToString();
            string email = row.Cells["Email"].Value?.ToString() ?? "";
            string sdt = row.Cells["SoDienThoai"].Value?.ToString() ?? "";
            string viTri = row.Cells["ViTri"].Value?.ToString() ?? "Nhân Viên";
            string trangThai = row.Cells["TrangThai"].Value?.ToString() ?? "";

            // Tải lịch ca làm việc gần đây (10 ca gần nhất)
            // TODO: Implement GetLichCaNhanVien method in ManagerDAL
            // DataTable dtCaLamViec = _managerDAL.GetLichCaNhanVien(maNguoiDung, 10);
            DataTable dtCaLamViec = new DataTable(); // Temp: empty for now

            // Tạo form popup chi tiết
            Form frmChiTiet = new Form
            {
                Text = $"Chi tiết nhân viên: {hoTen}",
                Size = new Size(900, 650),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = Color.White
            };

            // Thông tin cơ bản
            Panel pnlInfo = new Panel { Dock = DockStyle.Top, Height = 180, Padding = new Padding(20) };
            Label lblHoTen = CreateLabel($"Họ tên: {hoTen}", 10, FontStyle.Bold, 14);
            Label lblEmail = CreateLabel($"Email: {email}", 40);
            Label lblSDT = CreateLabel($"Số điện thoại: {sdt}", 70);
            Label lblViTri = CreateLabel($"Vị trí: {viTri}", 100);
            Label lblTrangThai = CreateLabel($"Trạng thái: {trangThai}", 130);
            pnlInfo.Controls.AddRange(new Control[] { lblHoTen, lblEmail, lblSDT, lblViTri, lblTrangThai });

            // Tiêu đề lịch ca
            Label lblLichCa = new Label
            {
                Text = "📅 Lịch ca làm việc gần đây (10 ca gần nhất)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 10)
            };

            // DataGridView lịch ca
            DataGridView dgvCa = new DataGridView
            {
                Location = new Point(20, 40),
                Size = new Size(840, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                DataSource = dtCaLamViec
            };

            if (dtCaLamViec.Rows.Count == 0)
            {
                dgvCa.DataSource = null;
                Label lblNoData = new Label
                {
                    Text = "Chưa có dữ liệu phân công ca làm việc.",
                    Font = new Font("Segoe UI", 11, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(20, 200)
                };
                frmChiTiet.Controls.Add(lblNoData);
            }
            else
            {
                // Định dạng cột
                dgvCa.Columns["NgayLamViec"].HeaderText = "Ngày làm";
                dgvCa.Columns["NgayLamViec"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvCa.Columns["TenCa"].HeaderText = "Ca làm";
                dgvCa.Columns["GioBatDau"].HeaderText = "Giờ bắt đầu";
                dgvCa.Columns["GioKetThuc"].HeaderText = "Giờ kết thúc";
                dgvCa.Columns["TrangThai"].HeaderText = "Trạng thái";
                dgvCa.Columns["TienDauCa"].HeaderText = "Tiền đầu ca";
                dgvCa.Columns["TienCuoiCa"].HeaderText = "Tiền cuối ca";
                dgvCa.Columns["ChenhLech"].HeaderText = "Chênh lệch";

                // Màu trạng thái
                foreach (DataGridViewRow r in dgvCa.Rows)
                {
                    string tt = r.Cells["TrangThai"].Value?.ToString();
                    switch (tt)
                    {
                        case "DaKetThuc": r.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220); break;
                        case "DangLam": r.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 180); break;
                        case "ChuaBatDau": r.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240); break;
                    }
                }
            }

            Panel pnlCa = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            pnlCa.Controls.Add(dgvCa);
            pnlCa.Controls.Add(lblLichCa);

            frmChiTiet.Controls.Add(pnlCa);
            frmChiTiet.Controls.Add(pnlInfo);

            frmChiTiet.ShowDialog();
        }

        private Label CreateLabel(string text, int top, FontStyle style = FontStyle.Regular, float fontSize = 11)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", fontSize, style),
                AutoSize = true,
                Location = new Point(10, top),
                ForeColor = Color.FromArgb(50, 50, 50)
            };
        }
    }
}