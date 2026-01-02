using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_RoomManagement : UserControl
    {
        private DataGridView dgvRooms;
        private Label lblTotalRooms, lblTotalSeats, lblActiveRooms;
        private ComboBox cboFilterBranch;
        private DataTable dtBranches;
        private DataTable dtRooms;
        private int? _maChiNhanh; // Null = Admin (xem tất cả), có giá trị = Quản lý chi nhánh
        
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        // Constructor cho Admin - xem tất cả chi nhánh
        public UC_RoomManagement()
        {
            _maChiNhanh = null;
            InitializeComponent();
            SetupUI();
            LoadBranches();
            LoadRooms();
        }

        // Constructor cho Quản lý chi nhánh - chỉ xem chi nhánh mình
        public UC_RoomManagement(int maChiNhanh)
        {
            _maChiNhanh = maChiNhanh;
            InitializeComponent();
            SetupUI();
            LoadBranches();
            LoadRooms();
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);
            this.Dock = DockStyle.Fill;

            // ========== HEADER ==========
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 50 };
            Label lblTitle = new Label
            {
                Text = "🎬 QUẢN LÝ PHÒNG CHIẾU",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Location = new Point(0, 10)
            };
            headerPanel.Controls.Add(lblTitle);

            // ========== STATS PANEL ==========
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(0, 10, 0, 10) };
            TableLayoutPanel statsGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var card1 = CreateStatCard("TỔNG PHÒNG CHIẾU", "0", _cgvRed, out lblTotalRooms);
            var card2 = CreateStatCard("PHÒNG HOẠT ĐỘNG", "0", Color.FromArgb(39, 174, 96), out lblActiveRooms);
            var card3 = CreateStatCard("TỔNG SỐ GHẾ", "0", Color.FromArgb(52, 152, 219), out lblTotalSeats);
            
            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsGrid.Controls.Add(card3, 2, 0);
            statsPanel.Controls.Add(statsGrid);

            // ========== FILTER TOOLBAR ==========
            Panel filterPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(15, 10, 15, 10) };
            filterPanel.BorderRadius(12);

            Label lblFilter = new Label { Text = "Chi nhánh:", Font = new Font("Segoe UI", 10), Location = new Point(15, 18), AutoSize = true };
            cboFilterBranch = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(250, 35),
                Location = new Point(90, 14),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboFilterBranch.SelectedIndexChanged += (s, e) => LoadRooms();

            // Nếu là quản lý chi nhánh, ẨN dropdown filter (không cần filter vì chỉ xem chi nhánh mình)
            if (_maChiNhanh.HasValue)
            {
                lblFilter.Visible = false;
                cboFilterBranch.Visible = false;
                filterPanel.Height = 0; // Ẩn luôn panel
            }

            filterPanel.Controls.AddRange(new Control[] { lblFilter, cboFilterBranch });

            // ========== ACTION TOOLBAR ==========
            Panel toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(15, 10, 15, 10), Margin = new Padding(0, 10, 0, 0) };
            toolBar.BorderRadius(12);

            Button btnAdd = CreateButton("➕ THÊM PHÒNG", _cgvRed);
            btnAdd.Location = new Point(toolBar.Width - 160, 10);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += BtnAdd_Click;

            Button btnDesignSeats = CreateButton("🪑 THIẾT KẾ GHẾ", Color.FromArgb(155, 89, 182));
            btnDesignSeats.Location = new Point(toolBar.Width - 310, 10);
            btnDesignSeats.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDesignSeats.Click += BtnDesignSeats_Click;

            Button btnEdit = CreateButton("✏️ SỬA", Color.FromArgb(52, 152, 219));
            btnEdit.Location = new Point(toolBar.Width - 460, 10);
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.Click += BtnEdit_Click;

            Button btnDelete = CreateButton("🗑️ XÓA", Color.FromArgb(231, 76, 60));
            btnDelete.Location = new Point(toolBar.Width - 610, 10);
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Click += BtnDelete_Click;

            toolBar.Controls.AddRange(new Control[] { btnAdd, btnDesignSeats, btnEdit, btnDelete });

            // ========== DATA GRID ==========
            Panel gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            gridPanel.BorderRadius(12);

            dgvRooms = new DataGridView
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

            dgvRooms.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI Semibold", 10),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            dgvRooms.ColumnHeadersHeight = 50;

            dgvRooms.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 10),
                ForeColor = _cgvBlack,
                SelectionBackColor = Color.FromArgb(255, 235, 238),
                SelectionForeColor = _cgvRed,
                Padding = new Padding(10, 0, 0, 0)
            };

            dgvRooms.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            gridPanel.Controls.Add(dgvRooms);

            // ========== ASSEMBLE ==========
            this.Controls.Add(gridPanel);
            Panel spacer1 = new Panel { Dock = DockStyle.Top, Height = 15 };
            this.Controls.Add(spacer1);
            this.Controls.Add(toolBar);
            Panel spacer2 = new Panel { Dock = DockStyle.Top, Height = 15 };
            this.Controls.Add(spacer2);
            this.Controls.Add(filterPanel);
            this.Controls.Add(statsPanel);
            this.Controls.Add(headerPanel);
        }

        private Panel CreateStatCard(string title, string value, Color accentColor, out Label valueLabel)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 15, 0) };
            card.BorderRadius(15);
            
            Panel accent = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accentColor };
            card.Controls.Add(accent);
            
            Label lblTitle = new Label 
            { 
                Text = title, 
                Font = new Font("Segoe UI Semibold", 9), 
                ForeColor = Color.Gray, 
                Location = new Point(25, 15), 
                AutoSize = true 
            };
            
            valueLabel = new Label 
            { 
                Text = value, 
                Font = new Font("Montserrat", 24, FontStyle.Bold), 
                ForeColor = _cgvBlack, 
                Location = new Point(22, 40), 
                AutoSize = true 
            };
            
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

                cboFilterBranch.Items.Clear();
                
                // Nếu là Quản lý chi nhánh, CHỈ load chi nhánh của mình
                if (_maChiNhanh.HasValue)
                {
                    foreach (DataRow row in dtBranches.Rows)
                    {
                        if (Convert.ToInt32(row["MaChiNhanh"]) == _maChiNhanh.Value)
                        {
                            cboFilterBranch.Items.Add(row["TenChiNhanh"].ToString());
                            cboFilterBranch.SelectedIndex = 0;
                            break;
                        }
                    }
                }
                else
                {
                    // Admin: Hiển thị "Tất cả" và tất cả chi nhánh
                    cboFilterBranch.Items.Add("Tất cả chi nhánh");
                    foreach (DataRow row in dtBranches.Rows)
                    {
                        cboFilterBranch.Items.Add(row["TenChiNhanh"].ToString());
                    }
                    cboFilterBranch.SelectedIndex = 0;
                }
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
                // Try with LoaiPhong column first, fallback if not exists
                string query = @"
                    SELECT pc.MaPhong, pc.TenPhong, cn.TenChiNhanh, pc.TongSoGhe, 
                           ISNULL(pc.LoaiPhong, N'2D') AS LoaiPhong, pc.TrangThai, cn.MaChiNhanh
                    FROM PhongChieu pc
                    INNER JOIN ChiNhanh cn ON pc.MaChiNhanh = cn.MaChiNhanh
                    WHERE 1=1";

                // LOGIC MỚI: Nếu là Quản lý chi nhánh, CHỈ load phòng của chi nhánh mình
                if (_maChiNhanh.HasValue)
                {
                    query += $" AND pc.MaChiNhanh = {_maChiNhanh.Value}";
                }
                // Nếu là Admin và có chọn filter, dùng filter
                else if (cboFilterBranch != null && cboFilterBranch.SelectedIndex > 0)
                {
                    int maChiNhanh = Convert.ToInt32(dtBranches.Rows[cboFilterBranch.SelectedIndex - 1]["MaChiNhanh"]);
                    query += $" AND pc.MaChiNhanh = {maChiNhanh}";
                }

                query += " ORDER BY cn.TenChiNhanh, pc.TenPhong";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    dtRooms = new DataTable();
                    
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                        {
                            adapter.Fill(dtRooms);
                        }
                    }
                    catch (SqlException ex) when (ex.Message.Contains("LoaiPhong"))
                    {
                        // Fallback: Column doesn't exist, query without it
                        System.Diagnostics.Debug.WriteLine("LoaiPhong column not found, using fallback query");
                        
                        string fallbackQuery = @"
                            SELECT pc.MaPhong, pc.TenPhong, cn.TenChiNhanh, pc.TongSoGhe, 
                                   N'2D' AS LoaiPhong, pc.TrangThai, cn.MaChiNhanh
                            FROM PhongChieu pc
                            INNER JOIN ChiNhanh cn ON pc.MaChiNhanh = cn.MaChiNhanh
                            WHERE 1=1";
                        
                        if (_maChiNhanh.HasValue)
                        {
                            fallbackQuery += $" AND pc.MaChiNhanh = {_maChiNhanh.Value}";
                        }
                        else if (cboFilterBranch != null && cboFilterBranch.SelectedIndex > 0)
                        {
                            int maChiNhanh = Convert.ToInt32(dtBranches.Rows[cboFilterBranch.SelectedIndex - 1]["MaChiNhanh"]);
                            fallbackQuery += $" AND pc.MaChiNhanh = {maChiNhanh}";
                        }
                        fallbackQuery += " ORDER BY cn.TenChiNhanh, pc.TenPhong";
                        
                        dtRooms = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(fallbackQuery, conn))
                        {
                            adapter.Fill(dtRooms);
                        }
                        
                        // Try to add the column for future use
                        TryAddLoaiPhongColumn(conn);
                    }
                }

                BindDataToGrid();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải phòng chiếu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TryAddLoaiPhongColumn(SqlConnection conn)
        {
            try
            {
                string addColumnScript = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                   WHERE TABLE_NAME = 'PhongChieu' AND COLUMN_NAME = 'LoaiPhong')
                    BEGIN
                        ALTER TABLE PhongChieu ADD LoaiPhong NVARCHAR(20) NULL DEFAULT N'2D';
                        UPDATE PhongChieu SET LoaiPhong = N'2D' WHERE LoaiPhong IS NULL;
                    END";
                using (SqlCommand cmd = new SqlCommand(addColumnScript, conn))
                {
                    cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("LoaiPhong column added successfully");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not add LoaiPhong column: {ex.Message}");
            }
        }

        private void BindDataToGrid()
        {
            dgvRooms.Columns.Clear();
            dgvRooms.Rows.Clear();

            dgvRooms.Columns.Add("MaPhong", "Mã");
            dgvRooms.Columns.Add("TenPhong", "Tên Phòng");
            dgvRooms.Columns.Add("ChiNhanh", "Chi Nhánh");
            dgvRooms.Columns.Add("LoaiPhong", "Loại Phòng");
            dgvRooms.Columns.Add("TongSoGhe", "Tổng Ghế");
            dgvRooms.Columns.Add("TrangThai", "Trạng Thái");

            dgvRooms.Columns["MaPhong"].Width = 60;
            dgvRooms.Columns["TongSoGhe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataRow row in dtRooms.Rows)
            {
                string loaiPhong = row["LoaiPhong"]?.ToString() ?? "2D";
                string loaiPhongDisplay = loaiPhong switch
                {
                    "2D" => "🎬 2D",
                    "3D" => "🎥 3D",
                    "IMAX" => "🌟 IMAX",
                    "4DX" => "🎢 4DX",
                    _ => loaiPhong
                };

                bool isActive = Convert.ToBoolean(row["TrangThai"]);
                string trangThaiDisplay = isActive ? "✅ Hoạt động" : "❌ Ngừng";

                dgvRooms.Rows.Add(
                    row["MaPhong"],
                    row["TenPhong"],
                    row["TenChiNhanh"],
                    loaiPhongDisplay,
                    row["TongSoGhe"],
                    trangThaiDisplay
                );

                // Color inactive rows
                if (!isActive)
                {
                    dgvRooms.Rows[dgvRooms.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Gray;
                }
            }
        }

        private void UpdateStats()
        {
            int totalRooms = dtRooms?.Rows.Count ?? 0;
            int activeRooms = 0;
            int totalSeats = 0;

            if (dtRooms != null)
            {
                foreach (DataRow row in dtRooms.Rows)
                {
                    if (Convert.ToBoolean(row["TrangThai"]))
                        activeRooms++;
                    totalSeats += Convert.ToInt32(row["TongSoGhe"]);
                }
            }

            lblTotalRooms.Text = totalRooms.ToString();
            lblActiveRooms.Text = activeRooms.ToString();
            lblTotalSeats.Text = totalSeats.ToString("N0");
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ShowRoomDialog(0);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng chiếu cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maPhong = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["MaPhong"].Value);
            
            // KIỂM TRA PHÂN QUYỀN: Nếu là quản lý chi nhánh, chỉ được sửa phòng của chi nhánh mình
            if (_maChiNhanh.HasValue)
            {
                int maChiNhanhPhong = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["MaChiNhanh"].Value);
                if (maChiNhanhPhong != _maChiNhanh.Value)
                {
                    MessageBox.Show("Bạn không có quyền sửa phòng chiếu của chi nhánh khác!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            
            ShowRoomDialog(maPhong);
        }

        private void ShowRoomDialog(int maPhong)
        {
            bool isEdit = maPhong > 0;
            DataRow roomData = null;

            if (isEdit)
            {
                foreach (DataRow row in dtRooms.Rows)
                {
                    if (Convert.ToInt32(row["MaPhong"]) == maPhong)
                    {
                        roomData = row;
                        break;
                    }
                }
            }

            using (Form frm = new Form())
            {
                frm.Text = isEdit ? "✏️ Sửa Phòng Chiếu" : "➕ Thêm Phòng Chiếu";
                frm.Size = new Size(450, 380);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = false;
                frm.MinimizeBox = false;
                frm.BackColor = Color.White;

                int y = 25;
                int lblWidth = 100;
                int ctrlWidth = 280;

                // Tên phòng
                Label lblTen = new Label { Text = "Tên phòng:", Location = new Point(25, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                TextBox txtTen = new TextBox { Location = new Point(lblWidth + 30, y), Size = new Size(ctrlWidth, 30), Font = new Font("Segoe UI", 10) };
                if (isEdit) txtTen.Text = roomData["TenPhong"].ToString();

                y += 50;

                // Chi nhánh
                Label lblCN = new Label { Text = "Chi nhánh:", Location = new Point(25, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                ComboBox cboCN = new ComboBox { Location = new Point(lblWidth + 30, y), Size = new Size(ctrlWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
                
                // LOGIC MỚI: Nếu là Quản lý chi nhánh, CHỈ hiển thị chi nhánh của mình
                if (_maChiNhanh.HasValue)
                {
                    // Tìm chi nhánh của quản lý
                    foreach (DataRow row in dtBranches.Rows)
                    {
                        if (Convert.ToInt32(row["MaChiNhanh"]) == _maChiNhanh.Value)
                        {
                            cboCN.Items.Add(row["TenChiNhanh"].ToString());
                            break;
                        }
                    }
                    cboCN.Enabled = false; // Không cho chọn chi nhánh khác
                }
                else
                {
                    // Admin: hiển thị tất cả chi nhánh
                    foreach (DataRow row in dtBranches.Rows)
                        cboCN.Items.Add(row["TenChiNhanh"].ToString());
                }
                
                if (cboCN.Items.Count > 0) cboCN.SelectedIndex = 0;
                if (isEdit) cboCN.SelectedItem = roomData["TenChiNhanh"].ToString();

                y += 50;

                // Loại phòng
                Label lblLoai = new Label { Text = "Loại phòng:", Location = new Point(25, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                ComboBox cboLoai = new ComboBox { Location = new Point(lblWidth + 30, y), Size = new Size(ctrlWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
                cboLoai.Items.AddRange(new[] { "2D", "3D", "IMAX", "4DX" });
                cboLoai.SelectedIndex = 0;
                if (isEdit && roomData["LoaiPhong"] != DBNull.Value)
                {
                    string loai = roomData["LoaiPhong"].ToString();
                    int idx = cboLoai.FindString(loai);
                    if (idx >= 0) cboLoai.SelectedIndex = idx;
                }

                y += 50;

                // Tổng số ghế
                Label lblGhe = new Label { Text = "Tổng ghế:", Location = new Point(25, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                NumericUpDown numGhe = new NumericUpDown { Location = new Point(lblWidth + 30, y), Size = new Size(ctrlWidth, 30), Font = new Font("Segoe UI", 10), Minimum = 10, Maximum = 500, Value = 100 };
                if (isEdit) numGhe.Value = Convert.ToInt32(roomData["TongSoGhe"]);

                y += 50;

                // Trạng thái
                Label lblTT = new Label { Text = "Trạng thái:", Location = new Point(25, y + 3), AutoSize = true, Font = new Font("Segoe UI", 10) };
                ComboBox cboTT = new ComboBox { Location = new Point(lblWidth + 30, y), Size = new Size(ctrlWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
                cboTT.Items.AddRange(new[] { "Hoạt động", "Ngừng hoạt động" });
                cboTT.SelectedIndex = 0;
                if (isEdit && !Convert.ToBoolean(roomData["TrangThai"])) cboTT.SelectedIndex = 1;

                y += 60;

                // Buttons
                Button btnSave = new Button
                {
                    Text = "💾 Lưu",
                    Location = new Point(lblWidth + 30, y),
                    Size = new Size(120, 40),
                    BackColor = _cgvRed,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnSave.FlatAppearance.BorderSize = 0;

                Button btnCancel = new Button
                {
                    Text = "Hủy",
                    Location = new Point(lblWidth + 170, y),
                    Size = new Size(100, 40),
                    Font = new Font("Segoe UI", 10)
                };

                btnSave.Click += (s, ev) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(txtTen.Text))
                        {
                            MessageBox.Show("Vui lòng nhập tên phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // LẤY MaChiNhanh ĐÚNG: Nếu là quản lý thì dùng _maChiNhanh, nếu Admin thì lấy từ combo
                        int maCN;
                        if (_maChiNhanh.HasValue)
                        {
                            maCN = _maChiNhanh.Value;
                        }
                        else
                        {
                            // Admin: SelectedIndex - 1 vì có "Tất cả chi nhánh" ở index 0
                            int selectedBranchIndex = cboCN.SelectedIndex - 1;
                            if (selectedBranchIndex < 0)
                            {
                                MessageBox.Show("Vui lòng chọn chi nhánh!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            maCN = Convert.ToInt32(dtBranches.Rows[selectedBranchIndex]["MaChiNhanh"]);
                        }
                        
                        string loaiPhong = cboLoai.SelectedItem.ToString();
                        int tongGhe = (int)numGhe.Value;
                        bool trangThai = cboTT.SelectedIndex == 0;

                        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                        {
                            conn.Open();

                            // Đảm bảo cột LoaiPhong tồn tại
                            try
                            {
                                string addColQuery = @"
                                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                                   WHERE TABLE_NAME = 'PhongChieu' AND COLUMN_NAME = 'LoaiPhong')
                                    BEGIN
                                        ALTER TABLE PhongChieu ADD LoaiPhong NVARCHAR(20) NULL DEFAULT N'2D';
                                    END";
                                using (SqlCommand addCmd = new SqlCommand(addColQuery, conn))
                                {
                                    addCmd.ExecuteNonQuery();
                                }
                            }
                            catch { /* Ignore - column may already exist */ }

                            string query;
                            if (isEdit)
                            {
                                query = @"UPDATE PhongChieu SET TenPhong = @TenPhong, MaChiNhanh = @MaCN, 
                                         LoaiPhong = @LoaiPhong, TongSoGhe = @TongGhe, TrangThai = @TrangThai 
                                         WHERE MaPhong = @MaPhong";
                            }
                            else
                            {
                                query = @"INSERT INTO PhongChieu (TenPhong, MaChiNhanh, LoaiPhong, TongSoGhe, TrangThai) 
                                         VALUES (@TenPhong, @MaCN, @LoaiPhong, @TongGhe, @TrangThai)";
                            }

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@TenPhong", txtTen.Text.Trim());
                                cmd.Parameters.AddWithValue("@MaCN", maCN);
                                cmd.Parameters.AddWithValue("@LoaiPhong", loaiPhong);
                                cmd.Parameters.AddWithValue("@TongGhe", tongGhe);
                                cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                                if (isEdit)
                                    cmd.Parameters.AddWithValue("@MaPhong", maPhong);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show(isEdit ? "Cập nhật phòng chiếu thành công!" : "Thêm phòng chiếu thành công!",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frm.DialogResult = DialogResult.OK;
                        frm.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnCancel.Click += (s, ev) => frm.Close();

                frm.Controls.AddRange(new Control[] { lblTen, txtTen, lblCN, cboCN, lblLoai, cboLoai, lblGhe, numGhe, lblTT, cboTT, btnSave, btnCancel });

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadRooms();
                }
            }
        }

        private void BtnDesignSeats_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng chiếu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maPhong = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["MaPhong"].Value);
            string tenPhong = dgvRooms.SelectedRows[0].Cells["TenPhong"].Value.ToString();

            // KIỂM TRA PHÂN QUYỀN: Nếu là quản lý chi nhánh, chỉ được thiết kế ghế cho phòng của chi nhánh mình
            if (_maChiNhanh.HasValue)
            {
                int maChiNhanhPhong = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["MaChiNhanh"].Value);
                if (maChiNhanhPhong != _maChiNhanh.Value)
                {
                    MessageBox.Show("Bạn không có quyền thiết kế ghế cho phòng chiếu của chi nhánh khác!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            ShowSeatDesigner(maPhong, tenPhong);
        }

        private void ShowSeatDesigner(int maPhong, string tenPhong)
        {
            GheDAL gheDAL = new GheDAL();

            using (Form frm = new Form())
            {
                frm.Text = $"🪑 Thiết Kế Sơ Đồ Ghế - {tenPhong}";
                frm.Size = new Size(900, 650);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.BackColor = Color.FromArgb(30, 30, 30);

                // Header
                Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = _cgvRed };
                Label lblHeader = new Label
                {
                    Text = $"SƠ ĐỒ GHẾ - {tenPhong.ToUpper()}",
                    Font = new Font("Montserrat", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(20, 18)
                };
                header.Controls.Add(lblHeader);

                // Screen
                Panel screenPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(30, 30, 30) };
                Panel screen = new Panel
                {
                    Size = new Size(600, 30),
                    Location = new Point(130, 15),
                    BackColor = Color.FromArgb(200, 200, 200)
                };
                Label lblScreen = new Label
                {
                    Text = "M À N   H Ì N H",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                screen.Controls.Add(lblScreen);
                screenPanel.Controls.Add(screen);

                // Seat grid
                Panel seatPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(50, 20, 50, 20) };

                // Load ghế từ database
                DataTable dtGhe = gheDAL.LayGheTheoPhong(maPhong);

                int rows = 10;
                int cols = 12;
                int seatSize = 45;
                int spacing = 5;

                // Tạo dictionary để tra cứu ghế từ DB
                var gheDict = new System.Collections.Generic.Dictionary<string, DataRow>();
                foreach (DataRow row in dtGhe.Rows)
                {
                    string key = row["SoGhe"].ToString();
                    gheDict[key] = row;
                }

                for (int r = 0; r < rows; r++)
                {
                    char rowLetter = (char)('A' + r);

                    // Row label
                    Label lblRow = new Label
                    {
                        Text = rowLetter.ToString(),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.White,
                        Size = new Size(30, seatSize),
                        Location = new Point(10, 20 + r * (seatSize + spacing)),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    seatPanel.Controls.Add(lblRow);

                    for (int c = 0; c < cols; c++)
                    {
                        string soGhe = $"{rowLetter}{c + 1}";

                        // Xác định loại ghế từ DB hoặc default
                        Color seatColor = Color.FromArgb(39, 174, 96); // Default: Thuong
                        bool isActive = true;

                        if (gheDict.ContainsKey(soGhe))
                        {
                            DataRow gheRow = gheDict[soGhe];
                            string loaiGhe = gheRow["LoaiGhe"].ToString();
                            isActive = Convert.ToBoolean(gheRow["TrangThai"]);

                            if (!isActive)
                                seatColor = Color.Gray;
                            else if (loaiGhe == "VIP")
                                seatColor = Color.FromArgb(155, 89, 182);
                            else
                                seatColor = Color.FromArgb(39, 174, 96);
                        }

                        Button seat = new Button
                        {
                            Text = $"{c + 1}",
                            Size = new Size(seatSize, seatSize),
                            Location = new Point(50 + c * (seatSize + spacing), 20 + r * (seatSize + spacing)),
                            FlatStyle = FlatStyle.Flat,
                            BackColor = seatColor,
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 8, FontStyle.Bold),
                            Tag = soGhe
                        };
                        seat.FlatAppearance.BorderSize = 0;

                        seat.Click += (s, ev) =>
                        {
                            Button btn = (Button)s;
                            // Toggle seat type: Thuong -> VIP -> Disabled -> Thuong
                            if (btn.BackColor == Color.FromArgb(39, 174, 96))
                                btn.BackColor = Color.FromArgb(155, 89, 182); // VIP
                            else if (btn.BackColor == Color.FromArgb(155, 89, 182))
                                btn.BackColor = Color.Gray; // Disabled
                            else
                                btn.BackColor = Color.FromArgb(39, 174, 96); // Thuong
                        };

                        seatPanel.Controls.Add(seat);
                    }
                }

                // Legend
                Panel legend = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.FromArgb(40, 40, 40), Padding = new Padding(20) };

                int legX = 20;
                AddLegendItem(legend, ref legX, Color.FromArgb(39, 174, 96), "Ghế thường");
                AddLegendItem(legend, ref legX, Color.FromArgb(155, 89, 182), "Ghế VIP");
                AddLegendItem(legend, ref legX, Color.Gray, "Không sử dụng");

                Button btnSaveSeat = new Button
                {
                    Text = "💾 Lưu sơ đồ",
                    Size = new Size(120, 40),
                    Location = new Point(legend.Width - 280, 20),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = _cgvRed,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                btnSaveSeat.FlatAppearance.BorderSize = 0;
                btnSaveSeat.Click += (s, ev) =>
                {
                    // Disable button and show progress
                    btnSaveSeat.Enabled = false;
                    btnSaveSeat.Text = "Đang lưu...";
                    Application.DoEvents();

                    try
                    {
                        // Thu thập dữ liệu ghế từ UI
                        DataTable gheData = new DataTable();
                        gheData.Columns.Add("SoGhe", typeof(string));
                        gheData.Columns.Add("SoHang", typeof(string));
                        gheData.Columns.Add("LoaiGhe", typeof(string));
                        gheData.Columns.Add("TrangThai", typeof(bool));

                        foreach (Control ctrl in seatPanel.Controls)
                        {
                            if (ctrl is Button btn && btn.Tag != null)
                            {
                                string soGhe = btn.Tag.ToString();
                                string soHang = soGhe.Substring(0, 1);
                                string loaiGhe;
                                bool trangThai = true;

                                if (btn.BackColor == Color.FromArgb(155, 89, 182))
                                    loaiGhe = "VIP";
                                else if (btn.BackColor == Color.Gray)
                                {
                                    loaiGhe = "Thuong";
                                    trangThai = false;
                                }
                                else
                                    loaiGhe = "Thuong";

                                gheData.Rows.Add(soGhe, soHang, loaiGhe, trangThai);
                            }
                        }

                        if (gheDAL.LuuSoDoGhe(maPhong, gheData))
                        {
                            MessageBox.Show("Sơ đồ ghế đã được lưu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi lưu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnSaveSeat.Enabled = true;
                        btnSaveSeat.Text = "💾 Lưu sơ đồ";
                    }
                };

                Button btnClose = new Button
                {
                    Text = "Đóng",
                    Size = new Size(100, 40),
                    Location = new Point(legend.Width - 140, 20),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 60),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnClose.FlatAppearance.BorderSize = 1;
                btnClose.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                btnClose.Click += (s, ev) => frm.Close();

                legend.Controls.AddRange(new Control[] { btnSaveSeat, btnClose });

                frm.Controls.Add(seatPanel);
                frm.Controls.Add(legend);
                frm.Controls.Add(screenPanel);
                frm.Controls.Add(header);

                frm.ShowDialog();
            }
        }

        private void AddLegendItem(Panel parent, ref int x, Color color, string text)
        {
            Panel colorBox = new Panel { Size = new Size(25, 25), Location = new Point(x, 27), BackColor = color };
            Label lbl = new Label { Text = text, Font = new Font("Segoe UI", 9), ForeColor = Color.White, Location = new Point(x + 30, 30), AutoSize = true };
            parent.Controls.AddRange(new Control[] { colorBox, lbl });
            x += 150;
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng chiếu cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maPhong = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["MaPhong"].Value);
            string tenPhong = dgvRooms.SelectedRows[0].Cells["TenPhong"].Value.ToString();

            // KIỂM TRA PHÂN QUYỀN: Nếu là quản lý chi nhánh, chỉ được xóa phòng của chi nhánh mình
            if (_maChiNhanh.HasValue)
            {
                int maChiNhanhPhong = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["MaChiNhanh"].Value);
                if (maChiNhanhPhong != _maChiNhanh.Value)
                {
                    MessageBox.Show("Bạn không có quyền xóa phòng chiếu của chi nhánh khác!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phòng '{tenPhong}'?\n\nLưu ý: Không thể xóa phòng đã có suất chiếu.",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // Check if room has showtimes
                    string checkQuery = "SELECT COUNT(*) FROM SuatChieu WHERE MaPhong = @MaPhong";
                    using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                            int count = Convert.ToInt32(cmd.ExecuteScalar());
                            if (count > 0)
                            {
                                MessageBox.Show($"Không thể xóa! Phòng này có {count} suất chiếu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        string deleteQuery = "DELETE FROM PhongChieu WHERE MaPhong = @MaPhong";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Xóa phòng chiếu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRooms();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_RoomManagement";
            this.Size = new Size(1200, 800);
            this.ResumeLayout(false);
        }
    }
}
