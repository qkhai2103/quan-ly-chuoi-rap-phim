using QuanLiChuoiRapPhim.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class UserBLL
    {
        private UserDAL userDAL = new UserDAL();

        public DataRow GetUserById(int userId)
        {
            DataTable dt = userDAL.GetUserById(userId);

            if (dt.Rows.Count == 0)
            {
                throw new ArgumentException("Không tìm thấy người dùng");
            }

            return dt.Rows[0];
        }

        public DataTable GetBranches()
        {
            return userDAL.GetBranches();
        }

        public string GetUserRole(int userId)
        {
            DataRow user = GetUserById(userId);
            return user["VaiTro"].ToString();
        }

        public bool IsAdmin(int userId)
        {
            return GetUserRole(userId) == "Admin";
        }

        public bool IsManager(int userId)
        {
            return GetUserRole(userId) == "QuanLy";
        }

        public bool IsStaff(int userId)
        {
            return GetUserRole(userId) == "NhanVien";
        }

        public DataTable GetShowsByDate(DateTime date)
        {
            return userDAL.GetShowsByDate(date);
        }

        public DataTable GetTodayShows()
        {
            return GetShowsByDate(DateTime.Today);
        }
    }
}
