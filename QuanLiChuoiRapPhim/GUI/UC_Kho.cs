using QuanLiChuoiRapPhim.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// UC_Kho - UserControl Quản Lý Kho Bắp Nước
    /// 
    /// Chức năng chính:
    /// 1. Theo dõi tồn kho hiện tại của các sản phẩm (bắp nước, đồ ăn)
    /// 2. Quản lý phiếu nhập kho từ nhà cung cấp
    /// 3. Quản lý phiếu xuất kho cho các chi nhánh
    /// 4. Báo cáo tồn kho và lịch sử giao dịch
    /// 5. Cảnh báo sản phẩm sắp hết hoặc quá tồn
    /// 
    /// Tabs:
    /// - TỒN KHO: Hiển thị tồn kho hiện tại, lọc theo loại, tìm kiếm
    /// - NHẬP KHO: Tạo/Sửa/Xóa phiếu nhập, thêm chi tiết hàng nhập
    /// - XUẤT KHO: Tạo/Sửa/Xóa phiếu xuất, quản lý đơn hàng xuất
    /// - BÁO CÁO: Thống kê tồn kho theo thời gian, xu hướng sử dụng
    /// 
    /// Quyền hạn: Quản lý chi nhánh (Manager)
    /// Tác giả: CGV Management System
    /// Ngày tạo: Tháng 12 - 2025
    /// </summary>
    public partial class UC_Kho : UserControl
    {
        private int _maChiNhanh;                    // Chi nhánh đang chọn (có thể thay đổi nếu Admin)
        private readonly int _maNguoiDung;          // ID của người dùng hiện tại
        private readonly bool _isAdmin;             // Flag xác định có phải Admin không
        private DataTable _dtSanPham;               // Danh sách sản phẩm
        private DataTable _dtTonKho;                // Tồn kho hiện tại
        private DataTable _dtNhapKho;               // Lịch sử nhập
        private DataTable _dtXuatKho;               // Lịch sử xuất

        // Controls chính
        private TabControl tabMain;
        private TabPage tabTonKho, tabNhapKho, tabXuatKho, tabBaoCao;
        private DataGridView dgvTonKho, dgvNhapKho, dgvXuatKho, dgvBaoCao;
        private Button btnNhapKho, btnXuatKho, btnCapNhatTon, btnXemChiTiet;
        private ComboBox cboLoaiSPFilter, cboTrangThaiXuat, cboChiNhanh;
        private DateTimePicker dtpTuNgay, dtpDenNgay;
        private TextBox txtTimKiemSP;

        // Controls thêm/sửa
        private Panel pnlChiTiet;
        private DataGridView dgvChiTiet;
        private Label lblTongTien, lblSoLuongSP;
        private Button btnLuuPhieu, btnInPhieu, btnHuyPhieu;

        // Biến tạm
        private DataTable _dtChiTietTam;           // Chi tiết phiếu tạm
        private bool _isNhapKho = true;            // true: Nhập, false: Xuất
        private int _maPhieuHienTai = 0;           // Mã phiếu đang sửa

        /// <summary>
        /// Constructor - Khởi tạo UC_Kho
        /// </summary>
        /// <param name="maChiNhanh">Mã chi nhánh của Manager (0 nếu Admin)</param>
        /// <param name="maNguoiDung">ID người dùng đang đăng nhập</param>
        public UC_Kho(int maChiNhanh, int maNguoiDung)
        {
            _maChiNhanh = maChiNhanh;
            _maNguoiDung = maNguoiDung;
            _isAdmin = (maChiNhanh == 0); // Admin không có chi nhánh cụ thể
            _khoDAL = new KhoDAL();

            // Debug log
            System.Diagnostics.Debug.WriteLine($"UC_Kho initialized with MaChiNhanh={_maChiNhanh}, MaNguoiDung={_maNguoiDung}, IsAdmin={_isAdmin}");

            // Auto-run migration to ensure tables exist
            EnsureTablesExist();

            ThietLapGiaoDien();
            
            // Nếu là Admin, cần chọn chi nhánh trước khi tải dữ liệu
            if (_isAdmin)
            {
                LoadChiNhanhCombobox();
            }
            else
            {
                // Insert mock data để demo (chỉ insert 1 lần, check trong DAL)
                try
                {
                    _khoDAL.InsertMockInventoryData(_maChiNhanh, _maNguoiDung);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Mock data insertion skipped: {ex.Message}");
                }
                
                TaiDuLieuKhoiDau();
                UpdateLowStockWarning();
            }
        }

        // DAL instance
        private KhoDAL _khoDAL;

        // Low-stock warning label (Phase 4: Quick Win)
        private Label lblLowStockWarning;

        // KPI Labels
        private Label lblKPITongSP, lblKPITongTon, lblKPIGiaTri, lblKPISapHet;

        /// <summary>
        /// Đảm bảo các bảng kho tồn tại trong database
        /// </summary>
        private void EnsureTablesExist()
        {
            try
            {
                if (!_khoDAL.CheckNhapKhoTableExists())
                {
                    // Hiển thị thông báo và cho phép chạy migration
                    var result = MessageBox.Show(
                        "Hệ thống phát hiện chưa có các bảng quản lý kho.\n\nBạn có muốn tạo các bảng cần thiết không?\n\n(NhaCungCap, NhapKho, XuatKho, ChiTietNhapKho, ChiTietXuatKho, KiemKe)",
                        "Cấu hình Database",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        RunDatabaseMigration();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureTablesExist error: {ex.Message}");
            }
        }

        /// <summary>
        /// Chạy migration script tạo bảng
        /// </summary>
        private void RunDatabaseMigration()
        {
            try
            {
                // Đọc và chạy script SQL trực tiếp
                string script = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NhaCungCap')
                    BEGIN
                        CREATE TABLE NhaCungCap (
                            MaNhaCungCap INT PRIMARY KEY IDENTITY(1,1),
                            TenNhaCungCap NVARCHAR(200) NOT NULL,
                            DiaChi NVARCHAR(500),
                            SoDienThoai VARCHAR(20),
                            Email VARCHAR(100),
                            NguoiLienHe NVARCHAR(100),
                            GhiChu NVARCHAR(500),
                            TrangThai BIT DEFAULT 1,
                            NgayTao DATETIME DEFAULT GETDATE()
                        );
                    END

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NhapKho')
                    BEGIN
                        CREATE TABLE NhapKho (
                            MaNhapKho INT PRIMARY KEY IDENTITY(1,1),
                            MaChiNhanh INT NOT NULL,
                            MaNhaCungCap INT NULL,
                            MaNguoiNhap INT NOT NULL,
                            NgayNhap DATETIME DEFAULT GETDATE(),
                            TongTien DECIMAL(15,2) DEFAULT 0,
                            SoHoaDon NVARCHAR(100),
                            GhiChu NVARCHAR(500),
                            TrangThai NVARCHAR(50) DEFAULT N'DaNhap'
                        );
                    END

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChiTietNhapKho')
                    BEGIN
                        CREATE TABLE ChiTietNhapKho (
                            MaChiTietNhap INT PRIMARY KEY IDENTITY(1,1),
                            MaNhapKho INT NOT NULL,
                            MaSanPham INT NOT NULL,
                            SoLuongNhap INT NOT NULL,
                            DonGiaNhap DECIMAL(10,2) NOT NULL,
                            ThanhTien DECIMAL(15,2) NOT NULL,
                            GhiChu NVARCHAR(200)
                        );
                    END

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'XuatKho')
                    BEGIN
                        CREATE TABLE XuatKho (
                            MaXuatKho INT PRIMARY KEY IDENTITY(1,1),
                            MaChiNhanhXuat INT NOT NULL,
                            MaChiNhanhNhan INT NULL,
                            MaNguoiXuat INT NOT NULL,
                            MaNguoiXacNhan INT NULL,
                            NgayXuat DATETIME DEFAULT GETDATE(),
                            NgayXacNhan DATETIME NULL,
                            TongTien DECIMAL(15,2) DEFAULT 0,
                            LoaiXuat NVARCHAR(50) DEFAULT N'XuatBan',
                            LyDoXuat NVARCHAR(500),
                            TrangThai NVARCHAR(50) DEFAULT N'ChoXacNhan',
                            GhiChu NVARCHAR(500)
                        );
                    END

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChiTietXuatKho')
                    BEGIN
                        CREATE TABLE ChiTietXuatKho (
                            MaChiTietXuat INT PRIMARY KEY IDENTITY(1,1),
                            MaXuatKho INT NOT NULL,
                            MaSanPham INT NOT NULL,
                            SoLuongXuat INT NOT NULL,
                            DonGiaXuat DECIMAL(10,2) NOT NULL,
                            ThanhTien DECIMAL(15,2) NOT NULL,
                            GhiChu NVARCHAR(200)
                        );
                    END

                    -- Insert sample suppliers if empty
                    IF NOT EXISTS (SELECT TOP 1 1 FROM NhaCungCap)
                    BEGIN
                        INSERT INTO NhaCungCap (TenNhaCungCap, DiaChi, SoDienThoai, NguoiLienHe, GhiChu) VALUES
                        (N'Công ty TNHH Bắp Ngô Việt Nam', N'123 Nguyễn Văn Linh, Q.7, TP.HCM', '028-1234567', N'Nguyễn Văn A', N'NCC chính - Bắp rang'),
                        (N'Pepsi Vietnam', N'456 Lê Văn Việt, Q.9, TP.HCM', '028-9876543', N'Trần Thị B', N'NCC nước ngọt Pepsi'),
                        (N'Coca-Cola Vietnam', N'789 Điện Biên Phủ, Q.3, TP.HCM', '028-5555555', N'Lê Văn C', N'NCC nước ngọt Coca');
                    END
                ";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(script, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Đã tạo các bảng quản lý kho thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo bảng: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Thiết lập giao diện chính của UserControl
        /// Tạo: Header tiêu đề, KPI Cards, TabControl với 4 tab chính, Panel chi tiết phiếu
        /// </summary>
        private void ThietLapGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // === TIÊU ĐỀ ===
            Panel pnlTieuDe = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(255, 193, 7) // Vàng
            };

            Label lblTieuDe = new Label
            {
                Text = "📦 QUẢN LÝ KHO BẮP NƯỚC",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlTieuDe.Controls.Add(lblTieuDe);

            // === COMBOBOX CHỌN CHI NHÁNH (CHỈ ADMIN) ===
            if (_isAdmin)
            {
                Label lblChiNhanh = new Label
                {
                    Text = "Chi nhánh:",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(380, 18),
                    AutoSize = true
                };
                pnlTieuDe.Controls.Add(lblChiNhanh);

                cboChiNhanh = new ComboBox
                {
                    Location = new Point(470, 14),
                    Size = new Size(250, 30),
                    Font = new Font("Segoe UI", 10F),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboChiNhanh.SelectedIndexChanged += CboChiNhanh_SelectedIndexChanged;
                pnlTieuDe.Controls.Add(cboChiNhanh);
            }

            // Low-stock warning indicator
            lblLowStockWarning = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(220, 53, 69),
                AutoSize = false,
                Size = new Size(160, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            lblLowStockWarning.Location = new Point(pnlTieuDe.Width - 180, 15);
            lblLowStockWarning.Click += (s, e) => { tabMain.SelectedTab = tabTonKho; };
            pnlTieuDe.Controls.Add(lblLowStockWarning);

            // === KPI CARDS ===
            Panel pnlKPI = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(15, 10, 15, 10)
            };

            TableLayoutPanel kpiGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };
            for (int i = 0; i < 4; i++)
                kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            // KPI Card 1: Tổng sản phẩm
            Panel card1 = CreateKPICard("📦", "LOẠI SẢN PHẨM", "0", Color.FromArgb(52, 152, 219), out lblKPITongSP);
            // KPI Card 2: Tổng tồn kho
            Panel card2 = CreateKPICard("📊", "TỔNG TỒN KHO", "0", Color.FromArgb(46, 204, 113), out lblKPITongTon);
            // KPI Card 3: Giá trị kho
            Panel card3 = CreateKPICard("💰", "GIÁ TRỊ KHO", "0đ", Color.FromArgb(155, 89, 182), out lblKPIGiaTri);
            // KPI Card 4: Sắp hết
            Panel card4 = CreateKPICard("⚠️", "SẮP HẾT HÀNG", "0", Color.FromArgb(231, 76, 60), out lblKPISapHet);

            kpiGrid.Controls.Add(card1, 0, 0);
            kpiGrid.Controls.Add(card2, 1, 0);
            kpiGrid.Controls.Add(card3, 2, 0);
            kpiGrid.Controls.Add(card4, 3, 0);
            pnlKPI.Controls.Add(kpiGrid);

            // === TAB CONTROL ===
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };

            tabTonKho = new TabPage("📊 TỒN KHO");
            tabNhapKho = new TabPage("⬇️ NHẬP KHO");
            tabXuatKho = new TabPage("⬆️ XUẤT KHO");
            tabBaoCao = new TabPage("📈 BÁO CÁO");

            tabMain.TabPages.AddRange(new TabPage[] { tabTonKho, tabNhapKho, tabXuatKho, tabBaoCao });
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

            // Thiết lập từng tab
            ThietLapTabTonKho();
            ThietLapTabNhapKho();
            ThietLapTabXuatKho();
            ThietLapTabBaoCao();

            // === PANEL CHI TIẾT PHIẾU ===
            // Panel này hiển thị chi tiết của phiếu nhập/xuất kho
            // Cho phép thêm, sửa, xóa các dòng chi tiết
            pnlChiTiet = new Panel
            {
                Dock = DockStyle.Right,
                Width = 500,
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Padding = new Padding(15)
            };
            TaoPanelChiTiet();

            // Thêm controls vào UserControl
            this.Controls.Add(pnlChiTiet);
            this.Controls.Add(tabMain);
            this.Controls.Add(pnlKPI);
            this.Controls.Add(pnlTieuDe);
        }

        /// <summary>
        /// Tạo KPI Card
        /// </summary>
        private Panel CreateKPICard(string icon, string title, string value, Color accentColor, out Label lblValue)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(5)
            };
            card.BorderRadius(10);

            Panel accent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accentColor
            };

            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 20),
                Location = new Point(15, 15),
                AutoSize = true
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(75, 12),
                AutoSize = true
            };

            lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(75, 35),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { accent, lblIcon, lblTitle, lblValue });
            return card;
        }

        /// <summary>
        /// Thiết lập giao diện Tab "TỒN KHO"
        /// Hiển thị: DataGridView tồn kho, Combobox lọc loại, TextBox tìm kiếm, nút Làm mới/Xem chi tiết
        /// </summary>
        private void ThietLapTabTonKho()
        {
            tabTonKho.Padding = new Padding(10);
            tabTonKho.BackColor = Color.FromArgb(245, 245, 245);

            // Panel công cụ
            Panel pnlCongCu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(10, 10, 10, 10)
            };
            pnlCongCu.BorderRadius(8);

            // Tìm kiếm
            Label lblTimKiem = new Label
            {
                Text = "Tìm:",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 15),
                AutoSize = true
            };

            txtTimKiemSP = new TextBox
            {
                Size = new Size(250, 30),
                Location = new Point(60, 10),
                Font = new Font("Segoe UI", 10F)
            };
            txtTimKiemSP.TextChanged += TxtTimKiemSP_TextChanged;

            // Lọc loại SP
            Label lblLoaiSP = new Label
            {
                Text = "Loại:",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(330, 15),
                AutoSize = true
            };

            cboLoaiSPFilter = new ComboBox
            {
                Size = new Size(120, 30),
                Location = new Point(380, 10),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLoaiSPFilter.Items.AddRange(new string[] { "Tất cả", "Bap", "Nuoc", "Combo" });
            cboLoaiSPFilter.SelectedIndex = 0;
            cboLoaiSPFilter.SelectedIndexChanged += CboLoaiSPFilter_SelectedIndexChanged;

            // Nút cập nhật
            btnCapNhatTon = new Button
            {
                Text = "🔄 CẬP NHẬT",
                Size = new Size(140, 35),
                Location = new Point(520, 8),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnCapNhatTon.Click += BtnCapNhatTon_Click;

            // Nút thêm sản phẩm mới
            Button btnThemSP = new Button
            {
                Text = "➕ THÊM SP",
                Size = new Size(120, 35),
                Location = new Point(670, 8),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThemSP.FlatAppearance.BorderSize = 0;
            btnThemSP.Click += BtnThemSanPham_Click;

            // Nút nhập kho
            btnNhapKho = new Button
            {
                Text = "⬇️ NHẬP KHO",
                Size = new Size(120, 35),
                Location = new Point(800, 8),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNhapKho.FlatAppearance.BorderSize = 0;
            btnNhapKho.Click += BtnNhapKho_Click;

            // Nút xuất Excel
            Button btnExportExcel = new Button
            {
                Text = "📤 EXCEL",
                Size = new Size(100, 35),
                Location = new Point(930, 8),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExportExcel.FlatAppearance.BorderSize = 0;
            btnExportExcel.Click += BtnExportExcel_Click;

            pnlCongCu.Controls.AddRange(new Control[]
            {
                lblTimKiem, txtTimKiemSP,
                lblLoaiSP, cboLoaiSPFilter,
                btnCapNhatTon, btnThemSP, btnNhapKho, btnExportExcel
            });

            // DataGridView
            dgvTonKho = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                MultiSelect = false,
                RowTemplate = { Height = 40 }
            };
            DinhDangDataGridView(dgvTonKho);

            // Double-click để sửa sản phẩm
            dgvTonKho.CellDoubleClick += DgvTonKho_CellDoubleClick;

            // Sự kiện
            dgvTonKho.SelectionChanged += (s, e) =>
            {
                btnXuatKho.Enabled = dgvTonKho.SelectedRows.Count > 0;
            };

            dgvTonKho.CellFormatting += DgvTonKho_CellFormatting;

            // Panel chứa grid với margin
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };
            pnlGrid.Controls.Add(dgvTonKho);

            tabTonKho.Controls.Add(pnlGrid);
            tabTonKho.Controls.Add(pnlCongCu);
        }

        /// <summary>
        /// Xử lý double-click để sửa sản phẩm
        /// </summary>
        private void DgvTonKho_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            var row = dgvTonKho.Rows[e.RowIndex];
            int maSP = Convert.ToInt32(row.Cells["MaSanPham"].Value);
            string tenSP = row.Cells["TenSanPham"].Value?.ToString() ?? "";
            string loaiSP = row.Cells["LoaiSanPham"].Value?.ToString() ?? "Bap";
            decimal giaBan = Convert.ToDecimal(row.Cells["GiaBan"].Value);
            string donVi = row.Cells["DonVi"].Value?.ToString() ?? "";
            int soLuong = Convert.ToInt32(row.Cells["SoLuongTon"].Value);

            HienFormSanPham(maSP, tenSP, loaiSP, giaBan, donVi, soLuong);
        }

        /// <summary>
        /// Nút thêm sản phẩm mới
        /// </summary>
        private void BtnThemSanPham_Click(object sender, EventArgs e)
        {
            HienFormSanPham(0, "", "Bap", 0, "", 0);
        }

        /// <summary>
        /// Hiển thị form thêm/sửa sản phẩm
        /// </summary>
        private void HienFormSanPham(int maSP, string tenSP, string loaiSP, decimal giaBan, string donVi, int soLuong)
        {
            // Kiểm tra mã chi nhánh hợp lệ
            if (_maChiNhanh <= 0)
            {
                if (_isAdmin)
                {
                    MessageBox.Show("Vui lòng chọn chi nhánh trước khi thêm/sửa sản phẩm.",
                        "Chưa chọn chi nhánh",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Lỗi: Mã chi nhánh không hợp lệ (MaChiNhanh = {_maChiNhanh}).\n\nVui lòng đăng xuất và đăng nhập lại.",
                        "Lỗi Dữ Liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                return;
            }

            using (var frm = new FormSanPham(maSP, tenSP, loaiSP, giaBan, donVi, soLuong))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (maSP == 0)
                        {
                            // Thêm mới
                            _khoDAL.ThemSanPham(frm.TenSanPham, frm.LoaiSanPham, frm.GiaBan, frm.DonVi, frm.SoLuong, _maChiNhanh);
                            MessageBox.Show("Thêm sản phẩm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            // Cập nhật
                            _khoDAL.CapNhatSanPham(maSP, frm.TenSanPham, frm.LoaiSanPham, frm.GiaBan, frm.DonVi);
                            if (frm.SoLuong != soLuong)
                            {
                                _khoDAL.CapNhatSoLuongTon(maSP, frm.SoLuong);
                            }
                            MessageBox.Show("Cập nhật sản phẩm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        TaiTonKho();
                        UpdateKPIs();
                        UpdateLowStockWarning();
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Lỗi khi {(maSP == 0 ? "thêm" : "cập nhật")} sản phẩm:\n\n{ex.Message}";
                        
                        // Xử lý lỗi FOREIGN KEY cụ thể
                        if (ex.Message.Contains("FOREIGN KEY") && ex.Message.Contains("MaChiNhanh"))
                        {
                            errorMsg += $"\n\nMã chi nhánh {_maChiNhanh} không tồn tại trong hệ thống.\nVui lòng liên hệ quản trị viên.";
                        }
                        
                        MessageBox.Show(errorMsg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Thiết lập giao diện Tab "NHẬP KHO"
        /// Hiển thị: Danh sách phiếu nhập, nút Thêm/Sửa/Xóa, DatePicker chọn ngày
        /// Cho phép Manager nhập hàng từ nhà cung cấp
        /// </summary>
        private void ThietLapTabNhapKho()
        {
            tabNhapKho.Padding = new Padding(10);

            // Panel filter
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent
            };

            Label lblTuNgay = new Label
            {
                Text = "Từ:",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 15),
                AutoSize = true
            };

            dtpTuNgay = new DateTimePicker
            {
                Size = new Size(120, 30),
                Location = new Point(50, 10),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-1)
            };
            dtpTuNgay.ValueChanged += DtpTuNgay_ValueChanged;

            Label lblDenNgay = new Label
            {
                Text = "Đến:",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(180, 15),
                AutoSize = true
            };

            dtpDenNgay = new DateTimePicker
            {
                Size = new Size(120, 30),
                Location = new Point(220, 10),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            dtpDenNgay.ValueChanged += DtpDenNgay_ValueChanged;

            // Nút xem chi tiết
            btnXemChiTiet = new Button
            {
                Text = "👁️ XEM CHI TIẾT",
                Size = new Size(150, 35),
                Location = new Point(360, 8),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Enabled = false
            };
            btnXemChiTiet.Click += BtnXemChiTiet_Click;

            // Nút xuất kho (từ tab này)
            btnXuatKho = new Button
            {
                Text = "⬆️ XUẤT KHO",
                Size = new Size(140, 35),
                Location = new Point(530, 8),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Enabled = false
            };
            btnXuatKho.Click += BtnXuatKho_Click;

            pnlFilter.Controls.AddRange(new Control[]
            {
                lblTuNgay, dtpTuNgay,
                lblDenNgay, dtpDenNgay,
                btnXemChiTiet, btnXuatKho
            });

            // DataGridView
            dgvNhapKho = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            DinhDangDataGridView(dgvNhapKho);

            // Sự kiện
            dgvNhapKho.SelectionChanged += (s, e) =>
            {
                btnXemChiTiet.Enabled = dgvNhapKho.SelectedRows.Count > 0;
            };

            tabNhapKho.Controls.Add(dgvNhapKho);
            tabNhapKho.Controls.Add(pnlFilter);
        }

        /// <summary>
        /// Thiết lập giao diện Tab "XUẤT KHO"
        /// Hiển thị: Danh sách phiếu xuất cho các chi nhánh, nút Thêm/Sửa/Xóa
        /// Cho phép quản lý việc xuất bắp nước đến chi nhánh
        /// </summary>
        private void ThietLapTabXuatKho()
        {
            tabXuatKho.Padding = new Padding(10);

            // Panel filter - Lọc phiếu xuất theo ngày, trạng thái
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent
            };

            // Lọc trạng thái
            Label lblTrangThai = new Label
            {
                Text = "Trạng thái:",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 15),
                AutoSize = true
            };

            cboTrangThaiXuat = new ComboBox
            {
                Size = new Size(150, 30),
                Location = new Point(100, 10),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTrangThaiXuat.Items.AddRange(new string[] { "Tất cả", "ChoXacNhan", "DaXacNhan", "DaNhan", "Huy" });
            cboTrangThaiXuat.SelectedIndex = 0;
            cboTrangThaiXuat.SelectedIndexChanged += CboTrangThaiXuat_SelectedIndexChanged;

            pnlFilter.Controls.AddRange(new Control[] { lblTrangThai, cboTrangThaiXuat });

            // DataGridView
            dgvXuatKho = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            DinhDangDataGridView(dgvXuatKho);

            // Sự kiện
            dgvXuatKho.CellContentClick += DgvXuatKho_CellContentClick;

            tabXuatKho.Controls.Add(dgvXuatKho);
            tabXuatKho.Controls.Add(pnlFilter);
        }

        /// <summary>
        /// Thiết lập giao diện Tab "BÁO CÁO"
        /// Hiển thị: Biểu đồ tồn kho theo thời gian, thống kê nhập/xuất, xu hướng sử dụng
        /// Cung cấp các chỉ số quản lý kho chi tiết
        /// </summary>
        private void ThietLapTabBaoCao()
        {
            tabBaoCao.Padding = new Padding(10);

            dgvBaoCao = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            tabBaoCao.Controls.Add(dgvBaoCao);
        }

        private void TaoPanelChiTiet()
        {
            // Tiêu đề động
            Label lblTitle = new Label
            {
                Text = "CHI TIẾT PHIẾU NHẬP",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(10, 20),
                Size = new Size(400, 30)
            };
            pnlChiTiet.Controls.Add(lblTitle);

            // DataGridView chi tiết
            dgvChiTiet = new DataGridView
            {
                Location = new Point(10, 60),
                Size = new Size(460, 300),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            // Tạo cột cho chi tiết
            dgvChiTiet.Columns.Add("MaSanPham", "MÃ SP");
            dgvChiTiet.Columns.Add("TenSanPham", "TÊN SẢN PHẨM");
            dgvChiTiet.Columns.Add("SoLuong", "SỐ LƯỢNG");
            dgvChiTiet.Columns.Add("DonGia", "ĐƠN GIÁ");
            dgvChiTiet.Columns.Add("ThanhTien", "THÀNH TIỀN");

            dgvChiTiet.Columns["MaSanPham"].Visible = false;
            dgvChiTiet.Columns["TenSanPham"].Width = 200;
            dgvChiTiet.Columns["SoLuong"].Width = 80;
            dgvChiTiet.Columns["DonGia"].Width = 100;
            dgvChiTiet.Columns["ThanhTien"].Width = 120;

            dgvChiTiet.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            dgvChiTiet.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";

            // Thêm nút Xóa vào mỗi dòng
            DataGridViewButtonColumn btnXoa = new DataGridViewButtonColumn
            {
                Text = "Xóa",
                UseColumnTextForButtonValue = true,
                Width = 60
            };
            dgvChiTiet.Columns.Add(btnXoa);
            dgvChiTiet.CellContentClick += DgvChiTiet_CellContentClick;

            // Thông tin tổng
            Panel pnlTong = new Panel
            {
                Location = new Point(10, 370),
                Size = new Size(460, 60),
                BackColor = Color.FromArgb(240, 240, 245)
            };

            lblSoLuongSP = new Label
            {
                Text = "Số sản phẩm: 0",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(200, 30)
            };

            lblTongTien = new Label
            {
                Text = "Tổng tiền: 0 đ",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                Location = new Point(250, 15),
                Size = new Size(200, 30)
            };

            pnlTong.Controls.AddRange(new Control[] { lblSoLuongSP, lblTongTien });

            // Nút hành động
            Panel pnlNut = new Panel
            {
                Location = new Point(10, 440),
                Size = new Size(460, 50)
            };

            btnLuuPhieu = new Button
            {
                Text = "💾 LƯU PHIẾU",
                Size = new Size(120, 35),
                Location = new Point(20, 8),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnLuuPhieu.Click += BtnLuuPhieu_Click;

            btnInPhieu = new Button
            {
                Text = "🖨️ IN PHIẾU",
                Size = new Size(120, 35),
                Location = new Point(160, 8),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Visible = false
            };
            btnInPhieu.Click += BtnInPhieu_Click;

            btnHuyPhieu = new Button
            {
                Text = "❌ HỦY",
                Size = new Size(120, 35),
                Location = new Point(300, 8),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnHuyPhieu.Click += BtnHuyPhieu_Click;

            pnlNut.Controls.AddRange(new Control[] { btnLuuPhieu, btnInPhieu, btnHuyPhieu });

            pnlChiTiet.Controls.Add(dgvChiTiet);
            pnlChiTiet.Controls.Add(pnlTong);
            pnlChiTiet.Controls.Add(pnlNut);
        }

        private void TaiDuLieuKhoiDau()
        {
            TaiTonKho();
            TaiDanhSachSanPham();
            UpdateKPIs();
            UpdateLowStockWarning();
        }

        /// <summary>
        /// Update low-stock warning indicator in header (Phase 4: Quick Win)
        /// Shows warning badge if any products are low stock (<=10 units)
        /// </summary>
        private void UpdateLowStockWarning()
        {
            if (_dtTonKho == null || lblLowStockWarning == null) return;

            try
            {
                int lowStockCount = 0;
                foreach (DataRow row in _dtTonKho.Rows)
                {
                    if (row["SoLuongKhaDung"] != DBNull.Value)
                    {
                        int qty = Convert.ToInt32(row["SoLuongKhaDung"]);
                        if (qty <= 10) lowStockCount++;
                    }
                }

                if (lowStockCount > 0)
                {
                    lblLowStockWarning.Text = $"⚠️ {lowStockCount} SP sắp hết";
                    lblLowStockWarning.Visible = true;
                }
                else
                {
                    lblLowStockWarning.Visible = false;
                }
            }
            catch
            {
                lblLowStockWarning.Visible = false;
            }
        }

        /// <summary>
        /// Cập nhật các KPI Cards hiển thị trên header
        /// </summary>
        private void UpdateKPIs()
        {
            if (_dtTonKho == null) return;

            try
            {
                int tongLoaiSP = _dtTonKho.Rows.Count;
                int tongTonKho = 0;
                decimal giaTriKho = 0;
                int sapHet = 0;

                foreach (DataRow row in _dtTonKho.Rows)
                {
                    int soLuong = row["SoLuongTon"] != DBNull.Value ? Convert.ToInt32(row["SoLuongTon"]) : 0;
                    decimal giaBan = row["GiaBan"] != DBNull.Value ? Convert.ToDecimal(row["GiaBan"]) : 0;

                    tongTonKho += soLuong;
                    giaTriKho += soLuong * giaBan;

                    if (soLuong <= 10) sapHet++;
                }

                // Update KPI labels
                if (lblKPITongSP != null) lblKPITongSP.Text = tongLoaiSP.ToString();
                if (lblKPITongTon != null) lblKPITongTon.Text = tongTonKho.ToString("N0");
                if (lblKPIGiaTri != null) lblKPIGiaTri.Text = giaTriKho.ToString("N0") + "đ";
                if (lblKPISapHet != null)
                {
                    lblKPISapHet.Text = sapHet.ToString();
                    // Highlight if there are low stock items
                    if (sapHet > 0)
                    {
                        lblKPISapHet.ForeColor = Color.FromArgb(231, 76, 60);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateKPIs error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load danh sách chi nhánh cho Admin chọn
        /// </summary>
        private void LoadChiNhanhCombobox()
        {
            if (cboChiNhanh == null) return;

            try
            {
                var chiNhanhDAL = new ChiNhanhDAL();
                var dtChiNhanh = chiNhanhDAL.LayTatCaChiNhanh();

                cboChiNhanh.Items.Clear();
                cboChiNhanh.Items.Add(new ComboboxItem { Text = "-- Chọn chi nhánh --", Value = 0 });

                foreach (DataRow row in dtChiNhanh.Rows)
                {
                    cboChiNhanh.Items.Add(new ComboboxItem
                    {
                        Text = row["TenChiNhanh"].ToString(),
                        Value = Convert.ToInt32(row["MaChiNhanh"])
                    });
                }

                // FIX: Auto-select chi nhánh đầu tiên để hiện data ngay thay vì chọn placeholder
                if (cboChiNhanh.Items.Count > 1)
                {
                    cboChiNhanh.SelectedIndex = 1; // Trigger CboChiNhanh_SelectedIndexChanged để load data
                }
                else
                {
                    cboChiNhanh.SelectedIndex = 0;
                }

                // Hiển thị thông báo hướng dẫn
                if (dtChiNhanh.Rows.Count == 0)
                {
                    MessageBox.Show("Chưa có chi nhánh nào trong hệ thống.\nVui lòng thêm chi nhánh trước khi quản lý kho.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách chi nhánh: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xử lý khi Admin chọn chi nhánh khác
        /// </summary>
        private void CboChiNhanh_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboChiNhanh.SelectedItem is ComboboxItem item && (int)item.Value > 0)
            {
                _maChiNhanh = (int)item.Value;
                System.Diagnostics.Debug.WriteLine($"Admin selected branch: MaChiNhanh={_maChiNhanh}");

                TaiDuLieuKhoiDau();
                UpdateKPIs();
                UpdateLowStockWarning();
            }
            else
            {
                // Clear data khi chưa chọn chi nhánh
                _maChiNhanh = 0;
                if (dgvTonKho != null)
                {
                    dgvTonKho.DataSource = null;
                }
                // Reset KPI
                if (lblKPITongSP != null) lblKPITongSP.Text = "0";
                if (lblKPITongTon != null) lblKPITongTon.Text = "0";
                if (lblKPIGiaTri != null) lblKPIGiaTri.Text = "0đ";
                if (lblKPISapHet != null) lblKPISapHet.Text = "0";
            }
        }

        /// <summary>
        /// Tải dữ liệu tồn kho hiện tại từ database
        /// Lấy thông tin: Mã SP, Tên SP, Loại, Số lượng tồn, Giá, Đơn vị
        /// </summary>
        private void TaiTonKho()
        {
            // Query directly from SanPham table (TonKho table does not exist in schema)
            string query = @"
                SELECT 
                    MaSanPham,
                    TenSanPham,
                    LoaiSanPham,
                    GiaBan,
                    DonVi,
                    SoLuongTon,
                    SoLuongTon AS SoLuongKhaDung,
                    0 AS SoLuongChoXuat,
                    NgayTao AS NgayCapNhat
                FROM SanPham
                WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                ORDER BY LoaiSanPham, TenSanPham";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtTonKho = new DataTable();
                        da.Fill(_dtTonKho);

                        dgvTonKho.DataSource = _dtTonKho;
                        DinhDangDataGridViewTonKho();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải tồn kho: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tải danh sách sản phẩm từ database
        /// Sử dụng cho: Combobox lọc, thêm chi tiết phiếu
        /// </summary>
        private void TaiDanhSachSanPham()
        {
            string query = @"
                SELECT MaSanPham, TenSanPham, LoaiSanPham, GiaBan, DonVi
                FROM SanPham 
                WHERE MaChiNhanh = @MaChiNhanh
                ORDER BY TenSanPham";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtSanPham = new DataTable();
                        da.Fill(_dtSanPham);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách sản phẩm: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiNhapKho()
        {
            try
            {
                // FIX: Gọi DAL để lấy dữ liệu thực thay vì tạo DataTable trống
                if (_maChiNhanh <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("TaiNhapKho: Chưa chọn chi nhánh");
                    return;
                }

                DateTime tuNgay = dtpTuNgay?.Value.Date ?? DateTime.Today.AddMonths(-1);
                DateTime denNgay = dtpDenNgay?.Value.Date ?? DateTime.Today;

                // Check if NhapKho table exists first
                if (!_khoDAL.CheckNhapKhoTableExists())
                {
                    // Table doesn't exist - show empty with message
                    _dtNhapKho = new DataTable();
                    _dtNhapKho.Columns.Add("MaNhapKho", typeof(int));
                    _dtNhapKho.Columns.Add("NgayNhap", typeof(DateTime));
                    _dtNhapKho.Columns.Add("TongTien", typeof(decimal));
                    _dtNhapKho.Columns.Add("NhaCungCap", typeof(string));
                    _dtNhapKho.Columns.Add("GhiChu", typeof(string));
                    _dtNhapKho.Columns.Add("NguoiNhap", typeof(string));
                    _dtNhapKho.Columns.Add("SoLoaiSP", typeof(int));
                    dgvNhapKho.DataSource = _dtNhapKho;
                    System.Diagnostics.Debug.WriteLine("TaiNhapKho: Bảng NhapKho chưa được tạo");
                    return;
                }

                _dtNhapKho = _khoDAL.GetPhieuNhapKho(_maChiNhanh, tuNgay, denNgay);
                dgvNhapKho.DataSource = _dtNhapKho;
                DinhDangDataGridViewNhapKho();

                System.Diagnostics.Debug.WriteLine($"TaiNhapKho: Loaded {_dtNhapKho.Rows.Count} phiếu nhập");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TaiNhapKho error: {ex.Message}");
                // Fallback to empty DataTable on error
                _dtNhapKho = new DataTable();
                dgvNhapKho.DataSource = _dtNhapKho;
            }
        }

        private void TaiXuatKho()
        {
            try
            {
                // FIX: Gọi DAL để lấy dữ liệu thực thay vì tạo DataTable trống
                if (_maChiNhanh <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("TaiXuatKho: Chưa chọn chi nhánh");
                    return;
                }

                // Get filter from combobox if available
                string trangThai = null;
                if (cboTrangThaiXuat?.SelectedItem is string selectedStatus && selectedStatus != "Tất cả")
                {
                    trangThai = selectedStatus;
                }

                // Check if XuatKho table exists first
                if (!_khoDAL.CheckXuatKhoTableExists())
                {
                    // Table doesn't exist - show empty with message  
                    _dtXuatKho = new DataTable();
                    _dtXuatKho.Columns.Add("MaXuatKho", typeof(int));
                    _dtXuatKho.Columns.Add("NgayXuat", typeof(DateTime));
                    _dtXuatKho.Columns.Add("ChiNhanhXuat", typeof(string));
                    _dtXuatKho.Columns.Add("ChiNhanhNhan", typeof(string));
                    _dtXuatKho.Columns.Add("TongTien", typeof(decimal));
                    _dtXuatKho.Columns.Add("TrangThai", typeof(string));
                    _dtXuatKho.Columns.Add("NguoiXuat", typeof(string));
                    dgvXuatKho.DataSource = _dtXuatKho;
                    System.Diagnostics.Debug.WriteLine("TaiXuatKho: Bảng XuatKho chưa được tạo");
                    return;
                }

                _dtXuatKho = _khoDAL.GetPhieuXuatKho(_maChiNhanh, trangThai);
                dgvXuatKho.DataSource = _dtXuatKho;
                DinhDangDataGridViewXuatKho();

                System.Diagnostics.Debug.WriteLine($"TaiXuatKho: Loaded {_dtXuatKho.Rows.Count} phiếu xuất");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TaiXuatKho error: {ex.Message}");
                // Fallback to empty DataTable on error
                _dtXuatKho = new DataTable();
                dgvXuatKho.DataSource = _dtXuatKho;
            }
        }

        private void TaiBaoCao()
        {
            // Simplified report using only SanPham table (NhapKho, XuatKho tables don't exist)
            string query = @"
                SELECT N'Tổng giá trị tồn kho' AS ChiTieu, 
                       SUM(GiaBan * SoLuongTon) AS GiaTri,
                       N'Tính theo giá bán hiện tại' AS GhiChu
                FROM SanPham
                WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                
                UNION ALL
                
                SELECT N'Số loại sản phẩm', 
                       COUNT(*),
                       N'Đang có trong kho'
                FROM SanPham 
                WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                
                UNION ALL
                
                SELECT N'Tổng số lượng tồn',
                       SUM(SoLuongTon),
                       N'Tính theo đơn vị sản phẩm'
                FROM SanPham 
                WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                
                UNION ALL
                
                SELECT N'Sản phẩm sắp hết',
                       COUNT(*),
                       N'Số lượng <= 10'
                FROM SanPham 
                WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1 AND SoLuongTon <= 10";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvBaoCao.DataSource = dt;
                        DinhDangDataGridViewBaoCao();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DinhDangDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 123, 255);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        }

        private void DinhDangDataGridViewTonKho()
        {
            if (dgvTonKho.Columns.Count == 0) return;

            // Ẩn cột ID
            if (dgvTonKho.Columns.Contains("MaSanPham"))
                dgvTonKho.Columns["MaSanPham"].Visible = false;

            // Đổi tên cột
            Dictionary<string, string> columnNames = new Dictionary<string, string>
            {
                { "TenSanPham", "TÊN SẢN PHẨM" },
                { "LoaiSanPham", "LOẠI" },
                { "GiaBan", "GIÁ BÁN" },
                { "DonVi", "ĐƠN VỊ" },
                { "SoLuongTon", "TỒN KHO" },
                { "SoLuongKhaDung", "KHẢ DỤNG" },
                { "SoLuongChoXuat", "CHỜ XUẤT" },
                { "NgayCapNhat", "NGÀY CẬP NHẬT" }
            };

            foreach (var pair in columnNames)
            {
                if (dgvTonKho.Columns.Contains(pair.Key))
                {
                    dgvTonKho.Columns[pair.Key].HeaderText = pair.Value;
                    if (pair.Key == "GiaBan")
                        dgvTonKho.Columns[pair.Key].DefaultCellStyle.Format = "N0";
                }
            }
        }

        private void DinhDangDataGridViewNhapKho()
        {
            if (dgvNhapKho.Columns.Count == 0) return;

            // Đổi tên cột
            Dictionary<string, string> columnNames = new Dictionary<string, string>
            {
                { "MaNhapKho", "MÃ PHIẾU" },
                { "NgayNhap", "NGÀY NHẬP" },
                { "TongTien", "TỔNG TIỀN" },
                { "NhaCungCap", "NHÀ CUNG CẤP" },
                { "HoaDonNhap", "HÓA ĐƠN" },
                { "GhiChu", "GHI CHÚ" },
                { "NguoiNhap", "NGƯỜI NHẬP" },
                { "SoLoaiSP", "SỐ LOẠI SP" }
            };

            foreach (var pair in columnNames)
            {
                if (dgvNhapKho.Columns.Contains(pair.Key))
                {
                    dgvNhapKho.Columns[pair.Key].HeaderText = pair.Value;
                    if (pair.Key == "TongTien")
                        dgvNhapKho.Columns[pair.Key].DefaultCellStyle.Format = "N0";
                    if (pair.Key == "NgayNhap")
                        dgvNhapKho.Columns[pair.Key].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
            }
        }

        private void DinhDangDataGridViewXuatKho()
        {
            if (dgvXuatKho.Columns.Count == 0) return;

            // Thêm cột hành động
            if (!dgvXuatKho.Columns.Contains("Action"))
            {
                DataGridViewButtonColumn btnAction = new DataGridViewButtonColumn
                {
                    Name = "Action",
                    HeaderText = "THAO TÁC",
                    Text = "Xác nhận",
                    UseColumnTextForButtonValue = true,
                    Width = 100
                };
                dgvXuatKho.Columns.Add(btnAction);
            }

            // Đổi tên cột
            Dictionary<string, string> columnNames = new Dictionary<string, string>
            {
                { "MaXuatKho", "MÃ PHIẾU" },
                { "NgayXuat", "NGÀY XUẤT" },
                { "ChiNhanhXuat", "CHI NHÁNH XUẤT" },
                { "ChiNhanhNhan", "CHI NHÁNH NHẬN" },
                { "TongTien", "TỔNG TIỀN" },
                { "LyDoXuat", "LÝ DO XUẤT" },
                { "TrangThai", "TRẠNG THÁI" },
                { "NguoiXuat", "NGƯỜI XUẤT" },
                { "NguoiXacNhan", "NGƯỜI XÁC NHẬN" }
            };

            foreach (var pair in columnNames)
            {
                if (dgvXuatKho.Columns.Contains(pair.Key))
                {
                    dgvXuatKho.Columns[pair.Key].HeaderText = pair.Value;
                    if (pair.Key == "TongTien")
                        dgvXuatKho.Columns[pair.Key].DefaultCellStyle.Format = "N0";
                    if (pair.Key == "NgayXuat")
                        dgvXuatKho.Columns[pair.Key].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
            }
        }

        private void DinhDangDataGridViewBaoCao()
        {
            if (dgvBaoCao.Columns.Count == 0) return;

            dgvBaoCao.Columns["ChiTieu"].Width = 300;
            dgvBaoCao.Columns["GiaTri"].Width = 150;
            dgvBaoCao.Columns["GhiChu"].Width = 250;

            if (dgvBaoCao.Columns.Contains("GiaTri"))
                dgvBaoCao.Columns["GiaTri"].DefaultCellStyle.Format = "N0";
        }

        private void DgvTonKho_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataGridView dgv = (DataGridView)sender;

            // Get SoLuongKhaDung value for this row
            if (dgv.Columns.Contains("SoLuongKhaDung"))
            {
                int soLuongIndex = dgv.Columns["SoLuongKhaDung"].Index;
                var cellValue = dgv.Rows[e.RowIndex].Cells[soLuongIndex].Value;
                
                if (cellValue != null && int.TryParse(cellValue.ToString(), out int soLuong))
                {
                    // Highlight entire row based on stock level
                    if (soLuong <= 10)
                    {
                        // Critical low stock - red background
                        e.CellStyle.BackColor = Color.FromArgb(255, 235, 238);
                        e.CellStyle.ForeColor = Color.FromArgb(183, 28, 28);
                        
                        // Bold for the quantity column
                        if (dgv.Columns[e.ColumnIndex].Name == "SoLuongKhaDung")
                        {
                            e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                        }
                    }
                    else if (soLuong <= 20)
                    {
                        // Warning - orange/yellow background
                        e.CellStyle.BackColor = Color.FromArgb(255, 248, 225);
                        e.CellStyle.ForeColor = Color.FromArgb(230, 126, 34);
                    }
                    // Normal stock keeps default styling
                }
            }
        }

        private void TaoPhieuMoi(bool isNhapKho)
        {
            _isNhapKho = isNhapKho;
            _maPhieuHienTai = 0;

            // Tạo DataTable tạm
            _dtChiTietTam = new DataTable();
            _dtChiTietTam.Columns.Add("MaSanPham", typeof(int));
            _dtChiTietTam.Columns.Add("TenSanPham", typeof(string));
            _dtChiTietTam.Columns.Add("SoLuong", typeof(int));
            _dtChiTietTam.Columns.Add("DonGia", typeof(decimal));
            _dtChiTietTam.Columns.Add("ThanhTien", typeof(decimal));

            // Cập nhật giao diện
            Label lblTitle = (Label)pnlChiTiet.Controls[0];
            lblTitle.Text = isNhapKho ? "📝 PHIẾU NHẬP KHO" : "📝 PHIẾU XUẤT KHO";

            btnLuuPhieu.Text = isNhapKho ? "💾 LƯU PHIẾU NHẬP" : "💾 LƯU PHIẾU XUẤT";
            btnInPhieu.Visible = false;

            dgvChiTiet.DataSource = _dtChiTietTam;
            CapNhatTongTien();

            pnlChiTiet.Visible = true;
        }

        private void ThemSanPhamVaoPhieu()
        {
            if (dgvTonKho.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm từ bảng tồn kho!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataGridViewRow row = dgvTonKho.SelectedRows[0];
            int maSP = Convert.ToInt32(row.Cells["MaSanPham"].Value);
            string tenSP = row.Cells["TenSanPham"].Value.ToString();
            int tonKho = Convert.ToInt32(row.Cells["SoLuongTon"].Value);

            // Kiểm tra nếu sản phẩm đã có trong phiếu
            foreach (DataRow dr in _dtChiTietTam.Rows)
            {
                if (Convert.ToInt32(dr["MaSanPham"]) == maSP)
                {
                    MessageBox.Show("Sản phẩm này đã có trong phiếu!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Form nhập số lượng
            using (var frm = new FormNhapSoLuong(tenSP, tonKho, _isNhapKho))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    int soLuong = frm.SoLuong;
                    decimal donGia = frm.DonGia;

                    DataRow newRow = _dtChiTietTam.NewRow();
                    newRow["MaSanPham"] = maSP;
                    newRow["TenSanPham"] = tenSP;
                    newRow["SoLuong"] = soLuong;
                    newRow["DonGia"] = donGia;
                    newRow["ThanhTien"] = soLuong * donGia;
                    _dtChiTietTam.Rows.Add(newRow);

                    CapNhatTongTien();
                }
            }
        }

        private void CapNhatTongTien()
        {
            int soSP = _dtChiTietTam.Rows.Count;
            decimal tongTien = 0;

            foreach (DataRow row in _dtChiTietTam.Rows)
            {
                tongTien += Convert.ToDecimal(row["ThanhTien"]);
            }

            lblSoLuongSP.Text = $"Số sản phẩm: {soSP}";
            lblTongTien.Text = $"Tổng tiền: {tongTien:N0} đ";
        }

        private void LuuPhieuNhapKho()
        {
            if (_dtChiTietTam == null || _dtChiTietTam.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm sản phẩm vào phiếu!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Simplified: Only update SoLuongTon in SanPham table
            // (NhapKho, ChiTietNhapKho, TonKho tables don't exist in current schema)
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        foreach (DataRow row in _dtChiTietTam.Rows)
                        {
                            // Directly update SoLuongTon in SanPham table
                            string queryUpdateTon = @"
                                UPDATE SanPham 
                                SET SoLuongTon = SoLuongTon + @SoLuong
                                WHERE MaSanPham = @MaSanPham 
                                  AND MaChiNhanh = @MaChiNhanh";

                            using (SqlCommand cmdTon = new SqlCommand(queryUpdateTon, conn, transaction))
                            {
                                cmdTon.Parameters.AddWithValue("@SoLuong", row["SoLuong"]);
                                cmdTon.Parameters.AddWithValue("@MaSanPham", row["MaSanPham"]);
                                cmdTon.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                                cmdTon.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("Cập nhật tồn kho thành công!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh data
                        TaiTonKho();
                        UpdateLowStockWarning();
                        pnlChiTiet.Visible = false;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Lỗi cập nhật tồn kho: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetTongTien()
        {
            decimal tong = 0;
            foreach (DataRow row in _dtChiTietTam.Rows)
            {
                tong += Convert.ToDecimal(row["ThanhTien"]);
            }
            return tong;
        }

        // ==================== SỰ KIỆN ====================

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabMain.SelectedIndex)
            {
                case 0: // Tồn kho
                    TaiTonKho();
                    break;
                case 1: // Nhập kho
                    TaiNhapKho();
                    break;
                case 2: // Xuất kho
                    TaiXuatKho();
                    break;
                case 3: // Báo cáo
                    TaiBaoCao();
                    break;
            }
        }

        private void TxtTimKiemSP_TextChanged(object sender, EventArgs e)
        {
            if (_dtTonKho == null) return;

            string filter = txtTimKiemSP.Text.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                _dtTonKho.DefaultView.RowFilter = "";
            }
            else
            {
                _dtTonKho.DefaultView.RowFilter = string.Format(
                    "TenSanPham LIKE '%{0}%'",
                    filter.Replace("'", "''"));
            }
        }

        private void CboLoaiSPFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dtTonKho == null) return;

            string filter = cboLoaiSPFilter.SelectedItem.ToString();
            if (filter == "Tất cả")
            {
                _dtTonKho.DefaultView.RowFilter = "";
            }
            else
            {
                _dtTonKho.DefaultView.RowFilter = $"LoaiSanPham = '{filter}'";
            }
        }

        private void BtnCapNhatTon_Click(object sender, EventArgs e)
        {
            TaiTonKho();
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dtTonKho == null || _dtTonKho.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"TonKho_{DateTime.Now:yyyyMMdd_HHmmss}";
                    sfd.Title = "Xuất báo cáo tồn kho";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = sfd.FileName;
                        
                        if (filePath.EndsWith(".csv"))
                        {
                            // Export to CSV using ExportHelper
                            QuanLiChuoiRapPhim.BLL.ExportHelper.ExportToCsv(_dtTonKho, filePath);
                        }
                        else
                        {
                            // Export as CSV with xlsx extension (basic Excel compatibility)
                            QuanLiChuoiRapPhim.BLL.ExportHelper.ExportToCsv(_dtTonKho, filePath);
                        }

                        MessageBox.Show($"Xuất file thành công!\n{filePath}", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Open containing folder
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(DataTable dt, string filePath)
        {
            var sb = new System.Text.StringBuilder();
            
            // Header
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));
            
            // Rows
            foreach (DataRow row in dt.Rows)
            {
                var fields = row.ItemArray.Select(f => 
                    f?.ToString()?.Contains(",") == true ? $"\"{f}\"" : f?.ToString() ?? "");
                sb.AppendLine(string.Join(",", fields));
            }
            
            System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
        }

        private void BtnNhapKho_Click(object sender, EventArgs e)
        {
            TaoPhieuMoi(true);
        }

        private void BtnXuatKho_Click(object sender, EventArgs e)
        {
            if (dgvTonKho.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để xuất kho!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TaoPhieuMoi(false);
        }

        private void BtnXemChiTiet_Click(object sender, EventArgs e)
        {
            if (dgvNhapKho.SelectedRows.Count == 0) return;

            int maPhieu = Convert.ToInt32(dgvNhapKho.SelectedRows[0].Cells["MaNhapKho"].Value);
            HienChiTietPhieu(maPhieu, true);
        }

        private void DtpTuNgay_ValueChanged(object sender, EventArgs e)
        {
            TaiNhapKho();
        }

        private void DtpDenNgay_ValueChanged(object sender, EventArgs e)
        {
            TaiNhapKho();
        }

        private void CboTrangThaiXuat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dtXuatKho == null) return;

            string filter = cboTrangThaiXuat.SelectedItem.ToString();
            if (filter == "Tất cả")
            {
                _dtXuatKho.DefaultView.RowFilter = "";
            }
            else
            {
                _dtXuatKho.DefaultView.RowFilter = $"TrangThai = '{filter}'";
            }
        }

        private void DgvXuatKho_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgvXuatKho.Columns[e.ColumnIndex].Name == "Action")
            {
                int maXuatKho = Convert.ToInt32(dgvXuatKho.Rows[e.RowIndex].Cells["MaXuatKho"].Value);
                string trangThai = dgvXuatKho.Rows[e.RowIndex].Cells["TrangThai"].Value.ToString();

                if (trangThai == "ChoXacNhan")
                {
                    // Xác nhận đã nhận hàng
                    XacNhanNhanHang(maXuatKho);
                }
            }
        }

        private void BtnLuuPhieu_Click(object sender, EventArgs e)
        {
            LuuPhieuNhapKho();
        }

        private void BtnInPhieu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tính năng in phiếu đang phát triển!", "Thông báo");
        }

        private void BtnHuyPhieu_Click(object sender, EventArgs e)
        {
            pnlChiTiet.Visible = false;
        }

        private void DgvChiTiet_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            DataGridView dgv = (DataGridView)sender;
            if (dgv.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                _dtChiTietTam.Rows.RemoveAt(e.RowIndex);
                CapNhatTongTien();
            }
        }

        // ==================== CÁC PHƯƠNG THỨC HỖ TRỢ ====================

        private void HienChiTietPhieu(int maPhieu, bool isNhapKho)
        {
            // Tables ChiTietNhapKho, ChiTietXuatKho don't exist in current schema
            MessageBox.Show("Chức năng xem chi tiết phiếu chưa được hỗ trợ.\n\nDatabase hiện tại chưa có bảng lịch sử nhập/xuất kho.", 
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void XacNhanNhanHang(int maXuatKho)
        {
            if (MessageBox.Show("Xác nhận đã nhận hàng từ phiếu xuất này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                    {
                        conn.Open();
                        string query = @"
                            UPDATE XuatKho 
                            SET TrangThai = N'DaNhan',
                                NguoiXacNhan = @NguoiXacNhan,
                                NgayXacNhan = GETDATE()
                            WHERE MaXuatKho = @MaXuatKho
                              AND MaChiNhanhNhan = @MaChiNhanh";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@NguoiXacNhan", _maNguoiDung);
                            cmd.Parameters.AddWithValue("@MaXuatKho", maXuatKho);
                            cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Đã xác nhận nhận hàng!", "Thành công",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Cập nhật tồn kho
                                CapNhatTonKhoSauKhiNhan(maXuatKho);
                                TaiXuatKho();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xác nhận nhận hàng: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CapNhatTonKhoSauKhiNhan(int maXuatKho)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();

                    // Lấy chi tiết xuất kho
                    string queryChiTiet = @"
                        SELECT MaSanPham, SoLuongXuat 
                        FROM ChiTietXuatKho 
                        WHERE MaXuatKho = @MaXuatKho";

                    using (SqlCommand cmd = new SqlCommand(queryChiTiet, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaXuatKho", maXuatKho);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int maSP = reader.GetInt32(0);
                                int soLuong = reader.GetInt32(1);

                                // Cập nhật tồn kho
                                string queryUpdate = @"
                                    UPDATE TonKho 
                                    SET SoLuongTon = SoLuongTon + @SoLuong,
                                        SoLuongKhaDung = SoLuongKhaDung + @SoLuong,
                                        NgayCapNhat = GETDATE()
                                    WHERE MaSanPham = @MaSanPham 
                                      AND MaChiNhanh = @MaChiNhanh";

                                using (SqlCommand cmdUpdate = new SqlCommand(queryUpdate, conn))
                                {
                                    cmdUpdate.Parameters.AddWithValue("@SoLuong", soLuong);
                                    cmdUpdate.Parameters.AddWithValue("@MaSanPham", maSP);
                                    cmdUpdate.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                                    cmdUpdate.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật tồn kho: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== FORM HỖ TRỢ ====================

        private class FormNhapSoLuong : Form
        {
            public int SoLuong { get; private set; }
            public decimal DonGia { get; private set; }

            private NumericUpDown numSoLuong;
            private NumericUpDown numDonGia;
            private Button btnOK, btnCancel;

            public FormNhapSoLuong(string tenSP, int tonKho, bool isNhapKho)
            {
                InitializeComponent(tenSP, tonKho, isNhapKho);
            }

            private void InitializeComponent(string tenSP, int tonKho, bool isNhapKho)
            {
                this.Text = isNhapKho ? "NHẬP SỐ LƯỢNG" : "XUẤT SỐ LƯỢNG";
                this.Size = new Size(400, 250);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;

                Label lblTitle = new Label
                {
                    Text = $"Sản phẩm: {tenSP}",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(20, 20),
                    Size = new Size(350, 30)
                };

                Label lblTonKho = new Label
                {
                    Text = $"Tồn kho hiện tại: {tonKho}",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, 50),
                    Size = new Size(350, 25)
                };

                Label lblSoLuong = new Label
                {
                    Text = "Số lượng:",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, 85),
                    Size = new Size(100, 25)
                };

                numSoLuong = new NumericUpDown
                {
                    Minimum = 1,
                    Maximum = isNhapKho ? 10000 : tonKho,
                    Value = 1,
                    Location = new Point(130, 85),
                    Size = new Size(150, 25),
                    Font = new Font("Segoe UI", 10)
                };

                Label lblDonGia = new Label
                {
                    Text = isNhapKho ? "Giá nhập:" : "Giá xuất:",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, 120),
                    Size = new Size(100, 25)
                };

                numDonGia = new NumericUpDown
                {
                    Minimum = 1000,
                    Maximum = 1000000,
                    Value = isNhapKho ? 10000 : 15000,
                    Increment = 1000,
                    Location = new Point(130, 120),
                    Size = new Size(150, 25),
                    Font = new Font("Segoe UI", 10)
                };

                btnOK = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Size = new Size(100, 35),
                    Location = new Point(80, 160),
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White
                };
                btnOK.Click += (s, e) => { SoLuong = (int)numSoLuong.Value; DonGia = numDonGia.Value; };

                btnCancel = new Button
                {
                    Text = "Hủy",
                    DialogResult = DialogResult.Cancel,
                    Size = new Size(100, 35),
                    Location = new Point(200, 160),
                    BackColor = Color.FromArgb(108, 117, 125),
                    ForeColor = Color.White
                };

                this.Controls.AddRange(new Control[]
                {
                    lblTitle, lblTonKho,
                    lblSoLuong, numSoLuong,
                    lblDonGia, numDonGia,
                    btnOK, btnCancel
                });
            }
        }

        private class FormThongTinPhieu : Form
        {
            public string NhaCungCap { get; private set; }
            public string HoaDon { get; private set; }
            public string GhiChu { get; private set; }

            private TextBox txtNhaCungCap, txtHoaDon, txtGhiChu;
            private Button btnOK, btnCancel;

            public FormThongTinPhieu(bool isNhapKho)
            {
                InitializeComponent(isNhapKho);
            }

            private void InitializeComponent(bool isNhapKho)
            {
                this.Text = isNhapKho ? "THÔNG TIN PHIẾU NHẬP" : "THÔNG TIN PHIẾU XUẤT";
                this.Size = new Size(500, 350);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;

                int yPos = 20;

                Label lblTitle = new Label
                {
                    Text = isNhapKho ? "THÔNG TIN NHÀ CUNG CẤP" : "THÔNG TIN XUẤT KHO",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, yPos),
                    Size = new Size(400, 30)
                };
                yPos += 40;

                if (isNhapKho)
                {
                    Label lblNCC = new Label
                    {
                        Text = "Nhà cung cấp:",
                        Font = new Font("Segoe UI", 10),
                        Location = new Point(20, yPos),
                        Size = new Size(120, 25)
                    };

                    txtNhaCungCap = new TextBox
                    {
                        Location = new Point(150, yPos),
                        Size = new Size(300, 25),
                        Font = new Font("Segoe UI", 10)
                    };
                    yPos += 35;

                    Label lblHD = new Label
                    {
                        Text = "Số hóa đơn:",
                        Font = new Font("Segoe UI", 10),
                        Location = new Point(20, yPos),
                        Size = new Size(120, 25)
                    };

                    txtHoaDon = new TextBox
                    {
                        Location = new Point(150, yPos),
                        Size = new Size(200, 25),
                        Font = new Font("Segoe UI", 10)
                    };
                    yPos += 35;

                    // Add controls inside the if block where they're in scope
                    this.Controls.Add(lblNCC);
                    this.Controls.Add(txtNhaCungCap);
                    this.Controls.Add(lblHD);
                    this.Controls.Add(txtHoaDon);
                }
                else
                {
                    // Form xuất kho có thể thêm các field khác
                }

                Label lblGhiChu = new Label
                {
                    Text = "Ghi chú:",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, yPos),
                    Size = new Size(120, 25)
                };

                txtGhiChu = new TextBox
                {
                    Location = new Point(150, yPos),
                    Size = new Size(300, 80),
                    Multiline = true,
                    Font = new Font("Segoe UI", 10)
                };
                yPos += 100;

                btnOK = new Button
                {
                    Text = "LƯU",
                    DialogResult = DialogResult.OK,
                    Size = new Size(120, 35),
                    Location = new Point(100, yPos),
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White
                };
                btnOK.Click += (s, e) =>
                {
                    NhaCungCap = isNhapKho ? txtNhaCungCap.Text : "";
                    HoaDon = isNhapKho ? txtHoaDon.Text : "";
                    GhiChu = txtGhiChu.Text;
                };

                btnCancel = new Button
                {
                    Text = "HỦY",
                    DialogResult = DialogResult.Cancel,
                    Size = new Size(120, 35),
                    Location = new Point(250, yPos),
                    BackColor = Color.FromArgb(108, 117, 125),
                    ForeColor = Color.White
                };

                this.Controls.Add(lblTitle);
                this.Controls.AddRange(new Control[] { lblGhiChu, txtGhiChu, btnOK, btnCancel });
            }
        }

        /// <summary>
        /// Form thêm/sửa sản phẩm
        /// </summary>
        private class FormSanPham : Form
        {
            public string TenSanPham { get; private set; }
            public string LoaiSanPham { get; private set; }
            public decimal GiaBan { get; private set; }
            public string DonVi { get; private set; }
            public int SoLuong { get; private set; }

            private TextBox txtTen, txtDonVi;
            private NumericUpDown numGia, numSoLuong;
            private ComboBox cboLoai;
            private Button btnOK, btnCancel;

            public FormSanPham(int maSP, string tenSP, string loaiSP, decimal giaBan, string donVi, int soLuong)
            {
                this.Text = maSP == 0 ? "THÊM SẢN PHẨM MỚI" : "CẬP NHẬT SẢN PHẨM";
                this.Size = new Size(450, 380);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                int yPos = 20;
                int lblWidth = 120;
                int ctrlX = 140;
                int ctrlWidth = 270;

                // Title
                Label lblTitle = new Label
                {
                    Text = maSP == 0 ? "📦 THÊM SẢN PHẨM MỚI" : "✏️ CẬP NHẬT SẢN PHẨM",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(20, yPos),
                    AutoSize = true
                };
                yPos += 45;

                // Tên sản phẩm
                Label lblTen = new Label
                {
                    Text = "Tên sản phẩm:",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, yPos + 3),
                    Size = new Size(lblWidth, 25)
                };
                txtTen = new TextBox
                {
                    Text = tenSP,
                    Location = new Point(ctrlX, yPos),
                    Size = new Size(ctrlWidth, 28),
                    Font = new Font("Segoe UI", 10)
                };
                yPos += 38;

                // Loại sản phẩm
                Label lblLoai = new Label
                {
                    Text = "Loại sản phẩm:",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, yPos + 3),
                    Size = new Size(lblWidth, 25)
                };
                cboLoai = new ComboBox
                {
                    Location = new Point(ctrlX, yPos),
                    Size = new Size(ctrlWidth, 28),
                    Font = new Font("Segoe UI", 10),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboLoai.Items.AddRange(new object[] { "Bap", "Nuoc", "Combo", "DoAn", "Khac" });
                cboLoai.SelectedItem = loaiSP;
                if (cboLoai.SelectedIndex < 0) cboLoai.SelectedIndex = 0;
                yPos += 38;

                // Giá bán
                Label lblGia = new Label
                {
                    Text = "Giá bán (VNĐ):",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, yPos + 3),
                    Size = new Size(lblWidth, 25)
                };
                numGia = new NumericUpDown
                {
                    Location = new Point(ctrlX, yPos),
                    Size = new Size(ctrlWidth, 28),
                    Font = new Font("Segoe UI", 10),
                    Maximum = 999999999,
                    Minimum = 0,
                    DecimalPlaces = 0,
                    ThousandsSeparator = true,
                    Value = giaBan
                };
                yPos += 38;

                // Đơn vị
                Label lblDonVi = new Label
                {
                    Text = "Đơn vị:",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, yPos + 3),
                    Size = new Size(lblWidth, 25)
                };
                txtDonVi = new TextBox
                {
                    Text = donVi,
                    Location = new Point(ctrlX, yPos),
                    Size = new Size(ctrlWidth, 28),
                    Font = new Font("Segoe UI", 10)
                };
                yPos += 38;

                // Số lượng tồn
                Label lblSL = new Label
                {
                    Text = "Số lượng tồn:",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, yPos + 3),
                    Size = new Size(lblWidth, 25)
                };
                numSoLuong = new NumericUpDown
                {
                    Location = new Point(ctrlX, yPos),
                    Size = new Size(ctrlWidth, 28),
                    Font = new Font("Segoe UI", 10),
                    Maximum = 999999,
                    Minimum = 0,
                    Value = soLuong
                };
                yPos += 50;

                // Buttons
                btnOK = new Button
                {
                    Text = maSP == 0 ? "➕ THÊM" : "💾 LƯU",
                    DialogResult = DialogResult.OK,
                    Size = new Size(120, 38),
                    Location = new Point(ctrlX, yPos),
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };
                btnOK.Click += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtTen.Text))
                    {
                        MessageBox.Show("Vui lòng nhập tên sản phẩm!", "Thông báo", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                    TenSanPham = txtTen.Text.Trim();
                    LoaiSanPham = cboLoai.SelectedItem?.ToString() ?? "Bap";
                    GiaBan = numGia.Value;
                    DonVi = txtDonVi.Text.Trim();
                    SoLuong = (int)numSoLuong.Value;
                };

                btnCancel = new Button
                {
                    Text = "❌ HỦY",
                    DialogResult = DialogResult.Cancel,
                    Size = new Size(120, 38),
                    Location = new Point(ctrlX + 140, yPos),
                    BackColor = Color.FromArgb(108, 117, 125),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };

                this.Controls.AddRange(new Control[] { 
                    lblTitle, 
                    lblTen, txtTen, 
                    lblLoai, cboLoai, 
                    lblGia, numGia, 
                    lblDonVi, txtDonVi, 
                    lblSL, numSoLuong, 
                    btnOK, btnCancel 
                });

                this.AcceptButton = btnOK;
                this.CancelButton = btnCancel;
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