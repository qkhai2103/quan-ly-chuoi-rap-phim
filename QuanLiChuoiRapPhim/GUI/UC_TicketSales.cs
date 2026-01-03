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
        // Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

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

        // UI Panels
        private Panel _mainContent;
        private Panel _summaryPanel;
        private Panel _navPanel;
        private Panel[] _stepPanels;
        private Label[] _stepIndicators;

        public UC_TicketSales()
        {
            _phimBLL = new PhimBLL();
            _stepPanels = new Panel[6];
            _stepIndicators = new Label[6];
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Fill;
            this.BackColor = _cgvGray;

            // Main layout
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            // Left side
            Panel leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            CreateHeader(leftPanel);
            CreateStepIndicator(leftPanel);
            CreateMainContent(leftPanel);
            CreateNavigation(leftPanel);
            layout.Controls.Add(leftPanel, 0, 0);

            // Right side - Summary
            CreateSummaryPanel();
            layout.Controls.Add(_summaryPanel, 1, 0);

            this.Controls.Add(layout);
            this.ResumeLayout();

            this.Load += (s, e) => { ShowStep(1); LoadMovies(); };
        }

        #region Header & Navigation

        private void CreateHeader(Panel parent)
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = _cgvBlack
            };

            Label title = new Label
            {
                Text = "🎟️ BÁN VÉ XEM PHIM",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 12),
                AutoSize = true
            };
            header.Controls.Add(title);

            Button btnReset = new Button
            {
                Text = "🔄 LÀM MỚI",
                Size = new Size(90, 30),
                Location = new Point(header.Width - 110, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => ResetAll();
            header.Controls.Add(btnReset);

            parent.Controls.Add(header);
        }

        private void CreateStepIndicator(Panel parent)
        {
            Panel stepBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White
            };

            string[] stepNames = { "Phim", "Suất", "Ghế", "Combo", "KH", "Thanh toán" };
            int startX = 30;
            int spacing = 110;

            for (int i = 0; i < 6; i++)
            {
                Panel stepItem = new Panel
                {
                    Location = new Point(startX + i * spacing, 10),
                    Size = new Size(100, 35)
                };

                Label circle = new Label
                {
                    Text = (i + 1).ToString(),
                    Size = new Size(28, 28),
                    Location = new Point(0, 3),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    BackColor = i == 0 ? _cgvRed : Color.LightGray,
                    ForeColor = i == 0 ? Color.White : Color.DimGray
                };
                MakeCircular(circle);
                stepItem.Controls.Add(circle);
                _stepIndicators[i] = circle;

                Label name = new Label
                {
                    Text = stepNames[i],
                    Location = new Point(32, 8),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9)
                };
                stepItem.Controls.Add(name);

                stepBar.Controls.Add(stepItem);
            }

            parent.Controls.Add(stepBar);
        }

        private void CreateNavigation(Panel parent)
        {
            _navPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            Button btnBack = new Button
            {
                Name = "btnBack",
                Text = "← QUAY LẠI",
                Size = new Size(120, 40),
                Location = new Point(20, 10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Visible = false
            };
            btnBack.FlatAppearance.BorderColor = Color.Gray;
            btnBack.Click += (s, e) => { if (_currentStep > 1) ShowStep(_currentStep - 1); };
            _navPanel.Controls.Add(btnBack);

            Button btnNext = new Button
            {
                Name = "btnNext",
                Text = "TIẾP TỤC →",
                Size = new Size(140, 40),
                Location = new Point(_navPanel.Width - 160, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += BtnNext_Click;
            _navPanel.Controls.Add(btnNext);

            parent.Controls.Add(_navPanel);
        }

        private void CreateMainContent(Panel parent)
        {
            _mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            for (int i = 0; i < 6; i++)
            {
                _stepPanels[i] = new Panel
                {
                    Dock = DockStyle.Fill,
                    Visible = false,
                    AutoScroll = true,
                    BackColor = Color.White
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

        #region Step 1: Movie Selection

        private void CreateStep1_Movie()
        {
            Panel p = _stepPanels[0];

            Label lbl = new Label
            {
                Text = "🎬 CHỌN PHIM",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Name = "moviesFlow",
                Location = new Point(10, 45),
                Size = new Size(700, 400),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
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
                    flow.Controls.Add(new Label { Text = "Không có phim", AutoSize = true, Padding = new Padding(20) });
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
                        Size = new Size(130, 200),
                        Margin = new Padding(8),
                        BackColor = Color.White,
                        Cursor = Cursors.Hand,
                        Tag = new { Id = id, Name = name }
                    };
                    card.BorderStyle = BorderStyle.FixedSingle;

                    // Poster
                    PictureBox pb = new PictureBox
                    {
                        Size = new Size(120, 140),
                        Location = new Point(5, 5),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.FromArgb(50, 50, 50),
                        Cursor = Cursors.Hand
                    };

                    // Load image
                    try
                    {
                        string path = Path.Combine(Application.StartupPath, "uploads", "movies", Path.GetFileName(img));
                        if (File.Exists(path)) pb.Image = Image.FromFile(path);
                    }
                    catch { }

                    card.Controls.Add(pb);

                    Label lblName = new Label
                    {
                        Text = name.Length > 14 ? name.Substring(0, 11) + "..." : name,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        Location = new Point(5, 150),
                        Size = new Size(120, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Cursor = Cursors.Hand
                    };
                    card.Controls.Add(lblName);

                    Label lblDur = new Label
                    {
                        Text = $"⏱ {duration} phút",
                        Font = new Font("Segoe UI", 8),
                        ForeColor = Color.Gray,
                        Location = new Point(5, 172),
                        Size = new Size(120, 18),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Cursor = Cursors.Hand
                    };
                    card.Controls.Add(lblDur);

                    // Click handler for entire card
                    EventHandler clickHandler = (s, e) => SelectMovie(id, name);
                    card.Click += clickHandler;
                    pb.Click += clickHandler;
                    lblName.Click += clickHandler;
                    lblDur.Click += clickHandler;

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

            // Highlight
            FlowLayoutPanel flow = _stepPanels[0].Controls.Find("moviesFlow", true).FirstOrDefault() as FlowLayoutPanel;
            if (flow != null)
            {
                foreach (Control c in flow.Controls)
                {
                    if (c is Panel p && p.Tag != null)
                    {
                        dynamic tag = p.Tag;
                        p.BackColor = tag.Id == id ? Color.FromArgb(255, 230, 230) : Color.White;
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
                Text = "⏰ CHỌN SUẤT CHIẾU",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Name = "showtimesFlow",
                Location = new Point(10, 45),
                Size = new Size(700, 380),
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
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Size = new Size(680, 28),
                    Margin = new Padding(5, 15, 5, 5)
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
                        Size = new Size(95, 48),
                        Margin = new Padding(5),
                        BackColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9),
                        Cursor = Cursors.Hand,
                        Tag = new { Id = stId, Time = time, Room = room, Date = date }
                    };
                    btn.FlatAppearance.BorderColor = _cgvRed;
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
                        b.BackColor = b == btn ? _cgvRed : Color.White;
                        b.ForeColor = b == btn ? Color.White : _cgvBlack;
                    }
                }
            }
            UpdateSummary();
        }

        #endregion

        #region Step 3: Seat Selection

        private void CreateStep3_Seats()
        {
            Panel p = _stepPanels[2];

            Label lbl = new Label
            {
                Text = "🪑 CHỌN GHẾ NGỒI",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            // Screen
            Label screen = new Label
            {
                Text = "📺 MÀN HÌNH",
                Size = new Size(400, 30),
                Location = new Point(100, 45),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = _cgvBlack,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            p.Controls.Add(screen);

            Panel seatArea = new Panel
            {
                Name = "seatArea",
                Location = new Point(50, 90),
                Size = new Size(550, 300)
            };
            p.Controls.Add(seatArea);

            // Legend
            FlowLayoutPanel legend = new FlowLayoutPanel
            {
                Location = new Point(50, 400),
                Size = new Size(550, 35),
                FlowDirection = FlowDirection.LeftToRight
            };
            legend.Controls.Add(CreateLegend("Trống", Color.White));
            legend.Controls.Add(CreateLegend("Đã chọn", _cgvRed));
            legend.Controls.Add(CreateLegend("Đã bán", Color.Gray));
            legend.Controls.Add(CreateLegend("VIP +30k", _cgvGold));
            p.Controls.Add(legend);
        }

        private Panel CreateLegend(string text, Color color)
        {
            Panel item = new Panel { Size = new Size(120, 25), Margin = new Padding(5) };
            Panel box = new Panel { Size = new Size(18, 18), Location = new Point(0, 3), BackColor = color };
            box.BorderStyle = BorderStyle.FixedSingle;
            item.Controls.Add(box);
            item.Controls.Add(new Label { Text = text, Location = new Point(22, 4), AutoSize = true, Font = new Font("Segoe UI", 8) });
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
                    Size = new Size(25, 35),
                    Location = new Point(0, r * 38),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                seatArea.Controls.Add(lblRow);

                for (int c = 0; c < 10; c++)
                {
                    string seatId = $"{row}{c + 1}";
                    bool isVip = r >= 6;
                    bool isTaken = rand.Next(100) < 20;

                    Button btn = new Button
                    {
                        Text = (c + 1).ToString(),
                        Size = new Size(35, 32),
                        Location = new Point(30 + c * 40, r * 38),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8),
                        Cursor = isTaken ? Cursors.No : Cursors.Hand,
                        Tag = new { SeatId = seatId, IsVip = isVip, IsTaken = isTaken },
                        Enabled = !isTaken
                    };
                    btn.FlatAppearance.BorderSize = 1;

                    if (isTaken)
                    {
                        btn.BackColor = Color.Gray;
                        btn.ForeColor = Color.White;
                    }
                    else if (isVip)
                    {
                        btn.BackColor = Color.FromArgb(255, 250, 230);
                        btn.FlatAppearance.BorderColor = _cgvGold;
                    }
                    else
                    {
                        btn.BackColor = Color.White;
                        btn.FlatAppearance.BorderColor = Color.LightGray;
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
                btn.BackColor = isVip ? Color.FromArgb(255, 250, 230) : Color.White;
                btn.ForeColor = _cgvBlack;
            }
            else
            {
                if (_selectedSeats.Count >= 8)
                {
                    MessageBox.Show("Tối đa 8 ghế!", "Thông báo");
                    return;
                }
                _selectedSeats.Add(seatId);
                btn.BackColor = _cgvRed;
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
                Text = "🍿 COMBO BẮP NƯỚC (Bỏ qua nếu không cần)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Location = new Point(10, 50),
                Size = new Size(680, 380),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            flow.Controls.Add(CreateComboItem(1, "Combo 1", "Bắp + Nước", 85000));
            flow.Controls.Add(CreateComboItem(2, "Combo 2", "Bắp lớn + 2 Nước", 120000));
            flow.Controls.Add(CreateComboItem(3, "Combo GĐ", "2 Bắp + 4 Nước", 199000));
            flow.Controls.Add(CreateComboItem(4, "Snack Box", "Nachos + Nước", 75000));
            flow.Controls.Add(CreateComboItem(5, "Hot Dog", "Hot dog + Nước", 69000));
            flow.Controls.Add(CreateComboItem(6, "Nước ngọt", "Coca/Pepsi", 35000));

            p.Controls.Add(flow);
        }

        private Panel CreateComboItem(int id, string name, string desc, decimal price)
        {
            Panel card = new Panel
            {
                Size = new Size(200, 100),
                Margin = new Padding(8),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(10, 8), AutoSize = true });
            card.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(10, 28), AutoSize = true });
            card.Controls.Add(new Label { Text = $"{price:N0}đ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = _cgvRed, Location = new Point(10, 48), AutoSize = true });

            Button btnM = new Button { Text = "-", Size = new Size(30, 25), Location = new Point(10, 70), FlatStyle = FlatStyle.Flat };
            Label lblQ = new Label { Text = "0", Size = new Size(30, 25), Location = new Point(45, 73), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Button btnP = new Button { Text = "+", Size = new Size(30, 25), Location = new Point(80, 70), BackColor = _cgvRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnP.FlatAppearance.BorderSize = 0;

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
                Text = "👤 THÔNG TIN KHÁCH HÀNG",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            p.Controls.Add(new Label { Text = "Số ĐT:", Location = new Point(15, 55), AutoSize = true });
            TextBox txtPhone = new TextBox { Name = "txtPhone", Location = new Point(100, 52), Width = 150, Font = new Font("Segoe UI", 10) };
            txtPhone.TextChanged += (s, e) => _customerPhone = txtPhone.Text;
            p.Controls.Add(txtPhone);

            Button btnFind = new Button
            {
                Text = "🔍 Tìm",
                Location = new Point(260, 50),
                Size = new Size(70, 28),
                BackColor = _cgvBlack,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFind.FlatAppearance.BorderSize = 0;
            btnFind.Click += BtnFindCustomer_Click;
            p.Controls.Add(btnFind);

            p.Controls.Add(new Label { Text = "Họ tên:", Location = new Point(15, 95), AutoSize = true });
            TextBox txtName = new TextBox { Name = "txtName", Location = new Point(100, 92), Width = 230, Font = new Font("Segoe UI", 10) };
            txtName.TextChanged += (s, e) => _customerName = txtName.Text;
            p.Controls.Add(txtName);

            Label lblPoints = new Label
            {
                Name = "lblPoints",
                Text = "⭐ Điểm tích lũy: 0",
                Location = new Point(15, 135),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            p.Controls.Add(lblPoints);

            CheckBox chkUse = new CheckBox
            {
                Name = "chkUsePoints",
                Text = "Sử dụng điểm giảm giá (10 điểm = 1,000đ)",
                Location = new Point(15, 165),
                AutoSize = true
            };
            chkUse.CheckedChanged += (s, e) => { _usePoints = chkUse.Checked; UpdateSummary(); };
            p.Controls.Add(chkUse);

            Label lblEarn = new Label
            {
                Name = "lblEarnPoints",
                Text = "💰 Điểm nhận được: 0",
                Location = new Point(15, 200),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Green
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
                MessageBox.Show("Nhập SĐT!", "Thông báo");
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
                                _customerName = r["HoTen"]?.ToString() ?? "";
                                _customerPoints = r["DiemTichLuy"] != DBNull.Value ? Convert.ToInt32(r["DiemTichLuy"]) : 0;
                                if (txtName != null) txtName.Text = _customerName;
                                if (lblPoints != null) lblPoints.Text = $"⭐ Điểm tích lũy: {_customerPoints:N0}";
                                MessageBox.Show($"Tìm thấy: {_customerName}\nĐiểm: {_customerPoints:N0}", "Khách hàng");
                            }
                            else
                            {
                                _customerPoints = 0;
                                if (lblPoints != null) lblPoints.Text = "⭐ Điểm: 0 (Khách mới)";
                                MessageBox.Show("Khách hàng mới", "Thông báo");
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
                Text = "💳 THANH TOÁN",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            // Invoice
            GroupBox grpInv = new GroupBox
            {
                Text = "📋 Hóa đơn",
                Location = new Point(10, 45),
                Size = new Size(300, 200),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            Label lblInv = new Label
            {
                Name = "lblInvoice",
                Location = new Point(10, 25),
                Size = new Size(280, 165),
                Font = new Font("Consolas", 9)
            };
            grpInv.Controls.Add(lblInv);
            p.Controls.Add(grpInv);

            // Cash
            GroupBox grpCash = new GroupBox
            {
                Text = "💵 Tiền mặt",
                Location = new Point(320, 45),
                Size = new Size(280, 130),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            grpCash.Controls.Add(new Label { Text = "Tổng:", Location = new Point(10, 30), AutoSize = true });
            Label lblTotal = new Label { Name = "lblPayTotal", Text = "0đ", Location = new Point(100, 28), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = _cgvRed };
            grpCash.Controls.Add(lblTotal);

            grpCash.Controls.Add(new Label { Text = "Khách đưa:", Location = new Point(10, 65), AutoSize = true });
            TextBox txtCash = new TextBox { Name = "txtCash", Location = new Point(100, 62), Width = 100, Font = new Font("Segoe UI", 10), TextAlign = HorizontalAlignment.Right };
            txtCash.TextChanged += TxtCash_Changed;
            grpCash.Controls.Add(txtCash);

            grpCash.Controls.Add(new Label { Text = "Tiền thừa:", Location = new Point(10, 100), AutoSize = true });
            Label lblChange = new Label { Name = "lblChange", Text = "0đ", Location = new Point(100, 98), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Green };
            grpCash.Controls.Add(lblChange);

            p.Controls.Add(grpCash);

            // QR
            GroupBox grpQR = new GroupBox
            {
                Text = "📱 QR Chuyển khoản",
                Location = new Point(320, 185),
                Size = new Size(280, 200),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            grpQR.Controls.Add(new Label { Text = "MB Bank: 0865691072", Location = new Point(10, 25), AutoSize = true });
            PictureBox pbQR = new PictureBox { Name = "pbQR", Location = new Point(70, 50), Size = new Size(140, 140), BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            grpQR.Controls.Add(pbQR);
            p.Controls.Add(grpQR);

            // Complete
            Button btnComplete = new Button
            {
                Text = "✅ HOÀN TẤT THANH TOÁN",
                Location = new Point(10, 400),
                Size = new Size(280, 45),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnComplete.FlatAppearance.BorderSize = 0;
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
                lblChange.ForeColor = change >= 0 ? Color.Green : Color.Red;
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
                             $"─────────────────\n" +
                             $"TỔNG: {_totalAmount:N0}đ";
            }

            if (lblTotal != null) lblTotal.Text = $"{_totalAmount:N0}đ";

            // Load QR
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
                MessageBox.Show("Chưa chọn ghế!", "Thông báo");
                return;
            }

            string msg = $"Xác nhận thanh toán?\n\n🎬 {_selectedMovieName}\n💰 {_totalAmount:N0}đ";
            if (MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                MessageBox.Show("✅ Thanh toán thành công!", "Hoàn tất");
                ResetAll();
            }
        }

        #endregion

        #region Summary Panel

        private void CreateSummaryPanel()
        {
            _summaryPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            Label lbl = new Label
            {
                Text = "📋 TÓM TẮT ĐƠN HÀNG",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            _summaryPanel.Controls.Add(lbl);

            Panel content = new Panel
            {
                Name = "summaryContent",
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            _summaryPanel.Controls.Add(content);
        }

        private void UpdateSummary()
        {
            Panel content = _summaryPanel.Controls.Find("summaryContent", true).FirstOrDefault() as Panel;
            if (content == null) return;

            content.Controls.Clear();
            int y = 10;

            // Movie
            if (!string.IsNullOrEmpty(_selectedMovieName))
                content.Controls.Add(SummaryRow("🎬 Phim:", _selectedMovieName, ref y));

            // Showtime
            if (_selectedShowtimeId > 0)
            {
                content.Controls.Add(SummaryRow("⏰ Suất:", _selectedShowtimeInfo, ref y));
                content.Controls.Add(SummaryRow("🚪 Phòng:", _selectedRoom, ref y));
            }

            // Seats
            decimal seatTotal = 0;
            if (_selectedSeats.Count > 0)
            {
                content.Controls.Add(SummaryRow("🪑 Ghế:", string.Join(", ", _selectedSeats), ref y));
                int vipCount = _selectedSeats.Count(s => s.StartsWith("G") || s.StartsWith("H"));
                seatTotal = _selectedSeats.Count * _ticketPrice + vipCount * 30000;
                content.Controls.Add(SummaryRow($"  ({_selectedSeats.Count} vé):", $"{seatTotal:N0}đ", ref y, _cgvRed));
            }

            // Combo
            decimal comboTotal = 0;
            foreach (var c in _selectedCombos)
            {
                if (c.Value > 0)
                {
                    decimal pr = GetComboPrice(c.Key);
                    decimal sub = c.Value * pr;
                    comboTotal += sub;
                    content.Controls.Add(SummaryRow($"🍿 {GetComboName(c.Key)} x{c.Value}:", $"{sub:N0}đ", ref y));
                }
            }

            // Points discount
            decimal pointsDiscount = 0;
            if (_usePoints && _customerPoints > 0)
            {
                pointsDiscount = Math.Min(_customerPoints * 100, (seatTotal + comboTotal) * 0.1m);
                content.Controls.Add(SummaryRow("⭐ Giảm điểm:", $"-{pointsDiscount:N0}đ", ref y, Color.Green));
            }

            // Divider
            y += 8;
            content.Controls.Add(new Panel { Location = new Point(5, y), Size = new Size(180, 2), BackColor = Color.LightGray });
            y += 12;

            // Total
            _totalAmount = seatTotal + comboTotal - pointsDiscount;
            content.Controls.Add(new Label { Text = "TỔNG CỘNG:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(5, y), AutoSize = true });
            content.Controls.Add(new Label { Text = $"{_totalAmount:N0}đ", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = _cgvRed, Location = new Point(5, y + 22), AutoSize = true });

            // Points to earn
            int earnPoints = (int)(_totalAmount / 10000);
            Label lblEarn = _stepPanels[4].Controls.Find("lblEarnPoints", true).FirstOrDefault() as Label;
            if (lblEarn != null) lblEarn.Text = $"💰 Điểm nhận được: {earnPoints}";
        }

        private Label SummaryRow(string label, string value, ref int y, Color? color = null)
        {
            Label lbl = new Label
            {
                Text = $"{label} {(value.Length > 15 ? value.Substring(0, 12) + "..." : value)}",
                Font = new Font("Segoe UI", 9),
                ForeColor = color ?? Color.DimGray,
                Location = new Point(5, y),
                AutoSize = true
            };
            y += 22;
            return lbl;
        }

        private string GetComboName(int id) => id switch { 1 => "Combo 1", 2 => "Combo 2", 3 => "Combo GĐ", 4 => "Snack", 5 => "Hot Dog", 6 => "Nước", _ => "Combo" };
        private decimal GetComboPrice(int id) => id switch { 1 => 85000, 2 => 120000, 3 => 199000, 4 => 75000, 5 => 69000, 6 => 35000, _ => 0 };

        #endregion

        #region Navigation & Utilities

        private void ShowStep(int step)
        {
            _currentStep = step;

            for (int i = 0; i < 6; i++)
            {
                _stepPanels[i].Visible = i == step - 1;
                _stepIndicators[i].BackColor = i < step ? Color.Green : (i == step - 1 ? _cgvRed : Color.LightGray);
                _stepIndicators[i].ForeColor = i < step || i == step - 1 ? Color.White : Color.DimGray;
                _stepIndicators[i].Text = i < step - 1 ? "✓" : (i + 1).ToString();
            }

            Button btnBack = _navPanel.Controls.Find("btnBack", false).FirstOrDefault() as Button;
            Button btnNext = _navPanel.Controls.Find("btnNext", false).FirstOrDefault() as Button;

            if (btnBack != null) btnBack.Visible = step > 1;
            if (btnNext != null)
            {
                btnNext.Text = step == 6 ? "" : "TIẾP TỤC →";
                btnNext.Visible = step < 6;
            }

            // Load step data
            if (step == 2) LoadShowtimes();
            if (step == 3) LoadSeats();
            if (step == 6) UpdatePaymentView();

            UpdateSummary();
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            // Validate
            if (_currentStep == 1 && _selectedMovieId < 0)
            {
                MessageBox.Show("Vui lòng chọn phim!", "Thông báo");
                return;
            }
            if (_currentStep == 2 && _selectedShowtimeId < 0)
            {
                MessageBox.Show("Vui lòng chọn suất chiếu!", "Thông báo");
                return;
            }
            if (_currentStep == 3 && _selectedSeats.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ghế!", "Thông báo");
                return;
            }

            if (_currentStep < 6)
                ShowStep(_currentStep + 1);
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

        private void MakeCircular(Control c)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, c.Width, c.Height);
            c.Region = new Region(path);
        }

        #endregion
    }
}
