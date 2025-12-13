using QuanLiChuoiRapPhim.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_DonXinNghi : UserControl
    {
        // Chi nhánh của Manager
        private int _maChiNhanh;                    // Chi nhánh của Manager
        private int _maNguoiDung;                   // ID của Manager hiện tại
        private DataTable _dtDonXinNghi;            // Dữ liệu gốc
        private DataTable _dtNhanVien;              // Danh sách nhân viên để chọn thay thế

        // Controls
        private DataGridView dgvDonXinNghi;
        private TabControl tabMain;
        private TabPage tabChoDuyet, tabDaDuyet, tabTuChoi;
        private Panel pnlChiTiet;
        private Button btnDuyet, btnTuChoi, btnLamMoi, btnXemChiTiet;
        private ComboBox cboLoaiNghiFilter;
        private DateTimePicker dtpTuNgay, dtpDenNgay;
        private Label lblThongKe;

        // Detail controls
        private Label lblMaDon, lblNhanVien, lblLoaiNghi, lblNgayNghi, lblSoNgay, lblLyDo, lblTrangThai;
        private ComboBox cboNguoiThayThe;
        private TextBox txtGhiChuDuyet;
        private Button btnHuyDon, btnLuuThayThe;

        public UC_DonXinNghi()
        {
       
            _maChiNhanh = 1;
            _maNguoiDung = 1;
        }

        public UC_DonXinNghi(int maChiNhanh, int maNguoiDung)
        {
            
            _maChiNhanh = maChiNhanh;
            _maNguoiDung = maNguoiDung;
            ThietLapGiaoDien();
        }

        private void ThietLapGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // === TIÊU ĐỀ ===
            Panel pnlTieuDe = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(40, 167, 69) // Xanh lá
            };

            Label lblTieuDe = new Label
            {
                Text = "📋 DUYỆT ĐƠN XIN NGHỈ PHÉP",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlTieuDe.Controls.Add(lblTieuDe);

            // === THANH CÔNG CỤ ===
            Panel pnlCongCu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 10, 20, 10)
            };

            // Lọc loại nghỉ
            Label lblLoaiFilter = new Label
            {
                Text = "Loại nghỉ:",
                AutoSize = true,
                Location = new Point(20, 15),
                Font = new Font("Segoe UI", 10F)
            };

            cboLoaiNghiFilter = new ComboBox
            {
                Size = new Size(120, 30),
                Location = new Point(100, 10),
                Font = new Font("Segoe UI", 10F)
            };
            cboLoaiNghiFilter.Items.AddRange(new string[] { "Tất cả", "Phep", "KhongPhep", "OmBenh", "Huy", "KhongPhepODai" });
            cboLoaiNghiFilter.SelectedIndex = 0;
            cboLoaiNghiFilter.SelectedIndexChanged += CboLoaiNghiFilter_SelectedIndexChanged;

            // Lọc theo ngày
            Label lblTuNgay = new Label
            {
                Text = "Từ:",
                AutoSize = true,
                Location = new Point(240, 15),
                Font = new Font("Segoe UI", 10F)
            };

            dtpTuNgay = new DateTimePicker
            {
                Size = new Size(120, 30),
                Location = new Point(270, 10),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-1)
            };
            dtpTuNgay.ValueChanged += DtpTuNgay_ValueChanged;

            Label lblDenNgay = new Label
            {
                Text = "Đến:",
                AutoSize = true,
                Location = new Point(400, 15),
                Font = new Font("Segoe UI", 10F)
            };

            dtpDenNgay = new DateTimePicker
            {
                Size = new Size(120, 30),
                Location = new Point(440, 10),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            dtpDenNgay.ValueChanged += DtpDenNgay_ValueChanged;

            // Nút làm mới
            btnLamMoi = new Button
            {
                Text = "🔄 Làm mới",
                Size = new Size(100, 35),
                Location = new Point(580, 10),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnLamMoi.Click += BtnLamMoi_Click;

            // Nút xem chi tiết
            btnXemChiTiet = new Button
            {
                Text = "👁️ Xem chi tiết",
                Size = new Size(120, 35),
                Location = new Point(690, 10),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Enabled = false
            };
            btnXemChiTiet.Click += BtnXemChiTiet_Click;

            pnlCongCu.Controls.AddRange(new Control[] {
                lblLoaiFilter, cboLoaiNghiFilter,
                lblTuNgay, dtpTuNgay, lblDenNgay, dtpDenNgay,
                btnLamMoi, btnXemChiTiet
            });

            // === TAB CONTROL ===
            tabMain = new TabControl
            {
                Dock = DockStyle.Top,
                Height = 350,
                Font = new Font("Segoe UI", 10F)
            };

            tabChoDuyet = new TabPage("⏳ CHỜ DUYỆT");
            tabDaDuyet = new TabPage("✅ ĐÃ DUYỆT");
            tabTuChoi = new TabPage("❌ TỪ CHỐI");

            tabMain.TabPages.AddRange(new TabPage[] { tabChoDuyet, tabDaDuyet, tabTuChoi });
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

            // Tạo DataGridView cho mỗi tab
            TaoDataGridViewChoTab(tabChoDuyet);
            TaoDataGridViewChoTab(tabDaDuyet);
            TaoDataGridViewChoTab(tabTuChoi);

            // === PANEL CHI TIẾT (Bên phải) ===
            pnlChiTiet = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 252),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Padding = new Padding(15)
            };

            TaoPanelChiTiet();

            // === PANEL NÚT HÀNH ĐỘNG ===
            Panel pnlHanhDong = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 245)
            };

            btnDuyet = new Button
            {
                Text = "✅ DUYỆT ĐƠN",
                Size = new Size(150, 40),
                Location = new Point(50, 10),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Enabled = false,
                Visible = false
            };
            btnDuyet.Click += BtnDuyet_Click;

            btnTuChoi = new Button
            {
                Text = "❌ TỪ CHỐI",
                Size = new Size(150, 40),
                Location = new Point(220, 10),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Enabled = false,
                Visible = false
            };
            btnTuChoi.Click += BtnTuChoi_Click;

            pnlHanhDong.Controls.AddRange(new Control[] { btnDuyet, btnTuChoi });

            // === THỐNG KÊ ===
            Panel pnlThongKe = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            lblThongKe = new Label
            {
                Text = "Đang tải...",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlThongKe.Controls.Add(lblThongKe);

            // Thêm controls vào UserControl
            this.Controls.Add(pnlChiTiet);
            this.Controls.Add(pnlHanhDong);
            this.Controls.Add(pnlThongKe);
            this.Controls.Add(tabMain);
            this.Controls.Add(pnlCongCu);
            this.Controls.Add(pnlTieuDe);
        }

        private void TaoDataGridViewChoTab(TabPage tab)
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

            // Style header
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 123, 255);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            // Sự kiện
            dgv.SelectionChanged += (s, e) => {
                if (dgv.SelectedRows.Count > 0)
                {
                    btnXemChiTiet.Enabled = true;
                    HienThiChiTietDon(dgv.SelectedRows[0].Cells["MaDonXinNghi"].Value);
                }
            };

            dgv.CellDoubleClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    HienThiChiTietDon(dgv.Rows[e.RowIndex].Cells["MaDonXinNghi"].Value);
                    btnXemChiTiet.PerformClick();
                }
            };

            // Gán tag để phân biệt
            if (tab == tabChoDuyet) dgv.Tag = "Cho";
            else if (tab == tabDaDuyet) dgv.Tag = "Duyet";
            else if (tab == tabTuChoi) dgv.Tag = "TuChoi";

            tab.Controls.Add(dgv);
        }

        private void TaoPanelChiTiet()
        {
            int yPos = 20;
            int labelWidth = 150;

            // Tiêu đề chi tiết
            Label lblTitle = new Label
            {
                Text = "📄 CHI TIẾT ĐƠN XIN NGHỈ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(10, yPos),
                Size = new Size(400, 30)
            };
            pnlChiTiet.Controls.Add(lblTitle);
            yPos += 40;

            // Mã đơn
            lblMaDon = TaoLabelChiTiet("Mã đơn:", yPos);
            yPos += 30;

            // Nhân viên
            lblNhanVien = TaoLabelChiTiet("Nhân viên:", yPos);
            yPos += 30;

            // Loại nghỉ
            lblLoaiNghi = TaoLabelChiTiet("Loại nghỉ:", yPos);
            yPos += 30;

            // Ngày nghỉ
            lblNgayNghi = TaoLabelChiTiet("Ngày nghỉ:", yPos);
            yPos += 30;

            // Số ngày
            lblSoNgay = TaoLabelChiTiet("Số ngày:", yPos);
            yPos += 30;

            // Lý do
            lblLyDo = TaoLabelChiTiet("Lý do:", yPos);
            yPos += 50;

            // Người thay thế (chỉ hiện với đơn chờ duyệt)
            Label lblThayThe = new Label
            {
                Text = "Người thay thế:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleRight,
                Visible = false
            };

            cboNguoiThayThe = new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                Size = new Size(250, 30),
                Location = new Point(180, yPos),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };

            pnlChiTiet.Controls.Add(lblThayThe);
            pnlChiTiet.Controls.Add(cboNguoiThayThe);
            yPos += 40;

            // Ghi chú duyệt
            Label lblGhiChu = new Label
            {
                Text = "Ghi chú duyệt:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            txtGhiChuDuyet = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Size = new Size(250, 60),
                Location = new Point(180, yPos),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                MaxLength = 500
            };
            yPos += 70;

            pnlChiTiet.Controls.Add(lblGhiChu);
            pnlChiTiet.Controls.Add(txtGhiChuDuyet);

            // Trạng thái
            lblTrangThai = TaoLabelChiTiet("Trạng thái:", yPos);
            yPos += 40;

            // Nút hủy đơn (chỉ cho đơn chờ duyệt)
            btnHuyDon = new Button
            {
                Text = "🗑️ HỦY ĐƠN",
                Size = new Size(120, 35),
                Location = new Point(50, yPos),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Visible = false
            };
            btnHuyDon.Click += BtnHuyDon_Click;

            // Nút lưu thay thế
            btnLuuThayThe = new Button
            {
                Text = "💾 LƯU THAY THẾ",
                Size = new Size(150, 35),
                Location = new Point(180, yPos),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Visible = false
            };
            btnLuuThayThe.Click += BtnLuuThayThe_Click;

            pnlChiTiet.Controls.Add(btnHuyDon);
            pnlChiTiet.Controls.Add(btnLuuThayThe);
        }

        private Label TaoLabelChiTiet(string text, int yPos)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(20, yPos),
                Size = new Size(150, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            Label lblValue = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(180, yPos),
                Size = new Size(250, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            pnlChiTiet.Controls.Add(lbl);
            pnlChiTiet.Controls.Add(lblValue);

            return lblValue;
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

        private void TaiDonXinNghi()
        {
            string query = @"
                SELECT 
                    d.MaDonXinNghi,
                    d.MaNhanVien,
                    nv.HoTen AS TenNhanVien,
                    nv.TenDangNhap AS TenDangNhapNV,
                    d.MaChiNhanh,
                    c.TenChiNhanh,
                    d.LoaiNghi,
                    d.NgayBatDau,
                    d.NgayKetThuc,
                    d.SoNgay,
                    d.LyDo,
                    d.DiaDiem,
                    d.NguoiThayThe,
                    thay.HoTen AS TenNguoiThayThe,
                    d.TrangThai,
                    d.NguoiDuyet,
                    duyet.HoTen AS TenNguoiDuyet,
                    d.GhiChuDuyet,
                    d.NgayGui,
                    d.NgayDuyet
                FROM DonXinNghi d
                INNER JOIN NguoiDung nv ON d.MaNhanVien = nv.MaNguoiDung
                INNER JOIN ChiNhanh c ON d.MaChiNhanh = c.MaChiNhanh
                LEFT JOIN NguoiDung thay ON d.NguoiThayThe = thay.MaNguoiDung
                LEFT JOIN NguoiDung duyet ON d.NguoiDuyet = duyet.MaNguoiDung
                WHERE d.MaChiNhanh = @MaChiNhanh
                ORDER BY d.NgayGui DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtDonXinNghi = new DataTable();
                        da.Fill(_dtDonXinNghi);

                        PhanLoaiDonVaoTab();
                        CapNhatThongKe();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách đơn xin nghỉ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblThongKe.Text = "Lỗi tải dữ liệu";
            }
        }

        private void PhanLoaiDonVaoTab()
        {
            foreach (TabPage tab in tabMain.TabPages)
            {
                DataGridView dgv = (DataGridView)tab.Controls[0];
                string trangThai = (string)dgv.Tag;

                DataView dv = new DataView(_dtDonXinNghi);

                // Lọc theo ngày
                DateTime tuNgay = dtpTuNgay.Value.Date;
                DateTime denNgay = dtpDenNgay.Value.Date.AddDays(1);
                dv.RowFilter = $"TrangThai = '{trangThai}' AND NgayGui >= #{tuNgay:yyyy-MM-dd}# AND NgayGui < #{denNgay:yyyy-MM-dd}#";

                // Lọc thêm theo loại nghỉ nếu cần
                if (cboLoaiNghiFilter.SelectedIndex > 0)
                {
                    string loaiNghi = cboLoaiNghiFilter.SelectedItem.ToString();
                    if (dv.RowFilter.Length > 0)
                        dv.RowFilter += " AND ";
                    dv.RowFilter += $"LoaiNghi = '{loaiNghi}'";
                }

                dgv.DataSource = dv;
                DinhDangDataGridView(dgv);
            }
        }

        private void DinhDangDataGridView(DataGridView dgv)
        {
            if (dgv.Columns.Count == 0) return;

            // Ẩn cột ID
            if (dgv.Columns.Contains("MaDonXinNghi"))
                dgv.Columns["MaDonXinNghi"].Visible = false;
            if (dgv.Columns.Contains("MaNhanVien"))
                dgv.Columns["MaNhanVien"].Visible = false;
            if (dgv.Columns.Contains("MaChiNhanh"))
                dgv.Columns["MaChiNhanh"].Visible = false;
            if (dgv.Columns.Contains("NguoiDuyet"))
                dgv.Columns["NguoiDuyet"].Visible = false;

            // Đổi tên cột
            Dictionary<string, string> columnNames = new Dictionary<string, string>
            {
                { "TenNhanVien", "NHÂN VIÊN" },
                { "TenDangNhapNV", "TÊN ĐĂNG NHẬP" },
                { "TenChiNhanh", "CHI NHÁNH" },
                { "LoaiNghi", "LOẠI NGHỈ" },
                { "NgayBatDau", "TỪ NGÀY" },
                { "NgayKetThuc", "ĐẾN NGÀY" },
                { "SoNgay", "SỐ NGÀY" },
                { "LyDo", "LÝ DO" },
                { "TenNguoiThayThe", "NGƯỜI THAY THẾ" },
                { "TrangThai", "TRẠNG THÁI" },
                { "TenNguoiDuyet", "NGƯỜI DUYỆT" },
                { "GhiChuDuyet", "GHI CHÚ DUYỆT" },
                { "NgayGui", "NGÀY GỬI" },
                { "NgayDuyet", "NGÀY DUYỆT" }
            };

            foreach (var pair in columnNames)
            {
                if (dgv.Columns.Contains(pair.Key))
                    dgv.Columns[pair.Key].HeaderText = pair.Value;
            }

            // Format ngày
            if (dgv.Columns.Contains("NgayBatDau"))
                dgv.Columns["NgayBatDau"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgv.Columns.Contains("NgayKetThuc"))
                dgv.Columns["NgayKetThuc"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgv.Columns.Contains("NgayGui"))
                dgv.Columns["NgayGui"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (dgv.Columns.Contains("NgayDuyet"))
                dgv.Columns["NgayDuyet"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            // Auto size cột Lý do
            if (dgv.Columns.Contains("LyDo"))
            {
                dgv.Columns["LyDo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgv.Columns["LyDo"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
        }

        private void HienThiChiTietDon(object maDon)
        {
            if (maDon == null || maDon == DBNull.Value)
            {
                pnlChiTiet.Visible = false;
                return;
            }

            try
            {
                int maDonXinNghi = Convert.ToInt32(maDon);
                DataRow[] rows = _dtDonXinNghi.Select($"MaDonXinNghi = {maDonXinNghi}");

                if (rows.Length == 0) return;

                DataRow row = rows[0];
                string trangThai = row["TrangThai"].ToString();

                // Hiển thị thông tin
                lblMaDon.Text = row["MaDonXinNghi"].ToString();
                lblNhanVien.Text = $"{row["TenNhanVien"]} ({row["TenDangNhapNV"]})";
                lblLoaiNghi.Text = row["LoaiNghi"].ToString();
                lblNgayNghi.Text = $"{Convert.ToDateTime(row["NgayBatDau"]):dd/MM/yyyy} - {Convert.ToDateTime(row["NgayKetThuc"]):dd/MM/yyyy}";
                lblSoNgay.Text = row["SoNgay"].ToString() + " ngày";
                lblLyDo.Text = row["LyDo"].ToString();
                lblTrangThai.Text = row["TrangThai"].ToString();

                txtGhiChuDuyet.Text = row["GhiChuDuyet"] != DBNull.Value ? row["GhiChuDuyet"].ToString() : "";

                // Hiển thị/ẩn controls theo trạng thái
                bool isChoDuyet = trangThai == "Cho";
                bool isDaDuyet = trangThai == "Duyet";

                // Người thay thế
                Label lblThayThe = (Label)pnlChiTiet.Controls[8];
                lblThayThe.Visible = isChoDuyet;
                cboNguoiThayThe.Visible = isChoDuyet;

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

                // Nút hành động
                btnDuyet.Visible = isChoDuyet;
                btnTuChoi.Visible = isChoDuyet;
                btnHuyDon.Visible = isChoDuyet;
                btnLuuThayThe.Visible = isChoDuyet;

                btnDuyet.Enabled = isChoDuyet;
                btnTuChoi.Enabled = isChoDuyet;
                btnHuyDon.Enabled = isChoDuyet;
                btnLuuThayThe.Enabled = isChoDuyet && cboNguoiThayThe.SelectedIndex > 0;

                // Cho phép edit ghi chú
                txtGhiChuDuyet.ReadOnly = !isChoDuyet;

                pnlChiTiet.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị chi tiết: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CapNhatThongKe()
        {
            if (_dtDonXinNghi == null) return;

            int tongDon = _dtDonXinNghi.Rows.Count;
            int choDuyet = 0, daDuyet = 0, tuChoi = 0;

            foreach (DataRow row in _dtDonXinNghi.Rows)
            {
                string trangThai = row["TrangThai"].ToString();
                if (trangThai == "Cho") choDuyet++;
                else if (trangThai == "Duyet") daDuyet++;
                else if (trangThai == "TuChoi") tuChoi++;
            }

            lblThongKe.Text = $"Tổng: {tongDon} đơn | ⏳ Chờ: {choDuyet} | ✅ Đã duyệt: {daDuyet} | ❌ Từ chối: {tuChoi}";
        }

        // ==================== SỰ KIỆN ====================

        private void CboLoaiNghiFilter_SelectedIndexChanged(object sender, EventArgs e)
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
            TaiDonXinNghi();
            pnlChiTiet.Visible = false;
            btnXemChiTiet.Enabled = false;
        }

        private void BtnXemChiTiet_Click(object sender, EventArgs e)
        {
            // Đã xử lý trong SelectionChanged
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear selection khi chuyển tab
            foreach (TabPage tab in tabMain.TabPages)
            {
                DataGridView dgv = (DataGridView)tab.Controls[0];
                dgv.ClearSelection();
            }
            pnlChiTiet.Visible = false;
            btnXemChiTiet.Enabled = false;
        }

        private void BtnDuyet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblMaDon.Text)) return;

            int maDon = Convert.ToInt32(lblMaDon.Text);
            string ghiChu = txtGhiChuDuyet.Text.Trim();

            if (MessageBox.Show("Bạn có chắc chắn DUYỆT đơn xin nghỉ này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                    {
                        conn.Open();
                        string query = @"
                            UPDATE DonXinNghi 
                            SET TrangThai = N'Duyet',
                                NguoiDuyet = @NguoiDuyet,
                                GhiChuDuyet = @GhiChu,
                                NgayDuyet = GETDATE()
                            WHERE MaDonXinNghi = @MaDon";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@NguoiDuyet", _maNguoiDung);
                            cmd.Parameters.AddWithValue("@GhiChu", ghiChu);
                            cmd.Parameters.AddWithValue("@MaDon", maDon);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Đã duyệt đơn xin nghỉ thành công!", "Thành công",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                TaiDonXinNghi();
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
        }

        private void BtnTuChoi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblMaDon.Text)) return;

            int maDon = Convert.ToInt32(lblMaDon.Text);
            string ghiChu = txtGhiChuDuyet.Text.Trim();

            if (string.IsNullOrEmpty(ghiChu))
            {
                MessageBox.Show("Vui lòng nhập lý do từ chối!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGhiChuDuyet.Focus();
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn TỪ CHỐI đơn xin nghỉ này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                    {
                        conn.Open();
                        string query = @"
                            UPDATE DonXinNghi 
                            SET TrangThai = N'TuChoi',
                                NguoiDuyet = @NguoiDuyet,
                                GhiChuDuyet = @GhiChu,
                                NgayDuyet = GETDATE()
                            WHERE MaDonXinNghi = @MaDon";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@NguoiDuyet", _maNguoiDung);
                            cmd.Parameters.AddWithValue("@GhiChu", ghiChu);
                            cmd.Parameters.AddWithValue("@MaDon", maDon);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Đã từ chối đơn xin nghỉ!", "Thành công",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                TaiDonXinNghi();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi từ chối đơn: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnHuyDon_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblMaDon.Text)) return;

            int maDon = Convert.ToInt32(lblMaDon.Text);

            if (MessageBox.Show("Bạn có chắc chắn HỦY đơn xin nghỉ này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                    {
                        conn.Open();
                        string query = @"
                            UPDATE DonXinNghi 
                            SET TrangThai = N'Huy'
                            WHERE MaDonXinNghi = @MaDon";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaDon", maDon);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Đã hủy đơn xin nghỉ!", "Thành công",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                TaiDonXinNghi();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi hủy đơn: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

                            // Cập nhật lại chi tiết
                            HienThiChiTietDon(maDon);
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

        // Helper class cho combobox
        private class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}