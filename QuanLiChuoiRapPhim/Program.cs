using QuanLiChuoiRapPhim.GUI;
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

            // Hiển thị form đăng nhập
            frmLogin loginForm = new frmLogin();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Nếu đăng nhập thành công, chạy form chính
                // Pass LoggedInUsername also as fullName for now (frmMain expects 4 args)
                Application.Run(new frmMain(loginForm.LoggedInUsername, loginForm.LoggedInRole, loginForm.LoggedInBranch, loginForm.LoggedInUsername));
            }
        }
    }
}