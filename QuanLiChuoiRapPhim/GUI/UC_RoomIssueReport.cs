using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// User Control for Branch Managers to report room/equipment issues
    /// </summary>
    public class UC_RoomIssueReport : UserControl
    {
        // CGV Branding Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

        private string _branchName;
        private int _userId;
        private DataGridView _dgvIssues;
        private DataTable _dtIssues;

        public UC_RoomIssueReport(string branchName, int userId)
        {
            _branchName = branchName;
            _userId = userId;
            InitializeComponent();
            LoadIssues();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(20);

            // Header Panel
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
                Text = "🔧 BÁO CÁO SỰ CỐ PHÒNG CHIẾU",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
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
                Location = new Point(450, 30)
            };
            headerPanel.Controls.Add(lblBranch);

            Button btnNewIssue = new Button
            {
                Text = "➕ BÁO SỰ CỐ MỚI",
                Size = new Size(160, 40),
                Location = new Point(headerPanel.Width - 200, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNewIssue.FlatAppearance.BorderSize = 0;
            btnNewIssue.Click += BtnNewIssue_Click;
            headerPanel.Controls.Add(btnNewIssue);

            // Stats cards
            Panel statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10)
            };

            FlowLayoutPanel cardsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };

            cardsFlow.Controls.Add(CreateStatCard("📋 Tổng báo cáo", "0", Color.FromArgb(52, 152, 219), "total"));
            cardsFlow.Controls.Add(CreateStatCard("🔴 Chờ xử lý", "0", Color.FromArgb(231, 76, 60), "pending"));
            cardsFlow.Controls.Add(CreateStatCard("🟡 Đang sửa", "0", Color.FromArgb(241, 196, 15), "inprogress"));
            cardsFlow.Controls.Add(CreateStatCard("🟢 Đã xong", "0", Color.FromArgb(39, 174, 96), "resolved"));

            statsPanel.Controls.Add(cardsFlow);

            // Content Panel - Issues list
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            AddRoundedCorners(contentPanel, 10);

            // Toolbar
            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent
            };

            ComboBox cboFilter = new ComboBox
            {
                Width = 150,
                Location = new Point(0, 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboFilter.Items.AddRange(new[] { "Tất cả", "Chờ xử lý", "Đang sửa", "Đã xong" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += (s, e) => FilterIssues(cboFilter.SelectedItem.ToString());
            toolbar.Controls.Add(cboFilter);

            ComboBox cboRoom = new ComboBox
            {
                Width = 150,
                Location = new Point(160, 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboRoom.Items.Add("Tất cả phòng");
            // Add rooms from database
            LoadRooms(cboRoom);
            cboRoom.SelectedIndex = 0;
            toolbar.Controls.Add(cboRoom);

            Button btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Size = new Size(100, 30),
                Location = new Point(320, 10),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadIssues();
            toolbar.Controls.Add(btnRefresh);

            contentPanel.Controls.Add(toolbar);

            // DataGridView for issues
            _dgvIssues = new DataGridView
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
                RowTemplate = { Height = 45 }
            };

            // Style headers
            _dgvIssues.ColumnHeadersDefaultCellStyle.BackColor = _cgvBlack;
            _dgvIssues.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvIssues.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _dgvIssues.ColumnHeadersHeight = 45;
            _dgvIssues.EnableHeadersVisualStyles = false;

            // Alternating row colors
            _dgvIssues.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            _dgvIssues.CellDoubleClick += DgvIssues_CellDoubleClick;
            _dgvIssues.CellFormatting += DgvIssues_CellFormatting;

            contentPanel.Controls.Add(_dgvIssues);

            // Add controls in order
            this.Controls.Add(contentPanel);
            this.Controls.Add(statsPanel);
            this.Controls.Add(headerPanel);
        }

        private Panel CreateStatCard(string title, string value, Color accentColor, string tag)
        {
            Panel card = new Panel
            {
                Size = new Size(180, 70),
                Margin = new Padding(0, 0, 15, 0),
                BackColor = Color.White,
                Tag = tag
            };
            AddRoundedCorners(card, 8);
            AddShadow(card);

            // Accent bar
            Panel accent = new Panel
            {
                Size = new Size(5, 70),
                Location = new Point(0, 0),
                BackColor = accentColor
            };
            card.Controls.Add(accent);

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(15, 30),
                AutoSize = true,
                Tag = "value"
            };
            card.Controls.Add(lblValue);

            return card;
        }

        private void LoadRooms(ComboBox cbo)
        {
            try
            {
                PhongChieuDAL phongDAL = new PhongChieuDAL();
                DataTable dt = phongDAL.LayTatCaPhongChieu();
                foreach (DataRow row in dt.Rows)
                {
                    cbo.Items.Add(row["TenPhong"]?.ToString() ?? row["MaPhong"]?.ToString());
                }
            }
            catch
            {
                // Silently fail - rooms will be loaded without filter
            }
        }

        private void LoadIssues()
        {
            // Create sample data structure (in real app, load from database)
            _dtIssues = new DataTable();
            _dtIssues.Columns.Add("MaBaoCao", typeof(int));
            _dtIssues.Columns.Add("TenPhong", typeof(string));
            _dtIssues.Columns.Add("LoaiSuCo", typeof(string));
            _dtIssues.Columns.Add("MoTa", typeof(string));
            _dtIssues.Columns.Add("TrangThai", typeof(string));
            _dtIssues.Columns.Add("DoUuTien", typeof(string));
            _dtIssues.Columns.Add("NgayBao", typeof(DateTime));
            _dtIssues.Columns.Add("NguoiBao", typeof(string));

            // Sample data - replace with actual database query
            _dtIssues.Rows.Add(1, "Phòng 1", "Máy chiếu", "Máy chiếu bị mờ, cần thay bóng đèn", "Chờ xử lý", "Cao", DateTime.Now.AddDays(-1), "Nguyễn Văn A");
            _dtIssues.Rows.Add(2, "Phòng 3", "Điều hòa", "Điều hòa không lạnh", "Đang sửa", "Trung bình", DateTime.Now.AddDays(-3), "Trần Thị B");
            _dtIssues.Rows.Add(3, "Phòng 2", "Ghế", "Ghế hàng D5 bị hỏng", "Đã xong", "Thấp", DateTime.Now.AddDays(-7), "Lê Văn C");
            _dtIssues.Rows.Add(4, "Phòng 4", "Âm thanh", "Loa surround bên phải không kêu", "Chờ xử lý", "Cao", DateTime.Now, "Phạm Thị D");

            _dgvIssues.DataSource = _dtIssues;

            // Configure columns
            if (_dgvIssues.Columns.Count > 0)
            {
                _dgvIssues.Columns["MaBaoCao"].HeaderText = "Mã";
                _dgvIssues.Columns["MaBaoCao"].Width = 50;
                _dgvIssues.Columns["TenPhong"].HeaderText = "Phòng";
                _dgvIssues.Columns["TenPhong"].Width = 80;
                _dgvIssues.Columns["LoaiSuCo"].HeaderText = "Loại sự cố";
                _dgvIssues.Columns["LoaiSuCo"].Width = 100;
                _dgvIssues.Columns["MoTa"].HeaderText = "Mô tả";
                _dgvIssues.Columns["MoTa"].Width = 200;
                _dgvIssues.Columns["TrangThai"].HeaderText = "Trạng thái";
                _dgvIssues.Columns["TrangThai"].Width = 100;
                _dgvIssues.Columns["DoUuTien"].HeaderText = "Ưu tiên";
                _dgvIssues.Columns["DoUuTien"].Width = 80;
                _dgvIssues.Columns["NgayBao"].HeaderText = "Ngày báo";
                _dgvIssues.Columns["NgayBao"].Width = 100;
                _dgvIssues.Columns["NgayBao"].DefaultCellStyle.Format = "dd/MM/yyyy";
                _dgvIssues.Columns["NguoiBao"].HeaderText = "Người báo";
                _dgvIssues.Columns["NguoiBao"].Width = 120;
            }

            UpdateStats();
        }

        private void UpdateStats()
        {
            if (_dtIssues == null) return;

            int total = _dtIssues.Rows.Count;
            int pending = 0, inProgress = 0, resolved = 0;

            foreach (DataRow row in _dtIssues.Rows)
            {
                string status = row["TrangThai"]?.ToString() ?? "";
                if (status == "Chờ xử lý") pending++;
                else if (status == "Đang sửa") inProgress++;
                else if (status == "Đã xong") resolved++;
            }

            // Update stat cards
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel statsPanel && statsPanel.Height == 100)
                {
                    foreach (Control inner in statsPanel.Controls)
                    {
                        if (inner is FlowLayoutPanel flow)
                        {
                            foreach (Control card in flow.Controls)
                            {
                                if (card is Panel p && p.Tag != null)
                                {
                                    string tag = p.Tag.ToString();
                                    foreach (Control c in p.Controls)
                                    {
                                        if (c is Label lbl && lbl.Tag?.ToString() == "value")
                                        {
                                            lbl.Text = tag switch
                                            {
                                                "total" => total.ToString(),
                                                "pending" => pending.ToString(),
                                                "inprogress" => inProgress.ToString(),
                                                "resolved" => resolved.ToString(),
                                                _ => "0"
                                            };
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FilterIssues(string filter)
        {
            if (_dtIssues == null) return;

            DataView dv = _dtIssues.DefaultView;
            if (filter == "Tất cả")
                dv.RowFilter = "";
            else
                dv.RowFilter = $"TrangThai = '{filter}'";
        }

        private void DgvIssues_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvIssues.Columns[e.ColumnIndex].Name == "TrangThai" && e.Value != null)
            {
                string status = e.Value.ToString();
                switch (status)
                {
                    case "Chờ xử lý":
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                        break;
                    case "Đang sửa":
                        e.CellStyle.ForeColor = Color.Orange;
                        e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                        break;
                    case "Đã xong":
                        e.CellStyle.ForeColor = Color.Green;
                        break;
                }
            }

            if (_dgvIssues.Columns[e.ColumnIndex].Name == "DoUuTien" && e.Value != null)
            {
                string priority = e.Value.ToString();
                switch (priority)
                {
                    case "Cao":
                        e.CellStyle.BackColor = Color.FromArgb(255, 230, 230);
                        break;
                    case "Trung bình":
                        e.CellStyle.BackColor = Color.FromArgb(255, 250, 230);
                        break;
                }
            }
        }

        private void DgvIssues_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = _dgvIssues.Rows[e.RowIndex];
            ShowIssueDetail(row);
        }

        private void ShowIssueDetail(DataGridViewRow row)
        {
            string info = $"📋 CHI TIẾT SỰ CỐ\n\n" +
                         $"Mã báo cáo: {row.Cells["MaBaoCao"].Value}\n" +
                         $"Phòng: {row.Cells["TenPhong"].Value}\n" +
                         $"Loại: {row.Cells["LoaiSuCo"].Value}\n" +
                         $"Mô tả: {row.Cells["MoTa"].Value}\n" +
                         $"Trạng thái: {row.Cells["TrangThai"].Value}\n" +
                         $"Độ ưu tiên: {row.Cells["DoUuTien"].Value}\n" +
                         $"Ngày báo: {row.Cells["NgayBao"].Value:dd/MM/yyyy}\n" +
                         $"Người báo: {row.Cells["NguoiBao"].Value}";

            MessageBox.Show(info, "Chi tiết sự cố", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnNewIssue_Click(object sender, EventArgs e)
        {
            ShowNewIssueForm();
        }

        private void ShowNewIssueForm()
        {
            Form frm = new Form
            {
                Text = "Báo cáo sự cố mới",
                Size = new Size(500, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = _cgvLightGray
            };

            int y = 20;

            // Room selection
            Label lblRoom = new Label { Text = "Phòng chiếu:", Location = new Point(20, y), AutoSize = true };
            frm.Controls.Add(lblRoom);
            ComboBox cboRoom = new ComboBox
            {
                Location = new Point(130, y - 3),
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            LoadRooms(cboRoom);
            if (cboRoom.Items.Count > 0) cboRoom.SelectedIndex = 0;
            frm.Controls.Add(cboRoom);
            y += 40;

            // Issue type
            Label lblType = new Label { Text = "Loại sự cố:", Location = new Point(20, y), AutoSize = true };
            frm.Controls.Add(lblType);
            ComboBox cboType = new ComboBox
            {
                Location = new Point(130, y - 3),
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboType.Items.AddRange(new[] { "Máy chiếu", "Âm thanh", "Điều hòa", "Ghế", "Ánh sáng", "Khác" });
            cboType.SelectedIndex = 0;
            frm.Controls.Add(cboType);
            y += 40;

            // Priority
            Label lblPriority = new Label { Text = "Độ ưu tiên:", Location = new Point(20, y), AutoSize = true };
            frm.Controls.Add(lblPriority);
            ComboBox cboPriority = new ComboBox
            {
                Location = new Point(130, y - 3),
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboPriority.Items.AddRange(new[] { "Cao", "Trung bình", "Thấp" });
            cboPriority.SelectedIndex = 1;
            frm.Controls.Add(cboPriority);
            y += 40;

            // Description
            Label lblDesc = new Label { Text = "Mô tả chi tiết:", Location = new Point(20, y), AutoSize = true };
            frm.Controls.Add(lblDesc);
            y += 25;
            TextBox txtDesc = new TextBox
            {
                Location = new Point(20, y),
                Size = new Size(440, 150),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            frm.Controls.Add(txtDesc);
            y += 170;

            // Buttons
            Button btnSubmit = new Button
            {
                Text = "📤 GỬI BÁO CÁO",
                Size = new Size(150, 40),
                Location = new Point(150, y),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtDesc.Text))
                {
                    MessageBox.Show("Vui lòng nhập mô tả sự cố!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Add to DataTable (in real app, save to database)
                int newId = _dtIssues.Rows.Count + 1;
                _dtIssues.Rows.Add(
                    newId,
                    cboRoom.SelectedItem?.ToString() ?? "N/A",
                    cboType.SelectedItem?.ToString() ?? "Khác",
                    txtDesc.Text,
                    "Chờ xử lý",
                    cboPriority.SelectedItem?.ToString() ?? "Trung bình",
                    DateTime.Now,
                    "Người dùng hiện tại"
                );

                UpdateStats();
                MessageBox.Show("Đã gửi báo cáo sự cố thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                frm.Close();
            };
            frm.Controls.Add(btnSubmit);

            Button btnCancel = new Button
            {
                Text = "Hủy",
                Size = new Size(100, 40),
                Location = new Point(310, y),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, ev) => frm.Close();
            frm.Controls.Add(btnCancel);

            frm.ShowDialog();
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

        private void AddShadow(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(20, 0, 0, 0), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, panel.Height - 1, panel.Width - 1, 1);
                }
            };
        }
    }
}
