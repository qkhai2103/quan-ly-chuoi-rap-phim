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
            // Show backup options dialog
            using (Form optionsForm = new Form())
            {
                optionsForm.Text = "💾 Tùy chọn sao lưu";
                optionsForm.Size = new Size(450, 350);
                optionsForm.StartPosition = FormStartPosition.CenterParent;
                optionsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                optionsForm.MaximizeBox = false;
                optionsForm.MinimizeBox = false;
                optionsForm.BackColor = Color.White;

                Label lblTitle = new Label
                {
                    Text = "Chọn phương thức sao lưu:",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Location = new Point(20, 20),
                    AutoSize = true
                };
                optionsForm.Controls.Add(lblTitle);

                // Option 1: Full Database Backup (SQL Server native)
                Button btnSqlBackup = new Button
                {
                    Text = "📥 Backup SQL Server (*.bak)",
                    Size = new Size(390, 50),
                    Location = new Point(20, 60),
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0)
                };
                btnSqlBackup.FlatAppearance.BorderSize = 0;
                btnSqlBackup.Click += (s, ev) =>
                {
                    optionsForm.DialogResult = DialogResult.Yes;
                    optionsForm.Close();
                };
                optionsForm.Controls.Add(btnSqlBackup);

                Label lblNote1 = new Label
                {
                    Text = "⚠️ File .bak được lưu trên SQL Server, cần quyền admin",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(25, 115),
                    AutoSize = true
                };
                optionsForm.Controls.Add(lblNote1);

                // Option 2: Export all data to CSV
                Button btnCsvExport = new Button
                {
                    Text = "📊 Xuất dữ liệu CSV (*.csv)",
                    Size = new Size(390, 50),
                    Location = new Point(20, 150),
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0)
                };
                btnCsvExport.FlatAppearance.BorderSize = 0;
                btnCsvExport.Click += (s, ev) =>
                {
                    optionsForm.DialogResult = DialogResult.No;
                    optionsForm.Close();
                };
                optionsForm.Controls.Add(btnCsvExport);

                Label lblNote2 = new Label
                {
                    Text = "✓ Xuất toàn bộ bảng dữ liệu ra file CSV có thể mở bằng Excel",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(25, 205),
                    AutoSize = true
                };
                optionsForm.Controls.Add(lblNote2);

                // Option 3: Export Schema SQL
                Button btnSchemaExport = new Button
                {
                    Text = "🗃️ Xuất cấu trúc Database (*.sql)",
                    Size = new Size(390, 50),
                    Location = new Point(20, 240),
                    BackColor = Color.FromArgb(155, 89, 182),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0)
                };
                btnSchemaExport.FlatAppearance.BorderSize = 0;
                btnSchemaExport.Click += (s, ev) =>
                {
                    optionsForm.DialogResult = DialogResult.Retry; // Use Retry for 3rd option
                    optionsForm.Close();
                };
                optionsForm.Controls.Add(btnSchemaExport);

                Label lblNote3 = new Label
                {
                    Text = "✓ Tạo script SQL để tái tạo cấu trúc database",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(25, 295),
                    AutoSize = true
                };
                optionsForm.Controls.Add(lblNote3);

                DialogResult result = optionsForm.ShowDialog(this);

                if (result == DialogResult.Yes)
                {
                    // SQL Server Backup
                    PerformSqlBackup();
                }
                else if (result == DialogResult.No)
                {
                    // CSV Export
                    PerformCsvBackup();
                }
                else if (result == DialogResult.Retry)
                {
                    // Schema Export
                    ExportDatabaseSchema();
                }
            }
        }

        private void PerformSqlBackup()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                
                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    
                    string dbName = conn.Database;
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFileName = $"{dbName}_Backup_{timestamp}.bak";
                    
                    // Get default backup location from SQL Server
                    string getBackupPath = @"
                        SELECT SERVERPROPERTY('InstanceDefaultBackupPath') as BackupPath";
                    
                    string backupPath = "";
                    using (SqlCommand pathCmd = new SqlCommand(getBackupPath, conn))
                    {
                        var result = pathCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            backupPath = result.ToString();
                        }
                        else
                        {
                            // Fallback - use SQL Server data directory
                            backupPath = @"C:\SQLBackups";
                        }
                    }

                    string fullBackupPath = Path.Combine(backupPath, backupFileName);
                    
                    // Execute backup command
                    string backupQuery = $@"
                        BACKUP DATABASE [{dbName}] 
                        TO DISK = @BackupPath
                        WITH FORMAT, 
                             MEDIANAME = 'CinemaBackup',
                             NAME = 'Full Backup of {dbName}',
                             COMPRESSION,
                             STATS = 10";

                    using (SqlCommand cmd = new SqlCommand(backupQuery, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes timeout
                        cmd.Parameters.AddWithValue("@BackupPath", fullBackupPath);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show(
                        $"✅ Sao lưu database thành công!\n\n" +
                        $"📁 File: {backupFileName}\n" +
                        $"📍 Vị trí: {backupPath}\n\n" +
                        $"Lưu ý: File backup được lưu trên SQL Server.\n" +
                        $"Liên hệ admin để tải về máy local.",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 3201 || sqlEx.Number == 3013)
                {
                    MessageBox.Show(
                        "⚠️ Không thể tạo backup do quyền truy cập.\n\n" +
                        "SQL Server từ xa (hosting) thường không cho phép backup trực tiếp.\n\n" +
                        "Giải pháp:\n" +
                        "• Sử dụng chức năng 'Xuất dữ liệu CSV' để backup dữ liệu\n" +
                        "• Sử dụng 'Xuất cấu trúc Database' để backup schema\n" +
                        "• Liên hệ nhà cung cấp hosting để backup",
                        "Không có quyền backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Lỗi SQL: {sqlEx.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void PerformCsvBackup()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Chọn thư mục lưu file backup CSV";
                fbd.ShowNewFolderButton = true;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        ExportAllDataToCsv(fbd.SelectedPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void ExportDatabaseSchema()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "SQL Files|*.sql";
                sfd.FileName = $"CinemaDB_Schema_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                sfd.Title = "Chọn nơi lưu file cấu trúc database";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        
                        using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                        {
                            conn.Open();
                            string dbName = conn.Database;

                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            
                            // Header
                            sb.AppendLine("-- ============================================");
                            sb.AppendLine($"-- CGV Cinema Database Schema Export");
                            sb.AppendLine($"-- Database: {dbName}");
                            sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                            sb.AppendLine($"-- Server: {conn.DataSource}");
                            sb.AppendLine("-- ============================================");
                            sb.AppendLine();
                            sb.AppendLine($"USE [{dbName}]");
                            sb.AppendLine("GO");
                            sb.AppendLine();

                            // Get all tables
                            string getTablesQuery = @"
                                SELECT TABLE_NAME 
                                FROM INFORMATION_SCHEMA.TABLES 
                                WHERE TABLE_TYPE = 'BASE TABLE' 
                                ORDER BY TABLE_NAME";

                            DataTable tables = new DataTable();
                            using (SqlDataAdapter adapter = new SqlDataAdapter(getTablesQuery, conn))
                            {
                                adapter.Fill(tables);
                            }

                            foreach (DataRow tableRow in tables.Rows)
                            {
                                string tableName = tableRow["TABLE_NAME"].ToString();
                                sb.AppendLine($"-- ============================================");
                                sb.AppendLine($"-- Table: {tableName}");
                                sb.AppendLine($"-- ============================================");

                                // Get column definitions
                                string getColumnsQuery = $@"
                                    SELECT 
                                        c.COLUMN_NAME,
                                        c.DATA_TYPE,
                                        c.CHARACTER_MAXIMUM_LENGTH,
                                        c.NUMERIC_PRECISION,
                                        c.NUMERIC_SCALE,
                                        c.IS_NULLABLE,
                                        c.COLUMN_DEFAULT,
                                        COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') as IsIdentity
                                    FROM INFORMATION_SCHEMA.COLUMNS c
                                    WHERE c.TABLE_NAME = @TableName
                                    ORDER BY c.ORDINAL_POSITION";

                                DataTable columns = new DataTable();
                                using (SqlCommand cmd = new SqlCommand(getColumnsQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@TableName", tableName);
                                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                                    {
                                        adapter.Fill(columns);
                                    }
                                }

                                sb.AppendLine($"CREATE TABLE [{tableName}] (");

                                for (int i = 0; i < columns.Rows.Count; i++)
                                {
                                    DataRow col = columns.Rows[i];
                                    string colName = col["COLUMN_NAME"].ToString();
                                    string dataType = col["DATA_TYPE"].ToString().ToUpper();
                                    string maxLength = col["CHARACTER_MAXIMUM_LENGTH"]?.ToString();
                                    string precision = col["NUMERIC_PRECISION"]?.ToString();
                                    string scale = col["NUMERIC_SCALE"]?.ToString();
                                    string nullable = col["IS_NULLABLE"].ToString() == "YES" ? "NULL" : "NOT NULL";
                                    string defaultVal = col["COLUMN_DEFAULT"]?.ToString();
                                    bool isIdentity = Convert.ToInt32(col["IsIdentity"]) == 1;

                                    string colDef = $"    [{colName}] {dataType}";

                                    // Add type length/precision
                                    if (!string.IsNullOrEmpty(maxLength) && maxLength != "-1")
                                    {
                                        colDef += $"({maxLength})";
                                    }
                                    else if (maxLength == "-1")
                                    {
                                        colDef += "(MAX)";
                                    }
                                    else if (!string.IsNullOrEmpty(precision) && dataType == "DECIMAL")
                                    {
                                        colDef += $"({precision},{scale})";
                                    }

                                    // Add IDENTITY
                                    if (isIdentity)
                                    {
                                        colDef += " IDENTITY(1,1)";
                                    }

                                    // Add NULL/NOT NULL
                                    colDef += $" {nullable}";

                                    // Add default value
                                    if (!string.IsNullOrEmpty(defaultVal))
                                    {
                                        colDef += $" DEFAULT {defaultVal}";
                                    }

                                    // Add comma except for last column
                                    if (i < columns.Rows.Count - 1)
                                    {
                                        colDef += ",";
                                    }

                                    sb.AppendLine(colDef);
                                }

                                sb.AppendLine(")");
                                sb.AppendLine("GO");
                                sb.AppendLine();

                                // Get Primary Keys
                                string getPKQuery = $@"
                                    SELECT kc.COLUMN_NAME
                                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                                    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc 
                                        ON tc.CONSTRAINT_NAME = kc.CONSTRAINT_NAME
                                    WHERE tc.TABLE_NAME = @TableName 
                                        AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                                    ORDER BY kc.ORDINAL_POSITION";

                                DataTable pkCols = new DataTable();
                                using (SqlCommand cmd = new SqlCommand(getPKQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@TableName", tableName);
                                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                                    {
                                        adapter.Fill(pkCols);
                                    }
                                }

                                if (pkCols.Rows.Count > 0)
                                {
                                    var pkColNames = new System.Collections.Generic.List<string>();
                                    foreach (DataRow pk in pkCols.Rows)
                                    {
                                        pkColNames.Add($"[{pk["COLUMN_NAME"]}]");
                                    }
                                    sb.AppendLine($"ALTER TABLE [{tableName}] ADD PRIMARY KEY ({string.Join(", ", pkColNames)})");
                                    sb.AppendLine("GO");
                                    sb.AppendLine();
                                }
                            }

                            // Get Foreign Keys
                            sb.AppendLine("-- ============================================");
                            sb.AppendLine("-- Foreign Key Constraints");
                            sb.AppendLine("-- ============================================");

                            string getFKQuery = @"
                                SELECT 
                                    fk.name AS FK_Name,
                                    tp.name AS Parent_Table,
                                    cp.name AS Parent_Column,
                                    tr.name AS Referenced_Table,
                                    cr.name AS Referenced_Column
                                FROM sys.foreign_keys fk
                                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                                INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
                                INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
                                INNER JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
                                INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
                                ORDER BY tp.name, fk.name";

                            DataTable fks = new DataTable();
                            using (SqlDataAdapter adapter = new SqlDataAdapter(getFKQuery, conn))
                            {
                                adapter.Fill(fks);
                            }

                            foreach (DataRow fk in fks.Rows)
                            {
                                sb.AppendLine($"ALTER TABLE [{fk["Parent_Table"]}] ADD CONSTRAINT [{fk["FK_Name"]}]");
                                sb.AppendLine($"    FOREIGN KEY ([{fk["Parent_Column"]}])");
                                sb.AppendLine($"    REFERENCES [{fk["Referenced_Table"]}] ([{fk["Referenced_Column"]}])");
                                sb.AppendLine("GO");
                                sb.AppendLine();
                            }

                            // Write to file
                            File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);

                            MessageBox.Show(
                                $"✅ Xuất cấu trúc database thành công!\n\n" +
                                $"📁 File: {Path.GetFileName(sfd.FileName)}\n" +
                                $"📊 {tables.Rows.Count} bảng\n" +
                                $"🔗 {fks.Rows.Count} foreign keys",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Open folder
                            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\"");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xuất schema: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            DialogResult confirm = MessageBox.Show(
                "⚠️ CẢNH BÁO: Khôi phục database sẽ ghi đè toàn bộ dữ liệu hiện tại!\n\n" +
                "Bạn có chắc chắn muốn tiếp tục?",
                "Xác nhận khôi phục", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Backup Files|*.bak|All Files|*.*";
                ofd.Title = "Chọn file backup để khôi phục";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        // Note: Restore requires exclusive access and typically admin permissions
                        MessageBox.Show(
                            "⚠️ Với SQL Server từ xa (hosting), việc restore thường cần:\n\n" +
                            "• Upload file .bak lên server\n" +
                            "• Thực hiện RESTORE thông qua hosting control panel\n" +
                            "• Hoặc liên hệ admin hệ thống\n\n" +
                            "File backup đã chọn:\n" + ofd.FileName,
                            "Hướng dẫn khôi phục", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // If local SQL Server, attempt restore
                        if (MessageBox.Show(
                            "Bạn có đang sử dụng SQL Server local và muốn thử khôi phục tự động?",
                            "Thử khôi phục local", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            PerformRestore(ofd.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khôi phục: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void PerformRestore(string backupFilePath)
        {
            try
            {
                // Connect to master database for restore operation
                string masterConnString = DatabaseConfig.ConnectionString.Replace(
                    "Initial Catalog=db_ac1f20_khaideptrai", 
                    "Initial Catalog=master");

                using (SqlConnection conn = new SqlConnection(masterConnString))
                {
                    conn.Open();
                    string dbName = "db_ac1f20_khaideptrai";

                    // Set database to single-user mode
                    string singleUserQuery = $@"
                        ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    
                    using (SqlCommand cmd = new SqlCommand(singleUserQuery, conn))
                    {
                        cmd.CommandTimeout = 60;
                        cmd.ExecuteNonQuery();
                    }

                    // Restore database
                    string restoreQuery = $@"
                        RESTORE DATABASE [{dbName}] 
                        FROM DISK = @BackupPath
                        WITH REPLACE, RECOVERY";

                    using (SqlCommand cmd = new SqlCommand(restoreQuery, conn))
                    {
                        cmd.CommandTimeout = 600; // 10 minutes
                        cmd.Parameters.AddWithValue("@BackupPath", backupFilePath);
                        cmd.ExecuteNonQuery();
                    }

                    // Set database back to multi-user mode
                    string multiUserQuery = $@"
                        ALTER DATABASE [{dbName}] SET MULTI_USER";

                    using (SqlCommand cmd = new SqlCommand(multiUserQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show(
                        "✅ Khôi phục database thành công!\n\n" +
                        "Vui lòng đăng xuất và đăng nhập lại để áp dụng thay đổi.",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(
                    $"❌ Không thể khôi phục database.\n\n" +
                    $"Lỗi: {sqlEx.Message}\n\n" +
                    "Nguyên nhân có thể:\n" +
                    "• Không có quyền ALTER DATABASE\n" +
                    "• File backup không tương thích\n" +
                    "• SQL Server từ xa không hỗ trợ",
                    "Lỗi khôi phục", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
