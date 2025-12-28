using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_PricingPromotion : UserControl
    {
        private TabControl tabControl;
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvLightGray = Color.FromArgb(245, 245, 245);

        public UC_PricingPromotion()
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
                Text = "GIÁ VÉ & KHUYẾN MÃI",
                Font = new Font("Montserrat", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 15, 15),
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20),
                AutoSize = true
            };

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11) };
            
            TabPage tabPricing = new TabPage("💳 Bảng Giá Vé");
            TabPage tabPromotion = new TabPage("🎁 Khuyến Mãi");

            Label lblPricingPlaceholder = new Label { Text = "Chức năng quản lý bảng giá đang được phát triển...", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14), ForeColor = Color.Gray };
            Label lblPromotionPlaceholder = new Label { Text = "Chức năng quản lý khuyến mãi đang được phát triển...", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14), ForeColor = Color.Gray };

            tabPricing.Controls.Add(lblPricingPlaceholder);
            tabPromotion.Controls.Add(lblPromotionPlaceholder);

            tabControl.TabPages.Add(tabPricing);
            tabControl.TabPages.Add(tabPromotion);

            this.Controls.Add(tabControl);
            this.Controls.Add(lblTitle);
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
