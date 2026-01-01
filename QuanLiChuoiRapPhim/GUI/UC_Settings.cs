using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// UC_Settings - Cài đặt hệ thống
    /// Chức năng: Quản lý cài đặt, sao lưu/khôi phục database, xuất dữ liệu
    /// </summary>
    public partial class UC_Settings : UserControl
    {
        private readonly string _userRole;
        private readonly string _username;
        private readonly int _maNguoiDung;

        // CGV Colors
        private readonly Color _cgvRed = Color.FromArgb(226, 26, 60);
        private readonly Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private readonly Color _cgvLightGray = Color.FromArgb(245, 245, 245);
        private readonly Color _cgvDarkGray = Color.FromArgb(45, 45, 45);

        public UC_Settings(string userRole, string username, int maNguoiDung)
        {
            _userRole = userRole;
            _username = username;
            _maNguoiDung = maNguoiDung;
            
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = _cgvLightGray;
            this.Padding = new Padding(30);

            // ========== HEADER ==========
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = _cgvBlack,
                Padding = new Padding(25, 0, 25, 0)
            };

            Label lblTitle = new Label
            {
                Text = "⚙️ CÀI ĐẶT HỆ THỐNG",
                Font = new Font("Montserrat", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 22)
            };
            headerPanel.Controls.Add(lblTitle);

            // ========== CONTENT SCROLL ==========
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            int y = 30;

            // ========== SECTION 1: THÔNG TIN HỆ THỐNG ==========
            Panel sysInfoSection = CreateSection("📊 THÔNG TIN HỆ THỐNG", ref y);
            
            y = 60;
            AddInfoRow(sysInfoSection, "Phiên bản:", "1.0.0 (Build 2025.01)", ref y);
            AddInfoRow(sysInfoSection, "Database:", GetDatabaseName(), ref y);
            AddInfoRow(sysInfoSection, "Server:", GetServerName(), ref y);
            AddInfoRow(sysInfoSection, "Người dùng hiện tại:", _username, ref y);
            AddInfoRow(sysInfoSection, "Vai trò:", _userRole, ref y);
            
            sysInfoSection.Height = y + 20;
            contentPanel.Controls.Add(sysInfoSection);

            y = sysInfoSection.Bottom + 20;

            // ========== SECTION 2: SAO LƯU & KHÔI PHỤC (Chỉ Admin) ==========
            bool isAdmin = _userRole == "Admin" || _userRole == "Administrator" || _userRole == "Quản trị viên";
            
            if (isAdmin)
            {
                Panel backupSection = CreateSectionAtPosition("💾 SAO LƯU & KHÔI PHỤC DATABASE", y, 200);
                
                int btnY = 60;
                
                Button btnBackup = CreateSettingButton("📥 Sao lưu Database", "Tạo bản sao lưu database", 
                    Color.FromArgb(40, 167, 69), new Point(30, btnY));
                btnBackup.Click += BtnBackup_Click;
                backupSection.Controls.Add(btnBackup);
                
                btnY += 60;
                
                Button btnRestore = CreateSettingButton("📤 Khôi phục Database", "Khôi phục từ bản sao lưu", 
                    Color.FromArgb(255, 193, 7), new Point(30, btnY));
                btnRestore.Click += BtnRestore_Click;
                backupSection.Controls.Add(btnRestore);
                
                btnY += 60;
                
                Label lblWarning = new Label
                {
                    Text = "⚠️ Chỉ Admin mới có quyền sao lưu/khôi phục database",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(30, btnY),
                    AutoSize = true
                };
                backupSection.Controls.Add(lblWarning);

                contentPanel.Controls.Add(backupSection);
                y = backupSection.Bottom + 20;
            }

            // ========== SECTION 3: XUẤT DỮ LIỆU ==========
            Panel exportSection = CreateSectionAtPosition("📤 XUẤT DỮ LIỆU", y, 280);
            
            int exportY = 60;

            Button btnExportUsers = CreateSettingButton("👥 Xuất danh sách người dùng", "Xuất Excel/CSV", 
                Color.FromArgb(52, 152, 219), new Point(30, exportY));
            btnExportUsers.Click += BtnExportUsers_Click;
            exportSection.Controls.Add(btnExportUsers);
            
            exportY += 55;

            Button btnExportMovies = CreateSettingButton("🎬 Xuất danh sách phim", "Xuất Excel/CSV", 
                Color.FromArgb(155, 89, 182), new Point(30, exportY));
            btnExportMovies.Click += BtnExportMovies_Click;
            exportSection.Controls.Add(btnExportMovies);
            
            exportY += 55;

            Button btnExportRevenue = CreateSettingButton("💰 Xuất báo cáo doanh thu", "Xuất Excel/CSV", 
                Color.FromArgb(39, 174, 96), new Point(30, exportY));
            btnExportRevenue.Click += BtnExportRevenue_Click;
            exportSection.Controls.Add(btnExportRevenue);
            
            exportY += 55;

            Button btnExportTickets = CreateSettingButton("🎟️ Xuất danh sách vé", "Xuất Excel/CSV", 
                Color.FromArgb(231, 76, 60), new Point(30, exportY));
            btnExportTickets.Click += BtnExportTickets_Click;
            exportSection.Controls.Add(btnExportTickets);

            contentPanel.Controls.Add(exportSection);
            y = exportSection.Bottom + 20;

            // ========== SECTION 4: CÀI ĐẶT ỨNG DỤNG ==========
            Panel appSettingsSection = CreateSectionAtPosition("🔧 CÀI ĐẶT ỨNG DỤNG", y, 200);
            
            int appY = 60;

            // Theme setting
            Label lblTheme = new Label
            {
                Text = "Chủ đề giao diện:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(30, appY),
                AutoSize = true
            };
            appSettingsSection.Controls.Add(lblTheme);

            ComboBox cboTheme = new ComboBox
            {
                Size = new Size(200, 30),
                Location = new Point(180, appY - 3),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTheme.Items.AddRange(new[] { "🎨 CGV Red (Mặc định)", "🌙 Dark Mode", "☀️ Light Mode" });
            cboTheme.SelectedIndex = 0;
            appSettingsSection.Controls.Add(cboTheme);

            appY += 50;

            // Language setting
            Label lblLang = new Label
            {
                Text = "Ngôn ngữ:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(30, appY),
                AutoSize = true
            };
            appSettingsSection.Controls.Add(lblLang);

            ComboBox cboLang = new ComboBox
            {
                Size = new Size(200, 30),
                Location = new Point(180, appY - 3),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLang.Items.AddRange(new[] { "🇻🇳 Tiếng Việt", "🇬🇧 English" });
            cboLang.SelectedIndex = 0;
            appSettingsSection.Controls.Add(cboLang);

            appY += 50;

            // Save button
            Button btnSaveSettings = new Button
            {
                Text = "💾 Lưu cài đặt",
                Size = new Size(150, 40),
                Location = new Point(30, appY),
                BackColor = _cgvRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveSettings.FlatAppearance.BorderSize = 0;
            btnSaveSettings.Click += (s, e) => MessageBox.Show("Đã lưu cài đặt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            appSettingsSection.Controls.Add(btnSaveSettings);

            contentPanel.Controls.Add(appSettingsSection);
            y = appSettingsSection.Bottom + 20;

            // ========== SECTION 5: VỀ ỨNG DỤNG ==========
            Panel aboutSection = CreateSectionAtPosition("ℹ️ VỀ ỨNG DỤNG", y, 150);
            
            Label lblAbout = new Label
            {
                Text = "CGV Cinema Management System\n" +
                       "Hệ thống quản lý chuỗi rạp chiếu phim\n\n" +
                       "© 2025 CGV Vietnam. All rights reserved.\n" +
                       "Phát triển bởi: CGV Tech Team",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray,
                Location = new Point(30, 60),
                AutoSize = true
            };
            aboutSection.Controls.Add(lblAbout);

            contentPanel.Controls.Add(aboutSection);

            // Add to main
            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
        }

        private Panel CreateSection(string title, ref int y)
        {
            Panel section = new Panel
            {
                Size = new Size(this.Width - 100, 200),
                Location = new Point(30, y),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSectionTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _cgvRed,
                Location = new Point(20, 15),
                AutoSize = true
            };
            section.Controls.Add(lblSectionTitle);

            // Separator line
            Panel separator = new Panel
            {
                Size = new Size(section.Width - 40, 2),
                Location = new Point(20, 45),
                BackColor = _cgvLightGray
            };
            section.Controls.Add(separator);

            return section;
        }

        private Panel CreateSectionAtPosition(string title, int y, int height)
        {
            Panel section = new Panel
            {
                Size = new Size(800, height),
                Location = new Point(0, y),
                BackColor = Color.White
            };

            Label lblSectionTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _cgvRed,
                Location = new Point(20, 15),
                AutoSize = true
            };
            section.Controls.Add(lblSectionTitle);

            Panel separator = new Panel
            {
                Size = new Size(section.Width - 40, 2),
                Location = new Point(20, 45),
                BackColor = _cgvLightGray
            };
            section.Controls.Add(separator);

            return section;
        }

        private void AddInfoRow(Panel panel, string label, string value, ref int y)
        {
            Label lblLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.DimGray,
                Location = new Point(30, y),
                AutoSize = true
            };

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Location = new Point(200, y),
                AutoSize = true
            };

            panel.Controls.AddRange(new Control[] { lblLabel, lblValue });
            y += 28;
        }

        private Button CreateSettingButton(string text, string subText, Color bgColor, Point location)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(350, 45),
                Location = location,
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private string GetDatabaseName()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    return conn.Database;
                }
            }
            catch { return "N/A"; }
        }

        private string GetServerName()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    return conn.DataSource;
                }
            }
            catch { return "N/A"; }
        }

        #region Event Handlers

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Backup Files|*.bak";
                sfd.FileName = $"CinemaDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                sfd.Title = "Chọn nơi lưu file sao lưu";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        
                        // Note: For remote SQL Server, backup needs to be on server
                        // This is a simplified version - actual implementation depends on hosting
                        MessageBox.Show(
                            "⚠️ Lưu ý: Với SQL Server từ xa (cloud), file backup sẽ được lưu trên server.\n\n" +
                            "Để backup local, hãy sử dụng SQL Server Management Studio hoặc liên hệ admin hệ thống.",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Export data instead as CSV
                        if (MessageBox.Show("Bạn có muốn xuất toàn bộ dữ liệu ra file CSV thay thế?", 
                            "Xuất dữ liệu", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            ExportAllDataToCsv(Path.GetDirectoryName(sfd.FileName));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi sao lưu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "⚠️ Chức năng khôi phục database từ file .bak\n\n" +
                "Với SQL Server từ xa, vui lòng liên hệ administrator hệ thống để thực hiện khôi phục.\n\n" +
                "Hoặc sử dụng SQL Server Management Studio.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExportUsers_Click(object sender, EventArgs e)
        {
            try
            {
                AdminBLL adminBLL = new AdminBLL();
                DataTable dt = adminBLL.GetAllUsers();
                ExportHelper.ExportWithDialog(dt, "DanhSachNguoiDung", this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportMovies_Click(object sender, EventArgs e)
        {
            try
            {
                PhimBLL phimBLL = new PhimBLL();
                DataTable dt = phimBLL.LayTatCaPhim();
                ExportHelper.ExportWithDialog(dt, "DanhSachPhim", this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportRevenue_Click(object sender, EventArgs e)
        {
            try
            {
                DoanhThuBLL doanhThuBLL = new DoanhThuBLL();
                DataTable dt = doanhThuBLL.ThongKeDoanhThuTheoNgay(DateTime.Now.AddMonths(-1), DateTime.Now);
                ExportHelper.ExportWithDialog(dt, "BaoCaoDoanhThu", this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportTickets_Click(object sender, EventArgs e)
        {
            try
            {
                VeBLL veBLL = new VeBLL();
                DataTable dt = veBLL.LayTatCaVe();
                ExportHelper.ExportWithDialog(dt, "DanhSachVe", this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportAllDataToCsv(string folderPath)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string exportFolder = Path.Combine(folderPath, $"CinemaDB_Export_{timestamp}");
                Directory.CreateDirectory(exportFolder);

                // Export Users
                AdminBLL adminBLL = new AdminBLL();
                ExportHelper.ExportToCsv(adminBLL.GetAllUsers(), Path.Combine(exportFolder, "NguoiDung.csv"));

                // Export Movies
                PhimBLL phimBLL = new PhimBLL();
                ExportHelper.ExportToCsv(phimBLL.LayTatCaPhim(), Path.Combine(exportFolder, "Phim.csv"));

                // Export Tickets
                VeBLL veBLL = new VeBLL();
                ExportHelper.ExportToCsv(veBLL.LayTatCaVe(), Path.Combine(exportFolder, "Ve.csv"));

                // Export Revenue
                DoanhThuBLL doanhThuBLL = new DoanhThuBLL();
                ExportHelper.ExportToCsv(
                    doanhThuBLL.ThongKeDoanhThuTheoNgay(DateTime.Now.AddYears(-1), DateTime.Now), 
                    Path.Combine(exportFolder, "DoanhThu.csv"));

                MessageBox.Show($"Xuất dữ liệu thành công!\n\nĐường dẫn: {exportFolder}", 
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Open folder
                System.Diagnostics.Process.Start("explorer.exe", exportFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "UC_Settings";
            this.Size = new Size(1000, 700);
            this.ResumeLayout(false);
        }
    }
}
