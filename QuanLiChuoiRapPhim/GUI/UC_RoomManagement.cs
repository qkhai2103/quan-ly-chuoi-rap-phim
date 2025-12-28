using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_RoomManagement : UserControl
    {
        private DataGridView dgvRooms;
        private Label lblTotalRooms, lblTotalSeats;
        
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_RoomManagement()
        {
            InitializeComponent();
            SetupUI();
            LoadRooms();
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);

            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ PHÒNG CHIẾU",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20)
            };

            // Stats
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 10, 0, 20) };
            TableLayoutPanel statsGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var card1 = CreateStatCard("TỔNG PHÒNG CHIẾU", "0", Color.FromArgb(226, 26, 60), out lblTotalRooms);
            var card2 = CreateStatCard("TỔNG SỐ GHẾ", "0", Color.FromArgb(39, 174, 96), out lblTotalSeats);
            
            statsGrid.Controls.Add(card1, 0, 0);
            statsGrid.Controls.Add(card2, 1, 0);
            statsPanel.Controls.Add(statsGrid);

            // Toolbar
            Panel toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(20, 10, 20, 10) };
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

            // Grid
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
                RowTemplate = { Height = 50 }
            };

            gridPanel.Controls.Add(dgvRooms);

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

        private void LoadRooms()
        {
            try
            {
                PhongChieuBLL phongChieuBLL = new PhongChieuBLL();
                DataTable dt = phongChieuBLL.LayTatCaPhongChieu();
                
                dgvRooms.Rows.Clear();
                dgvRooms.Columns.Clear();
                
                dgvRooms.Columns.Add("MaPhong", "Mã");
                dgvRooms.Columns.Add("TenPhong", "Tên Phòng");
                dgvRooms.Columns.Add("ChiNhanh", "Chi Nhánh");
                dgvRooms.Columns.Add("TongSoGhe", "Tổng Ghế");
                dgvRooms.Columns.Add("TrangThai", "Trạng Thái");
                
                int totalRooms = 0;
                int totalSeats = 0;
                
                foreach (DataRow row in dt.Rows)
                {
                    dgvRooms.Rows.Add(
                        row["MaPhong"],
                        row["TenPhong"],
                        row["TenChiNhanh"],
                        row["TongSoGhe"],
                        Convert.ToBoolean(row["TrangThai"]) ? "Hoạt động" : "Ngừng hoạt động"
                    );
                    totalRooms++;
                    totalSeats += Convert.ToInt32(row["TongSoGhe"]);
                }
                
                lblTotalRooms.Text = totalRooms.ToString();
                lblTotalSeats.Text = totalSeats.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (Form frmThemPhong = new Form())
            {
                frmThemPhong.Text = "Thêm Phòng Chiếu";
                frmThemPhong.Size = new Size(400, 250);
                frmThemPhong.StartPosition = FormStartPosition.CenterParent;

                Label lblTenPhong = new Label() { Text = "Tên Phòng:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtTenPhong = new TextBox() { Location = new Point(120, 20), Size = new Size(250, 25) };

                Label lblChiNhanh = new Label() { Text = "Chi Nhánh:", Location = new Point(20, 60), AutoSize = true };
                ComboBox cboChiNhanh = new ComboBox() { Location = new Point(120, 60), Size = new Size(250, 25) };
                cboChiNhanh.Items.AddRange(new[] { "CGV Vincom Xuân Khánh", "CGV Sense City", "CGV Vincom Hùng Vương" });

                Label lblTongSoGhe = new Label() { Text = "Tổng Số Ghế:", Location = new Point(20, 100), AutoSize = true };
                TextBox txtTongSoGhe = new TextBox() { Location = new Point(120, 100), Size = new Size(250, 25) };

                Button btnLuu = new Button() { Text = "Lưu", Location = new Point(150, 160), Size = new Size(80, 35), BackColor = _cgvRed, ForeColor = Color.White };
                Button btnHuy = new Button() { Text = "Hủy", Location = new Point(250, 160), Size = new Size(80, 35) };

                btnLuu.Click += (s, e2) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(txtTenPhong.Text))
                        {
                            MessageBox.Show("Vui lòng nhập tên phòng!", "Thông báo");
                            return;
                        }

                        if (!int.TryParse(txtTongSoGhe.Text, out int tongSoGhe))
                        {
                            MessageBox.Show("Tổng số ghế phải là số!", "Thông báo");
                            return;
                        }

                        PhongChieuBLL phongChieuBLL = new PhongChieuBLL();
                        int maChiNhanh = cboChiNhanh.SelectedIndex + 1;
                        phongChieuBLL.ThemPhongChieu(txtTenPhong.Text, maChiNhanh, tongSoGhe);

                        MessageBox.Show("Thêm phòng chiếu thành công!", "Thông báo");
                        frmThemPhong.Close();
                        LoadRooms();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnHuy.Click += (s, e2) => frmThemPhong.Close();

                frmThemPhong.Controls.AddRange(new Control[] {
                    lblTenPhong, txtTenPhong, lblChiNhanh, cboChiNhanh, lblTongSoGhe, txtTongSoGhe, btnLuu, btnHuy
                });

                frmThemPhong.ShowDialog();
            }
        }

        private void BtnDesignSeats_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng chiếu!", "Thông báo");
                return;
            }

            MessageBox.Show("Chức năng thiết kế sơ đồ ghế đang được phát triển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng chiếu cần sửa!", "Thông báo");
                return;
            }

            int maPhong = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["MaPhong"].Value);
            string tenPhong = dgvRooms.SelectedRows[0].Cells["TenPhong"].Value.ToString();
            int tongSoGhe = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["TongSoGhe"].Value);

            using (Form frmSuaPhong = new Form())
            {
                frmSuaPhong.Text = "Sửa Phòng Chiếu";
                frmSuaPhong.Size = new Size(400, 200);
                frmSuaPhong.StartPosition = FormStartPosition.CenterParent;

                Label lblTenPhong = new Label() { Text = "Tên Phòng:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtTenPhong = new TextBox() { Location = new Point(120, 20), Size = new Size(250, 25), Text = tenPhong };

                Label lblTongSoGhe = new Label() { Text = "Tổng Số Ghế:", Location = new Point(20, 60), AutoSize = true };
                TextBox txtTongSoGhe = new TextBox() { Location = new Point(120, 60), Size = new Size(250, 25), Text = tongSoGhe.ToString() };

                Button btnLuu = new Button() { Text = "Lưu", Location = new Point(150, 120), Size = new Size(80, 35), BackColor = _cgvRed, ForeColor = Color.White };
                Button btnHuy = new Button() { Text = "Hủy", Location = new Point(250, 120), Size = new Size(80, 35) };

                btnLuu.Click += (s, e2) =>
                {
                    try
                    {
                        if (!int.TryParse(txtTongSoGhe.Text, out int newTongSoGhe))
                        {
                            MessageBox.Show("Tổng số ghế phải là số!", "Thông báo");
                            return;
                        }

                        PhongChieuBLL phongChieuBLL = new PhongChieuBLL();
                        phongChieuBLL.CapNhatPhongChieu(maPhong, txtTenPhong.Text, newTongSoGhe);

                        MessageBox.Show("Cập nhật phòng chiếu thành công!", "Thông báo");
                        frmSuaPhong.Close();
                        LoadRooms();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnHuy.Click += (s, e2) => frmSuaPhong.Close();

                frmSuaPhong.Controls.AddRange(new Control[] {
                    lblTenPhong, txtTenPhong, lblTongSoGhe, txtTongSoGhe, btnLuu, btnHuy
                });

                frmSuaPhong.ShowDialog();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng chiếu cần xóa!", "Thông báo");
                return;
            }

            int maPhong = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["MaPhong"].Value);
            string tenPhong = dgvRooms.SelectedRows[0].Cells["TenPhong"].Value.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phòng '{tenPhong}'?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    PhongChieuBLL phongChieuBLL = new PhongChieuBLL();
                    phongChieuBLL.XoaPhongChieu(maPhong);
                    MessageBox.Show("Xóa phòng chiếu thành công!", "Thông báo");
                    LoadRooms();
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
            this.Name = "UC_RoomManagement";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
