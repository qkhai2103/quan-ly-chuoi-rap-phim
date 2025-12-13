using QuanLiChuoiRapPhim.GUI;
using System;
using System.Windows.Forms;
//using OperatingSystem;
using System.Windows.Forms;
using QuanLiChuoiRapPhim.GUI;

namespace QuanLiChuoiRapPhim
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Hiển thị form đăng nhập trong khối using
            using (frmLogin frm = new frmLogin())
            {
                // Nếu đăng nhập thành công (DialogResult.OK)
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    // Mở form chính và truyền dữ liệu
                    Application.Run(new frmMain("admin", "Admin", "Toàn hệ thống"));
                }
            }
        }
      
    }
}