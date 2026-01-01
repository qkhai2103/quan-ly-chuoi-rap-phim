using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// UC_ShowtimeManagement - UserControl Quản Lý Suất Chiếu
    /// Chức năng: Thêm, sửa, xóa suất chiếu. Xem lịch chiếu theo ngày, phòng, phim
    /// </summary>
    public partial class UC_ShowtimeManagement : UserControl
    {
        // UI Components
        private DataGridView dgvShowtimes;
        private DateTimePicker dtpDate;
        private ComboBox cboBranch, cboRoom, cboMovie;
        private Label lblTotalShowtimes, lblTodayShowtimes;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        // Data
        private DataTable dtShowtimes;
        private DataTable dtMovies;
        private DataTable dtRooms;
        private DataTable dtBranches;

        // Colors
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private Color _cgvTextColor = Color.FromArgb(33, 33, 33);

        public UC_ShowtimeManagement()
        {
            InitializeComponent();
            SetupUI();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_ShowtimeManagement";
            this.Size = new Size(1200, 800);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);
            this.Dock = DockStyle.Fill;

            // ========== HEADER ==========
            Label lblTitle = new Label
            {
                Text = "📅 QUẢN LÝ SUẤT CHIẾU",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20)
            };

            // ========== STATS PANEL ==========
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 10, 0, 20) };
            TableLayoutPanel statsGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var card1 = CreateStatCard("SUẤT CHIẾU HÔM NAY", "0", _cgvRed, out lblTodayShowtimes);
            var card2 = CreateStatCard("TỔNG SUẤT CHIẾU", "0", Color.FromArgb(39, 174, 96), out lblTotalShowtimes);
            var card3 = CreateStatCard("PHÒNG CHIẾU", "0", Color.FromArgb(52, 152, 219), out Label lblRooms);

            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsGrid.Controls.Add(card3, 2, 0);
            statsPanel.Controls.Add(statsGrid);

            // ========== FILTER TOOLBAR ==========
            Panel filterPanel = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            filterPanel.BorderRadius(12);

            // Date picker
            Label lblDate = new Label { Text = "Ngày chiếu:", Font = new Font("Segoe UI", 10), Location = new Point(20, 22), AutoSize = true };
            dtpDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(150, 35),
                Location = new Point(100, 18),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                MinDate = DateTime.Today.AddYears(-2),
                MaxDate = DateTime.Today.AddYears(1)
            };
            dtpDate.ValueChanged += (s, e) => LoadShowtimes();

            // Branch filter
            Label lblBranch = new Label { Text = "Chi nhánh:", Font = new Font("Segoe UI", 10), Location = new Point(270, 22), AutoSize = true };
            cboBranch = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(180, 35),
                Location = new Point(350, 18),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboBranch.SelectedIndexChanged += (s, e) => { LoadRooms(); LoadShowtimes(); };

            // Room filter
            Label lblRoom = new Label { Text = "Phòng:", Font = new Font("Segoe UI", 10), Location = new Point(550, 22), AutoSize = true };
            cboRoom = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(150, 35),
                Location = new Point(600, 18),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRoom.SelectedIndexChanged += (s, e) => LoadShowtimes();

            filterPanel.Controls.AddRange(new Control[] { lblDate, dtpDate, lblBranch, cboBranch, lblRoom, cboRoom });

            // ========== ACTION TOOLBAR ==========
            Panel toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(20, 10, 20, 10), Margin = new Padding(0, 10, 0, 0) };
            toolBar.BorderRadius(12);

            btnAdd = CreateButton("➕ THÊM SUẤT", _cgvRed);
            btnAdd.Location = new Point(toolBar.Width - 160, 10);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += BtnAdd_Click;

            btnEdit = CreateButton("✏️ SỬA", Color.FromArgb(52, 152, 219));
            btnEdit.Location = new Point(toolBar.Width - 310, 10);
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.Click += BtnEdit_Click;

            btnDelete = CreateButton("🗑️ XÓA", Color.FromArgb(231, 76, 60));
            btnDelete.Location = new Point(toolBar.Width - 460, 10);
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = CreateButton("🔄 LÀM MỚI", Color.FromArgb(70, 70, 70));
            btnRefresh.Location = new Point(toolBar.Width - 610, 10);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (s, e) => LoadShowtimes();

            toolBar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

            // ========== DATA GRID ==========
            Panel gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            gridPanel.BorderRadius(12);

            dgvShowtimes = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 50 },
                GridColor = Color.FromArgb(240, 240, 240),
                EnableHeadersVisualStyles = false
            };

            dgvShowtimes.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI Semibold", 10),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            dgvShowtimes.ColumnHeadersHeight = 50;

            dgvShowtimes.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 10),
                ForeColor = _cgvTextColor,
                SelectionBackColor = Color.FromArgb(255, 235, 238),
                SelectionForeColor = _cgvRed,
                Padding = new Padding(10, 0, 0, 0)
            };

            dgvShowtimes.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            gridPanel.Controls.Add(dgvShowtimes);

            // ========== ASSEMBLE ==========
            this.Controls.Add(gridPanel);
            Panel spacer1 = new Panel { Dock = DockStyle.Top, Height = 15 };
            this.Controls.Add(spacer1);
            this.Controls.Add(toolBar);
            Panel spacer2 = new Panel { Dock = DockStyle.Top, Height = 15 };
            this.Controls.Add(spacer2);
            this.Controls.Add(filterPanel);
            this.Controls.Add(statsPanel);
            this.Controls.Add(lblTitle);
        }

        private Panel CreateStatCard(string title, string value, Color accentColor, out Label valueLabel)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 20, 0) };
            card.BorderRadius(15);
            Panel accent = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accentColor };
            card.Controls.Add(accent);
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.Gray, Location = new Point(25, 20), AutoSize = true };
            valueLabel = new Label { Text = value, Font = new Font("Montserrat", 22, FontStyle.Bold), ForeColor = _cgvBlack, Location = new Point(22, 45), AutoSize = true };
            card.Controls.AddRange(new Control[] { lblTitle, valueLabel });
            return card;
        }

        private Button CreateButton(string text, Color backColor)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 40),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.BorderRadius(8);
            return btn;
        }

        private void LoadData()
        {
            LoadBranches();
            LoadMovies();
            
            // Tìm ngày gần nhất có suất chiếu
            FindNearestShowtimeDate();
            LoadShowtimes();
        }

        private void FindNearestShowtimeDate()
        {
            try
            {
                // Kiểm tra xem hôm nay có suất chiếu không
                string checkQuery = @"
                    SELECT TOP 1 NgayChieu FROM SuatChieu 
                    WHERE NgayChieu >= @Today
                    ORDER BY NgayChieu ASC";
                
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Today", DateTime.Today);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            dtpDate.Value = Convert.ToDateTime(result);
                            return;
                        }
                    }

                    // Nếu không có suất chiếu trong tương lai, tìm suất chiếu gần nhất trong quá khứ
                    string pastQuery = @"
                        SELECT TOP 1 NgayChieu FROM SuatChieu 
                        ORDER BY NgayChieu DESC";
                    using (SqlCommand cmd = new SqlCommand(pastQuery, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            dtpDate.Value = Convert.ToDateTime(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding nearest showtime: {ex.Message}");
            }
        }

        private void LoadBranches()
        {
            try
            {
                string query = "SELECT MaChiNhanh, TenChiNhanh FROM ChiNhanh WHERE TrangThai = 1 ORDER BY TenChiNhanh";
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    dtBranches = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dtBranches);
                    }
                }

                cboBranch.Items.Clear();
                cboBranch.Items.Add("Tất cả");
                foreach (DataRow row in dtBranches.Rows)
                {
                    cboBranch.Items.Add(row["TenChiNhanh"].ToString());
                }
                cboBranch.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi nhánh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRooms()
        {
            try
            {
                cboRoom.Items.Clear();
                cboRoom.Items.Add("Tất cả");

                if (cboBranch.SelectedIndex <= 0) return;

                int maChiNhanh = Convert.ToInt32(dtBranches.Rows[cboBranch.SelectedIndex - 1]["MaChiNhanh"]);
                string query = "SELECT MaPhong, TenPhong FROM PhongChieu WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    dtRooms = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dtRooms);
                        }
                    }
                }

                foreach (DataRow row in dtRooms.Rows)
                {
                    cboRoom.Items.Add(row["TenPhong"].ToString());
                }
                cboRoom.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải phòng chiếu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMovies()
        {
            try
            {
                string query = "SELECT MaPhim, TenPhim, ThoiLuong FROM Phim WHERE TrangThai = 1 ORDER BY TenPhim";
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    dtMovies = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dtMovies);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách phim: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadShowtimes()
        {
            try
            {
                string query = @"
                    SELECT 
                        sc.MaSuatChieu,
                        p.TenPhim,
                        pc.TenPhong,
                        cn.TenChiNhanh,
                        sc.NgayChieu,
                        sc.GioChieu,
                        sc.GiaVe,
                        sc.TrangThai,
                        p.ThoiLuong
                    FROM SuatChieu sc
                    INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                    INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                    INNER JOIN ChiNhanh cn ON pc.MaChiNhanh = cn.MaChiNhanh
                    WHERE sc.NgayChieu = @NgayChieu";

                // Add branch filter
                if (cboBranch.SelectedIndex > 0)
                {
                    int maChiNhanh = Convert.ToInt32(dtBranches.Rows[cboBranch.SelectedIndex - 1]["MaChiNhanh"]);
                    query += " AND cn.MaChiNhanh = " + maChiNhanh;
                }

                // Add room filter
                if (cboRoom.SelectedIndex > 0 && dtRooms != null && dtRooms.Rows.Count >= cboRoom.SelectedIndex)
                {
                    int maPhong = Convert.ToInt32(dtRooms.Rows[cboRoom.SelectedIndex - 1]["MaPhong"]);
                    query += " AND sc.MaPhong = " + maPhong;
                }

                query += " ORDER BY sc.GioChieu";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    dtShowtimes = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NgayChieu", dtpDate.Value.Date);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dtShowtimes);
                        }
                    }
                }

                BindDataToGrid();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải suất chiếu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindDataToGrid()
        {
            dgvShowtimes.Columns.Clear();
            dgvShowtimes.Rows.Clear();

            dgvShowtimes.Columns.Add("MaSuatChieu", "ID");
            dgvShowtimes.Columns.Add("TenPhim", "Phim");
            dgvShowtimes.Columns.Add("TenPhong", "Phòng");
            dgvShowtimes.Columns.Add("TenChiNhanh", "Chi Nhánh");
            dgvShowtimes.Columns.Add("GioChieu", "Giờ Chiếu");
            dgvShowtimes.Columns.Add("ThoiLuong", "Thời Lượng");
            dgvShowtimes.Columns.Add("GiaVe", "Giá Vé");
            dgvShowtimes.Columns.Add("TrangThai", "Trạng Thái");

            dgvShowtimes.Columns["MaSuatChieu"].Width = 60;
            dgvShowtimes.Columns["TenPhim"].Width = 200;
            dgvShowtimes.Columns["GiaVe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            foreach (DataRow row in dtShowtimes.Rows)
            {
                string trangThai = row["TrangThai"].ToString();
                string trangThaiDisplay = trangThai switch
                {
                    "SapChieu" => "🕐 Sắp chiếu",
                    "DangChieu" => "🎬 Đang chiếu",
                    "KetThuc" => "✅ Kết thúc",
                    _ => trangThai
                };

                dgvShowtimes.Rows.Add(
                    row["MaSuatChieu"],
                    row["TenPhim"],
                    row["TenPhong"],
                    row["TenChiNhanh"],
                    Convert.ToDateTime(row["GioChieu"].ToString()).ToString("HH:mm"),
                    row["ThoiLuong"] + " phút",
                    Convert.ToDecimal(row["GiaVe"]).ToString("N0") + " ₫",
                    trangThaiDisplay
                );
            }
        }

        private void UpdateStats()
        {
            lblTodayShowtimes.Text = dtShowtimes?.Rows.Count.ToString() ?? "0";

            // Get total showtimes
            try
            {
                string query = "SELECT COUNT(*) FROM SuatChieu";
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        lblTotalShowtimes.Text = cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch { }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowShowtimeDialog(0); // 0 = new
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvShowtimes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn suất chiếu cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maSuatChieu = Convert.ToInt32(dgvShowtimes.SelectedRows[0].Cells["MaSuatChieu"].Value);
            ShowShowtimeDialog(maSuatChieu);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvShowtimes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn suất chiếu cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maSuatChieu = Convert.ToInt32(dgvShowtimes.SelectedRows[0].Cells["MaSuatChieu"].Value);
            string tenPhim = dgvShowtimes.SelectedRows[0].Cells["TenPhim"].Value.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa suất chiếu phim '{tenPhim}'?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // Check if tickets have been sold
                    string checkQuery = "SELECT COUNT(*) FROM Ve WHERE MaSuatChieu = @MaSuatChieu AND TrangThai = N'DaBan'";
                    using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaSuatChieu", maSuatChieu);
                            int ticketCount = Convert.ToInt32(cmd.ExecuteScalar());
                            if (ticketCount > 0)
                            {
                                MessageBox.Show($"Không thể xóa! Suất chiếu này đã bán {ticketCount} vé.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        string deleteQuery = "DELETE FROM SuatChieu WHERE MaSuatChieu = @MaSuatChieu";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaSuatChieu", maSuatChieu);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Xóa suất chiếu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadShowtimes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa suất chiếu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowShowtimeDialog(int maSuatChieu)
        {
            using (Form frm = new Form())
            {
                frm.Text = maSuatChieu == 0 ? "Thêm Suất Chiếu Mới" : "Sửa Suất Chiếu";
                frm.Size = new Size(500, 450);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = false;
                frm.MinimizeBox = false;
                frm.BackColor = Color.White;

                int y = 20;
                int lblWidth = 120;
                int ctrlWidth = 320;

                // Movie
                Label lblPhim = new Label { Text = "Phim:", Location = new Point(20, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                ComboBox cboPhim = new ComboBox { Location = new Point(lblWidth + 20, y), Size = new Size(ctrlWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
                foreach (DataRow row in dtMovies.Rows)
                    cboPhim.Items.Add(row["TenPhim"].ToString());
                if (cboPhim.Items.Count > 0) cboPhim.SelectedIndex = 0;

                y += 45;

                // Branch
                Label lblCN = new Label { Text = "Chi nhánh:", Location = new Point(20, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                ComboBox cboCN = new ComboBox { Location = new Point(lblWidth + 20, y), Size = new Size(ctrlWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
                foreach (DataRow row in dtBranches.Rows)
                    cboCN.Items.Add(row["TenChiNhanh"].ToString());
                if (cboCN.Items.Count > 0) cboCN.SelectedIndex = 0;

                y += 45;

                // Room
                Label lblPhong = new Label { Text = "Phòng chiếu:", Location = new Point(20, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                ComboBox cboPhong = new ComboBox { Location = new Point(lblWidth + 20, y), Size = new Size(ctrlWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

                cboCN.SelectedIndexChanged += (s, e) =>
                {
                    cboPhong.Items.Clear();
                    if (cboCN.SelectedIndex < 0) return;
                    int maCN = Convert.ToInt32(dtBranches.Rows[cboCN.SelectedIndex]["MaChiNhanh"]);
                    string query = "SELECT MaPhong, TenPhong FROM PhongChieu WHERE MaChiNhanh = @MaCN AND TrangThai = 1";
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@MaCN", maCN);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                        cboPhong.Items.Add(reader["TenPhong"].ToString());
                                }
                            }
                        }
                        if (cboPhong.Items.Count > 0) cboPhong.SelectedIndex = 0;
                    }
                    catch { }
                };
                
                // Trigger load rooms for first branch - MUST be after event handler is registered
                if (cboCN.Items.Count > 0)
                {
                    cboCN.SelectedIndex = -1;
                    cboCN.SelectedIndex = 0;
                }

                y += 45;

                // Date
                Label lblNgay = new Label { Text = "Ngày chiếu:", Location = new Point(20, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                DateTimePicker dtpNgay = new DateTimePicker { Location = new Point(lblWidth + 20, y), Size = new Size(ctrlWidth, 30), Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };

                y += 45;

                // Time
                Label lblGio = new Label { Text = "Giờ chiếu:", Location = new Point(20, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                DateTimePicker dtpGio = new DateTimePicker { Location = new Point(lblWidth + 20, y), Size = new Size(ctrlWidth, 30), Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = new Font("Segoe UI", 10) };

                y += 45;

                // Price
                Label lblGia = new Label { Text = "Giá vé:", Location = new Point(20, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                TextBox txtGia = new TextBox { Location = new Point(lblWidth + 20, y), Size = new Size(ctrlWidth, 30), Font = new Font("Segoe UI", 10), Text = "75000" };

                y += 45;

                // Status
                Label lblStatus = new Label { Text = "Trạng thái:", Location = new Point(20, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                ComboBox cboStatus = new ComboBox { Location = new Point(lblWidth + 20, y), Size = new Size(ctrlWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
                cboStatus.Items.AddRange(new[] { "Sắp chiếu", "Đang chiếu", "Kết thúc" });
                cboStatus.SelectedIndex = 0;

                y += 60;

                // Buttons
                Button btnSave = new Button { Text = "💾 Lưu", Location = new Point(lblWidth + 20, y), Size = new Size(120, 40), BackColor = _cgvRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
                btnSave.FlatAppearance.BorderSize = 0;

                Button btnCancel = new Button { Text = "Hủy", Location = new Point(lblWidth + 160, y), Size = new Size(100, 40), Font = new Font("Segoe UI", 10) };

                // Load data if editing
                if (maSuatChieu > 0)
                {
                    try
                    {
                        string query = @"
                            SELECT sc.*, p.TenPhim, pc.TenPhong, cn.TenChiNhanh
                            FROM SuatChieu sc
                            INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                            INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                            INNER JOIN ChiNhanh cn ON pc.MaChiNhanh = cn.MaChiNhanh
                            WHERE sc.MaSuatChieu = @MaSuatChieu";

                        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@MaSuatChieu", maSuatChieu);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        cboPhim.SelectedItem = reader["TenPhim"].ToString();
                                        cboCN.SelectedItem = reader["TenChiNhanh"].ToString();
                                        // Room will be loaded by cboCN.SelectedIndexChanged
                                        dtpNgay.Value = Convert.ToDateTime(reader["NgayChieu"]);
                                        dtpGio.Value = DateTime.Today.Add((TimeSpan)reader["GioChieu"]);
                                        txtGia.Text = reader["GiaVe"].ToString();

                                        string tt = reader["TrangThai"].ToString();
                                        cboStatus.SelectedIndex = tt == "SapChieu" ? 0 : tt == "DangChieu" ? 1 : 2;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                btnSave.Click += (s, e) =>
                {
                    try
                    {
                        if (cboPhim.SelectedIndex < 0 || cboCN.SelectedIndex < 0 || cboPhong.SelectedIndex < 0)
                        {
                            MessageBox.Show("Vui lòng chọn đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (!decimal.TryParse(txtGia.Text, out decimal giaVe) || giaVe <= 0)
                        {
                            MessageBox.Show("Giá vé không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        int maPhim = Convert.ToInt32(dtMovies.Rows[cboPhim.SelectedIndex]["MaPhim"]);
                        int maCN = Convert.ToInt32(dtBranches.Rows[cboCN.SelectedIndex]["MaChiNhanh"]);

                        // Get room ID
                        int maPhong = 0;
                        string roomQuery = "SELECT MaPhong FROM PhongChieu WHERE TenPhong = @TenPhong AND MaChiNhanh = @MaCN";
                        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand(roomQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@TenPhong", cboPhong.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@MaCN", maCN);
                                maPhong = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        string trangThai = cboStatus.SelectedIndex == 0 ? "SapChieu" : cboStatus.SelectedIndex == 1 ? "DangChieu" : "KetThuc";

                        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                        {
                            conn.Open();
                            string query;
                            if (maSuatChieu == 0)
                            {
                                query = @"INSERT INTO SuatChieu (MaPhim, MaPhong, NgayChieu, GioChieu, GiaVe, TrangThai) 
                                         VALUES (@MaPhim, @MaPhong, @NgayChieu, @GioChieu, @GiaVe, @TrangThai)";
                            }
                            else
                            {
                                query = @"UPDATE SuatChieu SET MaPhim = @MaPhim, MaPhong = @MaPhong, NgayChieu = @NgayChieu, 
                                         GioChieu = @GioChieu, GiaVe = @GiaVe, TrangThai = @TrangThai WHERE MaSuatChieu = @MaSuatChieu";
                            }

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@MaPhim", maPhim);
                                cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                                cmd.Parameters.AddWithValue("@NgayChieu", dtpNgay.Value.Date);
                                cmd.Parameters.AddWithValue("@GioChieu", dtpGio.Value.TimeOfDay);
                                cmd.Parameters.AddWithValue("@GiaVe", giaVe);
                                cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                                if (maSuatChieu > 0)
                                    cmd.Parameters.AddWithValue("@MaSuatChieu", maSuatChieu);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show(maSuatChieu == 0 ? "Thêm suất chiếu thành công!" : "Cập nhật suất chiếu thành công!",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frm.DialogResult = DialogResult.OK;
                        frm.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnCancel.Click += (s, e) => frm.Close();

                frm.Controls.AddRange(new Control[] { lblPhim, cboPhim, lblCN, cboCN, lblPhong, cboPhong, lblNgay, dtpNgay, lblGio, dtpGio, lblGia, txtGia, lblStatus, cboStatus, btnSave, btnCancel });

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadShowtimes();
                }
            }
        }
    }
}
