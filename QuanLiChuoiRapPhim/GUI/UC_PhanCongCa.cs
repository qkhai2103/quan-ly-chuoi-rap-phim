// File: UC_PhanCongCa.cs
// Chức năng: PHÂN CÔNG CA LÀM VIỆC cho nhân viên chi nhánh (dành cho Quản lý)
// Vị trí: QuanLiChuoiRapPhim.GUI

using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_PhanCongCa : UserControl
    {
        private readonly int _maChiNhanh;
        private DateTime _tuanBatDau; // Thứ Hai của tuần hiện tại
        private DateTime _ngayHienTai = DateTime.Now;

        private DataGridView dgvPhanCong;
        private Label lblTuan, lblSoLuongChon;
        private Button btnTuanTruoc, btnTuanSau, btnLamMoi, btnPhanCongTuDong, btnLuuPhanCong, btnXoaChon, btnChonTatCa;
        private ComboBox cboNhanVien, cboCa, cboLocNhanVien, cboLocCa;
        private DateTimePicker dtpNgayLam;
        private DataTable _dtNhanVien, _dtCaLamViec, _dtPhanCongFull;

        public UC_PhanCongCa(int maChiNhanh)
        {
            _maChiNhanh = maChiNhanh;
            TinhTuanHienTai();
            ThietLapGiaoDien();
            TaiDanhSachNhanVien();
            TaiDanhSachCaLamViec();
            TaiLichPhanCong();
        }

        private void TinhTuanHienTai()
        {
            int diff = (int)_ngayHienTai.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            _tuanBatDau = _ngayHienTai.AddDays(-diff);
        }

        private void ThietLapGiaoDien()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // === HEADER ===
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(76, 175, 80) // Màu xanh lá
            };
            Label lblTitle = new Label
            {
                Text = "PHÂN CÔNG CA LÀM VIỆC",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblTitle);

            // === TOOLBAR TUẦN ===
            Panel pnlTuan = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            btnTuanTruoc = CreateButton("◄ Tuần trước", 10, Color.FromArgb(108, 117, 125));
            btnTuanTruoc.Click += (s, e) => { _tuanBatDau = _tuanBatDau.AddDays(-7); CapNhatLabelTuan(); TaiLichPhanCong(); };

            lblTuan = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(140, 18),
                ForeColor = Color.FromArgb(23, 32, 42)
            };
            CapNhatLabelTuan();

            btnTuanSau = CreateButton("Tuần sau ►", 360, Color.FromArgb(40, 167, 69));
            btnTuanSau.Click += (s, e) => { _tuanBatDau = _tuanBatDau.AddDays(7); CapNhatLabelTuan(); TaiLichPhanCong(); };

            btnLamMoi = CreateButton("🔄 Làm mới", 490, Color.FromArgb(0, 123, 255));
            btnLamMoi.Click += (s, e) => TaiLichPhanCong();

            btnPhanCongTuDong = CreateButton("🤖 Phân công tự động", 610, Color.FromArgb(156, 39, 176));
            btnPhanCongTuDong.Click += BtnPhanCongTuDong_Click;

            pnlTuan.Controls.AddRange(new Control[] { btnTuanTruoc, lblTuan, btnTuanSau, btnLamMoi, btnPhanCongTuDong });

            // === BUTTON THÊM PHÂN CÔNG ===
            Panel pnlAddButton = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            btnLuuPhanCong = new Button
            {
                Text = "➕ Thêm phân công",
                Width = 180,
                Height = 40,
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLuuPhanCong.FlatAppearance.BorderSize = 0;
            btnLuuPhanCong.Click += (s, e) => ShowAddAssignmentForm();

            pnlAddButton.Controls.Add(btnLuuPhanCong);

            // === PANEL LỌC VÀ QUẢN LÝ ===
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10)
            };

            Label lblLocNV = new Label { Text = "Lọc NV:", Location = new Point(10, 18), AutoSize = true };
            cboLocNhanVien = new ComboBox { Width = 180, Location = new Point(70, 15), DropDownStyle = ComboBoxStyle.DropDownList };
            cboLocNhanVien.Items.Add("-- Tất cả nhân viên --");
            cboLocNhanVien.SelectedIndex = 0;
            cboLocNhanVien.SelectedIndexChanged += ApDungBoLoc;

            Label lblLocCa = new Label { Text = "Lọc ca:", Location = new Point(270, 18), AutoSize = true };
            cboLocCa = new ComboBox { Width = 150, Location = new Point(330, 15), DropDownStyle = ComboBoxStyle.DropDownList };
            cboLocCa.Items.AddRange(new[] { "-- Tất cả ca --", "Ca sáng", "Ca chiều", "Ca tối" });
            cboLocCa.SelectedIndex = 0;
            cboLocCa.SelectedIndexChanged += ApDungBoLoc;

            btnChonTatCa = new Button
            {
                Text = "☑ Chọn tất cả",
                Width = 120,
                Height = 30,
                Location = new Point(500, 13),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            btnChonTatCa.Click += BtnChonTatCa_Click;

            btnXoaChon = new Button
            {
                Text = "🗑️ Xóa đã chọn",
                Width = 120,
                Height = 30,
                Location = new Point(630, 13),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnXoaChon.Click += BtnXoaChon_Click;

            lblSoLuongChon = new Label
            {
                Text = "Đã chọn: 0",
                Location = new Point(760, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60)
            };

            pnlFilter.Controls.AddRange(new Control[] { lblLocNV, cboLocNhanVien, lblLocCa, cboLocCa, btnChonTatCa, btnXoaChon, lblSoLuongChon });

            // === DATAGRIDVIEW ===
            dgvPhanCong = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };
            dgvPhanCong.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(76, 175, 80);
            dgvPhanCong.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPhanCong.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPhanCong.EnableHeadersVisualStyles = false;
            dgvPhanCong.CellContentClick += DgvPhanCong_CellContentClick;
            dgvPhanCong.CellValueChanged += DgvPhanCong_CellValueChanged;
            dgvPhanCong.CurrentCellDirtyStateChanged += DgvPhanCong_CurrentCellDirtyStateChanged;

            this.Controls.Add(dgvPhanCong);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlAddButton);
            this.Controls.Add(pnlTuan);
            this.Controls.Add(pnlHeader);
        }

        private Button CreateButton(string text, int x, Color backColor)
        {
            return new Button
            {
                Text = text,
                Width = text.Contains("tự động") ? 180 : 120,
                Height = 35,
                Location = new Point(x, 12),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
        }

        private void CapNhatLabelTuan()
        {
            DateTime tuanKetThuc = _tuanBatDau.AddDays(6);
            lblTuan.Text = $"Tuần {_tuanBatDau:dd/MM} - {tuanKetThuc:dd/MM/yyyy}";
        }

        private void TaiDanhSachNhanVien()
        {
            string query = @"SELECT MaNguoiDung, HoTen FROM NguoiDung 
                            WHERE MaChiNhanh = @MaChiNhanh AND VaiTro = N'NhanVien' AND TrangThai = 1";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtNhanVien = new DataTable();
                        da.Fill(_dtNhanVien);

                        // Populate combo lọc
                        if (cboLocNhanVien != null)
                        {
                            cboLocNhanVien.Items.Clear();
                            cboLocNhanVien.Items.Add("-- Tất cả nhân viên --");
                            foreach (DataRow row in _dtNhanVien.Rows)
                            {
                                cboLocNhanVien.Items.Add(row["HoTen"].ToString());
                            }
                            cboLocNhanVien.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách nhân viên: " + ex.Message);
            }
        }

        private void TaiDanhSachCaLamViec()
        {
            string query = @"SELECT MaCa, TenCa, GioBatDau, GioKetThuc 
                            FROM CaLamViec WHERE TrangThai = 1 ORDER BY GioBatDau";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        _dtCaLamViec = new DataTable();
                        da.Fill(_dtCaLamViec);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách ca làm việc: " + ex.Message);
            }
        }

        private void TaiLichPhanCong()
        {
            string query = @"
                SELECT 
                    pc.MaPhanCong,
                    nv.HoTen AS [Nhân viên],
                    pc.NgayLamViec AS [Ngày],
                    cl.TenCa AS [Ca làm],
                    FORMAT(pc.ThoiGianBatDau, 'HH:mm') AS [Giờ vào],
                    FORMAT(pc.ThoiGianKetThuc, 'HH:mm') AS [Giờ ra],
                    pc.TrangThai AS [Trạng thái]
                FROM PhanCongCa pc
                INNER JOIN NguoiDung nv ON pc.MaNguoiDung = nv.MaNguoiDung
                INNER JOIN CaLamViec cl ON pc.MaCa = cl.MaCa
                WHERE nv.MaChiNhanh = @MaChiNhanh
                  AND pc.NgayLamViec BETWEEN @TuBatDau AND @TuKetThuc
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

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        _dtPhanCongFull = dt.Copy(); // Lưu data đầy đủ để lọc
                        dgvPhanCong.DataSource = dt;

                        // Thêm checkbox column
                        if (!dgvPhanCong.Columns.Contains("chkChon"))
                        {
                            DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn
                            {
                                Name = "chkChon",
                                HeaderText = "☐",
                                Width = 40,
                                ReadOnly = false
                            };
                            dgvPhanCong.Columns.Insert(0, chkCol);
                        }

                        // Ẩn cột MaPhanCong
                        if (dgvPhanCong.Columns.Contains("MaPhanCong"))
                            dgvPhanCong.Columns["MaPhanCong"].Visible = false;

                        // Thêm button Xóa
                        if (!dgvPhanCong.Columns.Contains("btnXoa"))
                        {
                            DataGridViewButtonColumn btnXoa = new DataGridViewButtonColumn
                            {
                                Name = "btnXoa",
                                HeaderText = "Thao tác",
                                Text = "🗑️ Xóa",
                                UseColumnTextForButtonValue = true,
                                Width = 100
                            };
                            dgvPhanCong.Columns.Add(btnXoa);
                        }

                        // Format date
                        if (dgvPhanCong.Columns.Contains("Ngày"))
                            dgvPhanCong.Columns["Ngày"].DefaultCellStyle.Format = "dd/MM/yyyy (ddd)";

                        CapNhatSoLuongChon();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch phân công: " + ex.Message);
            }
        }

        /// <summary>
        /// Hiển thị modal form để thêm phân công mới
        /// </summary>
        private void ShowAddAssignmentForm()
        {
            Form frmAdd = new Form
            {
                Text = "➕ Thêm phân công ca làm việc",
                Size = new Size(500, 320),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            int y = 20;

            // Nhân viên
            Label lblNV = new Label
            {
                Text = "Nhân viên:",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            ComboBox cboNV = new ComboBox
            {
                Width = 350,
                Location = new Point(120, y - 3),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                DisplayMember = "HoTen",
                ValueMember = "MaNguoiDung",
                DataSource = _dtNhanVien
            };
            y += 50;

            // Ngày làm
            Label lblNgay = new Label
            {
                Text = "Ngày làm:",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            DateTimePicker dtpNgay = new DateTimePicker
            {
                Width = 350,
                Location = new Point(120, y - 3),
                Format = DateTimePickerFormat.Long,
                Font = new Font("Segoe UI", 10),
                Value = DateTime.Now // Default to current date instead of week start
            };
            y += 50;

            // Ca làm việc
            Label lblCa = new Label
            {
                Text = "Ca làm việc:",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            ComboBox cboCaModal = new ComboBox
            {
                Width = 350,
                Location = new Point(120, y - 3),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                DisplayMember = "TenCa",
                ValueMember = "MaCa",
                DataSource = _dtCaLamViec.Copy() // Use copy to avoid data binding conflicts
            };
            y += 60;

            // Buttons
            Button btnOK = new Button
            {
                Text = "✔ Thêm",
                Size = new Size(120, 40),
                Location = new Point(200, y),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += (s, e) =>
            {
                if (cboNV.SelectedValue == null || cboCaModal.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn đầy đủ thông tin!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int maNguoiDung = Convert.ToInt32(cboNV.SelectedValue);
                int maCa = Convert.ToInt32(cboCaModal.SelectedValue);
                DateTime ngayLam = dtpNgay.Value.Date;

                // Get shift times
                DataRow caInfo = _dtCaLamViec.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["MaCa"]) == maCa);
                if (caInfo == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin ca làm việc!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                TimeSpan gioBatDau = (TimeSpan)caInfo["GioBatDau"];
                TimeSpan gioKetThuc = (TimeSpan)caInfo["GioKetThuc"];
                DateTime thoiGianBatDau = ngayLam.Add(gioBatDau);
                DateTime thoiGianKetThuc = ngayLam.Add(gioKetThuc);

                string query = @"
                    INSERT INTO PhanCongCa (MaNguoiDung, MaCa, NgayLamViec, ThoiGianBatDau, ThoiGianKetThuc, TrangThai)
                    VALUES (@MaNguoiDung, @MaCa, @NgayLam, @ThoiGianBatDau, @ThoiGianKetThuc, N'ChuaBatDau')";

                try
                {
                    using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                            cmd.Parameters.AddWithValue("@MaCa", maCa);
                            cmd.Parameters.AddWithValue("@NgayLam", ngayLam);
                            cmd.Parameters.AddWithValue("@ThoiGianBatDau", thoiGianBatDau);
                            cmd.Parameters.AddWithValue("@ThoiGianKetThuc", thoiGianKetThuc);

                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Thêm phân công thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            frmAdd.DialogResult = DialogResult.OK;
                            frmAdd.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thêm phân công: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnCancel = new Button
            {
                Text = "✖ Hủy",
                Size = new Size(100, 40),
                Location = new Point(330, y),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            frmAdd.Controls.AddRange(new Control[] { lblNV, cboNV, lblNgay, dtpNgay, lblCa, cboCaModal, btnOK, btnCancel });

            if (frmAdd.ShowDialog() == DialogResult.OK)
            {
                TaiLichPhanCong(); // Refresh grid
            }
        }

        private void BtnPhanCongTuDong_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Phân công tự động cho tuần từ {_tuanBatDau:dd/MM} đến {_tuanBatDau.AddDays(6):dd/MM}?\n\n" +
                "Lưu ý: Sẽ phân đều nhân viên cho các ca trong tuần.",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;
            btnPhanCongTuDong.Enabled = false;
            btnPhanCongTuDong.Text = "⏳ Đang xử lý...";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // Xóa phân công cũ trong tuần này
                        string deleteQuery = @"DELETE FROM PhanCongCa WHERE MaNguoiDung IN 
                            (SELECT MaNguoiDung FROM NguoiDung WHERE MaChiNhanh = @MaChiNhanh)
                            AND NgayLamViec BETWEEN @TuBatDau AND @TuKetThuc";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                            cmd.Parameters.AddWithValue("@TuBatDau", _tuanBatDau);
                            cmd.Parameters.AddWithValue("@TuKetThuc", _tuanBatDau.AddDays(6));
                            cmd.ExecuteNonQuery();
                        }

                        // Phân công tự động với BATCH INSERT
                        int soNhanVien = _dtNhanVien.Rows.Count;
                        int soCa = _dtCaLamViec.Rows.Count;
                        if (soNhanVien == 0 || soCa == 0)
                        {
                            transaction.Rollback();
                            return;
                        }

                        // Tạo VALUES string cho batch insert
                        var valuesList = new System.Collections.Generic.List<string>();
                        var parameters = new System.Collections.Generic.List<SqlParameter>();
                        int paramIndex = 0;

                        int nhanVienIndex = 0;
                        for (int ngay = 0; ngay < 7; ngay++)
                        {
                            DateTime ngayLam = _tuanBatDau.AddDays(ngay);

                            foreach (DataRow caRow in _dtCaLamViec.Rows)
                            {
                                int maCa = Convert.ToInt32(caRow["MaCa"]);
                                TimeSpan gioBatDau = (TimeSpan)caRow["GioBatDau"];
                                TimeSpan gioKetThuc = (TimeSpan)caRow["GioKetThuc"];

                                // Phân 2 nhân viên cho mỗi ca
                                for (int i = 0; i < 2; i++)
                                {
                                    if (nhanVienIndex >= soNhanVien) nhanVienIndex = 0;
                                    int maNguoiDung = Convert.ToInt32(_dtNhanVien.Rows[nhanVienIndex]["MaNguoiDung"]);

                                    valuesList.Add($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, N'ChuaBatDau')");
                                    parameters.Add(new SqlParameter($"@p{paramIndex}", maNguoiDung));
                                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", maCa));
                                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", ngayLam));
                                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", ngayLam.Add(gioBatDau)));
                                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", ngayLam.Add(gioKetThuc)));
                                    paramIndex += 5;

                                    nhanVienIndex++;

                                    // Batch 100 records at a time
                                    if (valuesList.Count >= 100)
                                    {
                                        string batchInsert = $"INSERT INTO PhanCongCa (MaNguoiDung, MaCa, NgayLamViec, ThoiGianBatDau, ThoiGianKetThuc, TrangThai) VALUES {string.Join(", ", valuesList)}";
                                        using (SqlCommand cmd = new SqlCommand(batchInsert, conn, transaction))
                                        {
                                            cmd.Parameters.AddRange(parameters.ToArray());
                                            cmd.ExecuteNonQuery();
                                        }
                                        valuesList.Clear();
                                        parameters.Clear();
                                        paramIndex = 0;
                                    }
                                }
                            }
                        }

                        // Insert remaining records
                        if (valuesList.Count > 0)
                        {
                            string batchInsert = $"INSERT INTO PhanCongCa (MaNguoiDung, MaCa, NgayLamViec, ThoiGianBatDau, ThoiGianKetThuc, TrangThai) VALUES {string.Join(", ", valuesList)}";
                            using (SqlCommand cmd = new SqlCommand(batchInsert, conn, transaction))
                            {
                                cmd.Parameters.AddRange(parameters.ToArray());
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("Phân công tự động thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        TaiLichPhanCong();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi phân công tự động: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPhanCongTuDong.Enabled = true;
                btnPhanCongTuDong.Text = "🤖 Phân công tự động";
                Cursor.Current = Cursors.Default;
            }
        }

        private void DgvPhanCong_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvPhanCong.Columns[e.ColumnIndex].Name != "btnXoa") return;

            if (MessageBox.Show("Xác nhận xóa phân công này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int maPhanCong = Convert.ToInt32(dgvPhanCong.Rows[e.RowIndex].Cells["MaPhanCong"].Value);

            string query = "DELETE FROM PhanCongCa WHERE MaPhanCong = @MaPhanCong";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaPhanCong", maPhanCong);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        TaiLichPhanCong();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApDungBoLoc(object sender, EventArgs e)
        {
            if (_dtPhanCongFull == null) return;

            string filterNV = cboLocNhanVien.SelectedIndex > 0 ? cboLocNhanVien.SelectedItem.ToString() : "";
            string filterCa = cboLocCa.SelectedIndex > 0 ? cboLocCa.SelectedItem.ToString() : "";

            DataTable dtFiltered = _dtPhanCongFull.Clone();

            foreach (DataRow row in _dtPhanCongFull.Rows)
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

            dgvPhanCong.DataSource = dtFiltered;

            // Re-add columns if needed
            if (!dgvPhanCong.Columns.Contains("chkChon"))
            {
                DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn
                {
                    Name = "chkChon",
                    HeaderText = "☐",
                    Width = 40,
                    ReadOnly = false
                };
                dgvPhanCong.Columns.Insert(0, chkCol);
            }

            if (dgvPhanCong.Columns.Contains("MaPhanCong"))
                dgvPhanCong.Columns["MaPhanCong"].Visible = false;

            if (!dgvPhanCong.Columns.Contains("btnXoa"))
            {
                DataGridViewButtonColumn btnXoa = new DataGridViewButtonColumn
                {
                    Name = "btnXoa",
                    HeaderText = "Thao tác",
                    Text = "🗑️ Xóa",
                    UseColumnTextForButtonValue = true,
                    Width = 100
                };
                dgvPhanCong.Columns.Add(btnXoa);
            }

            CapNhatSoLuongChon();
        }

        private void BtnChonTatCa_Click(object sender, EventArgs e)
        {
            bool selectAll = btnChonTatCa.Text.Contains("Chọn tất cả");

            foreach (DataGridViewRow row in dgvPhanCong.Rows)
            {
                row.Cells["chkChon"].Value = selectAll;
            }

            btnChonTatCa.Text = selectAll ? "☐ Bỏ chọn tất cả" : "☑ Chọn tất cả";
            CapNhatSoLuongChon();
        }

        private void BtnXoaChon_Click(object sender, EventArgs e)
        {
            var selectedIds = new System.Collections.Generic.List<int>();

            foreach (DataGridViewRow row in dgvPhanCong.Rows)
            {
                if (row.Cells["chkChon"].Value != null && Convert.ToBoolean(row.Cells["chkChon"].Value))
                {
                    selectedIds.Add(Convert.ToInt32(row.Cells["MaPhanCong"].Value));
                }
            }

            if (selectedIds.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 dòng để xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Xác nhận xóa {selectedIds.Count} phân công đã chọn?", "Xác nhận", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;
            btnXoaChon.Enabled = false;
            btnXoaChon.Text = "⏳ Đang xóa...";

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    
                    // Xóa hàng loạt bằng IN clause - NHANH HỌN NHIỀU!
                    string ids = string.Join(",", selectedIds);
                    string query = $"DELETE FROM PhanCongCa WHERE MaPhanCong IN ({ids})";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int deleted = cmd.ExecuteNonQuery();
                        MessageBox.Show($"Đã xóa {deleted} phân công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    TaiLichPhanCong();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa hàng loạt: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnXoaChon.Enabled = true;
                btnXoaChon.Text = "🗑️ Xóa đã chọn";
                Cursor.Current = Cursors.Default;
            }
        }

        private void DgvPhanCong_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgvPhanCong.Columns[e.ColumnIndex].Name == "chkChon")
            {
                CapNhatSoLuongChon();
            }
        }

        private void DgvPhanCong_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvPhanCong.IsCurrentCellDirty && dgvPhanCong.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvPhanCong.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void CapNhatSoLuongChon()
        {
            int count = 0;
            foreach (DataGridViewRow row in dgvPhanCong.Rows)
            {
                if (row.Cells["chkChon"].Value != null && Convert.ToBoolean(row.Cells["chkChon"].Value))
                    count++;
            }
            lblSoLuongChon.Text = $"Đã chọn: {count}";
        }
    }
}
