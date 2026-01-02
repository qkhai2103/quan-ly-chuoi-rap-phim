using System.Drawing;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.Services
{
    /// <summary>
    /// UI Constants and Helpers for CGV Cinema branding consistency
    /// Phase 4: UI consistency review
    /// </summary>
    public static class UIConstants
    {
        #region CGV Brand Colors

        /// <summary>
        /// CGV Primary Red - Main brand color
        /// </summary>
        public static readonly Color CgvRed = Color.FromArgb(226, 26, 60);

        /// <summary>
        /// CGV Black - Text and headers
        /// </summary>
        public static readonly Color CgvBlack = Color.FromArgb(15, 15, 15);

        /// <summary>
        /// CGV Dark Gray - Secondary text, borders
        /// </summary>
        public static readonly Color CgvDarkGray = Color.FromArgb(45, 45, 45);

        /// <summary>
        /// CGV Light Gray - Backgrounds
        /// </summary>
        public static readonly Color CgvLightGray = Color.FromArgb(245, 245, 245);

        /// <summary>
        /// CGV Gold - Premium, highlights
        /// </summary>
        public static readonly Color CgvGold = Color.FromArgb(255, 193, 7);

        /// <summary>
        /// CGV Text Color - Body text
        /// </summary>
        public static readonly Color CgvTextColor = Color.FromArgb(33, 33, 33);

        #endregion

        #region Status Colors

        /// <summary>
        /// Success color (Green)
        /// </summary>
        public static readonly Color SuccessColor = Color.FromArgb(40, 167, 69);

        /// <summary>
        /// Warning color (Orange)
        /// </summary>
        public static readonly Color WarningColor = Color.FromArgb(255, 193, 7);

        /// <summary>
        /// Danger/Error color (Red)
        /// </summary>
        public static readonly Color DangerColor = Color.FromArgb(220, 53, 69);

        /// <summary>
        /// Info color (Blue)
        /// </summary>
        public static readonly Color InfoColor = Color.FromArgb(52, 152, 219);

        #endregion

        #region Fonts

        /// <summary>
        /// Header font - Large titles
        /// </summary>
        public static readonly Font HeaderFont = new Font("Segoe UI", 20, FontStyle.Bold);

        /// <summary>
        /// Subheader font - Section titles
        /// </summary>
        public static readonly Font SubheaderFont = new Font("Segoe UI", 14, FontStyle.Bold);

        /// <summary>
        /// Body font - Regular text
        /// </summary>
        public static readonly Font BodyFont = new Font("Segoe UI", 11);

        /// <summary>
        /// Small font - Labels, hints
        /// </summary>
        public static readonly Font SmallFont = new Font("Segoe UI", 9);

        /// <summary>
        /// Button font - Action buttons
        /// </summary>
        public static readonly Font ButtonFont = new Font("Segoe UI", 10, FontStyle.Bold);

        #endregion

        #region Button Styles

        /// <summary>
        /// Apply CGV primary button style (Red background, white text)
        /// </summary>
        public static void ApplyPrimaryButtonStyle(Button button)
        {
            button.BackColor = CgvRed;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Apply secondary button style (Dark gray background, white text)
        /// </summary>
        public static void ApplySecondaryButtonStyle(Button button)
        {
            button.BackColor = CgvDarkGray;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Apply success button style (Green background, white text)
        /// </summary>
        public static void ApplySuccessButtonStyle(Button button)
        {
            button.BackColor = SuccessColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Apply danger button style (Red background, white text)
        /// </summary>
        public static void ApplyDangerButtonStyle(Button button)
        {
            button.BackColor = DangerColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Apply outline button style (White background, red border)
        /// </summary>
        public static void ApplyOutlineButtonStyle(Button button)
        {
            button.BackColor = Color.White;
            button.ForeColor = CgvRed;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = CgvRed;
            button.FlatAppearance.BorderSize = 2;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
        }

        #endregion

        #region Panel Styles

        /// <summary>
        /// Apply header panel style (CGV Black background)
        /// </summary>
        public static void ApplyHeaderPanelStyle(Panel panel)
        {
            panel.BackColor = CgvBlack;
            panel.Padding = new Padding(25, 0, 25, 0);
        }

        /// <summary>
        /// Apply card panel style (White background with padding)
        /// </summary>
        public static void ApplyCardPanelStyle(Panel panel)
        {
            panel.BackColor = Color.White;
            panel.Padding = new Padding(15);
        }

        /// <summary>
        /// Apply section panel style (Light gray background)
        /// </summary>
        public static void ApplySectionPanelStyle(Panel panel)
        {
            panel.BackColor = CgvLightGray;
            panel.Padding = new Padding(20);
        }

        #endregion

        #region DataGridView Styles

        /// <summary>
        /// Apply CGV style to DataGridView
        /// </summary>
        public static void ApplyDataGridViewStyle(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(230, 230, 230);
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Header style
            dgv.ColumnHeadersDefaultCellStyle.BackColor = CgvBlack;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 45;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Row style
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.ForeColor = CgvTextColor;
            dgv.DefaultCellStyle.Padding = new Padding(8, 5, 8, 5);
            dgv.RowTemplate.Height = 40;

            // Alternating row style
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Selection style
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 238);
            dgv.DefaultCellStyle.SelectionForeColor = CgvRed;
        }

        #endregion

        #region TextBox Styles

        /// <summary>
        /// Apply search textbox style
        /// </summary>
        public static void ApplySearchTextBoxStyle(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.None;
            textBox.Font = new Font("Segoe UI", 11);
            textBox.ForeColor = CgvTextColor;
        }

        #endregion

        #region Label Styles

        /// <summary>
        /// Create a header label
        /// </summary>
        public static Label CreateHeaderLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = HeaderFont,
                ForeColor = Color.White,
                AutoSize = true
            };
        }

        /// <summary>
        /// Create a section label
        /// </summary>
        public static Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = SubheaderFont,
                ForeColor = CgvBlack,
                AutoSize = true
            };
        }

        #endregion

        #region Spacing Constants

        /// <summary>
        /// Standard padding
        /// </summary>
        public const int StandardPadding = 20;

        /// <summary>
        /// Small padding
        /// </summary>
        public const int SmallPadding = 10;

        /// <summary>
        /// Large padding
        /// </summary>
        public const int LargePadding = 30;

        /// <summary>
        /// Header panel height
        /// </summary>
        public const int HeaderHeight = 80;

        /// <summary>
        /// Button height
        /// </summary>
        public const int ButtonHeight = 40;

        /// <summary>
        /// Standard row height
        /// </summary>
        public const int RowHeight = 45;

        #endregion
    }
}
