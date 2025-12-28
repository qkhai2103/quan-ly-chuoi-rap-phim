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
        private TextBox txtSearch;
        private DateTimePicker dtpFrom, dtpTo;
        private Label lblTotalTickets, lblTotalRevenue, lblCancelledTickets;
        
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

            // Filter Toolbar
            Panel toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(20, 10, 20, 10) };
            toolBar.BorderRadius(12);

            Label lblFrom = new Label { Text = "Từ ngày:", Font = new Font("Segoe UI", 10), Location = new Point(20, 18), AutoSize = true };
            dtpFrom = new DateTimePicker { Font = new Font("Segoe UI", 10), Size = new Size(150, 30), Location = new Point(90, 15), Value = DateTime.Now.AddDays(-30) };

            Label lblTo = new Label { Text = "Đến:", Font = new Font("Segoe UI", 10), Location = new Point(260, 18), AutoSize = true };
            dtpTo = new DateTimePicker { Font = new Font("Segoe UI", 10), Size = new Size(150, 30), Location = new Point(310, 15), Value = DateTime.Now };

            Button btnSearch = CreateButton("🔍 TÌM KIẾM", Color.FromArgb(52, 152, 219));
            btnSearch.Location = new Point(480, 10);
            btnSearch.Click += (s, e) => LoadTickets();

            Button btnViewDetail = CreateButton("👁️ XEM CHI TIẾT", Color.FromArgb(155, 89, 182));
            btnViewDetail.Location = new Point(toolBar.Width - 310, 10);
            btnViewDetail.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnViewDetail.Click += BtnViewDetail_Click;

            Button btnCancelTicket = CreateButton("❌ HỦY VÉ", _cgvRed);
            btnCancelTicket.Location = new Point(toolBar.Width - 160, 10);
            btnCancelTicket.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelTicket.Click += BtnCancelTicket_Click;

            toolBar.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnSearch, btnViewDetail, btnCancelTicket });

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

        private void LoadTickets() { MessageBox.Show("Chức năng đang được phát triển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        private void BtnViewDetail_Click(object sender, EventArgs e) { MessageBox.Show("Chức năng xem chi tiết vé đang được phát triển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        private void BtnCancelTicket_Click(object sender, EventArgs e) { MessageBox.Show("Chức năng hủy vé đang được phát triển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_TicketManagement";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
