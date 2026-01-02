using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// Modal form to display detailed movie information with actions
    /// </summary>
    public class frmMovieDetail : Form
    {
        // CGV Branding Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

        private DataRow _movieData;
        private PhimBLL _phimBLL;
        private PictureBox _poster;
        private bool _isEditMode = false;

        // Edit controls
        private TextBox _txtTenPhim, _txtMoTa, _txtDaoDien, _txtDienVien, _txtNgonNgu;
        private NumericUpDown _nudThoiLuong;
        private ComboBox _cboTheLoai, _cboDoTuoi, _cboTrangThai;
        private DateTimePicker _dtpNgayKhoiChieu;

        public frmMovieDetail(DataRow movieData)
        {
            _movieData = movieData;
            _phimBLL = new PhimBLL();
            InitializeComponent();
            LoadMovieData();
        }

        private void InitializeComponent()
        {
            this.Text = "Chi tiết phim";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = _cgvLightGray;
            this.Font = new Font("Segoe UI", 10);

            // Main container with padding
            Panel mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = _cgvLightGray
            };

            // Left panel - Poster
            Panel leftPanel = new Panel
            {
                Width = 300,
                Dock = DockStyle.Left,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 20, 0)
            };

            _poster = new PictureBox
            {
                Size = new Size(280, 400),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.DarkGray
            };
            AddRoundedCorners(_poster, 15);
            leftPanel.Controls.Add(_poster);

            // Age rating badge
            string doTuoi = _movieData["DoTuoi"]?.ToString() ?? "P";
            Panel ageBadge = CreateAgeBadge(doTuoi);
            ageBadge.Location = new Point(235, 10);
            leftPanel.Controls.Add(ageBadge);
            ageBadge.BringToFront();

            // Status badge
            string trangThai = _movieData["TrangThai"]?.ToString() ?? "Đang chiếu";
            Panel statusBadge = CreateStatusBadge(trangThai);
            statusBadge.Location = new Point(0, 410);
            leftPanel.Controls.Add(statusBadge);

            // Action buttons panel
            Panel actionPanel = new Panel
            {
                Location = new Point(0, 460),
                Size = new Size(280, 180),
                BackColor = Color.Transparent
            };

            Button btnEdit = CreateActionButton("✏️ CHỈNH SỬA", _cgvRed);
            btnEdit.Location = new Point(0, 0);
            btnEdit.Click += BtnEdit_Click;
            actionPanel.Controls.Add(btnEdit);

            Button btnSchedule = CreateActionButton("📅 XẾP LỊCH CHIẾU", Color.FromArgb(41, 128, 185));
            btnSchedule.Location = new Point(0, 50);
            btnSchedule.Click += BtnSchedule_Click;
            actionPanel.Controls.Add(btnSchedule);

            Button btnStats = CreateActionButton("📊 XEM THỐNG KÊ", Color.FromArgb(39, 174, 96));
            btnStats.Location = new Point(0, 100);
            btnStats.Click += BtnStats_Click;
            actionPanel.Controls.Add(btnStats);

            Button btnDelete = CreateActionButton("🗑️ XÓA PHIM", Color.FromArgb(192, 57, 43));
            btnDelete.Location = new Point(0, 150);
            btnDelete.Click += BtnDelete_Click;
            actionPanel.Controls.Add(btnDelete);

            leftPanel.Controls.Add(actionPanel);

            // Right panel - Details
            Panel rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(25)
            };
            AddRoundedCorners(rightPanel, 15);

            // Movie title
            _txtTenPhim = new TextBox
            {
                Text = _movieData["TenPhim"]?.ToString() ?? "N/A",
                Font = new Font("Montserrat", 20, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(25, 25),
                Width = 500,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = Color.White
            };
            rightPanel.Controls.Add(_txtTenPhim);

            // Rating & Duration row
            Panel infoRow = new Panel
            {
                Location = new Point(25, 70),
                Size = new Size(500, 30),
                BackColor = Color.Transparent
            };

            Label lblRating = new Label
            {
                Text = $"⭐ {_movieData["DanhGia"]?.ToString() ?? "N/A"}/10",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _cgvGold,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            infoRow.Controls.Add(lblRating);

            Label lblDuration = new Label
            {
                Text = $"⏱️ {_movieData["ThoiLuong"]?.ToString() ?? "N/A"} phút",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(120, 0)
            };
            infoRow.Controls.Add(lblDuration);

            Label lblGenre = new Label
            {
                Text = $"🎭 {_movieData["TheLoai"]?.ToString() ?? "N/A"}",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(250, 0)
            };
            infoRow.Controls.Add(lblGenre);

            rightPanel.Controls.Add(infoRow);

            // Separator
            Panel separator = new Panel
            {
                Location = new Point(25, 110),
                Size = new Size(500, 2),
                BackColor = _cgvLightGray
            };
            rightPanel.Controls.Add(separator);

            // Details form
            int y = 130;

            // Director
            rightPanel.Controls.Add(CreateDetailLabel("Đạo diễn:", new Point(25, y)));
            _txtDaoDien = CreateDetailTextBox(_movieData["DaoDien"]?.ToString() ?? "N/A", new Point(130, y - 3));
            rightPanel.Controls.Add(_txtDaoDien);
            y += 35;

            // Cast
            rightPanel.Controls.Add(CreateDetailLabel("Diễn viên:", new Point(25, y)));
            _txtDienVien = CreateDetailTextBox(_movieData["DienVien"]?.ToString() ?? "N/A", new Point(130, y - 3));
            rightPanel.Controls.Add(_txtDienVien);
            y += 35;

            // Language
            rightPanel.Controls.Add(CreateDetailLabel("Ngôn ngữ:", new Point(25, y)));
            _txtNgonNgu = CreateDetailTextBox(_movieData["NgonNgu"]?.ToString() ?? "Tiếng Việt", new Point(130, y - 3));
            rightPanel.Controls.Add(_txtNgonNgu);
            y += 35;

            // Release date
            rightPanel.Controls.Add(CreateDetailLabel("Khởi chiếu:", new Point(25, y)));
            DateTime ngayKhoiChieu = DateTime.Now;
            if (_movieData["NgayKhoiChieu"] != DBNull.Value)
                DateTime.TryParse(_movieData["NgayKhoiChieu"].ToString(), out ngayKhoiChieu);
            _dtpNgayKhoiChieu = new DateTimePicker
            {
                Value = ngayKhoiChieu,
                Format = DateTimePickerFormat.Short,
                Location = new Point(130, y - 3),
                Width = 150,
                Enabled = false
            };
            rightPanel.Controls.Add(_dtpNgayKhoiChieu);
            y += 45;

            // Description
            rightPanel.Controls.Add(CreateDetailLabel("Mô tả:", new Point(25, y)));
            y += 25;
            _txtMoTa = new TextBox
            {
                Text = _movieData["MoTa"]?.ToString() ?? "Chưa có mô tả",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray,
                Location = new Point(25, y),
                Size = new Size(500, 120),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                BackColor = Color.White
            };
            rightPanel.Controls.Add(_txtMoTa);

            // Bottom buttons
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = _cgvLightGray
            };

            Button btnSave = new Button
            {
                Text = "💾 LƯU THAY ĐỔI",
                Size = new Size(150, 40),
                Location = new Point(600, 10),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false,
                Name = "btnSave"
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            bottomPanel.Controls.Add(btnSave);

            Button btnCancel = new Button
            {
                Text = "❌ HỦY",
                Size = new Size(100, 40),
                Location = new Point(760, 10),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false,
                Name = "btnCancel"
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;
            bottomPanel.Controls.Add(btnCancel);

            Button btnClose = new Button
            {
                Text = "ĐÓNG",
                Size = new Size(100, 40),
                Location = new Point(760, 10),
                BackColor = _cgvBlack,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Name = "btnClose"
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            bottomPanel.Controls.Add(btnClose);

            mainContainer.Controls.Add(rightPanel);
            mainContainer.Controls.Add(leftPanel);

            this.Controls.Add(bottomPanel);
            this.Controls.Add(mainContainer);
        }

        private void LoadMovieData()
        {
            // Load poster image
            string posterPath = _movieData["HinhAnh"]?.ToString();
            if (!string.IsNullOrEmpty(posterPath))
            {
                try
                {
                    string localPath = Path.Combine(Application.StartupPath, "uploads", "movies", Path.GetFileName(posterPath));
                    if (File.Exists(localPath))
                    {
                        _poster.Image = Image.FromFile(localPath);
                    }
                    else
                    {
                        _poster.Image = CreatePlaceholderPoster(_movieData["TenPhim"]?.ToString() ?? "Movie");
                    }
                }
                catch
                {
                    _poster.Image = CreatePlaceholderPoster(_movieData["TenPhim"]?.ToString() ?? "Movie");
                }
            }
            else
            {
                _poster.Image = CreatePlaceholderPoster(_movieData["TenPhim"]?.ToString() ?? "Movie");
            }
        }

        private Label CreateDetailLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = location,
                AutoSize = true
            };
        }

        private TextBox CreateDetailTextBox(string text, Point location)
        {
            return new TextBox
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray,
                Location = location,
                Width = 395,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = Color.White
            };
        }

        private Panel CreateAgeBadge(string doTuoi)
        {
            Color badgeColor = doTuoi switch
            {
                "C18" => Color.Red,
                "C16" => Color.OrangeRed,
                "C13" => Color.Orange,
                _ => Color.Green
            };

            Panel badge = new Panel
            {
                Size = new Size(40, 30),
                BackColor = badgeColor
            };

            Label lbl = new Label
            {
                Text = doTuoi,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            badge.Controls.Add(lbl);

            return badge;
        }

        private Panel CreateStatusBadge(string status)
        {
            Color bgColor = status switch
            {
                "Đang chiếu" => Color.FromArgb(39, 174, 96),
                "Sắp chiếu" => Color.FromArgb(41, 128, 185),
                _ => Color.Gray
            };

            Panel badge = new Panel
            {
                Size = new Size(280, 35),
                BackColor = bgColor
            };
            AddRoundedCorners(badge, 8);

            Label lbl = new Label
            {
                Text = status.ToUpper(),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            badge.Controls.Add(lbl);

            return badge;
        }

        private Button CreateActionButton(string text, Color bgColor)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(280, 40),
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Image CreatePlaceholderPoster(string title)
        {
            Bitmap bmp = new Bitmap(280, 400);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(60, 60, 60));

                // Movie icon
                using (Font iconFont = new Font("Segoe UI", 48))
                {
                    string icon = "🎬";
                    SizeF iconSize = g.MeasureString(icon, iconFont);
                    g.DrawString(icon, iconFont, Brushes.White, 
                        (280 - iconSize.Width) / 2, 150);
                }

                // Title
                using (Font titleFont = new Font("Segoe UI", 12, FontStyle.Bold))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(title, titleFont, Brushes.White, 
                        new RectangleF(10, 280, 260, 100), sf);
                }
            }
            return bmp;
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

        #region Event Handlers

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            _isEditMode = !_isEditMode;
            ToggleEditMode(_isEditMode);
        }

        private void ToggleEditMode(bool editMode)
        {
            _txtTenPhim.ReadOnly = !editMode;
            _txtTenPhim.BackColor = editMode ? Color.FromArgb(255, 255, 230) : Color.White;
            
            _txtDaoDien.ReadOnly = !editMode;
            _txtDaoDien.BackColor = editMode ? Color.FromArgb(255, 255, 230) : Color.White;
            
            _txtDienVien.ReadOnly = !editMode;
            _txtDienVien.BackColor = editMode ? Color.FromArgb(255, 255, 230) : Color.White;
            
            _txtNgonNgu.ReadOnly = !editMode;
            _txtNgonNgu.BackColor = editMode ? Color.FromArgb(255, 255, 230) : Color.White;
            
            _txtMoTa.ReadOnly = !editMode;
            _txtMoTa.BackColor = editMode ? Color.FromArgb(255, 255, 230) : Color.White;
            
            _dtpNgayKhoiChieu.Enabled = editMode;

            // Show/hide save buttons
            var btnSave = this.Controls.Find("btnSave", true);
            var btnCancel = this.Controls.Find("btnCancel", true);
            var btnClose = this.Controls.Find("btnClose", true);

            if (btnSave.Length > 0) btnSave[0].Visible = editMode;
            if (btnCancel.Length > 0) btnCancel[0].Visible = editMode;
            if (btnClose.Length > 0) btnClose[0].Visible = !editMode;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int maPhim = Convert.ToInt32(_movieData["MaPhim"]);
                
                // Update movie data
                _movieData["TenPhim"] = _txtTenPhim.Text;
                _movieData["DaoDien"] = _txtDaoDien.Text;
                _movieData["DienVien"] = _txtDienVien.Text;
                _movieData["NgonNgu"] = _txtNgonNgu.Text;
                _movieData["MoTa"] = _txtMoTa.Text;
                _movieData["NgayKhoiChieu"] = _dtpNgayKhoiChieu.Value;

                // Call BLL to update (9 arguments: maPhim, tenPhim, theLoai, thoiLuong, daoDien, dienVien, moTa, doTuoi, ngayKhoiChieu)
                bool success = _phimBLL.CapNhatPhim(
                    maPhim,
                    _txtTenPhim.Text,
                    _movieData["TheLoai"]?.ToString() ?? "",
                    Convert.ToInt32(_movieData["ThoiLuong"]),
                    _txtDaoDien.Text,
                    _txtDienVien.Text,
                    _txtMoTa.Text,
                    _movieData["DoTuoi"]?.ToString() ?? "",
                    _dtpNgayKhoiChieu.Value
                );

                if (success)
                {
                    MessageBox.Show("Cập nhật thông tin phim thành công!", "Thành công", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ToggleEditMode(false);
                    _isEditMode = false;
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật thông tin phim!", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Restore original values
            _txtTenPhim.Text = _movieData["TenPhim"]?.ToString() ?? "";
            _txtDaoDien.Text = _movieData["DaoDien"]?.ToString() ?? "";
            _txtDienVien.Text = _movieData["DienVien"]?.ToString() ?? "";
            _txtNgonNgu.Text = _movieData["NgonNgu"]?.ToString() ?? "";
            _txtMoTa.Text = _movieData["MoTa"]?.ToString() ?? "";
            
            ToggleEditMode(false);
            _isEditMode = false;
        }

        private void BtnSchedule_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Mở form xếp lịch chiếu cho phim: {_movieData["TenPhim"]}", 
                "Xếp lịch chiếu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Open UC_ShowtimeManagement with pre-selected movie
        }

        private void BtnStats_Click(object sender, EventArgs e)
        {
            int maPhim = Convert.ToInt32(_movieData["MaPhim"]);
            string tenPhim = _movieData["TenPhim"]?.ToString() ?? "N/A";
            
            // Show basic stats dialog
            string stats = $"📊 THỐNG KÊ PHIM: {tenPhim}\n\n" +
                          $"• Số suất chiếu: Đang tính...\n" +
                          $"• Tổng vé bán: Đang tính...\n" +
                          $"• Doanh thu: Đang tính...\n" +
                          $"• Tỷ lệ lấp đầy: Đang tính...";
            
            MessageBox.Show(stats, "Thống kê phim", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            string tenPhim = _movieData["TenPhim"]?.ToString() ?? "";
            
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc muốn xóa phim '{tenPhim}'?\n\nHành động này không thể hoàn tác!",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int maPhim = Convert.ToInt32(_movieData["MaPhim"]);
                    bool success = _phimBLL.XoaPhim(maPhim);
                    
                    if (success)
                    {
                        MessageBox.Show("Đã xóa phim thành công!", "Thành công", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa phim. Phim có thể đang có suất chiếu.", 
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }
}
