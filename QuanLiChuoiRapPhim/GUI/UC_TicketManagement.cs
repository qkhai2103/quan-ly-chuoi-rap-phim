using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_TicketManagement : UserControl
    {
        private DataGridView dgvTickets;
        private DateTimePicker dtpFrom, dtpTo;
        private Label lblTotalTickets, lblTotalRevenue, lblCancelledTickets;
        private ComboBox cboStatusFilter;
        private TextBox txtSearch;
        
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_TicketManagement()
        {
            InitializeComponent();
            SetupUI();
            LoadTickets();
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);

            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ VÉ & GIAO DỊCH",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20)
            };

            // Stats
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 10, 0, 20) };
            TableLayoutPanel statsGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var card1 = CreateStatCard("TỔNG VÉ ĐÃ BÁN", "0", Color.FromArgb(226, 26, 60), out lblTotalTickets);
            var card2 = CreateStatCard("DOANH THU", "0 đ", Color.FromArgb(39, 174, 96), out lblTotalRevenue);
            var card3 = CreateStatCard("VÉ ĐÃ HỦY", "0", Color.FromArgb(231, 76, 60), out lblCancelledTickets);
            
            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsGrid.Controls.Add(card3, 2, 0);
            statsPanel.Controls.Add(statsGrid);

            // Filter Toolbar - Row 1
            Panel toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(20, 10, 20, 10) };
            toolBar.BorderRadius(12);

            Label lblFrom = new Label { Text = "Từ ngày:", Font = new Font("Segoe UI", 10), Location = new Point(20, 18), AutoSize = true };
            dtpFrom = new DateTimePicker { Font = new Font("Segoe UI", 10), Size = new Size(130, 30), Location = new Point(90, 15), Value = DateTime.Now.AddDays(-30) };

            Label lblTo = new Label { Text = "Đến:", Font = new Font("Segoe UI", 10), Location = new Point(235, 18), AutoSize = true };
            dtpTo = new DateTimePicker { Font = new Font("Segoe UI", 10), Size = new Size(130, 30), Location = new Point(280, 15), Value = DateTime.Now };

            // Status filter combo
            Label lblStatus = new Label { Text = "Trạng thái:", Font = new Font("Segoe UI", 10), Location = new Point(425, 18), AutoSize = true };
            cboStatusFilter = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(120, 30),
                Location = new Point(510, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatusFilter.Items.AddRange(new object[] { "Tất cả", "Đã bán", "Đã hủy" });
            cboStatusFilter.SelectedIndex = 0;
            cboStatusFilter.SelectedIndexChanged += (s, e) => LoadTickets();

            Button btnSearch = CreateButton("🔍 LỌC", Color.FromArgb(52, 152, 219));
            btnSearch.Size = new Size(80, 40);
            btnSearch.Location = new Point(650, 10);
            btnSearch.Click += (s, e) => LoadTickets();

            Button btnViewDetail = CreateButton("👁️ CHI TIẾT", Color.FromArgb(155, 89, 182));
            btnViewDetail.Size = new Size(110, 40);
            btnViewDetail.Location = new Point(toolBar.Width - 280, 10);
            btnViewDetail.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnViewDetail.Click += BtnViewDetail_Click;

            // Replace Cancel button with Transaction History button
            Button btnHistory = CreateButton("📋 LỊCH SỬ GD", Color.FromArgb(41, 128, 185));
            btnHistory.Size = new Size(130, 40);
            btnHistory.Location = new Point(toolBar.Width - 150, 10);
            btnHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHistory.Click += BtnTransactionHistory_Click;

            toolBar.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, lblStatus, cboStatusFilter, btnSearch, btnViewDetail, btnHistory });

            // Grid
            Panel gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            gridPanel.BorderRadius(12);

            dgvTickets = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 50 }
            };

            gridPanel.Controls.Add(dgvTickets);

            this.Controls.Add(gridPanel);
            Panel spacer = new Panel { Dock = DockStyle.Top, Height = 20 };
            this.Controls.Add(spacer);
            this.Controls.Add(toolBar);
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
            Button btn = new Button { Text = text, BackColor = backColor, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Size = new Size(140, 40), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.BorderRadius(8);
            return btn;
        }

        private void LoadTickets()
        {
            try
            {
                VeBLL veBLL = new VeBLL();
                DataTable dt = veBLL.LayTatCaVe();

                dgvTickets.Rows.Clear();
                dgvTickets.Columns.Clear();

                dgvTickets.Columns.Add("MaVe", "Mã Vé");
                dgvTickets.Columns.Add("MaVeCode", "Mã Code");
                dgvTickets.Columns.Add("TenPhim", "Phim");
                dgvTickets.Columns.Add("SoGhe", "Ghế");
                dgvTickets.Columns.Add("GiaVe", "Giá");
                dgvTickets.Columns.Add("TrangThai", "Trạng Thái");
                dgvTickets.Columns.Add("NgayDat", "Ngày Đặt");

                decimal totalRevenue = 0;
                int totalTickets = 0;
                int cancelledTickets = 0;

                // Get filter values
                DateTime fromDate = dtpFrom.Value.Date;
                DateTime toDate = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);
                string statusFilter = cboStatusFilter?.SelectedItem?.ToString() ?? "Tất cả";

                foreach (DataRow row in dt.Rows)
                {
                    string trangThai = row["TrangThai"].ToString();
                    DateTime ngayDat = Convert.ToDateTime(row["NgayDat"]);

                    // Apply date filter
                    if (ngayDat < fromDate || ngayDat > toDate)
                        continue;

                    // Apply status filter
                    string displayStatus = trangThai == "DaBan" ? "Đã bán" : "Đã hủy";
                    if (statusFilter != "Tất cả" && displayStatus != statusFilter)
                        continue;

                    string maVeCode = row["MaVeCode"] != DBNull.Value ? row["MaVeCode"].ToString() : "";

                    dgvTickets.Rows.Add(
                        row["MaVe"],
                        maVeCode,
                        row["TenPhim"],
                        row["SoGhe"],
                        Convert.ToDecimal(row["GiaVe"]).ToString("N0") + " đ",
                        displayStatus,
                        ngayDat.ToString("dd/MM/yyyy HH:mm")
                    );

                    if (trangThai == "DaBan")
                    {
                        totalRevenue += Convert.ToDecimal(row["GiaVe"]);
                        totalTickets++;
                    }
                    else
                    {
                        cancelledTickets++;
                    }
                }

                // Color rows based on status
                foreach (DataGridViewRow row in dgvTickets.Rows)
                {
                    if (row.Cells["TrangThai"].Value?.ToString() == "Đã hủy")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 60, 60);
                    }
                }

                lblTotalTickets.Text = totalTickets.ToString();
                lblTotalRevenue.Text = totalRevenue.ToString("N0") + " đ";
                lblCancelledTickets.Text = cancelledTickets.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnViewDetail_Click(object sender, EventArgs e)
        {
            if (dgvTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn vé cần xem!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dgvTickets.SelectedRows[0];
            int maVe = Convert.ToInt32(selectedRow.Cells["MaVe"].Value);
            string maVeCode = selectedRow.Cells["MaVeCode"].Value?.ToString() ?? "";
            string tenPhim = selectedRow.Cells["TenPhim"].Value.ToString();
            string soGhe = selectedRow.Cells["SoGhe"].Value.ToString();
            string giaVe = selectedRow.Cells["GiaVe"].Value.ToString();
            string trangThai = selectedRow.Cells["TrangThai"].Value.ToString();
            string ngayDat = selectedRow.Cells["NgayDat"].Value.ToString();

            // Create detail form
            ShowTicketDetailForm(maVe, maVeCode, tenPhim, soGhe, giaVe, trangThai, ngayDat);
        }

        private void ShowTicketDetailForm(int maVe, string maVeCode, string tenPhim, string soGhe, string giaVe, string trangThai, string ngayDat)
        {
            using (Form frm = new Form())
            {
                frm.Text = $"Chi tiết vé #{maVe}";
                frm.Size = new Size(500, 400);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.BackColor = Color.White;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = false;

                Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = _cgvRed };
                Label lblHeader = new Label
                {
                    Text = $"🎫 VÉ #{maVe}",
                    Font = new Font("Montserrat", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(20, 18)
                };
                header.Controls.Add(lblHeader);

                Panel content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
                int y = 20;

                AddDetailRow(content, "Mã Code:", maVeCode, ref y);
                AddDetailRow(content, "Phim:", tenPhim, ref y);
                AddDetailRow(content, "Ghế:", soGhe, ref y);
                AddDetailRow(content, "Giá vé:", giaVe, ref y);
                AddDetailRow(content, "Trạng thái:", trangThai, ref y);
                AddDetailRow(content, "Ngày đặt:", ngayDat, ref y);

                Button btnClose = new Button
                {
                    Text = "Đóng",
                    Size = new Size(100, 40),
                    Location = new Point(180, y + 20),
                    BackColor = _cgvBlack,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, e) => frm.Close();
                content.Controls.Add(btnClose);

                frm.Controls.Add(content);
                frm.Controls.Add(header);
                frm.ShowDialog();
            }
        }

        private void AddDetailRow(Panel parent, string label, string value, ref int y)
        {
            Label lblLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI Semibold", 11),
                ForeColor = Color.Gray,
                Location = new Point(20, y),
                AutoSize = true
            };
            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(150, y),
                AutoSize = true
            };
            parent.Controls.AddRange(new Control[] { lblLabel, lblValue });
            y += 35;
        }

        private void BtnTransactionHistory_Click(object sender, EventArgs e)
        {
            // Show transaction history in a modal form
            using (Form frm = new Form())
            {
                frm.Text = "📋 Lịch sử giao dịch";
                frm.Size = new Size(900, 600);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.BackColor = Color.White;

                Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(41, 128, 185) };
                Label lblHeader = new Label
                {
                    Text = "LỊCH SỬ GIAO DỊCH VÉ",
                    Font = new Font("Montserrat", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(20, 18)
                };
                header.Controls.Add(lblHeader);

                DataGridView dgvHistory = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    RowHeadersVisible = false,
                    AllowUserToAddRows = false,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    RowTemplate = { Height = 40 }
                };

                dgvHistory.Columns.Add("NgayGiaoDich", "Ngày");
                dgvHistory.Columns.Add("LoaiGiaoDich", "Loại GD");
                dgvHistory.Columns.Add("MaVe", "Mã Vé");
                dgvHistory.Columns.Add("TenPhim", "Phim");
                dgvHistory.Columns.Add("SoTien", "Số Tiền");
                dgvHistory.Columns.Add("GhiChu", "Ghi Chú");

                // Load transaction history from VeBLL
                try
                {
                    VeBLL veBLL = new VeBLL();
                    DataTable dt = veBLL.LayTatCaVe();

                    foreach (DataRow row in dt.Rows)
                    {
                        string trangThai = row["TrangThai"].ToString();
                        string loaiGD = trangThai == "DaBan" ? "Bán vé" : "Hủy vé";
                        Color rowColor = trangThai == "DaBan" ? Color.FromArgb(39, 174, 96) : Color.FromArgb(231, 76, 60);

                        int rowIndex = dgvHistory.Rows.Add(
                            Convert.ToDateTime(row["NgayDat"]).ToString("dd/MM/yyyy HH:mm"),
                            loaiGD,
                            row["MaVe"],
                            row["TenPhim"],
                            Convert.ToDecimal(row["GiaVe"]).ToString("N0") + " đ",
                            trangThai == "DaBan" ? "Giao dịch thành công" : "Đã hoàn tiền"
                        );

                        dgvHistory.Rows[rowIndex].Cells["LoaiGiaoDich"].Style.ForeColor = rowColor;
                        dgvHistory.Rows[rowIndex].Cells["LoaiGiaoDich"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(245, 245, 245) };
                Button btnClose = new Button
                {
                    Text = "Đóng",
                    Size = new Size(100, 40),
                    Location = new Point(390, 10),
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, ev) => frm.Close();
                footer.Controls.Add(btnClose);

                frm.Controls.Add(dgvHistory);
                frm.Controls.Add(footer);
                frm.Controls.Add(header);
                frm.ShowDialog();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_TicketManagement";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
