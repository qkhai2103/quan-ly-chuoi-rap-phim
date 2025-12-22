using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiChuoiRapPhim.DAL
{
    internal class DatabaseSecurityHelperDAL
    {
        /// <summary>
        /// Thêm điều kiện WHERE theo chi nhánh cho Manager
        /// </summary>
        public static string AddBranchFilter(string query, int maChiNhanh, string userRole)
        {
            if (userRole == "Quản lý" && maChiNhanh > 0)
            {
                if (query.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) >= 0)
                    return query + $" AND MaChiNhanh = {maChiNhanh}";
                else
                    return query + $" WHERE MaChiNhanh = {maChiNhanh}";
            }
            return query;
        }

        /// <summary>
        /// Thêm điều kiện WHERE theo người dùng cho Staff
        /// </summary>
        public static string AddUserFilter(string query, int maNguoiDung, string userRole)
        {
            if (userRole == "Nhân viên" && maNguoiDung > 0)
            {
                if (query.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) >= 0)
                    return query + $" AND MaNhanVien = {maNguoiDung}";
                else
                    return query + $" WHERE MaNhanVien = {maNguoiDung}";
            }
            return query;
        }

        /// <summary>
        /// Tạo SqlCommand với security parameters
        /// </summary>
        public static SqlCommand CreateSecureCommand(string query, int maChiNhanh, int maNguoiDung, string userRole)
        {
            var secureQuery = AddBranchFilter(query, maChiNhanh, userRole);
            secureQuery = AddUserFilter(secureQuery, maNguoiDung, userRole);

            var command = new SqlCommand(secureQuery);

            // Thêm parameters để tránh SQL Injection
            if (maChiNhanh > 0 && userRole == "Quản lý")
                command.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);

            if (maNguoiDung > 0 && userRole == "Nhân viên")
                command.Parameters.AddWithValue("@MaNhanVien", maNguoiDung);

            return command;
        }
    }
}
