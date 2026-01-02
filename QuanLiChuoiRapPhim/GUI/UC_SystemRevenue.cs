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

        public UC_SystemRevenue()
        {
            InitializeComponent();
            SetupUI();
            LoadData();
        }

        // Event handler riêng cho nút Xem để đảm bảo hoạt động
        private void BtnXem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"BtnXem_Click: Loading data from {_dtpFrom.Value:dd/MM/yyyy} to {_dtpTo.Value:dd/MM/yyyy}");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);

            Label lblTitle = new Label
            {
                Text = "DOANH THU TOÀN HỆ THỐNG",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20),
                AutoSize = true
            };

            // Filter Panel
            Panel filterPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(20, 10, 20, 10) };
            filterPanel.BorderRadius(12);

            Label lblFrom = new Label { Text = "Từ ngày:", Font = new Font("Segoe UI", 10), Location = new Point(20, 18), AutoSize = true };
            // FIX: Sử dụng class field + Format ngày đúng
            _dtpFrom = new DateTimePicker 
            { 
                Font = new Font("Segoe UI", 10), 
                Size = new Size(150, 30), 
                Location = new Point(90, 15), 
                Value = DateTime.Now.AddMonths(-1),
                Format = DateTimePickerFormat.Short
            };

            Label lblTo = new Label { Text = "Đến:", Font = new Font("Segoe UI", 10), Location = new Point(260, 18), AutoSize = true };
            // FIX: Sử dụng class field + Format ngày đúng
            _dtpTo = new DateTimePicker 
            { 
                Font = new Font("Segoe UI", 10), 
                Size = new Size(150, 30), 
                Location = new Point(310, 15), 
                Value = DateTime.Now,
                Format = DateTimePickerFormat.Short
            };

            Button btnXem = new Button 
            { 
                Text = "📊 Xem", 
                BackColor = _cgvRed, 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                FlatStyle = FlatStyle.Flat, 
                Size = new Size(100, 35), 
                Location = new Point(480, 12), 
                Cursor = Cursors.Hand 
            };
            btnXem.FlatAppearance.BorderSize = 0;
            btnXem.Click += BtnXem_Click;

            filterPanel.Controls.AddRange(new Control[] { lblFrom, _dtpFrom, lblTo, _dtpTo, btnXem });

            // Stats Panel
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 10, 0, 20) };
            TableLayoutPanel statsGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            // FIX: Lưu reference đến value labels để update sau
            var (card1, lbl1) = CreateStatCardWithLabel("DOANH THU HÔM NAY", "Đang tải...", _cgvRed);
            var (card2, lbl2) = CreateStatCardWithLabel("TỔNG DOANH THU", "Đang tải...", Color.FromArgb(39, 174, 96));
            var (card3, lbl3) = CreateStatCardWithLabel("SỐ GIAO DỊCH", "Đang tải...", Color.FromArgb(52, 152, 219));

            _lblDoanhThuHomNay = lbl1;
            _lblTongDoanhThu = lbl2;
            _lblSoGiaoDich = lbl3;

            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsGrid.Controls.Add(card3, 2, 0);
            statsPanel.Controls.Add(statsGrid);

            // Tabs - sử dụng biến class thay vì local
            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

            // Tab 1: Doanh thu theo ngày
            TabPage tabNgay = new TabPage("Doanh Thu Theo Ngày");
            DataGridView dgvNgay = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvNgay.Columns.Add("NgayBan", "Ngày");
            dgvNgay.Columns.Add("SoHoaDon", "Số HĐ");
            dgvNgay.Columns.Add("TongDoanhThu", "Doanh Thu");
            dgvNgay.Columns.Add("TongGiamGia", "Giảm Giá");
            tabNgay.Controls.Add(dgvNgay);

            // Tab 2: Doanh thu chi nhánh
            TabPage tabChiNhanh = new TabPage("Doanh Thu Chi Nhánh");
            DataGridView dgvChiNhanh = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvChiNhanh.Columns.Add("TenChiNhanh", "Chi Nhánh");
            dgvChiNhanh.Columns.Add("SoHoaDon", "Số HĐ");
            dgvChiNhanh.Columns.Add("TongDoanhThu", "Doanh Thu");
            dgvChiNhanh.Columns.Add("DoanhThuTrungBinh", "TB/HĐ");
            tabChiNhanh.Controls.Add(dgvChiNhanh);

            // Tab 3: Phim bán chạy
            TabPage tabPhim = new TabPage("Phim Bán Chạy");
            DataGridView dgvPhim = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvPhim.Columns.Add("TenPhim", "Phim");
            dgvPhim.Columns.Add("TheLoai", "Thể Loại");
            dgvPhim.Columns.Add("SoVeBan", "Vé Bán");
            dgvPhim.Columns.Add("DoanhThu", "Doanh Thu");
            tabPhim.Controls.Add(dgvPhim);

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

        // FIX: Method mới trả về cả Panel và Label để có thể update value sau
        private (Panel card, Label valueLabel) CreateStatCardWithLabel(string title, string value, Color accentColor)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 20, 0) };
            card.BorderRadius(15);
            Panel accent = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accentColor };
            card.Controls.Add(accent);
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.Gray, Location = new Point(25, 20), AutoSize = true };
            Label lblValue = new Label { Text = value, Font = new Font("Montserrat", 18, FontStyle.Bold), ForeColor = _cgvBlack, Location = new Point(22, 45), AutoSize = true };
            card.Controls.AddRange(new Control[] { lblTitle, lblValue });
            return (card, lblValue);
        }

        private void LoadData()
        {
            try
            {
                // FIX: Sử dụng giá trị từ DateTimePicker thay vì hardcode
                DateTime tuNgay = _dtpFrom.Value.Date;
                DateTime denNgay = _dtpTo.Value.Date.AddDays(1).AddSeconds(-1); // Cuối ngày

                // Load doanh thu theo ngày
                DataTable dtNgay = _doanhThuBLL.ThongKeDoanhThuTheoNgay(tuNgay, denNgay);
                DataGridView dgvNgay = (DataGridView)tabControl.TabPages[0].Controls[0];
                dgvNgay.Rows.Clear();

                decimal tongDoanhThu = 0;
                int tongGiaoDich = 0;

                foreach (DataRow row in dtNgay.Rows)
                {
                    decimal doanhThu = Convert.ToDecimal(row["TongDoanhThu"]);
                    int soHoaDon = Convert.ToInt32(row["SoHoaDon"]);

                    tongDoanhThu += doanhThu;
                    tongGiaoDich += soHoaDon;

                    dgvNgay.Rows.Add(
                        Convert.ToDateTime(row["NgayBan"]).ToString("dd/MM/yyyy"),
                        soHoaDon,
                        doanhThu.ToString("N0") + " đ",
                        Convert.ToDecimal(row["TongGiamGia"]).ToString("N0") + " đ"
                    );
                }

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
                DataGridView dgvChiNhanh = (DataGridView)tabControl.TabPages[1].Controls[0];
                dgvChiNhanh.Rows.Clear();
                foreach (DataRow row in dtChiNhanh.Rows)
                {
                    dgvChiNhanh.Rows.Add(
                        row["TenChiNhanh"],
                        row["SoHoaDon"],
                        Convert.ToDecimal(row["TongDoanhThu"]).ToString("N0") + " đ",
                        Convert.ToDecimal(row["DoanhThuTrungBinh"]).ToString("N0") + " đ"
                    );
                }

                // Load phim bán chạy
                DataTable dtPhim = _doanhThuBLL.ThongKePhimBanChay(10);
                DataGridView dgvPhim = (DataGridView)tabControl.TabPages[2].Controls[0];
                dgvPhim.Rows.Clear();
                foreach (DataRow row in dtPhim.Rows)
                {
                    dgvPhim.Rows.Add(
                        row["TenPhim"],
                        row["TheLoai"],
                        row["SoVeBan"],
                        Convert.ToDecimal(row["DoanhThu"]).ToString("N0") + " đ"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
