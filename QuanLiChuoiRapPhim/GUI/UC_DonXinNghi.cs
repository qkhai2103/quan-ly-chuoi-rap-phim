using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_DonXinNghi : UserControl
    {
        private int _maChiNhanh;
        private int _maNguoiDung;
        private string _hoTenNguoiDung;
        private DataTable _dtDonXinNghi;
        private DataTable _dtLoaiNghi;
        private DataTable _dtLichCaTrongTuan;

        // UI Components
        private Panel pnlHeader, pnlToolbar, pnlMain, pnlFooter;
        private TabControl tabControl;
        private TabPage tabTaoDon, tabLichSu;
        private DataGridView dgvLichSu;
        private Button btnTaoDon, btnLamMoi, btnXemChiTiet, btnHuyDon;
        private ComboBox cboLoaiNghi, cboNguoiThayThe;
        private DateTimePicker dtpNgayBatDau, dtpNgayKetThuc;
        private TextBox txtLyDo;
        private NumericUpDown nudSoNgay;
        private CheckBox chkUuTien;
        private Button btnTaiFile, btnXemFile;
        private Label lblFileDinhKem;
        private OpenFileDialog openFileDialog;

        // Chart
        private System.Windows.Forms.DataVisualization.Charting.Chart chartThongKe;

        public UC_DonXinNghi(int maChiNhanh, int maNguoiDung)
        {
            _maChiNhanh = maChiNhanh;
            _maNguoiDung = maNguoiDung;

            // Lấy thông tin người dùng
            _hoTenNguoiDung = LayHoTenNguoiDung(maNguoiDung);

            KhoiTaoGiaoDien();
            TaiDuLieuKhoiDau();
            TaiLichSuDon();
        }

        private string LayHoTenNguoiDung(int maNguoiDung)
        {
            try
            {
                string query = "SELECT HoTen FROM NguoiDung WHERE MaNguoiDung = @MaNguoiDung";
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                        return cmd.ExecuteScalar()?.ToString() ?? "Người dùng";
                    }
                }
            }
            catch
            {
                return "Người dùng";
            }
        }

        private void KhoiTaoGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 249);
            this.Font = new Font("Segoe UI", 10F);

            // ========== HEADER ==========
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(33, 150, 243) // Xanh dương Material
            };

            Label lblTitle = new Label
            {
                Text = "📄 ĐƠN XIN NGHỈ PHÉP",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubtitle = new Label
            {
                Text = $"Nhân viên: {_hoTenNguoiDung} | Chi nhánh: {_maChiNhanh}",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(224, 224, 224),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);

            // ========== TOOLBAR ==========
            pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnTaoDon = TaoButton("➕ TẠO ĐƠN MỚI", 20, Color.FromArgb(76, 175, 80));
            btnTaoDon.Click += BtnTaoDon_Click;

            btnLamMoi = TaoButton("🔄 LÀM MỚI", 180, Color.FromArgb(33, 150, 243));
            btnLamMoi.Click += BtnLamMoi_Click;

            btnXemChiTiet = TaoButton("👁️ XEM CHI TIẾT", 340, Color.FromArgb(255, 193, 7));
            btnXemChiTiet.Enabled = false;
            btnXemChiTiet.Click += BtnXemChiTiet_Click;

            btnHuyDon = TaoButton("🗑️ HỦY ĐƠN", 500, Color.FromArgb(244, 67, 54));
            btnHuyDon.Enabled = false;
            btnHuyDon.Click += BtnHuyDon_Click;

            pnlToolbar.Controls.AddRange(new Control[] { btnTaoDon, btnLamMoi, btnXemChiTiet, btnHuyDon });

            // ========== MAIN CONTENT (TAB CONTROL) ==========
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(120, 35),
                SizeMode = TabSizeMode.Fixed
            };

            tabTaoDon = new TabPage("📝 TẠO ĐƠN MỚI");
            tabLichSu = new TabPage("📜 LỊCH SỬ ĐƠN");

            tabControl.TabPages.AddRange(new TabPage[] { tabTaoDon, tabLichSu });
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            // Tạo giao diện cho tab Tạo đơn
            TaoTabTaoDon();

            // Tạo giao diện cho tab Lịch sử
            TaoTabLichSu();

            // ========== FOOTER ==========
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(33, 33, 33)
            };

            Label lblFooter = new Label
            {
                Text = "Hệ thống quản lý đơn xin nghỉ phép © 2024",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlFooter.Controls.Add(lblFooter);

            // Thêm tất cả controls vào UserControl
            this.Controls.Add(tabControl);
            this.Controls.Add(pnlToolbar);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private Button TaoButton(string text, int x, Color backColor)
        {
            return new Button
            {
                Text = text,
                Size = new Size(150, 40),
                Location = new Point(x, 10),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void TaoTabTaoDon()
        {
            tabTaoDon.Padding = new Padding(30);
            tabTaoDon.BackColor = Color.White;

            int y = 20;
            int labelWidth = 180;

            // Tiêu đề
            Label lblTitle = new Label
            {
                Text = "THÔNG TIN ĐƠN XIN NGHỈ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 150, 243),
                Location = new Point(0, y),
                Size = new Size(400, 40)
            };
            tabTaoDon.Controls.Add(lblTitle);
            y += 50;

            // Loại nghỉ
            Label lblLoaiNghi = TaoLabel("Loại nghỉ:", y);
            cboLoaiNghi = new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                Size = new Size(300, 35),
                Location = new Point(labelWidth, y - 3),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            tabTaoDon.Controls.Add(cboLoaiNghi);
            y += 45;

            // Ngày bắt đầu
            Label lblNgayBatDau = TaoLabel("Ngày bắt đầu:", y);
            dtpNgayBatDau = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10F),
                Size = new Size(200, 35),
                Location = new Point(labelWidth, y - 3),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today
            };
            dtpNgayBatDau.ValueChanged += DtpNgayBatDau_ValueChanged;
            tabTaoDon.Controls.Add(dtpNgayBatDau);
            y += 45;

            // Số ngày
            Label lblSoNgay = TaoLabel("Số ngày nghỉ:", y);
            nudSoNgay = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10F),
                Size = new Size(100, 35),
                Location = new Point(labelWidth, y - 3),
                Minimum = 0.5M,
                Maximum = 30,
                DecimalPlaces = 1,
                Increment = 0.5M,
                Value = 1
            };
            nudSoNgay.ValueChanged += NudSoNgay_ValueChanged;
            tabTaoDon.Controls.Add(nudSoNgay);
            y += 45;

            // Ngày kết thúc (tự động tính)
            Label lblNgayKetThuc = TaoLabel("Ngày kết thúc:", y);
            dtpNgayKetThuc = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10F),
                Size = new Size(200, 35),
                Location = new Point(labelWidth, y - 3),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today,
                Enabled = false
            };
            TinhNgayKetThuc();
            tabTaoDon.Controls.Add(dtpNgayKetThuc);
            y += 45;

            // Người thay thế
            Label lblNguoiThayThe = TaoLabel("Người thay thế (tùy chọn):", y);
            cboNguoiThayThe = new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                Size = new Size(300, 35),
                Location = new Point(labelWidth, y - 3),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            tabTaoDon.Controls.Add(cboNguoiThayThe);
            y += 45;

            // Ưu tiên
            Label lblUuTien = TaoLabel("Ưu tiên:", y);
            chkUuTien = new CheckBox
            {
                Text = "Cần xử lý gấp",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(labelWidth, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(244, 67, 54)
            };
            tabTaoDon.Controls.Add(chkUuTien);
            y += 45;

            // Lý do
            Label lblLyDo = TaoLabel("Lý do nghỉ:", y);
            txtLyDo = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Size = new Size(400, 100),
                Location = new Point(labelWidth, y - 3),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                MaxLength = 1000
            };
            tabTaoDon.Controls.Add(txtLyDo);
            y += 110;

            // File đính kèm
            Label lblFile = TaoLabel("File đính kèm:", y);
            btnTaiFile = new Button
            {
                Text = "📎 TẢI FILE",
                Size = new Size(120, 35),
                Location = new Point(labelWidth, y - 3),
                BackColor = Color.FromArgb(63, 81, 181),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F)
            };
            btnTaiFile.Click += BtnTaiFile_Click;

            btnXemFile = new Button
            {
                Text = "👁️ XEM FILE",
                Size = new Size(120, 35),
                Location = new Point(labelWidth + 130, y - 3),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            btnXemFile.Click += BtnXemFile_Click;

            lblFileDinhKem = new Label
            {
                Text = "Chưa có file đính kèm",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(labelWidth + 260, y + 5)
            };

            openFileDialog = new OpenFileDialog
            {
                Filter = "Tài liệu (*.pdf;*.doc;*.docx;*.jpg;*.png)|*.pdf;*.doc;*.docx;*.jpg;*.png|Tất cả files (*.*)|*.*",
                Title = "Chọn file đính kèm"
            };

            tabTaoDon.Controls.AddRange(new Control[] { lblFile, btnTaiFile, btnXemFile, lblFileDinhKem });
            y += 50;

            // Nút gửi đơn
            Button btnGuiDon = new Button
            {
                Text = "📤 GỬI ĐƠN XIN NGHỈ",
                Size = new Size(200, 45),
                Location = new Point(labelWidth, y),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGuiDon.Click += BtnGuiDon_Click;
            tabTaoDon.Controls.Add(btnGuiDon);

            // Thêm labels vào tab
            tabTaoDon.Controls.AddRange(new Control[] { lblLoaiNghi, lblNgayBatDau, lblSoNgay, lblNgayKetThuc, lblNguoiThayThe, lblUuTien, lblLyDo });
        }

        private Label TaoLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 66, 66),
                Location = new Point(20, y),
                Size = new Size(150, 30),
                TextAlign = ContentAlignment.MiddleRight
            };
        }

        private void TaoTabLichSu()
        {
            tabLichSu.Padding = new Padding(20);
            tabLichSu.BackColor = Color.White;

            // DataGridView
            dgvLichSu = new DataGridView
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
                MultiSelect = false
            };

            // Style
            dgvLichSu.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 150, 243);
            dgvLichSu.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLichSu.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvLichSu.ColumnHeadersHeight = 40;
            dgvLichSu.EnableHeadersVisualStyles = false;

            // Alternating rows
            dgvLichSu.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Sự kiện
            dgvLichSu.SelectionChanged += DgvLichSu_SelectionChanged;
            dgvLichSu.CellFormatting += DgvLichSu_CellFormatting;

            tabLichSu.Controls.Add(dgvLichSu);
        }

        private void TaiDuLieuKhoiDau()
        {
            TaiDanhSachLoaiNghi();
            TaiDanhSachNhanVienThayThe();
        }

        private void TaiDanhSachLoaiNghi()
        {
            try
            {
                string query = @"
                    SELECT MaLoaiNghi, TenLoaiNghi, SoNgayToiDa, YeuCauFile 
                    FROM LoaiNghi 
                    WHERE TrangThai = 1 
                    ORDER BY ThuTu";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtLoaiNghi = new DataTable();
                        da.Fill(_dtLoaiNghi);

                        cboLoaiNghi.Items.Clear();
                        cboLoaiNghi.Items.Add("-- Chọn loại nghỉ --");

                        foreach (DataRow row in _dtLoaiNghi.Rows)
                        {
                            string display = $"{row["TenLoaiNghi"]} (tối đa {row["SoNgayToiDa"]} ngày)";
                            cboLoaiNghi.Items.Add(new ComboboxItem
                            {
                                Text = display,
                                Value = row["MaLoaiNghi"],
                                Tag = row
                            });
                        }
                        cboLoaiNghi.SelectedIndex = 0;
                        cboLoaiNghi.SelectedIndexChanged += CboLoaiNghi_SelectedIndexChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách loại nghỉ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiDanhSachNhanVienThayThe()
        {
            try
            {
                string query = @"
                    SELECT MaNguoiDung, HoTen, TenDangNhap
                    FROM NguoiDung
                    WHERE MaChiNhanh = @MaChiNhanh
                      AND VaiTro = N'Nhân viên'
                      AND TrangThai = 1
                      AND MaNguoiDung != @MaNguoiDung
                    ORDER BY HoTen";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@MaNguoiDung", _maNguoiDung);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        cboNguoiThayThe.Items.Clear();
                        cboNguoiThayThe.Items.Add("-- Không chọn --");

                        foreach (DataRow row in dt.Rows)
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

        private void TaiLichSuDon()
        {
            try
            {
                string query = @"
                    SELECT 
                        d.MaDonXinNghi,
                        d.NgayBatDau,
                        d.NgayKetThuc,
                        d.SoNgay,
                        ln.TenLoaiNghi AS LoaiNghi,
                        d.LyDo,
                        d.TrangThai,
                        d.NgayGui,
                        d.NgayDuyet,
                        nd.HoTen AS NguoiDuyet,
                        d.GhiChuDuyet,
                        thay.HoTen AS NguoiThayThe,
                        d.FileDinhKem,
                        d.UuTien
                    FROM DonXinNghi d
                    LEFT JOIN LoaiNghi ln ON d.MaLoaiNghi = ln.MaLoaiNghi
                    LEFT JOIN NguoiDung nd ON d.NguoiDuyet = nd.MaNguoiDung
                    LEFT JOIN NguoiDung thay ON d.NguoiThayThe = thay.MaNguoiDung
                    WHERE d.MaNhanVien = @MaNhanVien
                    ORDER BY d.NgayGui DESC";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNhanVien", _maNguoiDung);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtDonXinNghi = new DataTable();
                        da.Fill(_dtDonXinNghi);

                        dgvLichSu.DataSource = _dtDonXinNghi;
                        DinhDangDataGridView();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử đơn: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DinhDangDataGridView()
        {
            if (dgvLichSu.Columns.Count == 0) return;

            // Ẩn cột ID
            string[] hiddenColumns = { "MaDonXinNghi", "FileDinhKem", "UuTien" };
            foreach (string colName in hiddenColumns)
            {
                if (dgvLichSu.Columns.Contains(colName))
                    dgvLichSu.Columns[colName].Visible = false;
            }

            // Đổi tên cột
            Dictionary<string, string> columnNames = new Dictionary<string, string>
            {
                { "NgayBatDau", "TỪ NGÀY" },
                { "NgayKetThuc", "ĐẾN NGÀY" },
                { "SoNgay", "SỐ NGÀY" },
                { "LoaiNghi", "LOẠI NGHỈ" },
                { "LyDo", "LÝ DO" },
                { "TrangThai", "TRẠNG THÁI" },
                { "NgayGui", "NGÀY GỬI" },
                { "NgayDuyet", "NGÀY DUYỆT" },
                { "NguoiDuyet", "NGƯỜI DUYỆT" },
                { "GhiChuDuyet", "GHI CHÚ DUYỆT" },
                { "NguoiThayThe", "NGƯỜI THAY THẾ" }
            };

            foreach (var pair in columnNames)
            {
                if (dgvLichSu.Columns.Contains(pair.Key))
                {
                    dgvLichSu.Columns[pair.Key].HeaderText = pair.Value;

                    // Format ngày
                    if (pair.Key.Contains("Ngay"))
                    {
                        dgvLichSu.Columns[pair.Key].DefaultCellStyle.Format = "dd/MM/yyyy";
                        if (pair.Key == "NgayGui" || pair.Key == "NgayDuyet")
                            dgvLichSu.Columns[pair.Key].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    }
                }
            }

            // Cột Lý do auto size
            if (dgvLichSu.Columns.Contains("Lý do"))
            {
                dgvLichSu.Columns["Lý do"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvLichSu.Columns["Lý do"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
        }

        // ==================== SỰ KIỆN ====================

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabLichSu)
            {
                TaiLichSuDon();
            }
        }

        private void DgvLichSu_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvLichSu.SelectedRows.Count > 0;
            btnXemChiTiet.Enabled = hasSelection;
            btnHuyDon.Enabled = hasSelection &&
                dgvLichSu.SelectedRows[0].Cells["TrangThai"].Value?.ToString() == "ChoDuyet";
        }

        private void DgvLichSu_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string trangThai = dgvLichSu.Rows[e.RowIndex].Cells["TrangThai"].Value?.ToString();
            string columnName = dgvLichSu.Columns[e.ColumnIndex].HeaderText;

            if (columnName == "TRẠNG THÁI")
            {
                switch (trangThai)
                {
                    case "ChoDuyet":
                        e.Value = "⏳ Chờ duyệt";
                        e.CellStyle.ForeColor = Color.FromArgb(255, 193, 7); // Vàng
                        e.CellStyle.Font = new Font(dgvLichSu.Font, FontStyle.Bold);
                        break;
                    case "DaDuyet":
                        e.Value = "✅ Đã duyệt";
                        e.CellStyle.ForeColor = Color.FromArgb(76, 175, 80); // Xanh lá
                        break;
                    case "TuChoi":
                        e.Value = "❌ Từ chối";
                        e.CellStyle.ForeColor = Color.FromArgb(244, 67, 54); // Đỏ
                        break;
                    case "DaNghi":
                        e.Value = "🏖️ Đã nghỉ";
                        e.CellStyle.ForeColor = Color.FromArgb(33, 150, 243); // Xanh dương
                        break;
                    case "Huy":
                        e.Value = "🗑️ Đã hủy";
                        e.CellStyle.ForeColor = Color.FromArgb(158, 158, 158); // Xám
                        break;
                }
            }

            // Tô màu dòng theo ưu tiên
            if (trangThai == "ChoDuyet")
            {
                object uuTien = dgvLichSu.Rows[e.RowIndex].Cells["UuTien"].Value;
                if (uuTien != DBNull.Value && Convert.ToBoolean(uuTien))
                {
                    dgvLichSu.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
                    dgvLichSu.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dgvLichSu.Font, FontStyle.Bold);
                }
            }
        }

        private void CboLoaiNghi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboLoaiNghi.SelectedIndex > 0 && cboLoaiNghi.SelectedItem is ComboboxItem item)
            {
                DataRow row = (DataRow)item.Tag;
                int soNgayToiDa = Convert.ToInt32(row["SoNgayToiDa"]);
                nudSoNgay.Maximum = soNgayToiDa;

                bool yeuCauFile = Convert.ToBoolean(row["YeuCauFile"]);
                if (yeuCauFile)
                {
                    MessageBox.Show("Loại nghỉ này yêu cầu có file đính kèm (giấy khám bệnh, xác nhận,...)",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DtpNgayBatDau_ValueChanged(object sender, EventArgs e)
        {
            TinhNgayKetThuc();
        }

        private void NudSoNgay_ValueChanged(object sender, EventArgs e)
        {
            TinhNgayKetThuc();
        }

        private void TinhNgayKetThuc()
        {
            if (dtpNgayBatDau.Value != null && nudSoNgay.Value > 0)
            {
                // Tính ngày kết thúc (trừ ngày bắt đầu)
                DateTime ngayKetThuc = dtpNgayBatDau.Value.AddDays((double)nudSoNgay.Value - 1);
                dtpNgayKetThuc.Value = ngayKetThuc;
            }
        }

        private void BtnTaiFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                lblFileDinhKem.Text = Path.GetFileName(filePath);
                lblFileDinhKem.Tag = filePath;
                lblFileDinhKem.ForeColor = Color.FromArgb(33, 150, 243);
                btnXemFile.Enabled = true;
            }
        }

        private void BtnXemFile_Click(object sender, EventArgs e)
        {
            if (lblFileDinhKem.Tag != null)
            {
                string filePath = lblFileDinhKem.Tag.ToString();
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

        private void BtnTaoDon_Click(object sender, EventArgs e)
        {
            tabControl.SelectedTab = tabTaoDon;
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            TaiLichSuDon();
            dgvLichSu.ClearSelection();
            btnXemChiTiet.Enabled = false;
            btnHuyDon.Enabled = false;
        }

        private void BtnXemChiTiet_Click(object sender, EventArgs e)
        {
            if (dgvLichSu.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvLichSu.SelectedRows[0];
            int maDon = Convert.ToInt32(row.Cells["MaDonXinNghi"].Value);

            // Mở form chi tiết
            Form frmChiTiet = new Form
            {
                Text = "Chi tiết đơn xin nghỉ",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = Color.White
            };

            // Hiển thị thông tin chi tiết
            string info = $"Mã đơn: {maDon}\n\n" +
                         $"Loại nghỉ: {row.Cells["LoaiNghi"].Value}\n" +
                         $"Từ ngày: {Convert.ToDateTime(row.Cells["TỪ NGÀY"].Value):dd/MM/yyyy}\n" +
                         $"Đến ngày: {Convert.ToDateTime(row.Cells["ĐẾN NGÀY"].Value):dd/MM/yyyy}\n" +
                         $"Số ngày: {row.Cells["SỐ NGÀY"].Value}\n" +
                         $"Lý do: {row.Cells["LÝ DO"].Value}\n" +
                         $"Trạng thái: {row.Cells["TRẠNG THÁI"].Value}\n" +
                         $"Ngày gửi: {Convert.ToDateTime(row.Cells["NGÀY GỬI"].Value):dd/MM/yyyy HH:mm}\n";

            if (row.Cells["NGÀY DUYỆT"].Value != DBNull.Value)
                info += $"Ngày duyệt: {Convert.ToDateTime(row.Cells["NGÀY DUYỆT"].Value):dd/MM/yyyy HH:mm}\n";

            if (row.Cells["NGƯỜI DUYỆT"].Value != DBNull.Value)
                info += $"Người duyệt: {row.Cells["NGƯỜI DUYỆT"].Value}\n";

            if (row.Cells["GHI CHÚ DUYỆT"].Value != DBNull.Value)
                info += $"Ghi chú duyệt: {row.Cells["GHI CHÚ DUYỆT"].Value}\n";

            if (row.Cells["NGƯỜI THAY THẾ"].Value != DBNull.Value)
                info += $"Người thay thế: {row.Cells["NGƯỜI THAY THẾ"].Value}";

            RichTextBox rtbInfo = new RichTextBox
            {
                Text = info,
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };

            frmChiTiet.Controls.Add(rtbInfo);
            frmChiTiet.ShowDialog();
        }

        private void BtnHuyDon_Click(object sender, EventArgs e)
        {
            if (dgvLichSu.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvLichSu.SelectedRows[0];
            int maDon = Convert.ToInt32(row.Cells["MaDonXinNghi"].Value);
            string trangThai = row.Cells["TrangThai"].Value?.ToString();

            if (trangThai != "ChoDuyet")
            {
                MessageBox.Show("Chỉ có thể hủy đơn đang chờ duyệt!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn hủy đơn này?", "Xác nhận hủy",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Đã hủy đơn thành công!", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            TaiLichSuDon();
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

        private void BtnGuiDon_Click(object sender, EventArgs e)
        {
            // Kiểm tra dữ liệu
            if (cboLoaiNghi.SelectedIndex <= 0)
            {
                MessageBox.Show("Vui lòng chọn loại nghỉ!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLoaiNghi.Focus();
                return;
            }

            if (nudSoNgay.Value <= 0)
            {
                MessageBox.Show("Số ngày nghỉ phải lớn hơn 0!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudSoNgay.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLyDo.Text))
            {
                MessageBox.Show("Vui lòng nhập lý do nghỉ!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLyDo.Focus();
                return;
            }

            // Kiểm tra file đính kèm nếu loại nghỉ yêu cầu
            if (cboLoaiNghi.SelectedItem is ComboboxItem item)
            {
                DataRow row = (DataRow)item.Tag;
                bool yeuCauFile = Convert.ToBoolean(row["YeuCauFile"]);

                if (yeuCauFile && lblFileDinhKem.Tag == null)
                {
                    MessageBox.Show("Loại nghỉ này yêu cầu có file đính kèm!", "Cảnh báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                // Lấy thông tin
                ComboboxItem selectedLoaiNghi = (ComboboxItem)cboLoaiNghi.SelectedItem;
                int maLoaiNghi = Convert.ToInt32(selectedLoaiNghi.Value);

                int? nguoiThayThe = null;
                if (cboNguoiThayThe.SelectedIndex > 0)
                {
                    ComboboxItem selectedThayThe = (ComboboxItem)cboNguoiThayThe.SelectedItem;
                    nguoiThayThe = Convert.ToInt32(selectedThayThe.Value);
                }

                // Lưu file đính kèm nếu có
                string filePath = null;
                if (lblFileDinhKem.Tag != null)
                {
                    string sourceFile = lblFileDinhKem.Tag.ToString();
                    string fileName = $"DonNghi_{_maNguoiDung}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(sourceFile)}";
                    string destFolder = Path.Combine(Application.StartupPath, "DonXinNghiFiles");

                    if (!Directory.Exists(destFolder))
                        Directory.CreateDirectory(destFolder);

                    filePath = Path.Combine(destFolder, fileName);
                    File.Copy(sourceFile, filePath, true);
                }

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO DonXinNghi (
                            MaNhanVien, MaChiNhanh, MaLoaiNghi,
                            NgayBatDau, NgayKetThuc, SoNgay,
                            LyDo, NguoiThayThe, UuTien,
                            FileDinhKem, TrangThai, NgayGui
                        ) VALUES (
                            @MaNhanVien, @MaChiNhanh, @MaLoaiNghi,
                            @NgayBatDau, @NgayKetThuc, @SoNgay,
                            @LyDo, @NguoiThayThe, @UuTien,
                            @FileDinhKem, N'ChoDuyet', GETDATE()
                        )";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNhanVien", _maNguoiDung);
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@MaLoaiNghi", maLoaiNghi);
                        cmd.Parameters.AddWithValue("@NgayBatDau", dtpNgayBatDau.Value);
                        cmd.Parameters.AddWithValue("@NgayKetThuc", dtpNgayKetThuc.Value);
                        cmd.Parameters.AddWithValue("@SoNgay", nudSoNgay.Value);
                        cmd.Parameters.AddWithValue("@LyDo", txtLyDo.Text.Trim());
                        cmd.Parameters.AddWithValue("@UuTien", chkUuTien.Checked);
                        cmd.Parameters.AddWithValue("@NguoiThayThe", nguoiThayThe ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FileDinhKem", filePath ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Đã gửi đơn xin nghỉ thành công!\nĐơn của bạn đang chờ quản lý duyệt.",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Reset form
                        ResetFormTaoDon();
                        tabControl.SelectedTab = tabLichSu;
                        TaiLichSuDon();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi đơn: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetFormTaoDon()
        {
            cboLoaiNghi.SelectedIndex = 0;
            dtpNgayBatDau.Value = DateTime.Today;
            nudSoNgay.Value = 1;
            cboNguoiThayThe.SelectedIndex = 0;
            chkUuTien.Checked = false;
            txtLyDo.Clear();
            lblFileDinhKem.Text = "Chưa có file đính kèm";
            lblFileDinhKem.Tag = null;
            lblFileDinhKem.ForeColor = Color.Gray;
            btnXemFile.Enabled = false;
        }

        // Helper class cho combobox
        private class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public object Tag { get; set; }
            public override string ToString() => Text;
        }
    }
}