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
    /// <summary>
    /// Complete Ticket Sales Wizard with 4-step flow:
    /// Step 1: Select Movie & Showtime
    /// Step 2: Select Seats
    /// Step 3: Add Snacks/Combos (optional)
    /// Step 4: Payment & Confirmation
    /// </summary>
    public class UC_TicketSales : UserControl
    {
        // CGV Branding Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

        // Data
        private PhimBLL _phimBLL;
        private VeBLL _veBLL;
        private DataTable _dtPhim;
        private DataTable _dtShowtimes;
        private DataTable _dtCombos;

        // Selection state
        private int _currentStep = 1;
        private int _selectedMovieId = -1;
        private string _selectedMovieName = "";
        private int _selectedShowtimeId = -1;
        private string _selectedShowtimeInfo = "";
        private string _selectedRoom = "";
        private List<string> _selectedSeats = new List<string>();
        private Dictionary<int, int> _selectedCombos = new Dictionary<int, int>(); // ComboId -> Quantity
        private decimal _ticketPrice = 90000; // Default price
        private decimal _totalAmount = 0;
        private string _customerPhone = "";
        private string _customerName = "";

        // UI Panels
        private Panel _headerPanel;
        private Panel _progressPanel;
        private Panel _contentPanel;
        private Panel _footerPanel;
        private Panel _summaryPanel;

        // Step panels
        private Panel _step1Panel;
        private Panel _step2Panel;
        private Panel _step3Panel;
        private Panel _step4Panel;

        public UC_TicketSales()
        {
            _phimBLL = new PhimBLL();
            _veBLL = new VeBLL();
            InitializeComponent();
            LoadMovies();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(20);

            CreateHeader();
            CreateProgressBar();
            CreateFooter();
            CreateSummaryPanel();
            CreateContentPanel();
            CreateAllSteps();

            ShowStep(1);
        }

        #region UI Creation

        private void CreateHeader()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = _cgvBlack,
                Padding = new Padding(20, 15, 20, 15)
            };

            Label lblTitle = new Label
            {
                Text = "🎟️ BÁN VÉ XEM PHIM",
                Font = new Font("Montserrat", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };
            _headerPanel.Controls.Add(lblTitle);

            Button btnReset = new Button
            {
                Text = "🔄 BẮT ĐẦU LẠI",
                Size = new Size(140, 35),
                Location = new Point(_headerPanel.Width - 170, 17),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => ResetWizard();
            _headerPanel.Controls.Add(btnReset);

            this.Controls.Add(_headerPanel);
        }

        private void CreateProgressBar()
        {
            _progressPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(50, 20, 50, 20)
            };

            // Create step indicators
            string[] steps = { "Chọn phim & suất", "Chọn ghế", "Thêm combo", "Thanh toán" };
            int stepWidth = (_progressPanel.Width - 100) / 4;

            FlowLayoutPanel stepsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            for (int i = 0; i < steps.Length; i++)
            {
                Panel stepPanel = CreateStepIndicator(i + 1, steps[i], i + 1 == _currentStep);
                stepsFlow.Controls.Add(stepPanel);
            }

            _progressPanel.Controls.Add(stepsFlow);
            this.Controls.Add(_progressPanel);
        }

        private Panel CreateStepIndicator(int stepNum, string text, bool isActive)
        {
            Panel panel = new Panel
            {
                Size = new Size(200, 50),
                BackColor = Color.Transparent,
                Tag = stepNum
            };

            // Circle indicator
            Panel circle = new Panel
            {
                Size = new Size(30, 30),
                Location = new Point(85, 0),
                BackColor = isActive ? _cgvRed : (stepNum < _currentStep ? Color.Green : Color.LightGray),
                Tag = "circle"
            };
            MakeCircular(circle);

            Label lblNum = new Label
            {
                Text = stepNum < _currentStep ? "✓" : stepNum.ToString(),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            circle.Controls.Add(lblNum);
            panel.Controls.Add(circle);

            // Step text
            Label lblText = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, isActive ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = isActive ? _cgvRed : Color.DimGray,
                Location = new Point(0, 32),
                Size = new Size(200, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = "text"
            };
            panel.Controls.Add(lblText);

            return panel;
        }

        private void CreateFooter()
        {
            _footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Button btnBack = new Button
            {
                Text = "← QUAY LẠI",
                Size = new Size(140, 45),
                Location = new Point(20, 12),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Name = "btnBack"
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += BtnBack_Click;
            _footerPanel.Controls.Add(btnBack);

            Button btnNext = new Button
            {
                Text = "TIẾP TỤC →",
                Size = new Size(180, 45),
                Location = new Point(_footerPanel.Width - 220, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Name = "btnNext"
            };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += BtnNext_Click;
            _footerPanel.Controls.Add(btnNext);

            this.Controls.Add(_footerPanel);
        }

        private void CreateSummaryPanel()
        {
            _summaryPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            Label lblSummary = new Label
            {
                Text = "📋 TÓM TẮT ĐƠN HÀNG",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 35
            };
            _summaryPanel.Controls.Add(lblSummary);

            Panel divider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = _cgvLightGray
            };
            _summaryPanel.Controls.Add(divider);

            // Summary content will be updated dynamically
            Panel summaryContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Name = "summaryContent",
                AutoScroll = true
            };
            _summaryPanel.Controls.Add(summaryContent);

            this.Controls.Add(_summaryPanel);
        }

        private void CreateContentPanel()
        {
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _cgvLightGray,
                Padding = new Padding(10)
            };
            this.Controls.Add(_contentPanel);
        }

        private void CreateAllSteps()
        {
            CreateStep1_MovieSelection();
            CreateStep2_SeatSelection();
            CreateStep3_Combos();
            CreateStep4_Payment();
        }

        #endregion

        #region Step 1: Movie & Showtime Selection

        private void CreateStep1_MovieSelection()
        {
            _step1Panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Visible = false
            };
            AddRoundedCorners(_step1Panel, 10);

            // Movies section
            Label lblMovies = new Label
            {
                Text = "🎬 CHỌN PHIM",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40
            };
            _step1Panel.Controls.Add(lblMovies);

            // Movie cards container
            FlowLayoutPanel moviesFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 280,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Name = "moviesFlow",
                BackColor = Color.Transparent
            };
            _step1Panel.Controls.Add(moviesFlow);

            // Showtimes section
            Label lblShowtimes = new Label
            {
                Text = "⏰ CHỌN SUẤT CHIẾU",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(0, 10, 0, 0)
            };
            _step1Panel.Controls.Add(lblShowtimes);

            FlowLayoutPanel showtimesFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Name = "showtimesFlow",
                BackColor = Color.Transparent
            };
            _step1Panel.Controls.Add(showtimesFlow);

            _contentPanel.Controls.Add(_step1Panel);
        }

        private void LoadMovies()
        {
            try
            {
                _dtPhim = _phimBLL.LayTatCaPhim();
                FlowLayoutPanel moviesFlow = _step1Panel.Controls.Find("moviesFlow", true).FirstOrDefault() as FlowLayoutPanel;
                if (moviesFlow == null) return;

                moviesFlow.Controls.Clear();

                foreach (DataRow row in _dtPhim.Rows)
                {
                    // Only show currently showing movies
                    string trangThai = row["TrangThai"]?.ToString() ?? "";
                    if (trangThai != "Đang chiếu") continue;

                    Panel card = CreateMovieCard(row);
                    moviesFlow.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách phim: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateMovieCard(DataRow row)
        {
            int maPhim = Convert.ToInt32(row["MaPhim"]);
            string tenPhim = row["TenPhim"]?.ToString() ?? "N/A";

            Panel card = new Panel
            {
                Size = new Size(130, 220),
                Margin = new Padding(5),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = maPhim
            };
            AddRoundedCorners(card, 8);
            AddShadow(card);

            // Poster
            PictureBox poster = new PictureBox
            {
                Size = new Size(120, 160),
                Location = new Point(5, 5),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.DarkGray
            };
            LoadPoster(poster, row["HinhAnh"]?.ToString());
            card.Controls.Add(poster);

            // Title
            Label lblTitle = new Label
            {
                Text = tenPhim.Length > 15 ? tenPhim.Substring(0, 15) + "..." : tenPhim,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(5, 170),
                Size = new Size(120, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblTitle);

            // Duration
            Label lblDuration = new Label
            {
                Text = $"⏱️ {row["ThoiLuong"]} phút",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.DimGray,
                Location = new Point(5, 190),
                Size = new Size(120, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblDuration);

            // Click event
            card.Click += (s, e) => SelectMovie(maPhim, tenPhim);
            poster.Click += (s, e) => SelectMovie(maPhim, tenPhim);
            lblTitle.Click += (s, e) => SelectMovie(maPhim, tenPhim);
            lblDuration.Click += (s, e) => SelectMovie(maPhim, tenPhim);

            return card;
        }

        private void LoadPoster(PictureBox pb, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string localPath = Path.Combine(Application.StartupPath, "uploads", "movies", Path.GetFileName(path));
                    if (File.Exists(localPath))
                    {
                        pb.Image = Image.FromFile(localPath);
                        return;
                    }
                }
                catch { }
            }
            pb.BackColor = Color.FromArgb(60, 60, 60);
        }

        private void SelectMovie(int maPhim, string tenPhim)
        {
            _selectedMovieId = maPhim;
            _selectedMovieName = tenPhim;

            // Highlight selected card
            FlowLayoutPanel moviesFlow = _step1Panel.Controls.Find("moviesFlow", true).FirstOrDefault() as FlowLayoutPanel;
            if (moviesFlow != null)
            {
                foreach (Panel card in moviesFlow.Controls.OfType<Panel>())
                {
                    int cardId = card.Tag != null ? Convert.ToInt32(card.Tag) : -1;
                    card.BackColor = cardId == maPhim ? Color.FromArgb(255, 240, 240) : Color.White;
                }
            }

            // Load showtimes for this movie
            LoadShowtimes(maPhim);
            UpdateSummary();
        }

        private void LoadShowtimes(int maPhim)
        {
            FlowLayoutPanel showtimesFlow = _step1Panel.Controls.Find("showtimesFlow", true).FirstOrDefault() as FlowLayoutPanel;
            if (showtimesFlow == null) return;

            showtimesFlow.Controls.Clear();

            try
            {
                // Get showtimes from BLL (simplified - would need actual showtime query)
                // For demo, create sample showtimes
                string[] times = { "09:00", "11:30", "14:00", "16:30", "19:00", "21:30" };
                string[] rooms = { "Phòng 1", "Phòng 2", "Phòng 3" };
                Random rand = new Random(maPhim);

                DateTime today = DateTime.Today;

                for (int d = 0; d < 3; d++) // 3 days
                {
                    DateTime date = today.AddDays(d);
                    
                    // Date header
                    Label lblDate = new Label
                    {
                        Text = d == 0 ? "Hôm nay" : (d == 1 ? "Ngày mai" : date.ToString("dd/MM")),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = _cgvBlack,
                        Size = new Size(700, 30),
                        Margin = new Padding(0, 10, 0, 5)
                    };
                    showtimesFlow.Controls.Add(lblDate);

                    foreach (string time in times)
                    {
if (d == 0 && DateTime.Today.Add(TimeSpan.Parse(time)) < DateTime.Now.AddMinutes(30))
                            continue; // Skip past showtimes

                        string room = rooms[rand.Next(rooms.Length)];
                        int showtimeId = (d * 100) + Array.IndexOf(times, time);

                        Button btnShowtime = new Button
                        {
                            Text = $"{time}\n{room}",
                            Size = new Size(100, 50),
                            Margin = new Padding(5),
                            BackColor = Color.White,
                            ForeColor = _cgvBlack,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 9),
                            Cursor = Cursors.Hand,
                            Tag = new { Id = showtimeId, Time = time, Room = room, Date = date }
                        };
                        btnShowtime.FlatAppearance.BorderColor = _cgvRed;
                        btnShowtime.Click += BtnShowtime_Click;
                        showtimesFlow.Controls.Add(btnShowtime);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải suất chiếu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnShowtime_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null) return;

            dynamic tag = btn.Tag;
            _selectedShowtimeId = tag.Id;
            _selectedShowtimeInfo = $"{((DateTime)tag.Date).ToString("dd/MM")} - {tag.Time}";
            _selectedRoom = tag.Room;

            // Highlight selected
            FlowLayoutPanel showtimesFlow = _step1Panel.Controls.Find("showtimesFlow", true).FirstOrDefault() as FlowLayoutPanel;
            if (showtimesFlow != null)
            {
                foreach (Button b in showtimesFlow.Controls.OfType<Button>())
                {
                    b.BackColor = b == btn ? _cgvRed : Color.White;
                    b.ForeColor = b == btn ? Color.White : _cgvBlack;
                }
            }

            UpdateSummary();
        }

        #endregion

        #region Step 2: Seat Selection

        private void CreateStep2_SeatSelection()
        {
            _step2Panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Visible = false
            };
            AddRoundedCorners(_step2Panel, 10);

            // Screen indicator
            Panel screenPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };

            Label lblScreen = new Label
            {
                Text = "📺 MÀN HÌNH",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = _cgvBlack,
                Size = new Size(400, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point((_step2Panel.Width - 400) / 2, 10)
            };
            screenPanel.Controls.Add(lblScreen);
            _step2Panel.Controls.Add(screenPanel);

            // Seat grid
            Panel seatContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Name = "seatContainer"
            };
            _step2Panel.Controls.Add(seatContainer);

            // Legend
            Panel legendPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Transparent
            };

            FlowLayoutPanel legendFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            legendFlow.Controls.Add(CreateLegendItem("🪑", "Trống", Color.White));
            legendFlow.Controls.Add(CreateLegendItem("✅", "Đã chọn", _cgvRed));
            legendFlow.Controls.Add(CreateLegendItem("❌", "Đã bán", Color.Gray));
            legendFlow.Controls.Add(CreateLegendItem("💺", "VIP (+30k)", _cgvGold));

            legendPanel.Controls.Add(legendFlow);
            _step2Panel.Controls.Add(legendPanel);

            _contentPanel.Controls.Add(_step2Panel);
        }

        private Panel CreateLegendItem(string icon, string text, Color color)
        {
            Panel item = new Panel
            {
                Size = new Size(120, 30),
                Margin = new Padding(10, 5, 10, 5),
                BackColor = Color.Transparent
            };

            Panel indicator = new Panel
            {
                Size = new Size(25, 25),
                Location = new Point(0, 2),
                BackColor = color
            };
            item.Controls.Add(indicator);

            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = _cgvBlack,
                Location = new Point(30, 5),
                AutoSize = true
            };
            item.Controls.Add(lbl);

            return item;
        }

        private void LoadSeatMap()
        {
            Panel seatContainer = _step2Panel.Controls.Find("seatContainer", true).FirstOrDefault() as Panel;
            if (seatContainer == null) return;

            seatContainer.Controls.Clear();
            _selectedSeats.Clear();

            // Create seat grid (8 rows x 10 columns)
            int rows = 8;
            int cols = 10;
            int seatSize = 40;
            int gap = 5;
            int startX = (seatContainer.Width - (cols * (seatSize + gap))) / 2;
            int startY = 20;

            Random rand = new Random(_selectedShowtimeId);

            for (int r = 0; r < rows; r++)
            {
                char rowLetter = (char)('A' + r);
                
                // Row label
                Label lblRow = new Label
                {
                    Text = rowLetter.ToString(),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = _cgvBlack,
                    Size = new Size(30, seatSize),
                    Location = new Point(startX - 35, startY + r * (seatSize + gap)),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                seatContainer.Controls.Add(lblRow);

                for (int c = 0; c < cols; c++)
                {
                    string seatId = $"{rowLetter}{c + 1}";
                    bool isVip = r >= 6; // Last 2 rows are VIP
                    bool isTaken = rand.Next(100) < 30; // 30% chance taken

                    Button btnSeat = new Button
                    {
                        Text = (c + 1).ToString(),
                        Size = new Size(seatSize, seatSize),
                        Location = new Point(startX + c * (seatSize + gap), startY + r * (seatSize + gap)),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8),
                        Cursor = isTaken ? Cursors.No : Cursors.Hand,
                        Tag = new { SeatId = seatId, IsVip = isVip, IsTaken = isTaken },
                        Enabled = !isTaken
                    };
                    btnSeat.FlatAppearance.BorderSize = 1;

                    if (isTaken)
                    {
                        btnSeat.BackColor = Color.Gray;
                        btnSeat.ForeColor = Color.White;
                    }
                    else if (isVip)
                    {
                        btnSeat.BackColor = Color.FromArgb(255, 248, 230);
                        btnSeat.ForeColor = _cgvBlack;
                        btnSeat.FlatAppearance.BorderColor = _cgvGold;
                    }
                    else
                    {
                        btnSeat.BackColor = Color.White;
                        btnSeat.ForeColor = _cgvBlack;
                        btnSeat.FlatAppearance.BorderColor = Color.LightGray;
                    }

                    btnSeat.Click += BtnSeat_Click;
                    seatContainer.Controls.Add(btnSeat);
                }
            }

            UpdateSummary();
        }

        private void BtnSeat_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null) return;

            dynamic tag = btn.Tag;
            string seatId = tag.SeatId;
            bool isVip = tag.IsVip;

            if (_selectedSeats.Contains(seatId))
            {
                // Deselect
                _selectedSeats.Remove(seatId);
                btn.BackColor = isVip ? Color.FromArgb(255, 248, 230) : Color.White;
                btn.ForeColor = _cgvBlack;
            }
            else
            {
                // Select
                if (_selectedSeats.Count >= 8)
                {
                    MessageBox.Show("Chỉ được chọn tối đa 8 ghế!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _selectedSeats.Add(seatId);
                btn.BackColor = _cgvRed;
                btn.ForeColor = Color.White;
            }

            UpdateSummary();
        }

        #endregion

        #region Step 3: Combos

        private void CreateStep3_Combos()
        {
            _step3Panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Visible = false
            };
            AddRoundedCorners(_step3Panel, 10);

            Label lblTitle = new Label
            {
                Text = "🍿 THÊM COMBO / ĐỒ ĂN",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40
            };
            _step3Panel.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = "Bỏ qua nếu không cần - Nhấn TIẾP TỤC để sang thanh toán",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Dock = DockStyle.Top,
                Height = 30
            };
            _step3Panel.Controls.Add(lblSubtitle);

            FlowLayoutPanel combosFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Name = "combosFlow"
            };

            // Add combo items
            combosFlow.Controls.Add(CreateComboCard(1, "Combo 1", "Bắp + Nước", 85000, "🍿🥤"));
            combosFlow.Controls.Add(CreateComboCard(2, "Combo 2", "Bắp lớn + 2 Nước", 120000, "🍿🥤🥤"));
            combosFlow.Controls.Add(CreateComboCard(3, "Combo Gia đình", "2 Bắp + 4 Nước", 199000, "🍿🍿🥤🥤🥤🥤"));
            combosFlow.Controls.Add(CreateComboCard(4, "Snack Box", "Nachos + Nước", 75000, "🌮🥤"));
            combosFlow.Controls.Add(CreateComboCard(5, "Hot Dog Combo", "Hot Dog + Nước", 69000, "🌭🥤"));
            combosFlow.Controls.Add(CreateComboCard(6, "Nước ngọt", "Coca/Pepsi/Sprite", 35000, "🥤"));

            _step3Panel.Controls.Add(combosFlow);
            _contentPanel.Controls.Add(_step3Panel);
        }

        private Panel CreateComboCard(int id, string name, string desc, decimal price, string emoji)
        {
            Panel card = new Panel
            {
                Size = new Size(200, 180),
                Margin = new Padding(10),
                BackColor = Color.White,
                Tag = id
            };
            AddRoundedCorners(card, 10);
            AddShadow(card);

            Label lblEmoji = new Label
            {
                Text = emoji,
                Font = new Font("Segoe UI", 24),
                Location = new Point(10, 10),
                Size = new Size(180, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblEmoji);

            Label lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(10, 60),
                Size = new Size(180, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblName);

            Label lblDesc = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                Location = new Point(10, 85),
                Size = new Size(180, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblDesc);

            Label lblPrice = new Label
            {
                Text = $"{price:N0}đ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _cgvRed,
                Location = new Point(10, 105),
                Size = new Size(180, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblPrice);

            // Quantity controls
            Panel qtyPanel = new Panel
            {
                Location = new Point(40, 135),
                Size = new Size(120, 35),
                BackColor = Color.Transparent
            };

            Button btnMinus = new Button
            {
                Text = "-",
                Size = new Size(35, 30),
                Location = new Point(0, 0),
                BackColor = _cgvLightGray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnMinus.FlatAppearance.BorderSize = 0;

            Label lblQty = new Label
            {
                Text = "0",
                Size = new Size(40, 30),
                Location = new Point(40, 0),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = "qty"
            };

            Button btnPlus = new Button
            {
                Text = "+",
                Size = new Size(35, 30),
                Location = new Point(85, 0),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnPlus.FlatAppearance.BorderSize = 0;

            btnMinus.Click += (s, e) =>
            {
                int qty = int.Parse(lblQty.Text);
                if (qty > 0)
                {
                    qty--;
                    lblQty.Text = qty.ToString();
                    if (qty == 0)
                        _selectedCombos.Remove(id);
                    else
                        _selectedCombos[id] = qty;
                    UpdateSummary();
                }
            };

            btnPlus.Click += (s, e) =>
            {
                int qty = int.Parse(lblQty.Text);
                if (qty < 10)
                {
                    qty++;
                    lblQty.Text = qty.ToString();
                    _selectedCombos[id] = qty;
                    UpdateSummary();
                }
            };

            qtyPanel.Controls.AddRange(new Control[] { btnMinus, lblQty, btnPlus });
            card.Controls.Add(qtyPanel);

            return card;
        }

        #endregion

        #region Step 4: Payment

        private void CreateStep4_Payment()
        {
            _step4Panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Visible = false
            };
            AddRoundedCorners(_step4Panel, 10);

            Label lblTitle = new Label
            {
                Text = "💳 THANH TOÁN",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40
            };
            _step4Panel.Controls.Add(lblTitle);

            Panel formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Name = "paymentForm"
            };

            int y = 20;

            // Customer info section
            Label lblCustomer = new Label
            {
                Text = "👤 THÔNG TIN KHÁCH HÀNG (không bắt buộc)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(0, y),
                AutoSize = true
            };
            formPanel.Controls.Add(lblCustomer);
            y += 35;

            Label lblPhone = new Label { Text = "SĐT:", Location = new Point(0, y), AutoSize = true };
            formPanel.Controls.Add(lblPhone);
            TextBox txtPhone = new TextBox
            {
                Location = new Point(100, y - 3),
                Width = 200,
                Font = new Font("Segoe UI", 10),
                Name = "txtPhone"
            };
            txtPhone.TextChanged += (s, e) => _customerPhone = txtPhone.Text;
            formPanel.Controls.Add(txtPhone);

            Button btnLookup = new Button
            {
                Text = "🔍 Tìm",
                Location = new Point(310, y - 5),
                Size = new Size(70, 28),
                BackColor = _cgvBlack,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLookup.FlatAppearance.BorderSize = 0;
            btnLookup.Click += BtnLookup_Click;
            formPanel.Controls.Add(btnLookup);
            y += 40;

            Label lblName = new Label { Text = "Tên KH:", Location = new Point(0, y), AutoSize = true };
            formPanel.Controls.Add(lblName);
            TextBox txtName = new TextBox
            {
                Location = new Point(100, y - 3),
                Width = 280,
                Font = new Font("Segoe UI", 10),
                Name = "txtName"
            };
            txtName.TextChanged += (s, e) => _customerName = txtName.Text;
            formPanel.Controls.Add(txtName);
            y += 50;

            // Payment method
            Label lblMethod = new Label
            {
                Text = "💰 PHƯƠNG THỨC THANH TOÁN",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(0, y),
                AutoSize = true
            };
            formPanel.Controls.Add(lblMethod);
            y += 35;

            // Payment buttons
            string[] methods = { "💵 Tiền mặt", "💳 Thẻ", "📱 MoMo", "📱 ZaloPay" };
            Color[] colors = { Color.FromArgb(39, 174, 96), Color.FromArgb(41, 128, 185), 
                              Color.FromArgb(166, 54, 124), Color.FromArgb(0, 106, 190) };

            for (int i = 0; i < methods.Length; i++)
            {
                RadioButton rb = new RadioButton
                {
                    Text = methods[i],
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(i % 2 * 200, y + (i / 2) * 45),
                    Size = new Size(180, 35),
                    Appearance = Appearance.Button,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Tag = methods[i]
                };
                rb.FlatAppearance.BorderColor = colors[i];
                rb.CheckedChanged += (s, e) =>
                {
                    if (rb.Checked)
                    {
                        rb.BackColor = colors[Array.IndexOf(methods, rb.Tag.ToString())];
                        rb.ForeColor = Color.White;
                    }
                    else
                    {
                        rb.BackColor = Color.White;
                        rb.ForeColor = _cgvBlack;
                    }
                };
                if (i == 0) rb.Checked = true;
                formPanel.Controls.Add(rb);
            }
            y += 110;

            // Cash received (for cash payment)
            Label lblReceived = new Label
            {
                Text = "Tiền nhận:",
                Location = new Point(0, y),
                AutoSize = true,
                Name = "lblReceived"
            };
            formPanel.Controls.Add(lblReceived);

            TextBox txtReceived = new TextBox
            {
                Location = new Point(100, y - 3),
                Width = 150,
                Font = new Font("Segoe UI", 10),
                Name = "txtReceived"
            };
            txtReceived.TextChanged += TxtReceived_TextChanged;
            formPanel.Controls.Add(txtReceived);

            Label lblChange = new Label
            {
                Text = "Tiền thừa: 0đ",
                Location = new Point(270, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Green,
                Name = "lblChange"
            };
            formPanel.Controls.Add(lblChange);

            _step4Panel.Controls.Add(formPanel);
            _contentPanel.Controls.Add(_step4Panel);
        }

        private void BtnLookup_Click(object sender, EventArgs e)
        {
            TextBox txtPhone = _step4Panel.Controls.Find("txtPhone", true).FirstOrDefault() as TextBox;
            TextBox txtName = _step4Panel.Controls.Find("txtName", true).FirstOrDefault() as TextBox;

            if (txtPhone == null || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Simulate customer lookup
            if (txtPhone.Text == "0901234567")
            {
                txtName.Text = "Nguyễn Văn A";
                _customerName = "Nguyễn Văn A";
                MessageBox.Show("Đã tìm thấy khách hàng!\nĐiểm tích lũy: 1,500", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Không tìm thấy khách hàng.\nSẽ tạo khách mới khi thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TxtReceived_TextChanged(object sender, EventArgs e)
        {
            TextBox txtReceived = sender as TextBox;
            Label lblChange = _step4Panel.Controls.Find("lblChange", true).FirstOrDefault() as Label;

            if (lblChange == null) return;

            if (decimal.TryParse(txtReceived.Text.Replace(",", ""), out decimal received))
            {
                decimal change = received - _totalAmount;
                lblChange.Text = $"Tiền thừa: {Math.Max(0, change):N0}đ";
                lblChange.ForeColor = change >= 0 ? Color.Green : Color.Red;
            }
        }

        #endregion

        #region Navigation & Summary

        private void ShowStep(int step)
        {
            _currentStep = step;

            _step1Panel.Visible = step == 1;
            _step2Panel.Visible = step == 2;
            _step3Panel.Visible = step == 3;
            _step4Panel.Visible = step == 4;

            // Update progress indicators
            UpdateProgressBar();

            // Update navigation buttons
            Button btnBack = _footerPanel.Controls.Find("btnBack", false).FirstOrDefault() as Button;
            Button btnNext = _footerPanel.Controls.Find("btnNext", false).FirstOrDefault() as Button;

            if (btnBack != null) btnBack.Visible = step > 1;
            if (btnNext != null)
            {
                btnNext.Text = step == 4 ? "💳 THANH TOÁN" : "TIẾP TỤC →";
                btnNext.BackColor = step == 4 ? Color.Green : _cgvRed;
            }

            // Load step-specific content
            if (step == 2) LoadSeatMap();

            UpdateSummary();
        }

        private void UpdateProgressBar()
        {
            if (_progressPanel == null) return;

            foreach (Control ctrl in _progressPanel.Controls)
            {
                if (ctrl is FlowLayoutPanel flow)
                {
                    foreach (Control stepCtrl in flow.Controls)
                    {
                        if (stepCtrl is Panel stepPanel && stepPanel.Tag != null)
                        {
                            int stepNum = Convert.ToInt32(stepPanel.Tag);
                            bool isActive = stepNum == _currentStep;
                            bool isCompleted = stepNum < _currentStep;

                            foreach (Control c in stepPanel.Controls)
                            {
                                if (c is Panel circle && c.Tag?.ToString() == "circle")
                                {
                                    circle.BackColor = isCompleted ? Color.Green : (isActive ? _cgvRed : Color.LightGray);
                                    foreach (Control lbl in circle.Controls)
                                    {
                                        if (lbl is Label l)
                                            l.Text = isCompleted ? "✓" : stepNum.ToString();
                                    }
                                }
                                else if (c is Label label && c.Tag?.ToString() == "text")
                                {
                                    label.ForeColor = isActive ? _cgvRed : Color.DimGray;
                                    label.Font = new Font("Segoe UI", 9, isActive ? FontStyle.Bold : FontStyle.Regular);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSummary()
        {
            Panel summaryContent = _summaryPanel.Controls.Find("summaryContent", true).FirstOrDefault() as Panel;
            if (summaryContent == null) return;

            summaryContent.Controls.Clear();
            int y = 10;

            // Movie
            if (!string.IsNullOrEmpty(_selectedMovieName))
            {
                summaryContent.Controls.Add(CreateSummaryRow("🎬 Phim:", _selectedMovieName, ref y));
            }

            // Showtime
            if (_selectedShowtimeId > 0)
            {
                summaryContent.Controls.Add(CreateSummaryRow("⏰ Suất:", _selectedShowtimeInfo, ref y));
                summaryContent.Controls.Add(CreateSummaryRow("🚪 Phòng:", _selectedRoom, ref y));
            }

            // Seats
            if (_selectedSeats.Count > 0)
            {
                summaryContent.Controls.Add(CreateSummaryRow("🪑 Ghế:", string.Join(", ", _selectedSeats), ref y));
                decimal ticketTotal = _selectedSeats.Count * _ticketPrice;
                
                // VIP surcharge
                int vipCount = _selectedSeats.Count(s => s.StartsWith("G") || s.StartsWith("H"));
                ticketTotal += vipCount * 30000;
                
                summaryContent.Controls.Add(CreateSummaryRow($"   ({_selectedSeats.Count} vé):", $"{ticketTotal:N0}đ", ref y, false, _cgvRed));
            }

            // Combos
            decimal comboTotal = 0;
            foreach (var combo in _selectedCombos)
            {
                if (combo.Value > 0)
                {
                    decimal comboPrice = GetComboPrice(combo.Key);
                    decimal subtotal = combo.Value * comboPrice;
                    comboTotal += subtotal;
                    summaryContent.Controls.Add(CreateSummaryRow($"🍿 {GetComboName(combo.Key)} x{combo.Value}:", $"{subtotal:N0}đ", ref y));
                }
            }

            // Divider
            y += 10;
            Panel divider = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(250, 2),
                BackColor = _cgvLightGray
            };
            summaryContent.Controls.Add(divider);
            y += 15;

            // Total
            decimal ticketsTotal = _selectedSeats.Count * _ticketPrice;
            int vipSeats = _selectedSeats.Count(s => s.StartsWith("G") || s.StartsWith("H"));
            ticketsTotal += vipSeats * 30000;
            _totalAmount = ticketsTotal + comboTotal;

            Label lblTotal = new Label
            {
                Text = $"TỔNG CỘNG:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(10, y),
                AutoSize = true
            };
            summaryContent.Controls.Add(lblTotal);

            Label lblTotalValue = new Label
            {
                Text = $"{_totalAmount:N0}đ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _cgvRed,
                Location = new Point(10, y + 25),
                AutoSize = true
            };
            summaryContent.Controls.Add(lblTotalValue);
        }

        private Panel CreateSummaryRow(string label, string value, ref int y, bool bold = false, Color? valueColor = null)
        {
            Panel row = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(250, 25),
                BackColor = Color.Transparent
            };

            Label lblLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(10, 0),
                AutoSize = true
            };
            row.Controls.Add(lblLabel);

            Label lblValue = new Label
            {
                Text = value.Length > 20 ? value.Substring(0, 17) + "..." : value,
                Font = new Font("Segoe UI", 9, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = valueColor ?? _cgvBlack,
                Location = new Point(10, 0),
                Size = new Size(230, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            row.Controls.Add(lblValue);

            y += 25;
            return row;
        }

        private string GetComboName(int id)
        {
            return id switch
            {
                1 => "Combo 1",
                2 => "Combo 2",
                3 => "Combo Gia đình",
                4 => "Snack Box",
                5 => "Hot Dog Combo",
                6 => "Nước ngọt",
                _ => "Combo"
            };
        }

        private decimal GetComboPrice(int id)
        {
            return id switch
            {
                1 => 85000,
                2 => 120000,
                3 => 199000,
                4 => 75000,
                5 => 69000,
                6 => 35000,
                _ => 0
            };
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (_currentStep > 1)
                ShowStep(_currentStep - 1);
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            // Validate current step
            if (!ValidateStep(_currentStep))
                return;

            if (_currentStep < 4)
            {
                ShowStep(_currentStep + 1);
            }
            else
            {
                // Process payment
                ProcessPayment();
            }
        }

        private bool ValidateStep(int step)
        {
            switch (step)
            {
                case 1:
                    if (_selectedMovieId < 0)
                    {
                        MessageBox.Show("Vui lòng chọn phim!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    if (_selectedShowtimeId < 0)
                    {
                        MessageBox.Show("Vui lòng chọn suất chiếu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    return true;

                case 2:
                    if (_selectedSeats.Count == 0)
                    {
                        MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    return true;

                case 3:
                    return true; // Combos are optional

                case 4:
                    return true;

                default:
                    return true;
            }
        }

        private void ProcessPayment()
        {
            try
            {
                // Show confirmation
                string summary = $"📋 XÁC NHẬN ĐƠN HÀNG\n\n" +
                                $"🎬 Phim: {_selectedMovieName}\n" +
                                $"⏰ Suất: {_selectedShowtimeInfo}\n" +
                                $"🚪 Phòng: {_selectedRoom}\n" +
                                $"🪑 Ghế: {string.Join(", ", _selectedSeats)}\n" +
                                $"💰 Tổng tiền: {_totalAmount:N0}đ\n\n" +
                                $"Xác nhận thanh toán?";

                if (MessageBox.Show(summary, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                // Create tickets (would be actual DB call)
                // For demo, just show success

                // Generate receipt using ReceiptPrinter.TicketReceipt class
                var receipt = new ReceiptPrinter.TicketReceipt
                {
                    TransactionId = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    MovieTitle = _selectedMovieName,
                    ShowDate = DateTime.Today.ToString("dd/MM/yyyy"),
                    ShowTime = _selectedShowtimeInfo.Split('-').Last().Trim(),
                    RoomName = _selectedRoom,
                    Seats = string.Join(", ", _selectedSeats),
                    TicketCount = _selectedSeats.Count,
                    TicketPrice = _ticketPrice,
                    CustomerPhone = _customerPhone,
                    Total = _totalAmount,
                    PaymentMethod = "Tiền mặt",
                    CashierName = "Nhân viên",
                    BranchName = "CGV Cinema"
                };

                // Show print preview
                if (MessageBox.Show("Thanh toán thành công!\n\nIn hóa đơn?", "Thành công", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    var printer = new ReceiptPrinter();
                    string receiptText = printer.GenerateTicketReceipt(receipt);
                    printer.ShowPrintPreview(receiptText);
                }

                // Reset wizard
                ResetWizard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thanh toán: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetWizard()
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
            _totalAmount = 0;

            ShowStep(1);
            LoadMovies();
        }

        #endregion

        #region Helpers

        private void AddRoundedCorners(Control control, int radius)
        {
            try
            {
                GraphicsPath path = new GraphicsPath();
                int w = control.Width > 0 ? control.Width : 100;
                int h = control.Height > 0 ? control.Height : 100;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(w - radius, 0, radius, radius, 270, 90);
                path.AddArc(w - radius, h - radius, radius, radius, 0, 90);
                path.AddArc(0, h - radius, radius, radius, 90, 90);
                path.CloseFigure();
                control.Region = new Region(path);
            }
            catch { }
        }

        private void MakeCircular(Control control)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, control.Width, control.Height);
            control.Region = new Region(path);
        }

        private void AddShadow(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, panel.Height - 2, panel.Width - 1, 2);
                }
            };
        }

        #endregion
    }
}
