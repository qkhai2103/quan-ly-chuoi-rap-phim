using System;
using System.Data;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class DoanhThuBLL
    {
        private readonly DoanhThuDAL _doanhThuDal;

        public DoanhThuBLL()
        {
            _doanhThuDal = new DoanhThuDAL();
        }

        public DataTable ThongKeDoanhThuTheoNgay(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                return _doanhThuDal.ThongKeDoanhThuTheoNgay(tuNgay, denNgay);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thống kê doanh thu theo ngày: {ex.Message}");
            }
        }

        public DataTable ThongKeDoanhThuChiNhanh(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                return _doanhThuDal.ThongKeDoanhThuChiNhanh(tuNgay, denNgay);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thống kê doanh thu chi nhánh: {ex.Message}");
            }
        }

        public DataTable ThongKePhimBanChay(int top = 10)
        {
            try
            {
                return _doanhThuDal.ThongKePhimBanChay(top);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thống kê phim bán chạy: {ex.Message}");
            }
        }

        public DataTable ThongKeSanPhamBanChay(int top = 10)
        {
            try
            {
                return _doanhThuDal.ThongKeSanPhamBanChay(top);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thống kê sản phẩm bán chạy: {ex.Message}");
            }
        }

        public decimal LayDoanhThuHienTai()
        {
            try
            {
                return _doanhThuDal.LayDoanhThuHienTai();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy doanh thu hiện tại: {ex.Message}");
            }
        }
    }
}
