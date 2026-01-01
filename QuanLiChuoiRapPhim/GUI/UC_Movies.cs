using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_Movies : UserControl
    {
        private PhimBLL _phimBLL = new PhimBLL();
        private FlowLayoutPanel _movieCardsPanel;
        private DataGridView _dgvPhim;
        private TextBox _txtSearch;
        private ComboBox _cboFilter;
        private Button _btnViewGrid, _btnViewCards;
        private DataTable _dtPhim;
        private bool _isCardView = true;
        private Label _lblStats;

        // CGV Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvDarkGray = Color.FromArgb(45, 45, 45);
        private readonly Color _cgvGold = Color.FromArgb(212, 175, 55);

        public UC_Movies()
        {
            InitializeComponent();
            SetupUI();
            LoadPhim();
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);

            // ========== HEADER PANEL ==========
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = _cgvBlack,
                Padding = new Padding(25, 15, 25, 15)
            };

            // Title with icon
            Label lblTitle = new Label
            {
                Text = "🎬 QUẢN LÝ PHIM",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 22)
            };

            // Stats label
            _lblStats = new Label
            {
                Text = "📊 Đang chiếu: -- | Sắp chiếu: -- | Tổng: --",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(headerPanel.Width - 380, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(_lblStats);

            // ========== TOOLBAR PANEL ==========
            Panel toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };

            // Search box
            Panel searchContainer = new Panel
            {
                Size = new Size(300, 40),
                Location = new Point(20, 15),
                BackColor = _cgvLightGray
            };

            _txtSearch = new TextBox
            {
                Size = new Size(280, 30),
                Location = new Point(10, 8),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                BackColor = _cgvLightGray,
                ForeColor = _cgvDarkGray
            };
            _txtSearch.Text = "🔍 Tìm kiếm phim...";
            _txtSearch.GotFocus += (s, e) => { if (_txtSearch.Text == "🔍 Tìm kiếm phim...") _txtSearch.Text = ""; };
            _txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_txtSearch.Text)) _txtSearch.Text = "🔍 Tìm kiếm phim..."; };
            _txtSearch.TextChanged += TxtSearch_TextChanged;
            searchContainer.Controls.Add(_txtSearch);

            // Filter dropdown
            _cboFilter = new ComboBox
            {
                Size = new Size(150, 35),
                Location = new Point(340, 17),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            _cboFilter.Items.AddRange(new[] { "Tất cả", "Đang chiếu", "Sắp chiếu" });
            _cboFilter.SelectedIndex = 0;
            _cboFilter.SelectedIndexChanged += CboFilter_SelectedIndexChanged;

            // View toggle buttons
            _btnViewCards = CreateToolButton("📇", new Point(520, 15), true);
            _btnViewCards.Click += (s, e) => SwitchView(true);
            
            _btnViewGrid = CreateToolButton("📋", new Point(565, 15), false);
            _btnViewGrid.Click += (s, e) => SwitchView(false);

            // Action buttons
            Button btnAdd = CreateActionButton("➕ THÊM PHIM", Color.FromArgb(40, 167, 69), new Point(650, 12));
            btnAdd.Click += BtnThem_Click;

            Button btnRefresh = CreateActionButton("🔄 LÀM MỚI", Color.FromArgb(23, 162, 184), new Point(800, 12));
            btnRefresh.Click += (s, e) => LoadPhim();

            Button btnExport = CreateActionButton("📤 XUẤT CSV", Color.FromArgb(108, 117, 125), new Point(950, 12));
            btnExport.Click += BtnExport_Click;

            toolbarPanel.Controls.AddRange(new Control[] { 
                searchContainer, _cboFilter, _btnViewCards, _btnViewGrid, 
                btnAdd, btnRefresh, btnExport 
            });

            // ========== CONTENT PANEL ==========
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _cgvLightGray,
                Padding = new Padding(20)
            };

            // Card view panel
            _movieCardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = _cgvLightGray,
                WrapContents = true,
                Padding = new Padding(10)
            };

            // Grid view
            _dgvPhim = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowTemplate = { Height = 50 },
                Font = new Font("Segoe UI", 10),
                Visible = false
            };
            
            // Style DataGridView
            _dgvPhim.EnableHeadersVisualStyles = false;
            _dgvPhim.ColumnHeadersDefaultCellStyle.BackColor = _cgvRed;
            _dgvPhim.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvPhim.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            _dgvPhim.ColumnHeadersHeight = 45;
            _dgvPhim.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            _dgvPhim.CellDoubleClick += DgvPhim_CellDoubleClick;

            contentPanel.Controls.Add(_movieCardsPanel);
            contentPanel.Controls.Add(_dgvPhim);

            // Add all panels
            this.Controls.Add(contentPanel);
            this.Controls.Add(toolbarPanel);
            this.Controls.Add(headerPanel);
        }

        private Button CreateToolButton(string text, Point location, bool isActive)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(40, 40),
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor = isActive ? _cgvRed : Color.White,
                ForeColor = isActive ? Color.White : _cgvDarkGray,
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = isActive ? _cgvRed : Color.LightGray;
            return btn;
        }

        private Button CreateActionButton(string text, Color bgColor, Point location)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(130, 40),
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SwitchView(bool cardView)
        {
            _isCardView = cardView;
            _movieCardsPanel.Visible = cardView;
            _dgvPhim.Visible = !cardView;

            _btnViewCards.BackColor = cardView ? _cgvRed : Color.White;
            _btnViewCards.ForeColor = cardView ? Color.White : _cgvDarkGray;
            _btnViewGrid.BackColor = !cardView ? _cgvRed : Color.White;
            _btnViewGrid.ForeColor = !cardView ? Color.White : _cgvDarkGray;

            if (!cardView && _dtPhim != null)
            {
                LoadDataGrid();
            }
        }

        private void LoadPhim()
        {
            try
            {
                _dtPhim = _phimBLL.LayTatCaPhim();
                if (_dtPhim.PrimaryKey.Length == 0 && _dtPhim.Columns.Contains("MaPhim"))
                {
                    _dtPhim.PrimaryKey = new[] { _dtPhim.Columns["MaPhim"] };
                }

                if (_isCardView)
                    LoadMovieCards();
                else
                    LoadDataGrid();

                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMovieCards()
        {
            _movieCardsPanel.SuspendLayout();
            _movieCardsPanel.Controls.Clear();

            foreach (DataRow row in _dtPhim.Rows)
            {
                Panel card = CreateMovieCard(row);
                _movieCardsPanel.Controls.Add(card);
            }

            _movieCardsPanel.ResumeLayout();
        }

        private Panel CreateMovieCard(DataRow row)
        {
            Panel card = new Panel
            {
                Size = new Size(220, 380),
                Margin = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            // Movie Poster
            PictureBox poster = new PictureBox
            {
                Size = new Size(200, 280),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = _cgvDarkGray
            };

            // Load poster image
            string posterPath = row["HinhAnh"]?.ToString();
            if (!string.IsNullOrEmpty(posterPath))
            {
                try
                {
                    string localPath = Path.Combine(Application.StartupPath, "uploads", "movies", Path.GetFileName(posterPath));
                    if (File.Exists(localPath))
                    {
                        poster.Image = Image.FromFile(localPath);
                    }
                }
                catch
                {
                    poster.Image = CreatePlaceholderImage(row["TenPhim"].ToString());
                }
            }
            else
            {
                poster.Image = CreatePlaceholderImage(row["TenPhim"].ToString());
            }

            // Age Rating badge
            string doTuoi = row["DoTuoi"]?.ToString() ?? "P";
            Color badgeColor = doTuoi switch
            {
                "C18" => Color.Red,
                "C16" => Color.Orange,
                "C13" => Color.Yellow,
                _ => Color.Green
            };
            
            Panel ageBadge = new Panel
            {
                Size = new Size(35, 25),
                Location = new Point(170, 15),
                BackColor = badgeColor
            };
            Label lblAge = new Label
            {
                Text = doTuoi,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = doTuoi == "C13" ? Color.Black : Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            ageBadge.Controls.Add(lblAge);
            poster.Controls.Add(ageBadge);

            // Movie title
            Label lblTitle = new Label
            {
                Text = row["TenPhim"].ToString(),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(10, 300),
                Size = new Size(200, 22),
                AutoEllipsis = true
            };

            // Genre & Duration
            string genre = row["TheLoai"]?.ToString() ?? "";
            string duration = row["ThoiLuong"]?.ToString() ?? "0";
            Label lblInfo = new Label
            {
                Text = $"{genre} • {duration} phút",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(10, 322),
                Size = new Size(200, 18),
                AutoEllipsis = true
            };

            // Release date
            DateTime releaseDate = Convert.ToDateTime(row["NgayKhoiChieu"]);
            Label lblRelease = new Label
            {
                Text = $"📅 {releaseDate:dd/MM/yyyy}",
                Font = new Font("Segoe UI", 9),
                ForeColor = _cgvRed,
                Location = new Point(10, 345),
                Size = new Size(200, 18)
            };

            // Click handlers
            int maPhim = Convert.ToInt32(row["MaPhim"]);

            // Upload button for poster
            Button btnUpload = new Button
            {
                Text = "📷",
                Size = new Size(35, 30),
                Location = new Point(175, 255),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 226, 26, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnUpload.FlatAppearance.BorderSize = 0;
            btnUpload.Click += (s, e) => UploadPosterForMovie(maPhim, poster);
            poster.Controls.Add(btnUpload);

            card.Click += (s, e) => ShowMovieDetail(maPhim);
            poster.Click += (s, e) => ShowMovieDetail(maPhim);
            lblTitle.Click += (s, e) => ShowMovieDetail(maPhim);

            card.Controls.AddRange(new Control[] { poster, lblTitle, lblInfo, lblRelease });

            return card;
        }

        private Image CreatePlaceholderImage(string title)
        {
            Bitmap bmp = new Bitmap(200, 280);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillRectangle(new SolidBrush(_cgvDarkGray), 0, 0, 200, 280);

                // Draw movie icon
                using (Font iconFont = new Font("Segoe UI", 48))
                {
                    g.DrawString("🎬", iconFont, Brushes.White, new PointF(60, 80));
                }

                // Draw title
                using (Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(title, titleFont, Brushes.White, new RectangleF(10, 180, 180, 80), sf);
                }
            }
            return bmp;
        }

        private void LoadDataGrid()
        {
            _dgvPhim.Columns.Clear();
            _dgvPhim.DataSource = null;

            // Add columns
            _dgvPhim.Columns.Add("MaPhim", "Mã");
            _dgvPhim.Columns.Add("TenPhim", "Tên Phim");
            _dgvPhim.Columns.Add("TheLoai", "Thể Loại");
            _dgvPhim.Columns.Add("ThoiLuong", "Thời Lượng");
            _dgvPhim.Columns.Add("DaoDien", "Đạo Diễn");
            _dgvPhim.Columns.Add("DoTuoi", "Phân Loại");
            _dgvPhim.Columns.Add("NgayKhoiChieu", "Ngày KC");
            _dgvPhim.Columns.Add("TrangThai", "Trạng Thái");

            // Add action column
            DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn
            {
                Name = "btnEdit",
                Text = "✏️ Sửa",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            _dgvPhim.Columns.Add(btnEdit);

            DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn
            {
                Name = "btnDelete",
                Text = "🗑️ Xóa",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            _dgvPhim.Columns.Add(btnDelete);

            // Set column widths
            _dgvPhim.Columns["MaPhim"].Width = 60;
            _dgvPhim.Columns["ThoiLuong"].Width = 100;
            _dgvPhim.Columns["DoTuoi"].Width = 80;
            _dgvPhim.Columns["NgayKhoiChieu"].Width = 110;

            // Load data
            _dgvPhim.Rows.Clear();
            foreach (DataRow row in _dtPhim.Rows)
            {
                DateTime releaseDate = Convert.ToDateTime(row["NgayKhoiChieu"]);
                string status = releaseDate > DateTime.Now ? "Sắp chiếu" : "Đang chiếu";

                _dgvPhim.Rows.Add(
                    row["MaPhim"],
                    row["TenPhim"],
                    row["TheLoai"],
                    $"{row["ThoiLuong"]} phút",
                    row["DaoDien"],
                    row["DoTuoi"],
                    releaseDate.ToString("dd/MM/yyyy"),
                    status
                );
            }

            // Cell click for buttons
            _dgvPhim.CellClick -= DgvPhim_CellClick;
            _dgvPhim.CellClick += DgvPhim_CellClick;
        }

        private void DgvPhim_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int maPhim = Convert.ToInt32(_dgvPhim.Rows[e.RowIndex].Cells["MaPhim"].Value);

            if (e.ColumnIndex == _dgvPhim.Columns["btnEdit"].Index)
            {
                ShowEditDialog(maPhim);
            }
            else if (e.ColumnIndex == _dgvPhim.Columns["btnDelete"].Index)
            {
                DeleteMovie(maPhim);
            }
        }

        private void DgvPhim_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int maPhim = Convert.ToInt32(_dgvPhim.Rows[e.RowIndex].Cells["MaPhim"].Value);
            ShowMovieDetail(maPhim);
        }

        private void UpdateStats()
        {
            int total = _dtPhim.Rows.Count;
            int dangChieu = 0, sapChieu = 0;

            foreach (DataRow row in _dtPhim.Rows)
            {
                DateTime releaseDate = Convert.ToDateTime(row["NgayKhoiChieu"]);
                if (releaseDate <= DateTime.Now)
                    dangChieu++;
                else
                    sapChieu++;
            }

            _lblStats.Text = $"📊 Đang chiếu: {dangChieu} | Sắp chiếu: {sapChieu} | Tổng: {total}";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_txtSearch.Text == "🔍 Tìm kiếm phim..." || string.IsNullOrWhiteSpace(_txtSearch.Text))
            {
                return;
            }

            try
            {
                _dtPhim = _phimBLL.TimKiemPhim(_txtSearch.Text);
                if (_dtPhim.PrimaryKey.Length == 0 && _dtPhim.Columns.Contains("MaPhim"))
                {
                    _dtPhim.PrimaryKey = new[] { _dtPhim.Columns["MaPhim"] };
                }
                
                if (_isCardView)
                    LoadMovieCards();
                else
                    LoadDataGrid();
                
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPhim(); // Reload all first
            
            if (_cboFilter.SelectedIndex == 0) return; // All

            DataTable dtFiltered = _dtPhim.Clone();

            foreach (DataRow row in _dtPhim.Rows)
            {
                DateTime releaseDate = Convert.ToDateTime(row["NgayKhoiChieu"]);
                bool include = _cboFilter.SelectedIndex switch
                {
                    1 => releaseDate <= DateTime.Now, // Đang chiếu
                    2 => releaseDate > DateTime.Now,  // Sắp chiếu
                    _ => true
                };

                if (include)
                {
                    dtFiltered.ImportRow(row);
                }
            }

            _dtPhim = dtFiltered;
            if (_dtPhim.PrimaryKey.Length == 0 && _dtPhim.Columns.Contains("MaPhim"))
            {
                _dtPhim.PrimaryKey = new[] { _dtPhim.Columns["MaPhim"] };
            }

            if (_isCardView)
                LoadMovieCards();
            else
                LoadDataGrid();

            UpdateStats();
        }

        private void ShowMovieDetail(int maPhim)
        {
            DataRow row = _dtPhim.Rows.Find(maPhim);
            if (row == null) return;

            using (Form detailForm = new Form())
            {
                detailForm.Text = row["TenPhim"].ToString();
                detailForm.Size = new Size(750, 550);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.BackColor = Color.White;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.MaximizeBox = false;

                // Left - Poster
                PictureBox poster = new PictureBox
                {
                    Size = new Size(250, 350),
                    Location = new Point(20, 20),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = _cgvDarkGray
                };

                string posterPath = row["HinhAnh"]?.ToString();
                if (!string.IsNullOrEmpty(posterPath))
                {
                    try
                    {
                        string localPath = Path.Combine(Application.StartupPath, "uploads", "movies", Path.GetFileName(posterPath));
                        if (File.Exists(localPath))
                            poster.Image = Image.FromFile(localPath);
                    }
                    catch { }
                }
                
                if (poster.Image == null)
                    poster.Image = CreatePlaceholderImage(row["TenPhim"].ToString());

                detailForm.Controls.Add(poster);

                // Right - Details
                int x = 290, y = 20;

                // Title
                Label lblTitle = new Label
                {
                    Text = row["TenPhim"].ToString(),
                    Font = new Font("Segoe UI", 18, FontStyle.Bold),
                    ForeColor = _cgvBlack,
                    Location = new Point(x, y),
                    AutoSize = true,
                    MaximumSize = new Size(400, 0)
                };
                detailForm.Controls.Add(lblTitle);
                y += lblTitle.Height + 15;

                // Age badge
                string doTuoi = row["DoTuoi"]?.ToString() ?? "P";
                Panel ageBadge = new Panel
                {
                    Size = new Size(50, 30),
                    Location = new Point(x, y),
                    BackColor = doTuoi switch { "C18" => Color.Red, "C16" => Color.Orange, "C13" => Color.Yellow, _ => Color.Green }
                };
                Label lblAge = new Label
                {
                    Text = doTuoi,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = doTuoi == "C13" ? Color.Black : Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                ageBadge.Controls.Add(lblAge);
                detailForm.Controls.Add(ageBadge);
                y += 50;

                // Info rows
                AddDetailRow(detailForm, "🎭 Thể loại:", row["TheLoai"]?.ToString(), x, ref y);
                AddDetailRow(detailForm, "⏱️ Thời lượng:", $"{row["ThoiLuong"]} phút", x, ref y);
                AddDetailRow(detailForm, "🎬 Đạo diễn:", row["DaoDien"]?.ToString(), x, ref y);
                AddDetailRow(detailForm, "⭐ Diễn viên:", row["DienVien"]?.ToString(), x, ref y);
                AddDetailRow(detailForm, "📅 Khởi chiếu:", Convert.ToDateTime(row["NgayKhoiChieu"]).ToString("dd/MM/yyyy"), x, ref y);

                // Description
                y += 15;
                Label lblDescTitle = new Label
                {
                    Text = "📝 Mô tả:",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = _cgvBlack,
                    Location = new Point(x, y),
                    AutoSize = true
                };
                detailForm.Controls.Add(lblDescTitle);
                y += 25;

                Label lblDesc = new Label
                {
                    Text = row["MoTa"]?.ToString() ?? "Chưa có mô tả",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Gray,
                    Location = new Point(x, y),
                    Size = new Size(400, 80),
                    AutoEllipsis = true
                };
                detailForm.Controls.Add(lblDesc);

                // Buttons
                Button btnEdit = new Button
                {
                    Text = "✏️ Chỉnh sửa",
                    Size = new Size(120, 40),
                    Location = new Point(290, 450),
                    BackColor = Color.FromArgb(0, 123, 255),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnEdit.FlatAppearance.BorderSize = 0;
                btnEdit.Click += (s, e) => { detailForm.Close(); ShowEditDialog(maPhim); };

                Button btnDelete = new Button
                {
                    Text = "🗑️ Xóa",
                    Size = new Size(100, 40),
                    Location = new Point(420, 450),
                    BackColor = _cgvRed,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnDelete.FlatAppearance.BorderSize = 0;
                btnDelete.Click += (s, e) => { detailForm.Close(); DeleteMovie(maPhim); };

                Button btnClose = new Button
                {
                    Text = "Đóng",
                    Size = new Size(100, 40),
                    Location = new Point(620, 450),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10)
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, e) => detailForm.Close();

                detailForm.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnClose });
                detailForm.ShowDialog(this);
            }
        }

        private void AddDetailRow(Form form, string label, string value, int x, ref int y)
        {
            Label lblLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(x, y),
                AutoSize = true
            };

            Label lblValue = new Label
            {
                Text = value ?? "N/A",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.DimGray,
                Location = new Point(x + 120, y),
                AutoSize = true,
                MaximumSize = new Size(300, 0)
            };

            form.Controls.AddRange(new Control[] { lblLabel, lblValue });
            y += Math.Max(28, lblValue.Height + 8);
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            ShowAddEditDialog(0, false);
        }

        private void ShowEditDialog(int maPhim)
        {
            ShowAddEditDialog(maPhim, true);
        }

        private void ShowAddEditDialog(int maPhim, bool isEdit)
        {
            DataRow row = isEdit ? _dtPhim.Rows.Find(maPhim) : null;

            using (Form form = new Form())
            {
                form.Text = isEdit ? "Chỉnh sửa phim" : "Thêm phim mới";
                form.Size = new Size(600, 600);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = Color.White;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;

                // Header
                Panel header = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    BackColor = _cgvRed
                };
                Label lblHeader = new Label
                {
                    Text = isEdit ? "✏️ CHỈNH SỬA PHIM" : "➕ THÊM PHIM MỚI",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                header.Controls.Add(lblHeader);
                form.Controls.Add(header);

                int y = 70;
                int labelWidth = 130;
                int inputLeft = 150;

                // Poster section
                PictureBox poster = new PictureBox
                {
                    Size = new Size(100, 140),
                    Location = new Point(460, 70),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = _cgvLightGray,
                    BorderStyle = BorderStyle.FixedSingle
                };

                if (isEdit && row != null)
                {
                    string posterPath = row["HinhAnh"]?.ToString();
                    if (!string.IsNullOrEmpty(posterPath))
                    {
                        try
                        {
                            string localPath = Path.Combine(Application.StartupPath, "uploads", "movies", Path.GetFileName(posterPath));
                            if (File.Exists(localPath))
                                poster.Image = Image.FromFile(localPath);
                        }
                        catch { }
                    }
                }

                Button btnChoosePoster = new Button
                {
                    Text = "📷 Ảnh",
                    Size = new Size(100, 28),
                    Location = new Point(460, 218),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9)
                };
                btnChoosePoster.FlatAppearance.BorderSize = 0;

                string selectedPosterPath = "";
                btnChoosePoster.Click += (s, e) =>
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            selectedPosterPath = ofd.FileName;
                            poster.Image = Image.FromFile(selectedPosterPath);
                        }
                    }
                };

                form.Controls.AddRange(new Control[] { poster, btnChoosePoster });

                // Input fields
                Label lblTenPhim = new Label { Text = "Tên phim *", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
                TextBox txtTenPhim = new TextBox { Location = new Point(inputLeft, y), Size = new Size(280, 28), Font = new Font("Segoe UI", 10) };
                if (isEdit && row != null) txtTenPhim.Text = row["TenPhim"].ToString();
                form.Controls.AddRange(new Control[] { lblTenPhim, txtTenPhim });
                y += 38;

                Label lblTheLoai = new Label { Text = "Thể loại", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
                TextBox txtTheLoai = new TextBox { Location = new Point(inputLeft, y), Size = new Size(280, 28), Font = new Font("Segoe UI", 10) };
                if (isEdit && row != null) txtTheLoai.Text = row["TheLoai"]?.ToString();
                form.Controls.AddRange(new Control[] { lblTheLoai, txtTheLoai });
                y += 38;

                Label lblThoiLuong = new Label { Text = "Thời lượng (phút) *", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
                NumericUpDown numThoiLuong = new NumericUpDown { Location = new Point(inputLeft, y), Size = new Size(100, 28), Font = new Font("Segoe UI", 10), Minimum = 1, Maximum = 500 };
                if (isEdit && row != null) numThoiLuong.Value = Convert.ToInt32(row["ThoiLuong"]);
                else numThoiLuong.Value = 120;
                form.Controls.AddRange(new Control[] { lblThoiLuong, numThoiLuong });
                y += 38;

                Label lblDaoDien = new Label { Text = "Đạo diễn", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
                TextBox txtDaoDien = new TextBox { Location = new Point(inputLeft, y), Size = new Size(280, 28), Font = new Font("Segoe UI", 10) };
                if (isEdit && row != null) txtDaoDien.Text = row["DaoDien"]?.ToString();
                form.Controls.AddRange(new Control[] { lblDaoDien, txtDaoDien });
                y += 38;

                Label lblDienVien = new Label { Text = "Diễn viên", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
                TextBox txtDienVien = new TextBox { Location = new Point(inputLeft, y), Size = new Size(280, 28), Font = new Font("Segoe UI", 10) };
                if (isEdit && row != null) txtDienVien.Text = row["DienVien"]?.ToString();
                form.Controls.AddRange(new Control[] { lblDienVien, txtDienVien });
                y += 38;

                Label lblDoTuoi = new Label { Text = "Phân loại tuổi *", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
                ComboBox cboDoTuoi = new ComboBox { Location = new Point(inputLeft, y), Size = new Size(100, 28), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
                cboDoTuoi.Items.AddRange(new[] { "P", "C13", "C16", "C18" });
                if (isEdit && row != null) cboDoTuoi.SelectedItem = row["DoTuoi"]?.ToString();
                else cboDoTuoi.SelectedIndex = 0;
                form.Controls.AddRange(new Control[] { lblDoTuoi, cboDoTuoi });
                y += 38;

                Label lblNgayKC = new Label { Text = "Ngày khởi chiếu *", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
                DateTimePicker dtpNgayKC = new DateTimePicker { Location = new Point(inputLeft, y), Size = new Size(180, 28), Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short };
                if (isEdit && row != null) dtpNgayKC.Value = Convert.ToDateTime(row["NgayKhoiChieu"]);
                form.Controls.AddRange(new Control[] { lblNgayKC, dtpNgayKC });
                y += 38;

                Label lblMoTa = new Label { Text = "Mô tả", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10) };
                TextBox txtMoTa = new TextBox { Location = new Point(inputLeft, y), Size = new Size(400, 80), Font = new Font("Segoe UI", 10), Multiline = true, ScrollBars = ScrollBars.Vertical };
                if (isEdit && row != null) txtMoTa.Text = row["MoTa"]?.ToString();
                form.Controls.AddRange(new Control[] { lblMoTa, txtMoTa });
                y += 100;

                // Buttons
                Button btnSave = new Button
                {
                    Text = "💾 Lưu",
                    Size = new Size(120, 42),
                    Location = new Point(200, y),
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold)
                };
                btnSave.FlatAppearance.BorderSize = 0;

                Button btnCancel = new Button
                {
                    Text = "Hủy",
                    Size = new Size(100, 42),
                    Location = new Point(340, y),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11)
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                btnCancel.Click += (s, e) => form.Close();

                btnSave.Click += (s, e) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(txtTenPhim.Text))
                        {
                            MessageBox.Show("Vui lòng nhập tên phim!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Save poster if selected
                        string posterUrl = isEdit ? row?["HinhAnh"]?.ToString() : "";
                        if (!string.IsNullOrEmpty(selectedPosterPath))
                        {
                            try
                            {
                                string uploadsDir = Path.Combine(Application.StartupPath, "uploads", "movies");
                                if (!Directory.Exists(uploadsDir))
                                    Directory.CreateDirectory(uploadsDir);

                                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(selectedPosterPath)}";
                                string destPath = Path.Combine(uploadsDir, fileName);
                                File.Copy(selectedPosterPath, destPath, true);
                                posterUrl = fileName;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Lỗi lưu ảnh: {ex.Message}", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }

                        if (isEdit)
                        {
                            _phimBLL.CapNhatPhim(maPhim, txtTenPhim.Text, txtTheLoai.Text,
                                (int)numThoiLuong.Value, txtDaoDien.Text, txtDienVien.Text,
                                txtMoTa.Text, cboDoTuoi.SelectedItem.ToString(), dtpNgayKC.Value);

                            // Update PosterURL if changed
                            if (!string.IsNullOrEmpty(selectedPosterPath))
                            {
                                UpdatePosterUrl(maPhim, posterUrl);
                            }

                            MessageBox.Show("Cập nhật phim thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            _phimBLL.ThemPhim(txtTenPhim.Text, txtTheLoai.Text,
                                (int)numThoiLuong.Value, txtDaoDien.Text, txtDienVien.Text,
                                txtMoTa.Text, cboDoTuoi.SelectedItem.ToString(), dtpNgayKC.Value);

                            MessageBox.Show("Thêm phim thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        form.Close();
                        LoadPhim();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                form.Controls.AddRange(new Control[] { btnSave, btnCancel });
                form.ShowDialog(this);
            }
        }

        private void UploadPosterForMovie(int maPhim, PictureBox poster)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Chọn hình ảnh poster";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Create uploads folder if not exists
                        string uploadsPath = Path.Combine(Application.StartupPath, "uploads", "movies");
                        if (!Directory.Exists(uploadsPath))
                            Directory.CreateDirectory(uploadsPath);

                        // Generate unique filename
                        string ext = Path.GetExtension(ofd.FileName);
                        string newFileName = $"movie_{maPhim}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                        string destPath = Path.Combine(uploadsPath, newFileName);

                        // Copy file
                        File.Copy(ofd.FileName, destPath, true);

                        // Update database
                        UpdatePosterUrl(maPhim, newFileName);

                        // Update poster display
                        if (poster.Image != null)
                            poster.Image.Dispose();
                        poster.Image = Image.FromFile(destPath);

                        MessageBox.Show("✅ Cập nhật poster thành công!", "Thành công", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi upload: {ex.Message}", "Lỗi", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void UpdatePosterUrl(int maPhim, string posterUrl)
        {
            try
            {
                string query = "UPDATE Phim SET HinhAnh = @HinhAnh WHERE MaPhim = @MaPhim";
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HinhAnh", posterUrl);
                        cmd.Parameters.AddWithValue("@MaPhim", maPhim);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating poster: {ex.Message}");
            }
        }

        private void DeleteMovie(int maPhim)
        {
            DataRow row = _dtPhim.Rows.Find(maPhim);
            string tenPhim = row?["TenPhim"]?.ToString() ?? "phim này";

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa '{tenPhim}'?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _phimBLL.XoaPhim(maPhim);
                    MessageBox.Show("Xóa phim thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPhim();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa phim: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            ShowExportDialog();
        }

        private void ShowExportDialog()
        {
            Form exportDialog = new Form
            {
                Text = "Xuất Danh Sách Phim",
                Size = new Size(450, 320),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            // Title
            Label lblTitle = new Label
            {
                Text = "📤 CHỌN ĐỊNH DẠNG XUẤT FILE",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(226, 26, 60),
                AutoSize = true,
                Location = new Point(80, 20)
            };

            // Export format buttons
            int btnY = 70;
            Button btnCsv = CreateExportFormatButton("📊 CSV (Excel)", "Xuất file CSV tương thích Excel", btnY);
            btnCsv.Click += (s, ev) => { exportDialog.Tag = "CSV"; exportDialog.DialogResult = DialogResult.OK; };

            Button btnHtml = CreateExportFormatButton("🌐 HTML", "Xuất báo cáo HTML đẹp mắt", btnY + 60);
            btnHtml.Click += (s, ev) => { exportDialog.Tag = "HTML"; exportDialog.DialogResult = DialogResult.OK; };

            Button btnXml = CreateExportFormatButton("📁 XML (Excel)", "Xuất file XML mở được bằng Excel", btnY + 120);
            btnXml.Click += (s, ev) => { exportDialog.Tag = "XML"; exportDialog.DialogResult = DialogResult.OK; };

            // Cancel button
            Button btnCancel = new Button
            {
                Text = "Hủy",
                Size = new Size(100, 35),
                Location = new Point(170, 240),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCancel.Click += (s, ev) => exportDialog.DialogResult = DialogResult.Cancel;

            exportDialog.Controls.AddRange(new Control[] { lblTitle, btnCsv, btnHtml, btnXml, btnCancel });

            if (exportDialog.ShowDialog() == DialogResult.OK)
            {
                string format = exportDialog.Tag?.ToString();
                PerformExport(format);
            }
        }

        private Button CreateExportFormatButton(string text, string tooltip, int y)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(380, 45),
                Location = new Point(25, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            
            ToolTip tt = new ToolTip();
            tt.SetToolTip(btn, tooltip);
            
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(226, 26, 60);
            btn.MouseEnter += (s, e) => btn.ForeColor = Color.White;
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(240, 240, 240);
            btn.MouseLeave += (s, e) => btn.ForeColor = Color.FromArgb(50, 50, 50);
            
            return btn;
        }

        private void PerformExport(string format)
        {
            try
            {
                string filter = "";
                string ext = "";
                
                switch (format)
                {
                    case "CSV":
                        filter = "CSV Files (*.csv)|*.csv";
                        ext = ".csv";
                        break;
                    case "HTML":
                        filter = "HTML Files (*.html)|*.html";
                        ext = ".html";
                        break;
                    case "XML":
                        filter = "XML Files (*.xml)|*.xml";
                        ext = ".xml";
                        break;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = filter;
                    sfd.FileName = $"DanhSachPhim_CGV_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                    sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = sfd.FileName;
                        
                        switch (format)
                        {
                            case "CSV":
                                ExportToCsv(filePath);
                                break;
                            case "HTML":
                                ExportToHtml(filePath);
                                break;
                            case "XML":
                                ExportToXml(filePath);
                                break;
                        }

                        // Show success with option to open file
                        DialogResult result = MessageBox.Show(
                            $"✅ Xuất file thành công!\n\n📁 File: {filePath}\n\nBạn có muốn mở file không?",
                            "Xuất File Thành Công",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(filePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, new System.Text.UTF8Encoding(true)))
            {
                // Title rows
                sw.WriteLine("HỆ THỐNG QUẢN LÝ CGV CINEMA");
                sw.WriteLine($"DANH SÁCH PHIM - Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm}");
                sw.WriteLine($"Tổng số phim: {_dtPhim.Rows.Count}");
                sw.WriteLine();

                // Header with separator for Excel
                sw.WriteLine("Mã Phim;Tên Phim;Thể Loại;Thời Lượng (phút);Đạo Diễn;Diễn Viên;Độ Tuổi;Ngày Khởi Chiếu;Trạng Thái");

                // Data
                foreach (DataRow row in _dtPhim.Rows)
                {
                    string trangThai = row["TrangThai"]?.ToString() == "1" ? "Đang chiếu" : "Ngừng chiếu";
                    sw.WriteLine($"{row["MaPhim"]};" +
                        $"{EscapeCsvSemicolon(row["TenPhim"])};" +
                        $"{EscapeCsvSemicolon(row["TheLoai"])};" +
                        $"{row["ThoiLuong"]};" +
                        $"{EscapeCsvSemicolon(row["DaoDien"])};" +
                        $"{EscapeCsvSemicolon(row["DienVien"])};" +
                        $"{row["DoTuoi"]};" +
                        $"{Convert.ToDateTime(row["NgayKhoiChieu"]):dd/MM/yyyy};" +
                        $"{trangThai}");
                }
            }
        }

        private void ExportToHtml(string filePath)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='vi'><head><meta charset='UTF-8'>");
            html.AppendLine("<title>Danh Sách Phim - CGV Cinema</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
            html.AppendLine(".header { background: linear-gradient(135deg, #E21A3C, #B71C1C); color: white; padding: 30px; text-align: center; border-radius: 10px; margin-bottom: 20px; }");
            html.AppendLine(".header h1 { margin: 0; font-size: 28px; }");
            html.AppendLine(".header p { margin: 10px 0 0 0; opacity: 0.9; }");
            html.AppendLine(".stats { display: flex; justify-content: center; gap: 30px; margin: 20px 0; }");
            html.AppendLine(".stat-box { background: white; padding: 20px 40px; border-radius: 10px; text-align: center; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine(".stat-box .number { font-size: 32px; font-weight: bold; color: #E21A3C; }");
            html.AppendLine(".stat-box .label { color: #666; margin-top: 5px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine("th { background: #E21A3C; color: white; padding: 15px; text-align: left; font-weight: 600; }");
            html.AppendLine("td { padding: 12px 15px; border-bottom: 1px solid #eee; }");
            html.AppendLine("tr:hover { background: #fff5f5; }");
            html.AppendLine(".status-active { color: #28a745; font-weight: bold; }");
            html.AppendLine(".status-inactive { color: #dc3545; }");
            html.AppendLine(".footer { text-align: center; padding: 20px; color: #666; margin-top: 20px; }");
            html.AppendLine("</style></head><body>");

            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>🎬 HỆ THỐNG QUẢN LÝ CGV CINEMA</h1>");
            html.AppendLine($"<p>Danh sách phim - Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine("</div>");

            // Stats
            int dangChieu = _dtPhim.AsEnumerable().Count(r => r["TrangThai"]?.ToString() == "1");
            html.AppendLine("<div class='stats'>");
            html.AppendLine($"<div class='stat-box'><div class='number'>{_dtPhim.Rows.Count}</div><div class='label'>Tổng số phim</div></div>");
            html.AppendLine($"<div class='stat-box'><div class='number'>{dangChieu}</div><div class='label'>Đang chiếu</div></div>");
            html.AppendLine($"<div class='stat-box'><div class='number'>{_dtPhim.Rows.Count - dangChieu}</div><div class='label'>Ngừng chiếu</div></div>");
            html.AppendLine("</div>");

            // Table
            html.AppendLine("<table><thead><tr>");
            html.AppendLine("<th>STT</th><th>Tên Phim</th><th>Thể Loại</th><th>Thời Lượng</th><th>Đạo Diễn</th><th>Độ Tuổi</th><th>Ngày Khởi Chiếu</th><th>Trạng Thái</th>");
            html.AppendLine("</tr></thead><tbody>");

            int stt = 1;
            foreach (DataRow row in _dtPhim.Rows)
            {
                bool isActive = row["TrangThai"]?.ToString() == "1";
                string statusClass = isActive ? "status-active" : "status-inactive";
                string statusText = isActive ? "✅ Đang chiếu" : "❌ Ngừng chiếu";

                html.AppendLine("<tr>");
                html.AppendLine($"<td>{stt++}</td>");
                html.AppendLine($"<td><strong>{EscapeHtml(row["TenPhim"])}</strong></td>");
                html.AppendLine($"<td>{EscapeHtml(row["TheLoai"])}</td>");
                html.AppendLine($"<td>{row["ThoiLuong"]} phút</td>");
                html.AppendLine($"<td>{EscapeHtml(row["DaoDien"])}</td>");
                html.AppendLine($"<td>{row["DoTuoi"]}</td>");
                html.AppendLine($"<td>{Convert.ToDateTime(row["NgayKhoiChieu"]):dd/MM/yyyy}</td>");
                html.AppendLine($"<td class='{statusClass}'>{statusText}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
            html.AppendLine($"<div class='footer'>© {DateTime.Now.Year} CGV Cinema Vietnam - Hệ thống quản lý rạp phim</div>");
            html.AppendLine("</body></html>");

            File.WriteAllText(filePath, html.ToString(), System.Text.Encoding.UTF8);
        }

        private void ExportToXml(string filePath)
        {
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            xml.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            xml.AppendLine("  xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");

            // Styles
            xml.AppendLine("<Styles>");
            xml.AppendLine("<Style ss:ID=\"Header\"><Font ss:Bold=\"1\" ss:Color=\"#FFFFFF\" ss:Size=\"11\"/><Interior ss:Color=\"#E21A3C\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/></Style>");
            xml.AppendLine("<Style ss:ID=\"Title\"><Font ss:Bold=\"1\" ss:Color=\"#E21A3C\" ss:Size=\"16\"/><Alignment ss:Horizontal=\"Center\"/></Style>");
            xml.AppendLine("<Style ss:ID=\"SubTitle\"><Font ss:Color=\"#666666\" ss:Size=\"10\"/><Alignment ss:Horizontal=\"Center\"/></Style>");
            xml.AppendLine("<Style ss:ID=\"Data\"><Alignment ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#EEEEEE\"/></Borders></Style>");
            xml.AppendLine("<Style ss:ID=\"Active\"><Font ss:Color=\"#28A745\" ss:Bold=\"1\"/></Style>");
            xml.AppendLine("<Style ss:ID=\"Inactive\"><Font ss:Color=\"#DC3545\"/></Style>");
            xml.AppendLine("</Styles>");

            // Worksheet
            xml.AppendLine("<Worksheet ss:Name=\"Danh Sách Phim\">");
            xml.AppendLine("<Table>");

            // Column widths
            xml.AppendLine("<Column ss:Width=\"50\"/><Column ss:Width=\"200\"/><Column ss:Width=\"150\"/><Column ss:Width=\"80\"/>");
            xml.AppendLine("<Column ss:Width=\"150\"/><Column ss:Width=\"200\"/><Column ss:Width=\"70\"/><Column ss:Width=\"100\"/><Column ss:Width=\"100\"/>");

            // Title rows
            xml.AppendLine("<Row ss:Height=\"30\"><Cell ss:MergeAcross=\"8\" ss:StyleID=\"Title\"><Data ss:Type=\"String\">🎬 HỆ THỐNG QUẢN LÝ CGV CINEMA</Data></Cell></Row>");
            xml.AppendLine($"<Row><Cell ss:MergeAcross=\"8\" ss:StyleID=\"SubTitle\"><Data ss:Type=\"String\">Danh sách phim - Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm} - Tổng: {_dtPhim.Rows.Count} phim</Data></Cell></Row>");
            xml.AppendLine("<Row></Row>");

            // Header
            xml.AppendLine("<Row ss:Height=\"25\">");
            string[] headers = { "Mã", "Tên Phim", "Thể Loại", "Thời Lượng", "Đạo Diễn", "Diễn Viên", "Độ Tuổi", "Ngày KC", "Trạng Thái" };
            foreach (string h in headers)
                xml.AppendLine($"<Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">{h}</Data></Cell>");
            xml.AppendLine("</Row>");

            // Data rows
            foreach (DataRow row in _dtPhim.Rows)
            {
                string trangThai = row["TrangThai"]?.ToString() == "1" ? "Đang chiếu" : "Ngừng chiếu";
                string styleId = row["TrangThai"]?.ToString() == "1" ? "Active" : "Inactive";

                xml.AppendLine("<Row>");
                xml.AppendLine($"<Cell ss:StyleID=\"Data\"><Data ss:Type=\"Number\">{row["MaPhim"]}</Data></Cell>");
                xml.AppendLine($"<Cell ss:StyleID=\"Data\"><Data ss:Type=\"String\">{EscapeXml(row["TenPhim"])}</Data></Cell>");
                xml.AppendLine($"<Cell ss:StyleID=\"Data\"><Data ss:Type=\"String\">{EscapeXml(row["TheLoai"])}</Data></Cell>");
                xml.AppendLine($"<Cell ss:StyleID=\"Data\"><Data ss:Type=\"Number\">{row["ThoiLuong"]}</Data></Cell>");
                xml.AppendLine($"<Cell ss:StyleID=\"Data\"><Data ss:Type=\"String\">{EscapeXml(row["DaoDien"])}</Data></Cell>");
                xml.AppendLine($"<Cell ss:StyleID=\"Data\"><Data ss:Type=\"String\">{EscapeXml(row["DienVien"])}</Data></Cell>");
                xml.AppendLine($"<Cell ss:StyleID=\"Data\"><Data ss:Type=\"String\">{row["DoTuoi"]}</Data></Cell>");
                xml.AppendLine($"<Cell ss:StyleID=\"Data\"><Data ss:Type=\"String\">{Convert.ToDateTime(row["NgayKhoiChieu"]):dd/MM/yyyy}</Data></Cell>");
                xml.AppendLine($"<Cell ss:StyleID=\"{styleId}\"><Data ss:Type=\"String\">{trangThai}</Data></Cell>");
                xml.AppendLine("</Row>");
            }

            xml.AppendLine("</Table></Worksheet></Workbook>");
            File.WriteAllText(filePath, xml.ToString(), System.Text.Encoding.UTF8);
        }

        private string EscapeCsvSemicolon(object value)
        {
            string s = value?.ToString() ?? "";
            if (s.Contains(";") || s.Contains("\"") || s.Contains("\n"))
            {
                return $"\"{s.Replace("\"", "\"\"")}\"";
            }
            return s;
        }

        private string EscapeXml(object value)
        {
            return System.Security.SecurityElement.Escape(value?.ToString() ?? "");
        }

        private string EscapeHtml(object value)
        {
            string s = value?.ToString() ?? "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
