using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.Services;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// Enhanced Payment Form with multiple payment methods (Phase 3)
    /// Supports: Cash, Card, MoMo, ZaloPay
    /// </summary>
    public partial class frmPayment : Form
    {
        // CGV Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvGold = Color.FromArgb(255, 193, 7);

        // Receipt Printer Service
        private readonly ReceiptPrinter _receiptPrinter = new ReceiptPrinter();

        // Payment data
        private string _selectedPaymentMethod = "Cash";
        private decimal _totalAmount = 0;
        private string _orderSummary = "";
        private string _branchName = "CGV Vincom";
        private string _cashierName = "Admin";

        // Ticket data (for receipt)
        private string _movieTitle = "";
        private string _showDate = "";
        private string _showTime = "";
        private string _roomName = "";
        private string _seats = "";
        private int _ticketCount = 0;
        private decimal _ticketPrice = 0;

        // UI Components
        private Panel pnlCash, pnlCard, pnlMomo, pnlZaloPay;
        private Label lblTotalDisplay;
        private TextBox txtCustomerPhone;
        private Label lblChangeAmount;
        private TextBox txtCashReceived;

        public frmPayment()
        {
            InitializeComponent();
            SetupUI();
        }

        public frmPayment(decimal totalAmount, string orderSummary) : this()
        {
            _totalAmount = totalAmount;
            _orderSummary = orderSummary;
            UpdateTotalDisplay();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "frmPayment";
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.Text = "💳 THANH TOÁN";
            this.Size = new Size(550, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = _cgvLightGray;

            // ========== HEADER ==========
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = _cgvRed
            };

            Label lblTitle = new Label
            {
                Text = "💳 THANH TOÁN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(180, 20)
            };
            header.Controls.Add(lblTitle);

            // ========== ORDER SUMMARY PANEL ==========
            Panel orderPanel = new Panel
            {
                Location = new Point(20, 85),
                Size = new Size(495, 80),
                BackColor = Color.White
            };

            Label lblOrderTitle = new Label
            {
                Text = "📋 Thông tin đơn hàng",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(15, 10),
                AutoSize = true
            };

            Label lblOrderDetails = new Label
            {
                Text = _orderSummary.Length > 0 ? _orderSummary : "Vé xem phim + Combo",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(15, 35),
                Size = new Size(300, 35),
                AutoEllipsis = true
            };

            lblTotalDisplay = new Label
            {
                Text = _totalAmount.ToString("N0") + " ₫",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = _cgvRed,
                Location = new Point(320, 25),
                Size = new Size(160, 40),
                TextAlign = ContentAlignment.MiddleRight
            };

            orderPanel.Controls.AddRange(new Control[] { lblOrderTitle, lblOrderDetails, lblTotalDisplay });

            // ========== PAYMENT METHOD SELECTION ==========
            Label lblPaymentMethod = new Label
            {
                Text = "Chọn phương thức thanh toán:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(20, 175),
                AutoSize = true
            };

            // Payment method buttons
            pnlCash = CreatePaymentMethodButton("💵 Tiền mặt", "Cash", new Point(20, 205), true);
            pnlCard = CreatePaymentMethodButton("💳 Thẻ ngân hàng", "Card", new Point(145, 205), false);
            pnlMomo = CreatePaymentMethodButton("📱 MoMo", "MoMo", new Point(270, 205), false);
            pnlZaloPay = CreatePaymentMethodButton("📲 ZaloPay", "ZaloPay", new Point(395, 205), false);

            // ========== CASH PAYMENT DETAILS ==========
            Panel cashDetailsPanel = new Panel
            {
                Location = new Point(20, 280),
                Size = new Size(495, 120),
                BackColor = Color.White,
                Tag = "CashDetails"
            };

            Label lblCashTitle = new Label
            {
                Text = "💵 Thanh toán tiền mặt",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(15, 10),
                AutoSize = true
            };

            Label lblCashReceived = new Label
            {
                Text = "Tiền nhận:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 45),
                AutoSize = true
            };

            txtCashReceived = new TextBox
            {
                Location = new Point(120, 42),
                Size = new Size(150, 30),
                Font = new Font("Segoe UI", 11),
                TextAlign = HorizontalAlignment.Right
            };
            txtCashReceived.TextChanged += TxtCashReceived_TextChanged;

            Label lblChange = new Label
            {
                Text = "Tiền thối:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 80),
                AutoSize = true
            };

            lblChangeAmount = new Label
            {
                Text = "0 ₫",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Green,
                Location = new Point(120, 75),
                AutoSize = true
            };

            // Quick amount buttons
            string[] quickAmounts = { "100K", "200K", "500K", "1M" };
            int[] amounts = { 100000, 200000, 500000, 1000000 };
            int btnX = 290;
            for (int i = 0; i < quickAmounts.Length; i++)
            {
                int amount = amounts[i];
                Button btnQuick = new Button
                {
                    Text = quickAmounts[i],
                    Size = new Size(50, 30),
                    Location = new Point(btnX + i * 52, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(240, 240, 240),
                    Font = new Font("Segoe UI", 8),
                    Cursor = Cursors.Hand
                };
                btnQuick.FlatAppearance.BorderColor = Color.LightGray;
                btnQuick.Click += (s, e) => { txtCashReceived.Text = amount.ToString(); };
                cashDetailsPanel.Controls.Add(btnQuick);
            }

            cashDetailsPanel.Controls.AddRange(new Control[] { lblCashTitle, lblCashReceived, txtCashReceived, lblChange, lblChangeAmount });

            // ========== E-WALLET PAYMENT (MoMo/ZaloPay) ==========
            Panel ewalletPanel = new Panel
            {
                Location = new Point(20, 280),
                Size = new Size(495, 120),
                BackColor = Color.White,
                Tag = "EwalletDetails",
                Visible = false
            };

            PictureBox qrCode = new PictureBox
            {
                Size = new Size(100, 100),
                Location = new Point(15, 10),
                BackColor = Color.FromArgb(240, 240, 240),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            // QR code placeholder
            qrCode.Paint += (s, e) =>
            {
                e.Graphics.DrawString("QR Code", new Font("Segoe UI", 10), Brushes.Gray, 15, 40);
            };

            Label lblScanInfo = new Label
            {
                Text = "📱 Quét mã QR để thanh toán\n\nHoặc chuyển khoản đến:\nCGV CINEMA\n9704 xxxx xxxx xxxx",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(130, 15),
                Size = new Size(350, 90)
            };

            ewalletPanel.Controls.AddRange(new Control[] { qrCode, lblScanInfo });

            // ========== CARD PAYMENT ==========
            Panel cardPanel = new Panel
            {
                Location = new Point(20, 280),
                Size = new Size(495, 120),
                BackColor = Color.White,
                Tag = "CardDetails",
                Visible = false
            };

            Label lblCardInfo = new Label
            {
                Text = "💳 Thanh toán bằng thẻ\n\nVui lòng quẹt thẻ tại máy POS\nHỗ trợ: Visa, MasterCard, JCB, NAPAS",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Location = new Point(15, 15),
                Size = new Size(450, 90)
            };

            cardPanel.Controls.Add(lblCardInfo);

            // ========== CUSTOMER INFO ==========
            Panel customerPanel = new Panel
            {
                Location = new Point(20, 410),
                Size = new Size(495, 60),
                BackColor = Color.White
            };

            Label lblCustomer = new Label
            {
                Text = "📞 SĐT Khách hàng (tích điểm):",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 18),
                AutoSize = true
            };

            txtCustomerPhone = new TextBox
            {
                Location = new Point(230, 15),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 11)
            };

            Button btnLookup = new Button
            {
                Text = "🔍",
                Size = new Size(40, 28),
                Location = new Point(435, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLookup.FlatAppearance.BorderSize = 0;
            btnLookup.Click += (s, e) => LookupCustomer();

            customerPanel.Controls.AddRange(new Control[] { lblCustomer, txtCustomerPhone, btnLookup });

            // ========== ACTION BUTTONS ==========
            Button btnConfirm = new Button
            {
                Text = "✅ XÁC NHẬN THANH TOÁN",
                Size = new Size(240, 50),
                Location = new Point(20, 490),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;

            Button btnCancel = new Button
            {
                Text = "❌ HỦY",
                Size = new Size(120, 50),
                Location = new Point(275, 490),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            Button btnPrintReceipt = new Button
            {
                Text = "🖨️ IN",
                Size = new Size(100, 50),
                Location = new Point(410, 490),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrintReceipt.FlatAppearance.BorderSize = 0;
            btnPrintReceipt.Click += BtnPrintReceipt_Click;

            // ========== STATUS BAR ==========
            Panel statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.FromArgb(50, 50, 50)
            };

            Label lblStatus = new Label
            {
                Text = "💡 Chọn phương thức thanh toán để tiếp tục",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            statusBar.Controls.Add(lblStatus);

            // Add all controls
            this.Controls.Add(header);
            this.Controls.Add(orderPanel);
            this.Controls.Add(lblPaymentMethod);
            this.Controls.Add(pnlCash);
            this.Controls.Add(pnlCard);
            this.Controls.Add(pnlMomo);
            this.Controls.Add(pnlZaloPay);
            this.Controls.Add(cashDetailsPanel);
            this.Controls.Add(ewalletPanel);
            this.Controls.Add(cardPanel);
            this.Controls.Add(customerPanel);
            this.Controls.Add(btnConfirm);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnPrintReceipt);
            this.Controls.Add(statusBar);

            // Store payment detail panels for switching
            this.Tag = new Panel[] { cashDetailsPanel, cardPanel, ewalletPanel };
        }

        private Panel CreatePaymentMethodButton(string text, string method, Point location, bool isSelected)
        {
            Panel panel = new Panel
            {
                Size = new Size(115, 60),
                Location = location,
                BackColor = isSelected ? _cgvRed : Color.White,
                Cursor = Cursors.Hand,
                Tag = method
            };

            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = isSelected ? Color.White : _cgvBlack,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(lbl);

            // Click event
            panel.Click += PaymentMethod_Click;
            lbl.Click += (s, e) => PaymentMethod_Click(panel, e);

            return panel;
        }

        private void PaymentMethod_Click(object sender, EventArgs e)
        {
            Panel clickedPanel = sender as Panel;
            if (clickedPanel == null) return;

            _selectedPaymentMethod = clickedPanel.Tag.ToString();

            // Update button states
            foreach (Control ctrl in new Control[] { pnlCash, pnlCard, pnlMomo, pnlZaloPay })
            {
                if (ctrl is Panel p)
                {
                    bool isSelected = p == clickedPanel;
                    p.BackColor = isSelected ? _cgvRed : Color.White;
                    if (p.Controls.Count > 0 && p.Controls[0] is Label lbl)
                    {
                        lbl.ForeColor = isSelected ? Color.White : _cgvBlack;
                    }
                }
            }

            // Show/hide payment detail panels
            Panel[] detailPanels = this.Tag as Panel[];
            if (detailPanels != null)
            {
                detailPanels[0].Visible = _selectedPaymentMethod == "Cash"; // Cash
                detailPanels[1].Visible = _selectedPaymentMethod == "Card"; // Card
                detailPanels[2].Visible = _selectedPaymentMethod == "MoMo" || _selectedPaymentMethod == "ZaloPay"; // E-wallet
            }
        }

        private void TxtCashReceived_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtCashReceived.Text, out decimal received))
            {
                decimal change = received - _totalAmount;
                if (change >= 0)
                {
                    lblChangeAmount.Text = change.ToString("N0") + " ₫";
                    lblChangeAmount.ForeColor = Color.Green;
                }
                else
                {
                    lblChangeAmount.Text = "Thiếu " + Math.Abs(change).ToString("N0") + " ₫";
                    lblChangeAmount.ForeColor = Color.Red;
                }
            }
            else
            {
                lblChangeAmount.Text = "0 ₫";
                lblChangeAmount.ForeColor = Color.Green;
            }
        }

        private void UpdateTotalDisplay()
        {
            if (lblTotalDisplay != null)
            {
                lblTotalDisplay.Text = _totalAmount.ToString("N0") + " ₫";
            }
        }

        private void LookupCustomer()
        {
            string phone = txtCustomerPhone.Text.Trim();
            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // TODO: Lookup customer from database
            MessageBox.Show($"🔍 Tìm kiếm khách hàng: {phone}\n\n⭐ Điểm tích lũy: 1,250\n💳 Thành viên: Gold", 
                "Thông tin khách hàng", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            // Validate cash payment
            if (_selectedPaymentMethod == "Cash")
            {
                if (!decimal.TryParse(txtCashReceived.Text, out decimal received) || received < _totalAmount)
                {
                    MessageBox.Show("Vui lòng nhập số tiền nhận >= tổng tiền!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Confirm payment
            if (MessageBox.Show($"Xác nhận thanh toán {_totalAmount:N0} ₫ bằng {GetPaymentMethodName()}?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // TODO: Process payment and save to database
                MessageBox.Show("✅ Thanh toán thành công!\n\nMã giao dịch: TXN" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
        }

        private void BtnPrintReceipt_Click(object sender, EventArgs e)
        {
            // Create receipt using ReceiptPrinter service
            var receiptData = new ReceiptPrinter.TicketReceipt
            {
                TransactionId = "TXN" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TransactionDate = DateTime.Now,
                BranchName = _branchName,
                CashierName = _cashierName,
                MovieTitle = !string.IsNullOrEmpty(_movieTitle) ? _movieTitle : "Phim chiếu rạp",
                ShowDate = !string.IsNullOrEmpty(_showDate) ? _showDate : DateTime.Now.ToString("dd/MM/yyyy"),
                ShowTime = !string.IsNullOrEmpty(_showTime) ? _showTime : "19:00",
                RoomName = !string.IsNullOrEmpty(_roomName) ? _roomName : "Phòng 1",
                Seats = !string.IsNullOrEmpty(_seats) ? _seats : "A1, A2",
                TicketCount = _ticketCount > 0 ? _ticketCount : 1,
                TicketPrice = _ticketPrice > 0 ? _ticketPrice : _totalAmount,
                SubTotal = _totalAmount,
                Discount = 0,
                Total = _totalAmount,
                PaymentMethod = GetPaymentMethodName(),
                AmountPaid = decimal.TryParse(txtCashReceived?.Text, out decimal paid) ? paid : _totalAmount,
                Change = decimal.TryParse(txtCashReceived?.Text, out decimal recv) ? Math.Max(0, recv - _totalAmount) : 0,
                CustomerPhone = txtCustomerPhone?.Text?.Trim(),
                PointsEarned = (int)(_totalAmount / 10000)
            };

            string receipt = _receiptPrinter.GenerateTicketReceipt(receiptData);
            _receiptPrinter.ShowPrintPreview(receipt, this);
        }

        private string GenerateReceiptText()
        {
            string line = new string('═', 40);
            return $@"
{line}
        CGV CINEMA VIỆT NAM
       Rạp chiếu phim hàng đầu
{line}
 Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}
 Mã GD: TXN{DateTime.Now:yyyyMMddHHmmss}
{line}
 {_orderSummary ?? "Vé xem phim"}

{line}
 TỔNG CỘNG:     {_totalAmount:N0} VNĐ
 Thanh toán:    {GetPaymentMethodName()}
 Khách đưa:    {txtCashReceived?.Text ?? "0"} VNĐ
 Tiền thối:    {lblChangeAmount?.Text ?? "0 đ"}
{line}
 Cảm ơn quý khách!
 Hotline: 1900 6017
{line}
";
        }

        private string GetPaymentMethodName()
        {
            return _selectedPaymentMethod switch
            {
                "Cash" => "Tiền mặt",
                "Card" => "Thẻ ngân hàng",
                "MoMo" => "Ví MoMo",
                "ZaloPay" => "ZaloPay",
                _ => _selectedPaymentMethod
            };
        }

        // Public method to set order data
        public void SetOrderData(decimal total, string summary)
        {
            _totalAmount = total;
            _orderSummary = summary;
            UpdateTotalDisplay();
        }

        /// <summary>
        /// Set ticket data for receipt printing
        /// </summary>
        public void SetTicketData(string movieTitle, string showDate, string showTime, 
            string roomName, string seats, int ticketCount, decimal ticketPrice)
        {
            _movieTitle = movieTitle;
            _showDate = showDate;
            _showTime = showTime;
            _roomName = roomName;
            _seats = seats;
            _ticketCount = ticketCount;
            _ticketPrice = ticketPrice;
        }

        /// <summary>
        /// Set branch and cashier info
        /// </summary>
        public void SetBranchInfo(string branchName, string cashierName)
        {
            _branchName = branchName;
            _cashierName = cashierName;
        }
    }
}