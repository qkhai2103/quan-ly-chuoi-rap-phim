using QuanLiChuoiRapPhim.BLL;
using QuanLiChuoiRapPhim.GUI;
using System;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            SetupDefaultValues();
        }

        private void SetupDefaultValues()
        {
            // Thiết lập giá trị mặc định cho demo
            txtTenDangNhap.Text = "admin";
            txtMatKhau.Text = "123456";
            txtMatKhau.UseSystemPasswordChar = true;
        }

        private void BtnDangNhap_Click(object sender, EventArgs e)
        {
           
        }

        private void PerformLogin()
        {
            string username = txtTenDangNhap.Text.Trim();
            string password = txtMatKhau.Text;

            // Kiểm tra dữ liệu đầu vào
            if (!ValidateInput(username, password))
                return;

            try
            {
                AdminBLL adminBLL = new AdminBLL();
                string fullName, errorMessage;

                if (adminBLL.Login(username, password, out fullName, out errorMessage))
                {
                    LoginSuccessful(fullName, username);
                }
                else
                {
                    ShowErrorMessage(errorMessage);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private bool ValidateInput(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void LoginSuccessful(string fullName, string username)
        {
            // Xác định vai trò dựa trên tên đăng nhập
            string userRole = DetermineUserRole(username);
            string branch = DetermineUserBranch(username);

            MessageBox.Show($"Đăng nhập thành công!\nChào mừng {fullName}",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Mở form chính
            frmMain mainForm = new frmMain(username, userRole, branch);
            mainForm.Show();
            this.Hide();
        }

        private string DetermineUserRole(string username)
        {
            // Phân loại vai trò dựa trên tên đăng nhập
            if (username.StartsWith("admin"))
                return "Admin";
            else if (username.StartsWith("ql_"))
                return "Quản lý";
            else if (username.StartsWith("nv_"))
                return "Nhân viên";
            else
                return "Người dùng";
        }

        private string DetermineUserBranch(string username)
        {
            // Xác định chi nhánh dựa trên tên đăng nhập
            if (username.EndsWith("_cn1") || username.Contains("cn1"))
                return "CGV Vincom Xuân Khánh";
            else if (username.EndsWith("_cn2") || username.Contains("cn2"))
                return "CGV Sense City Cần Thơ";
            else if (username.EndsWith("_cn3") || username.Contains("cn3"))
                return "CGV Vincom Hùng Vương";
            else
                return "Toàn hệ thống";
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Đăng nhập thất bại",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Lỗi hệ thống",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnThoat_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát chương trình?", "Xác nhận thoát",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void ChkHienMatKhau_CheckedChanged(object sender, EventArgs e)
        {
            txtMatKhau.UseSystemPasswordChar = !chkHienMatKhau.Checked;
        }

        // Xử lý phím Enter để đăng nhập
        private void txtMatKhau_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                PerformLogin();
                e.Handled = true;
            }
        }

        private void txtTenDangNhap_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Chuyển focus đến ô mật khẩu
                txtMatKhau.Focus();
                e.Handled = true;
            }
        }

        private void btnDangNhap_Click_1(object sender, EventArgs e)
        {
            PerformLogin();
        }
    }
}