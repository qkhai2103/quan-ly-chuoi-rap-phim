// File: UC_DeXuatLichChieu.cs
// UserControl cho Quản lý chi nhánh đề xuất lịch chiếu lên Admin

using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.GUI
{
    public class UC_DeXuatLichChieu : UserControl
    {
        // CGV Branding Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

        private int _maChiNhanh;
        private int _userId;
        private string _branchName;
        private DataGridView _dgvDeXuat;
        private DataGridView _dgvPhimDoanhThuThap;
        private DataTable _dtDeXuat;
        private DeXuatLichChieuDAL _deXuatDAL;
        private PhimDAL _phimDAL;

        public UC_DeXuatLichChieu(int maChiNhanh, int userId, string branchName)
        {
            _maChiNhanh = maChiNhanh;
            _userId = userId;
            _branchName = branchName;
            _deXuatDAL = new DeXuatLichChieuDAL();
            _phimDAL = new PhimDAL();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(20);

            // Header
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            AddRoundedCorners(headerPanel, 10);

            Label lblTitle = new Label
            {
                Text = "[*] DE XUAT LICH CHIEU CHO ADMIN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Location = new Point(20, 25)
            };
            headerPanel.Controls.Add(lblTitle);

            Label lblBranch = new Label
            {
                Text = $"Chi nhánh: {_branchName}",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(500, 30)
            };
            headerPanel.Controls.Add(lblBranch);

            // Split container for 2 sections
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 350,
                Panel1MinSize = 200,
                Panel2MinSize = 150,
                BackColor = Color.Transparent
            };

            // Top Panel: Phim có doanh thu thấp
            Panel topPanel = CreatePhimDoanhThuThapPanel();
            splitContainer.Panel1.Controls.Add(topPanel);

            // Bottom Panel: Danh sách đề xuất đã gửi
            Panel bottomPanel = CreateDeXuatDaGuiPanel();
            splitContainer.Panel2.Controls.Add(bottomPanel);

            this.Controls.Add(splitContainer);
            this.Controls.Add(headerPanel);
        }

        private Panel CreatePhimDoanhThuThapPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            AddRoundedCorners(panel, 10);

            Label lblTitle = new Label
            {
                Text = "[!] PHIM CO DOANH THU THAP (Can xem xet giam suat hoac xoa)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                Dock = DockStyle.Top,
                Height = 35
            };

            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.Transparent
            };

            Button btnRefresh = new Button
            {
                Text = "Lam moi",
                Size = new Size(100, 30),
                Location = new Point(0, 8),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadPhimDoanhThuThap();
            toolbar.Controls.Add(btnRefresh);

            Button btnDeXuatGiam = new Button
            {
                Text = "De xuat giam suat",
                Size = new Size(140, 30),
                Location = new Point(110, 8),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeXuatGiam.FlatAppearance.BorderSize = 0;
            btnDeXuatGiam.Click += (s, e) => TaoDeXuat("GiamSuatChieu");
            toolbar.Controls.Add(btnDeXuatGiam);

            Button btnDeXuatXoa = new Button
            {
                Text = "De xuat xoa phim",
                Size = new Size(140, 30),
                Location = new Point(260, 8),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeXuatXoa.FlatAppearance.BorderSize = 0;
            btnDeXuatXoa.Click += (s, e) => TaoDeXuat("XoaPhim");
            toolbar.Controls.Add(btnDeXuatXoa);

            _dgvPhimDoanhThuThap = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = 40 }
            };
            StyleDataGridView(_dgvPhimDoanhThuThap);

            panel.Controls.Add(_dgvPhimDoanhThuThap);
            panel.Controls.Add(toolbar);
            panel.Controls.Add(lblTitle);

            return panel;
        }

        private Panel CreateDeXuatDaGuiPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            AddRoundedCorners(panel, 10);

            Label lblTitle2 = new Label
            {
                Text = "[#] DE XUAT DA GUI",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 35
            };

            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.Transparent
            };

            ComboBox cboFilter = new ComboBox
            {
                Width = 150,
                Location = new Point(0, 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboFilter.Items.AddRange(new[] { "Tất cả", "Chờ duyệt", "Đã duyệt", "Từ chối" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += (s, e) => FilterDeXuat(cboFilter.SelectedItem.ToString());
            toolbar.Controls.Add(cboFilter);

            Button btnHuy = new Button
            {
                Text = "Huy de xuat",
                Size = new Size(120, 30),
                Location = new Point(160, 8),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnHuy.FlatAppearance.BorderSize = 0;
            btnHuy.Click += (s, e) => HuyDeXuat();
            toolbar.Controls.Add(btnHuy);

            _dgvDeXuat = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = 40 }
            };
            StyleDataGridView(_dgvDeXuat);
            _dgvDeXuat.CellFormatting += DgvDeXuat_CellFormatting;

            panel.Controls.Add(_dgvDeXuat);
            panel.Controls.Add(toolbar);
            panel.Controls.Add(lblTitle2);

            return panel;
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersDefaultCellStyle.BackColor = _cgvBlack;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        }

        private void LoadData()
        {
            LoadPhimDoanhThuThap();
            LoadDeXuatDaGui();
        }

        private void LoadPhimDoanhThuThap()
        {
            try
            {
                DataTable dt = _deXuatDAL.LayPhimDoanhThuThap(_maChiNhanh);
                _dgvPhimDoanhThuThap.DataSource = dt;

                if (_dgvPhimDoanhThuThap.Columns.Count > 0)
                {
                    _dgvPhimDoanhThuThap.Columns["MaPhim"].HeaderText = "Mã";
                    _dgvPhimDoanhThuThap.Columns["MaPhim"].Width = 50;
                    _dgvPhimDoanhThuThap.Columns["TenPhim"].HeaderText = "Tên phim";
                    _dgvPhimDoanhThuThap.Columns["TheLoai"].HeaderText = "Thể loại";
                    _dgvPhimDoanhThuThap.Columns["SoSuatChieu"].HeaderText = "Số suất";
                    _dgvPhimDoanhThuThap.Columns["SoVeBan"].HeaderText = "Vé bán";
                    _dgvPhimDoanhThuThap.Columns["DoanhThu"].HeaderText = "Doanh thu";
                    _dgvPhimDoanhThuThap.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                    _dgvPhimDoanhThuThap.Columns["TyLeLapDay"].HeaderText = "% Lấp đầy";
                    _dgvPhimDoanhThuThap.Columns["TyLeLapDay"].DefaultCellStyle.Format = "N1";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading low revenue movies: {ex.Message}");
            }
        }

        private void LoadDeXuatDaGui()
        {
            try
            {
                _dtDeXuat = _deXuatDAL.LayDeXuatTheoChiNhanh(_maChiNhanh);
                _dgvDeXuat.DataSource = _dtDeXuat;

                if (_dgvDeXuat.Columns.Count > 0)
                {
                    _dgvDeXuat.Columns["MaDeXuat"].HeaderText = "Mã";
                    _dgvDeXuat.Columns["MaDeXuat"].Width = 50;
                    _dgvDeXuat.Columns["TenPhim"].HeaderText = "Phim";
                    _dgvDeXuat.Columns["LoaiDeXuat"].HeaderText = "Loại đề xuất";
                    _dgvDeXuat.Columns["LyDo"].HeaderText = "Lý do";
                    _dgvDeXuat.Columns["TrangThai"].HeaderText = "Trạng thái";
                    _dgvDeXuat.Columns["NgayTao"].HeaderText = "Ngày gửi";
                    _dgvDeXuat.Columns["NgayTao"].DefaultCellStyle.Format = "dd/MM/yyyy";
                    _dgvDeXuat.Columns["GhiChuDuyet"].HeaderText = "Phản hồi Admin";

                    // Ẩn các cột không cần thiết
                    if (_dgvDeXuat.Columns.Contains("MaPhim"))
                        _dgvDeXuat.Columns["MaPhim"].Visible = false;
                    if (_dgvDeXuat.Columns.Contains("MaChiNhanh"))
                        _dgvDeXuat.Columns["MaChiNhanh"].Visible = false;
                    if (_dgvDeXuat.Columns.Contains("TenChiNhanh"))
                        _dgvDeXuat.Columns["TenChiNhanh"].Visible = false;
                    if (_dgvDeXuat.Columns.Contains("TheLoai"))
                        _dgvDeXuat.Columns["TheLoai"].Visible = false;
                    if (_dgvDeXuat.Columns.Contains("ThongTinBoSung"))
                        _dgvDeXuat.Columns["ThongTinBoSung"].Visible = false;
                    if (_dgvDeXuat.Columns.Contains("NguoiDeXuat"))
                        _dgvDeXuat.Columns["NguoiDeXuat"].Visible = false;
                    if (_dgvDeXuat.Columns.Contains("NguoiDuyet"))
                        _dgvDeXuat.Columns["NguoiDuyet"].Visible = false;
                    if (_dgvDeXuat.Columns.Contains("NgayDuyet"))
                        _dgvDeXuat.Columns["NgayDuyet"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading proposals: {ex.Message}");
            }
        }

        private void TaoDeXuat(string loaiDeXuat)
        {
            if (_dgvPhimDoanhThuThap.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phim cần đề xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = _dgvPhimDoanhThuThap.SelectedRows[0];
            int maPhim = Convert.ToInt32(row.Cells["MaPhim"].Value);
            string tenPhim = row.Cells["TenPhim"].Value?.ToString() ?? "";

            string loaiText = loaiDeXuat == "GiamSuatChieu" ? "giảm suất chiếu" : "xóa khỏi lịch chiếu";

            // Hiện form nhập lý do
            Form frmLyDo = new Form
            {
                Text = $"Đề xuất {loaiText}",
                Size = new Size(500, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Label lblPhim = new Label
            {
                Text = $"Phim: {tenPhim}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblLyDo = new Label
            {
                Text = "Lý do đề xuất:",
                Location = new Point(20, 55),
                AutoSize = true
            };

            TextBox txtLyDo = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(440, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = $"Phim có doanh thu thấp trong 30 ngày qua. Đề xuất {loaiText} để nhường chỗ cho phim mới."
            };

            Button btnGui = new Button
            {
                Text = "📤 Gửi đề xuất",
                Size = new Size(120, 35),
                Location = new Point(170, 200),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGui.FlatAppearance.BorderSize = 0;
            btnGui.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtLyDo.Text))
                {
                    MessageBox.Show("Vui lòng nhập lý do!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    int newId = _deXuatDAL.ThemDeXuat(_userId, maPhim, _maChiNhanh, loaiDeXuat, txtLyDo.Text);
                    if (newId > 0)
                    {
                        MessageBox.Show("Đã gửi đề xuất thành công!\nAdmin sẽ xem xét và phản hồi.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmLyDo.Close();
                        LoadDeXuatDaGui();
                    }
                    else
                    {
                        MessageBox.Show("Có lỗi xảy ra. Vui lòng thử lại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnHuy = new Button
            {
                Text = "Hủy",
                Size = new Size(80, 35),
                Location = new Point(300, 200),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnHuy.FlatAppearance.BorderSize = 0;
            btnHuy.Click += (s, e) => frmLyDo.Close();

            frmLyDo.Controls.AddRange(new Control[] { lblPhim, lblLyDo, txtLyDo, btnGui, btnHuy });
            frmLyDo.ShowDialog();
        }

        private void HuyDeXuat()
        {
            if (_dgvDeXuat.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn đề xuất cần hủy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = _dgvDeXuat.SelectedRows[0];
            string trangThai = row.Cells["TrangThai"].Value?.ToString() ?? "";

            if (trangThai != "ChoDuyet" && trangThai != "Chờ duyệt")
            {
                MessageBox.Show("Chỉ có thể hủy đề xuất đang chờ duyệt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maDeXuat = Convert.ToInt32(row.Cells["MaDeXuat"].Value);

            if (MessageBox.Show("Bạn có chắc muốn hủy đề xuất này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool success = _deXuatDAL.HuyDeXuat(maDeXuat, _userId);
                    if (success)
                    {
                        MessageBox.Show("Đã hủy đề xuất!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDeXuatDaGui();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FilterDeXuat(string filter)
        {
            if (_dtDeXuat == null) return;

            DataView dv = _dtDeXuat.DefaultView;
            if (filter == "Tất cả")
                dv.RowFilter = "";
            else if (filter == "Chờ duyệt")
                dv.RowFilter = "TrangThai = 'Chờ duyệt' OR TrangThai = 'ChoDuyet'";
            else if (filter == "Đã duyệt")
                dv.RowFilter = "TrangThai = 'Đã duyệt' OR TrangThai = 'DaDuyet'";
            else if (filter == "Từ chối")
                dv.RowFilter = "TrangThai = 'Từ chối' OR TrangThai = 'TuChoi'";
        }

        private void DgvDeXuat_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvDeXuat.Columns[e.ColumnIndex].Name == "TrangThai" && e.Value != null)
            {
                string status = e.Value.ToString();
                switch (status)
                {
                    case "ChoDuyet":
                    case "Chờ duyệt":
                        e.CellStyle.ForeColor = Color.Orange;
                        e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                        e.Value = "⏳ Chờ duyệt";
                        break;
                    case "DaDuyet":
                    case "Đã duyệt":
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                        e.Value = "✅ Đã duyệt";
                        break;
                    case "TuChoi":
                    case "Từ chối":
                        e.CellStyle.ForeColor = Color.Red;
                        e.Value = "❌ Từ chối";
                        break;
                    case "DaHuy":
                        e.CellStyle.ForeColor = Color.Gray;
                        e.Value = "🚫 Đã hủy";
                        break;
                }
            }

            if (_dgvDeXuat.Columns[e.ColumnIndex].Name == "LoaiDeXuat" && e.Value != null)
            {
                string loai = e.Value.ToString();
                switch (loai)
                {
                    case "GiamSuatChieu":
                        e.Value = "📉 Giảm suất chiếu";
                        break;
                    case "XoaPhim":
                        e.Value = "🗑️ Xóa phim";
                        break;
                    case "TangSuatChieu":
                        e.Value = "📈 Tăng suất chiếu";
                        break;
                    case "ThemPhim":
                        e.Value = "➕ Thêm phim mới";
                        break;
                }
            }
        }

        private void AddRoundedCorners(Control control, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(control.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, control.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            control.Region = new Region(path);
        }
    }
}
