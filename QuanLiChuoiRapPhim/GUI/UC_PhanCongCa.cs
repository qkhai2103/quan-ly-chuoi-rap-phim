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
        private Label lblTuan;
        private Button btnTuanTruoc, btnTuanSau, btnLamMoi, btnPhanCongTuDong, btnLuuPhanCong;
        private ComboBox cboNhanVien, cboCa;
        private DateTimePicker dtpNgayLam;
        private DataTable _dtNhanVien, _dtCaLamViec;

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

            // === FORM PHÂN CÔNG ===
            Panel pnlForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            Label lblNV = new Label { Text = "Nhân viên:", Location = new Point(10, 18), AutoSize = true };
            cboNhanVien = new ComboBox { Width = 200, Location = new Point(90, 15), DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblNgay = new Label { Text = "Ngày:", Location = new Point(310, 18), AutoSize = true };
            dtpNgayLam = new DateTimePicker { Width = 150, Location = new Point(360, 15), Format = DateTimePickerFormat.Short };
            dtpNgayLam.Value = _tuanBatDau;

            Label lblCa = new Label { Text = "Ca:", Location = new Point(530, 18), AutoSize = true };
            cboCa = new ComboBox { Width = 180, Location = new Point(570, 15), DropDownStyle = ComboBoxStyle.DropDownList };

            btnLuuPhanCong = new Button
            {
                Text = "➕ Thêm phân công",
                Width = 150,
                Height = 35,
                Location = new Point(770, 13),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLuuPhanCong.Click += BtnLuuPhanCong_Click;

            pnlForm.Controls.AddRange(new Control[] { lblNV, cboNhanVien, lblNgay, dtpNgayLam, lblCa, cboCa, btnLuuPhanCong });

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

            this.Controls.Add(dgvPhanCong);
            this.Controls.Add(pnlForm);
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

                        cboNhanVien.DisplayMember = "HoTen";
                        cboNhanVien.ValueMember = "MaNguoiDung";
                        cboNhanVien.DataSource = _dtNhanVien;
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

                        cboCa.DisplayMember = "TenCa";
                        cboCa.ValueMember = "MaCa";
                        cboCa.DataSource = _dtCaLamViec;
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

                        dgvPhanCong.DataSource = dt;

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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch phân công: " + ex.Message);
            }
        }

        private void BtnLuuPhanCong_Click(object sender, EventArgs e)
        {
            if (cboNhanVien.SelectedValue == null || cboCa.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên và ca làm việc!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maNguoiDung = Convert.ToInt32(cboNhanVien.SelectedValue);
            int maCa = Convert.ToInt32(cboCa.SelectedValue);
            DateTime ngayLam = dtpNgayLam.Value.Date;

            // Lấy giờ bắt đầu và kết thúc từ CaLamViec
            DataRow caInfo = _dtCaLamViec.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["MaCa"]) == maCa);
            if (caInfo == null) return;

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
                        MessageBox.Show("Thêm phân công thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        TaiLichPhanCong();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm phân công: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPhanCongTuDong_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Phân công tự động cho tuần từ {_tuanBatDau:dd/MM} đến {_tuanBatDau.AddDays(6):dd/MM}?\n\n" +
                "Lưu ý: Sẽ phân đều nhân viên cho các ca trong tuần.",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();

                    // Xóa phân công cũ trong tuần này
                    string deleteQuery = @"DELETE FROM PhanCongCa WHERE MaNguoiDung IN 
                        (SELECT MaNguoiDung FROM NguoiDung WHERE MaChiNhanh = @MaChiNhanh)
                        AND NgayLamViec BETWEEN @TuBatDau AND @TuKetThuc";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", _maChiNhanh);
                        cmd.Parameters.AddWithValue("@TuBatDau", _tuanBatDau);
                        cmd.Parameters.AddWithValue("@TuKetThuc", _tuanBatDau.AddDays(6));
                        cmd.ExecuteNonQuery();
                    }

                    // Phân công tự động
                    int soNhanVien = _dtNhanVien.Rows.Count;
                    int soCa = _dtCaLamViec.Rows.Count;
                    if (soNhanVien == 0 || soCa == 0) return;

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

                                string insertQuery = @"INSERT INTO PhanCongCa (MaNguoiDung, MaCa, NgayLamViec, ThoiGianBatDau, ThoiGianKetThuc, TrangThai)
                                    VALUES (@MaNguoiDung, @MaCa, @NgayLam, @ThoiGianBatDau, @ThoiGianKetThuc, N'ChuaBatDau')";

                                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                                    cmd.Parameters.AddWithValue("@MaCa", maCa);
                                    cmd.Parameters.AddWithValue("@NgayLam", ngayLam);
                                    cmd.Parameters.AddWithValue("@ThoiGianBatDau", ngayLam.Add(gioBatDau));
                                    cmd.Parameters.AddWithValue("@ThoiGianKetThuc", ngayLam.Add(gioKetThuc));
                                    cmd.ExecuteNonQuery();
                                }

                                nhanVienIndex++;
                            }
                        }
                    }

                    MessageBox.Show("Phân công tự động thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TaiLichPhanCong();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi phân công tự động: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
