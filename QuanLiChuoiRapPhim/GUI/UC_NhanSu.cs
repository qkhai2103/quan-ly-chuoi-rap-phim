// File: UC_NhanSu.cs
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_NhanSu : UserControl
    {
        private readonly int _maChiNhanh;           // Chi nhánh của Quản lý hiện tại
        private DataTable _dtNhanVien;              // Dữ liệu gốc
        private DataGridView dgvNhanVien;
        private TextBox txtTimKiem;
        private Button btnLamMoi;
        private Label lblThongKe;

        // Constructor nhận MaChiNhanh từ frmMain sau khi đăng nhập
        public UC_NhanSu()
        {
     
             

            ThietLapGiaoDien();
            TaiDanhSachNhanVien();
        }

        private void ThietLapGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // === TIÊU ĐỀ ===
            Panel pnlTieuDe = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0, 123, 255)
            };
            Label lblTieuDe = new Label
            {
                Text = "QUẢN LÝ NHÂN SỰ CHI NHÁNH",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            pnlTieuDe.Controls.Add(lblTieuDe);

            // === THANH CÔNG CỤ ===
            Panel pnlCongCu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 12, 20, 0)
            };

            Label lblTimKiem = new Label
            {
                Text = "Tìm kiếm:",
                AutoSize = true,
                Location = new Point(5, 18),
                Font = new Font("Segoe UI", 10F)
            };

            txtTimKiem = new TextBox
            {
                Size = new Size(320, 30),
                Location = new Point(80, 13),
                Font = new Font("Segoe UI", 10F)
            };
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;

            btnLamMoi = new Button
            {
                Text = "Làm mới",
                Size = new Size(110, 35),
                Location = new Point(420, 12),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnLamMoi.Click += BtnLamMoi_Click;

            pnlCongCu.Controls.AddRange(new Control[] { lblTimKiem, txtTimKiem, btnLamMoi });

            // === DATAGRIDVIEW ===
            dgvNhanVien = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                MultiSelect = false
            };

            // Style header
            dgvNhanVien.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 123, 255);
            dgvNhanVien.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvNhanVien.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvNhanVien.EnableHeadersVisualStyles = false;

            // === FOOTER THỐNG KÊ ===
            Panel pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(236, 240, 245)
            };
            lblThongKe = new Label
            {
                Text = "Đang tải...",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(20, 13)
            };
            pnlFooter.Controls.Add(lblThongKe);

            // Thêm vào UserControl
            this.Controls.Add(dgvNhanVien);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlCongCu);
            this.Controls.Add(pnlTieuDe);
        }

        private void TaiDanhSachNhanVien()
        {
            string query = @"
                SELECT 
                    MaNguoiDung,
                    TenDangNhap,
                    HoTen,
                    Email,
                    SoDienThoai,
                    NgayTao,
                    TrangThai
                FROM NguoiDung
                WHERE MaChiNhanh = @MaChiNhanh
                  AND VaiTro = N'NhanVien'
                  AND TrangThai = 1
                ORDER BY HoTen ASC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtNhanVien = new DataTable();
                        da.Fill(_dtNhanVien);

                        dgvNhanVien.DataSource = _dtNhanVien;

                        // Ẩn cột không cần thiết
                        if (dgvNhanVien.Columns["MaNguoiDung"] != null)
                            dgvNhanVien.Columns["MaNguoiDung"].Visible = false;

                        // Đổi tên cột
                        dgvNhanVien.Columns["TenDangNhap"].HeaderText = "Tên đăng nhập";
                        dgvNhanVien.Columns["HoTen"].HeaderText = "Họ và tên";
                        dgvNhanVien.Columns["Email"].HeaderText = "Email";
                        dgvNhanVien.Columns["SoDienThoai"].HeaderText = "Số điện thoại";
                        dgvNhanVien.Columns["NgayTao"].HeaderText = "Ngày vào làm";
                        dgvNhanVien.Columns["TrangThai"].HeaderText = "Trạng thái";

                        // Format ngày
                        dgvNhanVien.Columns["NgayTao"].DefaultCellStyle.Format = "dd/MM/yyyy";

                        // Cập nhật thống kê
                        lblThongKe.Text = $"Tổng nhân viên đang hoạt động: {_dtNhanVien.Rows.Count} người";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách nhân viên:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblThongKe.Text = "Lỗi tải dữ liệu";
            }
        }

        private void TxtTimKiem_TextChanged(object sender, EventArgs e)
        {
            string filter = txtTimKiem.Text.Trim();
            if (_dtNhanVien == null) return;

            if (string.IsNullOrEmpty(filter))
            {
                _dtNhanVien.DefaultView.RowFilter = "";
            }
            else
            {
                _dtNhanVien.DefaultView.RowFilter = string.Format(
                    "HoTen LIKE '%{0}%' OR TenDangNhap LIKE '%{0}%' OR Email LIKE '%{0}%' OR SoDienThoai LIKE '%{0}%'",
                    filter.Replace("'", "''"));
            }
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            txtTimKiem.Clear();
            TaiDanhSachNhanVien();
        }
    }
}