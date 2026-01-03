// File: UC_AdminDeXuatReview.cs
// UserControl for Admin to review and approve/reject proposals from branch managers
// IMPROVED: Added checkbox selection, better UI layout, and multi-select support

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.GUI
{
    public class UC_AdminDeXuatReview : UserControl
    {
        // CGV Branding Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGreen = Color.FromArgb(40, 167, 69);
        private readonly Color _cgvBlue = Color.FromArgb(52, 73, 94);

        private int _userId;
        private DataGridView _dgvDeXuat;
        private DeXuatLichChieuDAL _deXuatDAL;
        private Label _lblStats;
        private TextBox _txtGhiChu;
        private Label _lblSelected;

        public UC_AdminDeXuatReview(int userId)
        {
            _userId = userId;
            _deXuatDAL = new DeXuatLichChieuDAL();
            InitializeComponent();
            LoadPendingProposals();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(20);

            // Main container with proper layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Toolbar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid

            // === HEADER ===
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _cgvBlack,
                Padding = new Padding(20, 15, 20, 15)
            };

            Label lblTitle = new Label
            {
                Text = "📋 DUYỆT ĐỀ XUẤT TỪ CHI NHÁNH",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            headerPanel.Controls.Add(lblTitle);

            _lblStats = new Label
            {
                Text = "Đang tải...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Location = new Point(headerPanel.Width - 250, 25)
            };
            headerPanel.Controls.Add(_lblStats);
            headerPanel.Resize += (s, e) => {
                _lblStats.Location = new Point(headerPanel.Width - _lblStats.Width - 20, 25);
            };

            mainLayout.Controls.Add(headerPanel, 0, 0);

            // === TOOLBAR ===
            Panel toolbar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(15, 10, 15, 10)
            };

            int xPos = 15;

            Button btnRefresh = CreateButton("🔄 Làm mới", _cgvBlue, xPos);
            btnRefresh.Click += (s, e) => LoadPendingProposals();
            toolbar.Controls.Add(btnRefresh);
            xPos += 125;

            Button btnApprove = CreateButton("✅ Duyệt", _cgvGreen, xPos);
            btnApprove.Click += (s, e) => ApproveCheckedItems("DaDuyet");
            toolbar.Controls.Add(btnApprove);
            xPos += 110;

            Button btnReject = CreateButton("❌ Từ chối", _cgvRed, xPos);
            btnReject.Click += (s, e) => ApproveCheckedItems("TuChoi");
            toolbar.Controls.Add(btnReject);
            xPos += 120;

            // Separator
            Panel sep = new Panel { Location = new Point(xPos, 10), Size = new Size(1, 35), BackColor = Color.LightGray };
            toolbar.Controls.Add(sep);
            xPos += 15;

            // Ghi chú input
            Label lblGhiChu = new Label
            {
                Text = "Ghi chú:",
                Location = new Point(xPos, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            toolbar.Controls.Add(lblGhiChu);
            xPos += 60;

            _txtGhiChu = new TextBox
            {
                Location = new Point(xPos, 10),
                Size = new Size(280, 30),
                Font = new Font("Segoe UI", 10)
            };
            toolbar.Controls.Add(_txtGhiChu);
            xPos += 300;

            // Selected count label
            _lblSelected = new Label
            {
                Text = "Chọn: 0",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _cgvRed,
                AutoSize = true,
                Location = new Point(xPos, 15)
            };
            toolbar.Controls.Add(_lblSelected);

            mainLayout.Controls.Add(toolbar, 0, 1);

            // === DATAGRIDVIEW ===
            Panel gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            _dgvDeXuat = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = 42 }
            };
            StyleDataGridView();
            _dgvDeXuat.CellFormatting += DgvDeXuat_CellFormatting;
            _dgvDeXuat.CellContentClick += DgvDeXuat_CellContentClick;

            gridPanel.Controls.Add(_dgvDeXuat);
            mainLayout.Controls.Add(gridPanel, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private Button CreateButton(string text, Color bgColor, int x)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(105, 35),
                Location = new Point(x, 10),
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void StyleDataGridView()
        {
            _dgvDeXuat.ColumnHeadersDefaultCellStyle.BackColor = _cgvBlack;
            _dgvDeXuat.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvDeXuat.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _dgvDeXuat.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            _dgvDeXuat.ColumnHeadersHeight = 42;
            _dgvDeXuat.EnableHeadersVisualStyles = false;

            _dgvDeXuat.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 240, 240);
            _dgvDeXuat.DefaultCellStyle.SelectionForeColor = _cgvBlack;
            _dgvDeXuat.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        }

        private void LoadPendingProposals()
        {
            try
            {
                DataTable dt = _deXuatDAL.LayTatCaDeXuatChoDuyet();

                // Add checkbox column if not exists
                if (!dt.Columns.Contains("Select"))
                {
                    dt.Columns.Add("Select", typeof(bool));
                    foreach (DataRow row in dt.Rows)
                        row["Select"] = false;
                }

                _dgvDeXuat.DataSource = dt;

                if (_dgvDeXuat.Columns.Count > 0)
                {
                    // Move Select column to first
                    if (_dgvDeXuat.Columns.Contains("Select"))
                    {
                        _dgvDeXuat.Columns["Select"].DisplayIndex = 0;
                        _dgvDeXuat.Columns["Select"].HeaderText = "☐";
                        _dgvDeXuat.Columns["Select"].Width = 40;
                        _dgvDeXuat.Columns["Select"].ReadOnly = false;
                    }

                    // Configure columns
                    if (_dgvDeXuat.Columns.Contains("MaDeXuat"))
                    {
                        _dgvDeXuat.Columns["MaDeXuat"].HeaderText = "Mã";
                        _dgvDeXuat.Columns["MaDeXuat"].Width = 50;
                    }
                    if (_dgvDeXuat.Columns.Contains("TenPhim"))
                    {
                        _dgvDeXuat.Columns["TenPhim"].HeaderText = "Phim";
                        _dgvDeXuat.Columns["TenPhim"].FillWeight = 120;
                    }
                    if (_dgvDeXuat.Columns.Contains("TenChiNhanh"))
                    {
                        _dgvDeXuat.Columns["TenChiNhanh"].HeaderText = "Chi nhánh";
                        _dgvDeXuat.Columns["TenChiNhanh"].FillWeight = 100;
                    }
                    if (_dgvDeXuat.Columns.Contains("LoaiDeXuatText"))
                    {
                        _dgvDeXuat.Columns["LoaiDeXuatText"].HeaderText = "Loại";
                        _dgvDeXuat.Columns["LoaiDeXuatText"].Width = 100;
                    }
                    if (_dgvDeXuat.Columns.Contains("LyDo"))
                    {
                        _dgvDeXuat.Columns["LyDo"].HeaderText = "Lý do";
                        _dgvDeXuat.Columns["LyDo"].FillWeight = 150;
                    }
                    if (_dgvDeXuat.Columns.Contains("NgayTao"))
                    {
                        _dgvDeXuat.Columns["NgayTao"].HeaderText = "Ngày gửi";
                        _dgvDeXuat.Columns["NgayTao"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        _dgvDeXuat.Columns["NgayTao"].Width = 130;
                    }
                    if (_dgvDeXuat.Columns.Contains("NguoiDeXuat"))
                    {
                        _dgvDeXuat.Columns["NguoiDeXuat"].HeaderText = "Người đề xuất";
                        _dgvDeXuat.Columns["NguoiDeXuat"].FillWeight = 100;
                    }

                    // Hide columns
                    string[] hideCols = { "MaPhim", "TheLoai", "MaChiNhanh", "LoaiDeXuat", "TrangThai", "MaNguoiDeXuat" };
                    foreach (string col in hideCols)
                    {
                        if (_dgvDeXuat.Columns.Contains(col))
                            _dgvDeXuat.Columns[col].Visible = false;
                    }

                    // Make all columns readonly except Select
                    foreach (DataGridViewColumn col in _dgvDeXuat.Columns)
                    {
                        if (col.Name != "Select")
                            col.ReadOnly = true;
                    }
                }

                // Update stats
                int count = dt?.Rows.Count ?? 0;
                _lblStats.Text = count > 0
                    ? $"📋 {count} đề xuất đang chờ duyệt"
                    : "✅ Không có đề xuất nào đang chờ";
                _lblStats.ForeColor = count > 0 ? Color.FromArgb(255, 200, 200) : Color.FromArgb(200, 255, 200);

                UpdateSelectedCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách đề xuất: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvDeXuat_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Handle checkbox click
            if (e.RowIndex >= 0 && _dgvDeXuat.Columns[e.ColumnIndex].Name == "Select")
            {
                _dgvDeXuat.EndEdit();
                UpdateSelectedCount();
            }
        }

        private void UpdateSelectedCount()
        {
            if (_dgvDeXuat.DataSource is DataTable dt)
            {
                int count = dt.AsEnumerable().Count(r => r.Field<bool?>("Select") == true);
                _lblSelected.Text = $"Đã chọn: {count}";
            }
        }

        private void ApproveCheckedItems(string trangThai)
        {
            if (_dgvDeXuat.DataSource is not DataTable dt) return;

            var checkedRows = dt.AsEnumerable()
                .Where(r => r.Field<bool?>("Select") == true)
                .ToList();

            if (checkedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng tick chọn ít nhất một đề xuất cần xử lý!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string action = trangThai == "DaDuyet" ? "duyệt" : "từ chối";
            string icon = trangThai == "DaDuyet" ? "✅" : "❌";

            // Build confirmation message
            string filmList = string.Join("\n", checkedRows.Take(5).Select(r => $"  • {r["TenPhim"]}"));
            if (checkedRows.Count > 5)
                filmList += $"\n  ... và {checkedRows.Count - 5} đề xuất khác";

            DialogResult result = MessageBox.Show(
                $"Bạn có chắc muốn {action} {checkedRows.Count} đề xuất sau?\n\n{filmList}" +
                (string.IsNullOrEmpty(_txtGhiChu.Text) ? "" : $"\n\nGhi chú: {_txtGhiChu.Text}"),
                $"Xác nhận {action}",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes) return;

            int successCount = 0;
            foreach (DataRow row in checkedRows)
            {
                try
                {
                    int maDeXuat = Convert.ToInt32(row["MaDeXuat"]);
                    bool success = _deXuatDAL.DuyetDeXuat(maDeXuat, trangThai, _userId, _txtGhiChu.Text);
                    if (success) successCount++;
                }
                catch { /* Continue with next */ }
            }

            MessageBox.Show(
                $"{icon} Đã {action} {successCount}/{checkedRows.Count} đề xuất thành công!\n\nThông báo đã được gửi đến người đề xuất.",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            _txtGhiChu.Text = "";
            LoadPendingProposals();
        }

        private void DgvDeXuat_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            string colName = _dgvDeXuat.Columns[e.ColumnIndex].Name;

            if (colName == "LoaiDeXuatText" && e.Value != null)
            {
                string value = e.Value.ToString();
                if (value.Contains("Xóa"))
                {
                    e.CellStyle.ForeColor = _cgvRed;
                    e.CellStyle.Font = new Font(_dgvDeXuat.Font, FontStyle.Bold);
                }
                else if (value.Contains("Giảm"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(230, 126, 34);
                    e.CellStyle.Font = new Font(_dgvDeXuat.Font, FontStyle.Bold);
                }
            }
        }
    }
}
