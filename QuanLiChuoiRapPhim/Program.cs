using QuanLiChuoiRapPhim.GUI;
using QuanLiChuoiRapPhim.Migrations;
using System;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run migrations
            try
            {
                MigrationRunner.RunMigration();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Migration error: {ex.Message}");
            }

            // Hiển thị form đăng nhập
            frmLogin loginForm = new frmLogin();
            try
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Nếu đăng nhập thành công, chạy form chính với toàn bộ thông tin người dùng
                    Application.Run(new frmMain(loginForm.LoggedInUsername, loginForm.LoggedInRole, 
                        loginForm.LoggedInBranch, loginForm.LoggedInFullName, 
                        loginForm.LoggedInMaNguoiDung, loginForm.LoggedInMaChiNhanh));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi động Dashboard: {ex.Message}\n\nChi tiết: {ex.StackTrace}", 
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}