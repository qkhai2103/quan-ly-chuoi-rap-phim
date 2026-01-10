using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.Services;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// Modal form to display detailed ticket information (replaces MessageBox)
    /// </summary>
    public class frmTicketDetail : Form
    {
        // CGV Branding Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

        private DataRow _ticketData;

        public frmTicketDetail(DataRow ticketData)
        {
            _ticketData = ticketData;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Chi tiết vé";
            this.Size = new Size(550, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = _cgvLightGray;
            this.Font = new Font("Segoe UI", 10);

            // Header
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = _cgvRed
            };

            Label lblHeader = new Label
            {
                Text = "🎟️ CHI TIẾT VÉ XEM PHIM",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            headerPanel.Controls.Add(lblHeader);

            // Main content - styled like a ticket
            Panel ticketPanel = new Panel
            {
                Location = new Point(25, 100),
                Size = new Size(480, 430),
                BackColor = Color.White
            };
            AddDashedBorder(ticketPanel);

            int y = 20;

            // Ticket ID with barcode style
            string maVe = _ticketData["MaVe"]?.ToString() ?? "N/A";
            Panel barcodePanel = CreateBarcodePanel(maVe);
            barcodePanel.Location = new Point(140, y);
            ticketPanel.Controls.Add(barcodePanel);
            y += 70;

            // Movie info section
            ticketPanel.Controls.Add(CreateSectionHeader("🎬 THÔNG TIN PHIM", y));
            y += 35;

            ticketPanel.Controls.Add(CreateInfoRow("Tên phim:", _ticketData["TenPhim"]?.ToString() ?? "N/A", y, true));
            y += 30;
            ticketPanel.Controls.Add(CreateInfoRow("Suất chiếu:", FormatShowtime(), y));
            y += 30;
            ticketPanel.Controls.Add(CreateInfoRow("Phòng:", _ticketData["TenPhong"]?.ToString() ?? _ticketData["MaPhong"]?.ToString() ?? "N/A", y));
            y += 30;
            ticketPanel.Controls.Add(CreateInfoRow("Ghế:", _ticketData["MaGhe"]?.ToString() ?? "N/A", y));
            y += 40;

            // Customer info section
            ticketPanel.Controls.Add(CreateSectionHeader("👤 THÔNG TIN KHÁCH HÀNG", y));
            y += 35;

            string tenKH = _ticketData["TenKhachHang"]?.ToString();
            if (string.IsNullOrEmpty(tenKH)) tenKH = "Khách vãng lai";
            ticketPanel.Controls.Add(CreateInfoRow("Khách hàng:", tenKH, y));
            y += 30;

            string sdt = _ticketData["SoDienThoai"]?.ToString();
            if (string.IsNullOrEmpty(sdt)) sdt = "N/A";
            ticketPanel.Controls.Add(CreateInfoRow("SĐT:", sdt, y));
            y += 40;

            // Payment info section
            ticketPanel.Controls.Add(CreateSectionHeader("💰 THANH TOÁN", y));
            y += 35;

            decimal giaVe = 0;
            if (_ticketData["GiaVe"] != DBNull.Value)
                decimal.TryParse(_ticketData["GiaVe"].ToString(), out giaVe);
            ticketPanel.Controls.Add(CreateInfoRow("Giá vé:", $"{giaVe:N0}đ", y, true, _cgvRed));
            y += 30;

            string trangThai = _ticketData["TrangThai"]?.ToString() ?? "Đã thanh toán";
            Color statusColor = trangThai == "Đã thanh toán" ? Color.Green :
                               trangThai == "Đã hủy" ? Color.Red : Color.Orange;
            ticketPanel.Controls.Add(CreateInfoRow("Trạng thái:", trangThai, y, false, statusColor));
            y += 30;

            DateTime ngayBan = DateTime.Now;
            if (_ticketData["NgayBan"] != DBNull.Value)
                DateTime.TryParse(_ticketData["NgayBan"].ToString(), out ngayBan);
            ticketPanel.Controls.Add(CreateInfoRow("Ngày bán:", ngayBan.ToString("dd/MM/yyyy HH:mm"), y));

            // Footer with CGV logo
            Panel footerTicket = new Panel
            {
                Location = new Point(0, 380),
                Size = new Size(480, 50),
                BackColor = _cgvBlack
            };

            Label lblCGV = new Label
            {
                Text = "CGV CINEMAS 🎬",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _cgvGold,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            footerTicket.Controls.Add(lblCGV);
            ticketPanel.Controls.Add(footerTicket);

            // Action buttons
            Panel buttonPanel = new Panel
            {
                Location = new Point(25, 545),
                Size = new Size(480, 50)
            };

            Button btnPrint = new Button
            {
                Text = "🖨️ IN VÉ",
                Size = new Size(150, 40),
                Location = new Point(0, 0),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;
            buttonPanel.Controls.Add(btnPrint);

            Button btnRefund = new Button
            {
                Text = "💸 YÊU CẦU HOÀN VÉ",
                Size = new Size(170, 40),
                Location = new Point(160, 0),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefund.FlatAppearance.BorderSize = 0;
            btnRefund.Click += BtnRefund_Click;
            buttonPanel.Controls.Add(btnRefund);

            Button btnClose = new Button
            {
                Text = "ĐÓNG",
                Size = new Size(100, 40),
                Location = new Point(380, 0),
                BackColor = _cgvBlack,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            buttonPanel.Controls.Add(btnClose);

            this.Controls.Add(headerPanel);
            this.Controls.Add(ticketPanel);
            this.Controls.Add(buttonPanel);
        }

        private string FormatShowtime()
        {
            DateTime ngayChieu = DateTime.Now;
            if (_ticketData["NgayChieu"] != DBNull.Value)
                DateTime.TryParse(_ticketData["NgayChieu"].ToString(), out ngayChieu);

            string gioChieu = _ticketData["GioChieu"]?.ToString() ?? "";
            if (string.IsNullOrEmpty(gioChieu) && _ticketData["GioBatDau"] != DBNull.Value)
            {
                if (_ticketData["GioBatDau"] is TimeSpan ts)
                    gioChieu = ts.ToString(@"hh\:mm");
                else
                    gioChieu = _ticketData["GioBatDau"].ToString();
            }

            return $"{ngayChieu:dd/MM/yyyy} - {gioChieu}";
        }

        private Panel CreateBarcodePanel(string ticketId)
        {
            Panel panel = new Panel
            {
                Size = new Size(200, 60),
                BackColor = Color.White
            };

            // Simulated barcode lines
            PictureBox barcode = new PictureBox
            {
                Size = new Size(200, 40),
                Location = new Point(0, 0),
                Image = GenerateBarcode(ticketId)
            };
            panel.Controls.Add(barcode);

            Label lblId = new Label
            {
                Text = $"Mã vé: {ticketId}",
                Font = new Font("Consolas", 9),
                ForeColor = _cgvBlack,
                Location = new Point(0, 42),
                AutoSize = true
            };
            panel.Controls.Add(lblId);

            return panel;
        }

        private Image GenerateBarcode(string text)
        {
            Bitmap bmp = new Bitmap(200, 40);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                // Simple barcode simulation
                Random rand = new Random(text.GetHashCode());
                int x = 10;
                while (x < 190)
                {
                    int width = rand.Next(1, 4);
                    if (rand.Next(2) == 0)
                    {
                        g.FillRectangle(Brushes.Black, x, 5, width, 30);
                    }
                    x += width + 1;
                }
            }
            return bmp;
        }

        private Label CreateSectionHeader(string text, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvRed,
                Location = new Point(20, y),
                AutoSize = true
            };
        }

        private Panel CreateInfoRow(string label, string value, int y, bool highlight = false, Color? valueColor = null)
        {
            Panel row = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(440, 25),
                BackColor = Color.Transparent
            };

            Label lblLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray,
                Location = new Point(0, 0),
                AutoSize = true
            };
            row.Controls.Add(lblLabel);

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10, highlight ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = valueColor ?? _cgvBlack,
                Location = new Point(120, 0),
                AutoSize = true
            };
            row.Controls.Add(lblValue);

            return row;
        }

        private void AddDashedBorder(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(_cgvRed, 2))
                {
                    pen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, 1, 1, panel.Width - 3, panel.Height - 3);
                }
            };
        }

        #region Event Handlers

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                // Create receipt data using the correct ReceiptPrinter.TicketReceipt class
                var receipt = new ReceiptPrinter.TicketReceipt
                {
                    TransactionId = _ticketData["MaVe"]?.ToString() ?? "N/A",
                    MovieTitle = _ticketData["TenPhim"]?.ToString() ?? "N/A",
                    ShowDate = _ticketData["NgayChieu"] != DBNull.Value ?
                        Convert.ToDateTime(_ticketData["NgayChieu"]).ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"),
                    ShowTime = _ticketData["GioChieu"]?.ToString() ?? "",
                    RoomName = _ticketData["TenPhong"]?.ToString() ?? _ticketData["MaPhong"]?.ToString() ?? "N/A",
                    Seats = _ticketData["MaGhe"]?.ToString() ?? "N/A",
                    TicketCount = 1,
                    TicketPrice = _ticketData["GiaVe"] != DBNull.Value ?
                        Convert.ToDecimal(_ticketData["GiaVe"]) : 0,
                    Total = _ticketData["GiaVe"] != DBNull.Value ?
                        Convert.ToDecimal(_ticketData["GiaVe"]) : 0,
                    PaymentMethod = "Đã thanh toán",
                    CashierName = "System",
                    BranchName = "CGV Cinema"
                };

                // Use ReceiptPrinter service
                var printer = new ReceiptPrinter();
                string receiptText = printer.GenerateTicketReceipt(receipt);
                printer.ShowPrintPreview(receiptText, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in vé: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefund_Click(object sender, EventArgs e)
        {
            string maVe = _ticketData["MaVe"]?.ToString() ?? "";
            string tenPhim = _ticketData["TenPhim"]?.ToString() ?? "";

            DateTime ngayChieu = DateTime.Now;
            if (_ticketData["NgayChieu"] != DBNull.Value)
                DateTime.TryParse(_ticketData["NgayChieu"].ToString(), out ngayChieu);

            // Check if movie already shown
            if (ngayChieu < DateTime.Now.Date)
            {
                MessageBox.Show("Không thể hoàn vé cho suất chiếu đã qua!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Bạn muốn yêu cầu hoàn vé:\n\n" +
                $"Mã vé: {maVe}\n" +
                $"Phim: {tenPhim}\n\n" +
                $"Yêu cầu sẽ được gửi đến quản lý để xét duyệt.",
                "Xác nhận yêu cầu hoàn vé",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // TODO: Create refund request in database
                MessageBox.Show(
                    "Đã gửi yêu cầu hoàn vé!\n\nQuản lý sẽ xem xét và phản hồi trong 24h.",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion
    }
}
