using System;
using System.Data;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class ChiNhanhBLL
    {
        private readonly ChiNhanhDAL _chiNhanhDal;

        public ChiNhanhBLL()
        {
            _chiNhanhDal = new ChiNhanhDAL();
        }

        public DataTable LayTatCaChiNhanh()
        {
            try
            {
                return _chiNhanhDal.LayTatCaChiNhanh();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách chi nhánh: {ex.Message}");
            }
        }

        public bool ThemChiNhanh(string tenChiNhanh, string diaChi, string soDienThoai)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenChiNhanh))
                    throw new Exception("Tên chi nhánh không được để trống!");

                return _chiNhanhDal.ThemChiNhanh(tenChiNhanh, diaChi, soDienThoai);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thêm chi nhánh: {ex.Message}");
            }
        }

        public bool CapNhatChiNhanh(int maChiNhanh, string tenChiNhanh, string diaChi, string soDienThoai)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenChiNhanh))
                    throw new Exception("Tên chi nhánh không được để trống!");

                return _chiNhanhDal.CapNhatChiNhanh(maChiNhanh, tenChiNhanh, diaChi, soDienThoai);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi cập nhật chi nhánh: {ex.Message}");
            }
        }

        public bool XoaChiNhanh(int maChiNhanh)
        {
            try
            {
                return _chiNhanhDal.XoaChiNhanh(maChiNhanh);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xóa chi nhánh: {ex.Message}");
            }
        }

        public DataTable TimKiemChiNhanh(string tuKhoa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tuKhoa))
                    return LayTatCaChiNhanh();

                return _chiNhanhDal.TimKiemChiNhanh(tuKhoa);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tìm kiếm chi nhánh: {ex.Message}");
            }
        }

        public DataTable LayThongKeChiNhanh()
        {
            try
            {
                return _chiNhanhDal.LayThongKeChiNhanh();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy thống kê chi nhánh: {ex.Message}");
            }
        }
    }
}
