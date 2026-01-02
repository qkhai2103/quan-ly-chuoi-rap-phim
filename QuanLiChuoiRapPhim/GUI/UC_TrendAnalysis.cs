using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_TrendAnalysis : UserControl
    {
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private Color _positiveGreen = Color.FromArgb(39, 174, 96);
        private Color _accentBlue = Color.FromArgb(52, 152, 219);
        private Color _accentPurple = Color.FromArgb(155, 89, 182);

        private Chart chartRevenue;
        private Chart chartPeakHours;
        private DataGridView dgvTopMovies;
        private ComboBox cboDateRange, cboBranch;
        private Label lblTotalRevenue, lblRevenueGrowth, lblTotalTickets, lblTicketGrowth;
        private Label lblTopMovie, lblTopMovieTickets, lblPeakHour, lblPeakHourPercent;

        private DataTable dtBranches;
        private bool _isLoaded = false;

        public UC_TrendAnalysis()
        {
            InitializeComponent();
            SetupUI();
            LoadBranches();
            // Defer data load until control is fully rendered
            this.HandleCreated += (s, e) => 
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (!_isLoaded && this.IsHandleCreated && this.Width > 100 && this.Height > 100)
                    {
                        _isLoaded = true;
                        LoadData();
                    }
                }));
            };
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(25);
            this.Dock = DockStyle.Fill;

            // ========== HEADER ==========
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 60 };

            Label lblTitle = new Label
            {
                Text = "📊 PHÂN TÍCH XU HƯỚNG",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Location = new Point(0, 15)
            };

            // Filters
            cboBranch = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(180, 35),
                Location = new Point(this.Width - 500, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboBranch.SelectedIndexChanged += (s, e) => LoadData();

            cboDateRange = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(130, 35),
                Location = new Point(this.Width - 300, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboDateRange.Items.AddRange(new[] { "7 ngày", "30 ngày", "90 ngày", "Năm nay" });
            cboDateRange.SelectedIndex = 1;
            cboDateRange.SelectedIndexChanged += (s, e) => LoadData();

            Button btnRefresh = new Button
            {
                Text = "🔄",
                Font = new Font("Segoe UI", 12),
                Size = new Size(45, 35),
                Location = new Point(this.Width - 150, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadData();

            headerPanel.Controls.AddRange(new Control[] { lblTitle, cboBranch, cboDateRange, btnRefresh });

            // ========== KPI CARDS ==========
            Panel pnlKPIs = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 10, 0, 10) };

            TableLayoutPanel kpiGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };
            for (int i = 0; i < 4; i++)
                kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var card1 = CreateKPICard("💰 DOANH THU", "0đ", "↑ 0%", _cgvRed, out lblTotalRevenue, out lblRevenueGrowth);
            var card2 = CreateKPICard("🎫 VÉ BÁN", "0", "↑ 0%", _accentBlue, out lblTotalTickets, out lblTicketGrowth);
            var card3 = CreateKPICard("🎬 PHIM HOT NHẤT", "---", "0 vé", _positiveGreen, out lblTopMovie, out lblTopMovieTickets);
            var card4 = CreateKPICard("⏰ GIỜ VÀNG", "--:00", "0% doanh thu", _accentPurple, out lblPeakHour, out lblPeakHourPercent);

            kpiGrid.Controls.Add(card1, 0, 0);
            kpiGrid.Controls.Add(card2, 1, 0);
            kpiGrid.Controls.Add(card3, 2, 0);
            kpiGrid.Controls.Add(card4, 3, 0);
            pnlKPIs.Controls.Add(kpiGrid);

            // ========== MAIN CONTENT ==========
            TableLayoutPanel mainContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 10, 0, 0)
            };
            mainContent.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
            mainContent.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));

            // Top: Revenue Line Chart
            Panel chartPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
            chartPanel.BorderRadius(15);

            chartRevenue = new Chart { Dock = DockStyle.Fill, BackColor = Color.White, MinimumSize = new Size(100, 100) };

            ChartArea areaRevenue = new ChartArea("Revenue");
            areaRevenue.BackColor = Color.White;
            areaRevenue.AxisX.MajorGrid.LineColor = Color.FromArgb(245, 245, 245);
            areaRevenue.AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
            areaRevenue.AxisX.LabelStyle.Font = new Font("Segoe UI", 8);
            areaRevenue.AxisY.LabelStyle.Font = new Font("Segoe UI", 8);
            areaRevenue.AxisY.LabelStyle.Format = "#,##0";
            chartRevenue.ChartAreas.Add(areaRevenue);

            Series seriesRevenue = new Series("DoanhThu");
            seriesRevenue.ChartType = SeriesChartType.SplineArea;
            seriesRevenue.Color = Color.FromArgb(100, _cgvRed);
            seriesRevenue.BorderColor = _cgvRed;
            seriesRevenue.BorderWidth = 3;
            chartRevenue.Series.Add(seriesRevenue);

            Label lblChartTitle = new Label
            {
                Text = "📈 DOANH THU THEO THỜI GIAN",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 10, 0, 0)
            };
            chartPanel.Controls.Add(chartRevenue);
            chartPanel.Controls.Add(lblChartTitle);

            // Bottom: Top Movies + Peak Hours
            TableLayoutPanel bottomRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            // Top Movies Table
            Panel moviesPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 10, 0) };
            moviesPanel.BorderRadius(15);

            Label lblMoviesTitle = new Label
            {
                Text = "🏆 TOP 10 PHIM BÁN CHẠY",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 10, 0, 0)
            };

            dgvTopMovies = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 40 },
                GridColor = Color.FromArgb(250, 250, 250),
                EnableHeadersVisualStyles = false
            };

            dgvTopMovies.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI Semibold", 9)
            };
            dgvTopMovies.ColumnHeadersHeight = 40;
            dgvTopMovies.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9),
                ForeColor = _cgvBlack
            };

            moviesPanel.Controls.Add(dgvTopMovies);
            moviesPanel.Controls.Add(lblMoviesTitle);

            // Peak Hours Chart
            Panel peakPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            peakPanel.BorderRadius(15);

            Label lblPeakTitle = new Label
            {
                Text = "🕐 PHÂN BỐ THEO GIỜ",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 10, 0, 0)
            };

            chartPeakHours = new Chart { Dock = DockStyle.Fill, BackColor = Color.White, MinimumSize = new Size(100, 100) };

            ChartArea areaPeak = new ChartArea("Peak");
            areaPeak.BackColor = Color.White;
            areaPeak.AxisX.MajorGrid.Enabled = false;
            areaPeak.AxisY.MajorGrid.LineColor = Color.FromArgb(245, 245, 245);
            areaPeak.AxisX.LabelStyle.Font = new Font("Segoe UI", 8);
            areaPeak.AxisY.LabelStyle.Font = new Font("Segoe UI", 8);
            areaPeak.AxisX.Interval = 2;
            chartPeakHours.ChartAreas.Add(areaPeak);

            Series seriesPeak = new Series("GioVang");
            seriesPeak.ChartType = SeriesChartType.Column;
            seriesPeak.Color = _accentPurple;
            chartPeakHours.Series.Add(seriesPeak);

            peakPanel.Controls.Add(chartPeakHours);
            peakPanel.Controls.Add(lblPeakTitle);

            bottomRow.Controls.Add(moviesPanel, 0, 0);
            bottomRow.Controls.Add(peakPanel, 1, 0);

            mainContent.Controls.Add(chartPanel, 0, 0);
            mainContent.Controls.Add(bottomRow, 0, 1);

            // ========== ASSEMBLE ==========
            this.Controls.Add(mainContent);
            this.Controls.Add(pnlKPIs);
            this.Controls.Add(headerPanel);
        }

        private Panel CreateKPICard(string title, string value, string subtitle, Color accentColor, out Label lblValue, out Label lblSubtitle)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 15, 0) };
            card.BorderRadius(12);

            Panel accent = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accentColor };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 12),
                AutoSize = true
            };

            lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(18, 35),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9),
                ForeColor = accentColor,
                Location = new Point(20, 65),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { accent, lblTitle, lblValue, lblSubtitle });
            return card;
        }

        private void LoadBranches()
        {
            try
            {
                string query = "SELECT MaChiNhanh, TenChiNhanh FROM ChiNhanh WHERE TrangThai = 1";
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    dtBranches = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dtBranches);
                    }
                }

                cboBranch.Items.Clear();
                cboBranch.Items.Add("Tất cả chi nhánh");
                foreach (DataRow row in dtBranches.Rows)
                {
                    cboBranch.Items.Add(row["TenChiNhanh"].ToString());
                }
                cboBranch.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadBranches error: {ex.Message}");
            }
        }

        private void LoadData()
        {
            try
            {
                int days = GetDaysFromSelection();
                DateTime fromDate = DateTime.Today.AddDays(-days);
                DateTime toDate = DateTime.Today;

                LoadKPIs(fromDate, toDate);
                LoadRevenueChart(fromDate, toDate);
                LoadTopMovies(fromDate, toDate);
                LoadPeakHours(fromDate, toDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetDaysFromSelection()
        {
            return cboDateRange.SelectedIndex switch
            {
                0 => 7,
                1 => 30,
                2 => 90,
                3 => 365,
                _ => 30
            };
        }

        private string GetBranchFilter()
        {
            if (cboBranch.SelectedIndex <= 0) return "";
            int maChiNhanh = Convert.ToInt32(dtBranches.Rows[cboBranch.SelectedIndex - 1]["MaChiNhanh"]);
            return $" AND nd.MaChiNhanh = {maChiNhanh}";
        }

        private void LoadKPIs(DateTime fromDate, DateTime toDate)
        {
            string branchFilter = GetBranchFilter();

            // Total revenue and tickets
            string query = $@"
                SELECT 
                    ISNULL(SUM(hd.ThanhTien), 0) AS TongDoanhThu,
                    ISNULL(COUNT(DISTINCT v.MaVe), 0) AS TongVe
                FROM HoaDon hd
                LEFT JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                LEFT JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                LEFT JOIN Ve v ON ct.MaVe = v.MaVe
                WHERE hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @FromDate AND @ToDate
                  {branchFilter}";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal revenue = reader.GetDecimal(0);
                            int tickets = reader.GetInt32(1);

                            lblTotalRevenue.Text = string.Format("{0:N0}đ", revenue);
                            lblTotalTickets.Text = tickets.ToString("N0");

                            // Mock growth (would compare with previous period)
                            lblRevenueGrowth.Text = "↑ 12%";
                            lblRevenueGrowth.ForeColor = _positiveGreen;
                            lblTicketGrowth.Text = "↑ 8%";
                            lblTicketGrowth.ForeColor = _positiveGreen;
                        }
                    }
                }

                // Top movie
                string topMovieQuery = $@"
                    SELECT TOP 1 p.TenPhim, COUNT(v.MaVe) AS SoVe
                    FROM Ve v
                    INNER JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu
                    INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                    INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                    WHERE v.TrangThai = N'DaBan'
                      AND CAST(v.NgayDat AS DATE) BETWEEN @FromDate AND @ToDate
                    GROUP BY p.TenPhim
                    ORDER BY SoVe DESC";

                using (SqlCommand cmd = new SqlCommand(topMovieQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string movieName = reader.GetString(0);
                            if (movieName.Length > 15) movieName = movieName.Substring(0, 12) + "...";
                            lblTopMovie.Text = movieName;
                            lblTopMovieTickets.Text = $"{reader.GetInt32(1):N0} vé";
                        }
                        else
                        {
                            lblTopMovie.Text = "---";
                            lblTopMovieTickets.Text = "Không có dữ liệu";
                        }
                    }
                }

                // Peak hour
                string peakQuery = $@"
                    SELECT TOP 1 DATEPART(HOUR, sc.GioChieu) AS Gio, COUNT(*) AS SoLuong
                    FROM Ve v
                    INNER JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu
                    WHERE v.TrangThai = N'DaBan'
                      AND CAST(v.NgayDat AS DATE) BETWEEN @FromDate AND @ToDate
                    GROUP BY DATEPART(HOUR, sc.GioChieu)
                    ORDER BY SoLuong DESC";

                using (SqlCommand cmd = new SqlCommand(peakQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int hour = reader.GetInt32(0);
                            lblPeakHour.Text = $"{hour:00}:00";
                            lblPeakHourPercent.Text = "Giờ cao điểm";
                        }
                        else
                        {
                            lblPeakHour.Text = "--:00";
                            lblPeakHourPercent.Text = "Không có dữ liệu";
                        }
                    }
                }
            }
        }

        private void LoadRevenueChart(DateTime fromDate, DateTime toDate)
        {
            // Ensure chart has proper size before updating
            if (chartRevenue == null || !chartRevenue.IsHandleCreated || 
                chartRevenue.Width <= 50 || chartRevenue.Height <= 50 || 
                chartRevenue.ChartAreas.Count == 0)
            {
                return;
            }

            try
            {
            string branchFilter = GetBranchFilter();

            string query = $@"
                SELECT CAST(hd.NgayLap AS DATE) AS Ngay, SUM(hd.ThanhTien) AS DoanhThu
                FROM HoaDon hd
                LEFT JOIN NguoiDung nd ON hd.MaNguoiDung = nd.MaNguoiDung
                WHERE hd.TrangThaiThanhToan = N'DaThanhToan'
                  AND CAST(hd.NgayLap AS DATE) BETWEEN @FromDate AND @ToDate
                  {branchFilter}
                GROUP BY CAST(hd.NgayLap AS DATE)
                ORDER BY Ngay";

            chartRevenue.Series["DoanhThu"].Points.Clear();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime date = reader.GetDateTime(0);
                            double revenue = Convert.ToDouble(reader.GetDecimal(1)) / 1000000;
                            
                            var point = chartRevenue.Series["DoanhThu"].Points.Add(revenue);
                            point.AxisLabel = date.ToString("dd/MM");
                        }
                    }
                }
            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRevenueChart error: {ex.Message}");
            }
        }

        private void LoadTopMovies(DateTime fromDate, DateTime toDate)
        {
            dgvTopMovies.Columns.Clear();
            dgvTopMovies.Rows.Clear();

            dgvTopMovies.Columns.Add("Rank", "#");
            dgvTopMovies.Columns.Add("TenPhim", "Tên phim");
            dgvTopMovies.Columns.Add("SoVe", "Số vé");
            dgvTopMovies.Columns.Add("DoanhThu", "Doanh thu");

            dgvTopMovies.Columns["Rank"].Width = 35;
            dgvTopMovies.Columns["SoVe"].Width = 60;

            string query = @"
                SELECT TOP 10 p.TenPhim, COUNT(v.MaVe) AS SoVe, SUM(v.GiaVe) AS DoanhThu
                FROM Ve v
                INNER JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu
                INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                WHERE v.TrangThai = N'DaBan'
                  AND CAST(v.NgayDat AS DATE) BETWEEN @FromDate AND @ToDate
                GROUP BY p.TenPhim
                ORDER BY SoVe DESC";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    int rank = 1;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string rankIcon = rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => rank.ToString() };
                            string name = reader.GetString(0);
                            if (name.Length > 25) name = name.Substring(0, 22) + "...";

                            dgvTopMovies.Rows.Add(
                                rankIcon,
                                name,
                                reader.GetInt32(1),
                                string.Format("{0:N0}đ", reader.GetDecimal(2))
                            );
                            rank++;
                        }
                    }
                }
            }
        }

        private void LoadPeakHours(DateTime fromDate, DateTime toDate)
        {
            // Ensure chart has proper size before updating
            if (chartPeakHours == null || !chartPeakHours.IsHandleCreated || 
                chartPeakHours.Width <= 50 || chartPeakHours.Height <= 50 || 
                chartPeakHours.ChartAreas.Count == 0)
            {
                return;
            }

            try
            {
            chartPeakHours.Series["GioVang"].Points.Clear();

            string query = @"
                SELECT DATEPART(HOUR, sc.GioChieu) AS Gio, COUNT(*) AS SoLuong
                FROM Ve v
                INNER JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu
                WHERE v.TrangThai = N'DaBan'
                  AND CAST(v.NgayDat AS DATE) BETWEEN @FromDate AND @ToDate
                GROUP BY DATEPART(HOUR, sc.GioChieu)
                ORDER BY Gio";

            // Initialize all hours with 0
            int[] hourData = new int[24];

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int hour = reader.GetInt32(0);
                            int count = reader.GetInt32(1);
                            if (hour >= 0 && hour < 24)
                                hourData[hour] = count;
                        }
                    }
                }
            }

            // Find max for highlighting
            int maxVal = hourData.Max();

            // Add data points for hours 8-24
            for (int h = 8; h <= 23; h++)
            {
                var point = chartPeakHours.Series["GioVang"].Points.Add(hourData[h]);
                point.AxisLabel = $"{h}h";
                
                // Highlight peak hours
                if (maxVal > 0 && hourData[h] == maxVal)
                    point.Color = _cgvRed;
            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPeakHours error: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_TrendAnalysis";
            this.Size = new Size(1200, 800);
            this.ResumeLayout(false);
        }
    }

    // Extension for finding max in array
    public static class ArrayExtensions
    {
        public static int Max(this int[] array)
        {
            int max = 0;
            foreach (int val in array)
                if (val > max) max = val;
            return max;
        }
    }
}
