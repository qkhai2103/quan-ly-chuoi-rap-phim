using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_BranchPerformance : UserControl
    {
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_BranchPerformance()
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
                Text = "HIỆU SUẤT CHI NHÁNH",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 15, 15),
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20),
                AutoSize = true
            };

            Label lblPlaceholder = new Label
            {
                Text = "⭐ Đánh giá hiệu suất chi nhánh đang được phát triển...\n\n" +
                       "Sẽ bao gồm:\n" +
                       "• So sánh doanh thu các chi nhánh\n" +
                       "• Xếp hạng hiệu suất\n" +
                       "• Phân tích công suất phòng chiếu\n" +
                       "• Đánh giá chất lượng dịch vụ",
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
            this.Name = "UC_BranchPerformance";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
