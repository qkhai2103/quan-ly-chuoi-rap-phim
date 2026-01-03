using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// Shared UI Helper Library for consistent styling across all modules
    /// </summary>
    public static class UIHelper
    {
        #region Color Constants (CGV Brand)
        
        public static readonly Color CGV_RED = Color.FromArgb(226, 26, 60);
        public static readonly Color CGV_RED_DARK = Color.FromArgb(180, 20, 45);
        public static readonly Color CGV_BLACK = Color.FromArgb(15, 15, 15);
        public static readonly Color CGV_DARK_GRAY = Color.FromArgb(40, 40, 40);
        public static readonly Color CGV_LIGHT_GRAY = Color.FromArgb(245, 245, 245);
        public static readonly Color CGV_WHITE = Color.White;
        public static readonly Color CGV_TEXT_COLOR = Color.FromArgb(33, 33, 33);
        
        // Semantic Colors
        public static readonly Color SUCCESS_GREEN = Color.FromArgb(39, 174, 96);
        public static readonly Color WARNING_ORANGE = Color.FromArgb(243, 156, 18);
        public static readonly Color ERROR_RED = Color.FromArgb(231, 76, 60);
        public static readonly Color INFO_BLUE = Color.FromArgb(52, 152, 219);
        
        // UI Colors
        public static readonly Color BORDER_COLOR = Color.FromArgb(230, 232, 240);
        public static readonly Color GRID_COLOR = Color.FromArgb(240, 240, 240);
        public static readonly Color HEADER_BG = Color.FromArgb(250, 250, 250);
        
        #endregion

        #region Spacing Constants
        
        public const int SPACE_XS = 5;
        public const int SPACE_SM = 10;
        public const int SPACE_MD = 15;
        public const int SPACE_LG = 20;
        public const int SPACE_XL = 30;
        public const int SPACE_2XL = 40;
        
        #endregion

        #region Font Constants
        
        public static readonly Font FONT_TITLE = new Font("Montserrat", 18, FontStyle.Bold);
        public static readonly Font FONT_HEADER = new Font("Montserrat", 16, FontStyle.Bold);
        public static readonly Font FONT_SUBHEADER = new Font("Segoe UI", 11, FontStyle.Bold);
        public static readonly Font FONT_NORMAL = new Font("Segoe UI", 10);
        public static readonly Font FONT_SMALL = new Font("Segoe UI", 9);
        public static readonly Font FONT_STAT_VALUE = new Font("Montserrat", 22, FontStyle.Bold);
        
        #endregion

        #region Stat Card Component
        
        /// <summary>
        /// Creates a modern stat card with consistent styling
        /// </summary>
        public static Panel CreateStatCard(string title, string value, Color accentColor, out Label valueLabel)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CGV_WHITE,
                Margin = new Padding(0, 0, 20, 0)
            };
            card.BorderRadius(15);

            // Accent bar on left
            Panel accent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = accentColor
            };
            card.Controls.Add(accent);

            // Title label
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 9),
                ForeColor = Color.Gray,
                Location = new Point(25, 20),
                AutoSize = true
            };

            // Value label (returned as out parameter for updates)
            valueLabel = new Label
            {
                Text = value,
                Font = FONT_STAT_VALUE,
                ForeColor = CGV_BLACK,
                Location = new Point(22, 45),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { lblTitle, valueLabel });
            return card;
        }

        #endregion

        #region DataGridView Component
        
        /// <summary>
        /// Creates a modern styled DataGridView with consistent CGV branding
        /// </summary>
        public static DataGridView CreateModernDataGrid()
        {
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CGV_WHITE,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 50 },
                GridColor = GRID_COLOR,
                EnableHeadersVisualStyles = false
            };

            // Header styling
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = HEADER_BG,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI Semibold", 10),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            dgv.ColumnHeadersHeight = 50;

            // Cell styling
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = FONT_NORMAL,
                ForeColor = CGV_TEXT_COLOR,
                SelectionBackColor = Color.FromArgb(255, 235, 238),
                SelectionForeColor = CGV_RED,
                Padding = new Padding(10, 0, 0, 0)
            };

            return dgv;
        }

        #endregion

        #region Button Component
        
        /// <summary>
        /// Creates a styled button with consistent CGV design
        /// </summary>
        public static Button CreateStyledButton(string text, Color backColor, bool isPrimary = false)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = CGV_WHITE,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 40),
                Cursor = Cursors.Hand
            };
            
            btn.FlatAppearance.BorderSize = 0;
            btn.BorderRadius(8);

            // Hover effect
            Color hoverColor = ControlPaint.Dark(backColor, 0.1f);
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            return btn;
        }

        #endregion

        #region Panel Component
        
        /// <summary>
        /// Creates a rounded panel with consistent styling
        /// </summary>
        public static Panel CreateRoundedPanel(int radius = 12)
        {
            Panel panel = new Panel
            {
                BackColor = CGV_WHITE
            };
            panel.BorderRadius(radius);
            return panel;
        }

        #endregion

        #region Empty State Component
        
        /// <summary>
        /// Creates an empty state UI for when no data is available
        /// </summary>
        public static Panel CreateEmptyState(string icon, string message, string actionText = null, EventHandler actionClick = null)
        {
            Panel emptyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CGV_WHITE
            };

            // Icon
            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 48),
                ForeColor = Color.FromArgb(200, 200, 205),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Message
            Label lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.Gray,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Layout - center both labels
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = actionText != null ? 3 : 2,
                ColumnCount = 1
            };
            
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            if (actionText != null)
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Center icon
            Panel iconContainer = new Panel { Dock = DockStyle.Fill };
            lblIcon.Location = new Point((iconContainer.Width - lblIcon.Width) / 2, (iconContainer.Height - lblIcon.Height) / 2);
            lblIcon.Anchor = AnchorStyles.None;
            iconContainer.Controls.Add(lblIcon);

            // Center message
            Panel msgContainer = new Panel { Dock = DockStyle.Fill, Height = 40 };
            lblMessage.Location = new Point((msgContainer.Width - lblMessage.Width) / 2, 10);
            lblMessage.Anchor = AnchorStyles.None;
            msgContainer.Controls.Add(lblMessage);

            layout.Controls.Add(iconContainer, 0, 0);
            layout.Controls.Add(msgContainer, 0, 1);

            // Optional action button
            if (actionText != null && actionClick != null)
            {
                Button btnAction = CreateStyledButton(actionText, CGV_RED, true);
                btnAction.Size = new Size(180, 45);
                btnAction.Anchor = AnchorStyles.None;
                btnAction.Click += actionClick;
                
                Panel btnContainer = new Panel { Dock = DockStyle.Fill, Height = 60 };
                btnAction.Location = new Point((btnContainer.Width - btnAction.Width) / 2, 10);
                btnContainer.Controls.Add(btnAction);
                
                layout.Controls.Add(btnContainer, 0, 2);
            }

            emptyPanel.Controls.Add(layout);
            return emptyPanel;
        }

        #endregion
    }
}
