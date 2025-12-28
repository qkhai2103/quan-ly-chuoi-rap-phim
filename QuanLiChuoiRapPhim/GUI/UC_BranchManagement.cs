using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_BranchManagement : UserControl
    {
        private FlowLayoutPanel flpCards;
        private Label lblTotalBranches, lblActiveBranches;
        private ChiNhanhBLL _chiNhanhBLL = new ChiNhanhBLL();
        private DataTable _dtChiNhanh;
        
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private Color _cgvWhite = Color.White;

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

            var card1 = CreateStatCard("TỔNG CHI NHÁNH", "0", _cgvRed, out lblTotalBranches);
            var card2 = CreateStatCard("ĐANG HOẠT ĐỘNG", "0", Color.FromArgb(39, 174, 96), out lblActiveBranches);
            
            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsPanel.Controls.Add(statsGrid);

            // Toolbar
            Panel toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = _cgvWhite, Padding = new Padding(20, 10, 20, 10) };
            toolBar.BorderRadius(12);

            TextBox txtSearch = new TextBox { Location = new Point(20, 15), Size = new Size(300, 35), Font = new Font("Segoe UI", 10), Text = "Tìm kiếm chi nhánh..." };
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Enter += (s, e) => { if (txtSearch.Text == "Tìm kiếm chi nhánh...") { txtSearch.Text = ""; txtSearch.ForeColor = _cgvBlack; } };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrEmpty(txtSearch.Text)) { txtSearch.Text = "Tìm kiếm chi nhánh..."; txtSearch.ForeColor = Color.Gray; } };
            txtSearch.TextChanged += (s, e) => TimKiem(txtSearch.Text);

            Button btnAdd = CreateButton("➕ THÊM CHI NHÁNH", _cgvRed);
            btnAdd.Location = new Point(toolBar.Width - 180, 10);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += BtnAdd_Click;

            toolBar.Controls.AddRange(new Control[] { txtSearch, btnAdd });

            // Cards Container
            Panel cardsPanel = new Panel { Dock = DockStyle.Fill, BackColor = _cgvLightGray, Padding = new Padding(0) };
            
            flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = _cgvLightGray,
                Padding = new Padding(10)
            };

            cardsPanel.Controls.Add(flpCards);

            // Assemble
            this.Controls.Add(cardsPanel);
            Panel spacer = new Panel { Dock = DockStyle.Top, Height = 20 };
            this.Controls.Add(spacer);
            this.Controls.Add(toolBar);
            this.Controls.Add(statsPanel);
            this.Controls.Add(lblTitle);
        }

        private Panel CreateStatCard(string title, string value, Color accentColor, out Label valueLabel)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = _cgvWhite, Margin = new Padding(0, 0, 20, 0) };
            card.BorderRadius(15);

            Panel accent = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accentColor };
            card.Controls.Add(accent);

            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.Gray, Location = new Point(25, 20), AutoSize = true };
            valueLabel = new Label { Text = value, Font = new Font("Montserrat", 22, FontStyle.Bold), ForeColor = _cgvBlack, Location = new Point(22, 45), AutoSize = true };

            card.Controls.AddRange(new Control[] { lblTitle, valueLabel });
            return card;
        }

        private Panel CreateBranchCard(int maChiNhanh, string tenChiNhanh, string diaChi, string soDienThoai, bool trangThai)
        {
            Panel card = new Panel { Size = new Size(280, 220), BackColor = _cgvWhite, Margin = new Padding(10) };
            card.BorderRadius(12);

            // Header với màu sắc
            Panel header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = trangThai ? _cgvRed : Color.FromArgb(200, 200, 200) };
            header.BorderRadius(12);

            Label lblTenChiNhanh = new Label
            {
                Text = tenChiNhanh,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _cgvWhite,
                Location = new Point(15, 12),
                AutoSize = true
            };
            header.Controls.Add(lblTenChiNhanh);

            // Content
            Panel content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            Label lblDiaChiLabel = new Label { Text = "📍 Địa chỉ:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = _cgvBlack, Location = new Point(15, 70), AutoSize = true };
            Label lblDiaChiValue = new Label { Text = diaChi, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(15, 90), Size = new Size(250, 40), AutoEllipsis = true };

            Label lblPhoneLabel = new Label { Text = "📞 Điện thoại:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = _cgvBlack, Location = new Point(15, 135), AutoSize = true };
            Label lblPhoneValue = new Label { Text = soDienThoai, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(15, 155), AutoSize = true };

            // Buttons
            Button btnEdit = new Button { Text = "✏️ Sửa", Size = new Size(80, 30), Location = new Point(15, 180), BackColor = Color.FromArgb(0, 123, 255), ForeColor = _cgvWhite, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) => BtnEdit_Click(maChiNhanh, tenChiNhanh, diaChi, soDienThoai);

            Button btnDelete = new Button { Text = "🗑️ Xóa", Size = new Size(80, 30), Location = new Point(105, 180), BackColor = _cgvRed, ForeColor = _cgvWhite, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) => BtnDelete_Click(maChiNhanh, tenChiNhanh);

            Label lblStatus = new Label { Text = trangThai ? "✓ Hoạt động" : "✗ Ngừng hoạt động", Font = new Font("Segoe UI", 8), ForeColor = trangThai ? Color.Green : Color.Red, Location = new Point(195, 180), AutoSize = true };

            card.Controls.Add(header);
            card.Controls.AddRange(new Control[] { lblDiaChiLabel, lblDiaChiValue, lblPhoneLabel, lblPhoneValue, btnEdit, btnDelete, lblStatus });

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
                _dtChiNhanh = _chiNhanhBLL.LayTatCaChiNhanh();
                
                flpCards.Controls.Clear();
                
                int totalBranches = 0;
                int activeBranches = 0;
                
                foreach (DataRow row in _dtChiNhanh.Rows)
                {
                    int maChiNhanh = Convert.ToInt32(row["MaChiNhanh"]);
                    string tenChiNhanh = row["TenChiNhanh"].ToString();
                    string diaChi = row["DiaChi"].ToString();
                    string soDienThoai = row["SoDienThoai"].ToString();
                    bool trangThai = Convert.ToBoolean(row["TrangThai"]);

                    Panel card = CreateBranchCard(maChiNhanh, tenChiNhanh, diaChi, soDienThoai, trangThai);
                    flpCards.Controls.Add(card);

                    totalBranches++;
                    if (trangThai) activeBranches++;
                }
                
                lblTotalBranches.Text = totalBranches.ToString();
                lblActiveBranches.Text = activeBranches.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TimKiem(string tuKhoa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tuKhoa) || tuKhoa == "Tìm kiếm chi nhánh...")
                {
                    LoadBranches();
                    return;
                }

                DataTable dtKetQua = _chiNhanhBLL.TimKiemChiNhanh(tuKhoa);
                
                flpCards.Controls.Clear();
                
                foreach (DataRow row in dtKetQua.Rows)
                {
                    int maChiNhanh = Convert.ToInt32(row["MaChiNhanh"]);
                    string tenChiNhanh = row["TenChiNhanh"].ToString();
                    string diaChi = row["DiaChi"].ToString();
                    string soDienThoai = row["SoDienThoai"].ToString();
                    bool trangThai = Convert.ToBoolean(row["TrangThai"]);

                    Panel card = CreateBranchCard(maChiNhanh, tenChiNhanh, diaChi, soDienThoai, trangThai);
                    flpCards.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (Form frmThemChiNhanh = new Form())
            {
                frmThemChiNhanh.Text = "Thêm Chi Nhánh";
                frmThemChiNhanh.Size = new Size(450, 280);
                frmThemChiNhanh.StartPosition = FormStartPosition.CenterParent;

                Label lblTenChiNhanh = new Label() { Text = "Tên Chi Nhánh:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtTenChiNhanh = new TextBox() { Location = new Point(150, 20), Size = new Size(270, 25) };

                Label lblDiaChi = new Label() { Text = "Địa Chỉ:", Location = new Point(20, 60), AutoSize = true };
                TextBox txtDiaChi = new TextBox() { Location = new Point(150, 60), Size = new Size(270, 25) };

                Label lblSoDienThoai = new Label() { Text = "Số Điện Thoại:", Location = new Point(20, 100), AutoSize = true };
                TextBox txtSoDienThoai = new TextBox() { Location = new Point(150, 100), Size = new Size(270, 25) };

                Button btnLuu = new Button() { Text = "Lưu", Location = new Point(200, 160), Size = new Size(80, 35), BackColor = _cgvRed, ForeColor = _cgvWhite };
                Button btnHuy = new Button() { Text = "Hủy", Location = new Point(300, 160), Size = new Size(80, 35) };

                btnLuu.Click += (s, e2) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(txtTenChiNhanh.Text))
                        {
                            MessageBox.Show("Vui lòng nhập tên chi nhánh!", "Thông báo");
                            return;
                        }

                        _chiNhanhBLL.ThemChiNhanh(txtTenChiNhanh.Text, txtDiaChi.Text, txtSoDienThoai.Text);

                        MessageBox.Show("Thêm chi nhánh thành công!", "Thông báo");
                        frmThemChiNhanh.Close();
                        LoadBranches();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnHuy.Click += (s, e2) => frmThemChiNhanh.Close();

                frmThemChiNhanh.Controls.AddRange(new Control[] {
                    lblTenChiNhanh, txtTenChiNhanh, lblDiaChi, txtDiaChi, lblSoDienThoai, txtSoDienThoai, btnLuu, btnHuy
                });

                frmThemChiNhanh.ShowDialog();
            }
        }

        private void BtnEdit_Click(int maChiNhanh, string tenChiNhanh, string diaChi, string soDienThoai)
        {
            using (Form frmSuaChiNhanh = new Form())
            {
                frmSuaChiNhanh.Text = "Sửa Chi Nhánh";
                frmSuaChiNhanh.Size = new Size(450, 280);
                frmSuaChiNhanh.StartPosition = FormStartPosition.CenterParent;

                Label lblTenChiNhanh = new Label() { Text = "Tên Chi Nhánh:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtTenChiNhanh = new TextBox() { Location = new Point(150, 20), Size = new Size(270, 25), Text = tenChiNhanh };

                Label lblDiaChi = new Label() { Text = "Địa Chỉ:", Location = new Point(20, 60), AutoSize = true };
                TextBox txtDiaChi = new TextBox() { Location = new Point(150, 60), Size = new Size(270, 25), Text = diaChi };

                Label lblSoDienThoai = new Label() { Text = "Số Điện Thoại:", Location = new Point(20, 100), AutoSize = true };
                TextBox txtSoDienThoai = new TextBox() { Location = new Point(150, 100), Size = new Size(270, 25), Text = soDienThoai };

                Button btnLuu = new Button() { Text = "Lưu", Location = new Point(200, 160), Size = new Size(80, 35), BackColor = _cgvRed, ForeColor = _cgvWhite };
                Button btnHuy = new Button() { Text = "Hủy", Location = new Point(300, 160), Size = new Size(80, 35) };

                btnLuu.Click += (s, e2) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(txtTenChiNhanh.Text))
                        {
                            MessageBox.Show("Vui lòng nhập tên chi nhánh!", "Thông báo");
                            return;
                        }

                        _chiNhanhBLL.CapNhatChiNhanh(maChiNhanh, txtTenChiNhanh.Text, txtDiaChi.Text, txtSoDienThoai.Text);

                        MessageBox.Show("Cập nhật chi nhánh thành công!", "Thông báo");
                        frmSuaChiNhanh.Close();
                        LoadBranches();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnHuy.Click += (s, e2) => frmSuaChiNhanh.Close();

                frmSuaChiNhanh.Controls.AddRange(new Control[] {
                    lblTenChiNhanh, txtTenChiNhanh, lblDiaChi, txtDiaChi, lblSoDienThoai, txtSoDienThoai, btnLuu, btnHuy
                });

                frmSuaChiNhanh.ShowDialog();
            }
        }

        private void BtnDelete_Click(int maChiNhanh, string tenChiNhanh)
        {
            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa chi nhánh '{tenChiNhanh}'?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _chiNhanhBLL.XoaChiNhanh(maChiNhanh);
                    MessageBox.Show("Xóa chi nhánh thành công!", "Thông báo");
                    LoadBranches();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
