using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_PricingPromotion : UserControl
    {
        private TabControl tabControl;
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private GiaVeBLL _giaVeBLL = new GiaVeBLL();

        public UC_PricingPromotion()
        {
            InitializeComponent();
            SetupUI();
            LoadGiaVe();
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);

            Label lblTitle = new Label
            {
                Text = "GIÁ VÉ & KHUYẾN MÃI",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20),
                AutoSize = true
            };

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11) };

            // Tab 1: Bảng Giá Vé
            TabPage tabPricing = new TabPage("💳 Bảng Giá Vé");
            tabPricing.BackColor = Color.White;
            tabPricing.Padding = new Padding(20);

            DataGridView dgvGiaVe = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, RowHeadersVisible = false, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvGiaVe.Columns.Add("MaCauHinh", "Mã");
            dgvGiaVe.Columns.Add("LoaiGhe", "Loại Ghế");
            dgvGiaVe.Columns.Add("LoaiNgay", "Loại Ngày");
            dgvGiaVe.Columns.Add("DoiTuongKhachHang", "Đối Tượng");
            dgvGiaVe.Columns.Add("GiaGoc", "Giá Gốc");
            dgvGiaVe.Columns.Add("PhuThu", "Phụ Thu");
            dgvGiaVe.Columns.Add("GiaThuc", "Giá Thực");

            Button btnSuaGia = new Button { Text = "✏️ Sửa Giá", BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Size = new Size(100, 35), Cursor = Cursors.Hand };
            btnSuaGia.FlatAppearance.BorderSize = 0;
            btnSuaGia.Click += (s, e) =>
            {
                if (dgvGiaVe.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn dòng cần sửa!", "Thông báo");
                    return;
                }

                int maCauHinh = Convert.ToInt32(dgvGiaVe.SelectedRows[0].Cells[0].Value);
                string giaGocText = dgvGiaVe.SelectedRows[0].Cells[4].Value.ToString().Replace(" đ", "");
                string phuThuText = dgvGiaVe.SelectedRows[0].Cells[5].Value.ToString().Replace(" đ", "");

                using (Form frmSua = new Form())
                {
                    frmSua.Text = "Sửa Giá Vé";
                    frmSua.Size = new Size(350, 200);
                    frmSua.StartPosition = FormStartPosition.CenterParent;

                    Label lblGiaGoc = new Label() { Text = "Giá Gốc:", Location = new Point(20, 20), AutoSize = true };
                    TextBox txtGiaGoc = new TextBox() { Location = new Point(120, 20), Size = new Size(200, 25), Text = giaGocText };

                    Label lblPhuThu = new Label() { Text = "Phụ Thu:", Location = new Point(20, 60), AutoSize = true };
                    TextBox txtPhuThu = new TextBox() { Location = new Point(120, 60), Size = new Size(200, 25), Text = phuThuText };

                    Button btnLuu = new Button() { Text = "Lưu", Location = new Point(120, 120), Size = new Size(80, 35), BackColor = _cgvRed, ForeColor = Color.White };
                    Button btnHuy = new Button() { Text = "Hủy", Location = new Point(220, 120), Size = new Size(80, 35) };

                    btnLuu.Click += (s2, e2) =>
                    {
                        try
                        {
                            if (!decimal.TryParse(txtGiaGoc.Text, out decimal newGiaGoc) || !decimal.TryParse(txtPhuThu.Text, out decimal newPhuThu))
                            {
                                MessageBox.Show("Giá phải là số!", "Thông báo");
                                return;
                            }

                            _giaVeBLL.CapNhatGiaVe(maCauHinh, newGiaGoc, newPhuThu);
                            MessageBox.Show("Cập nhật giá vé thành công!", "Thông báo");
                            frmSua.Close();
                            LoadGiaVe();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    btnHuy.Click += (s2, e2) => frmSua.Close();

                    frmSua.Controls.AddRange(new Control[] { lblGiaGoc, txtGiaGoc, lblPhuThu, txtPhuThu, btnLuu, btnHuy });
                    frmSua.ShowDialog();
                }
            };

            Panel toolBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White, Padding = new Padding(20, 10, 20, 10) };
            toolBar.Controls.Add(btnSuaGia);

            tabPricing.Controls.Add(dgvGiaVe);
            tabPricing.Controls.Add(toolBar);

            // Tab 2: Khuyến Mãi
            TabPage tabPromotion = new TabPage("🎁 Khuyến Mãi");
            Label lblPromotionPlaceholder = new Label { Text = "Chức năng quản lý khuyến mãi đang được phát triển...", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14), ForeColor = Color.Gray };
            tabPromotion.Controls.Add(lblPromotionPlaceholder);

            tabControl.TabPages.Add(tabPricing);
            tabControl.TabPages.Add(tabPromotion);

            this.Controls.Add(tabControl);
            this.Controls.Add(lblTitle);
        }

        private void LoadGiaVe()
        {
            try
            {
                DataTable dt = _giaVeBLL.LayTatCaCauHinhGiaVe();
                DataGridView dgvGiaVe = (DataGridView)tabControl.TabPages[0].Controls[0];
                dgvGiaVe.Rows.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    decimal giaGoc = Convert.ToDecimal(row["GiaGoc"]);
                    decimal phuThu = Convert.ToDecimal(row["PhuThu"]);
                    decimal giaThuc = giaGoc + phuThu;

                    dgvGiaVe.Rows.Add(
                        row["MaCauHinh"],
                        row["LoaiGhe"],
                        row["LoaiNgay"],
                        row["DoiTuongKhachHang"],
                        giaGoc.ToString("N0") + " đ",
                        phuThu.ToString("N0") + " đ",
                        giaThuc.ToString("N0") + " đ"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_PricingPromotion";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
