// File: UC_BaoCaoCaNhan.cs
// Báo cáo cá nhân cho Nhân viên - Xem doanh thu, số vé bán

using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public class UC_BaoCaoCaNhan : UserControl
    {
        private readonly int _maNguoiDung;
        private readonly int _maChiNhanh;
        private readonly string _tenNhanVien;
        private readonly string _chiNhanh;
        
        // CGV Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

        // UI Controls
        private ComboBox cboThang, cboNam;
        private DataGridView dgvChiTiet;
        private Panel pnlStats;
        private Panel _pnlNoData;
        private Label lblTongDoanhThu, lblSoVe, lblSoSanPham, lblKhachHang;

        public UC_BaoCaoCaNhan(int maNguoiDung, int maChiNhanh, string tenNhanVien, string chiNhanh)
        {
            _maNguoiDung = maNguoiDung;
            _maChiNhanh = maChiNhanh;
            _tenNhanVien = tenNhanVien;
            _chiNhanh = chiNhanh;
            
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // === HEADER ===
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = "📊 BÁO CÁO CÁ NHÂN",
                Font = new Font("Montserrat", 22, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(0, 10),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            // Underline
            Panel underline = new Panel
            {
                BackColor = _cgvRed,
                Height = 4,
                Width = 300,
                Location = new Point(0, 55)
            };
            pnlHeader.Controls.Add(underline);

            // Info
            Label lblInfo = new Label
            {
                Text = $"Nhân viên: {_tenNhanVien} | Chi nhánh: {_chiNhanh}",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(0, 70),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblInfo);

            this.Controls.Add(pnlHeader);

            // === FILTER PANEL ===
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = _cgvLightGray,
                Padding = new Padding(10)
            };

            Label lblThang = new Label
            {
                Text = "Tháng:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 18),
                AutoSize = true
            };
            pnlFilter.Controls.Add(lblThang);

            cboThang = new ComboBox
            {
                Width = 80,
                Location = new Point(60, 15),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            for (int i = 1; i <= 12; i++)
                cboThang.Items.Add(i.ToString("D2"));
            cboThang.SelectedIndex = DateTime.Now.Month - 1;
            cboThang.SelectedIndexChanged += (s, e) => LoadData();
            pnlFilter.Controls.Add(cboThang);

            Label lblNam = new Label
            {
                Text = "Năm:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(160, 18),
                AutoSize = true
            };
            pnlFilter.Controls.Add(lblNam);

            cboNam = new ComboBox
            {
                Width = 100,
                Location = new Point(200, 15),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            for (int year = 2020; year <= DateTime.Now.Year + 1; year++)
                cboNam.Items.Add(year.ToString());
            cboNam.SelectedItem = DateTime.Now.Year.ToString();
            cboNam.SelectedIndexChanged += (s, e) => LoadData();
            pnlFilter.Controls.Add(cboNam);

            Button btnLamMoi = new Button
            {
                Text = "🔄 Làm mới",
                Size = new Size(100, 35),
                Location = new Point(320, 12),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLamMoi.FlatAppearance.BorderSize = 0;
            btnLamMoi.Click += (s, e) => LoadData();
            pnlFilter.Controls.Add(btnLamMoi);

            this.Controls.Add(pnlFilter);

            // === STATS PANEL ===
            pnlStats = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Create 4 stat cards
            CreateStatCard(pnlStats, "💰 TỔNG DOANH THU", "0 đ", 0, Color.FromArgb(46, 204, 113), out lblTongDoanhThu);
            CreateStatCard(pnlStats, "🎫 VÉ ĐÃ BÁN", "0", 1, Color.FromArgb(52, 152, 219), out lblSoVe);
            CreateStatCard(pnlStats, "🍿 SẢN PHẨM ĐÃ BÁN", "0", 2, Color.FromArgb(155, 89, 182), out lblSoSanPham);
            CreateStatCard(pnlStats, "👥 KHÁCH PHỤC VỤ", "0", 3, Color.FromArgb(241, 196, 15), out lblKhachHang);

            this.Controls.Add(pnlStats);

            // === DATA GRID ===
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            Label lblChiTiet = new Label
            {
                Text = "📋 Chi tiết doanh thu theo ngày (Tháng này)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlGrid.Controls.Add(lblChiTiet);

            // No data panel
            _pnlNoData = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250),
                Visible = false
            };
            
            Label lblNoDataIcon = new Label
            {
                Text = "📭",
                Font = new Font("Segoe UI", 48),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(0, 30, 0, 0)
            };
            _pnlNoData.Controls.Add(lblNoDataIcon);
            
            Label lblNoDataText = new Label
            {
                Text = "Chưa có doanh thu trong tháng này\nHãy bắt đầu bán vé để xem báo cáo!",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 60
            };
            _pnlNoData.Controls.Add(lblNoDataText);
            
            pnlGrid.Controls.Add(_pnlNoData);

            dgvChiTiet = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };

            // Style header
            dgvChiTiet.ColumnHeadersDefaultCellStyle.BackColor = _cgvRed;
            dgvChiTiet.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvChiTiet.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvChiTiet.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvChiTiet.EnableHeadersVisualStyles = false;
            dgvChiTiet.ColumnHeadersHeight = 40;

            // Alternating rows
            dgvChiTiet.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            pnlGrid.Controls.Add(dgvChiTiet);
            this.Controls.Add(pnlGrid);
        }

        private void CreateStatCard(Panel parent, string title, string value, int index, Color color, out Label valueLabel)
        {
            Panel card = new Panel
            {
                Size = new Size(200, 100),
                Location = new Point(10 + index * 210, 15),
                BackColor = color
            };
            MakeRoundedCorners(card, 10);

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            valueLabel = new Label
            {
                Text = value,
                Font = new Font("Montserrat", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 40),
                AutoSize = true,
                Tag = title
            };
            card.Controls.Add(valueLabel);

            parent.Controls.Add(card);
        }

        private void MakeRoundedCorners(Panel panel, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(panel.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(panel.Width - radius * 2, panel.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, panel.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            panel.Region = new Region(path);
        }

        private void LoadData()
        {
            int thang = int.Parse(cboThang.SelectedItem.ToString());
            int nam = int.Parse(cboNam.SelectedItem.ToString());

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();

                    // Lấy thống kê tổng từ HoaDon và ChiTietHoaDon
                    string statsQuery = @"
                        SELECT 
                            ISNULL(SUM(hd.ThanhTien), 0) as TongDoanhThu,
                            ISNULL(SUM(CASE WHEN ct.MaVe IS NOT NULL THEN 1 ELSE 0 END), 0) as SoVe,
                            ISNULL(SUM(CASE WHEN ct.MaSanPham IS NOT NULL THEN ct.SoLuong ELSE 0 END), 0) as SoSP,
                            COUNT(DISTINCT hd.MaKhachHang) as SoKhach
                        FROM HoaDon hd
                        LEFT JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                        WHERE hd.MaNguoiDung = @UserId
                          AND MONTH(hd.NgayLap) = @Month
                          AND YEAR(hd.NgayLap) = @Year
                          AND hd.TrangThaiThanhToan = N'DaThanhToan'";

                    using (SqlCommand cmd = new SqlCommand(statsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", _maNguoiDung);
                        cmd.Parameters.AddWithValue("@Month", thang);
                        cmd.Parameters.AddWithValue("@Year", nam);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal tongDoanhThu = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                int soVe = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                int soSP = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                int soKhach = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                                lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " đ";
                                lblSoVe.Text = soVe.ToString("N0");
                                lblSoSanPham.Text = soSP.ToString("N0");
                                lblKhachHang.Text = soKhach.ToString("N0");
                            }
                        }
                    }

                    // Lấy chi tiết theo ngày từ HoaDon
                    string detailQuery = @"
                        SELECT 
                            CONVERT(date, hd.NgayLap) as [Ngày],
                            ISNULL(SUM(CASE WHEN ct.MaVe IS NOT NULL THEN 1 ELSE 0 END), 0) as [Số vé],
                            ISNULL(SUM(CASE WHEN ct.MaVe IS NOT NULL THEN ct.ThanhTien ELSE 0 END), 0) as [Doanh thu vé],
                            ISNULL(SUM(CASE WHEN ct.MaSanPham IS NOT NULL THEN ct.SoLuong ELSE 0 END), 0) as [Số SP],
                            ISNULL(SUM(CASE WHEN ct.MaSanPham IS NOT NULL THEN ct.ThanhTien ELSE 0 END), 0) as [Doanh thu SP],
                            ISNULL(SUM(hd.ThanhTien), 0) as [Tổng doanh thu]
                        FROM HoaDon hd
                        LEFT JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                        WHERE hd.MaNguoiDung = @UserId
                          AND MONTH(hd.NgayLap) = @Month
                          AND YEAR(hd.NgayLap) = @Year
                          AND hd.TrangThaiThanhToan = N'DaThanhToan'
                        GROUP BY CONVERT(date, hd.NgayLap)
                        ORDER BY [Ngày] DESC";

                    using (SqlCommand cmd = new SqlCommand(detailQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", _maNguoiDung);
                        cmd.Parameters.AddWithValue("@Month", thang);
                        cmd.Parameters.AddWithValue("@Year", nam);

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dgvChiTiet.DataSource = dt;

                        // Show/hide no data panel
                        bool hasData = dt.Rows.Count > 0;
                        _pnlNoData.Visible = !hasData;
                        dgvChiTiet.Visible = hasData;

                        // Format columns
                        if (dgvChiTiet.Columns.Count > 0)
                        {
                            if (dgvChiTiet.Columns.Contains("Ngày"))
                            {
                                dgvChiTiet.Columns["Ngày"].DefaultCellStyle.Format = "dd/MM/yyyy";
                            }
                            foreach (DataGridViewColumn col in dgvChiTiet.Columns)
                            {
                                if (col.Name.Contains("Doanh thu") || col.Name.Contains("Tổng"))
                                {
                                    col.DefaultCellStyle.Format = "N0";
                                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                                }
                            }
                        }

                        // Color coding
                        foreach (DataGridViewRow row in dgvChiTiet.Rows)
                        {
                            if (row.Cells["Tổng doanh thu"].Value != null)
                            {
                                decimal tongNgay = Convert.ToDecimal(row.Cells["Tổng doanh thu"].Value);
                                if (tongNgay >= 1000000)
                                {
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(212, 237, 218);
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(21, 87, 36);
                                }
                                else if (tongNgay >= 500000)
                                {
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(146, 86, 0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
