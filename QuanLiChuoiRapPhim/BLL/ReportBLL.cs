using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    public class ReportBLL
    {
        private ReportDAL reportDAL = new ReportDAL();

        // 1. Báo cáo doanh thu theo ngày
        public DataTable GetRevenueByDate(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return reportDAL.GetRevenueByDate(fromDate, toDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy doanh thu: {ex.Message}");
            }
        }

        // 2. Báo cáo phim bán chạy
        public DataTable GetTopMovies(DateTime fromDate, DateTime toDate, int top = 10)
        {
            try
            {
                return reportDAL.GetTopMovies(fromDate, toDate, top);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy phim bán chạy: {ex.Message}");
            }
        }

        // 3. Báo cáo doanh thu theo chi nhánh
        public DataTable GetRevenueByBranch(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return reportDAL.GetRevenueByBranch(fromDate, toDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy doanh thu chi nhánh: {ex.Message}");
            }
        }

        // 4. Báo cáo vé bán
        public DataTable GetTicketSales(DateTime fromDate, DateTime toDate, string branchName = null)
        {
            try
            {
                return reportDAL.GetTicketSales(fromDate, toDate, branchName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy vé bán: {ex.Message}");
            }
        }

        // 5. Báo cáo sản phẩm bán chạy
        public DataTable GetTopProducts(DateTime fromDate, DateTime toDate, string branchName = null)
        {
            try
            {
                return reportDAL.GetTopProducts(fromDate, toDate, branchName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy sản phẩm bán chạy: {ex.Message}");
            }
        }

        // 6. Báo cáo khách hàng thành viên
        public DataTable GetMemberCustomers()
        {
            try
            {
                return reportDAL.GetMemberCustomers();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy khách hàng: {ex.Message}");
            }
        }

        // 7. Tổng hợp thống kê nhanh
        public DataTable GetQuickStats(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return reportDAL.GetQuickStats(fromDate, toDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy thống kê: {ex.Message}");
            }
        }
    }
}
