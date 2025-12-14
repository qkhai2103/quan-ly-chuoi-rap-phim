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
        private readonly int _maChiNhanh;           // Chi nhánh của Manager
        private readonly int _maNguoiDung;          // ID của Manager hiện tại
        private DataTable _dtSanPham;               // Danh sách sản phẩm
        private DataTable _dtTonKho;                // Tồn kho hiện tại
        private DataTable _dtNhapKho;               // Lịch sử nhập
        private DataTable _dtXuatKho;               // Lịch sử xuất

        // Controls chính
        private TabControl tabMain;
        private TabPage tabTonKho, tabNhapKho, tabXuatKho, tabBaoCao;
        private DataGridView dgvTonKho, dgvNhapKho, dgvXuatKho, dgvBaoCao;
        private Button btnNhapKho, btnXuatKho, btnCapNhatTon, btnXemChiTiet;
        private ComboBox cboLoaiSPFilter, cboTrangThaiXuat;
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
        /// <param name="maChiNhanh">Mã chi nhánh của Manager</param>
        /// <param name="maNguoiDung">ID người dùng (Manager) đang đăng nhập</param>
        public UC_Kho(int maChiNhanh, int maNguoiDung)
        {
            _maChiNhanh = maChiNhanh;
            _maNguoiDung = maNguoiDung;

            ThietLapGiaoDien();
            TaiDuLieuKhoiDau();
        }

        /// <summary>
        /// Thiết lập giao diện chính của UserControl
        /// Tạo: Header tiêu đề, TabControl với 4 tab chính, Panel chi tiết phiếu
        /// </summary>
        private void ThietLapGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // === TIÊU ĐỀ ===
            Panel pnlTieuDe = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(255, 193, 7) // Vàng
            };

            Label lblTieuDe = new Label
            {
                Text = "📦 QUẢN LÝ KHO BẮP NƯỚC",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            pnlTieuDe.Controls.Add(lblTieuDe);

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
            this.Controls.Add(pnlTieuDe);
        }

        /// <summary>
        /// Thiết lập giao diện Tab "TỒN KHO"
        /// Hiển thị: DataGridView tồn kho, Combobox lọc loại, TextBox tìm kiếm, nút Làm mới/Xem chi tiết
        /// </summary>
        private void ThietLapTabTonKho()
        {
            tabTonKho.Padding = new Padding(10);

            // Panel công cụ
            Panel pnlCongCu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent
            };

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

            // Nút nhập kho
            btnNhapKho = new Button
            {
                Text = "⬇️ NHẬP KHO",
                Size = new Size(140, 35),
                Location = new Point(670, 8),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnNhapKho.Click += BtnNhapKho_Click;

            pnlCongCu.Controls.AddRange(new Control[]
            {
                lblTimKiem, txtTimKiemSP,
                lblLoaiSP, cboLoaiSPFilter,
                btnCapNhatTon, btnNhapKho
            });

            // DataGridView
            dgvTonKho = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                MultiSelect = false
            };
            DinhDangDataGridView(dgvTonKho);

            // Sự kiện
            dgvTonKho.SelectionChanged += (s, e) =>
            {
                btnXuatKho.Enabled = dgvTonKho.SelectedRows.Count > 0;
            };

            dgvTonKho.CellFormatting += DgvTonKho_CellFormatting;

            tabTonKho.Controls.Add(dgvTonKho);
            tabTonKho.Controls.Add(pnlCongCu);
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
        }

        /// <summary>
        /// Tải dữ liệu tồn kho hiện tại từ database
        /// Lấy thông tin: Mã SP, Tên SP, Loại, Số lượng tồn, Giá, Người cập nhật lần cuối
        /// </summary>
        private void TaiTonKho()
        {
            string query = @"
                SELECT 
                    tk.MaSanPham,
                    sp.TenSanPham,
                    sp.LoaiSanPham,
                    sp.GiaBan,
                    sp.DonVi,
                    tk.SoLuongTon,
                    tk.SoLuongKhaDung,
                    tk.SoLuongChoXuat,
                    tk.NgayCapNhat
                FROM TonKho tk
                INNER JOIN SanPham sp ON tk.MaSanPham = sp.MaSanPham
                WHERE tk.MaChiNhanh = @MaChiNhanh
                ORDER BY sp.LoaiSanPham, sp.TenSanPham";

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
            string query = @"
                -- Tải danh sách phiếu nhập kho với thông tin chi tiết
                SELECT 
                    nk.MaNhapKho,
                    nk.NgayNhap,
                    nk.TongTien,
                    nk.NhaCungCap,
                    nk.HoaDonNhap,
                    nk.GhiChu,
                    nd.HoTen AS NguoiNhap,
                    COUNT(ct.MaChiTietNhap) AS SoLoaiSP
                FROM NhapKho nk
                INNER JOIN NguoiDung nd ON nk.MaNguoiNhap = nd.MaNguoiDung
                LEFT JOIN ChiTietNhapKho ct ON nk.MaNhapKho = ct.MaNhapKho
                WHERE nk.MaChiNhanh = @MaChiNhanh
                  AND CAST(nk.NgayNhap AS DATE) BETWEEN @TuNgay AND @DenNgay
                GROUP BY nk.MaNhapKho, nk.NgayNhap, nk.TongTien, 
                         nk.NhaCungCap, nk.HoaDonNhap, nk.GhiChu, nd.HoTen
                ORDER BY nk.NgayNhap DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuNgay", dtpTuNgay.Value.Date);
                        cmd.Parameters.AddWithValue("@DenNgay", dtpDenNgay.Value.Date);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtNhapKho = new DataTable();
                        da.Fill(_dtNhapKho);

                        dgvNhapKho.DataSource = _dtNhapKho;
                        DinhDangDataGridViewNhapKho();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử nhập kho: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiXuatKho()
        {
            string query = @"
                SELECT 
                    xk.MaXuatKho,
                    xk.NgayXuat,
                    cnx.TenChiNhanh AS ChiNhanhXuat,
                    cnn.TenChiNhanh AS ChiNhanhNhan,
                    xk.TongTien,
                    xk.LyDoXuat,
                    xk.TrangThai,
                    ndx.HoTen AS NguoiXuat,
                    ndxn.HoTen AS NguoiXacNhan
                FROM XuatKho xk
                INNER JOIN ChiNhanh cnx ON xk.MaChiNhanhXuat = cnx.MaChiNhanh
                INNER JOIN ChiNhanh cnn ON xk.MaChiNhanhNhan = cnn.MaChiNhanh
                INNER JOIN NguoiDung ndx ON xk.MaNguoiXuat = ndx.MaNguoiDung
                LEFT JOIN NguoiDung ndxn ON xk.NguoiXacNhan = ndxn.MaNguoiDung
                WHERE xk.MaChiNhanhXuat = @MaChiNhanh 
                   OR xk.MaChiNhanhNhan = @MaChiNhanh
                ORDER BY xk.NgayXuat DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtXuatKho = new DataTable();
                        da.Fill(_dtXuatKho);

                        dgvXuatKho.DataSource = _dtXuatKho;
                        DinhDangDataGridViewXuatKho();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử xuất kho: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiBaoCao()
        {
            string query = @"
                -- Báo cáo tổng hợp
                SELECT 'Tổng giá trị tồn kho' AS ChiTieu, 
                       SUM(sp.GiaBan * tk.SoLuongTon) AS GiaTri,
                       'Tính theo giá bán hiện tại' AS GhiChu
                FROM TonKho tk
                INNER JOIN SanPham sp ON tk.MaSanPham = sp.MaSanPham
                WHERE tk.MaChiNhanh = @MaChiNhanh
                
                UNION ALL
                
                SELECT 'Số loại sản phẩm', 
                       COUNT(*),
                       'Đang có trong kho'
                FROM TonKho 
                WHERE MaChiNhanh = @MaChiNhanh
                
                UNION ALL
                
                SELECT 'Tổng nhập tháng này',
                       SUM(TongTien),
                       CONCAT('Từ ', FORMAT(DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0), 'dd/MM/yyyy'))
                FROM NhapKho 
                WHERE MaChiNhanh = @MaChiNhanh
                  AND MONTH(NgayNhap) = MONTH(GETDATE())
                  AND YEAR(NgayNhap) = YEAR(GETDATE())
                
                UNION ALL
                
                SELECT 'Tổng xuất tháng này',
                       SUM(TongTien),
                       'Xuất sang chi nhánh khác'
                FROM XuatKho 
                WHERE MaChiNhanhXuat = @MaChiNhanh
                  AND MONTH(NgayXuat) = MONTH(GETDATE())
                  AND YEAR(NgayXuat) = YEAR(GETDATE())
                  AND TrangThai != 'Huy'";

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

            // Đổi màu cột số lượng
            if (dgv.Columns[e.ColumnIndex].Name == "SoLuongKhaDung")
            {
                int soLuong = Convert.ToInt32(e.Value);
                if (soLuong <= 10)
                    e.CellStyle.ForeColor = Color.Red;
                else if (soLuong <= 20)
                    e.CellStyle.ForeColor = Color.Orange;
                else
                    e.CellStyle.ForeColor = Color.Green;
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
            if (_dtChiTietTam.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm sản phẩm vào phiếu!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var frm = new FormThongTinPhieu(_isNhapKho))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                        {
                            conn.Open();
                            SqlTransaction transaction = conn.BeginTransaction();

                            try
                            {
                                if (_isNhapKho)
                                {
                                    // Lưu phiếu nhập
                                    string queryNhap = @"
                                        INSERT INTO NhapKho (MaChiNhanh, MaNguoiNhap, TongTien, NhaCungCap, HoaDonNhap, GhiChu)
                                        VALUES (@MaChiNhanh, @MaNguoiNhap, @TongTien, @NhaCungCap, @HoaDonNhap, @GhiChu);
                                        SELECT SCOPE_IDENTITY();";

                                    using (SqlCommand cmd = new SqlCommand(queryNhap, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                                        cmd.Parameters.AddWithValue("@MaNguoiNhap", _maNguoiDung);
                                        cmd.Parameters.AddWithValue("@TongTien", GetTongTien());
                                        cmd.Parameters.AddWithValue("@NhaCungCap", frm.NhaCungCap);
                                        cmd.Parameters.AddWithValue("@HoaDonNhap", frm.HoaDon);
                                        cmd.Parameters.AddWithValue("@GhiChu", frm.GhiChu);

                                        int maNhapKho = Convert.ToInt32(cmd.ExecuteScalar());

                                        // Lưu chi tiết nhập
                                        foreach (DataRow row in _dtChiTietTam.Rows)
                                        {
                                            string queryChiTiet = @"
                                                INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
                                                VALUES (@MaNhapKho, @MaSanPham, @SoLuong, @DonGia, @ThanhTien)";

                                            using (SqlCommand cmdCT = new SqlCommand(queryChiTiet, conn, transaction))
                                            {
                                                cmdCT.Parameters.AddWithValue("@MaNhapKho", maNhapKho);
                                                cmdCT.Parameters.AddWithValue("@MaSanPham", row["MaSanPham"]);
                                                cmdCT.Parameters.AddWithValue("@SoLuong", row["SoLuong"]);
                                                cmdCT.Parameters.AddWithValue("@DonGia", row["DonGia"]);
                                                cmdCT.Parameters.AddWithValue("@ThanhTien", row["ThanhTien"]);
                                                cmdCT.ExecuteNonQuery();
                                            }

                                            // Cập nhật tồn kho
                                            string queryUpdateTon = @"
                                                UPDATE TonKho 
                                                SET SoLuongTon = SoLuongTon + @SoLuong,
                                                    SoLuongKhaDung = SoLuongKhaDung + @SoLuong,
                                                    NgayCapNhat = GETDATE()
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
                                    }
                                }
                                else
                                {
                                    // Lưu phiếu xuất (tương tự)
                                    // Cần thêm logic kiểm tra tồn kho đủ
                                }

                                transaction.Commit();
                                MessageBox.Show("Lưu phiếu thành công!", "Thành công",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Refresh data
                                TaiTonKho();
                                TaiNhapKho();
                                pnlChiTiet.Visible = false;
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception("Lỗi lưu phiếu: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
            string query = isNhapKho ?
                @"SELECT ct.MaSanPham, sp.TenSanPham, ct.SoLuongNhap AS SoLuong, 
                         ct.DonGiaNhap AS DonGia, ct.ThanhTien
                  FROM ChiTietNhapKho ct
                  INNER JOIN SanPham sp ON ct.MaSanPham = sp.MaSanPham
                  WHERE ct.MaNhapKho = @MaPhieu" :
                @"SELECT ct.MaSanPham, sp.TenSanPham, ct.SoLuongXuat AS SoLuong, 
                         ct.DonGiaXuat AS DonGia, ct.SoLuongXuat * ct.DonGiaXuat AS ThanhTien
                  FROM ChiTietXuatKho ct
                  INNER JOIN SanPham sp ON ct.MaSanPham = sp.MaSanPham
                  WHERE ct.MaXuatKho = @MaPhieu";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvChiTiet.DataSource = dt;
                        CapNhatTongTien();

                        // Cập nhật tiêu đề
                        Label lblTitle = (Label)pnlChiTiet.Controls[0];
                        lblTitle.Text = isNhapKho ?
                            $"📋 CHI TIẾT PHIẾU NHẬP #{maPhieu}" :
                            $"📋 CHI TIẾT PHIẾU XUẤT #{maPhieu}";

                        btnLuuPhieu.Visible = false;
                        btnInPhieu.Visible = true;
                        pnlChiTiet.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết phiếu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        // Helper class cho combobox
        private class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}