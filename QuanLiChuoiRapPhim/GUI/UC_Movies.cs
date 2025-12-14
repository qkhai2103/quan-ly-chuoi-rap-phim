using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_Movies : UserControl
    {
        private DataGridView dgvMovies;
        private Button btnAddMovie, btnEditMovie, btnDeleteMovie, btnRefreshMovies;
        private TextBox txtSearchMovie;
        public UC_Movies()
        {
            InitializeComponent();
            SetupUI();
            LoadMovies();
        }
        private void SetupUI()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // Panel tiêu đề
            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 60;
            titlePanel.BackColor = Color.FromArgb(220, 53, 69);

            Label lblTitle = new Label();
            lblTitle.Text = "🎬 QUẢN LÝ PHIM";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            titlePanel.Controls.Add(lblTitle);

            // Panel chức năng
            Panel functionPanel = new Panel();
            functionPanel.Dock = DockStyle.Top;
            functionPanel.Height = 50;
            functionPanel.BackColor = Color.FromArgb(248, 249, 250);
            functionPanel.Padding = new Padding(20, 8, 20, 8);

            btnAddMovie = new Button();
            btnAddMovie.Text = "➕ Thêm phim";
            btnAddMovie.Size = new Size(120, 35);
            btnAddMovie.Location = new Point(20, 8);
            btnAddMovie.BackColor = Color.FromArgb(40, 167, 69);
            btnAddMovie.ForeColor = Color.White;
            btnAddMovie.Click += BtnAddMovie_Click;

            btnEditMovie = new Button();
            btnEditMovie.Text = "✏️ Sửa phim";
            btnEditMovie.Size = new Size(120, 35);
            btnEditMovie.Location = new Point(150, 8);
            btnEditMovie.BackColor = Color.FromArgb(0, 123, 255);
            btnEditMovie.ForeColor = Color.White;
            btnEditMovie.Click += BtnEditMovie_Click;

            btnDeleteMovie = new Button();
            btnDeleteMovie.Text = "🗑️ Xóa phim";
            btnDeleteMovie.Size = new Size(120, 35);
            btnDeleteMovie.Location = new Point(280, 8);
            btnDeleteMovie.BackColor = Color.FromArgb(220, 53, 69);
            btnDeleteMovie.ForeColor = Color.White;
            btnDeleteMovie.Click += BtnDeleteMovie_Click;

            btnRefreshMovies = new Button();
            btnRefreshMovies.Text = "🔄 Làm mới";
            btnRefreshMovies.Size = new Size(120, 35);
            btnRefreshMovies.Location = new Point(410, 8);
            btnRefreshMovies.BackColor = Color.FromArgb(23, 162, 184);
            btnRefreshMovies.ForeColor = Color.White;
            btnRefreshMovies.Click += BtnRefreshMovies_Click;

            functionPanel.Controls.AddRange(new Control[] {
                btnAddMovie, btnEditMovie, btnDeleteMovie, btnRefreshMovies
            });

            // DataGridView
            dgvMovies = new DataGridView();
            dgvMovies.Dock = DockStyle.Fill;
            dgvMovies.BackgroundColor = Color.White;
            dgvMovies.RowHeadersVisible = false;
            dgvMovies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMovies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Thêm các cột demo
            dgvMovies.Columns.Add("TenPhim", "TÊN PHIM");
            dgvMovies.Columns.Add("TheLoai", "THỂ LOẠI");
            dgvMovies.Columns.Add("ThoiLuong", "THỜI LƯỢNG");
            dgvMovies.Columns.Add("DaoDien", "ĐẠO DIỄN");
            dgvMovies.Columns.Add("DoTuoi", "ĐỘ TUỔI");
            dgvMovies.Columns.Add("TrangThai", "TRẠNG THÁI");

            // Thêm dữ liệu mẫu
            dgvMovies.Rows.Add("Mai", "Tâm lý, Tình cảm", "131 phút", "Trấn Thành", "C16", "Đang chiếu");
            dgvMovies.Rows.Add("Lật Mặt 7", "Hài, Gia đình", "138 phút", "Lý Hải", "P", "Đang chiếu");
            dgvMovies.Rows.Add("Kung Fu Panda 4", "Hoạt hình, Hài", "94 phút", "Mike Mitchell", "P", "Đang chiếu");

            // Thêm controls
            this.Controls.Add(dgvMovies);
            this.Controls.Add(functionPanel);
            this.Controls.Add(titlePanel);
        }
        private void LoadMovies()
        {
            // TODO: Load dữ liệu từ database
        }

        private void BtnAddMovie_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng Thêm phim đang phát triển!", "Thông báo");
        }
        private void BtnEditMovie_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng Sửa phim đang phát triển!", "Thông báo");
        }
        private void BtnDeleteMovie_Click(object sender, EventArgs e)
        {
            if (dgvMovies.SelectedRows.Count > 0)
            {
                string movieName = dgvMovies.SelectedRows[0].Cells["TenPhim"].Value.ToString();
                if (MessageBox.Show($"Xóa phim '{movieName}'?", "Xác nhận",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dgvMovies.Rows.RemoveAt(dgvMovies.SelectedRows[0].Index);
                }
            }
        }
        private void BtnRefreshMovies_Click(object sender, EventArgs e)
        {
            LoadMovies();
        }
    }
}
