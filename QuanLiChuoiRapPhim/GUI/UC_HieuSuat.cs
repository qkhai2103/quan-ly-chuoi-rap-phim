﻿// File: UC_HieuSuat.cs
// V? trí: QuanLiChuoiRapPhim.GUI
// Dành cho vai tr? Qu?n l? chi nhánh

using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_HieuSuat : UserControl
    {
        private readonly int _maChiNhanh;
        private DateTime _tuNgay;
        private DateTime _denNgay;

        private DataGridView dgvHieuSuat;
        private Label lblTieuDeThoiGian, lblTongNhanVien, lblTongDoanhThu;
        private Button btnThangNay, btnLamMoi;
        private ComboBox cboThang;

        public UC_HieuSuat(int maChiNhanh)
        {
            _maChiNhanh = maChiNhanh;

            DatThoiGianMacDinh(); // Tháng hi?n t?i: Tháng 12/2025
            ThietLapGiaoDien();
            TaiDuLieuHieuSuat();
        }

        private void DatThoiGianMacDinh()
        {
            // Ngày hi?n t?i theo ð? bài: 13/12/2025
            DateTime homNay = new DateTime(2025, 12, 13);
            _tuNgay = new DateTime(homNay.Year, homNay.Month, 1);
            _denNgay = _tuNgay.AddMonths(1).AddDays(-1); // Cu?i tháng
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
                BackColor = Color.FromArgb(255, 87, 34) // Cam ð?m
            };
            Label lblTieuDe = new Label
            {
                Text = "ÐÁNH GIÁ HIỆU SUẤT NHÂN VIÊN",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            pnlTieuDe.Controls.Add(lblTieuDe);

            // === THANH L?C TH?I GIAN ===
            Panel pnlLoc = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(20, 12, 20, 10)
            };

            Label lblLoc = new Label
            {
                Text = "Thời gian:",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(5, 20)
            };

            cboThang = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(180, 30),
                Location = new Point(80, 15),
                Font = new Font("Segoe UI", 10F)
            };
            // Thêm các tháng g?n ðây
            for (int i = -6; i <= 0; i++)
            {
                DateTime thang = DateTime.Today.AddMonths(i);
                cboThang.Items.Add(thang.ToString("MM/yyyy"));
            }
            cboThang.SelectedIndex = 6; // Tháng hi?n t?i
            cboThang.SelectedIndexChanged += CboThang_SelectedIndexChanged;

            btnThangNay = new Button
            {
                Text = "Tháng này",
                Size = new Size(100, 35),
                Location = new Point(280, 14),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnThangNay.Click += BtnThangNay_Click;

            btnLamMoi = new Button
            {
                Text = "Làm mới",
                Size = new Size(100, 35),
                Location = new Point(400, 14),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLamMoi.Click += BtnLamMoi_Click;

            lblTieuDeThoiGian = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = true,
                Location = new Point(520, 20)
            };
            CapNhatTieuDeThoiGian();

            pnlLoc.Controls.AddRange(new Control[] { lblLoc, cboThang, btnThangNay, btnLamMoi, lblTieuDeThoiGian });

            // === TH?NG KÊ T?NG ===
            Panel pnlThongKe = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(20, 10, 20, 10)
            };

            lblTongNhanVien = new Label
            {
                Text = "Tổng nhân viên: ...",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 15),
                AutoSize = true
            };

            lblTongDoanhThu = new Label
            {
                Text = "Tổng doanh thu: ...",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(250, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(76, 175, 80)
            };

            pnlThongKe.Controls.AddRange(new Control[] { lblTongNhanVien, lblTongDoanhThu });

            // === DATAGRIDVIEW ===
            dgvHieuSuat = new DataGridView
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

            dgvHieuSuat.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 87, 34);
            dgvHieuSuat.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHieuSuat.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvHieuSuat.EnableHeadersVisualStyles = false;

            this.Controls.Add(dgvHieuSuat);
            this.Controls.Add(pnlThongKe);
            this.Controls.Add(pnlLoc);
            this.Controls.Add(pnlTieuDe);
        }

        private void CapNhatTieuDeThoiGian()
        {
            lblTieuDeThoiGian.Text = $"Từ {_tuNgay:dd/MM/yyyy} đơn {_denNgay:dd/MM/yyyy}";
        }

        private void TaiDuLieuHieuSuat()
        {
            string query = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY SUM(hd.ThanhTien) DESC) AS [ThuHang],
                    nv.HoTen AS [NhanVien],
                    COUNT(hd.MaHoaDon) AS [SoHoaDon],
                    SUM(hd.ThanhTien) AS [DoanhThu],
                    AVG(hd.ThanhTien) AS [TrungBinh]
                FROM HoaDon hd
                INNER JOIN NguoiDung nv ON hd.MaNguoiDung = nv.MaNguoiDung
                WHERE nv.MaChiNhanh = @MaChiNhanh
                  AND nv.VaiTro = N'NhanVien'
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND hd.NgayLap BETWEEN @TuNgay AND @DenNgay
                GROUP BY nv.MaNguoiDung, nv.HoTen
                ORDER BY [DoanhThu] DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", _tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", _denNgay.AddDays(1).AddSeconds(-1)); // Ð?n cu?i ngày

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvHieuSuat.DataSource = dt;

                        // Rename columns for display
                        if (dgvHieuSuat.Columns.Contains("ThuHang")) dgvHieuSuat.Columns["ThuHang"].HeaderText = "Thứ hạng";
                        if (dgvHieuSuat.Columns.Contains("NhanVien")) dgvHieuSuat.Columns["NhanVien"].HeaderText = "Nhân viên";
                        if (dgvHieuSuat.Columns.Contains("SoHoaDon")) dgvHieuSuat.Columns["SoHoaDon"].HeaderText = "Số hóa đơn";
                        if (dgvHieuSuat.Columns.Contains("DoanhThu")) dgvHieuSuat.Columns["DoanhThu"].HeaderText = "Doanh thu";
                        if (dgvHieuSuat.Columns.Contains("TrungBinh")) dgvHieuSuat.Columns["TrungBinh"].HeaderText = "TB/hóa đơn";

                        // Format currency
                        if (dgvHieuSuat.Columns.Contains("DoanhThu"))
                        {
                            dgvHieuSuat.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                            dgvHieuSuat.Columns["DoanhThu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        }
                        
                        if (dgvHieuSuat.Columns.Contains("TrungBinh"))
                        {
                            dgvHieuSuat.Columns["TrungBinh"].DefaultCellStyle.Format = "N0";
                            dgvHieuSuat.Columns["TrungBinh"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        }

                        // Màu th? h?ng top 3
                        for (int i = 0; i < dgvHieuSuat.Rows.Count && i < 3; i++)
                        {
                            DataGridViewRow row = dgvHieuSuat.Rows[i];
                            row.DefaultCellStyle.BackColor = i == 0 ? Color.FromArgb(255, 215, 0) :   // Vàng cho top 1
                                                             i == 1 ? Color.FromArgb(192, 192, 192) : // B?c
                                                                      Color.FromArgb(205, 127, 50);   // Ð?ng
                            row.DefaultCellStyle.ForeColor = Color.Black;
                            row.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                        }

                        // Tính tổng doanh thu
                        decimal tongDoanhThu = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            tongDoanhThu += Convert.ToDecimal(row["DoanhThu"]);
                        }

                        lblTongNhanVien.Text = $"Tổng nhân viên có doanh thu: {dt.Rows.Count} người";
                        lblTongDoanhThu.Text = $"Tổng doanh thu chi nhánh: {tongDoanhThu:N0} đ";
                        if (dt.Rows.Count == 0)
                        {
                            MessageBox.Show($"Chưa có dữ liệu doanh thu trong khoảng thời gian này.\\n" +
                                $"Từ: {_tuNgay:dd/MM/yyyy} - Đến: {_denNgay:dd/MM/yyyy}\\n\\n" +
                                $"Lưu ý: Tab này hiển thị doanh thu từ HÓA ĐƠN bán vé, không phải phân công ca.",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu hiệu suất:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CapNhatThoiGianTuCombo()
        {
            if (cboThang.SelectedItem == null) return;
            string[] parts = cboThang.SelectedItem.ToString().Split('/');
            int thang = int.Parse(parts[0]);
            int nam = int.Parse(parts[1]);

            _tuNgay = new DateTime(nam, thang, 1);
            _denNgay = _tuNgay.AddMonths(1).AddDays(-1);
            CapNhatTieuDeThoiGian();
            TaiDuLieuHieuSuat();
        }

        private void CboThang_SelectedIndexChanged(object sender, EventArgs e)
        {
            CapNhatThoiGianTuCombo();
        }

        private void BtnThangNay_Click(object sender, EventArgs e)
        {
            DatThoiGianMacDinh();
            cboThang.SelectedIndex = 6; // Tháng hi?n t?i
            CapNhatTieuDeThoiGian();
            TaiDuLieuHieuSuat();
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            TaiDuLieuHieuSuat();
        }
    }
}
