using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using QuanLiChuoiRapPhim.Services;

namespace QuanLiChuoiRapPhim.GUI
{
    public class UC_TicketSales : UserControl
    {
        // Modern Color Palette
        private readonly Color _primary = Color.FromArgb(226, 26, 60);       // CGV Red
        private readonly Color _primaryDark = Color.FromArgb(180, 20, 50);   // Darker Red
        private readonly Color _dark = Color.FromArgb(25, 25, 30);           // Almost Black
        private readonly Color _surface = Color.FromArgb(255, 255, 255);     // White
        private readonly Color _surfaceAlt = Color.FromArgb(248, 249, 252);  // Light Gray
        private readonly Color _border = Color.FromArgb(230, 232, 240);      // Border Gray
        private readonly Color _textPrimary = Color.FromArgb(30, 30, 35);    // Text Dark
        private readonly Color _textSecondary = Color.FromArgb(120, 125, 140);// Text Gray
        private readonly Color _success = Color.FromArgb(34, 197, 94);       // Green
        private readonly Color _gold = Color.FromArgb(255, 193, 7);          // VIP Gold
        private readonly Color _accent = Color.FromArgb(99, 102, 241);       // Indigo

        // Data
        private PhimBLL _phimBLL;
        private DataTable _dtPhim;

        // State
        private int _currentStep = 1;
        private int _selectedMovieId = -1;
        private string _selectedMovieName = "";
        private int _selectedShowtimeId = -1;
        private string _selectedShowtimeInfo = "";
        private string _selectedRoom = "";
        private List<string> _selectedSeats = new List<string>();
        private Dictionary<int, int> _selectedCombos = new Dictionary<int, int>();
        private decimal _ticketPrice = 90000;
        private decimal _totalAmount = 0;
        private string _customerPhone = "";
        private string _customerName = "";
        private int _customerPoints = 0;
        private bool _usePoints = false;

        // UI
        private Panel _mainContent;
        private Panel _summaryPanel;
        private Panel _navPanel;
        private Panel[] _stepPanels;
        private Label[] _stepCircles;
        private Panel[] _stepConnectors;

