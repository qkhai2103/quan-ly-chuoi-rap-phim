using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.Services
{
    /// <summary>
    /// Receipt Printing Service for CGV Cinema (Phase 3)
    /// Supports: Thermal printer (80mm), A4 printing, PDF export
    /// </summary>
    public class ReceiptPrinter
    {
        #region Properties

        /// <summary>
        /// Receipt width in characters (default for 80mm thermal printer)
        /// </summary>
        public int ReceiptWidth { get; set; } = 42;

        /// <summary>
        /// Company name to display on receipt
        /// </summary>
        public string CompanyName { get; set; } = "CGV CINEMA VIỆT NAM";

        /// <summary>
        /// Company tagline
        /// </summary>
        public string Tagline { get; set; } = "Rạp chiếu phim hàng đầu";

        /// <summary>
        /// Hotline number
        /// </summary>
        public string Hotline { get; set; } = "1900 6017";

        /// <summary>
        /// Branch name (set per transaction)
        /// </summary>
        public string BranchName { get; set; } = "";

        #endregion

        #region Receipt Data Model

        public class TicketReceipt
        {
            public string TransactionId { get; set; }
            public DateTime TransactionDate { get; set; } = DateTime.Now;
            public string BranchName { get; set; }
            public string CashierName { get; set; }

            // Movie info
            public string MovieTitle { get; set; }
            public string ShowDate { get; set; }
            public string ShowTime { get; set; }
            public string RoomName { get; set; }
            public string Seats { get; set; }
            public int TicketCount { get; set; }
            public decimal TicketPrice { get; set; }

            // Concessions (optional)
            public string[] ConcessionItems { get; set; }
            public decimal[] ConcessionPrices { get; set; }
            public decimal ConcessionTotal { get; set; }

            // Payment
            public decimal SubTotal { get; set; }
            public decimal Discount { get; set; }
            public decimal Total { get; set; }
            public string PaymentMethod { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal Change { get; set; }

            // Customer
            public string CustomerPhone { get; set; }
            public int PointsEarned { get; set; }
        }

        #endregion

        #region Generate Receipt Text

        /// <summary>
        /// Generate formatted receipt text for thermal printer (80mm)
        /// </summary>
        public string GenerateTicketReceipt(TicketReceipt receipt)
        {
            StringBuilder sb = new StringBuilder();
            string line = new string('═', ReceiptWidth);
            string thinLine = new string('─', ReceiptWidth);

            // Header
            sb.AppendLine(CenterText(CompanyName));
            sb.AppendLine(CenterText(Tagline));
            sb.AppendLine(line);

            // Transaction info
            sb.AppendLine($"Chi nhánh: {receipt.BranchName}");
            sb.AppendLine($"Ngày: {receipt.TransactionDate:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Mã GD: {receipt.TransactionId}");
            sb.AppendLine($"Thu ngân: {receipt.CashierName}");
            sb.AppendLine(thinLine);

            // Movie details
            sb.AppendLine(CenterText("🎬 THÔNG TIN VÉ"));
            sb.AppendLine(thinLine);
            sb.AppendLine($"Phim: {TruncateText(receipt.MovieTitle, ReceiptWidth - 6)}");
            sb.AppendLine($"Ngày chiếu: {receipt.ShowDate}");
            sb.AppendLine($"Suất chiếu: {receipt.ShowTime}");
            sb.AppendLine($"Phòng: {receipt.RoomName}");
            sb.AppendLine($"Ghế: {receipt.Seats}");
            sb.AppendLine($"Số vé: {receipt.TicketCount} x {receipt.TicketPrice:N0}đ");
            sb.AppendLine(thinLine);

            // Concessions (if any)
            if (receipt.ConcessionItems != null && receipt.ConcessionItems.Length > 0)
            {
                sb.AppendLine(CenterText("🍿 BẮP NƯỚC"));
                sb.AppendLine(thinLine);
                for (int i = 0; i < receipt.ConcessionItems.Length; i++)
                {
                    string item = receipt.ConcessionItems[i];
                    decimal price = receipt.ConcessionPrices[i];
                    sb.AppendLine(FormatLineItem(item, price));
                }
                sb.AppendLine(thinLine);
            }

            // Totals
            sb.AppendLine(FormatLineItem("Tạm tính:", receipt.SubTotal));
            if (receipt.Discount > 0)
            {
                sb.AppendLine(FormatLineItem("Giảm giá:", -receipt.Discount));
            }
            sb.AppendLine(line);
            sb.AppendLine(FormatLineItem("TỔNG CỘNG:", receipt.Total, true));
            sb.AppendLine(line);

            // Payment
            sb.AppendLine($"Thanh toán: {receipt.PaymentMethod}");
            sb.AppendLine(FormatLineItem("Tiền nhận:", receipt.AmountPaid));
            sb.AppendLine(FormatLineItem("Tiền thối:", receipt.Change));
            sb.AppendLine(thinLine);

            // Customer info (if available)
            if (!string.IsNullOrEmpty(receipt.CustomerPhone))
            {
                sb.AppendLine($"SĐT: {receipt.CustomerPhone}");
                sb.AppendLine($"Điểm tích: +{receipt.PointsEarned}");
                sb.AppendLine(thinLine);
            }

            // Footer
            sb.AppendLine();
            sb.AppendLine(CenterText("Cảm ơn quý khách!"));
            sb.AppendLine(CenterText($"Hotline: {Hotline}"));
            sb.AppendLine(line);

            // Barcode placeholder (transaction ID)
            sb.AppendLine(CenterText($"|||{receipt.TransactionId}|||"));

            return sb.ToString();
        }

        /// <summary>
        /// Generate a simple daily summary receipt
        /// </summary>
        public string GenerateDailySummary(string branchName, DateTime date, int ticketsSold, decimal ticketRevenue, 
            int concessionsSold, decimal concessionRevenue, decimal totalRevenue)
        {
            StringBuilder sb = new StringBuilder();
            string line = new string('═', ReceiptWidth);

            sb.AppendLine(CenterText(CompanyName));
            sb.AppendLine(CenterText("BÁO CÁO NGÀY"));
            sb.AppendLine(line);
            sb.AppendLine($"Chi nhánh: {branchName}");
            sb.AppendLine($"Ngày: {date:dd/MM/yyyy}");
            sb.AppendLine($"In lúc: {DateTime.Now:HH:mm:ss}");
            sb.AppendLine(line);
            sb.AppendLine();
            sb.AppendLine(FormatLineItem("Vé đã bán:", ticketsSold.ToString()));
            sb.AppendLine(FormatLineItem("Doanh thu vé:", ticketRevenue));
            sb.AppendLine();
            sb.AppendLine(FormatLineItem("Bắp nước bán:", concessionsSold.ToString()));
            sb.AppendLine(FormatLineItem("Doanh thu F&B:", concessionRevenue));
            sb.AppendLine(line);
            sb.AppendLine(FormatLineItem("TỔNG DOANH THU:", totalRevenue, true));
            sb.AppendLine(line);

            return sb.ToString();
        }

        #endregion

        #region Print Methods

        /// <summary>
        /// Print receipt to default printer
        /// </summary>
        public bool PrintReceipt(string receiptText)
        {
            try
            {
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrintPage += (sender, e) =>
                {
                    Font font = new Font("Consolas", 10);
                    e.Graphics.DrawString(receiptText, font, Brushes.Black, 10, 10);
                };

                PrintDialog dialog = new PrintDialog();
                dialog.Document = printDoc;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReceiptPrinter] Print Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Print receipt silently to specified printer
        /// </summary>
        public bool PrintSilent(string receiptText, string printerName)
        {
            try
            {
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = printerName;
                printDoc.PrintPage += (sender, e) =>
                {
                    Font font = new Font("Consolas", 10);
                    e.Graphics.DrawString(receiptText, font, Brushes.Black, 10, 10);
                };

                printDoc.Print();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReceiptPrinter] Silent Print Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Preview receipt in a form before printing
        /// </summary>
        public void ShowPrintPreview(string receiptText, Form parent = null)
        {
            using (Form previewForm = new Form())
            {
                previewForm.Text = "🖨️ Xem trước hóa đơn";
                previewForm.Size = new Size(450, 600);
                previewForm.StartPosition = parent != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
                previewForm.BackColor = Color.White;
                previewForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                previewForm.MaximizeBox = false;

                TextBox txtReceipt = new TextBox
                {
                    Text = receiptText,
                    Multiline = true,
                    ReadOnly = true,
                    Font = new Font("Consolas", 10),
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Vertical,
                    BackColor = Color.White
                };

                Panel buttonPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                Button btnPrint = new Button
                {
                    Text = "🖨️ IN",
                    Size = new Size(100, 40),
                    Location = new Point(110, 5),
                    BackColor = Color.FromArgb(226, 26, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnPrint.FlatAppearance.BorderSize = 0;
                btnPrint.Click += (s, e) =>
                {
                    if (PrintReceipt(receiptText))
                    {
                        MessageBox.Show("✅ Đã gửi lệnh in!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                Button btnSave = new Button
                {
                    Text = "💾 LƯU",
                    Size = new Size(100, 40),
                    Location = new Point(220, 5),
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnSave.FlatAppearance.BorderSize = 0;
                btnSave.Click += (s, e) =>
                {
                    SaveReceiptToFile(receiptText);
                };

                buttonPanel.Controls.AddRange(new Control[] { btnPrint, btnSave });

                previewForm.Controls.Add(txtReceipt);
                previewForm.Controls.Add(buttonPanel);

                if (parent != null)
                    previewForm.ShowDialog(parent);
                else
                    previewForm.ShowDialog();
            }
        }

        #endregion

        #region Export Methods

        /// <summary>
        /// Save receipt to text file
        /// </summary>
        public bool SaveReceiptToFile(string receiptText, string filePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                        sfd.FileName = $"Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            filePath = sfd.FileName;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                File.WriteAllText(filePath, receiptText, Encoding.UTF8);
                MessageBox.Show($"✅ Đã lưu hóa đơn: {filePath}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Get list of available printers
        /// </summary>
        public string[] GetAvailablePrinters()
        {
            var printers = new System.Collections.Generic.List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
            return printers.ToArray();
        }

        #endregion

        #region Helper Methods

        private string CenterText(string text)
        {
            if (text.Length >= ReceiptWidth)
                return text.Substring(0, ReceiptWidth);

            int padding = (ReceiptWidth - text.Length) / 2;
            return text.PadLeft(padding + text.Length).PadRight(ReceiptWidth);
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        private string FormatLineItem(string label, decimal value, bool bold = false)
        {
            string valueStr = value.ToString("N0") + "đ";
            int spaces = ReceiptWidth - label.Length - valueStr.Length;
            if (spaces < 1) spaces = 1;
            return label + new string(' ', spaces) + valueStr;
        }

        private string FormatLineItem(string label, string value)
        {
            int spaces = ReceiptWidth - label.Length - value.Length;
            if (spaces < 1) spaces = 1;
            return label + new string(' ', spaces) + value;
        }

        #endregion
    }
}
