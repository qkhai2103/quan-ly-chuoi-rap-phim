using QuanLiChuoiRapPhim.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_DuyetDonXinNghi : UserControl
    {
        private readonly int _maChiNhanh;
        private readonly int _maNguoiDung;
        private DataTable _dtDonXinNghi;
        private DataTable _dtNhanVien;

        // Controls chính
        private TabControl tabMain;
        private TabPage tabChoDuyet, tabDaDuyet, tabTuChoi, tabDaNghi, tabThongKe;
        private DataGridView dgvChoDuyet, dgvDaDuyet, dgvTuChoi, dgvDaNghi;
        private Panel pnlChiTiet;
        private Button btnDuyet, btnTuChoi, btnXemFile, btnLamMoi;
        private ComboBox cboLoaiNghiFilter, cboUuTienFilter;
        private DateTimePicker dtpTuNgay, dtpDenNgay;
        private TextBox txtTimKiem;

        // Controls chi tiết
        private Label lblMaDon, lblNhanVien, lblLoaiNghi, lblNgayNghi, lblSoNgay, lblLyDo;
        private Label lblTrangThai, lblNguoiDuyet, lblNgayGui, lblNgayDuyet;
        private RichTextBox rtbGhiChuDuyet;
        private ComboBox cboNguoiThayThe;
        private Button btnLuuThayThe, btnHuyDon, btnXacNhanDaNghi;
        private LinkLabel lnkFileDinhKem;

        // Chart thống kê
        private System.Windows.Forms.DataVisualization.Charting.Chart chartThongKe;

        public UC_DuyetDonXinNghi(int maChiNhanh, int maNguoiDung)
        {
            _maChiNhanh = maChiNhanh;
            _maNguoiDung = maNguoiDung;

            KhoiTaoGiaoDien();
            TaiDuLieuKhoiDau();
            TaiDanhSachDon();
        }

        private void KhoiTaoGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            // === HEADER ===
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(33, 150, 243) // Xanh dương Material
            };

            // Tiêu đề chính
            Label lblTitle = new Label
            {
                Text = "📋 DUYỆT ĐƠN XIN NGHỈ PHÉP",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle với thông tin chi nhánh
            Label lblSubtitle = new Label
            {
                Text = $"Chi nhánh: {_maChiNhanh} | Quản lý: User_{_maNguoiDung}",
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(224, 224, 224),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);

            // === TOOLBAR ===
            Panel pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            TaoToolbar(pnlToolbar);

            // === MAIN CONTENT (Split Container) ===
            SplitContainer splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 700,
                SplitterWidth = 2,
                BackColor = Color.White
            };

            // Panel trái - Danh sách đơn
            Panel pnlDanhSach = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Panel phải - Chi tiết đơn
            pnlChiTiet = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            splitMain.Panel1.Controls.Add(pnlDanhSach);
            splitMain.Panel2.Controls.Add(pnlChiTiet);

            // Tạo TabControl trong panel danh sách
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(100, 35),
                SizeMode = TabSizeMode.Fixed
            };

            tabChoDuyet = new TabPage("⏳ CHỜ DUYỆT");
            tabDaDuyet = new TabPage("✅ ĐÃ DUYỆT");
            tabTuChoi = new TabPage("❌ TỪ CHỐI");
            tabDaNghi = new TabPage("📅 ĐÃ NGHỈ");
            tabThongKe = new TabPage("📊 THỐNG KÊ");

            tabMain.TabPages.AddRange(new TabPage[]
            {
                tabChoDuyet, tabDaDuyet, tabTuChoi, tabDaNghi, tabThongKe
            });
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

            // Tạo DataGridView cho mỗi tab
            TaoDataGridViewChoMoiTab();

            // Tạo panel chi tiết
            TaoPanelChiTiet();

            // Tạo tab thống kê
            TaoTabThongKe();

            pnlDanhSach.Controls.Add(tabMain);

            // === FOOTER ===
            Panel pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(33, 33, 33)
            };

            Label lblFooter = new Label
            {
                Text = $"Tổng số đơn: 0 | Chờ duyệt: 0 | Đã duyệt: 0 | Từ chối: 0",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Name = "lblFooter"
            };

            pnlFooter.Controls.Add(lblFooter);

            // Thêm tất cả controls vào UserControl
            this.Controls.Add(splitMain);
            this.Controls.Add(pnlToolbar);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private void TaoToolbar(Panel pnlToolbar)
        {
            int y = 20;
            int x = 20;

            // Tìm kiếm
            Label lblTimKiem = new Label
            {
                Text = "Tìm kiếm:",
                Location = new Point(x, y),
                AutoSize = true
            };

            txtTimKiem = new TextBox
            {
                Location = new Point(x + 80, y - 3),
                Size = new Size(200, 30),
                Text = "Tên nhân viên, lý do..."
            };
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;
            x += 300;

            // Lọc loại nghỉ
            Label lblLoaiNghi = new Label
            {
                Text = "Loại nghỉ:",
                Location = new Point(x, y),
                AutoSize = true
            };

            cboLoaiNghiFilter = new ComboBox
            {
                Location = new Point(x + 80, y - 3),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLoaiNghiFilter.Items.AddRange(new string[]
            {
                "Tất cả", "Phép năm", "Phép ốm", "Thai sản",
                "Không phép", "Nghỉ lễ", "Hưởng chế độ"
            });
            cboLoaiNghiFilter.SelectedIndex = 0;
            cboLoaiNghiFilter.SelectedIndexChanged += CboLoaiNghiFilter_SelectedIndexChanged;
            x += 250;

            // Lọc ưu tiên
            Label lblUuTien = new Label
            {
                Text = "Ưu tiên:",
                Location = new Point(x, y),
                AutoSize = true
            };

            cboUuTienFilter = new ComboBox
            {
                Location = new Point(x + 60, y - 3),
                Size = new Size(120, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboUuTienFilter.Items.AddRange(new string[] { "Tất cả", "Cần duyệt gấp", "Bình thường" });
            cboUuTienFilter.SelectedIndex = 0;
            cboUuTienFilter.SelectedIndexChanged += CboUuTienFilter_SelectedIndexChanged;
            x += 200;

            // Ngày bắt đầu
            Label lblTuNgay = new Label
            {
                Text = "Từ:",
                Location = new Point(x, y),
                AutoSize = true
            };

            dtpTuNgay = new DateTimePicker
            {
                Location = new Point(x + 30, y - 3),
                Size = new Size(120, 30),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-1)
            };
            dtpTuNgay.ValueChanged += DtpTuNgay_ValueChanged;
            x += 170;

            // Ngày kết thúc
            Label lblDenNgay = new Label
            {
                Text = "Đến:",
                Location = new Point(x, y),
                AutoSize = true
            };

            dtpDenNgay = new DateTimePicker
            {
                Location = new Point(x + 40, y - 3),
                Size = new Size(120, 30),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            dtpDenNgay.ValueChanged += DtpDenNgay_ValueChanged;
            x += 180;

            // Nút làm mới
            btnLamMoi = new Button
            {
                Text = "🔄 Làm mới",
                Location = new Point(x, y - 3),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLamMoi.Click += BtnLamMoi_Click;

            pnlToolbar.Controls.AddRange(new Control[]
            {
                lblTimKiem, txtTimKiem,
                lblLoaiNghi, cboLoaiNghiFilter,
                lblUuTien, cboUuTienFilter,
                lblTuNgay, dtpTuNgay,
                lblDenNgay, dtpDenNgay,
                btnLamMoi
            });
        }

        private void TaoDataGridViewChoMoiTab()
        {
            // Tab Chờ duyệt
            dgvChoDuyet = TaoDataGridView();
            dgvChoDuyet.Tag = "ChoDuyet";
            tabChoDuyet.Controls.Add(dgvChoDuyet);

            // Tab Đã duyệt
            dgvDaDuyet = TaoDataGridView();
            dgvDaDuyet.Tag = "DaDuyet";
            tabDaDuyet.Controls.Add(dgvDaDuyet);

            // Tab Từ chối
            dgvTuChoi = TaoDataGridView();
            dgvTuChoi.Tag = "TuChoi";
            tabTuChoi.Controls.Add(dgvTuChoi);

            // Tab Đã nghỉ
            dgvDaNghi = TaoDataGridView();
            dgvDaNghi.Tag = "DaNghi";
            tabDaNghi.Controls.Add(dgvDaNghi);
        }

        private DataGridView TaoDataGridView()
        {
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                MultiSelect = false,
                AllowDrop = false
            };

            // Style hiện đại
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 150, 243);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;
            dgv.EnableHeadersVisualStyles = false;

            // Alternating rows
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Sự kiện
            dgv.SelectionChanged += Dgv_SelectionChanged;
            dgv.CellFormatting += Dgv_CellFormatting;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            return dgv;
        }

        private void TaoPanelChiTiet()
        {
            pnlChiTiet.Padding = new Padding(20);
            pnlChiTiet.AutoScroll = true;

            // Tiêu đề chi tiết
            Label lblTitle = new Label
            {
                Text = "CHI TIẾT ĐƠN XIN NGHỈ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 150, 243),
                Location = new Point(0, 10),
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlChiTiet.Controls.Add(lblTitle);

            int y = 60;
            int labelWidth = 150;

            // Mã đơn
            lblMaDon = TaoLabelChiTiet("Mã đơn:", "", y);
            y += 35;

            // Nhân viên
            lblNhanVien = TaoLabelChiTiet("Nhân viên:", "", y);
            y += 35;

            // Loại nghỉ
            lblLoaiNghi = TaoLabelChiTiet("Loại nghỉ:", "", y);
            y += 35;

            // Ngày nghỉ
            lblNgayNghi = TaoLabelChiTiet("Ngày nghỉ:", "", y);
            y += 35;

            // Số ngày
            lblSoNgay = TaoLabelChiTiet("Số ngày:", "", y);
            y += 35;

            // Lý do (RichTextBox)
            Label lblLyDoTitle = new Label
            {
                Text = "Lý do:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 66, 66),
                Location = new Point(10, y),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            lblLyDo = new Label
            {
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(33, 33, 33),
                Location = new Point(labelWidth + 20, y),
                Size = new Size(300, 60),
                Text = "",
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5),
                BackColor = Color.FromArgb(250, 250, 250)
            };
            y += 70;

            pnlChiTiet.Controls.Add(lblLyDoTitle);
            pnlChiTiet.Controls.Add(lblLyDo);

            // Người thay thế (chỉ hiện với đơn chờ duyệt)
            Label lblThayThe = new Label
            {
                Text = "Người thay thế:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 66, 66),
                Location = new Point(10, y),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleRight,
                Visible = false,
                Name = "lblThayThe"
            };

            cboNguoiThayThe = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(250, 30),
                Location = new Point(labelWidth + 20, y),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };
            y += 40;

            btnLuuThayThe = new Button
            {
                Text = "💾 Lưu thay thế",
                Size = new Size(120, 30),
                Location = new Point(labelWidth + 20, y),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            btnLuuThayThe.Click += BtnLuuThayThe_Click;
            y += 40;

            pnlChiTiet.Controls.Add(lblThayThe);
            pnlChiTiet.Controls.Add(cboNguoiThayThe);
            pnlChiTiet.Controls.Add(btnLuuThayThe);

            // File đính kèm
            lnkFileDinhKem = new LinkLabel
            {
                Text = "📎 Không có file đính kèm",
                Location = new Point(10, y),
                Size = new Size(300, 30),
                Visible = false
            };
            lnkFileDinhKem.LinkClicked += LnkFileDinhKem_LinkClicked;
            y += 35;

            pnlChiTiet.Controls.Add(lnkFileDinhKem);

            // Trạng thái
            lblTrangThai = TaoLabelChiTiet("Trạng thái:", "", y);
            y += 35;

            // Người duyệt
            lblNguoiDuyet = TaoLabelChiTiet("Người duyệt:", "", y);
            y += 35;

            // Ngày gửi
            lblNgayGui = TaoLabelChiTiet("Ngày gửi:", "", y);
            y += 35;

            // Ngày duyệt
            lblNgayDuyet = TaoLabelChiTiet("Ngày duyệt:", "", y);
            y += 35;

            // Ghi chú duyệt (RichTextBox)
            Label lblGhiChuTitle = new Label
            {
                Text = "Ghi chú duyệt:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 66, 66),
                Location = new Point(10, y),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            rtbGhiChuDuyet = new RichTextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(labelWidth + 20, y),
                Size = new Size(300, 80),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            y += 90;

            pnlChiTiet.Controls.Add(lblGhiChuTitle);
            pnlChiTiet.Controls.Add(rtbGhiChuDuyet);

            // Panel nút hành động
            Panel pnlActions = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(400, 50),
                BackColor = Color.Transparent
            };

            btnDuyet = new Button
            {
                Text = "✅ DUYỆT ĐƠN",
                Size = new Size(120, 40),
                Location = new Point(10, 5),
                BackColor = Color.FromArgb(76, 175, 80), // Xanh lá
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Visible = false
            };
            btnDuyet.Click += BtnDuyet_Click;

            btnTuChoi = new Button
            {
                Text = "❌ TỪ CHỐI",
                Size = new Size(120, 40),
                Location = new Point(140, 5),
                BackColor = Color.FromArgb(244, 67, 54), // Đỏ
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Visible = false
            };
            btnTuChoi.Click += BtnTuChoi_Click;

            btnXacNhanDaNghi = new Button
            {
                Text = "📅 XÁC NHẬN ĐÃ NGHỈ",
                Size = new Size(150, 40),
                Location = new Point(270, 5),
                BackColor = Color.FromArgb(255, 152, 0), // Cam
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Visible = false
            };
            btnXacNhanDaNghi.Click += BtnXacNhanDaNghi_Click;

            btnHuyDon = new Button
            {
                Text = "🗑️ HỦY ĐƠN",
                Size = new Size(120, 40),
                Location = new Point(10, 5),
                BackColor = Color.FromArgb(158, 158, 158), // Xám
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Visible = false
            };
            btnHuyDon.Click += BtnHuyDon_Click;

            btnXemFile = new Button
            {
                Text = "📂 XEM FILE",
                Size = new Size(120, 40),
                Location = new Point(140, 5),
                BackColor = Color.FromArgb(63, 81, 181), // Tím
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Visible = false
            };
            btnXemFile.Click += BtnXemFile_Click;

            pnlActions.Controls.AddRange(new Control[]
            {
                btnDuyet, btnTuChoi, btnXacNhanDaNghi, btnHuyDon, btnXemFile
            });

            pnlChiTiet.Controls.Add(pnlActions);
        }

        private Label TaoLabelChiTiet(string labelText, string valueText, int y)
        {
            int labelWidth = 150;

            Label lblLabel = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 66, 66),
                Location = new Point(10, y),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            Label lblValue = new Label
            {
                Text = valueText,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(33, 33, 33),
                Location = new Point(labelWidth + 20, y),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Name = "lbl" + labelText.Replace(":", "").Replace(" ", "")
            };

            pnlChiTiet.Controls.Add(lblLabel);
            pnlChiTiet.Controls.Add(lblValue);

            return lblValue;
        }

        private void TaoTabThongKe()
        {
            tabThongKe.Padding = new Padding(10);

            // Chart thống kê
            chartThongKe = new System.Windows.Forms.DataVisualization.Charting.Chart
            {
                Dock = DockStyle.Top,
                Height = 300
            };
            TaoChartThongKe();

            // DataGridView thống kê chi tiết
            DataGridView dgvThongKe = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                RowHeadersVisible = false
            };
            dgvThongKe.Columns.Add("ChiTieu", "CHỈ TIÊU");
            dgvThongKe.Columns.Add("GiaTri", "GIÁ TRỊ");
            dgvThongKe.Columns.Add("MoTa", "MÔ TẢ");

            tabThongKe.Controls.Add(dgvThongKe);
            tabThongKe.Controls.Add(chartThongKe);
        }

        private void TaoChartThongKe()
        {
            chartThongKe.ChartAreas.Clear();
            chartThongKe.Series.Clear();
            chartThongKe.Titles.Clear();

            ChartArea chartArea = new ChartArea("ThongKe");
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.LabelStyle.Interval = 1;
            chartThongKe.ChartAreas.Add(chartArea);

            Title title = new Title("THỐNG KÊ ĐƠN XIN NGHỈ", Docking.Top,
                new Font("Segoe UI", 14, FontStyle.Bold), Color.FromArgb(33, 150, 243));
            chartThongKe.Titles.Add(title);
        }

        private void TaiDuLieuKhoiDau()
        {
            TaiDanhSachNhanVien();
        }

        private void TaiDanhSachNhanVien()
        {
            string query = @"
                SELECT MaNguoiDung, HoTen, TenDangNhap
                FROM NguoiDung
                WHERE MaChiNhanh = @MaChiNhanh
                  AND VaiTro = N'Nhân viên'
                  AND TrangThai = 1
                  AND MaNguoiDung != @ManagerId
                ORDER BY HoTen";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@ManagerId", _maNguoiDung);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtNhanVien = new DataTable();
                        da.Fill(_dtNhanVien);

                        // Load vào combobox
                        cboNguoiThayThe.Items.Clear();
                        cboNguoiThayThe.Items.Add("-- Chọn người thay thế --");

                        foreach (DataRow row in _dtNhanVien.Rows)
                        {
                            string display = $"{row["HoTen"]} ({row["TenDangNhap"]})";
                            cboNguoiThayThe.Items.Add(new ComboboxItem
                            {
                                Text = display,
                                Value = row["MaNguoiDung"]
                            });
                        }
                        cboNguoiThayThe.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách nhân viên: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiDanhSachDon()
        {
            string query = @"
                SELECT * FROM vw_DonXinNghi_ChiTiet
                WHERE MaChiNhanh = @MaChiNhanh
                  AND NgayBatDau BETWEEN @TuNgay AND @DenNgay
                ORDER BY UuTien DESC, NgayGui DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", dtpTuNgay.Value.Date);
                        cmd.Parameters.AddWithValue("@DenNgay", dtpDenNgay.Value.Date.AddDays(1));

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtDonXinNghi = new DataTable();
                        da.Fill(_dtDonXinNghi);

                        PhanLoaiDonVaoTab();
                        CapNhatThongKeFooter();
                        CapNhatChartThongKe();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách đơn: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PhanLoaiDonVaoTab()
        {
            foreach (TabPage tab in tabMain.TabPages)
            {
                if (tab.Controls[0] is DataGridView dgv)
                {
                    string trangThai = (string)dgv.Tag;
                    DataView dv = new DataView(_dtDonXinNghi);

                    if (trangThai != "ThongKe")
                    {
                        dv.RowFilter = $"TrangThai = '{trangThai}'";

                        // Áp dụng filter loại nghỉ
                        if (cboLoaiNghiFilter.SelectedIndex > 0)
                        {
                            string loaiNghi = cboLoaiNghiFilter.SelectedItem.ToString();
                            if (dv.RowFilter.Length > 0)
                                dv.RowFilter += " AND ";
                            dv.RowFilter += $"LoaiNghiText = '{loaiNghi}'";
                        }

                        // Áp dụng filter ưu tiên
                        if (cboUuTienFilter.SelectedIndex > 0)
                        {
                            if (dv.RowFilter.Length > 0)
                                dv.RowFilter += " AND ";

                            if (cboUuTienFilter.SelectedItem.ToString() == "Cần duyệt gấp")
                                dv.RowFilter += "UuTien = 1";
                            else
                                dv.RowFilter += "UuTien = 0";
                        }

                        // Áp dụng tìm kiếm
                        if (!string.IsNullOrEmpty(txtTimKiem.Text))
                        {
                            string search = txtTimKiem.Text.ToLower();
                            if (dv.RowFilter.Length > 0)
                                dv.RowFilter += " AND ";
                            dv.RowFilter += $"(TenNhanVien LIKE '%{search}%' OR LyDo LIKE '%{search}%' OR TenDangNhapNV LIKE '%{search}%')";
                        }
                    }

                    dgv.DataSource = dv;
                    DinhDangDataGridView(dgv, trangThai);
                }
            }
        }

        private void DinhDangDataGridView(DataGridView dgv, string trangThai)
        {
            if (dgv.Columns.Count == 0) return;

            // Ẩn cột không cần thiết
            string[] hiddenColumns = { "MaDonXinNghi", "MaNhanVien", "MaChiNhanh", "NguoiDuyet",
                                       "TenDangNhapNV", "TenDangNhapThayThe", "SDTNhanVien",
                                       "TrangThai", "LoaiNghi", "UuTien", "SoNgayConLai" };

            foreach (string colName in hiddenColumns)
            {
                if (dgv.Columns.Contains(colName))
                    dgv.Columns[colName].Visible = false;
            }

            // Đổi tên cột
            Dictionary<string, string> columnNames = new Dictionary<string, string>
            {
                { "TenNhanVien", "NHÂN VIÊN" },
                { "TenChiNhanh", "CHI NHÁNH" },
                { "LoaiNghiText", "LOẠI NGHỈ" },
                { "NgayBatDau", "TỪ NGÀY" },
                { "NgayKetThuc", "ĐẾN NGÀY" },
                { "SoNgay", "SỐ NGÀY" },
                { "LyDo", "LÝ DO" },
                { "TenNguoiThayThe", "NGƯỜI THAY THẾ" },
                { "TrangThaiText", "TRẠNG THÁI" },
                { "TenNguoiDuyet", "NGƯỜI DUYỆT" },
                { "NgayGui", "NGÀY GỬI" },
                { "NgayDuyet", "NGÀY DUYỆT" },
                { "FileDinhKem", "FILE ĐÍNH KÈM" }
            };

            foreach (var pair in columnNames)
            {
                if (dgv.Columns.Contains(pair.Key))
                {
                    dgv.Columns[pair.Key].HeaderText = pair.Value;

                    // Format ngày
                    if (pair.Key.Contains("Ngay"))
                    {
                        dgv.Columns[pair.Key].DefaultCellStyle.Format = "dd/MM/yyyy";
                        if (pair.Key == "NgayGui" || pair.Key == "NgayDuyet")
                            dgv.Columns[pair.Key].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    }

                    // Cột Lý do auto size
                    if (pair.Key == "LyDo")
                    {
                        dgv.Columns[pair.Key].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dgv.Columns[pair.Key].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    }
                }
            }

            // Thêm cột Số ngày còn lại cho tab Chờ duyệt
            if (trangThai == "ChoDuyet" && dgv.Columns.Contains("SoNgayConLai"))
            {
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "SoNgayConLaiDisplay",
                    HeaderText = "CÒN LẠI",
                    Width = 80
                });
            }
        }

        private void CapNhatThongKeFooter()
        {
            if (_dtDonXinNghi == null) return;

            int tongDon = _dtDonXinNghi.Rows.Count;
            int choDuyet = 0, daDuyet = 0, tuChoi = 0, daNghi = 0;

            foreach (DataRow row in _dtDonXinNghi.Rows)
            {
                string trangThai = row["TrangThai"].ToString();
                if (trangThai == "ChoDuyet") choDuyet++;
                else if (trangThai == "DaDuyet") daDuyet++;
                else if (trangThai == "TuChoi") tuChoi++;
                else if (trangThai == "DaNghi") daNghi++;
            }

            // Tìm label footer
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel panel && panel.Dock == DockStyle.Bottom)
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child.Name == "lblFooter")
                        {
                            child.Text = $"Tổng số đơn: {tongDon} | ⏳ Chờ duyệt: {choDuyet} | ✅ Đã duyệt: {daDuyet} | ❌ Từ chối: {tuChoi} | 📅 Đã nghỉ: {daNghi}";
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void CapNhatChartThongKe()
        {
            if (_dtDonXinNghi == null) return;

            chartThongKe.Series.Clear();

            // Tạo series cho biểu đồ tròn
            Series series = new Series("ThongKe");
            series.ChartType = SeriesChartType.Pie;
            series.IsValueShownAsLabel = true;
            series.Label = "#PERCENT{P1}";
            series.LegendText = "#VALX";

            // Tính số lượng theo trạng thái
            var statusGroups = _dtDonXinNghi.AsEnumerable()
                .GroupBy(row => row.Field<string>("TrangThaiText"))
                .Select(g => new { Status = g.Key, Count = g.Count() });

            foreach (var group in statusGroups)
            {
                DataPoint point = series.Points.Add(group.Count);
                point.AxisLabel = group.Status;
                point.LegendText = $"{group.Status}: {group.Count}";

                // Màu sắc theo trạng thái
                point.Color = GetColorForStatus(group.Status);
            }

            chartThongKe.Series.Add(series);
            chartThongKe.Legends[0].Enabled = true;
        }

        private Color GetColorForStatus(string status)
        {
            return status switch
            {
                "⏳ Chờ duyệt" => Color.FromArgb(255, 193, 7),   // Vàng
                "✅ Đã duyệt" => Color.FromArgb(76, 175, 80),    // Xanh lá
                "❌ Từ chối" => Color.FromArgb(244, 67, 54),     // Đỏ
                "📅 Đã nghỉ" => Color.FromArgb(33, 150, 243),    // Xanh dương
                "🗑️ Đã hủy" => Color.FromArgb(158, 158, 158),  // Xám
                _ => Color.Gray
            };
        }

        private void HienThiChiTietDon()
        {
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv == null || dgv.SelectedRows.Count == 0)
            {
                AnPanelChiTiet();
                return;
            }

            DataRowView rowView = (DataRowView)dgv.SelectedRows[0].DataBoundItem;
            DataRow row = rowView.Row;

            // Hiển thị thông tin cơ bản
            lblMaDon.Text = row["MaDonXinNghi"].ToString();
            lblNhanVien.Text = $"{row["TenNhanVien"]} ({row["TenDangNhapNV"]})";
            lblLoaiNghi.Text = row["LoaiNghiText"].ToString();
            lblNgayNghi.Text = $"{Convert.ToDateTime(row["NgayBatDau"]):dd/MM/yyyy} - {Convert.ToDateTime(row["NgayKetThuc"]):dd/MM/yyyy}";
            lblSoNgay.Text = row["SoNgay"].ToString() + " ngày";
            lblLyDo.Text = row["LyDo"].ToString();
            lblTrangThai.Text = row["TrangThaiText"].ToString();
            lblNguoiDuyet.Text = row["TenNguoiDuyet"] != DBNull.Value ? row["TenNguoiDuyet"].ToString() : "Chưa duyệt";
            lblNgayGui.Text = Convert.ToDateTime(row["NgayGui"]).ToString("dd/MM/yyyy HH:mm");
            lblNgayDuyet.Text = row["NgayDuyet"] != DBNull.Value ?
                Convert.ToDateTime(row["NgayDuyet"]).ToString("dd/MM/yyyy HH:mm") : "Chưa duyệt";

            rtbGhiChuDuyet.Text = row["GhiChuDuyet"] != DBNull.Value ? row["GhiChuDuyet"].ToString() : "";

            // File đính kèm
            if (row["FileDinhKem"] != DBNull.Value && !string.IsNullOrEmpty(row["FileDinhKem"].ToString()))
            {
                lnkFileDinhKem.Text = $"📎 {Path.GetFileName(row["FileDinhKem"].ToString())}";
                lnkFileDinhKem.Tag = row["FileDinhKem"].ToString();
                lnkFileDinhKem.Visible = true;
                btnXemFile.Visible = true;
            }
            else
            {
                lnkFileDinhKem.Visible = false;
                btnXemFile.Visible = false;
            }

            // Xử lý theo trạng thái
            string trangThai = row["TrangThai"].ToString();
            bool isChoDuyet = trangThai == "ChoDuyet";
            bool isDaDuyet = trangThai == "DaDuyet";
            bool isDaNghi = trangThai == "DaNghi";

            // Hiển thị/ẩn controls
            Label lblThayThe = (Label)pnlChiTiet.Controls.Find("lblThayThe", true).FirstOrDefault();

            lblThayThe.Visible = isChoDuyet;
            cboNguoiThayThe.Visible = isChoDuyet;
            btnLuuThayThe.Visible = isChoDuyet;

            btnDuyet.Visible = isChoDuyet;
            btnTuChoi.Visible = isChoDuyet;
            btnXacNhanDaNghi.Visible = isDaDuyet && Convert.ToDateTime(row["NgayBatDau"]) <= DateTime.Today;
            btnHuyDon.Visible = isChoDuyet;

            // Cho phép edit ghi chú cho đơn chờ duyệt
            rtbGhiChuDuyet.ReadOnly = !isChoDuyet;

            // Load người thay thế nếu có
            if (isChoDuyet && row["NguoiThayThe"] != DBNull.Value)
            {
                int nguoiThayThe = Convert.ToInt32(row["NguoiThayThe"]);
                for (int i = 0; i < cboNguoiThayThe.Items.Count; i++)
                {
                    if (cboNguoiThayThe.Items[i] is ComboboxItem item)
                    {
                        if (Convert.ToInt32(item.Value) == nguoiThayThe)
                        {
                            cboNguoiThayThe.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            else if (isChoDuyet)
            {
                cboNguoiThayThe.SelectedIndex = 0;
            }
        }

        private void AnPanelChiTiet()
        {
            // Ẩn tất cả controls chi tiết
            foreach (Control ctrl in pnlChiTiet.Controls)
            {
                if (ctrl is Button btn)
                    btn.Visible = false;
            }

            lnkFileDinhKem.Visible = false;

            Label lblThayThe = (Label)pnlChiTiet.Controls.Find("lblThayThe", true).FirstOrDefault();
            if (lblThayThe != null)
            {
                lblThayThe.Visible = false;
                cboNguoiThayThe.Visible = false;
                btnLuuThayThe.Visible = false;
            }
        }

        private DataGridView GetCurrentDataGridView()
        {
            return tabMain.SelectedTab?.Controls[0] as DataGridView;
        }

        // ==================== SỰ KIỆN ====================

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            PhanLoaiDonVaoTab();
            AnPanelChiTiet();
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            HienThiChiTietDon();
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataGridView dgv = (DataGridView)sender;
            DataRowView rowView = (DataRowView)dgv.Rows[e.RowIndex].DataBoundItem;

            // Tô màu dòng theo ưu tiên
            if (dgv.Tag?.ToString() == "ChoDuyet" && rowView["UuTien"].ToString() == "1")
            {
                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224); // Vàng nhạt
                dgv.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
            }

            // Cột số ngày còn lại
            if (dgv.Tag?.ToString() == "ChoDuyet" && dgv.Columns.Contains("SoNgayConLaiDisplay"))
            {
                int soNgayConLai = Convert.ToInt32(rowView["SoNgayConLai"]);
                dgv.Rows[e.RowIndex].Cells["SoNgayConLaiDisplay"].Value = soNgayConLai;

                // Đổi màu theo số ngày
                if (soNgayConLai <= 0)
                {
                    dgv.Rows[e.RowIndex].Cells["SoNgayConLaiDisplay"].Style.ForeColor = Color.Red;
                    dgv.Rows[e.RowIndex].Cells["SoNgayConLaiDisplay"].Style.Font = new Font(dgv.Font, FontStyle.Bold);
                }
                else if (soNgayConLai <= 3)
                {
                    dgv.Rows[e.RowIndex].Cells["SoNgayConLaiDisplay"].Style.ForeColor = Color.Orange;
                }
                else
                {
                    dgv.Rows[e.RowIndex].Cells["SoNgayConLaiDisplay"].Style.ForeColor = Color.Green;
                }
            }
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                HienThiChiTietDon();
            }
        }

        private void TxtTimKiem_TextChanged(object sender, EventArgs e)
        {
            PhanLoaiDonVaoTab();
        }

        private void CboLoaiNghiFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            PhanLoaiDonVaoTab();
        }

        private void CboUuTienFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            PhanLoaiDonVaoTab();
        }

        private void DtpTuNgay_ValueChanged(object sender, EventArgs e)
        {
            PhanLoaiDonVaoTab();
        }

        private void DtpDenNgay_ValueChanged(object sender, EventArgs e)
        {
            PhanLoaiDonVaoTab();
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            TaiDanhSachDon();
            AnPanelChiTiet();
        }

        private void BtnDuyet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblMaDon.Text)) return;

            int maDon = Convert.ToInt32(lblMaDon.Text);
            string ghiChu = rtbGhiChuDuyet.Text.Trim();

            if (MessageBox.Show("Bạn có chắc chắn DUYỆT đơn xin nghỉ này?", "Xác nhận duyệt",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DuyetDon(maDon, "DaDuyet", ghiChu);
            }
        }

        private void BtnTuChoi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblMaDon.Text)) return;

            int maDon = Convert.ToInt32(lblMaDon.Text);
            string ghiChu = rtbGhiChuDuyet.Text.Trim();

            if (string.IsNullOrEmpty(ghiChu))
            {
                MessageBox.Show("Vui lòng nhập lý do từ chối!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rtbGhiChuDuyet.Focus();
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn TỪ CHỐI đơn xin nghỉ này?", "Xác nhận từ chối",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DuyetDon(maDon, "TuChoi", ghiChu);
            }
        }

        private void BtnXacNhanDaNghi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblMaDon.Text)) return;

            int maDon = Convert.ToInt32(lblMaDon.Text);

            if (MessageBox.Show("Xác nhận nhân viên đã nghỉ xong?", "Xác nhận đã nghỉ",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DuyetDon(maDon, "DaNghi", "Đã nghỉ xong");
            }
        }

        private void BtnHuyDon_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblMaDon.Text)) return;

            int maDon = Convert.ToInt32(lblMaDon.Text);

            if (MessageBox.Show("Bạn có chắc chắn HỦY đơn xin nghỉ này?", "Xác nhận hủy",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DuyetDon(maDon, "Huy", "Đã hủy bởi quản lý");
            }
        }

        private void BtnLuuThayThe_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblMaDon.Text)) return;
            if (cboNguoiThayThe.SelectedIndex <= 0)
            {
                MessageBox.Show("Vui lòng chọn người thay thế!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maDon = Convert.ToInt32(lblMaDon.Text);
            ComboboxItem selectedItem = (ComboboxItem)cboNguoiThayThe.SelectedItem;
            int maNguoiThayThe = Convert.ToInt32(selectedItem.Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE DonXinNghi 
                        SET NguoiThayThe = @NguoiThayThe
                        WHERE MaDonXinNghi = @MaDon";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NguoiThayThe", maNguoiThayThe);
                        cmd.Parameters.AddWithValue("@MaDon", maDon);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Đã cập nhật người thay thế!", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh data
                            TaiDanhSachDon();
                            HienThiChiTietDon();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật người thay thế: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXemFile_Click(object sender, EventArgs e)
        {
            if (lnkFileDinhKem.Tag != null)
            {
                string filePath = lnkFileDinhKem.Tag.ToString();

                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("File không tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LnkFileDinhKem_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BtnXemFile_Click(sender, e);
        }

        // ==================== CÁC PHƯƠNG THỨC HỖ TRỢ ====================

        private void DuyetDon(int maDon, string trangThaiMoi, string ghiChu)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_DuyetDonXinNghi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaDonXinNghi", maDon);
                        cmd.Parameters.AddWithValue("@MaNguoiDuyet", _maNguoiDung);
                        cmd.Parameters.AddWithValue("@TrangThaiMoi", trangThaiMoi);
                        cmd.Parameters.AddWithValue("@GhiChuDuyet", ghiChu);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool ketQua = reader.GetBoolean(0);
                                string thongBao = reader.GetString(1);

                                if (ketQua)
                                {
                                    MessageBox.Show(thongBao, "Thành công",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // Refresh data
                                    TaiDanhSachDon();
                                    AnPanelChiTiet();
                                }
                                else
                                {
                                    MessageBox.Show(thongBao, "Lỗi",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi duyệt đơn: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper class cho combobox
        private class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}