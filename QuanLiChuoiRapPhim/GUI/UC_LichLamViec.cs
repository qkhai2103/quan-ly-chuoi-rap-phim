// File: UC_LichLamViec.cs
// Vị trí: QuanLiChuoiRapPhim.GUI
// Dành cho vai trò Quản lý chi nhánh

using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_LichLamViec : UserControl
    {
        private readonly int _maChiNhanh;
        private DateTime _ngayHienTai = new DateTime(2025, 12, 13); // Ngày hiện tại theo đề bài
        private DateTime _tuanBatDau; // Thứ Hai của tuần hiện tại

        private DataGridView dgvLichCa;
        private Label lblTuan;
        private Button btnTuanTruoc, btnTuanSau, btnLamMoi;

        public UC_LichLamViec()
        {
            
           
            TinhTuanHienTai();
            ThietLapGiaoDien();
            TaiLichLamViec();
        }

        private void TinhTuanHienTai()
        {
            // Tìm thứ Hai của tuần chứa ngày hiện tại
            int diff = (int)_ngayHienTai.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            _tuanBatDau = _ngayHienTai.AddDays(-diff);
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
                BackColor = Color.FromArgb(220, 53, 69) // Đỏ CGV
            };
            Label lblTieuDe = new Label
            {
                Text = "LỊCH LÀM VIỆC & PHÂN CÔNG CA",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            pnlTieuDe.Controls.Add(lblTieuDe);

            // === THANH ĐIỀU KHIỂN TUẦN ===
            Panel pnlDieuKhien = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 10, 20, 10)
            };

            btnTuanTruoc = new Button
            {
                Text = "◄ Tuần trước",
                Size = new Size(120, 35),
                Location = new Point(10, 12),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTuanTruoc.Click += BtnTuanTruoc_Click;

            lblTuan = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(150, 18),
                ForeColor = Color.FromArgb(23, 32, 42)
            };
            CapNhatLabelTuan();

            btnTuanSau = new Button
            {
                Text = "Tuần sau ►",
                Size = new Size(120, 35),
                Location = new Point(350, 12),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTuanSau.Click += BtnTuanSau_Click;

            btnLamMoi = new Button
            {
                Text = "Làm mới",
                Size = new Size(100, 35),
                Location = new Point(490, 12),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLamMoi.Click += BtnLamMoi_Click;

            pnlDieuKhien.Controls.AddRange(new Control[] { btnTuanTruoc, lblTuan, btnTuanSau, btnLamMoi });

            // === DATAGRIDVIEW ===
            dgvLichCa = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            // Style
            dgvLichCa.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 53, 69);
            dgvLichCa.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLichCa.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvLichCa.EnableHeadersVisualStyles = false;

            this.Controls.Add(dgvLichCa);
            this.Controls.Add(pnlDieuKhien);
            this.Controls.Add(pnlTieuDe);
        }

        private void CapNhatLabelTuan()
        {
            DateTime tuanKetThuc = _tuanBatDau.AddDays(6);
            lblTuan.Text = $"Tuần từ {_tuanBatDau:dd/MM/yyyy} đến {tuanKetThuc:dd/MM/yyyy}";
        }

        private void TaiLichLamViec()
        {
            string query = @"
                SELECT 
                    nv.HoTen AS [Nhân viên],
                    cl.TenCa AS [Ca làm],
                    pc.NgayLamViec AS [Ngày],
                    FORMAT(pc.ThoiGianBatDau, 'HH:mm') AS [Giờ bắt đầu],
                    FORMAT(pc.ThoiGianKetThuc, 'HH:mm') AS [Giờ kết thúc],
                    pc.TrangThai AS [Trạng thái],
                    pc.TienDauCa AS [Tiền đầu ca],
                    pc.TienCuoiCa AS [Tiền cuối ca],
                    pc.TienThucTe AS [Tiền thực tế],
                    pc.ChenhLech AS [Chênh lệch]
                FROM PhanCongCa pc
                INNER JOIN NguoiDung nv ON pc.MaNguoiDung = nv.MaNguoiDung
                INNER JOIN CaLamViec cl ON pc.MaCa = cl.MaCa
                WHERE nv.MaChiNhanh = @MaChiNhanh
                  AND nv.VaiTro = N'NhanVien'
                  AND pc.NgayLamViec BETWEEN @TuBatDau AND @TuKetThuc
                ORDER BY pc.NgayLamViec, cl.GioBatDau, nv.HoTen";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuBatDau", _tuanBatDau);
                        cmd.Parameters.AddWithValue("@TuKetThuc", _tuanBatDau.AddDays(6));

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvLichCa.DataSource = dt;

                        // Format tiền tệ
                        foreach (DataGridViewColumn col in dgvLichCa.Columns)
                        {
                            if (col.Name.Contains("Tiền") || col.Name.Contains("Chênh"))
                            {
                                col.DefaultCellStyle.Format = "N0";
                                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                            }
                        }

                        // Màu trạng thái
                        foreach (DataGridViewRow row in dgvLichCa.Rows)
                        {
                            string trangThai = row.Cells["Trạng thái"].Value?.ToString();
                            switch (trangThai)
                            {
                                case "DaKetThuc":
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(212, 237, 218);
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(21, 87, 36);
                                    break;
                                case "DangLam":
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(146, 86, 0);
                                    break;
                                case "ChuaBatDau":
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(225, 236, 244);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch làm việc:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTuanTruoc_Click(object sender, EventArgs e)
        {
            _tuanBatDau = _tuanBatDau.AddDays(-7);
            CapNhatLabelTuan();
            TaiLichLamViec();
        }

        private void BtnTuanSau_Click(object sender, EventArgs e)
        {
            _tuanBatDau = _tuanBatDau.AddDays(7);
            CapNhatLabelTuan();
            TaiLichLamViec();
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            TinhTuanHienTai();
            CapNhatLabelTuan();
            TaiLichLamViec();
        }
    }
}