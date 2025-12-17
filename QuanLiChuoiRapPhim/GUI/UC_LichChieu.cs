// File: UC_LichChieu.cs
// Vị trí: QuanLiChuoiRapPhim.GUI
// Mô tả: UserControl quản lý lịch chiếu phim cho Quản lý chi nhánh
// Chức năng: Hiển thị, thêm, sửa, xóa lịch chiếu theo ngày, phim, phòng
// Tích hợp: Kiểm tra xung đột thời gian phòng chiếu
// Tác giả: Grok 4 (dựa trên dữ liệu dự án)
// Ngày: 2025-12-17

using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_LichChieu : UserControl
    {
        private readonly int _maChiNhanh; // Mã chi nhánh của Manager
        private DateTime _ngayHienTai = new DateTime(2025, 12, 17); // Ngày hiện tại theo đề bài

        // UI Components
        private Panel pnlHeader, pnlFilter;
        private DataGridView dgvLichChieu;
        private DateTimePicker dtpNgay;
        private ComboBox cboPhim, cboPhong;
        private Button btnThem, btnSua, btnXoa, btnLamMoi, btnKiemTraXungDot;

        // DAL
        private LichChieuDAL _lichChieuDAL;

        public UC_LichChieu(int maChiNhanh)
        {
            _maChiNhanh = maChiNhanh;
            _lichChieuDAL = new LichChieuDAL();
            InitializeComponent();
            ThietLapGiaoDien();
            TaiDuLieuLichChieu();
        }

        private void ThietLapGiaoDien()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            // Header Panel
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 123, 255) // Màu xanh dương
            };

            Label lblTieuDe = new Label
            {
                Text = "QUẢN LÝ LỊCH CHIẾU PHIM",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            pnlHeader.Controls.Add(lblTieuDe);

            // Filter Panel
            pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            Label lblNgay = new Label
            {
                Text = "Ngày:",
                Location = new Point(20, 15),
                AutoSize = true
            };

            dtpNgay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = _ngayHienTai,
                Location = new Point(70, 12),
                Width = 120
            };
            dtpNgay.ValueChanged += DtpNgay_ValueChanged;

            Label lblPhim = new Label
            {
                Text = "Phim:",
                Location = new Point(200, 15),
                AutoSize = true
            };

            cboPhim = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(240, 12),
                Width = 200
            };
            cboPhim.SelectedIndexChanged += CboFilter_SelectedIndexChanged;

            Label lblPhong = new Label
            {
                Text = "Phòng:",
                Location = new Point(450, 15),
                AutoSize = true
            };

            cboPhong = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(500, 12),
                Width = 150
            };
            cboPhong.SelectedIndexChanged += CboFilter_SelectedIndexChanged;

            pnlFilter.Controls.AddRange(new Control[] { lblNgay, dtpNgay, lblPhim, cboPhim, lblPhong, cboPhong });

            // DataGridView
            dgvLichChieu = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            dgvLichChieu.SelectionChanged += DgvLichChieu_SelectionChanged;

            // Buttons Panel (Bottom)
            Panel pnlButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            btnThem = new Button
            {
                Text = "THÊM",
                Location = new Point(20, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnThem.Click += BtnThem_Click;

            btnSua = new Button
            {
                Text = "SỬA",
                Location = new Point(130, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnSua.Click += BtnSua_Click;

            btnXoa = new Button
            {
                Text = "XÓA",
                Location = new Point(240, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnXoa.Click += BtnXoa_Click;

            btnLamMoi = new Button
            {
                Text = "LÀM MỚI",
                Location = new Point(350, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLamMoi.Click += BtnLamMoi_Click;

            btnKiemTraXungDot = new Button
            {
                Text = "KIỂM TRA XUNG ĐỘT",
                Location = new Point(460, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnKiemTraXungDot.Click += BtnKiemTraXungDot_Click;

            pnlButtons.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLamMoi, btnKiemTraXungDot });

            // Add to UserControl
            this.Controls.Add(dgvLichChieu);
            this.Controls.Add(pnlButtons);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlHeader);

            // Load ComboBoxes
            TaiDuLieuComboBoxes();
        }

        private void TaiDuLieuComboBoxes()
        {
            // Load Phim (giả định từ DAL)
            DataTable dtPhim = _lichChieuDAL.GetAllPhim(_maChiNhanh.ToString());
            cboPhim.Items.Add("Tất cả");
            foreach (DataRow row in dtPhim.Rows)
            {
                cboPhim.Items.Add(row["TenPhim"].ToString());
            }
            cboPhim.SelectedIndex = 0;

            // Load Phòng
            DataTable dtPhong = _lichChieuDAL.GetAllPhong(_maChiNhanh.ToString());
            cboPhong.Items.Add("Tất cả");
            foreach (DataRow row in dtPhong.Rows)
            {
                cboPhong.Items.Add(row["TenPhong"].ToString());
            }
            cboPhong.SelectedIndex = 0;
        }

        private void TaiDuLieuLichChieu()
        {
            try
            {
                DataTable dt = _lichChieuDAL.GetLichChieuByDate(_maChiNhanh.ToString(), dtpNgay.Value);

                // Lọc nếu có
                if (cboPhim.SelectedIndex > 0)
                {
                    dt = dt.Select($"TenPhim = '{cboPhim.SelectedItem}'").CopyToDataTable();
                }

                if (cboPhong.SelectedIndex > 0)
                {
                    dt = dt.Select($"TenPhong = '{cboPhong.SelectedItem}'").CopyToDataTable();
                }

                dgvLichChieu.DataSource = dt;

                // Cấu hình columns
                dgvLichChieu.Columns["MaSuat"].Visible = false;
                dgvLichChieu.Columns["TenPhim"].HeaderText = "Phim";
                dgvLichChieu.Columns["TenPhong"].HeaderText = "Phòng";
                dgvLichChieu.Columns["GioBatDau"].HeaderText = "Bắt đầu";
                dgvLichChieu.Columns["GioKetThuc"].HeaderText = "Kết thúc";
                dgvLichChieu.Columns["GiaVe"].HeaderText = "Giá vé";
                dgvLichChieu.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải lịch chiếu: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DtpNgay_ValueChanged(object sender, EventArgs e)
        {
            TaiDuLieuLichChieu();
        }

        private void CboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiDuLieuLichChieu();
        }

        private void DgvLichChieu_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvLichChieu.SelectedRows.Count > 0;
            btnSua.Enabled = hasSelection;
            btnXoa.Enabled = hasSelection;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            // Mở form thêm lịch chiếu (giả định có frmLichChieuDetail)
            frmLichChieuDetail frm = new frmLichChieuDetail(_maChiNhanh.ToString(), null); // null cho thêm mới
            if (frm.ShowDialog() == DialogResult.OK)
            {
                TaiDuLieuLichChieu();
            }
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (dgvLichChieu.SelectedRows.Count > 0)
            {
                int maSuat = Convert.ToInt32(dgvLichChieu.SelectedRows[0].Cells["MaSuat"].Value);
                frmLichChieuDetail frm = new frmLichChieuDetail(_maChiNhanh.ToString(), maSuat.ToString());
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    TaiDuLieuLichChieu();
                }
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvLichChieu.SelectedRows.Count > 0)
            {
                string tenPhim = dgvLichChieu.SelectedRows[0].Cells["TenPhim"].Value.ToString();
                if (MessageBox.Show($"Xóa lịch chiếu '{tenPhim}'?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int maSuat = Convert.ToInt32(dgvLichChieu.SelectedRows[0].Cells["MaSuat"].Value);
                    if (_lichChieuDAL.DeleteLichChieu(maSuat.ToString()))
                    {
                        MessageBox.Show("Xóa thành công!", "Thành công");
                        TaiDuLieuLichChieu();
                    }
                    else
                    {
                        MessageBox.Show("Lỗi xóa lịch chiếu!", "Lỗi");
                    }
                }
            }
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            dtpNgay.Value = _ngayHienTai;
            cboPhim.SelectedIndex = 0;
            cboPhong.SelectedIndex = 0;
            TaiDuLieuLichChieu();
        }

        private void BtnKiemTraXungDot_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvLichChieu.DataSource;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để kiểm tra!", "Thông báo");
                return;
            }

            // Kiểm tra xung đột (tương tự code Python)
            var conflicts = new System.Collections.Generic.List<string>();
            var rooms = dt.AsEnumerable().Select(r => r["TenPhong"].ToString()).Distinct();

            foreach (var room in rooms)
            {
                var roomRows = dt.Select($"TenPhong = '{room}'")
                    .OrderBy(r => DateTime.Parse(r["GioBatDau"].ToString()))
                    .ToArray();

                for (int i = 1; i < roomRows.Length; i++)
                {
                    DateTime prevEnd = DateTime.Parse(roomRows[i - 1]["GioKetThuc"].ToString());
                    DateTime currStart = DateTime.Parse(roomRows[i]["GioBatDau"].ToString());

                    if (prevEnd > currStart)
                    {
                        conflicts.Add($"Xung đột tại phòng {room}: {roomRows[i - 1]["TenPhim"]} và {roomRows[i]["TenPhim"]}");
                    }
                }
            }

            if (conflicts.Count > 0)
            {
                MessageBox.Show(string.Join("\n", conflicts), "Xung đột lịch chiếu",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Không có xung đột lịch chiếu!", "Kết quả",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}