        public UC_TicketSales()
        {
            _phimBLL = new PhimBLL();
            _stepPanels = new Panel[6];
            _stepCircles = new Label[6];
            _stepConnectors = new Panel[5];
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Fill;
            this.BackColor = _surfaceAlt;
            this.Padding = new Padding(16); // Increased outer padding

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F)); // Adjusted ratio
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));

            // Left Panel Container
            Panel leftPanel = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.Transparent, 
                Padding = new Padding(0, 0, 10, 0) // Spacing between left and right panels
            };
            
            // We need to add controls in reverse order of Docking to get the correct visual order (Top-down)
            // or use specific container panels for proper stacking.
            
            // 1. Navigation (Bottom)
            CreateNavigation(leftPanel);

            // 2. Header and Steps (Top)
            // Create a dedicated top container to hold Header and Steps
            Panel topContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140, // Fixed height for header + steps
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 20)
            };

            // Add Header and Steps to topContainer
            // Note: In WinForms, the last added Dock.Top control stays at the top if we add them directly.
            // But we want Header -> Steps.
            
            Panel stepsPanel = new Panel { Dock = DockStyle.Fill, BackColor = _surface }; // Steps take remaining space in topContainer
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = _dark };
            
            CreateStepProgress(stepsPanel);
            CreateHeader(headerPanel);
            
            topContainer.Controls.Add(stepsPanel);
            topContainer.Controls.Add(headerPanel); // Add Header last so it docks to Top first? 
                                                    // No, Dock=Top: The last control added to the Controls collection is at the bottom of the z-order, 
                                                    // so it is pushed away from the edge by controls with higher z-order (added earlier).
                                                    // Let's stick to adding Header first, then Steps, but fix z-order.
            
            // Actually, simpler approach:
            // topContainer.Controls.Add(headerPanel); (Index 0)
            // topContainer.Controls.Add(stepsPanel); (Index 0) -> Steps pushes Header down?
            // Let's use BringToFront() to be sure.
            
            // Proper Construction:
            leftPanel.Controls.Add(topContainer); 
            
            // 3. Main Content (Fill)
            CreateMainContent(leftPanel); // This will fill the space between TopContainer and NavPanel
            
            // Re-ordering leftPanel controls to ensure Docking works:
            // 1. Nav (Bottom) - Add first? No.
            // Correct Order for Docking:
            // Add(Nav) -> Docks Bottom
            // Add(TopContainer) -> Docks Top
            // Add(MainContent) -> Docks Fill
            
            leftPanel.Controls.Clear();
            leftPanel.Controls.Add(_navPanel); // Bottom
            leftPanel.Controls.Add(topContainer); // Top
            leftPanel.Controls.Add(_mainContent); // Fill
            _mainContent.BringToFront(); // Ensure it fills the middle
            
            // Fix TopContainer internal layout
            topContainer.Controls.Clear();
            topContainer.Controls.Add(stepsPanel); // Fill (bottom part of top container)
            topContainer.Controls.Add(headerPanel); // Top
            headerPanel.BringToFront(); // Ensure Header is at very top

            AddShadow(leftPanel); // Add shadow logic back if needed, but might complicate transparent panels
            
            // Right Panel (Summary)
            CreateSummaryPanel();
            
            // Add to Main Layout
            layout.Controls.Add(leftPanel, 0, 0);
            layout.Controls.Add(_summaryPanel, 1, 0);

            this.Controls.Add(layout);
            this.ResumeLayout();

            this.Load += (s, e) => { ShowStep(1); LoadMovies(); };
        }

        #region UI Helpers

        private void AddShadow(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(15, 0, 0, 0), 1))
                {
                    e.Graphics.DrawLine(pen, panel.Width - 1, 0, panel.Width - 1, panel.Height);
                }
            };
        }

        private void StyleButton(Button btn, Color bg, Color fg, bool primary = false)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = bg;
            btn.ForeColor = fg;
            btn.FlatAppearance.BorderSize = primary ? 0 : 1;
            btn.FlatAppearance.BorderColor = _border;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9, primary ? FontStyle.Bold : FontStyle.Regular);

            btn.MouseEnter += (s, e) => btn.BackColor = primary ? _primaryDark : Color.FromArgb(245, 246, 250);
            btn.MouseLeave += (s, e) => btn.BackColor = bg;
        }

        private void StyleCard(Panel card, bool hoverable = true)
        {
            card.BackColor = _surface;
            card.Padding = new Padding(10);

            if (hoverable)
            {
                card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(252, 252, 255);
                card.MouseLeave += (s, e) => card.BackColor = _surface;
            }
        }

        private void RoundCorners(Control ctrl, int radius)
        {
            ctrl.Region = new Region(CreateRoundedPath(ctrl.Width, ctrl.Height, radius));
            ctrl.Resize += (s, e) => ctrl.Region = new Region(CreateRoundedPath(ctrl.Width, ctrl.Height, radius));
        }

        private GraphicsPath CreateRoundedPath(int w, int h, int r)
        {
            GraphicsPath path = new GraphicsPath();
            if (w <= 0) w = 1;
            if (h <= 0) h = 1;
            path.AddArc(0, 0, r, r, 180, 90);
            path.AddArc(w - r - 1, 0, r, r, 270, 90);
            path.AddArc(w - r - 1, h - r - 1, r, r, 0, 90);
            path.AddArc(0, h - r - 1, r, r, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        #region Header

        private void CreateHeader(Panel parent)
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = _dark
            };

            Label title = new Label
            {
                Text = "🎟️  BÁN VÉ XEM PHIM",
                Font = new Font("Segoe UI Semibold", 15),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            header.Controls.Add(title);

            Button btnReset = new Button
            {
                Text = "🔄 Làm mới",
                Size = new Size(95, 34),
                Location = new Point(header.Width - 115, 11),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            StyleButton(btnReset, _primary, Color.White, true);
            btnReset.Click += (s, e) => ResetAll();
            header.Controls.Add(btnReset);

            parent.Controls.Add(header);
        }

        #endregion

        #region Step Progress

        private void CreateStepProgress(Panel parent)
        {
            Panel bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = _surface,
                Padding = new Padding(15, 12, 15, 12)
            };

            string[] names = { "Phim", "Suất chiếu", "Ghế", "Combo", "Khách hàng", "Thanh toán" };
            int circleSize = 32;
            int spacing = 100;
            int startX = 25;

            for (int i = 0; i < 6; i++)
            {
                int x = startX + i * spacing;

                // Connector line
                if (i < 5)
                {
                    Panel line = new Panel
                    {
                        Location = new Point(x + circleSize + 5, 26),
                        Size = new Size(spacing - circleSize - 10, 3),
                        BackColor = _border
                    };
                    _stepConnectors[i] = line;
                    bar.Controls.Add(line);
                }

                // Circle
                Label circle = new Label
                {
                    Text = (i + 1).ToString(),
                    Size = new Size(circleSize, circleSize),
                    Location = new Point(x, 10),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI Semibold", 11),
                    BackColor = i == 0 ? _primary : _surfaceAlt,
                    ForeColor = i == 0 ? Color.White : _textSecondary
                };
                RoundCorners(circle, circleSize);
                _stepCircles[i] = circle;
                bar.Controls.Add(circle);

                // Label
                Label lbl = new Label
                {
                    Text = names[i],
                    Location = new Point(x - 10, 45),
                    Size = new Size(circleSize + 20, 18),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8),
                    ForeColor = _textSecondary
                };
                bar.Controls.Add(lbl);
            }

            parent.Controls.Add(bar);
        }

        #endregion

        #region Navigation

        private void CreateNavigation(Panel parent)
        {
            _navPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = _surface,
                Padding = new Padding(20, 12, 20, 12)
            };

            // Top border
            Panel topLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = _border };
            _navPanel.Controls.Add(topLine);

            Button btnBack = new Button
            {
                Name = "btnBack",
                Text = "← Quay lại",
                Size = new Size(110, 38),
                Location = new Point(20, 14),
                Visible = false
            };
            StyleButton(btnBack, _surface, _textPrimary);
            btnBack.Click += (s, e) => { if (_currentStep > 1) ShowStep(_currentStep - 1); };
            _navPanel.Controls.Add(btnBack);

            Button btnNext = new Button
            {
                Name = "btnNext",
                Text = "Tiếp tục →",
                Size = new Size(130, 40),
                Location = new Point(_navPanel.Width - 150, 13),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            StyleButton(btnNext, _primary, Color.White, true);
            btnNext.Click += BtnNext_Click;
            _navPanel.Controls.Add(btnNext);

            parent.Controls.Add(_navPanel);
        }

        #endregion

        #region Main Content

        private void CreateMainContent(Panel parent)
        {
            _mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _surface,
                Padding = new Padding(20, 15, 20, 15)
            };

            for (int i = 0; i < 6; i++)
            {
                _stepPanels[i] = new Panel
                {
                    Dock = DockStyle.Fill,
                    Visible = false,
                    AutoScroll = true,
                    BackColor = _surface
                };
                _mainContent.Controls.Add(_stepPanels[i]);
            }

            CreateStep1_Movie();
            CreateStep2_Showtime();
            CreateStep3_Seats();
            CreateStep4_Combo();
            CreateStep5_Customer();
            CreateStep6_Payment();

            parent.Controls.Add(_mainContent);
        }

        #endregion

        #region Step 1: Movie

        private void CreateStep1_Movie()
        {
            Panel p = _stepPanels[0];

            Label lbl = new Label
            {
                Text = "🎬  Chọn phim bạn muốn xem",
                Font = new Font("Segoe UI Semibold", 13),
                ForeColor = _textPrimary,
                Location = new Point(5, 5),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Name = "moviesFlow",
                Location = new Point(5, 45),
                Size = new Size(680, 380),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };
            p.Controls.Add(flow);
        }

        private void LoadMovies()
        {
            try
            {
                _dtPhim = _phimBLL.LayTatCaPhim();
                FlowLayoutPanel flow = _stepPanels[0].Controls.Find("moviesFlow", true).FirstOrDefault() as FlowLayoutPanel;
                if (flow == null) return;

                flow.Controls.Clear();

                if (_dtPhim == null || _dtPhim.Rows.Count == 0)
                {
                    flow.Controls.Add(new Label { Text = "Không có phim nào", Padding = new Padding(20), ForeColor = _textSecondary });
                    return;
                }

                foreach (DataRow row in _dtPhim.Rows)
                {
                    int id = Convert.ToInt32(row["MaPhim"]);
                    string name = row["TenPhim"]?.ToString() ?? "";
                    int duration = row["ThoiLuong"] != DBNull.Value ? Convert.ToInt32(row["ThoiLuong"]) : 0;
                    string img = row["HinhAnh"]?.ToString() ?? "";

                    Panel card = new Panel
                    {
                        Size = new Size(140, 215),
                        Margin = new Padding(8),
                        BackColor = _surface,
                        Cursor = Cursors.Hand,
                        Tag = new { Id = id, Name = name }
                    };
                    RoundCorners(card, 12);

                    // Add border via Paint
                    card.Paint += (s, e) =>
                    {
                        using (Pen pen = new Pen(_border, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                    };

                    // Poster
                    PictureBox pb = new PictureBox
                    {
                        Size = new Size(124, 150),
                        Location = new Point(8, 8),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.FromArgb(45, 45, 50),
                        Cursor = Cursors.Hand
                    };
                    RoundCorners(pb, 8);

                    try
                    {
                        string path = Path.Combine(Application.StartupPath, "uploads", "movies", Path.GetFileName(img));
                        if (File.Exists(path)) pb.Image = Image.FromFile(path);
                    }
                    catch { }

                    card.Controls.Add(pb);

                    Label lblName = new Label
                    {
                        Text = name.Length > 16 ? name.Substring(0, 13) + "..." : name,
                        Font = new Font("Segoe UI Semibold", 9),
                        ForeColor = _textPrimary,
                        Location = new Point(8, 165),
                        Size = new Size(124, 20),
                        Cursor = Cursors.Hand
                    };
                    card.Controls.Add(lblName);

                    Label lblDur = new Label
                    {
                        Text = $"🕐 {duration} phút",
                        Font = new Font("Segoe UI", 8),
                        ForeColor = _textSecondary,
                        Location = new Point(8, 187),
                        Size = new Size(124, 18),
                        Cursor = Cursors.Hand
                    };
                    card.Controls.Add(lblDur);

                    // Click handlers
                    EventHandler click = (s, e) => SelectMovie(id, name);
                    card.Click += click;
                    pb.Click += click;
                    lblName.Click += click;
                    lblDur.Click += click;

                    // Hover
                    EventHandler enter = (s, e) => card.BackColor = Color.FromArgb(255, 245, 248);
                    EventHandler leave = (s, e) => card.BackColor = id == _selectedMovieId ? Color.FromArgb(255, 230, 235) : _surface;
                    card.MouseEnter += enter;
                    pb.MouseEnter += enter;
                    lblName.MouseEnter += enter;
                    card.MouseLeave += leave;
                    pb.MouseLeave += leave;
                    lblName.MouseLeave += leave;

                    flow.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void SelectMovie(int id, string name)
        {
            _selectedMovieId = id;
            _selectedMovieName = name;
            _selectedShowtimeId = -1;

            FlowLayoutPanel flow = _stepPanels[0].Controls.Find("moviesFlow", true).FirstOrDefault() as FlowLayoutPanel;
            if (flow != null)
            {
                foreach (Control c in flow.Controls)
                {
                    if (c is Panel p && p.Tag != null)
                    {
                        dynamic tag = p.Tag;
                        p.BackColor = tag.Id == id ? Color.FromArgb(255, 230, 235) : _surface;
                    }
                }
            }
            UpdateSummary();
        }

        #endregion

        #region Step 2: Showtime

        private void CreateStep2_Showtime()
        {
            Panel p = _stepPanels[1];

            Label lbl = new Label
            {
                Text = "⏰  Chọn suất chiếu",
                Font = new Font("Segoe UI Semibold", 13),
                ForeColor = _textPrimary,
                Location = new Point(5, 5),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Name = "showtimesFlow",
                Location = new Point(5, 45),
                Size = new Size(680, 380),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            p.Controls.Add(flow);
        }

        private void LoadShowtimes()
        {
            FlowLayoutPanel flow = _stepPanels[1].Controls.Find("showtimesFlow", true).FirstOrDefault() as FlowLayoutPanel;
            if (flow == null) return;

            flow.Controls.Clear();

            string[] times = { "09:00", "11:30", "14:00", "16:30", "19:00", "21:30" };
            string[] rooms = { "Phòng 1", "Phòng 2", "Phòng 3" };
            Random rand = new Random(_selectedMovieId);

            for (int d = 0; d < 3; d++)
            {
                DateTime date = DateTime.Today.AddDays(d);
                string dateText = d == 0 ? "📅 Hôm nay" : (d == 1 ? "📅 Ngày mai" : $"📅 {date:dd/MM}");

                Label lblDate = new Label
                {
                    Text = dateText,
                    Font = new Font("Segoe UI Semibold", 10),
                    ForeColor = _textPrimary,
                    Size = new Size(660, 32),
                    Margin = new Padding(5, 15, 5, 8)
                };
                flow.Controls.Add(lblDate);

                foreach (string time in times)
                {
                    if (d == 0 && DateTime.Today.Add(TimeSpan.Parse(time)) < DateTime.Now.AddMinutes(30))
                        continue;

                    string room = rooms[rand.Next(rooms.Length)];
                    int stId = d * 100 + Array.IndexOf(times, time);

                    Button btn = new Button
                    {
                        Text = $"{time}\n{room}",
                        Size = new Size(95, 52),
                        Margin = new Padding(6),
                        Tag = new { Id = stId, Time = time, Room = room, Date = date }
                    };
                    StyleButton(btn, _surface, _textPrimary);
                    RoundCorners(btn, 8);
                    btn.Click += ShowtimeBtn_Click;
                    flow.Controls.Add(btn);
                }
            }
        }

        private void ShowtimeBtn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null) return;

            dynamic tag = btn.Tag;
            _selectedShowtimeId = tag.Id;
            _selectedShowtimeInfo = $"{((DateTime)tag.Date):dd/MM} - {tag.Time}";
            _selectedRoom = tag.Room;

            FlowLayoutPanel flow = _stepPanels[1].Controls.Find("showtimesFlow", true).FirstOrDefault() as FlowLayoutPanel;
            if (flow != null)
            {
                foreach (Control c in flow.Controls)
                {
                    if (c is Button b)
                    {
                        b.BackColor = b == btn ? _primary : _surface;
                        b.ForeColor = b == btn ? Color.White : _textPrimary;
                    }
                }
            }
            UpdateSummary();
        }

        #endregion

        #region Step 3: Seats

        private void CreateStep3_Seats()
        {
            Panel p = _stepPanels[2];

            Label lbl = new Label
            {
                Text = "🪑  Chọn ghế ngồi",
                Font = new Font("Segoe UI Semibold", 13),
                ForeColor = _textPrimary,
                Location = new Point(5, 5),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            // Screen
            Panel screen = new Panel
            {
                Size = new Size(420, 35),
                Location = new Point(80, 45),
                BackColor = _dark
            };
            RoundCorners(screen, 6);

            Label screenLbl = new Label
            {
                Text = "📺  MÀN HÌNH",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10)
            };
            screen.Controls.Add(screenLbl);
            p.Controls.Add(screen);

            Panel seatArea = new Panel
            {
                Name = "seatArea",
                Location = new Point(40, 95),
                Size = new Size(520, 290)
            };
            p.Controls.Add(seatArea);

            // Legend
            FlowLayoutPanel legend = new FlowLayoutPanel
            {
                Location = new Point(40, 395),
                Size = new Size(520, 35),
                FlowDirection = FlowDirection.LeftToRight
            };
            legend.Controls.Add(CreateLegend("Trống", _surface, _border));
            legend.Controls.Add(CreateLegend("Đã chọn", _primary, _primary));
            legend.Controls.Add(CreateLegend("Đã bán", Color.FromArgb(200, 200, 205), Color.Gray));
            legend.Controls.Add(CreateLegend("VIP +30k", Color.FromArgb(255, 250, 235), _gold));
            p.Controls.Add(legend);
        }

        private Panel CreateLegend(string text, Color bg, Color border)
        {
            Panel item = new Panel { Size = new Size(115, 28), Margin = new Padding(6, 2, 6, 2) };
            Panel box = new Panel { Size = new Size(18, 18), Location = new Point(0, 5), BackColor = bg };
            box.Paint += (s, e) => { using (Pen pen = new Pen(border)) e.Graphics.DrawRectangle(pen, 0, 0, 17, 17); };
            item.Controls.Add(box);
            item.Controls.Add(new Label { Text = text, Location = new Point(24, 6), AutoSize = true, Font = new Font("Segoe UI", 8), ForeColor = _textSecondary });
            return item;
        }

        private void LoadSeats()
        {
            Panel seatArea = _stepPanels[2].Controls.Find("seatArea", true).FirstOrDefault() as Panel;
            if (seatArea == null) return;

            seatArea.Controls.Clear();
            _selectedSeats.Clear();

            Random rand = new Random(_selectedShowtimeId > 0 ? _selectedShowtimeId : 1);

            for (int r = 0; r < 8; r++)
            {
                char row = (char)('A' + r);

                Label lblRow = new Label
                {
                    Text = row.ToString(),
                    Size = new Size(25, 32),
                    Location = new Point(0, r * 36),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI Semibold", 9),
                    ForeColor = _textSecondary
                };
                seatArea.Controls.Add(lblRow);

                for (int c = 0; c < 10; c++)
                {
                    string seatId = $"{row}{c + 1}";
                    bool isVip = r >= 6;
                    bool isTaken = rand.Next(100) < 18;

                    Button btn = new Button
                    {
                        Text = (c + 1).ToString(),
                        Size = new Size(36, 30),
                        Location = new Point(30 + c * 42, r * 36),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8),
                        Cursor = isTaken ? Cursors.No : Cursors.Hand,
                        Tag = new { SeatId = seatId, IsVip = isVip, IsTaken = isTaken },
                        Enabled = !isTaken
                    };
                    btn.FlatAppearance.BorderSize = 1;
                    RoundCorners(btn, 6);

                    if (isTaken)
                    {
                        btn.BackColor = Color.FromArgb(200, 200, 205);
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = Color.Gray;
                    }
                    else if (isVip)
                    {
                        btn.BackColor = Color.FromArgb(255, 250, 235);
                        btn.FlatAppearance.BorderColor = _gold;
                    }
                    else
                    {
                        btn.BackColor = _surface;
                        btn.FlatAppearance.BorderColor = _border;
                    }

                    btn.Click += SeatBtn_Click;
                    seatArea.Controls.Add(btn);
                }
            }
            UpdateSummary();
        }

        private void SeatBtn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null) return;

            dynamic tag = btn.Tag;
            string seatId = tag.SeatId;
            bool isVip = tag.IsVip;

            if (_selectedSeats.Contains(seatId))
            {
                _selectedSeats.Remove(seatId);
                btn.BackColor = isVip ? Color.FromArgb(255, 250, 235) : _surface;
                btn.ForeColor = _textPrimary;
            }
            else
            {
                if (_selectedSeats.Count >= 8)
                {
                    MessageBox.Show("Tối đa 8 ghế!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _selectedSeats.Add(seatId);
                btn.BackColor = _primary;
                btn.ForeColor = Color.White;
            }
            UpdateSummary();
        }

        #endregion

        #region Step 4: Combo

        private void CreateStep4_Combo()
        {
            Panel p = _stepPanels[3];

            Label lbl = new Label
            {
                Text = "🍿  Thêm combo bắp nước (tùy chọn)",
                Font = new Font("Segoe UI Semibold", 13),
                ForeColor = _textPrimary,
                Location = new Point(5, 5),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Location = new Point(5, 45),
                Size = new Size(680, 380),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            string[] icons = { "🍿", "🍿🥤", "👨‍👩‍👧‍👦", "🌮", "🌭", "🥤" };
            string[] names = { "Combo 1", "Combo 2", "Combo Gia đình", "Snack Box", "Hot Dog", "Nước ngọt" };
            string[] descs = { "Bắp + Nước", "Bắp lớn + 2 Nước", "2 Bắp + 4 Nước", "Nachos + Nước", "Hot dog + Nước", "Coca/Pepsi" };
            int[] prices = { 85000, 120000, 199000, 75000, 69000, 35000 };

            for (int i = 0; i < 6; i++)
            {
                flow.Controls.Add(CreateComboCard(i + 1, icons[i], names[i], descs[i], prices[i]));
            }

            p.Controls.Add(flow);
        }

        private Panel CreateComboCard(int id, string icon, string name, string desc, int price)
        {
            Panel card = new Panel
            {
                Size = new Size(210, 115),
                Margin = new Padding(8),
                BackColor = _surface
            };
            RoundCorners(card, 10);
            card.Paint += (s, e) => { using (Pen pen = new Pen(_border)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };

            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 20),
                Location = new Point(12, 10),
                AutoSize = true
            };
            card.Controls.Add(lblIcon);

            Label lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = _textPrimary,
                Location = new Point(60, 12),
                AutoSize = true
            };
            card.Controls.Add(lblName);

            Label lblDesc = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 8),
                ForeColor = _textSecondary,
                Location = new Point(60, 32),
                AutoSize = true
            };
            card.Controls.Add(lblDesc);

            Label lblPrice = new Label
            {
                Text = $"{price:N0}đ",
                Font = new Font("Segoe UI Semibold", 11),
                ForeColor = _primary,
                Location = new Point(12, 55),
                AutoSize = true
            };
            card.Controls.Add(lblPrice);

            Button btnM = new Button { Text = "−", Size = new Size(32, 28), Location = new Point(12, 80) };
            StyleButton(btnM, _surfaceAlt, _textPrimary);
            RoundCorners(btnM, 6);

            Label lblQ = new Label
            {
                Text = "0",
                Size = new Size(35, 28),
                Location = new Point(48, 82),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 10)
            };

            Button btnP = new Button { Text = "+", Size = new Size(32, 28), Location = new Point(87, 80) };
            StyleButton(btnP, _primary, Color.White, true);
            RoundCorners(btnP, 6);

            btnM.Click += (s, e) =>
            {
                int q = int.Parse(lblQ.Text);
                if (q > 0) { q--; lblQ.Text = q.ToString(); if (q == 0) _selectedCombos.Remove(id); else _selectedCombos[id] = q; UpdateSummary(); }
            };
            btnP.Click += (s, e) =>
            {
                int q = int.Parse(lblQ.Text);
                if (q < 10) { q++; lblQ.Text = q.ToString(); _selectedCombos[id] = q; UpdateSummary(); }
            };

            card.Controls.AddRange(new Control[] { btnM, lblQ, btnP });
            return card;
        }

        #endregion

        #region Step 5: Customer

        private void CreateStep5_Customer()
        {
            Panel p = _stepPanels[4];

            Label lbl = new Label
            {
                Text = "👤  Thông tin khách hàng",
                Font = new Font("Segoe UI Semibold", 13),
                ForeColor = _textPrimary,
                Location = new Point(5, 5),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            // Phone
            p.Controls.Add(new Label { Text = "Số điện thoại", Location = new Point(10, 55), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = _textSecondary });
            TextBox txtPhone = new TextBox
            {
                Name = "txtPhone",
                Location = new Point(10, 78),
                Size = new Size(180, 32),
                Font = new Font("Segoe UI", 11)
            };
            txtPhone.TextChanged += (s, e) => _customerPhone = txtPhone.Text;
            p.Controls.Add(txtPhone);

            Button btnFind = new Button
            {
                Text = "🔍 Tìm KH",
                Location = new Point(200, 77),
                Size = new Size(90, 32)
            };
            StyleButton(btnFind, _dark, Color.White, true);
            RoundCorners(btnFind, 6);
            btnFind.Click += BtnFindCustomer_Click;
            p.Controls.Add(btnFind);

            // Name
            p.Controls.Add(new Label { Text = "Họ tên", Location = new Point(10, 125), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = _textSecondary });
            TextBox txtName = new TextBox
            {
                Name = "txtName",
                Location = new Point(10, 148),
                Size = new Size(280, 32),
                Font = new Font("Segoe UI", 11)
            };
            txtName.TextChanged += (s, e) => _customerName = txtName.Text;
            p.Controls.Add(txtName);

            // Points info box
            Panel pointsBox = new Panel
            {
                Location = new Point(10, 200),
                Size = new Size(280, 90),
                BackColor = _surfaceAlt
            };
            RoundCorners(pointsBox, 10);

            Label lblPoints = new Label
            {
                Name = "lblPoints",
                Text = "⭐ Điểm tích lũy: 0",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11),
                ForeColor = _textPrimary
            };
            pointsBox.Controls.Add(lblPoints);

            CheckBox chkUse = new CheckBox
            {
                Name = "chkUsePoints",
                Text = "Sử dụng điểm (10đ = 1,000đ giảm)",
                Location = new Point(15, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            chkUse.CheckedChanged += (s, e) => { _usePoints = chkUse.Checked; UpdateSummary(); };
            pointsBox.Controls.Add(chkUse);

            p.Controls.Add(pointsBox);

            Label lblEarn = new Label
            {
                Name = "lblEarnPoints",
                Text = "💰 Điểm nhận được từ đơn này: 0",
                Location = new Point(10, 305),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = _success
            };
            p.Controls.Add(lblEarn);
        }

        private void BtnFindCustomer_Click(object sender, EventArgs e)
        {
            TextBox txtPhone = _stepPanels[4].Controls.Find("txtPhone", true).FirstOrDefault() as TextBox;
            TextBox txtName = _stepPanels[4].Controls.Find("txtName", true).FirstOrDefault() as TextBox;
            Label lblPoints = _stepPanels[4].Controls.Find("lblPoints", true).FirstOrDefault() as Label;

            if (txtPhone == null || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo");
                return;
            }

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT HoTen, DiemTichLuy FROM KhachHang WHERE SoDienThoai = @P", conn))
                    {
                        cmd.Parameters.AddWithValue("@P", txtPhone.Text.Trim());
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                _customerPhone = txtPhone.Text.Trim();
                                _customerName = r["HoTen"]?.ToString() ?? "";
                                _customerPoints = r["DiemTichLuy"] != DBNull.Value ? Convert.ToInt32(r["DiemTichLuy"]) : 0;
                                if (txtName != null) txtName.Text = _customerName;
                                if (lblPoints != null) lblPoints.Text = $"⭐ Điểm tích lũy: {_customerPoints:N0}";
                                MessageBox.Show($"✓ Tìm thấy: {_customerName}\n⭐ Điểm: {_customerPoints:N0}", "Khách hàng thành viên", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                _customerPhone = txtPhone.Text.Trim();
                                _customerPoints = 0;
                                if (lblPoints != null) lblPoints.Text = "⭐ Điểm: 0 (Khách mới)";
                                MessageBox.Show("📝 Khách hàng mới - Sẽ tạo tài khoản khi thanh toán", "Thông báo");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }

            UpdateSummary();
        }

        #endregion

        #region Step 6: Payment

        private void CreateStep6_Payment()
        {
            Panel p = _stepPanels[5];

            Label lbl = new Label
            {
                Text = "💳  Thanh toán",
                Font = new Font("Segoe UI Semibold", 13),
                ForeColor = _textPrimary,
                Location = new Point(5, 5),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            // Invoice box
            Panel invBox = new Panel
            {
                Location = new Point(10, 45),
                Size = new Size(300, 180),
                BackColor = _surfaceAlt
            };
            RoundCorners(invBox, 10);

            Label lblInvTitle = new Label
            {
                Text = "📋 Chi tiết hóa đơn",
                Location = new Point(12, 10),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = _textPrimary
            };
            invBox.Controls.Add(lblInvTitle);

            Label lblInv = new Label
            {
                Name = "lblInvoice",
                Location = new Point(12, 35),
                Size = new Size(276, 135),
                Font = new Font("Consolas", 9),
                ForeColor = _textSecondary
            };
            invBox.Controls.Add(lblInv);
            p.Controls.Add(invBox);

            // Cash payment
            Panel cashBox = new Panel
            {
                Location = new Point(320, 45),
                Size = new Size(290, 145),
                BackColor = Color.FromArgb(240, 253, 244)
            };
            RoundCorners(cashBox, 10);

            cashBox.Controls.Add(new Label { Text = "💵 Thanh toán tiền mặt", Location = new Point(12, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 10), ForeColor = _textPrimary });

            cashBox.Controls.Add(new Label { Text = "Tổng tiền:", Location = new Point(12, 42), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = _textSecondary });
            Label lblTotal = new Label { Name = "lblPayTotal", Text = "0đ", Location = new Point(100, 38), AutoSize = true, Font = new Font("Segoe UI Semibold", 14), ForeColor = _primary };
            cashBox.Controls.Add(lblTotal);

            cashBox.Controls.Add(new Label { Text = "Khách đưa:", Location = new Point(12, 75), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = _textSecondary });
            TextBox txtCash = new TextBox { Name = "txtCash", Location = new Point(100, 72), Size = new Size(110, 28), Font = new Font("Segoe UI", 11), TextAlign = HorizontalAlignment.Right };
            txtCash.TextChanged += TxtCash_Changed;
            cashBox.Controls.Add(txtCash);

            cashBox.Controls.Add(new Label { Text = "Tiền thừa:", Location = new Point(12, 108), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = _textSecondary });
            Label lblChange = new Label { Name = "lblChange", Text = "0đ", Location = new Point(100, 104), AutoSize = true, Font = new Font("Segoe UI Semibold", 12), ForeColor = _success };
            cashBox.Controls.Add(lblChange);

            p.Controls.Add(cashBox);

            // QR payment
            Panel qrBox = new Panel
            {
                Location = new Point(320, 200),
                Size = new Size(290, 185),
                BackColor = Color.FromArgb(239, 246, 255)
            };
            RoundCorners(qrBox, 10);

            qrBox.Controls.Add(new Label { Text = "📱 QR Chuyển khoản", Location = new Point(12, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 10), ForeColor = _textPrimary });
            qrBox.Controls.Add(new Label { Text = "MB Bank: 0865691072", Location = new Point(12, 35), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = _textSecondary });

            PictureBox pbQR = new PictureBox { Name = "pbQR", Location = new Point(85, 55), Size = new Size(120, 120), BackColor = Color.White, SizeMode = PictureBoxSizeMode.Zoom };
            RoundCorners(pbQR, 8);
            qrBox.Controls.Add(pbQR);

            p.Controls.Add(qrBox);

            // Complete button
            Button btnComplete = new Button
            {
                Text = "✅  HOÀN TẤT THANH TOÁN",
                Location = new Point(10, 395),
                Size = new Size(300, 48)
            };
            StyleButton(btnComplete, _success, Color.White, true);
            btnComplete.Font = new Font("Segoe UI Semibold", 12);
            RoundCorners(btnComplete, 10);
            btnComplete.Click += BtnComplete_Click;
            p.Controls.Add(btnComplete);
        }

        private void TxtCash_Changed(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            Label lblChange = _stepPanels[5].Controls.Find("lblChange", true).FirstOrDefault() as Label;
            if (lblChange == null) return;

            if (decimal.TryParse(txt.Text.Replace(",", "").Replace(".", ""), out decimal cash))
            {
                decimal change = cash - _totalAmount;
                lblChange.Text = change >= 0 ? $"{change:N0}đ" : $"Thiếu {Math.Abs(change):N0}đ";
                lblChange.ForeColor = change >= 0 ? _success : _primary;
            }
        }

        private void UpdatePaymentView()
        {
            Label lblInv = _stepPanels[5].Controls.Find("lblInvoice", true).FirstOrDefault() as Label;
            Label lblTotal = _stepPanels[5].Controls.Find("lblPayTotal", true).FirstOrDefault() as Label;
            PictureBox pbQR = _stepPanels[5].Controls.Find("pbQR", true).FirstOrDefault() as PictureBox;

            if (lblInv != null)
            {
                lblInv.Text = $"🎬 {_selectedMovieName}\n" +
                             $"⏰ {_selectedShowtimeInfo}\n" +
                             $"🚪 {_selectedRoom}\n" +
                             $"🪑 {string.Join(", ", _selectedSeats)}\n" +
                             $"────────────────\n" +
                             $"💰 TỔNG: {_totalAmount:N0}đ";
            }

            if (lblTotal != null) lblTotal.Text = $"{_totalAmount:N0}đ";

            if (pbQR != null && _totalAmount > 0)
            {
                try
                {
                    string url = $"https://img.vietqr.io/image/970422-0865691072-compact2.png?amount={(int)_totalAmount}&addInfo=CGV";
                    using (var wc = new System.Net.WebClient())
                    {
                        byte[] data = wc.DownloadData(url);
                        using (var ms = new MemoryStream(data)) { pbQR.Image = Image.FromStream(ms); }
                    }
                }
                catch { pbQR.BackColor = Color.LightGray; }
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (_selectedSeats.Count == 0)
            {
                MessageBox.Show("Chưa chọn ghế!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_customerPhone))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại khách hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string msg = $"📋 XÁC NHẬN THANH TOÁN\n\n" +
                        $"🎬 {_selectedMovieName}\n" +
                        $"⏰ {_selectedShowtimeInfo}\n" +
                        $"🪑 {string.Join(", ", _selectedSeats)}\n\n" +
                        $"💰 Tổng tiền: {_totalAmount:N0}đ\n\n" +
                        $"Xác nhận thanh toán?";

            if (MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Find or Create Customer
                            int? maKhachHang = null;
                            int currentPoints = 0;
                            
                            using (var cmdCheck = new System.Data.SqlClient.SqlCommand(
                                "SELECT MaKhachHang, DiemTichLuy FROM KhachHang WHERE SoDienThoai = @Phone", conn, transaction))
                            {
                                cmdCheck.Parameters.AddWithValue("@Phone", _customerPhone.Trim());
                                using (var reader = cmdCheck.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        maKhachHang = Convert.ToInt32(reader["MaKhachHang"]);
                                        currentPoints = reader["DiemTichLuy"] != DBNull.Value ? Convert.ToInt32(reader["DiemTichLuy"]) : 0;
                                    }
                                }
                            }

                            // Create new customer if not exists
                            if (maKhachHang == null)
                            {
                                string customerName = string.IsNullOrWhiteSpace(_customerName) ? "Khách hàng" : _customerName;
                                using (var cmdInsert = new System.Data.SqlClient.SqlCommand(
                                    "INSERT INTO KhachHang (HoTen, SoDienThoai, Email, DiemTichLuy, NgayDangKy) " +
                                    "VALUES (@Name, @Phone, @Email, 0, GETDATE()); SELECT SCOPE_IDENTITY();", conn, transaction))
                                {
                                    cmdInsert.Parameters.AddWithValue("@Name", customerName);
                                    cmdInsert.Parameters.AddWithValue("@Phone", _customerPhone.Trim());
                                    cmdInsert.Parameters.AddWithValue("@Email", DBNull.Value);
                                    maKhachHang = Convert.ToInt32(cmdInsert.ExecuteScalar());
                                }
                            }

                            // 2. Calculate points
                            decimal pointsDiscount = 0;
                            int pointsToDeduct = 0;
                            if (_usePoints && currentPoints > 0)
                            {
                                pointsDiscount = Math.Min(currentPoints * 100, _totalAmount * 0.1m);
                                pointsToDeduct = (int)(pointsDiscount / 100);
                            }

                            decimal finalAmount = _totalAmount - pointsDiscount;
                            int pointsToEarn = (int)(finalAmount / 10000);

                            // 3. Update customer loyalty points
                            int newPoints = currentPoints - pointsToDeduct + pointsToEarn;
                            using (var cmdUpdate = new System.Data.SqlClient.SqlCommand(
                                "UPDATE KhachHang SET DiemTichLuy = @NewPoints WHERE MaKhachHang = @Id", conn, transaction))
                            {
                                cmdUpdate.Parameters.AddWithValue("@NewPoints", newPoints);
                                cmdUpdate.Parameters.AddWithValue("@Id", maKhachHang);
                                cmdUpdate.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            // Show success
                            string successMsg = $"✅ THANH TOÁN THÀNH CÔNG!\n\n" +
                                               $"🎟️ Số vé: {_selectedSeats.Count}\n" +
                                               $"💰 Tổng tiền: {finalAmount:N0}đ\n\n" +
                                               $"📞 Khách hàng: {_customerPhone}\n";
                            
                            if (pointsToDeduct > 0)
                                successMsg += $"⭐ Đã dùng: {pointsToDeduct} điểm (-{pointsDiscount:N0}đ)\n";
                            
                            successMsg += $"⭐ Điểm nhận: +{pointsToEarn}\n" +
                                         $"⭐ Tổng điểm: {newPoints:N0}\n\n" +
                                         $"Cảm ơn quý khách!";

                            MessageBox.Show(successMsg, "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ResetAll();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Lỗi trong quá trình thanh toán: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi thanh toán:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Summary Panel

        private void CreateSummaryPanel()
        {
            _summaryPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _surface,
                Padding = new Padding(15),
                Margin = new Padding(8, 0, 0, 0)
            };

            Label lbl = new Label
            {
                Text = "📋 TÓM TẮT ĐƠN HÀNG",
                Font = new Font("Segoe UI Semibold", 11),
                ForeColor = _textPrimary,
                Dock = DockStyle.Top,
                Height = 35
            };
            _summaryPanel.Controls.Add(lbl);

            Panel content = new Panel
            {
                Name = "summaryContent",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            _summaryPanel.Controls.Add(content);
        }

        private void UpdateSummary()
        {
            Panel content = _summaryPanel.Controls.Find("summaryContent", true).FirstOrDefault() as Panel;
            if (content == null) return;

            content.Controls.Clear();
            int y = 5;

            if (!string.IsNullOrEmpty(_selectedMovieName))
            {
                content.Controls.Add(SummaryItem("🎬 Phim", _selectedMovieName, ref y));
            }

            if (_selectedShowtimeId > 0)
            {
                content.Controls.Add(SummaryItem("⏰ Suất", _selectedShowtimeInfo, ref y));
                content.Controls.Add(SummaryItem("🚪 Phòng", _selectedRoom, ref y));
            }

            decimal seatTotal = 0;
            if (_selectedSeats.Count > 0)
            {
                content.Controls.Add(SummaryItem("🪑 Ghế", string.Join(", ", _selectedSeats), ref y));
                int vipCount = _selectedSeats.Count(s => s.StartsWith("G") || s.StartsWith("H"));
                seatTotal = _selectedSeats.Count * _ticketPrice + vipCount * 30000;
                content.Controls.Add(SummaryItem($"   ({_selectedSeats.Count} vé)", $"{seatTotal:N0}đ", ref y, _primary));
            }

            decimal comboTotal = 0;
            foreach (var c in _selectedCombos)
            {
                if (c.Value > 0)
                {
                    decimal pr = GetComboPrice(c.Key);
                    decimal sub = c.Value * pr;
                    comboTotal += sub;
                    content.Controls.Add(SummaryItem($"🍿 {GetComboName(c.Key)} x{c.Value}", $"{sub:N0}đ", ref y));
                }
            }

            decimal pointsDiscount = 0;
            if (_usePoints && _customerPoints > 0)
            {
                pointsDiscount = Math.Min(_customerPoints * 100, (seatTotal + comboTotal) * 0.1m);
                content.Controls.Add(SummaryItem("⭐ Giảm điểm", $"-{pointsDiscount:N0}đ", ref y, _success));
            }

            y += 10;
            content.Controls.Add(new Panel { Location = new Point(0, y), Size = new Size(200, 2), BackColor = _border });
            y += 15;

            _totalAmount = seatTotal + comboTotal - pointsDiscount;

            Label lblTotalLabel = new Label
            {
                Text = "TỔNG CỘNG",
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = _textSecondary,
                Location = new Point(0, y),
                AutoSize = true
            };
            content.Controls.Add(lblTotalLabel);

            Label lblTotalValue = new Label
            {
                Text = $"{_totalAmount:N0}đ",
                Font = new Font("Segoe UI Bold", 18),
                ForeColor = _primary,
                Location = new Point(0, y + 22),
                AutoSize = true
            };
            content.Controls.Add(lblTotalValue);

            int earnPoints = (int)(_totalAmount / 10000);
            Label lblEarn = _stepPanels[4].Controls.Find("lblEarnPoints", true).FirstOrDefault() as Label;
            if (lblEarn != null) lblEarn.Text = $"💰 Điểm nhận được từ đơn này: {earnPoints}";
        }

        private Label SummaryItem(string label, string value, ref int y, Color? color = null)
        {
            string displayVal = value.Length > 18 ? value.Substring(0, 15) + "..." : value;
            Label lbl = new Label
            {
                Text = $"{label}\n{displayVal}",
                Font = new Font("Segoe UI", 9),
                ForeColor = color ?? _textSecondary,
                Location = new Point(0, y),
                Size = new Size(200, 36)
            };
            y += 38;
            return lbl;
        }

        private string GetComboName(int id) => id switch { 1 => "Combo 1", 2 => "Combo 2", 3 => "Combo GĐ", 4 => "Snack", 5 => "Hot Dog", 6 => "Nước", _ => "Combo" };
        private decimal GetComboPrice(int id) => id switch { 1 => 85000, 2 => 120000, 3 => 199000, 4 => 75000, 5 => 69000, 6 => 35000, _ => 0 };

        #endregion

        #region Navigation

        private void ShowStep(int step)
        {
            _currentStep = step;

            for (int i = 0; i < 6; i++)
            {
                _stepPanels[i].Visible = i == step - 1;

                bool completed = i < step - 1;
                bool active = i == step - 1;

                _stepCircles[i].BackColor = completed ? _success : (active ? _primary : _surfaceAlt);
                _stepCircles[i].ForeColor = completed || active ? Color.White : _textSecondary;
                _stepCircles[i].Text = completed ? "✓" : (i + 1).ToString();

                if (i < 5)
                    _stepConnectors[i].BackColor = i < step - 1 ? _success : _border;
            }

            Button btnBack = _navPanel.Controls.Find("btnBack", false).FirstOrDefault() as Button;
            Button btnNext = _navPanel.Controls.Find("btnNext", false).FirstOrDefault() as Button;

            if (btnBack != null) btnBack.Visible = step > 1;
            if (btnNext != null)
            {
                btnNext.Text = step == 6 ? "" : "Tiếp tục →";
                btnNext.Visible = step < 6;
            }

            if (step == 2) LoadShowtimes();
            if (step == 3) LoadSeats();
            if (step == 6) UpdatePaymentView();

            UpdateSummary();
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (_currentStep == 1 && _selectedMovieId < 0)
            {
                MessageBox.Show("Vui lòng chọn phim!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_currentStep == 2 && _selectedShowtimeId < 0)
            {
                MessageBox.Show("Vui lòng chọn suất chiếu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_currentStep == 3 && _selectedSeats.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ghế!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentStep < 6) ShowStep(_currentStep + 1);
        }

        private void ResetAll()
        {
            _selectedMovieId = -1;
            _selectedMovieName = "";
            _selectedShowtimeId = -1;
            _selectedShowtimeInfo = "";
            _selectedRoom = "";
            _selectedSeats.Clear();
            _selectedCombos.Clear();
            _customerPhone = "";
            _customerName = "";
            _customerPoints = 0;
            _usePoints = false;
            _totalAmount = 0;

            ShowStep(1);
            LoadMovies();
        }

        #endregion
    }
}
