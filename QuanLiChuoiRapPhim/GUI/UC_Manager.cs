﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.GUI
{
    public partial class UC_Manager : UserControl
    {
        private string _managerName;
        private string _branchName;

        // Event to notify parent form (frmMain) to change screens
        public event EventHandler<string> OnFeatureClick;

        // CGV Theme Colors
        private Color _cgvRed = Color.FromArgb(226, 26, 60);
        private Color _cgvBlack = Color.FromArgb(15, 15, 15);
        private Color _cgvGray = Color.FromArgb(245, 245, 245);
        private Color _cgvWhite = Color.White;

        public UC_Manager()
        {
            InitializeComponent();
        }

        public UC_Manager(string managerName, string branchName) : this()
        {
            _managerName = managerName;
            _branchName = branchName;
            SetupUI();
        }

        private void SetupUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = _cgvGray;
            this.AutoScroll = true;
            this.Padding = new Padding(30);

            // 1. Header Section
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.Transparent };

            Label lblTitle = new Label
            {
                Text = $"DASHBOARD QUẢN LÝ - {(_branchName ?? "CHI NHÁNH").ToUpper()}",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = _cgvBlack,
                AutoSize = true,
                Location = new Point(0, 10)
            };

            Label lblSub = new Label
            {
                Text = $"👋 Xin chào, {_managerName} | Chúc bạn một ngày làm việc hiệu quả!",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(5, 55)
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSub);
            this.Controls.Add(headerPanel);

            // 2. Statistics Section
            FlowLayoutPanel statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 180,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Mock Data for Stats (Safe to compile without BLL)
            statsPanel.Controls.Add(CreateStatCard("DOANH THU NGÀY", "18.5M ₫", "💰", _cgvRed));
            statsPanel.Controls.Add(CreateStatCard("NHÂN VIÊN TRỰC", "8/12", "👔", Color.FromArgb(41, 128, 185)));
            statsPanel.Controls.Add(CreateStatCard("SUẤT CHIẾU", "24", "🎬", Color.FromArgb(142, 68, 173)));
            statsPanel.Controls.Add(CreateStatCard("CẢNH BÁO KHO", "3", "⚠️", Color.FromArgb(230, 126, 34)));

            this.Controls.Add(statsPanel);

            // 3. Quick Actions Section
            Panel actionsContainer = new Panel { Dock = DockStyle.Top, Height = 400, BackColor = Color.Transparent, Padding = new Padding(0, 30, 0, 0) };

            Label lblActions = new Label
            {
                Text = "CHỨC NĂNG QUẢN LÝ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = _cgvBlack,
                Dock = DockStyle.Top,
                Height = 40
            };

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Add buttons mapping to features in frmMain
            actionsPanel.Controls.Add(CreateActionButton("Bán Vé", "TicketSales", "🎟️"));
            actionsPanel.Controls.Add(CreateActionButton("Quản Lý Phim", "MovieEdit", "🎬"));
            actionsPanel.Controls.Add(CreateActionButton("Lịch Chiếu", "ShowtimeEdit", "📅"));
            actionsPanel.Controls.Add(CreateActionButton("Kho Hàng", "InventoryManagement", "📦"));
            actionsPanel.Controls.Add(CreateActionButton("Nhân Sự", "StaffManagement", "👥"));
            actionsPanel.Controls.Add(CreateActionButton("Lịch Làm Việc", "WorkSchedule", "🗓️"));
            actionsPanel.Controls.Add(CreateActionButton("Đánh Giá", "PerformanceReview", "📈"));
            actionsPanel.Controls.Add(CreateActionButton("Báo Cáo", "BranchReports", "📊"));

            actionsContainer.Controls.Add(actionsPanel);
            actionsContainer.Controls.Add(lblActions);
            this.Controls.Add(actionsContainer);
        }

        private Panel CreateStatCard(string title, string value, string icon, Color color)
        {
            Panel card = new Panel
            {
                Size = new Size(280, 150),
                BackColor = _cgvWhite,
                Margin = new Padding(0, 0, 20, 0),
                Cursor = Cursors.Hand
            };

            Label lblIcon = new Label { Text = icon, Font = new Font("Segoe UI", 24), Location = new Point(20, 20), AutoSize = true, ForeColor = color };
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 22, FontStyle.Bold), Location = new Point(20, 75), AutoSize = true, ForeColor = _cgvBlack };
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 115), AutoSize = true, ForeColor = Color.Gray };

            Panel line = new Panel { Size = new Size(4, 40), Location = new Point(0, 25), BackColor = color };

            card.Controls.Add(line);
            card.Controls.Add(lblIcon);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);

            return card;
        }

        private Button CreateActionButton(string text, string feature, string icon)
        {
            Button btn = new Button
            {
                Size = new Size(200, 120),
                Margin = new Padding(0, 0, 20, 20),
                BackColor = _cgvWhite,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Text = $"{icon}\n\n{text.ToUpper()}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _cgvBlack,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;

            // Fire event instead of opening form directly
            btn.Click += (s, e) => OnFeatureClick?.Invoke(this, feature);

            return btn;
        }
    }
}
