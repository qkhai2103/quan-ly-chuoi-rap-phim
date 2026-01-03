using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_BaoCaoChiNhanh : UserControl
    {
        private readonly int _maChiNhanh;           // Chi nhánh của Manager
        private readonly string _tenChiNhanh;       // Tên chi nhánh

        // Controls
        private TabControl tabMain;
        private TabPage tabTongQuan, tabDoanhThu, tabSanPham, tabPhim, tabNhanVien;
        private DateTimePicker dtpTuNgay, dtpDenNgay;
        private Button btnTaoBaoCao, btnXuatExcel, btnInBaoCao;
        private ComboBox cboLoaiBaoCao;

        // Charts
        private Chart chartDoanhThu, chartSanPham, chartPhim, chartNhanVien;

        // DataGridViews
        private DataGridView dgvTongQuan, dgvDoanhThu, dgvSanPham, dgvPhim, dgvNhanVien;

        // Labels thống kê
        private Label lblTongDoanhThu, lblTongHoaDon, lblKhachHangTB, lblDoanhThuTB;
        private Label lblTopPhim, lblTopSanPham, lblNhanVienXuatSac;
        private bool _isLoaded = false;
        
        // Print support
        private DataGridView _printDataGridView;
        private string _printReportTitle;
        private int _printCurrentPage = 0;

        public UC_BaoCaoChiNhanh(int maChiNhanh, string tenChiNhanh)
        {
            _maChiNhanh = maChiNhanh;
            _tenChiNhanh = tenChiNhanh;

            ThietLapGiaoDien();
            
            // Load data when control is shown - more reliable than HandleCreated
            this.Load += (s, e) =>
            {
                if (!_isLoaded)
                {
                    _isLoaded = true;
                    // Use BeginInvoke to ensure UI is fully rendered
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            btnTaoBaoCao.Text = "⏳ Đang tải...";
                            btnTaoBaoCao.Enabled = false;
                            TaiBaoCaoMacDinh();
                        }
                        finally
                        {
                            btnTaoBaoCao.Text = "📊 TẠO BÁO CÁO";
                            btnTaoBaoCao.Enabled = true;
                        }
                    }));
                }
            };
        }

        private void ThietLapGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // === TIÊU Ð? ===
            Panel pnlTieuDe = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0, 0, 0) // Black
            };

            Label lblTieuDe = new Label
            {
                Text = $" BÁO CÁO CHI NHÁNH: {_tenChiNhanh.ToUpper()}",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            pnlTieuDe.Controls.Add(lblTieuDe);

            // === THANH CÔNG C? ===
            Panel pnlCongCu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 10, 20, 10)
            };

            // Chọn loại báo cáo
            Label lblLoaiBaoCao = new Label
            {
                Text = "Loại báo cáo:",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 15),
                AutoSize = true
            };

            cboLoaiBaoCao = new ComboBox
            {
                Size = new Size(200, 30),
                Location = new Point(120, 10),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLoaiBaoCao.Items.AddRange(new string[]
            {
                "Tổng quan",
                "Doanh thu theo ngày",
                "Sản phẩm bán chạy",
                "Phim bán chạy",
                "Hiệu suất nhân viên"
            });
            cboLoaiBaoCao.SelectedIndex = 0;
            cboLoaiBaoCao.SelectedIndexChanged += CboLoaiBaoCao_SelectedIndexChanged;

            // Chọn thời gian
            Label lblTuNgay = new Label
            {
                Text = "Từ:",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(340, 15),
                AutoSize = true
            };

            dtpTuNgay = new DateTimePicker
            {
                Size = new Size(120, 30),
                Location = new Point(370, 10),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-1)
            };

            Label lblDenNgay = new Label
            {
                Text = "Đến:",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(500, 15),
                AutoSize = true
            };

            dtpDenNgay = new DateTimePicker
            {
                Size = new Size(120, 30),
                Location = new Point(540, 10),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            // Nút tạo báo cáo
            btnTaoBaoCao = new Button
            {
                Text = "📊 TẠO BÁO CÁO",
                Size = new Size(150, 35),
                Location = new Point(680, 8),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnTaoBaoCao.Click += BtnTaoBaoCao_Click;

            pnlCongCu.Controls.AddRange(new Control[]
            {
                lblLoaiBaoCao, cboLoaiBaoCao,
                lblTuNgay, dtpTuNgay,
                lblDenNgay, dtpDenNgay,
                btnTaoBaoCao
            });

            // === TH?NG KÊ NHANH ===
            Panel pnlThongKeNhanh = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            lblTongDoanhThu = TaoLabelThongKe("TỔNG DOANH THU: 0 ₫", Color.FromArgb(40, 167, 69), new Point(20, 25));
            lblTongHoaDon = TaoLabelThongKe("TỔNG HÓA ĐƠN: 0", Color.FromArgb(0, 123, 255), new Point(250, 25));
            lblKhachHangTB = TaoLabelThongKe("KHÁCH HÀNG: 0", Color.FromArgb(255, 193, 7), new Point(450, 25));
            lblDoanhThuTB = TaoLabelThongKe("DOANH THU TB: 0 ₫", Color.FromArgb(220, 53, 69), new Point(650, 25));

            pnlThongKeNhanh.Controls.AddRange(new Control[]
            {
                lblTongDoanhThu, lblTongHoaDon,
                lblKhachHangTB, lblDoanhThuTB
            });

            // === TAB CONTROL ===
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };

            tabTongQuan = new TabPage("📊 TỔNG QUAN");
            tabDoanhThu = new TabPage("💰 DOANH THU");
            tabSanPham = new TabPage("🍿 SẢN PHẨM");
            tabPhim = new TabPage("🎬 PHIM");
            tabNhanVien = new TabPage("👥 NHÂN VIÊN");

            tabMain.TabPages.AddRange(new TabPage[]
            {
                tabTongQuan, tabDoanhThu, tabSanPham, tabPhim, tabNhanVien
            });

            // Thi?t l?p t?ng tab
            ThietLapTabTongQuan();
            ThietLapTabDoanhThu();
            ThietLapTabSanPham();
            ThietLapTabPhim();
            ThietLapTabNhanVien();

            // === PANEL NÚT XU?T ===
            Panel pnlNutXuat = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 245)
            };

            btnXuatExcel = new Button
            {
                Text = "📊 XUẤT EXCEL",
                Size = new Size(150, 40),
                Location = new Point(300, 10),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnXuatExcel.Click += BtnXuatExcel_Click;

            btnInBaoCao = new Button
            {
                Text = "🖨️ IN BÁO CÁO",
                Size = new Size(150, 40),
                Location = new Point(470, 10),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnInBaoCao.Click += BtnInBaoCao_Click;

            pnlNutXuat.Controls.AddRange(new Control[] { btnXuatExcel, btnInBaoCao });

            // Thêm controls vào UserControl
            this.Controls.Add(pnlNutXuat);
            this.Controls.Add(tabMain);
            this.Controls.Add(pnlThongKeNhanh);
            this.Controls.Add(pnlCongCu);
            this.Controls.Add(pnlTieuDe);
        }

        private Label TaoLabelThongKe(string text, Color color, Point location)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = color,
                Location = location,
                Size = new Size(200, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
        }

        private void ThietLapTabTongQuan()
        {
            tabTongQuan.Padding = new Padding(10);

            // Panel th?ng kê n?i b?t
            Panel pnlNoiBat = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.Transparent
            };

            lblTopPhim = new Label
            {
                Text = "?? PHIM BÁN CHẠY: Ðang tải...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(20, 20),
                Size = new Size(400, 25)
            };

            lblTopSanPham = new Label
            {
                Text = "?? SẢN PHẨM BÁN CHẠY: Ðang tải...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(20, 50),
                Size = new Size(400, 25)
            };

            lblNhanVienXuatSac = new Label
            {
                Text = "?? NHÂN VIÊN XUẤT SẮC: Ðang tải...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(20, 80),
                Size = new Size(400, 25)
            };

            pnlNoiBat.Controls.AddRange(new Control[] { lblTopPhim, lblTopSanPham, lblNhanVienXuatSac });

            // DataGridView t?ng quan
            dgvTongQuan = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            tabTongQuan.Controls.Add(dgvTongQuan);
            tabTongQuan.Controls.Add(pnlNoiBat);
        }

        private void ThietLapTabDoanhThu()
        {
            tabDoanhThu.Padding = new Padding(10);

            // Chart doanh thu
            chartDoanhThu = new Chart
            {
                Dock = DockStyle.Top,
                Height = 300,
                MinimumSize = new Size(100, 100)
            };
            TaoChartDoanhThu();

            // DataGridView doanh thu chi ti?t
            dgvDoanhThu = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            tabDoanhThu.Controls.Add(dgvDoanhThu);
            tabDoanhThu.Controls.Add(chartDoanhThu);
        }

        private void ThietLapTabSanPham()
        {
            tabSanPham.Padding = new Padding(10);

            // Chart sản phẩm
            chartSanPham = new Chart
            {
                Dock = DockStyle.Top,
                Height = 300,
                MinimumSize = new Size(100, 100)
            };
            TaoChartSanPham();

            // DataGridView s?n ph?m
            dgvSanPham = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            tabSanPham.Controls.Add(dgvSanPham);
            tabSanPham.Controls.Add(chartSanPham);
        }

        private void ThietLapTabPhim()
        {
            tabPhim.Padding = new Padding(10);

            // Chart phim
            chartPhim = new Chart
            {
                Dock = DockStyle.Top,
                Height = 300,
                MinimumSize = new Size(100, 100)
            };
            TaoChartPhim();

            // DataGridView phim
            dgvPhim = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            tabPhim.Controls.Add(dgvPhim);
            tabPhim.Controls.Add(chartPhim);
        }

        private void ThietLapTabNhanVien()
        {
            tabNhanVien.Padding = new Padding(10);

            // Chart nhân viên
            chartNhanVien = new Chart
            {
                Dock = DockStyle.Top,
                Height = 300,
                MinimumSize = new Size(100, 100)
            };
            TaoChartNhanVien();

            // DataGridView nhân viên
            dgvNhanVien = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            tabNhanVien.Controls.Add(dgvNhanVien);
            tabNhanVien.Controls.Add(chartNhanVien);
        }

        private void TaoChartDoanhThu()
        {
            chartDoanhThu.ChartAreas.Clear();
            chartDoanhThu.Series.Clear();
            chartDoanhThu.Titles.Clear();

            ChartArea chartArea = new ChartArea("DoanhThu");
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.LabelStyle.Interval = 1;
            chartDoanhThu.ChartAreas.Add(chartArea);

            Title title = new Title("DOANH THU THEO NGÀY", Docking.Top,
                new Font("Segoe UI", 14, FontStyle.Bold), Color.FromArgb(52, 73, 94));
            chartDoanhThu.Titles.Add(title);
        }

        private void TaoChartSanPham()
        {
            chartSanPham.ChartAreas.Clear();
            chartSanPham.Series.Clear();
            chartSanPham.Titles.Clear();

            ChartArea chartArea = new ChartArea("SanPham");
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.LabelStyle.Interval = 1;
            chartSanPham.ChartAreas.Add(chartArea);

            Title title = new Title("TOP SẢN PHẨM BÁN CHẠY", Docking.Top,
                new Font("Segoe UI", 14, FontStyle.Bold), Color.FromArgb(52, 73, 94));
            chartSanPham.Titles.Add(title);
        }

        private void TaoChartPhim()
        {
            chartPhim.ChartAreas.Clear();
            chartPhim.Series.Clear();
            chartPhim.Titles.Clear();

            ChartArea chartArea = new ChartArea("Phim");
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.LabelStyle.Interval = 1;
            chartPhim.ChartAreas.Add(chartArea);

            Title title = new Title("TOP PHIM BÁN CHẠY", Docking.Top,
                new Font("Segoe UI", 14, FontStyle.Bold), Color.FromArgb(52, 73, 94));
            chartPhim.Titles.Add(title);
        }

        private void TaoChartNhanVien()
        {
            chartNhanVien.ChartAreas.Clear();
            chartNhanVien.Series.Clear();
            chartNhanVien.Titles.Clear();

            ChartArea chartArea = new ChartArea("NhanVien");
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.LabelStyle.Interval = 1;
            chartNhanVien.ChartAreas.Add(chartArea);

            Title title = new Title("HIỆU SUẤT NHÂN VIÊN", Docking.Top,
                new Font("Segoe UI", 14, FontStyle.Bold), Color.FromArgb(52, 73, 94));
            chartNhanVien.Titles.Add(title);
        }

        private void TaiBaoCaoMacDinh()
        {
            DateTime tuNgay = dtpTuNgay.Value.Date;
            DateTime denNgay = dtpDenNgay.Value.Date;

            TaiBaoCaoTongQuan(tuNgay, denNgay);
            TaiBaoCaoDoanhThu(tuNgay, denNgay);
            TaiBaoCaoSanPham(tuNgay, denNgay);
            TaiBaoCaoPhim(tuNgay, denNgay);
            TaiBaoCaoNhanVien(tuNgay, denNgay);
            CapNhatThongKeNhanh(tuNgay, denNgay);
        }

        private void TaiBaoCaoTongQuan(DateTime tuNgay, DateTime denNgay)
        {
            string query = @"
                SELECT 
                    N'Số hóa đơn' AS ChiTieu,
                    COUNT(DISTINCT hd.MaHoaDon) AS GiaTri,
                    N'Tổng số hóa đơn' AS MoTa
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                
                UNION ALL
                
                SELECT 
                    N'Tổng doanh thu',
                    SUM(hd.ThanhTien),
                    N'Doanh thu thực tế (đã trừ giảm giá)'
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                
                UNION ALL
                
                SELECT 
                    N'Doanh thu trung bình',
                    AVG(hd.ThanhTien),
                    N'Trung bình mỗi hóa đơn'
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                
                UNION ALL
                
                SELECT 
                    N'Số vé bán',
                    COUNT(ct.MaVe),
                    N'Tổng số vé đã bán'
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                INNER JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND ct.MaVe IS NOT NULL
                
                UNION ALL
                
                SELECT 
                    N'Sản phẩm bán',
                    SUM(ct.SoLuong),
                    N'Tổng sản phẩm bắp nước'
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                INNER JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND ct.MaSanPham IS NOT NULL";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvTongQuan.DataSource = dt;
                        DinhDangDataGridView(dgvTongQuan, "TONG_QUAN");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo tổng quan: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiBaoCaoDoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            string query = @"
                SELECT 
                    CAST(hd.NgayLap AS DATE) AS Ngay,
                    COUNT(DISTINCT hd.MaHoaDon) AS SoHoaDon,
                    SUM(hd.ThanhTien) AS DoanhThu,
                    AVG(hd.ThanhTien) AS DoanhThuTB,
                    SUM(hd.GiamGia) AS GiamGia
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                GROUP BY CAST(hd.NgayLap AS DATE)
                ORDER BY Ngay";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvDoanhThu.DataSource = dt;
                        DinhDangDataGridView(dgvDoanhThu, "DOANH_THU");
                        VeBieuDoDoanhThu(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo doanh thu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiBaoCaoSanPham(DateTime tuNgay, DateTime denNgay)
        {
            string query = @"
                SELECT TOP 10
                    sp.TenSanPham,
                    sp.LoaiSanPham,
                    SUM(ct.SoLuong) AS SoLuongBan,
                    SUM(ct.ThanhTien) AS DoanhThu,
                    AVG(ct.DonGia) AS DonGiaTB
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                INNER JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                INNER JOIN SanPham sp ON ct.MaSanPham = sp.MaSanPham
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                GROUP BY sp.TenSanPham, sp.LoaiSanPham
                ORDER BY SoLuongBan DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvSanPham.DataSource = dt;
                        DinhDangDataGridView(dgvSanPham, "SAN_PHAM");
                        VeBieuDoSanPham(dt);

                        // Cập nhật top sản phẩm
                        if (dt.Rows.Count > 0)
                        {
                            lblTopSanPham.Text = $"🍿 SẢN PHẨM BÁN CHẠY: {dt.Rows[0]["TenSanPham"]} ({dt.Rows[0]["SoLuongBan"]} phần)";
                        }
                        else
                        {
                            lblTopSanPham.Text = "🍿 SẢN PHẨM BÁN CHẠY: Chưa có dữ liệu trong kỳ này";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo sản phẩm: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiBaoCaoPhim(DateTime tuNgay, DateTime denNgay)
        {
            string query = @"
                SELECT TOP 10
                    p.TenPhim,
                    p.TheLoai,
                    COUNT(v.MaVe) AS SoVeBan,
                    SUM(v.GiaVe) AS DoanhThu,
                    AVG(v.GiaVe) AS GiaVeTB
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                INNER JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                INNER JOIN Ve v ON ct.MaVe = v.MaVe
                INNER JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                GROUP BY p.TenPhim, p.TheLoai
                ORDER BY SoVeBan DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvPhim.DataSource = dt;
                        DinhDangDataGridView(dgvPhim, "PHIM");
                        VeBieuDoPhim(dt);

                        // Cập nhật top phim
                        if (dt.Rows.Count > 0)
                        {
                            lblTopPhim.Text = $"🎬 PHIM BÁN CHẠY: {dt.Rows[0]["TenPhim"]} ({dt.Rows[0]["SoVeBan"]} vé)";
                        }
                        else
                        {
                            lblTopPhim.Text = "🎬 PHIM BÁN CHẠY: Chưa có dữ liệu trong kỳ này";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo phim: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiBaoCaoNhanVien(DateTime tuNgay, DateTime denNgay)
        {
            string query = @"
                SELECT 
                    nd.HoTen,
                    nd.TenDangNhap,
                    COUNT(hd.MaHoaDon) AS SoHoaDon,
                    SUM(hd.ThanhTien) AS DoanhThu,
                    AVG(hd.ThanhTien) AS DoanhThuTB,
                    MIN(hd.NgayLap) AS NgayBatDau,
                    MAX(hd.NgayLap) AS NgayGanNhat
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND nd.VaiTro = N'Nhân viên'
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                GROUP BY nd.HoTen, nd.TenDangNhap
                ORDER BY DoanhThu DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvNhanVien.DataSource = dt;
                        DinhDangDataGridView(dgvNhanVien, "NHAN_VIEN");
                        VeBieuDoNhanVien(dt);

                        // Cập nhật nhân viên xuất sắc
                        if (dt.Rows.Count > 0)
                        {
                            lblNhanVienXuatSac.Text = $"👥 NHÂN VIÊN XUẤT SẮC: {dt.Rows[0]["HoTen"]} ({dt.Rows[0]["DoanhThu"]:N0} ₫)";
                        }
                        else
                        {
                            lblNhanVienXuatSac.Text = "👥 NHÂN VIÊN XUẤT SẮC: Chưa có dữ liệu trong kỳ này";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo nhân viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CapNhatThongKeNhanh(DateTime tuNgay, DateTime denNgay)
        {
            string query = @"
                SELECT 
                    COUNT(DISTINCT hd.MaHoaDon) AS TongHoaDon,
                    SUM(hd.ThanhTien) AS TongDoanhThu,
                    AVG(hd.ThanhTien) AS DoanhThuTB,
                    COUNT(DISTINCT hd.MaKhachHang) AS KhachHang
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal tongDoanhThu = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                                int tongHoaDon = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                decimal doanhThuTB = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                                int khachHang = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                                lblTongDoanhThu.Text = $"TỔNG DOANH THU: {tongDoanhThu:N0} ₫";
                                lblTongHoaDon.Text = $"TỔNG HÓA ĐƠN: {tongHoaDon}";
                                lblKhachHangTB.Text = $"KHÁCH HÀNG: {khachHang}";
                                lblDoanhThuTB.Text = $"DOANH THU TB: {doanhThuTB:N0} ₫";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật thống kê nhanh: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DinhDangDataGridView(DataGridView dgv, string loaiBaoCao)
        {
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 123, 255);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            // Format s?
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                if (column.ValueType == typeof(decimal) ||
                    column.Name.Contains("DoanhThu") ||
                    column.Name.Contains("Gia") ||
                    column.Name.Contains("GiaTri"))
                {
                    column.DefaultCellStyle.Format = "N0";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.ForeColor = Color.Blue;
                }

                if (column.Name.Contains("Ngay"))
                {
                    column.DefaultCellStyle.Format = "dd/MM/yyyy";
                }
            }
        }

        private void VeBieuDoDoanhThu(DataTable dt)
        {
            try
            {
                // Check chart is ready
                if (chartDoanhThu == null || chartDoanhThu.Width <= 50 || chartDoanhThu.Height <= 50 || chartDoanhThu.ChartAreas.Count == 0)
                    return;

                chartDoanhThu.Series.Clear();

                Series seriesDoanhThu = new Series("Doanh thu");
                seriesDoanhThu.ChartType = SeriesChartType.Column;
                seriesDoanhThu.Color = Color.FromArgb(0, 123, 255);
                seriesDoanhThu.IsValueShownAsLabel = true;
                seriesDoanhThu.LabelFormat = "N0";

                Series seriesHoaDon = new Series("Số hóa đơn");
                seriesHoaDon.ChartType = SeriesChartType.Line;
                seriesHoaDon.Color = Color.FromArgb(220, 53, 69);
                seriesHoaDon.BorderWidth = 3;
                seriesHoaDon.YAxisType = AxisType.Secondary;

                int maxItems = Math.Min(dt.Rows.Count, 15);
                for (int i = 0; i < maxItems; i++)
                {
                    DataRow row = dt.Rows[i];
                    string label = Convert.ToDateTime(row["Ngay"]).ToString("dd/MM");
                    decimal doanhThu = Convert.ToDecimal(row["DoanhThu"]);
                    int soHoaDon = Convert.ToInt32(row["SoHoaDon"]);

                    seriesDoanhThu.Points.AddXY(label, doanhThu);
                    seriesHoaDon.Points.AddXY(label, soHoaDon);
                }

                chartDoanhThu.Series.Add(seriesDoanhThu);
                chartDoanhThu.Series.Add(seriesHoaDon);

                // Thiết lập trục Y phụ
                chartDoanhThu.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                chartDoanhThu.ChartAreas[0].AxisY2.LabelStyle.Format = "N0";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VeBieuDoDoanhThu error: {ex.Message}");
            }
        }

        private void VeBieuDoSanPham(DataTable dt)
        {
            try
            {
                if (chartSanPham == null || chartSanPham.Width <= 50 || chartSanPham.Height <= 50 || chartSanPham.ChartAreas.Count == 0)
                    return;

                chartSanPham.Series.Clear();

                Series series = new Series("Số lượng bán");
                series.ChartType = SeriesChartType.Bar;
                series.Color = Color.FromArgb(40, 167, 69);
                series.IsValueShownAsLabel = true;
                series.LabelFormat = "N0";

                int maxItems = Math.Min(dt.Rows.Count, 10);
                for (int i = 0; i < maxItems; i++)
                {
                    DataRow row = dt.Rows[i];
                    string tenSP = row["TenSanPham"].ToString();
                    if (tenSP.Length > 15) tenSP = tenSP.Substring(0, 12) + "...";
                    int soLuong = Convert.ToInt32(row["SoLuongBan"]);

                    series.Points.AddXY(tenSP, soLuong);
                    series.Points[i].LabelToolTip = $"{row["TenSanPham"]}\nDoanh thu: {Convert.ToDecimal(row["DoanhThu"]):N0} đ";
                }

                chartSanPham.Series.Add(series);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VeBieuDoSanPham error: {ex.Message}");
            }
        }

        private void VeBieuDoPhim(DataTable dt)
        {
            try
            {
                if (chartPhim == null || chartPhim.Width <= 50 || chartPhim.Height <= 50 || chartPhim.ChartAreas.Count == 0)
                    return;

                chartPhim.Series.Clear();

                Series seriesVe = new Series("Số vé bán");
                seriesVe.ChartType = SeriesChartType.Column;
                seriesVe.Color = Color.FromArgb(255, 193, 7);
                seriesVe.IsValueShownAsLabel = true;
                seriesVe.LabelFormat = "N0";

                Series seriesDoanhThu = new Series("Doanh thu");
                seriesDoanhThu.ChartType = SeriesChartType.Line;
                seriesDoanhThu.Color = Color.FromArgb(0, 0, 0);
                seriesDoanhThu.BorderWidth = 3;
                seriesDoanhThu.YAxisType = AxisType.Secondary;

                int maxItems = Math.Min(dt.Rows.Count, 8);
                for (int i = 0; i < maxItems; i++)
                {
                    DataRow row = dt.Rows[i];
                    string tenPhim = row["TenPhim"].ToString();
                    if (tenPhim.Length > 12) tenPhim = tenPhim.Substring(0, 10) + "...";
                    int soVe = Convert.ToInt32(row["SoVeBan"]);
                    decimal doanhThu = Convert.ToDecimal(row["DoanhThu"]);

                    seriesVe.Points.AddXY(tenPhim, soVe);
                    seriesDoanhThu.Points.AddXY(tenPhim, doanhThu);
                }

                chartPhim.Series.Add(seriesVe);
                chartPhim.Series.Add(seriesDoanhThu);

                chartPhim.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                chartPhim.ChartAreas[0].AxisY2.LabelStyle.Format = "N0";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VeBieuDoPhim error: {ex.Message}");
            }
        }

        private void VeBieuDoNhanVien(DataTable dt)
        {
            try
            {
                if (chartNhanVien == null || chartNhanVien.Width <= 50 || chartNhanVien.Height <= 50 || chartNhanVien.ChartAreas.Count == 0)
                    return;

                chartNhanVien.Series.Clear();

                Series series = new Series("Doanh thu");
                series.ChartType = SeriesChartType.Pie;
                series.IsValueShownAsLabel = true;
                series.LabelFormat = "N0";
                series.Label = "#PERCENT{P1}";

                int maxItems = Math.Min(dt.Rows.Count, 6);
                for (int i = 0; i < maxItems; i++)
                {
                    DataRow row = dt.Rows[i];
                    string tenNV = row["HoTen"].ToString();
                    decimal doanhThu = Convert.ToDecimal(row["DoanhThu"]);

                    DataPoint point = series.Points.Add((double)doanhThu);
                    point.LegendText = tenNV;
                    point.LabelToolTip = $"{tenNV}\nDoanh thu: {doanhThu:N0} đ\nHóa đơn: {row["SoHoaDon"]}";

                    // Màu sắc khác nhau
                    point.Color = GetColorForIndex(i);
                }

                chartNhanVien.Series.Add(series);
                if (chartNhanVien.Legends.Count > 0)
                    chartNhanVien.Legends[0].Enabled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VeBieuDoNhanVien error: {ex.Message}");
            }
        }

        private Color GetColorForIndex(int index)
        {
            Color[] colors = new Color[]
            {
                Color.FromArgb(0, 0, 0),        // Đen
                Color.FromArgb(40, 167, 69),    // Xanh lá
                Color.FromArgb(255, 193, 7),    // Vàng
                Color.FromArgb(220, 53, 69),    // Ð?
                Color.FromArgb(20, 20, 20),     // Đen đậm
                Color.FromArgb(23, 162, 184)    // Xanh ng?c
            };

            return colors[index % colors.Length];
        }

        private void TaiBaoCaoChiTiet()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_BaoCaoChiTietChiNhanh", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", dtpTuNgay.Value.Date);
                        cmd.Parameters.AddWithValue("@DenNgay", dtpDenNgay.Value.Date);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);

                            if (ds.Tables.Count > 0)
                            {
                                // Hi?n th? k?t qu?
                                // (Có th? t?o m?t form chi ti?t riêng)
                                MessageBox.Show($"Ðã tạo báo cáo chi tiết {ds.Tables.Count} phân", "Thông báo",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo báo cáo chi tiết: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== S? KI?N ====================

        private void CboLoaiBaoCao_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Chuy?n ð?n tab týõng ?ng
            tabMain.SelectedIndex = cboLoaiBaoCao.SelectedIndex;
        }

        private void BtnTaoBaoCao_Click(object sender, EventArgs e)
        {
            if (dtpTuNgay.Value > dtpDenNgay.Value)
            {
                MessageBox.Show("Ngày bắt đầu không lớn hơn ngày kết thúc!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            btnTaoBaoCao.Enabled = false;
            btnTaoBaoCao.Text = "⏳ ĐANG XỬ LÝ...";

            try
            {
                DateTime tuNgay = dtpTuNgay.Value.Date;
                DateTime denNgay = dtpDenNgay.Value.Date;

                TaiBaoCaoTongQuan(tuNgay, denNgay);
                TaiBaoCaoDoanhThu(tuNgay, denNgay);
                TaiBaoCaoSanPham(tuNgay, denNgay);
                TaiBaoCaoPhim(tuNgay, denNgay);
                TaiBaoCaoNhanVien(tuNgay, denNgay);
                CapNhatThongKeNhanh(tuNgay, denNgay);

                // Auto-switch to selected tab to show results
                int selectedIndex = cboLoaiBaoCao.SelectedIndex;
                if (selectedIndex >= 0 && selectedIndex < tabMain.TabCount)
                {
                    tabMain.SelectedIndex = selectedIndex;
                }

                // Force refresh all charts and grids
                chartDoanhThu.Invalidate();
                chartDoanhThu.Update();
                chartSanPham.Invalidate();
                chartSanPham.Update();
                chartPhim.Invalidate();
                chartPhim.Update();
                chartNhanVien.Invalidate();
                chartNhanVien.Update();
                
                this.Refresh();
                
                MessageBox.Show("Đã cập nhật báo cáo thành công!\n\nDữ liệu đã được tải và hiển thị.", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo báo cáo: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnTaoBaoCao.Enabled = true;
                btnTaoBaoCao.Text = "📊 TẠO BÁO CÁO";
            }
        }

        private void BtnXuatExcel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Xuất báo cáo ra file Excel?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx";
                    saveDialog.FilterIndex = 1; // Default to CSV
                    saveDialog.FileName = $"BaoCao_{_tenChiNhanh.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor;
                        btnXuatExcel.Enabled = false;
                        
                        // Get the current tab's DataGridView
                        DataGridView dgvCurrent = null;
                        string reportTitle = "";
                        
                        switch (tabMain.SelectedIndex)
                        {
                            case 0:
                                dgvCurrent = dgvTongQuan;
                                reportTitle = "TỔNG QUAN";
                                break;
                            case 1:
                                dgvCurrent = dgvDoanhThu;
                                reportTitle = "DOANH THU";
                                break;
                            case 2:
                                dgvCurrent = dgvSanPham;
                                reportTitle = "SẢN PHẨM";
                                break;
                            case 3:
                                dgvCurrent = dgvPhim;
                                reportTitle = "PHIM";
                                break;
                            case 4:
                                dgvCurrent = dgvNhanVien;
                                reportTitle = "NHÂN VIÊN";
                                break;
                        }

                        if (dgvCurrent != null && dgvCurrent.DataSource != null)
                        {
                            ExportToCSV(dgvCurrent, saveDialog.FileName, reportTitle);
                            MessageBox.Show($"Đã xuất báo cáo thành công!\n\nFile: {saveDialog.FileName}", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không có dữ liệu để xuất!", "Cảnh báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                    btnXuatExcel.Enabled = true;
                }
            }
        }

        private void ExportToCSV(DataGridView dgv, string filePath, string reportTitle)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Write UTF-8 BOM for Excel compatibility
                sw.Write('\uFEFF');
                
                // Write header info with better formatting
                sw.WriteLine($"╔════════════════════════════════════════════════════════════════╗");
                sw.WriteLine($"║  BÁO CÁO CHI NHÁNH: {_tenChiNhanh.PadRight(40)} ║");
                sw.WriteLine($"╠════════════════════════════════════════════════════════════════╣");
                sw.WriteLine($"║  Loại báo cáo: {reportTitle.PadRight(47)} ║");
                sw.WriteLine($"║  Từ ngày: {dtpTuNgay.Value:dd/MM/yyyy} - Đến ngày: {dtpDenNgay.Value:dd/MM/yyyy}              ║");
                sw.WriteLine($"║  Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}                          ║");
                sw.WriteLine($"╚════════════════════════════════════════════════════════════════╝");
                sw.WriteLine();

                // Write column headers
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].Visible)
                    {
                        sw.Write(dgv.Columns[i].HeaderText);
                        if (i < dgv.Columns.Count - 1)
                            sw.Write("\t"); // Use tab for better Excel formatting
                    }
                }
                sw.WriteLine();
                
                // Add separator line
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (dgv.Columns[i].Visible)
                    {
                        sw.Write("─────────────");
                        if (i < dgv.Columns.Count - 1)
                            sw.Write("\t");
                    }
                }
                sw.WriteLine();

                // Write data rows
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        for (int i = 0; i < dgv.Columns.Count; i++)
                        {
                            if (dgv.Columns[i].Visible)
                            {
                                object cellValue = row.Cells[i].Value;
                                if (cellValue != null)
                                {
                                    string value = cellValue.ToString();
                                    
                                    // Format numbers properly for Excel
                                    if (cellValue is decimal || cellValue is double || cellValue is float)
                                    {
                                        decimal numValue = Convert.ToDecimal(cellValue);
                                        value = numValue.ToString("N0");
                                    }
                                    else if (cellValue is DateTime)
                                    {
                                        value = ((DateTime)cellValue).ToString("dd/MM/yyyy");
                                    }
                                    
                                    sw.Write(value);
                                }
                                
                                if (i < dgv.Columns.Count - 1)
                                    sw.Write("\t");
                            }
                        }
                        sw.WriteLine();
                    }
                }
                
                // Add footer
                sw.WriteLine();
                sw.WriteLine("─────────────────────────────────────────────────────────────");
                sw.WriteLine($"Tổng số dòng: {dgv.Rows.Count - 1}");
                sw.WriteLine($"File được tạo bởi: Hệ thống quản lý rạp phim - {_tenChiNhanh}");
                sw.WriteLine("─────────────────────────────────────────────────────────────");
            }
        }

        private void BtnInBaoCao_Click(object sender, EventArgs e)
        {
            try
            {
                // Get current tab
                DataGridView dgvCurrent = null;
                string reportTitle = "";
                
                switch (tabMain.SelectedIndex)
                {
                    case 0:
                        dgvCurrent = dgvTongQuan;
                        reportTitle = "TỔNG QUAN";
                        break;
                    case 1:
                        dgvCurrent = dgvDoanhThu;
                        reportTitle = "DOANH THU";
                        break;
                    case 2:
                        dgvCurrent = dgvSanPham;
                        reportTitle = "SẢN PHẨM";
                        break;
                    case 3:
                        dgvCurrent = dgvPhim;
                        reportTitle = "PHIM";
                        break;
                    case 4:
                        dgvCurrent = dgvNhanVien;
                        reportTitle = "NHÂN VIÊN";
                        break;
                }

                if (dgvCurrent == null || dgvCurrent.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để in!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Use PrintDocument for real printing
                PrintDocument printDoc = new PrintDocument();
                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDoc;
                
                // Store data for printing
                _printDataGridView = dgvCurrent;
                _printReportTitle = reportTitle;
                _printCurrentPage = 0;
                
                printDoc.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);
                
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        printDoc.Print();
                        MessageBox.Show("Đã in báo cáo thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception printEx)
                    {
                        MessageBox.Show($"Lỗi khi in: {printEx.Message}\n\nBạn có muốn xuất ra CSV thay thế?",
                            "Lỗi in", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                        if (MessageBox.Show($"Bạn có muốn xuất ra CSV thay thế?", "Xuất CSV?", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            BtnXuatExcel_Click(sender, e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                // Print settings
                Font headerFont = new Font("Segoe UI", 16, FontStyle.Bold);
                Font subHeaderFont = new Font("Segoe UI", 10, FontStyle.Regular);
                Font contentFont = new Font("Segoe UI", 9, FontStyle.Regular);
                Font boldFont = new Font("Segoe UI", 9, FontStyle.Bold);
                
                float yPos = e.MarginBounds.Top;
                float xPos = e.MarginBounds.Left;
                float lineHeight = contentFont.GetHeight(e.Graphics);
                
                // Print header
                string headerText = $"BÁO CÁO CHI NHÁNH: {_tenChiNhanh}";
                e.Graphics.DrawString(headerText, headerFont, Brushes.Black, xPos, yPos);
                yPos += headerFont.GetHeight(e.Graphics) + 10;
                
                // Print sub-header info
                string subHeader1 = $"Loại báo cáo: {_printReportTitle}";
                e.Graphics.DrawString(subHeader1, subHeaderFont, Brushes.DarkGray, xPos, yPos);
                yPos += subHeaderFont.GetHeight(e.Graphics) + 5;
                
                string subHeader2 = $"Từ ngày: {dtpTuNgay.Value:dd/MM/yyyy} - Đến ngày: {dtpDenNgay.Value:dd/MM/yyyy}";
                e.Graphics.DrawString(subHeader2, subHeaderFont, Brushes.DarkGray, xPos, yPos);
                yPos += subHeaderFont.GetHeight(e.Graphics) + 5;
                
                string subHeader3 = $"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                e.Graphics.DrawString(subHeader3, subHeaderFont, Brushes.DarkGray, xPos, yPos);
                yPos += subHeaderFont.GetHeight(e.Graphics) + 20;
                
                // Draw separator line
                e.Graphics.DrawLine(Pens.Black, xPos, yPos, e.MarginBounds.Right, yPos);
                yPos += 10;
                
                // Calculate column widths
                int visibleColumnCount = _printDataGridView.Columns.Cast<DataGridViewColumn>()
                    .Count(c => c.Visible);
                float columnWidth = (e.MarginBounds.Width / visibleColumnCount);
                
                // Print column headers
                float currentX = xPos;
                foreach (DataGridViewColumn col in _printDataGridView.Columns)
                {
                    if (col.Visible)
                    {
                        e.Graphics.FillRectangle(Brushes.LightGray, currentX, yPos, columnWidth, lineHeight + 5);
                        e.Graphics.DrawRectangle(Pens.Black, currentX, yPos, columnWidth, lineHeight + 5);
                        e.Graphics.DrawString(col.HeaderText, boldFont, Brushes.Black, 
                            new RectangleF(currentX + 2, yPos + 2, columnWidth - 4, lineHeight),
                            new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                        currentX += columnWidth;
                    }
                }
                yPos += lineHeight + 10;
                
                // Print rows
                int maxRows = _printDataGridView.Rows.Count;
                int startRow = _printCurrentPage * 30; // 30 rows per page
                int endRow = Math.Min(startRow + 30, maxRows);
                
                for (int i = startRow; i < endRow && yPos < e.MarginBounds.Bottom - lineHeight; i++)
                {
                    if (!_printDataGridView.Rows[i].IsNewRow)
                    {
                        currentX = xPos;
                        foreach (DataGridViewColumn col in _printDataGridView.Columns)
                        {
                            if (col.Visible)
                            {
                                string cellValue = _printDataGridView.Rows[i].Cells[col.Index].Value?.ToString() ?? "";
                                e.Graphics.DrawRectangle(Pens.LightGray, currentX, yPos, columnWidth, lineHeight + 2);
                                e.Graphics.DrawString(cellValue, contentFont, Brushes.Black,
                                    new RectangleF(currentX + 2, yPos + 1, columnWidth - 4, lineHeight),
                                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                                currentX += columnWidth;
                            }
                        }
                        yPos += lineHeight + 2;
                    }
                }
                
                // Print page number
                string pageText = $"Trang {_printCurrentPage + 1}";
                e.Graphics.DrawString(pageText, contentFont, Brushes.Black, 
                    e.MarginBounds.Right - 50, e.MarginBounds.Bottom + 10);
                
                // Check if more pages needed
                _printCurrentPage++;
                e.HasMorePages = (endRow < maxRows);
                
                if (!e.HasMorePages)
                    _printCurrentPage = 0; // Reset for next print job
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in trang: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.HasMorePages = false;
            }
        }
    }
}
