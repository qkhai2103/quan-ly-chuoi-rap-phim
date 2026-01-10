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
