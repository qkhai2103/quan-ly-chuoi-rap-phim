using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_SystemRevenue : UserControl
    {
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private DoanhThuBLL _doanhThuBLL = new DoanhThuBLL();
        private TabControl tabControl;

        // FIX: Chuyển DateTimePicker thành class fields để LoadData() có thể truy cập
        private DateTimePicker _dtpFrom;
        private DateTimePicker _dtpTo;

        // Stats labels để update sau khi load data
        private Label _lblDoanhThuHomNay;
        private Label _lblTongDoanhThu;
        private Label _lblSoGiaoDich;

        // Grid references to avoid InvalidCastException after wrapping in Panels
        private DataGridView _dgvNgay;
        private DataGridView _dgvChiNhanh;
        private DataGridView _dgvPhim;

        public UC_SystemRevenue()
        {
            InitializeComponent();
            SetupUI();
            LoadData();
        }

        // Removed: BtnXem_Click - không cần nữa vì auto-refresh

        private void SetupUI()
        {
            this.BackColor = UIHelper.CGV_LIGHT_GRAY;
            this.Padding = new Padding(UIHelper.SPACE_XL);

            Label lblTitle = new Label
            {
                Text = "💰 DOANH THU TOÀN HỆ THỐNG",
                Font = UIHelper.FONT_TITLE,
                ForeColor = UIHelper.CGV_BLACK,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20),
                AutoSize = true
            };

            // Filter Panel - Increased height for better spacing
            Panel filterPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            filterPanel.BorderRadius(12);

            Label lblFrom = new Label { Text = "Từ ngày:", Font = new Font("Segoe UI", 10), Location = new Point(20, 25), AutoSize = true };
            // FIX: Sử dụng class field + Format ngày đúng + Auto-refresh on change
            _dtpFrom = new DateTimePicker 
            { 
                Font = new Font("Segoe UI", 10), 
                Size = new Size(150, 30), 
                Location = new Point(90, 22), 
                Value = DateTime.Now.AddMonths(-1),
                Format = DateTimePickerFormat.Short
            };
            _dtpFrom.ValueChanged += (s, e) => LoadData(); // Auto-refresh

            Label lblTo = new Label { Text = "Đến:", Font = new Font("Segoe UI", 10), Location = new Point(260, 25), AutoSize = true };
            // FIX: Sử dụng class field + Format ngày đúng + Auto-refresh on change
            _dtpTo = new DateTimePicker 
            { 
                Font = new Font("Segoe UI", 10), 
                Size = new Size(150, 30), 
                Location = new Point(310, 22), 
                Value = DateTime.Now,
                Format = DateTimePickerFormat.Short
            };
            _dtpTo.ValueChanged += (s, e) => LoadData(); // Auto-refresh

            // Removed: Xóa nút "Xem Báo Cáo" - data sẽ tự động refresh khi đổi ngày

            filterPanel.Controls.AddRange(new Control[] { lblFrom, _dtpFrom, lblTo, _dtpTo });

            // Stats Panel - Using UIHelper for consistent cards
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 140, Padding = new Padding(0, 15, 0, 20) };
            TableLayoutPanel statsGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            // Use UIHelper.CreateStatCard for consistency
            var card1 = UIHelper.CreateStatCard("💰 DOANH THU HÔM NAY", "Đang tải...", UIHelper.CGV_RED, out _lblDoanhThuHomNay);
            var card2 = UIHelper.CreateStatCard("📊 TỔNG DOANH THU", "Đang tải...", UIHelper.SUCCESS_GREEN, out _lblTongDoanhThu);
            var card3 = UIHelper.CreateStatCard("🎫 SỐ GIAO DỊCH", "0", UIHelper.INFO_BLUE, out _lblSoGiaoDich);

            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsGrid.Controls.Add(card3, 2, 0);
            statsPanel.Controls.Add(statsGrid);

            // Tabs - Using UIHelper for modern DataGridView styling
            tabControl = new TabControl { Dock = DockStyle.Fill, Font = UIHelper.FONT_NORMAL };
            tabControl.Padding = new Point(15, 10); // Add padding inside tabs

            // Tab 1: Doanh thu theo ngày - Use UIHelper
            TabPage tabNgay = new TabPage("📅 Doanh Thu Theo Ngày");
            tabNgay.BackColor = UIHelper.CGV_LIGHT_GRAY;
            tabNgay.Padding = new Padding(15);
            
            Panel gridPanel1 = UIHelper.CreateRoundedPanel(12);
            gridPanel1.Dock = DockStyle.Fill;
            gridPanel1.Padding = new Padding(1);
            
            _dgvNgay = UIHelper.CreateModernDataGrid();
            _dgvNgay.Columns.Add("NgayBan", "📅 Ngày");
            _dgvNgay.Columns.Add("SoHoaDon", "📋 Số HĐ");
            _dgvNgay.Columns.Add("TongDoanhThu", "💰 Doanh Thu");
            _dgvNgay.Columns.Add("TongGiamGia", "🎁 Giảm Giá");
            gridPanel1.Controls.Add(_dgvNgay);
            tabNgay.Controls.Add(gridPanel1);

            // Tab 2: Doanh thu chi nhánh
            TabPage tabChiNhanh = new TabPage("🏢 Doanh Thu Chi Nhánh");
            tabChiNhanh.BackColor = UIHelper.CGV_LIGHT_GRAY;
            tabChiNhanh.Padding = new Padding(15);
            
            Panel gridPanel2 = UIHelper.CreateRoundedPanel(12);
            gridPanel2.Dock = DockStyle.Fill;
            gridPanel2.Padding = new Padding(1);
            
            _dgvChiNhanh = UIHelper.CreateModernDataGrid();
            _dgvChiNhanh.Columns.Add("TenChiNhanh", "🏢 Chi Nhánh");
            _dgvChiNhanh.Columns.Add("SoHoaDon", "📋 Số HĐ");
            _dgvChiNhanh.Columns.Add("TongDoanhThu", "💰 Doanh Thu");
            _dgvChiNhanh.Columns.Add("DoanhThuTrungBinh", "📊 TB/HĐ");
            gridPanel2.Controls.Add(_dgvChiNhanh);
            tabChiNhanh.Controls.Add(gridPanel2);

            // Tab 3: Phim bán chạy
            TabPage tabPhim = new TabPage("🎬 Phim Bán Chạy");
            tabPhim.BackColor = UIHelper.CGV_LIGHT_GRAY;
            tabPhim.Padding = new Padding(15);
            
            Panel gridPanel3 = UIHelper.CreateRoundedPanel(12);
            gridPanel3.Dock = DockStyle.Fill;
            gridPanel3.Padding = new Padding(1);
            
            _dgvPhim = UIHelper.CreateModernDataGrid();
            _dgvPhim.Columns.Add("TenPhim", "🎬 Phim");
            _dgvPhim.Columns.Add("TheLoai", "📂 Thể Loại");
            _dgvPhim.Columns.Add("SoVeBan", "🎫 Vé Bán");
            _dgvPhim.Columns.Add("DoanhThu", "💰 Doanh Thu");
            gridPanel3.Controls.Add(_dgvPhim);
            tabPhim.Controls.Add(gridPanel3);

            tabControl.TabPages.Add(tabNgay);
            tabControl.TabPages.Add(tabChiNhanh);
            tabControl.TabPages.Add(tabPhim);

            this.Controls.Add(tabControl);
            Panel spacer = new Panel { Dock = DockStyle.Top, Height = 20 };
            this.Controls.Add(spacer);
            this.Controls.Add(statsPanel);
            this.Controls.Add(filterPanel);
            this.Controls.Add(lblTitle);
        }

        // Removed: Now using UIHelper.CreateStatCard instead

        private void LoadData()
        {
            try
            {
                // FIX: Sử dụng giá trị từ DateTimePicker thay vì hardcode
                DateTime tuNgay = _dtpFrom.Value.Date;
                DateTime denNgay = _dtpTo.Value.Date.AddDays(1).AddSeconds(-1); // Cuối ngày

                System.Diagnostics.Debug.WriteLine($"LoadData called: {tuNgay:dd/MM/yyyy} to {denNgay:dd/MM/yyyy}");

                // Load doanh thu theo ngày
                DataTable dtNgay = _doanhThuBLL.ThongKeDoanhThuTheoNgay(tuNgay, denNgay);
                System.Diagnostics.Debug.WriteLine($"dtNgay rows: {dtNgay?.Rows.Count ?? 0}");
                
                _dgvNgay.Rows.Clear();

                decimal tongDoanhThu = 0;
                int tongGiaoDich = 0;

                foreach (DataRow row in dtNgay.Rows)
                {
                    decimal doanhThu = Convert.ToDecimal(row["TongDoanhThu"]);
                    int soHoaDon = Convert.ToInt32(row["SoHoaDon"]);

                    tongDoanhThu += doanhThu;
                    tongGiaoDich += soHoaDon;

                    _dgvNgay.Rows.Add(
                        Convert.ToDateTime(row["NgayBan"]).ToString("dd/MM/yyyy"),
                        soHoaDon,
                        doanhThu.ToString("N0") + " đ",
                        Convert.ToDecimal(row["TongGiamGia"]).ToString("N0") + " đ"
                    );
                }

                System.Diagnostics.Debug.WriteLine($"Added {_dgvNgay.Rows.Count} rows to dgvNgay");

                // FIX: Tính doanh thu hôm nay
                decimal doanhThuHomNay = 0;
                DataTable dtHomNay = _doanhThuBLL.ThongKeDoanhThuTheoNgay(DateTime.Today, DateTime.Today.AddDays(1).AddSeconds(-1));
                if (dtHomNay.Rows.Count > 0)
                {
                    doanhThuHomNay = Convert.ToDecimal(dtHomNay.Rows[0]["TongDoanhThu"]);
                }

                // FIX: Update stats cards với giá trị thực
                if (_lblDoanhThuHomNay != null)
                    _lblDoanhThuHomNay.Text = doanhThuHomNay.ToString("N0") + " đ";
                if (_lblTongDoanhThu != null)
                    _lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " đ";
                if (_lblSoGiaoDich != null)
                    _lblSoGiaoDich.Text = tongGiaoDich.ToString("N0");

                // Load doanh thu chi nhánh
                DataTable dtChiNhanh = _doanhThuBLL.ThongKeDoanhThuChiNhanh(tuNgay, denNgay);
                System.Diagnostics.Debug.WriteLine($"dtChiNhanh rows: {dtChiNhanh?.Rows.Count ?? 0}");
                
                _dgvChiNhanh.Rows.Clear();
                foreach (DataRow row in dtChiNhanh.Rows)
                {
                    _dgvChiNhanh.Rows.Add(
                        row["TenChiNhanh"],
                        row["SoHoaDon"],
                        Convert.ToDecimal(row["TongDoanhThu"]).ToString("N0") + " đ",
                        Convert.ToDecimal(row["DoanhThuTrungBinh"]).ToString("N0") + " đ"
                    );
                }

                // Load phim bán chạy
                DataTable dtPhim = _doanhThuBLL.ThongKePhimBanChay(10);
                System.Diagnostics.Debug.WriteLine($"dtPhim rows: {dtPhim?.Rows.Count ?? 0}");
                
                _dgvPhim.Rows.Clear();
                foreach (DataRow row in dtPhim.Rows)
                {
                    _dgvPhim.Rows.Add(
                        row["TenPhim"],
                        row["TheLoai"],
                        row["SoVeBan"],
                        Convert.ToDecimal(row["DoanhThu"]).ToString("N0") + " đ"
                    );
                }

                System.Diagnostics.Debug.WriteLine("LoadData completed successfully");
                
                // Force grid refresh
                _dgvNgay.Refresh();
                _dgvChiNhanh.Refresh();
                _dgvPhim.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadData ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}\n\nChi tiết: {ex.StackTrace}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_SystemRevenue";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
