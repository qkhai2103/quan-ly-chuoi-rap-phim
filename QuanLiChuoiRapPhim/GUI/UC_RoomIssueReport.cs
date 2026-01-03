using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.GUI
{
    /// <summary>
    /// User Control for Branch Managers to report room/equipment issues
    /// </summary>
    public class UC_RoomIssueReport : UserControl
    {
        // Modern Color Palette (Matches UC_TicketSales)
        private readonly Color _primary = Color.FromArgb(226, 26, 60);       // CGV Red
        private readonly Color _primaryDark = Color.FromArgb(180, 20, 50);   // Darker Red
        private readonly Color _dark = Color.FromArgb(25, 25, 30);           // Almost Black
        private readonly Color _surface = Color.FromArgb(255, 255, 255);     // White
        private readonly Color _surfaceAlt = Color.FromArgb(248, 249, 252);  // Light Gray
        private readonly Color _border = Color.FromArgb(230, 232, 240);      // Border Gray
        private readonly Color _textPrimary = Color.FromArgb(30, 30, 35);    // Text Dark
        private readonly Color _textSecondary = Color.FromArgb(120, 125, 140);// Text Gray
        private readonly Color _success = Color.FromArgb(34, 197, 94);       // Green
        private readonly Color _warning = Color.FromArgb(241, 196, 15);      // Yellow
        private readonly Color _info = Color.FromArgb(52, 152, 219);         // Blue

        private string _branchName;
        private int _userId;
        private int _maChiNhanh;
        private bool _isManager; // Role flag
        private DataGridView _dgvIssues;
        private DataTable _dtIssues;
        private BaoCaoSuCoDAL _baoCaoDAL;

        private FlowLayoutPanel _statsPanel;
        private ComboBox _cboFilter;
        private ComboBox _cboRoom;
        
        // Validation and Image Upload Support
        private ErrorProvider _errorProvider;
        private List<string> _uploadedImagePaths = new List<string>();
        private const long MAX_IMAGE_SIZE = 5 * 1024 * 1024; // 5MB
        
        // Search & Filter Support (Priority 2)
        private TextBox _txtSearch;
        private System.Windows.Forms.Timer _searchDebounceTimer;
        private Panel _emptyStatePanel;

        public UC_RoomIssueReport(string branchName, int userId, int maChiNhanh = 0, bool isManager = false)
        {
            _branchName = branchName;
            _userId = userId;
            _maChiNhanh = maChiNhanh;
            _isManager = isManager;
            _baoCaoDAL = new BaoCaoSuCoDAL();
            
            // Initialize ErrorProvider
            _errorProvider = new ErrorProvider
            {
                BlinkStyle = ErrorBlinkStyle.NeverBlink,
                Icon = SystemIcons.Error
            };
            
            InitializeComponent();
            LoadIssues();
        }

        #region UI Setup

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = _surfaceAlt;
            this.Padding = new Padding(20);

            // 1. Header Section
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 10)
            };

            Label lblTitle = new Label
            {
                Text = "🔧 BÁO CÁO SỰ CỐ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = _primary,
                AutoSize = true,
                Location = new Point(0, 10)
            };
            headerPanel.Controls.Add(lblTitle);
            
            // 2. Stats Section
            _statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 20)
            };
            // Add initial placeholders, will be updated by UpdateStats
            _statsPanel.Controls.Add(CreateStatCard("Tổng báo cáo", "0", _info, "total"));
            _statsPanel.Controls.Add(CreateStatCard("Chờ xử lý", "0", _primary, "pending"));
            _statsPanel.Controls.Add(CreateStatCard("Đang sửa", "0", _warning, "inprogress"));
            _statsPanel.Controls.Add(CreateStatCard("Đã xong", "0", _success, "resolved"));

            // 3. Toolbar Section
            FlowLayoutPanel toolbarPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                AutoSize = false
            };

            // Filter by Status
            _cboFilter = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 5, 10, 0)
            };
            _cboFilter.Items.AddRange(new[] { "Tất cả trạng thái", "Chờ xử lý", "Đang sửa", "Đã xong" });
            _cboFilter.SelectedIndex = 0;
            _cboFilter.SelectedIndexChanged += (s, e) => FilterIssues();
            toolbarPanel.Controls.Add(_cboFilter);

            // Filter by Room
            _cboRoom = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 5, 10, 0)
            };
            _cboRoom.Items.Add("Tất cả phòng");
            LoadRooms(_cboRoom);
            _cboRoom.SelectedIndex = 0;
            _cboRoom.SelectedIndexChanged += (s, e) => FilterIssues();
            toolbarPanel.Controls.Add(_cboRoom);

            // Search TextBox with Debounce (Priority 2)
            const string SEARCH_PLACEHOLDER = "Tìm kiếm theo ID, mô tả...";
            
            _txtSearch = new TextBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(10, 6, 10, 0),
                Text = SEARCH_PLACEHOLDER,
                ForeColor = Color.Gray
            };
            
            // Placeholder behavior - Clear on focus, restore on leave if empty
            _txtSearch.Enter += (s, e) =>
            {
                if (_txtSearch.Text == SEARCH_PLACEHOLDER)
                {
                    _txtSearch.Text = "";
                    _txtSearch.ForeColor = _textPrimary;
                }
            };
            
            _txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text = SEARCH_PLACEHOLDER;
                    _txtSearch.ForeColor = Color.Gray;
                }
            };
            
            // Initialize debounce timer
            _searchDebounceTimer = new System.Windows.Forms.Timer { Interval = 400 };
            _searchDebounceTimer.Tick += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                FilterIssues(); // Apply filter after debounce delay
            };
            
            _txtSearch.TextChanged += (s, e) =>
            {
                // Don't trigger filter if it's the placeholder text
                if (_txtSearch.Text == SEARCH_PLACEHOLDER) return;
                
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start(); // Reset timer on each keystroke
            };
            toolbarPanel.Controls.Add(_txtSearch);

            // Refresh Button
            Button btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Size = new Size(100, 32),
                Margin = new Padding(0, 4, 10, 0)
            };
            StyleButton(btnRefresh, _surface, _textPrimary);
            btnRefresh.Click += (s, e) => LoadIssues();
            toolbarPanel.Controls.Add(btnRefresh);

            // Add New Issue Button (Push to right via margin or layout hack? FlowLayout simply stacks left)
            // Ideally we want this on the far right.
            // Let's keep it simple: Add it next to Refresh for now, or use a separate panel if rigorous right-align needed.
            Button btnNewIssue = new Button
            {
                Text = "+ Báo sự cố mới",
                Size = new Size(140, 32),
                Margin = new Padding(20, 4, 0, 0) // Extra left margin to separate actions
            };
            StyleButton(btnNewIssue, _primary, Color.White, true);
            btnNewIssue.Click += BtnNewIssue_Click;
            toolbarPanel.Controls.Add(btnNewIssue);


            // 4. Data Grid
            Panel gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = Color.Transparent
            };

            _dgvIssues = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = _surface,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = 45 },
                EnableHeadersVisualStyles = false,
                GridColor = _border
            };
            
            _dgvIssues.ColumnHeadersDefaultCellStyle.BackColor = _surface;
            _dgvIssues.ColumnHeadersDefaultCellStyle.ForeColor = _textSecondary;
            _dgvIssues.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            _dgvIssues.ColumnHeadersDefaultCellStyle.SelectionBackColor = _surface; // No highlight on header selection
            _dgvIssues.ColumnHeadersHeight = 45;
            _dgvIssues.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            _dgvIssues.DefaultCellStyle.BackColor = _surface;
            _dgvIssues.DefaultCellStyle.ForeColor = _textPrimary;
            _dgvIssues.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 240, 240); // Light Red selection
            _dgvIssues.DefaultCellStyle.SelectionForeColor = _textPrimary;
            _dgvIssues.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);

            _dgvIssues.CellDoubleClick += DgvIssues_CellDoubleClick;
            _dgvIssues.CellFormatting += DgvIssues_CellFormatting;
            _dgvIssues.CellContentClick += DgvIssues_CellContentClick;

            gridPanel.Controls.Add(_dgvIssues);

            // Assemble Layout
            this.Controls.Add(gridPanel);     // Fill
            this.Controls.Add(toolbarPanel);  // Top
            this.Controls.Add(_statsPanel);   // Top
            this.Controls.Add(headerPanel);   // Top
        }

        #endregion

        #region Style Helpers

        private void StyleButton(Button btn, Color bg, Color fg, bool primary = false)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = bg;
            btn.ForeColor = fg;
            btn.FlatAppearance.BorderSize = primary ? 0 : 1;
            btn.FlatAppearance.BorderColor = _border;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9, primary ? FontStyle.Bold : FontStyle.Regular);

            // Simple rounded corners
            RoundCorners(btn, 6);

            btn.MouseEnter += (s, e) => btn.BackColor = primary ? _primaryDark : Color.FromArgb(245, 246, 250);
            btn.MouseLeave += (s, e) => btn.BackColor = bg;
        }

        private Panel CreateStatCard(string title, string value, Color accentColor, string tag)
        {
            Panel card = new Panel
            {
                Size = new Size(220, 90),
                Margin = new Padding(0, 0, 20, 0),
                BackColor = _surface,
                Tag = tag
            };
            RoundCorners(card, 10);
            
            // Left Accent Bar
            Panel accent = new Panel
            {
                Size = new Size(6, 90),
                Location = new Point(0, 0),
                BackColor = accentColor
            };
            
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = _textSecondary,
                Location = new Point(20, 15),
                AutoSize = true
            };

            Label lblValue = new Label
            {
                Tag = "value",
                Text = value,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = _textPrimary,
                Location = new Point(20, 40),
                AutoSize = true
            };

            card.Controls.Add(accent);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);

            // Cheap shadow effect via paint if needed, keeping it simple for now
            card.Paint += (s, e) =>
            {
                using (Pen p = new Pen(_border))
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };

            return card;
        }

        private void RoundCorners(Control ctrl, int radius)
        {
            Rectangle bounds = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            ctrl.Region = new Region(path);
        }

        #endregion

        #region Logic & Data

        // Helper class for Room ComboBox
        private class ReportRoomItem
        {
            public int MaPhong { get; set; }
            public string TenPhong { get; set; }
            public override string ToString() => TenPhong;
        }

        /// <summary>
        /// Độ ưu tiên của sự cố
        /// </summary>
        private enum IssuePriority
        {
            [Description("Thấp")]
            Low = 1,
            
            [Description("Trung bình")]
            Medium = 2,
            
            [Description("Cao")]
            High = 3
        }

        /// <summary>
        /// Trạng thái của báo cáo sự cố
        /// </summary>
        private enum IssueStatus
        {
            [Description("Chờ xử lý")]
            Pending = 1,
            
            [Description("Đang sửa")]
            InProgress = 2,
            
            [Description("Đã xong")]
            Resolved = 3
        }

        /// <summary>
        /// Model cho việc validate và submit form báo cáo sự cố
        /// </summary>
        private class IssueReportModel
        {
            public int MaPhong { get; set; }
            public string TenPhong { get; set; }
            public string LoaiSuCo { get; set; }
            public string MoTa { get; set; }
            public IssuePriority DoUuTien { get; set; }
            public List<string> HinhAnhFilePaths { get; set; } = new List<string>();

            public static readonly string[] ValidIssueTypes = new[]
            {
                "Máy chiếu", "Âm thanh", "Điều hòa", "Ghế", "Ánh sáng", "Khác"
            };

            public bool IsValidIssueType()
            {
                return !string.IsNullOrWhiteSpace(LoaiSuCo) && ValidIssueTypes.Contains(LoaiSuCo);
            }

            public Dictionary<string, string> Validate()
            {
                var errors = new Dictionary<string, string>();

                if (MaPhong <= 0)
                    errors.Add(nameof(MaPhong), "Vui lòng chọn phòng chiếu");

                if (string.IsNullOrWhiteSpace(LoaiSuCo))
                    errors.Add(nameof(LoaiSuCo), "Vui lòng chọn loại sự cố");
                else if (!IsValidIssueType())
                    errors.Add(nameof(LoaiSuCo), "Loại sự cố không hợp lệ");

                if (string.IsNullOrWhiteSpace(MoTa))
                    errors.Add(nameof(MoTa), "Vui lòng nhập mô tả chi tiết");
                else if (MoTa.Length < 10)
                    errors.Add(nameof(MoTa), "Mô tả phải có ít nhất 10 ký tự");
                else if (MoTa.Length > 500)
                    errors.Add(nameof(MoTa), "Mô tả không được vượt quá 500 ký tự");

                if (HinhAnhFilePaths != null && HinhAnhFilePaths.Count > 0)
                {
                    const long MAX_SIZE = 5 * 1024 * 1024;
                    var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    foreach (var path in HinhAnhFilePaths)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(path);
                            if (fileInfo.Length > MAX_SIZE)
                                errors.Add($"Image_{fileInfo.Name}", $"File {fileInfo.Name} vượt quá 5MB");
                            if (!validExtensions.Contains(fileInfo.Extension.ToLower()))
                                errors.Add($"Image_{fileInfo.Name}", $"File {fileInfo.Name} không phải định dạng ảnh hợp lệ");
                        }
                        catch
                        {
                            errors.Add($"Image_{path}", "Không thể đọc file ảnh");
                        }
                    }
                }

                return errors;
            }

            public bool IsValid => Validate().Count == 0;
        }

        // Extension methods for enums
        private static string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();
            
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        private static T ParseByDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
            }
            return (T)Enum.Parse(typeof(T), description);
        }

        private static string ToLegacyValue(IssuePriority priority)
        {
            return priority switch
            {
                IssuePriority.High => "Cao",
                IssuePriority.Low => "Thap",
                IssuePriority.Medium => "BinhThuong",
                _ => "BinhThuong"
            };
        }

        private void LoadRooms(ComboBox cbo)
        {
            try
            {
                DataTable dtRooms = _baoCaoDAL.LayPhongChieuTheoChiNhanh(_maChiNhanh);
                if (dtRooms != null && dtRooms.Rows.Count > 0)
                {
                    foreach (DataRow row in dtRooms.Rows)
                    {
                        cbo.Items.Add(new ReportRoomItem
                        {
                            MaPhong = Convert.ToInt32(row["MaPhong"]),
                            TenPhong = row["TenPhong"]?.ToString() ?? ""
                        });
                    }
                }
                else
                {
                    // Fallback
                    PhongChieuDAL phongDAL = new PhongChieuDAL();
                    DataTable dt = phongDAL.LayTatCaPhongChieu();
                    foreach (DataRow row in dt.Rows)
                    {
                        cbo.Items.Add(new ReportRoomItem
                        {
                            MaPhong = Convert.ToInt32(row["MaPhong"]),
                            TenPhong = row["TenPhong"]?.ToString() ?? row["MaPhong"]?.ToString()
                        });
                    }
                }
            }
            catch { }
        }

        private void LoadIssues()
        {
            try
            {
                if (_maChiNhanh > 0)
                    _dtIssues = _baoCaoDAL.LayBaoCaoSuCoTheoChiNhanh(_maChiNhanh);
                else
                    _dtIssues = _baoCaoDAL.LayTatCaBaoCaoSuCo();
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                Debug.WriteLine($"[UC_RoomIssueReport] Database error in LoadIssues: {sqlEx.Message}");
                Debug.WriteLine($"SQL Error Number: {sqlEx.Number}, Line: {sqlEx.LineNumber}");
                ShowErrorToast("Không thể tải danh sách sự cố. Vui lòng kiểm tra kết nối database.");
                _dtIssues = new DataTable(); // Empty fallback
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UC_RoomIssueReport] Unexpected error in LoadIssues: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowErrorToast("Đã xảy ra lỗi không mong muốn khi tải dữ liệu.");
                _dtIssues = new DataTable(); // Empty fallback
            }
            finally
            {
                // Always update UI regardless of success/failure
                if (_dgvIssues != null)
                {
                    _dgvIssues.DataSource = _dtIssues;
                    ConfigureGridColumns();
                    FilterIssues(); // Apply current filters
                    UpdateStats();
                }
            }
        }

        private void ConfigureGridColumns()
        {
            _dgvIssues.Columns.Clear();
            _dgvIssues.AutoGenerateColumns = false;

            // Manual Helper to Add Text Column
            var addCol = new Action<string, string, int>((dataProp, header, width) =>
            {
                var col = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = dataProp,
                    HeaderText = header,
                    Name = dataProp,
                    AutoSizeMode = width > 0 ? DataGridViewAutoSizeColumnMode.None : DataGridViewAutoSizeColumnMode.Fill,
                    Width = width
                };
                _dgvIssues.Columns.Add(col);
            });

            addCol("MaBaoCao", "ID", 60);
            addCol("TenPhong", "Phòng", 80);
            addCol("LoaiSuCo", "Loại", 120);
            addCol("MoTa", "Mô tả", 0); // Fill
            addCol("TrangThai", "Trạng thái", 120);
            addCol("DoUuTien", "Ưu tiên", 100);
            
            // Format Date Column
            var dateCol = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NgayBao",
                HeaderText = "Ngày báo",
                Name = "NgayBao",
                Width = 140,
                DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" }
            };
            _dgvIssues.Columns.Add(dateCol);

            addCol("NguoiBao", "Người báo", 150);

            // Add Action Button
            var btnCol = new DataGridViewButtonColumn
            {
                HeaderText = "Hành động",
                Text = "Chi tiết",
                UseColumnTextForButtonValue = true,
                Name = "Action",
                Width = 100,
                FlatStyle = FlatStyle.Flat
            };
            btnCol.DefaultCellStyle.BackColor = _surface;
            btnCol.DefaultCellStyle.ForeColor = _primary;
            btnCol.DefaultCellStyle.SelectionBackColor = _surface;
            btnCol.DefaultCellStyle.SelectionForeColor = _primary;
            _dgvIssues.Columns.Add(btnCol);
        }

        private void FilterIssues()
        {
            if (_dtIssues == null) return;
            string statusFilter = _cboFilter.SelectedItem?.ToString();
            string roomFilter = _cboRoom.SelectedItem?.ToString();
            string searchText = _txtSearch?.Text?.Trim() ?? "";
            
            // Skip search if it's the placeholder text
            const string SEARCH_PLACEHOLDER = "Tìm kiếm theo ID, mô tả...";
            if (searchText == SEARCH_PLACEHOLDER) searchText = "";
            
            List<string> filters = new List<string>();

            // Status filter
            if (statusFilter != null && statusFilter != "Tất cả trạng thái")
            {
                filters.Add($"TrangThai = '{statusFilter}'");
            }

            // Room filter
            if (_cboRoom.SelectedItem is ReportRoomItem roomItem && roomItem.MaPhong > 0)
            {
                filters.Add($"TenPhong = '{roomItem.TenPhong}'"); 
            }

            // Text search filter (Priority 2: search by ID, description, reporter)
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Escape single quotes for SQL-like filter
                string escapedSearch = searchText.Replace("'", "''");
                
                // Search across multiple columns: MaBaoCao, MoTa, NguoiBao
                string searchFilter = $"(Convert(MaBaoCao, 'System.String') LIKE '%{escapedSearch}%' " +
                                      $"OR MoTa LIKE '%{escapedSearch}%' " +
                                      $"OR NguoiBao LIKE '%{escapedSearch}%')";
                filters.Add(searchFilter);
            }

            // Combine all filters
            string rowFilter = string.Join(" AND ", filters);
            
            try
            {
                _dtIssues.DefaultView.RowFilter = rowFilter;
                Debug.WriteLine($"[UC_RoomIssueReport] Filter applied: '{rowFilter}' → {_dtIssues.DefaultView.Count} matching rows out of {_dtIssues.Rows.Count} total");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UC_RoomIssueReport] Filter error: {ex.Message}");
                _dtIssues.DefaultView.RowFilter = ""; // Clear filter on error
            }

            // Show/hide empty state (Priority 2)
            UpdateEmptyState();
        }

        /// <summary>
        /// Reset all filters to their default 'All' state
        /// </summary>
        private void ResetFilters()
        {
            // Reset status filter to "Tất cả trạng thái"
            if (_cboFilter != null && _cboFilter.Items.Count > 0)
            {
                _cboFilter.SelectedIndex = 0;
            }
            
            // Reset room filter to "Tất cả phòng"
            if (_cboRoom != null && _cboRoom.Items.Count > 0)
            {
                _cboRoom.SelectedIndex = 0;
            }
            
            // Reset search text to placeholder
            const string SEARCH_PLACEHOLDER = "Tìm kiếm theo ID, mô tả...";
            if (_txtSearch != null)
            {
                _txtSearch.Text = SEARCH_PLACEHOLDER;
                _txtSearch.ForeColor = Color.Gray;
            }
            
            Debug.WriteLine("[UC_RoomIssueReport] Filters reset to default");
        }

        /// <summary>
        /// Show or hide empty state panel based on filtered results
        /// </summary>
        private void UpdateEmptyState()
        {
            bool hasData = _dtIssues?.DefaultView.Count > 0;
            
            if (_emptyStatePanel == null)
            {
                // Create empty state panel once
                _emptyStatePanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = _surface,
                    Visible = false
                };

                Label lblIcon = new Label
                {
                    Text = "📭",
                    Font = new Font("Segoe UI", 48),
                    ForeColor = _textSecondary,
                    AutoSize = true
                };

                Label lblMessage = new Label
                {
                    Text = "Không tìm thấy báo cáo nào",
                    Font = new Font("Segoe UI", 14),
                    ForeColor = _textSecondary,
                    AutoSize = true
                };

                Label lblHint = new Label
                {
                    Text = "Thử thay đổi bộ lọc hoặc nhấn '+ Báo sự cố mới' để tạo báo cáo",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = _textSecondary,
                    AutoSize = true
                };

                // Center the content
                _emptyStatePanel.Resize += (s, e) =>
                {
                    lblIcon.Location = new Point((_emptyStatePanel.Width - lblIcon.Width) / 2, 80);
                    lblMessage.Location = new Point((_emptyStatePanel.Width - lblMessage.Width) / 2, 160);
                    lblHint.Location = new Point((_emptyStatePanel.Width - lblHint.Width) / 2, 195);
                };

                _emptyStatePanel.Controls.AddRange(new Control[] { lblIcon, lblMessage, lblHint });
                
                // Add to parent (should be added after DataGridView so it can overlay)
                if (_dgvIssues?.Parent != null)
                {
                    _dgvIssues.Parent.Controls.Add(_emptyStatePanel);
                    _emptyStatePanel.BringToFront();
                }
            }

            // Toggle visibility
            _emptyStatePanel.Visible = !hasData;
            if (_dgvIssues != null) _dgvIssues.Visible = hasData;
        }

        private void UpdateStats()
        {
            if (_dtIssues == null) return;

            int total = _dtIssues.Rows.Count;
            int pending = 0, inProgress = 0, resolved = 0;

            foreach (DataRow row in _dtIssues.Rows)
            {
                string status = row["TrangThai"]?.ToString() ?? "";
                if (status == "Chờ xử lý") pending++;
                else if (status == "Đang sửa") inProgress++;
                else if (status == "Đã xong") resolved++;
            }

            foreach (Control card in _statsPanel.Controls)
            {
                if (card is Panel p && p.Tag != null)
                {
                    string tag = p.Tag.ToString();
                    foreach (Control c in p.Controls)
                    {
                        if (c is Label lbl && lbl.Tag?.ToString() == "value")
                        {
                            lbl.Text = tag switch
                            {
                                "total" => total.ToString(),
                                "pending" => pending.ToString(),
                                "inprogress" => inProgress.ToString(),
                                "resolved" => resolved.ToString(),
                                _ => "0"
                            };
                        }
                    }
                }
            }
        }

        #endregion

        #region Events & Forms

        private void DgvIssues_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvIssues.Columns[e.ColumnIndex].Name == "TrangThai" && e.Value != null)
            {
                string status = e.Value.ToString();
                switch (status)
                {
                    case "Chờ xử lý":
                        e.CellStyle.ForeColor = _primary;
                        e.CellStyle.Font = new Font("Segoe UI Semibold", 10);
                        break;
                    case "Đang sửa":
                        e.CellStyle.ForeColor = _warning;
                        e.CellStyle.Font = new Font("Segoe UI Semibold", 10);
                        break;
                    case "Đã xong":
                        e.CellStyle.ForeColor = _success;
                        break;
                }
            }
            else if (_dgvIssues.Columns[e.ColumnIndex].Name == "DoUuTien" && e.Value != null)
            {
                 string val = e.Value.ToString();
                 if (val == "Cao") 
                 {
                     e.CellStyle.ForeColor = _primary;
                     e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                 }
            }
        }

        private void DgvIssues_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _dgvIssues.Columns[e.ColumnIndex].Name == "Action")
            {
                ShowIssueDetail(_dgvIssues.Rows[e.RowIndex]);
            }
        }

        private void DgvIssues_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ShowIssueDetail(_dgvIssues.Rows[e.RowIndex]);
            }
        }

        private void ShowIssueDetail(DataGridViewRow row)
        {
             // ... Code to show detail form
             // Reusing the logic from before but ensuring modern styling
             
            int maBaoCao = Convert.ToInt32(row.Cells["MaBaoCao"].Value);
            string tenPhong = row.Cells["TenPhong"].Value?.ToString();
            string loaiSuCo = row.Cells["LoaiSuCo"].Value?.ToString();
            string moTa = row.Cells["MoTa"].Value?.ToString();
            string trangThai = row.Cells["TrangThai"].Value?.ToString();
            string doUuTien = row.Cells["DoUuTien"].Value?.ToString();
            string nguoiBao = row.Cells["NguoiBao"].Value?.ToString();
            
            DateTime dt;
            DateTime.TryParse(row.Cells["NgayBao"].Value?.ToString(), out dt);
            string ngayBao = dt.ToString("dd/MM/yyyy HH:mm");

            Form frm = new Form
            {
                Text = $"Chi tiết sự cố #{maBaoCao}",
                Size = new Size(500, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = _surface
            };

            int y = 20;
            // Helper to add label-value pairs
            Action<string, string, bool> addRow = (label, value, bold) =>
            {
                frm.Controls.Add(new Label { Text = label, Location = new Point(30, y), AutoSize = true, ForeColor = _textSecondary, Font = new Font("Segoe UI", 10) });
                Label lblVal = new Label 
                { 
                    Text = value, 
                    Location = new Point(140, y), 
                    AutoSize = true, 
                    ForeColor = _textPrimary,
                    Font = new Font("Segoe UI", 10, bold ? FontStyle.Bold : FontStyle.Regular),
                    MaximumSize = new Size(320, 0)
                };
                frm.Controls.Add(lblVal);
                y += Math.Max(35, lblVal.Height + 10);
            };

            frm.Controls.Add(new Label { Text = "THÔNG TIN CHI TIẾT", Location = new Point(30, y), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = _primary, AutoSize = true});
            y += 40;

            addRow("Phòng:", tenPhong, true);
            addRow("Loại sự cố:", loaiSuCo, false);
            addRow("Mức độ:", doUuTien, true);
            addRow("Ngày báo:", ngayBao, false);
            addRow("Người báo:", nguoiBao, false);
            
            // Description
            frm.Controls.Add(new Label { Text = "Mô tả:", Location = new Point(30, y), AutoSize = true, ForeColor = _textSecondary, Font = new Font("Segoe UI", 10) });
            y += 25;
            TextBox txtDesc = new TextBox
            {
                Text = moTa,
                Location = new Point(30, y),
                Size = new Size(420, 80),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = _surfaceAlt,
                BorderStyle = BorderStyle.FixedSingle
            };
            frm.Controls.Add(txtDesc);
            y += 90;

            // Manager Controls
             ComboBox cboStatus = null;
             TextBox txtNote = null;

             if (_isManager) 
             {
                 y += 10;
                 Panel separate = new Panel { Size = new Size(440, 1), BackColor = _border, Location=new Point(30, y)};
                 frm.Controls.Add(separate);
                 y += 15;

                 frm.Controls.Add(new Label { Text = "CẬP NHẬT TRẠNG THÁI", Location = new Point(30, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = _primary });
                 y += 30;

                 frm.Controls.Add(new Label { Text = "Trạng thái:", Location = new Point(30, y+3), AutoSize = true });
                 cboStatus = new ComboBox
                 {
                     Location = new Point(140, y),
                     Width = 310,
                     DropDownStyle = ComboBoxStyle.DropDownList
                 };
                 cboStatus.Items.AddRange(new[] { "Chờ xử lý", "Đang sửa", "Đã xong" });
                 cboStatus.SelectedItem = trangThai;
                 frm.Controls.Add(cboStatus);
                 y += 40;

                 frm.Controls.Add(new Label { Text = "Ghi chú:", Location = new Point(30, y), AutoSize = true });
                 y += 25;
                 txtNote = new TextBox
                 {
                     Location = new Point(30, y),
                     Size = new Size(420, 60),
                     Multiline = true,
                     BorderStyle = BorderStyle.FixedSingle
                 };
                 frm.Controls.Add(txtNote);
                 y += 70;
             }
             else
             {
                  addRow("Trạng thái:", trangThai, true);
             }

             // Buttons
             Button btnClose = new Button
             {
                 Text = "Đóng",
                 Size = new Size(100, 36),
                 Location = new Point(350, y),
             };
             StyleButton(btnClose, _surfaceAlt, _textPrimary);
             btnClose.Click += (s, ev) => frm.Close();
             frm.Controls.Add(btnClose);

             if (_isManager)
             {
                 Button btnSave = new Button
                 {
                     Text = "Lưu thay đổi",
                     Size = new Size(120, 36),
                     Location = new Point(220, y),
                 };
                 StyleButton(btnSave, _primary, Color.White, true);
                 btnSave.Click += (s, ev) =>
                 {
                     try
                     {
                         string newStatus = cboStatus.SelectedItem.ToString();
                         string note = txtNote.Text;
                         if (_baoCaoDAL.CapNhatTrangThai(maBaoCao, newStatus, _userId, note))
                         {
                             MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                             frm.Close();
                             LoadIssues();
                         }
                         else MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     }
                     catch(Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                 };
                 frm.Controls.Add(btnSave);
             }

             frm.Height = y + 80;
             frm.ShowDialog();
        }

        private void BtnNewIssue_Click(object sender, EventArgs e)
        {
            ShowNewIssueForm();
        }

        private void ShowNewIssueForm()
        {
            // Create form-specific ErrorProvider
            ErrorProvider formErrorProvider = new ErrorProvider
            {
                BlinkStyle = ErrorBlinkStyle.NeverBlink
            };

            List<string> uploadedImages = new List<string>();
            FlowLayoutPanel imagePreviewPanel = null;

            Form frm = new Form
            {
                Text = "Báo cáo sự cố mới",
                Size = new Size(550, 650),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = _surface
            };

            int y = 20;

            // Room selection
            Label lblRoom = new Label { Text = "Phòng chiếu: *", Location = new Point(30, y + 3), AutoSize = true, ForeColor = _textPrimary };
            frm.Controls.Add(lblRoom);
            
            ComboBox cboRoom = new ComboBox 
            { 
                Location = new Point(140, y), 
                Width = 350, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cboRoom"
            };
            LoadRooms(cboRoom);
            if (cboRoom.Items.Count > 0) cboRoom.SelectedIndex = 0;
            frm.Controls.Add(cboRoom);
            y += 45;

            // Type
            Label lblType = new Label { Text = "Loại sự cố: *", Location = new Point(30, y + 3), AutoSize = true, ForeColor = _textPrimary };
            frm.Controls.Add(lblType);
            
            ComboBox cboType = new ComboBox 
            { 
                Location = new Point(140, y), 
                Width = 350, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cboType"
            };
            cboType.Items.AddRange(IssueReportModel.ValidIssueTypes);
            cboType.SelectedIndex = 0;
            frm.Controls.Add(cboType);
            y += 45;

            // Priority using Enum
            Label lblPriority = new Label { Text = "Độ ưu tiên: *", Location = new Point(30, y + 3), AutoSize = true, ForeColor = _textPrimary };
            frm.Controls.Add(lblPriority);
            
            ComboBox cboPriority = new ComboBox 
            { 
                Location = new Point(140, y), 
                Width = 350, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cboPriority"
            };
            
            // Populate with enum descriptions
            foreach (IssuePriority priority in Enum.GetValues(typeof(IssuePriority)))
            {
                cboPriority.Items.Add(GetDescription(priority));
            }
            cboPriority.SelectedIndex = 1; // Medium by default
            frm.Controls.Add(cboPriority);
            y += 45;

            // Description
            Label lblDesc = new Label { Text = "Mô tả chi tiết: *", Location = new Point(30, y), AutoSize = true, ForeColor = _textPrimary };
            frm.Controls.Add(lblDesc);
            
            Label lblDescHint = new Label 
            { 
                Text = "(Tối thiểu 10 ký tự, tối đa 500 ký tự)", 
                Location = new Point(140, y), 
                AutoSize = true, 
                ForeColor = _textSecondary,
                Font = new Font("Segoe UI", 8)
            };
            frm.Controls.Add(lblDescHint);
            y += 25;
            
            TextBox txtDesc = new TextBox 
            { 
                Location = new Point(30, y), 
                Size = new Size(480, 100), 
                Multiline = true, 
                BorderStyle = BorderStyle.FixedSingle,
                Name = "txtDesc",
                ScrollBars = ScrollBars.Vertical
            };
            frm.Controls.Add(txtDesc);
            y += 110;

            // Image Upload Section
            Label lblImages = new Label { Text = "Hình ảnh minh chứng:", Location = new Point(30, y), AutoSize = true, ForeColor = _textPrimary };
            frm.Controls.Add(lblImages);
            y += 25;

            Button btnUpload = new Button 
            { 
                Text = "📷 Chọn ảnh", 
                Size = new Size(150, 35), 
                Location = new Point(30, y) 
            };
            StyleButton(btnUpload, _surfaceAlt, _textPrimary);
            
            btnUpload.Click += (s, ev) =>
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif";
                    dialog.Multiselect = true;
                    dialog.Title = "Chọn hình ảnh minh chứng";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        foreach (string filePath in dialog.FileNames)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            
                            if (fileInfo.Length > MAX_IMAGE_SIZE)
                            {
                                formErrorProvider.SetError(btnUpload, $"File {fileInfo.Name} vượt quá 5MB");
                                continue;
                            }

                            if (!uploadedImages.Contains(filePath))
                            {
                                uploadedImages.Add(filePath);
                                AddImagePreviewToPanel(imagePreviewPanel, filePath, uploadedImages);
                            }
                        }
                        
                        if (uploadedImages.Count > 0)
                        {
                            formErrorProvider.SetError(btnUpload, ""); // Clear error
                        }
                    }
                }
            };
            frm.Controls.Add(btnUpload);

            // Image preview panel
            imagePreviewPanel = new FlowLayoutPanel
            {
                Location = new Point(190, y),
                Size = new Size(320, 80),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                BackColor = _surfaceAlt
            };
            frm.Controls.Add(imagePreviewPanel);
            y += 90;

            // Submit Button
            Button btnSubmit = new Button 
            { 
                Text = "Gửi báo cáo", 
                Size = new Size(130, 40), 
                Location = new Point(250, y) 
            };
            StyleButton(btnSubmit, _primary, Color.White, true);
            
            btnSubmit.Click += (s, ev) =>
            {
                // Clear previous errors
                formErrorProvider.Clear();

                // Create model
                var model = new IssueReportModel
                {
                    MaPhong = (cboRoom.SelectedItem as ReportRoomItem)?.MaPhong ?? 0,
                    TenPhong = (cboRoom.SelectedItem as ReportRoomItem)?.TenPhong,
                    LoaiSuCo = cboType.SelectedItem?.ToString(),
                    MoTa = txtDesc.Text,
                    DoUuTien = ParseByDescription<IssuePriority>(cboPriority.SelectedItem?.ToString() ?? "Trung bình"),
                    HinhAnhFilePaths = uploadedImages
                };

                // Validate
                var errors = model.Validate();
                if (errors.Count > 0)
                {
                    // Display errors using ErrorProvider
                    foreach (var error in errors)
                    {
                        Control ctrl = frm.Controls.Find(error.Key, false).FirstOrDefault();
                        if (ctrl != null)
                        {
                            formErrorProvider.SetError(ctrl, error.Value);
                        }
                        else if (error.Key.StartsWith("Image_"))
                        {
                            formErrorProvider.SetError(btnUpload, error.Value);
                        }
                    }
                    
                    ShowErrorToast("Vui lòng kiểm tra lại thông tin đã nhập!");
                    return;
                }

                // Submit to database
                try
                {
                    string legacyPriority = ToLegacyValue(model.DoUuTien); // Convert enum to DB-compatible value
                    
                    int newId = _baoCaoDAL.ThemBaoCaoSuCo(
                        _userId, 
                        model.MaPhong, 
                        null, // MaTrangThietBi (optional)
                        model.LoaiSuCo, 
                        model.MoTa, 
                        legacyPriority
                    );

                    if (newId > 0)
                    {
                        // TODO: Save uploaded images to database or file system
                        // For now, just log them
                        foreach (var imagePath in uploadedImages)
                        {
                            Debug.WriteLine($"[UC_RoomIssueReport] Image attached: {imagePath}");
                            // SaveImageToDatabase(newId, imagePath);
                        }

                        ShowSuccessToast("Báo cáo sự cố đã được gửi thành công!");
                        frm.Close();
                        
                        // Reset filters to show the newly created report
                        ResetFilters();
                        LoadIssues();
                    }
                    else
                    {
                        ShowErrorToast("Không thể gửi báo cáo. Vui lòng thử lại!");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[UC_RoomIssueReport] Error submitting issue: {ex.Message}");
                    ShowErrorToast($"Lỗi: {ex.Message}");
                }
            };
            frm.Controls.Add(btnSubmit);

            // Cancel Button
            Button btnCancel = new Button 
            { 
                Text = "Hủy", 
                Size = new Size(100, 40), 
                Location = new Point(390, y) 
            };
            StyleButton(btnCancel, _surfaceAlt, _textPrimary, false);
            btnCancel.Click += (s, ev) => frm.Close();
            frm.Controls.Add(btnCancel);

            frm.ShowDialog();
            formErrorProvider.Dispose();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Add image preview thumbnail to panel
        /// </summary>
        private void AddImagePreviewToPanel(FlowLayoutPanel panel, string imagePath, List<string> imageList)
        {
            if (panel == null) return;

            try
            {
                Panel thumbPanel = new Panel
                {
                    Size = new Size(70, 70),
                    Margin = new Padding(5),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = imagePath
                };

                PictureBox pb = new PictureBox
                {
                    Size = new Size(66, 66),
                    Location = new Point(2, 2),
                    Image = Image.FromFile(imagePath),
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                // Delete button overlay
                Button btnDelete = new Button
                {
                    Text = "×",
                    Size = new Size(20, 20),
                    Location = new Point(thumbPanel.Width - 22, 2),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(200, 226, 26, 60),
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnDelete.FlatAppearance.BorderSize = 0;
                btnDelete.Click += (s, ev) =>
                {
                    imageList.Remove(imagePath);
                    panel.Controls.Remove(thumbPanel);
                    pb.Image?.Dispose();
                };

                thumbPanel.Controls.Add(pb);
                thumbPanel.Controls.Add(btnDelete);
                panel.Controls.Add(thumbPanel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UC_RoomIssueReport] Error adding image preview: {ex.Message}");
            }
        }

        /// <summary>
        /// Show error toast notification
        /// </summary>
        private void ShowErrorToast(string message)
        {
            ShowToast(message, _primary, Color.White);
        }

        /// <summary>
        /// Show success toast notification
        /// </summary>
        private void ShowSuccessToast(string message)
        {
            ShowToast(message, _success, Color.White);
        }

        /// <summary>
        /// Generic toast notification
        /// </summary>
        private void ShowToast(string message, Color bgColor, Color fgColor)
        {
            Form toast = new Form
            {
                Width = 400,
                Height = 90,
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                BackColor = bgColor,
                ShowInTaskbar = false,
                TopMost = true,
                Opacity = 0.95
            };

            // Position at bottom right
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            toast.Location = new Point(
                workingArea.Width - toast.Width - 20,
                workingArea.Height - toast.Height - 20
            );

            // Rounded corners
            RoundCorners(toast, 10);

            // Icon
            Label lblIcon = new Label
            {
                Text = bgColor == _success ? "✓" : "⚠",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = fgColor,
                Location = new Point(20, 25),
                AutoSize = true
            };
            toast.Controls.Add(lblIcon);

            // Message
            Label lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10),
                ForeColor = fgColor,
                Location = new Point(70, 20),
                MaximumSize = new Size(310, 0),
                AutoSize = true
            };
            toast.Controls.Add(lblMessage);

            // Click to close
            toast.Click += (s, e) => toast.Close();
            lblIcon.Click += (s, e) => toast.Close();
            lblMessage.Click += (s, e) => toast.Close();

            // Auto-close after 4 seconds
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 4000 };
            timer.Tick += (s, e) =>
            {
                toast.Close();
                timer.Dispose();
            };
            timer.Start();

            toast.Show();
        }

        #endregion

        private void AddRoundedCorners(Control control, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(control.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, control.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            control.Region = new Region(path);
        }

        private void AddShadow(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(20, 0, 0, 0), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, panel.Height - 1, panel.Width - 1, 1);
                }
            };
        }
    }
}
