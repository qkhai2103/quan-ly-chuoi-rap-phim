using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_TrendAnalysis : UserControl
    {
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_TrendAnalysis()
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
                Text = "PHÂN TÍCH XU HƯỚNG",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 15, 15),
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20),
                AutoSize = true
            };

            Label lblPlaceholder = new Label
            {
                Text = "🎯 Phân tích xu hướng đang được phát triển...\n\n" +
                       "Sẽ bao gồm:\n" +
                       "• Top phim bán chạy nhất\n" +
                       "• Xu hướng theo thời gian (ngày/tuần/tháng)\n" +
                       "• Phân tích khung giờ vàng\n" +
                       "• Dự đoán xu hướng tương lai",
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
            this.Name = "UC_TrendAnalysis";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
