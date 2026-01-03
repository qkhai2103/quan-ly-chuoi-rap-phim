﻿// File: UC_LichLamViec.cs
// Vị trí: QuanLiChuoiRapPhim.GUI
// Dành cho vai trò Quản lý chi nhánh và Nhân viên (xem lịch cá nhân)

using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_LichLamViec : UserControl
    {
        private readonly int _maChiNhanh;
        private readonly int _maNguoiDung; // ID nhân viên hiện tại (nếu là nhân viên)
        private readonly bool _isStaff; // Có phải nhân viên không
        private DateTime _ngayHienTai = DateTime.Now; // Ngày hiện tại
        private DateTime _tuanBatDau; // Thứ Hai của tuần hiện tại

        private DataGridView dgvLichCa;
        private Label lblTuan;
        private Button btnTuanTruoc, btnTuanSau, btnLamMoi;
        private ComboBox cboLocNhanVien, cboLocCa;
        private Panel pnlLoc;
        private Panel pnlXinNghi; // Panel xin nghỉ cho nhân viên
        private DataTable _dtLichFull;
        
        // CGV Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

        // Constructor cho Quản lý - xem tất cả nhân viên
        public UC_LichLamViec(int maChiNhanh)
        {
            _maChiNhanh = maChiNhanh;
            _maNguoiDung = 0;
            _isStaff = false;

            TinhTuanHienTai();
            ThietLapGiaoDien();
            TaiLichLamViec();
        }

        // Constructor cho Nhân viên - chỉ xem lịch của mình
        public UC_LichLamViec(int maChiNhanh, int maNguoiDung, bool isStaff)
        {
            _maChiNhanh = maChiNhanh;
            _maNguoiDung = maNguoiDung;
            _isStaff = isStaff;

            TinhTuanHienTai();
            ThietLapGiaoDien();
            TaiLichLamViec();
        }

        private void TinhTuanHienTai()
        {
            // Tìm thứ Hai của tuần chứa ngày hiện tại
            int diff = (int)_ngayHienTai.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            _tuanBatDau = _ngayHienTai.AddDays(-diff);
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
                BackColor = _cgvRed
            };
            
            // Title khác nhau cho Nhân viên và Quản lý
            string titleText = _isStaff ? "LỊCH LÀM VIỆC CỦA TÔI" : "LỊCH LÀM VIỆC & PHÂN CÔNG CA";
            Label lblTieuDe = new Label
            {
                Text = titleText,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            pnlTieuDe.Controls.Add(lblTieuDe);

            // === THANH ĐIỀU KHIỂN TUẦN ===
            Panel pnlDieuKhien = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 10, 20, 10)
            };

            btnTuanTruoc = new Button
            {
                Text = "◄ Tuần trước",
                Size = new Size(120, 35),
                Location = new Point(10, 12),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTuanTruoc.Click += BtnTuanTruoc_Click;

            lblTuan = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(150, 18),
                ForeColor = Color.FromArgb(23, 32, 42)
            };
            CapNhatLabelTuan();

            btnTuanSau = new Button
            {
                Text = "Tuần sau ►",
                Size = new Size(120, 35),
                Location = new Point(350, 12),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTuanSau.Click += BtnTuanSau_Click;

            btnLamMoi = new Button
            {
                Text = "Làm mới",
                Size = new Size(100, 35),
                Location = new Point(490, 12),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLamMoi.Click += BtnLamMoi_Click;

            pnlDieuKhien.Controls.AddRange(new Control[] { btnTuanTruoc, lblTuan, btnTuanSau, btnLamMoi });

            // === PANEL LỌC === (ẩn đi nếu là nhân viên)
            pnlLoc = new Panel
            {
                Dock = DockStyle.Top,
                Height = _isStaff ? 0 : 50, // Ẩn panel lọc nếu là nhân viên
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10),
                Visible = !_isStaff // Ẩn hoàn toàn nếu là nhân viên
            };

            if (!_isStaff)
            {
                Label lblLocNV = new Label { Text = "Lọc nhân viên:", Location = new Point(10, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
                cboLocNhanVien = new ComboBox { Width = 200, Location = new Point(120, 12), DropDownStyle = ComboBoxStyle.DropDownList };
                cboLocNhanVien.Items.Add("-- Tất cả --");
                cboLocNhanVien.SelectedIndex = 0;
                cboLocNhanVien.SelectedIndexChanged += (s, e) => ApDungBoLoc();

                Label lblLocCa = new Label { Text = "Lọc ca:", Location = new Point(340, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
                cboLocCa = new ComboBox { Width = 150, Location = new Point(410, 12), DropDownStyle = ComboBoxStyle.DropDownList };
                cboLocCa.Items.AddRange(new[] { "-- Tất cả --", "Ca sáng", "Ca chiều", "Ca tối" });
                cboLocCa.SelectedIndex = 0;
                cboLocCa.SelectedIndexChanged += (s, e) => ApDungBoLoc();

                pnlLoc.Controls.AddRange(new Control[] { lblLocNV, cboLocNhanVien, lblLocCa, cboLocCa });
            }

            // === PANEL XIN NGHỈ (chỉ cho nhân viên) ===
            if (_isStaff)
            {
                CreateAbsenceRequestPanel();
            }

            // === DATAGRIDVIEW ===
            dgvLichCa = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            // Style
            dgvLichCa.ColumnHeadersDefaultCellStyle.BackColor = _cgvRed;
            dgvLichCa.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLichCa.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvLichCa.EnableHeadersVisualStyles = false;

            // Event: Click vào row để tự động điền ngày xin nghỉ
            if (_isStaff)
            {
                dgvLichCa.CellClick += (s, e) =>
                {
                    if (e.RowIndex >= 0 && pnlXinNghi != null)
                    {
                        try
                        {
                            var row = dgvLichCa.Rows[e.RowIndex];
                            
                            // Lấy ngày từ cột "Ngày"
                            var ngayCell = row.Cells["Ngày"].Value;
                            if (ngayCell != null && ngayCell != DBNull.Value)
                            {
                                DateTime ngay = Convert.ToDateTime(ngayCell);
                                
                                // Chỉ cho phép xin nghỉ từ hôm nay trở đi
                                if (ngay >= DateTime.Today)
                                {
                                    // Find DateTimePicker in pnlXinNghi
                                    var dtp = pnlXinNghi.Controls.Find("dtpNgayNghi", true).FirstOrDefault() as DateTimePicker;
                                    if (dtp != null)
                                    {
                                        dtp.Value = ngay;
                                    }

                                    // Find ComboBox Ca
                                    var cboCa = pnlXinNghi.Controls.Find("cboCa", true).FirstOrDefault() as ComboBox;
                                    if (cboCa != null)
                                    {
                                        string caLam = row.Cells["Ca làm"].Value?.ToString() ?? "";
                                        if (caLam.ToLower().Contains("sáng"))
                                            cboCa.SelectedIndex = 0;
                                        else if (caLam.ToLower().Contains("chiều"))
                                            cboCa.SelectedIndex = 1;
                                        else if (caLam.ToLower().Contains("tối"))
                                            cboCa.SelectedIndex = 2;
                                    }
                                }
                            }
                        }
                        catch { /* Ignore errors */ }
                    }
                };
            }

            this.Controls.Add(dgvLichCa);
            if (_isStaff && pnlXinNghi != null)
            {
                this.Controls.Add(pnlXinNghi);
            }
            this.Controls.Add(pnlLoc);
            this.Controls.Add(pnlDieuKhien);
            this.Controls.Add(pnlTieuDe);
        }

        private void CreateAbsenceRequestPanel()
        {
            pnlXinNghi = new Panel
            {
                Dock = DockStyle.Top,
                Height = 250, // Tăng chiều cao để không bị che
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Header
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = _cgvGold
            };
            
            Label lblHeader = new Label
            {
                Text = "📝 XIN NGHỈ PHÉP / BÁO CÁO LÝ DO VẮNG MẶT",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblHeader);

            // Content panel - sử dụng TableLayoutPanel cho layout tốt hơn
            Panel pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 250, 230),
                Padding = new Padding(15, 10, 15, 10)
            };

            // Row 1: Date + Shift selection (cùng hàng)
            Label lblNgay = new Label
            {
                Text = "📅 Chọn ngày:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblNgay);

            DateTimePicker dtpNgayNghi = new DateTimePicker
            {
                Location = new Point(120, 12),
                Width = 180,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy (ddd)",
                MinDate = DateTime.Today,
                Name = "dtpNgayNghi",
                Font = new Font("Segoe UI", 9)
            };
            pnlContent.Controls.Add(dtpNgayNghi);

            Label lblCa = new Label
            {
                Text = "⏰ Ca:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(320, 15),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblCa);

            ComboBox cboCa = new ComboBox
            {
                Location = new Point(370, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cboCa",
                Font = new Font("Segoe UI", 9)
            };
            cboCa.Items.AddRange(new[] { "Ca Sáng", "Ca Chiều", "Ca Tối", "Cả ngày" });
            cboCa.SelectedIndex = 0;
            pnlContent.Controls.Add(cboCa);

            // Tip - nhỏ hơn
            Label lblTip = new Label
            {
                Text = "💡 Click vào dòng lịch bên dưới để tự động điền ngày",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(510, 15),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblTip);

            // Row 2: Reason type
            Label lblLoai = new Label
            {
                Text = "📋 Loại:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 50),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblLoai);

            ComboBox cboLoai = new ComboBox
            {
                Location = new Point(120, 47),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cboLoai",
                Font = new Font("Segoe UI", 9)
            };
            cboLoai.Items.AddRange(new[] { 
                "🏥 Nghỉ ốm", 
                "📅 Nghỉ phép có lương", 
                "🏠 Việc gia đình", 
                "📚 Học tập/Thi cử",
                "🚗 Tai nạn/Sự cố",
                "📋 Khác" 
            });
            cboLoai.SelectedIndex = 0;
            pnlContent.Controls.Add(cboLoai);

            // Row 3: Reason detail
            Label lblLyDo = new Label
            {
                Text = "✏️ Chi tiết lý do:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 85),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblLyDo);

            TextBox txtLyDo = new TextBox
            {
                Location = new Point(120, 82),
                Width = 550,
                Height = 28,
                Name = "txtLyDo",
                Text = "Nhập chi tiết lý do xin nghỉ...",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10)
            };
            txtLyDo.Enter += (s, e) =>
            {
                if (txtLyDo.Text == "Nhập chi tiết lý do xin nghỉ...")
                {
                    txtLyDo.Text = "";
                    txtLyDo.ForeColor = Color.Black;
                }
            };
            txtLyDo.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtLyDo.Text))
                {
                    txtLyDo.Text = "Nhập chi tiết lý do xin nghỉ...";
                    txtLyDo.ForeColor = Color.Gray;
                }
            };
            pnlContent.Controls.Add(txtLyDo);

            // Row 4: Buttons
            Button btnGuiYeuCau = new Button
            {
                Text = "📤 GỬI YÊU CẦU",
                Size = new Size(150, 35),
                Location = new Point(120, 120),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGuiYeuCau.FlatAppearance.BorderSize = 0;
            btnGuiYeuCau.Click += (s, e) => GuiYeuCauXinNghi(dtpNgayNghi, cboCa, cboLoai, txtLyDo);
            pnlContent.Controls.Add(btnGuiYeuCau);

            Button btnXemLichSu = new Button
            {
                Text = "📜 Lịch sử yêu cầu",
                Size = new Size(140, 35),
                Location = new Point(285, 120),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXemLichSu.FlatAppearance.BorderSize = 0;
            btnXemLichSu.Click += (s, e) => XemLichSuYeuCau();
            pnlContent.Controls.Add(btnXemLichSu);

            // Note - compact
            Label lblNote = new Label
            {
                Text = "⚠️ Yêu cầu cần được duyệt trước 24h",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 50, 50),
                Location = new Point(450, 128),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblNote);

            // Add controls in correct order
            pnlXinNghi.Controls.Add(pnlContent);
            pnlXinNghi.Controls.Add(pnlHeader);
        }

        private void GuiYeuCauXinNghi(DateTimePicker dtp, ComboBox cboCa, ComboBox cboLoai, TextBox txtLyDo)
        {
            string lyDoText = txtLyDo.Text.Trim();
            if (string.IsNullOrWhiteSpace(lyDoText) || lyDoText == "Nhập chi tiết lý do xin nghỉ...")
            {
                MessageBox.Show("Vui lòng nhập chi tiết lý do xin nghỉ!", "Thiếu thông tin", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLyDo.Focus();
                return;
            }

            // Get EXACT date from DateTimePicker - store it for message display
            DateTime ngayNghiChon = dtp.Value.Date;
            string ca = cboCa.Text;
            string loai = cboLoai.Text;
            string lyDo = lyDoText;

            try
            {
                // Kiểm tra xem có ca làm trong ngày đó không
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    
                    // Insert vào bảng YeuCauNghi (nếu chưa có bảng thì tạo)
                    string checkTable = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='YeuCauNghi' AND xtype='U')
                        CREATE TABLE YeuCauNghi (
                            MaYeuCau INT PRIMARY KEY IDENTITY(1,1),
                            MaNguoiDung INT NOT NULL,
                            MaChiNhanh INT NOT NULL,
                            NgayNghi DATE NOT NULL,
                            CaLamViec NVARCHAR(50),
                            LoaiNghi NVARCHAR(100),
                            LyDo NVARCHAR(500),
                            TrangThai NVARCHAR(50) DEFAULT N'ChoDuyet',
                            NgayGui DATETIME DEFAULT GETDATE(),
                            NguoiDuyet INT NULL,
                            NgayDuyet DATETIME NULL,
                            GhiChuDuyet NVARCHAR(500) NULL
                        )";
                    
                    using (SqlCommand cmd = new SqlCommand(checkTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Insert yêu cầu
                    string insertQuery = @"
                        INSERT INTO YeuCauNghi (MaNguoiDung, MaChiNhanh, NgayNghi, CaLamViec, LoaiNghi, LyDo)
                        VALUES (@MaNguoiDung, @MaChiNhanh, @NgayNghi, @CaLamViec, @LoaiNghi, @LyDo)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNguoiDung", _maNguoiDung);
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@NgayNghi", ngayNghiChon);
                        cmd.Parameters.AddWithValue("@CaLamViec", ca);
                        cmd.Parameters.AddWithValue("@LoaiNghi", loai);
                        cmd.Parameters.AddWithValue("@LyDo", lyDo);
                        
                        cmd.ExecuteNonQuery();
                    }
                }

                // Show confirmation with EXACT date selected - no conversion
                MessageBox.Show($"Đã gửi yêu cầu xin nghỉ thành công!\n\n" +
                    $"📅 Ngày: {ngayNghiChon:dd/MM/yyyy}\n" +
                    $"⏰ Ca: {ca}\n" +
                    $"📋 Loại: {loai}\n\n" +
                    $"Yêu cầu đang chờ Quản lý phê duyệt.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form
                txtLyDo.Text = "Nhập chi tiết lý do xin nghỉ...";
                txtLyDo.ForeColor = Color.Gray;
                cboCa.SelectedIndex = 0;
                cboLoai.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi yêu cầu: " + ex.Message, "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void XemLichSuYeuCau()
        {
            Form frmLichSu = new Form
            {
                Text = "📋 Lịch sử yêu cầu xin nghỉ",
                Size = new Size(800, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };

            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = _cgvRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            FORMAT(NgayNghi, 'dd/MM/yyyy') as [Ngày nghỉ],
                            CaLamViec as [Ca],
                            LoaiNghi as [Loại],
                            LyDo as [Lý do],
                            CASE TrangThai 
                                WHEN 'ChoDuyet' THEN N'⏳ Chờ duyệt'
                                WHEN 'DaDuyet' THEN N'✅ Đã duyệt'
                                WHEN 'TuChoi' THEN N'❌ Từ chối'
                            END as [Trạng thái],
                            FORMAT(NgayGui, 'dd/MM/yyyy HH:mm') as [Ngày gửi],
                            GhiChuDuyet as [Ghi chú]
                        FROM YeuCauNghi
                        WHERE MaNguoiDung = @UserId
                        ORDER BY NgayGui DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", _maNguoiDung);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgv.DataSource = dt;
                    }
                }
            }
            catch
            {
                dgv.DataSource = null;
            }

            // Color rows based on status
            dgv.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    string status = row.Cells["Trạng thái"].Value?.ToString() ?? "";
                    if (status.Contains("Đã duyệt"))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(212, 237, 218);
                    }
                    else if (status.Contains("Từ chối"))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(248, 215, 218);
                    }
                    else if (status.Contains("Chờ"))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
                    }
                }
            };

            frmLichSu.Controls.Add(dgv);
            frmLichSu.ShowDialog();
        }

        private void CapNhatLabelTuan()
        {
            DateTime tuanKetThuc = _tuanBatDau.AddDays(6);
            lblTuan.Text = $"Tuần từ {_tuanBatDau:dd/MM/yyyy} đến {tuanKetThuc:dd/MM/yyyy}";
        }

        private void TaiLichLamViec()
        {
            // Nếu là nhân viên, chỉ lấy lịch của mình
            string filterCondition = _isStaff && _maNguoiDung > 0 
                ? "AND pc.MaNguoiDung = @MaNguoiDung"
                : "";

            string query = $@"
                SELECT 
                    nv.HoTen AS [Nhân viên],
                    cl.TenCa AS [Ca làm],
                    pc.NgayLamViec AS [Ngày],
                    FORMAT(pc.ThoiGianBatDau, 'HH:mm') AS [Giờ bắt đầu],
                    FORMAT(pc.ThoiGianKetThuc, 'HH:mm') AS [Giờ kết thúc],
                    pc.TrangThai AS [Trạng thái],
                    pc.TienDauCa AS [Tiền đầu ca],
                    pc.TienCuoiCa AS [Tiền cuối ca],
                    pc.TienThucTe AS [Tiền thực tế],
                    pc.ChenhLech AS [Chênh lệch]
                FROM PhanCongCa pc
                INNER JOIN NguoiDung nv ON pc.MaNguoiDung = nv.MaNguoiDung
                INNER JOIN CaLamViec cl ON pc.MaCa = cl.MaCa
                WHERE nv.MaChiNhanh = @MaChiNhanh
                  AND pc.NgayLamViec BETWEEN @TuBatDau AND @TuKetThuc
                  {filterCondition}
                ORDER BY pc.NgayLamViec, cl.GioBatDau, nv.HoTen";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuBatDau", _tuanBatDau);
                        cmd.Parameters.AddWithValue("@TuKetThuc", _tuanBatDau.AddDays(6));
                        
                        // Thêm parameter cho nhân viên nếu cần
                        if (_isStaff && _maNguoiDung > 0)
                        {
                            cmd.Parameters.AddWithValue("@MaNguoiDung", _maNguoiDung);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        _dtLichFull = dt.Copy(); // Lưu data đầy đủ
                        dgvLichCa.DataSource = dt;

                        // Populate combo lọc nhân viên (chỉ cho quản lý)
                        if (!_isStaff && cboLocNhanVien != null)
                        {
                            cboLocNhanVien.Items.Clear();
                            cboLocNhanVien.Items.Add("-- Tất cả --");
                            var uniqueNV = dt.AsEnumerable().Select(r => r["Nhân viên"].ToString()).Distinct().OrderBy(x => x);
                            foreach (var nv in uniqueNV)
                                cboLocNhanVien.Items.Add(nv);
                            cboLocNhanVien.SelectedIndex = 0;
                        }

                        // Format tiền tệ
                        foreach (DataGridViewColumn col in dgvLichCa.Columns)
                        {
                            if (col.Name.Contains("Tiền") || col.Name.Contains("Chênh"))
                            {
                                col.DefaultCellStyle.Format = "N0";
                                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                            }
                        }

                        // Màu trạng thái
                        foreach (DataGridViewRow row in dgvLichCa.Rows)
                        {
                            string trangThai = row.Cells["Trạng thái"].Value?.ToString();
                            switch (trangThai)
                            {
                                case "DaKetThuc":
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(212, 237, 218);
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(21, 87, 36);
                                    break;
                                case "DangLam":
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(146, 86, 0);
                                    break;
                                case "ChuaBatDau":
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(225, 236, 244);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch làm việc:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTuanTruoc_Click(object sender, EventArgs e)
        {
            _tuanBatDau = _tuanBatDau.AddDays(-7);
            CapNhatLabelTuan();
            TaiLichLamViec();
        }

        private void BtnTuanSau_Click(object sender, EventArgs e)
        {
            _tuanBatDau = _tuanBatDau.AddDays(7);
            CapNhatLabelTuan();
            TaiLichLamViec();
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            TinhTuanHienTai();
            CapNhatLabelTuan();
            TaiLichLamViec();
        }

        private void ApDungBoLoc()
        {
            if (_dtLichFull == null) return;

            string filterNV = cboLocNhanVien.SelectedIndex > 0 ? cboLocNhanVien.SelectedItem.ToString() : "";
            string filterCa = cboLocCa.SelectedIndex > 0 ? cboLocCa.SelectedItem.ToString() : "";

            DataTable dtFiltered = _dtLichFull.Clone();

            foreach (DataRow row in _dtLichFull.Rows)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(filterNV) && row["Nhân viên"].ToString() != filterNV)
                    match = false;

                if (!string.IsNullOrEmpty(filterCa))
                {
                    string tenCa = row["Ca làm"].ToString().ToLower();
                    if (filterCa == "Ca sáng" && !tenCa.Contains("sáng"))
                        match = false;
                    else if (filterCa == "Ca chiều" && !tenCa.Contains("chiều"))
                        match = false;
                    else if (filterCa == "Ca tối" && !tenCa.Contains("tối") && !tenCa.Contains("đêm"))
                        match = false;
                }

                if (match)
                    dtFiltered.ImportRow(row);
            }

            dgvLichCa.DataSource = dtFiltered;
        }
    }
}