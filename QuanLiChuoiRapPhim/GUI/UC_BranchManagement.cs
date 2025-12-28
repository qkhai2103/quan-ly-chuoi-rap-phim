using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_BranchManagement : UserControl
    {
        private DataGridView dgvBranches;
        private TextBox txtSearch;
        private Label lblTotalBranches, lblActiveBranches;
        
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_BranchManagement()
        {
            InitializeComponent();
            SetupUI();
            LoadBranches();
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);

            // Header
            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ CHI NHÁNH",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20)
            };

            // Stats Panel
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 10, 0, 20) };
            TableLayoutPanel statsGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var card1 = CreateStatCard("TỔNG CHI NHÁNH", "0", Color.FromArgb(226, 26, 60), out lblTotalBranches);
            var card2 = CreateStatCard("ĐANG HOẠT ĐỘNG", "0", Color.FromArgb(39, 174, 96), out lblActiveBranches);
            
            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsPanel.Controls.Add(statsGrid);

            // Toolbar
            Panel toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(20, 10, 20, 10) };
            toolBar.BorderRadius(12);

            Button btnAdd = CreateButton("➕ THÊM CHI NHÁNH", _cgvRed);
            btnAdd.Location = new Point(toolBar.Width - 180, 10);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += BtnAdd_Click;

            Button btnEdit = CreateButton("✏️ SỬA", Color.FromArgb(52, 152, 219));
            btnEdit.Location = new Point(toolBar.Width - 330, 10);
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.Click += BtnEdit_Click;

            Button btnDelete = CreateButton("🗑️ XÓA", Color.FromArgb(231, 76, 60));
            btnDelete.Location = new Point(toolBar.Width - 480, 10);
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Click += BtnDelete_Click;

            toolBar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            // DataGridView
            Panel gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            gridPanel.BorderRadius(12);

            dgvBranches = new DataGridView
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

            gridPanel.Controls.Add(dgvBranches);

            // Assemble
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
            // TODO: Implement load branches from BLL
            MessageBox.Show("Chức năng đang được phát triển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng thêm chi nhánh đang được phát triển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng sửa chi nhánh đang được phát triển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng xóa chi nhánh đang được phát triển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_BranchManagement";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
