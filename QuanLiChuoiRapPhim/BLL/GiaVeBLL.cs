using System;
using System.Data;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class GiaVeBLL
    {
        private readonly GiaVeDAL _giaVeDal;

        public GiaVeBLL()
        {
            _giaVeDal = new GiaVeDAL();
        }

        public DataTable LayTatCaCauHinhGiaVe()
        {
            try
            {
                return _giaVeDal.LayTatCaCauHinhGiaVe();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy cấu hình giá vé: {ex.Message}");
            }
        }

        public bool CapNhatGiaVe(int maCauHinh, decimal giaGoc, decimal phuThu)
        {
            try
            {
                if (giaGoc <= 0)
                    throw new Exception("Giá gốc phải lớn hơn 0!");

                return _giaVeDal.CapNhatGiaVe(maCauHinh, giaGoc, phuThu);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi cập nhật giá vé: {ex.Message}");
            }
        }

        public decimal TinhGiaVe(string loaiGhe, string loaiNgay, string doiTuongKhachHang)
        {
            try
            {
                return _giaVeDal.TinhGiaVe(loaiGhe, loaiNgay, doiTuongKhachHang);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tính giá vé: {ex.Message}");
            }
        }
    }
}
