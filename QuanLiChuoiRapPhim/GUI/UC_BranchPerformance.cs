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
    public partial class UC_BranchPerformance : UserControl
    {
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private Color _positiveGreen = Color.FromArgb(39, 174, 96);
        private Color _warningOrange = Color.FromArgb(243, 156, 18);
        private Color _negativeRed = Color.FromArgb(231, 76, 60);

        private DataGridView dgvRanking;
        private Chart chartComparison;
        private ComboBox cboDateRange;
        private Panel pnlKPIs;
        private Label lblTopBranch, lblTopRevenue, lblNeedImprove, lblTotalRevenue, lblTargetPercent;
        private DataTable dtBranchData;
        private bool _isLoaded = false;

        public UC_BranchPerformance()
        {
            InitializeComponent();
            SetupUI();
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
            this.Padding = new Padding(30);
            this.Dock = DockStyle.Fill;

            // ========== HEADER ==========
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 60 };

            Label lblTitle = new Label
            {
                Text = "🏆 HIỆU SUẤT CHI NHÁNH",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Location = new Point(0, 15)
            };

            // Date range filter
            cboDateRange = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(150, 35),
                Location = new Point(this.Width - 320, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboDateRange.Items.AddRange(new[] { "7 ngày qua", "30 ngày qua", "90 ngày qua", "Năm nay" });
            cboDateRange.SelectedIndex = 1;
            cboDateRange.SelectedIndexChanged += (s, e) => LoadData();

            Button btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(100, 35),
                Location = new Point(this.Width - 150, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadData();

            headerPanel.Controls.AddRange(new Control[] { lblTitle, cboDateRange, btnRefresh });

            // ========== KPI CARDS ==========
            pnlKPIs = new Panel { Dock = DockStyle.Top, Height = 130, Padding = new Padding(0, 15, 0, 15) };

            TableLayoutPanel kpiGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };
            for (int i = 0; i < 4; i++)
                kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var card1 = CreateKPICard("🥇 CHI NHÁNH #1", "---", "---", _positiveGreen, out lblTopBranch, out lblTopRevenue);
            var card2 = CreateKPICard("⚠️ CẦN CẢI THIỆN", "---", "", _warningOrange, out lblNeedImprove, out _);
            var card3 = CreateKPICard("💰 TỔNG DOANH THU", "0đ", "Hệ thống", Color.FromArgb(52, 152, 219), out lblTotalRevenue, out _);
            var card4 = CreateKPICard("🎯 ĐẠT MỤC TIÊU", "0%", "Trung bình", _cgvRed, out lblTargetPercent, out _);

            kpiGrid.Controls.Add(card1, 0, 0);
            kpiGrid.Controls.Add(card2, 1, 0);
            kpiGrid.Controls.Add(card3, 2, 0);
            kpiGrid.Controls.Add(card4, 3, 0);
            pnlKPIs.Controls.Add(kpiGrid);

            // ========== MAIN CONTENT ==========
            TableLayoutPanel mainContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 0)
            };
            mainContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            mainContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            // Left: Chart
            Panel chartPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 10, 0) };
            chartPanel.BorderRadius(15);

            chartComparison = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                MinimumSize = new Size(100, 100)
            };

            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.BackColor = Color.White;
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
            chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 9);
            chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 9);
            chartArea.AxisY.LabelStyle.Format = "#,##0";
            chartComparison.ChartAreas.Add(chartArea);

            Series series = new Series("DoanhThu");
            series.ChartType = SeriesChartType.Bar;
            series.Color = _cgvRed;
            series.Font = new Font("Segoe UI", 8);
            series.IsValueShownAsLabel = true;
            series.LabelFormat = "#,##0";
            chartComparison.Series.Add(series);

            Label lblChartTitle = new Label
            {
                Text = "📊 SO SÁNH DOANH THU",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 10, 0, 0)
            };
            chartPanel.Controls.Add(chartComparison);
            chartPanel.Controls.Add(lblChartTitle);

            // Right: Ranking Table
            Panel tablePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            tablePanel.BorderRadius(15);

            Label lblTableTitle = new Label
            {
                Text = "🏆 BẢNG XẾP HẠNG",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 10, 0, 0)
            };

            dgvRanking = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 55 },
                GridColor = Color.FromArgb(245, 245, 245),
                EnableHeadersVisualStyles = false
            };

            dgvRanking.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI Semibold", 9),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            dgvRanking.ColumnHeadersHeight = 45;

            dgvRanking.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 10),
                ForeColor = _cgvBlack,
                SelectionBackColor = Color.FromArgb(255, 240, 240),
                SelectionForeColor = _cgvBlack
            };

            tablePanel.Controls.Add(dgvRanking);
            tablePanel.Controls.Add(lblTableTitle);

            mainContent.Controls.Add(chartPanel, 0, 0);
            mainContent.Controls.Add(tablePanel, 1, 0);

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
                Location = new Point(20, 15),
                AutoSize = true
            };

            lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(18, 40),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9),
                ForeColor = accentColor,
                Location = new Point(20, 72),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { accent, lblTitle, lblValue, lblSubtitle });
            return card;
        }

        private void LoadData()
        {
            try
            {
                int days = GetDaysFromSelection();
                DateTime fromDate = DateTime.Today.AddDays(-days);
                DateTime toDate = DateTime.Today;

                string query = $@"
                    SELECT 
                        cn.MaChiNhanh,
                        cn.TenChiNhanh,
                        ISNULL(SUM(hd.ThanhTien), 0) AS DoanhThu,
                        ISNULL(COUNT(DISTINCT hd.MaHoaDon), 0) AS SoHoaDon,
                        ISNULL(COUNT(DISTINCT v.MaVe), 0) AS SoVe,
                        (SELECT ISNULL(SUM(pc2.TongSoGhe), 0) FROM PhongChieu pc2 WHERE pc2.MaChiNhanh = cn.MaChiNhanh) AS TongGhe
                    FROM ChiNhanh cn
                    LEFT JOIN NguoiDung nd ON cn.MaChiNhanh = nd.MaChiNhanh
                    LEFT JOIN HoaDon hd ON nd.MaNguoiDung = hd.MaNguoiDung 
                        AND hd.TrangThaiThanhToan = N'DaThanhToan'
                        AND CAST(hd.NgayLap AS DATE) BETWEEN @FromDate AND @ToDate
                    LEFT JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
                    LEFT JOIN Ve v ON ct.MaVe = v.MaVe
                    WHERE cn.TrangThai = 1
                    GROUP BY cn.MaChiNhanh, cn.TenChiNhanh
                    ORDER BY DoanhThu DESC";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    dtBranchData = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FromDate", fromDate);
                        cmd.Parameters.AddWithValue("@ToDate", toDate);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dtBranchData);
                        }
                    }
                }

                UpdateKPIs();
                UpdateChart();
                UpdateRankingTable();
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

        private void UpdateKPIs()
        {
            if (dtBranchData == null || dtBranchData.Rows.Count == 0)
            {
                lblTopBranch.Text = "---";
                lblTopRevenue.Text = "Không có dữ liệu";
                lblNeedImprove.Text = "---";
                lblTotalRevenue.Text = "0đ";
                lblTargetPercent.Text = "0%";
                return;
            }

            // Top branch
            DataRow topRow = dtBranchData.Rows[0];
            string topName = topRow["TenChiNhanh"].ToString();
            if (topName.Length > 20) topName = topName.Substring(0, 17) + "...";
            lblTopBranch.Text = topName;
            lblTopRevenue.Text = string.Format("{0:N0}đ", topRow["DoanhThu"]);

            // Lowest performer
            DataRow lowRow = dtBranchData.Rows[dtBranchData.Rows.Count - 1];
            string lowName = lowRow["TenChiNhanh"].ToString();
            if (lowName.Length > 20) lowName = lowName.Substring(0, 17) + "...";
            lblNeedImprove.Text = lowName;

            // Total revenue
            decimal totalRevenue = 0;
            foreach (DataRow row in dtBranchData.Rows)
            {
                totalRevenue += Convert.ToDecimal(row["DoanhThu"]);
            }
            lblTotalRevenue.Text = string.Format("{0:N0}đ", totalRevenue);

            // Target percentage (mock - could be real target from settings)
            decimal avgRevenue = totalRevenue / dtBranchData.Rows.Count;
            decimal target = 100000000; // 100M mock target
            int percent = (int)Math.Min(100, (avgRevenue / target) * 100);
            lblTargetPercent.Text = $"{percent}%";
        }

        private void UpdateChart()
        {
            try
            {
                // Ensure chart has proper size before updating
                if (chartComparison == null || !chartComparison.IsHandleCreated ||
                    chartComparison.Width <= 50 || chartComparison.Height <= 50 ||
                    chartComparison.ChartAreas.Count == 0)
                {
                    return;
                }

                chartComparison.Series["DoanhThu"].Points.Clear();

                if (dtBranchData == null || dtBranchData.Rows.Count == 0) return;

                foreach (DataRow row in dtBranchData.Rows)
                {
                    string name = row["TenChiNhanh"].ToString();
                    if (name.Length > 15) name = name.Substring(0, 12) + "...";
                    double revenue = Convert.ToDouble(row["DoanhThu"]) / 1000000; // Convert to millions

                    var point = chartComparison.Series["DoanhThu"].Points.Add(revenue);
                    point.AxisLabel = name;
                    point.Label = $"{revenue:N1}M";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateChart error: {ex.Message}");
            }
        }

        private void UpdateRankingTable()
        {
            dgvRanking.Columns.Clear();
            dgvRanking.Rows.Clear();

            dgvRanking.Columns.Add("Rank", "#");
            dgvRanking.Columns.Add("TenChiNhanh", "Chi nhánh");
            dgvRanking.Columns.Add("DoanhThu", "Doanh thu");
            dgvRanking.Columns.Add("SoVe", "Vé bán");
            dgvRanking.Columns.Add("Trend", "Xu hướng");

            dgvRanking.Columns["Rank"].Width = 40;
            dgvRanking.Columns["DoanhThu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvRanking.Columns["SoVe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvRanking.Columns["Trend"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            if (dtBranchData == null) return;

            int rank = 1;
            foreach (DataRow row in dtBranchData.Rows)
            {
                string rankIcon = rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => rank.ToString() };
                string trend = rank <= dtBranchData.Rows.Count / 2 ? "↑" : "↓";
                Color trendColor = rank <= dtBranchData.Rows.Count / 2 ? _positiveGreen : _negativeRed;

                int rowIndex = dgvRanking.Rows.Add(
                    rankIcon,
                    row["TenChiNhanh"],
                    string.Format("{0:N0}đ", row["DoanhThu"]),
                    row["SoVe"],
                    trend
                );

                dgvRanking.Rows[rowIndex].Cells["Trend"].Style.ForeColor = trendColor;
                dgvRanking.Rows[rowIndex].Cells["Trend"].Style.Font = new Font("Segoe UI", 12, FontStyle.Bold);

                rank++;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_BranchPerformance";
            this.Size = new Size(1200, 800);
            this.ResumeLayout(false);
        }
    }
}
