using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_SystemRevenue : UserControl
    {
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_SystemRevenue()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);

            Label lblTitle = new Label
            {
                Text = "DOANH THU TOÀN HỆ THỐNG",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 15, 15),
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20),
                AutoSize = true
            };

            Label lblPlaceholder = new Label
            {
                Text = "📊 Dashboard doanh thu với biểu đồ đang được phát triển...\n\n" +
                       "Sẽ bao gồm:\n" +
                       "• Biểu đồ cột doanh thu theo tháng\n" +
                       "• Tổng hợp doanh thu toàn chuỗi\n" +
                       "• So sánh doanh thu các chi nhánh\n" +
                       "• Phân tích theo phim/thời gian",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray
            };

            this.Controls.Add(lblPlaceholder);
            this.Controls.Add(lblTitle);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_SystemRevenue";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
