// using AntdUI;
using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_Reports : UserControl
    {
        private AdminBLL adminBLL = new AdminBLL();
        private ReportBLL reportBLL = new ReportBLL();

        private System.Windows.Forms.ComboBox cboReportType, cboBranch;
        private System.Windows.Forms.DateTimePicker dtpFrom, dtpTo;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartReport;
        private System.Windows.Forms.DataGridView dgvReport;
        private System.Windows.Forms.Button btnGenerate, btnExport, btnPrint;
        private System.Windows.Forms.Label lblTotalRevenue, lblTotalTickets, lblTotalCustomers;

        public UC_Reports()
        {
            InitializeComponent();
            SetupUI();
            LoadInitialData();
        }

        private void SetupUI()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // ========== PANEL TIÊU Ð? ==========
            System.Windows.Forms.Panel titlePanel = new System.Windows.Forms.Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 60;
            titlePanel.BackColor = Color.FromArgb(111, 66, 193);

            System.Windows.Forms.Label lblTitle = new System.Windows.Forms.Label();
            lblTitle.Text = "?? BÁO CÁO & TH?NG KÊ";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            titlePanel.Controls.Add(lblTitle);

            // ========== PANEL ÐI?U KHI?N ==========
            System.Windows.Forms.Label controlPanel = new System.Windows.Forms.Label();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 100;
            controlPanel.BackColor = Color.FromArgb(248, 249, 250);
            controlPanel.Padding = new Padding(20, 10, 20, 10);

            // Hàng 1: Lo?i báo cáo và Chi nhánh
            System.Windows.Forms.Label lblReportType = new System.Windows.Forms.Label();
            lblReportType.Text = "Lo?i báo cáo:";
            lblReportType.Font = new Font("Segoe UI", 10);
            lblReportType.Location = new Point(20, 15);
            lblReportType.AutoSize = true;

            cboReportType = new ComboBox();
            cboReportType.Font = new Font("Segoe UI", 10);
            cboReportType.Size = new Size(200, 30);
            cboReportType.Location = new Point(120, 10);
            cboReportType.Items.AddRange(new string[] {
                "Doanh thu theo ngày",
                "Phim bán ch?y",
                "Doanh thu chi nhánh",
                "Vé bán theo su?t chi?u",
                "S?n ph?m bán ch?y",
                "Khách hàng thành viên"
            });
            cboReportType.SelectedIndex = 0;
            cboReportType.SelectedIndexChanged += CboReportType_SelectedIndexChanged;

            System.Windows.Forms.Label lblBranch = new System.Windows.Forms.Label();
            lblBranch.Text = "Chi nhánh:";
            lblBranch.Font = new Font("Segoe UI", 10);
            lblBranch.Location = new Point(340, 15);
            lblBranch.AutoSize = true;

            cboBranch = new ComboBox();
            cboBranch.Font = new Font("Segoe UI", 10);
            cboBranch.Size = new Size(200, 30);
            cboBranch.Location = new Point(420, 10);
            cboBranch.Items.Add("T?t c? chi nhánh");

            // Hàng 2: Ngày tháng
            System.Windows.Forms.Label lblFrom = new System.Windows.Forms.Label();
            lblFrom.Text = "T? ngày:";
            lblFrom.Font = new Font("Segoe UI", 10);
            lblFrom.Location = new Point(20, 55);
            lblFrom.AutoSize = true;

            dtpFrom = new DateTimePicker();
            dtpFrom.Font = new Font("Segoe UI", 10);
            dtpFrom.Size = new Size(150, 30);
            dtpFrom.Location = new Point(120, 50);
            dtpFrom.Value = DateTime.Today.AddDays(-30);

            System.Windows.Forms.Label lblTo = new System.Windows.Forms.Label();
            lblTo.Text = "Ð?n ngày:";
            lblTo.Font = new Font("Segoe UI", 10);
            lblTo.Location = new Point(290, 55);
            lblTo.AutoSize = true;

            dtpTo = new DateTimePicker();
            dtpTo.Font = new Font("Segoe UI", 10);
            dtpTo.Size = new Size(150, 30);
            dtpTo.Location = new Point(380, 50);
            dtpTo.Value = DateTime.Today;

            // Nút t?o báo cáo
            btnGenerate = new System.Windows.Forms.Button();
            btnGenerate.Text = "?? T?O BÁO CÁO";
            btnGenerate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnGenerate.Size = new Size(150, 35);
            btnGenerate.Location = new Point(550, 48);
            btnGenerate.BackColor = Color.FromArgb(0, 123, 255);
            btnGenerate.ForeColor = Color.White;
            btnGenerate.Click += BtnGenerate_Click;

            controlPanel.Controls.AddRange(new Control[] {
                lblReportType, cboReportType, lblBranch, cboBranch,
                lblFrom, dtpFrom, lblTo, dtpTo, btnGenerate
            });

            // ========== PANEL TH?NG KÊ T?NG ==========
            System.Windows.Forms.Panel statsPanel = new System.Windows.Forms.Panel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 50;
            statsPanel.BackColor = Color.FromArgb(234, 236, 238);
            statsPanel.Padding = new Padding(20, 5, 20, 5);

            lblTotalRevenue = CreateStatLabel("DOANH THU: 0 ð", Color.FromArgb(40, 167, 69), new Point(20, 10));
            lblTotalTickets = CreateStatLabel("VÉ BÁN: 0", Color.FromArgb(0, 123, 255), new Point(250, 10));
            lblTotalCustomers = CreateStatLabel("KHÁCH HÀNG: 0", Color.FromArgb(220, 53, 69), new Point(430, 10));

            statsPanel.Controls.AddRange(new Control[] { lblTotalRevenue, lblTotalTickets, lblTotalCustomers });

            // ========== BI?U Ð? ==========
            chartReport = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chartReport.Dock = DockStyle.Top;
            chartReport.Height = 300;
            chartReport.BackColor = Color.White;
            chartReport.BorderlineColor = Color.LightGray;
            chartReport.BorderlineDashStyle = ChartDashStyle.Solid;
            chartReport.BorderlineWidth = 1;

            // ========== DATA GRID VIEW ==========
            dgvReport = new DataGridView();
            dgvReport.Dock = DockStyle.Fill;
            dgvReport.BackgroundColor = Color.White;
            dgvReport.RowHeadersVisible = false;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // ========== PANEL NÚT XU?T ==========
            System.Windows.Forms.Panel exportPanel = new System.Windows.Forms.Panel();
            exportPanel.Dock = DockStyle.Bottom;
            exportPanel.Height = 60;
            exportPanel.BackColor = Color.FromArgb(248, 249, 250);

            btnPrint = new System.Windows.Forms.Button();
            btnPrint.Text = "??? IN BÁO CÁO";
            btnPrint.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnPrint.Size = new Size(150, 40);
            btnPrint.Location = new Point(250, 10);
            btnPrint.BackColor = Color.FromArgb(108, 117, 125);
            btnPrint.ForeColor = Color.White;
            btnPrint.Click += BtnPrint_Click;

            btnExport = new System.Windows.Forms.Button();
            btnExport.Text = "?? XU?T EXCEL";
            btnExport.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnExport.Size = new Size(150, 40);
            btnExport.Location = new Point(420, 10);
            btnExport.BackColor = Color.FromArgb(40, 167, 69);
            btnExport.ForeColor = Color.White;
            btnExport.Click += BtnExport_Click;

            exportPanel.Controls.AddRange(new Control[] { btnPrint, btnExport });

            // Thêm controls
            this.Controls.Add(dgvReport);
            this.Controls.Add(exportPanel);
            this.Controls.Add(chartReport);
            this.Controls.Add(statsPanel);
            this.Controls.Add(controlPanel);
            this.Controls.Add(titlePanel);
        }

        private System.Windows.Forms.Label CreateStatLabel(string text, Color color, Point location)
        {
            return new System.Windows.Forms.Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = color,
                Location = location,
                Size = new Size(200, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
        }

        private void LoadInitialData()
        {
            try
            {
                // Load danh sách chi nhánh t? database
                UserBLL userBLL = new UserBLL();
                DataTable dtBranches = userBLL.GetBranches();

                foreach (DataRow row in dtBranches.Rows)
                {
                    cboBranch.Items.Add(row["TenChiNhanh"].ToString());
                }
                cboBranch.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i load chi nhánh: {ex.Message}", "L?i",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Hi?n th?/?n filter chi nhánh tùy lo?i báo cáo
            string reportType = cboReportType.SelectedItem.ToString();
            bool showBranch = reportType != "Doanh thu theo ngày" && reportType != "Phim bán ch?y";

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is System.Windows.Forms.Label panel && panel.BackColor == Color.FromArgb(248, 249, 250))
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child.Text == "Chi nhánh:" || (child is ComboBox && child != cboReportType))
                        {
                            child.Visible = showBranch;
                        }
                    }
                }
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            // G?i phýõng th?c async
            _ = GenerateReportAsync();
        }

        private async Task GenerateReportAsync()
        {
            try
            {
                if (!adminBLL.TestDatabaseConnection())
                {
                    MessageBox.Show("Không th? k?t n?i database!", "L?i",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string reportType = cboReportType.SelectedItem.ToString();
                DateTime fromDate = dtpFrom.Value.Date;
                DateTime toDate = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);
                string branchName = cboBranch.SelectedItem?.ToString();

                if (fromDate > toDate)
                {
                    MessageBox.Show("Ngày b?t ð?u không ðý?c l?n hõn ngày k?t thúc!", "C?nh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Hi?n th? loading
                Cursor = Cursors.WaitCursor;
                btnGenerate.Enabled = false;
                btnGenerate.Text = "ÐANG X? L?...";

                // T?o reportBLL n?u chýa có
                if (reportBLL == null)
                    reportBLL = new ReportBLL();

                DataTable reportData = null;

                // S? d?ng Task.Run ð? ch?y query database trên background thread
                reportData = await Task.Run(() =>
                {
                    try
                    {
                        switch (reportType)
                        {
                            case "Doanh thu theo ngày":
                                return reportBLL.GetRevenueByDate(fromDate, toDate);
                            case "Phim bán ch?y":
                                return reportBLL.GetTopMovies(fromDate, toDate);
                            case "Doanh thu chi nhánh":
                                return reportBLL.GetRevenueByBranch(fromDate, toDate);
                            case "Vé bán theo su?t chi?u":
                                return reportBLL.GetTicketSales(fromDate, toDate, branchName);
                            case "S?n ph?m bán ch?y":
                                return reportBLL.GetTopProducts(fromDate, toDate, branchName);
                            case "Khách hàng thành viên":
                                return reportBLL.GetMemberCustomers();
                            default:
                                return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ghi log l?i
                        Console.WriteLine($"L?i trong Task.Run: {ex.Message}");
                        return null;
                    }
                });

                // X? l? k?t qu? trên UI thread
                ProcessReportResult(reportData, reportType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i t?o báo cáo: {ex.Message}", "L?i",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnGenerate.Enabled = true;
                btnGenerate.Text = "?? T?O BÁO CÁO";
            }
        }

        private void ProcessReportResult(DataTable reportData, string reportType)
        {
            if (reportData != null && reportData.Rows.Count > 0)
            {
                dgvReport.DataSource = reportData;
                FormatDataGridView(reportType);
                CreateChart(reportData, reportType);
                UpdateStatistics(reportData, reportType);
            }
            else
            {
                dgvReport.DataSource = null;
                chartReport.Series.Clear();
                MessageBox.Show("Không có d? li?u trong kho?ng th?i gian ð? ch?n!", "Thông báo");
            }
        }

        private void FormatDataGridView(string reportType)
        {
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Ki?m tra và ð?nh d?ng c?t tùy lo?i báo cáo
            try
            {
                switch (reportType)
                {
                    case "Doanh thu theo ngày":
                        if (dgvReport.Columns.Contains("NgayBan"))
                        {
                            dgvReport.Columns["NgayBan"].HeaderText = "NGÀY";
                            dgvReport.Columns["NgayBan"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        }
                        if (dgvReport.Columns.Contains("SoHoaDon"))
                            dgvReport.Columns["SoHoaDon"].HeaderText = "S? HÓA ÐÕN";
                        if (dgvReport.Columns.Contains("TongDoanhThu"))
                        {
                            dgvReport.Columns["TongDoanhThu"].HeaderText = "DOANH THU";
                            dgvReport.Columns["TongDoanhThu"].DefaultCellStyle.Format = "N0";
                        }
                        if (dgvReport.Columns.Contains("TongGiamGia"))
                        {
                            dgvReport.Columns["TongGiamGia"].HeaderText = "GI?M GIÁ";
                            dgvReport.Columns["TongGiamGia"].DefaultCellStyle.Format = "N0";
                        }
                        break;

                    case "Phim bán ch?y":
                        if (dgvReport.Columns.Contains("TenPhim"))
                            dgvReport.Columns["TenPhim"].HeaderText = "TÊN PHIM";
                        if (dgvReport.Columns.Contains("TheLoai"))
                            dgvReport.Columns["TheLoai"].HeaderText = "TH? LO?I";
                        if (dgvReport.Columns.Contains("SoVeBan"))
                            dgvReport.Columns["SoVeBan"].HeaderText = "S? VÉ BÁN";
                        if (dgvReport.Columns.Contains("DoanhThu"))
                        {
                            dgvReport.Columns["DoanhThu"].HeaderText = "DOANH THU";
                            dgvReport.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                        }
                        break;

                    case "Doanh thu chi nhánh":
                        if (dgvReport.Columns.Contains("TenChiNhanh"))
                            dgvReport.Columns["TenChiNhanh"].HeaderText = "CHI NHÁNH";
                        if (dgvReport.Columns.Contains("SoHoaDon"))
                            dgvReport.Columns["SoHoaDon"].HeaderText = "S? HÓA ÐÕN";
                        if (dgvReport.Columns.Contains("TongDoanhThu"))
                        {
                            dgvReport.Columns["TongDoanhThu"].HeaderText = "DOANH THU";
                            dgvReport.Columns["TongDoanhThu"].DefaultCellStyle.Format = "N0";
                        }
                        if (dgvReport.Columns.Contains("DoanhThuTrungBinh"))
                        {
                            dgvReport.Columns["DoanhThuTrungBinh"].HeaderText = "DOANH THU TB";
                            dgvReport.Columns["DoanhThuTrungBinh"].DefaultCellStyle.Format = "N0";
                        }
                        break;
                }

                // Format màu cho c?t s?
                foreach (DataGridViewColumn column in dgvReport.Columns)
                {
                    if (column.ValueType == typeof(decimal) || column.ValueType == typeof(int) ||
                        column.ValueType == typeof(double) || column.ValueType == typeof(float))
                    {
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.DefaultCellStyle.ForeColor = Color.Blue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"L?i ð?nh d?ng DataGridView: {ex.Message}");
            }
        }

        private void CreateChart(DataTable data, string reportType)
        {
            try
            {
                chartReport.Series.Clear();
                chartReport.ChartAreas.Clear();
                chartReport.Titles.Clear();

                ChartArea chartArea = new ChartArea();
                chartArea.AxisX.LabelStyle.Angle = -45;
                chartArea.AxisX.LabelStyle.Interval = 1;
                chartReport.ChartAreas.Add(chartArea);

                Title title = new Title(reportType.ToUpper(), Docking.Top,
                    new Font("Segoe UI", 14, FontStyle.Bold), Color.FromArgb(73, 80, 87));
                chartReport.Titles.Add(title);

                Series series = new Series("D? li?u");
                series.ChartType = SeriesChartType.Column;
                series.Color = Color.FromArgb(0, 170, 255);
                series.IsValueShownAsLabel = true;
                series.LabelFormat = "N0";

                int maxItems = Math.Min(data.Rows.Count, 10); // Hi?n th? t?i ða 10 items

                for (int i = 0; i < maxItems; i++)
                {
                    DataRow row = data.Rows[i];
                    string label = "";
                    double value = 0;

                    try
                    {
                        switch (reportType)
                        {
                            case "Doanh thu theo ngày":
                                if (data.Columns.Contains("NgayBan") && data.Columns.Contains("TongDoanhThu"))
                                {
                                    label = Convert.ToDateTime(row["NgayBan"]).ToString("dd/MM");
                                    value = Convert.ToDouble(row["TongDoanhThu"]);
                                }
                                break;
                            case "Phim bán ch?y":
                                if (data.Columns.Contains("TenPhim") && data.Columns.Contains("DoanhThu"))
                                {
                                    label = row["TenPhim"].ToString();
                                    if (label.Length > 15) label = label.Substring(0, 12) + "...";
                                    value = Convert.ToDouble(row["DoanhThu"]);
                                }
                                break;
                            case "Doanh thu chi nhánh":
                                if (data.Columns.Contains("TenChiNhanh") && data.Columns.Contains("TongDoanhThu"))
                                {
                                    label = row["TenChiNhanh"].ToString();
                                    value = Convert.ToDouble(row["TongDoanhThu"]);
                                }
                                break;
                            case "Vé bán theo su?t chi?u":
                                if (data.Columns.Contains("TenPhim") && data.Columns.Contains("SoVeBan"))
                                {
                                    label = row["TenPhim"].ToString();
                                    if (label.Length > 15) label = label.Substring(0, 12) + "...";
                                    value = Convert.ToDouble(row["SoVeBan"]);
                                }
                                break;
                        }

                        if (!string.IsNullOrEmpty(label))
                        {
                            series.Points.AddXY(label, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"L?i thêm ði?m vào bi?u ð?: {ex.Message}");
                    }
                }

                if (series.Points.Count > 0)
                {
                    chartReport.Series.Add(series);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"L?i t?o bi?u ð?: {ex.Message}");
            }
        }

        private void UpdateStatistics(DataTable data, string reportType)
        {
            try
            {
                decimal totalRevenue = 0;
                int totalTickets = 0;
                int totalCustomers = 0;

                foreach (DataRow row in data.Rows)
                {
                    if (reportType == "Doanh thu theo ngày" || reportType == "Doanh thu chi nhánh")
                    {
                        if (data.Columns.Contains("TongDoanhThu"))
                        {
                            try
                            {
                                totalRevenue += Convert.ToDecimal(row["TongDoanhThu"]);
                            }
                            catch { }
                        }
                        if (data.Columns.Contains("SoHoaDon"))
                        {
                            try
                            {
                                totalCustomers += Convert.ToInt32(row["SoHoaDon"]);
                            }
                            catch { }
                        }
                    }
                    else if (reportType == "Phim bán ch?y" || reportType == "Vé bán theo su?t chi?u")
                    {
                        if (data.Columns.Contains("DoanhThu"))
                        {
                            try
                            {
                                totalRevenue += Convert.ToDecimal(row["DoanhThu"]);
                            }
                            catch { }
                        }
                        if (data.Columns.Contains("SoVeBan"))
                        {
                            try
                            {
                                totalTickets += Convert.ToInt32(row["SoVeBan"]);
                            }
                            catch { }
                        }
                    }
                    else if (reportType == "Khách hàng thành viên")
                    {
                        totalCustomers = data.Rows.Count;
                    }
                }

                lblTotalRevenue.Text = $"DOANH THU: {totalRevenue:N0} ð";
                lblTotalTickets.Text = $"VÉ BÁN: {totalTickets}";
                lblTotalCustomers.Text = $"KHÁCH HÀNG: {totalCustomers}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"L?i c?p nh?t th?ng kê: {ex.Message}");
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("Không có d? li?u ð? xu?t!", "C?nh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel Files|*.xlsx|CSV Files|*.csv";
            saveDialog.FileName = $"BaoCao_{cboReportType.SelectedItem}_{DateTime.Now:yyyyMMdd_HHmm}";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // TODO: Th?c hi?n export th?c t?
                    MessageBox.Show($"Ð? xu?t báo cáo thành công!\n\n" +
                                  $"File: {saveDialog.FileName}", "Thành công",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i xu?t file: {ex.Message}", "L?i",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("Không có d? li?u ð? in!", "C?nh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Ðang in báo cáo...", "Thông báo");
                // TODO: Thêm ch?c nãng in th?c t?
            }
        }
    }
}
