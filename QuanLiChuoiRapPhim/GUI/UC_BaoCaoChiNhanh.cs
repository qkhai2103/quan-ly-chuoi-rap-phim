using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_BaoCaoChiNhanh : UserControl
    {
        private readonly int _maChiNhanh;           // Chi nhánh c?a Manager
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

        // Labels th?ng kê
        private Label lblTongDoanhThu, lblTongHoaDon, lblKhachHangTB, lblDoanhThuTB;
        private Label lblTopPhim, lblTopSanPham, lblNhanVienXuatSac;

        public UC_BaoCaoChiNhanh(int maChiNhanh, string tenChiNhanh)
        {
            _maChiNhanh = maChiNhanh;
            _tenChiNhanh = tenChiNhanh;

            ThietLapGiaoDien();
            TaiBaoCaoMacDinh();
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

            // Ch?n lo?i báo cáo
            Label lblLoaiBaoCao = new Label
            {
                Text = "Lỗi báo cáo:",
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
                "Sản phẩm bán chạy ...",
                "Phim bán chạy",
                "Hiệu suất nhân viên"
            });
            cboLoaiBaoCao.SelectedIndex = 0;
            cboLoaiBaoCao.SelectedIndexChanged += CboLoaiBaoCao_SelectedIndexChanged;

            // Ch?n th?i gian
            Label lblTuNgay = new Label
            {
                Text = "Tu:",
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
                Text = "Ðen:",
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

            // Nút t?o báo cáo
            btnTaoBaoCao = new Button
            {
                Text = "NÚT TẠO BÁO CÁO",
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

            lblTongDoanhThu = TaoLabelThongKe("TỔNG DOANH THU: 0 ð", Color.FromArgb(40, 167, 69), new Point(20, 25));
            lblTongHoaDon = TaoLabelThongKe("TỔNG HÓA ÐÕN: 0", Color.FromArgb(0, 123, 255), new Point(250, 25));
            lblKhachHangTB = TaoLabelThongKe("KHÁCH HÀNG TB: 0", Color.FromArgb(255, 193, 7), new Point(450, 25));
            lblDoanhThuTB = TaoLabelThongKe("DOANH THU TB: 0 ð", Color.FromArgb(220, 53, 69), new Point(650, 25));

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

            tabTongQuan = new TabPage("?? TỔNG QUAN");
            tabDoanhThu = new TabPage("?? DOANH THU");
            tabSanPham = new TabPage("?? SẢN PHẨM");
            tabPhim = new TabPage("?? PHIM");
            tabNhanVien = new TabPage("?? NHÂN VIÊN");

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
                Text = "?? XUẤT EXCEL",
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
                Text = "??? IN BÁO CÁO",
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
                Height = 300
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

            // Chart s?n ph?m
            chartSanPham = new Chart
            {
                Dock = DockStyle.Top,
                Height = 300
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
                Height = 300
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
                Height = 300
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

            Title title = new Title("TOP S?N PH?M BÁN CH?Y", Docking.Top,
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
                    'Số hóa đơn' AS ChiTieu,
                    COUNT(DISTINCT hd.MaHoaDon) AS GiaTri,
                    'Tổng số hóa đơn' AS MoTa
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                
                UNION ALL
                
                SELECT 
                    'T?ng doanh thu',
                    SUM(hd.ThanhTien),
                    'Doanh thu th?c t? (ð? tr? gi?m giá)'
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                
                UNION ALL
                
                SELECT 
                    'Doanh thu trung b?nh',
                    AVG(hd.ThanhTien),
                    'Trung b?nh m?i hóa ðõn'
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                
                UNION ALL
                
                SELECT 
                    'S? vé bán',
                    COUNT(ct.MaVe),
                    'T?ng s? vé ð? bán'
                FROM HoaDon hd
                INNER JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                INNER JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                WHERE nd.MaChiNhanh = @MaChiNhanh
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
                  AND hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND ct.MaVe IS NOT NULL
                
                UNION ALL
                
                SELECT 
                    'S?n ph?m bán',
                    SUM(ct.SoLuong),
                    'T?ng s?n ph?m b?p ný?c'
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
                MessageBox.Show("L?i t?i báo cáo t?ng quan: " + ex.Message, "L?i",
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
                MessageBox.Show("L?i t?i báo cáo doanh thu: " + ex.Message, "L?i",
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

                        // C?p nh?t top s?n ph?m
                        if (dt.Rows.Count > 0)
                        {
                            lblTopSanPham.Text = $"?? S?N PH?M BÁN CH?Y: {dt.Rows[0]["TenSanPham"]} ({dt.Rows[0]["SoLuongBan"]} ph?n)";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i t?i báo cáo s?n ph?m: " + ex.Message, "L?i",
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

                        // C?p nh?t top phim
                        if (dt.Rows.Count > 0)
                        {
                            lblTopPhim.Text = $"?? PHIM BÁN CH?Y: {dt.Rows[0]["TenPhim"]} ({dt.Rows[0]["SoVeBan"]} vé)";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i t?i báo cáo phim: " + ex.Message, "L?i",
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

                        // C?p nh?t nhân viên xu?t s?c
                        if (dt.Rows.Count > 0)
                        {
                            lblNhanVienXuatSac.Text = $"?? NHÂN VIÊN XU?T S?C: {dt.Rows[0]["HoTen"]} ({dt.Rows[0]["DoanhThu"]:N0} ð)";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tại báo cáo nhân viên: " + ex.Message, "Lỗi",
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

                                lblTongDoanhThu.Text = $"TỔNG DOANH THU: {tongDoanhThu:N0} ð";
                                lblTongHoaDon.Text = $"TỔNG HÓA ÐƠN: {tongHoaDon}";
                                lblKhachHangTB.Text = $"KHÁCH HÀNG: {khachHang}";
                                lblDoanhThuTB.Text = $"DOANH THU TB: {doanhThuTB:N0} ð";
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
            chartDoanhThu.Series.Clear();

            Series seriesDoanhThu = new Series("Doanh thu");
            seriesDoanhThu.ChartType = SeriesChartType.Column;
            seriesDoanhThu.Color = Color.FromArgb(0, 123, 255);
            seriesDoanhThu.IsValueShownAsLabel = true;
            seriesDoanhThu.LabelFormat = "N0";

            Series seriesHoaDon = new Series("Số hóa ðõn");
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

            // Thi?t l?p tr?c Y ph?
            chartDoanhThu.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chartDoanhThu.ChartAreas[0].AxisY2.LabelStyle.Format = "N0";
        }

        private void VeBieuDoSanPham(DataTable dt)
        {
            chartSanPham.Series.Clear();

            Series series = new Series("Số lượng bán bán");
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
                series.Points[i].LabelToolTip = $"{row["TenSanPham"]}\nDoanh thu: {Convert.ToDecimal(row["DoanhThu"]):N0} ð";
            }

            chartSanPham.Series.Add(series);
        }

        private void VeBieuDoPhim(DataTable dt)
        {
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

        private void VeBieuDoNhanVien(DataTable dt)
        {
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
                point.LabelToolTip = $"{tenNV}\nDoanh thu: {doanhThu:N0} ð\nHóa ðõn: {row["SoHoaDon"]}";

                // Màu s?c khác nhau
                point.Color = GetColorForIndex(i);
            }

            chartNhanVien.Series.Add(series);
            chartNhanVien.Legends[0].Enabled = true;
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
            btnTaoBaoCao.Text = "ÐANG XỬ LÝ...";

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

                MessageBox.Show("Ðã cập nhật báo cáo thành công!", "Thành công",
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
                btnTaoBaoCao.Text = "?? T?O BÁO CÁO";
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
                    saveDialog.Filter = "Excel Files|*.xlsx";
                    saveDialog.FileName = $"BaoCao_{_tenChiNhanh}_{DateTime.Now:yyyyMMdd_HHmm}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // TODO: Xu?t Excel th?c t?
                        // Có th? dùng thý vi?n EPPlus ho?c ClosedXML

                        MessageBox.Show($"ÐÃ xuất báo cáo thành công!\n\nFile: {saveDialog.FileName}", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnInBaoCao_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("In báo cáo hiện tại?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    // TODO: In báo cáo th?c t?
                    MessageBox.Show("Ðang in báo cáo...", "Thông báo");
                }
            }
        }
    }
}
