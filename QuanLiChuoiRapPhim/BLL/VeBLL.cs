using System;
using System.Data;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class VeBLL
    {
        private readonly VeDAL _veDal;

        public VeBLL()
        {
            _veDal = new VeDAL();
        }

        public DataTable LayTatCaVe()
        {
            try
            {
                return _veDal.LayTatCaVe();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách vé: {ex.Message}");
            }
        }

        public DataTable LayVeTheoSuatChieu(int maSuatChieu)
        {
            try
            {
                return _veDal.LayVeTheoSuatChieu(maSuatChieu);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy vé theo suất chiếu: {ex.Message}");
            }
        }

        public bool BanVe(int maSuatChieu, int maGhe, int? maKhachHang, string doiTuongKhachHang, decimal giaVe)
        {
            try
            {
                if (giaVe <= 0)
                    throw new Exception("Giá vé phải lớn hơn 0!");

                return _veDal.BanVe(maSuatChieu, maGhe, maKhachHang, doiTuongKhachHang, giaVe);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi bán vé: {ex.Message}");
            }
        }

        public bool HuyVe(int maVe)
        {
            try
            {
                return _veDal.HuyVe(maVe);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi hủy vé: {ex.Message}");
            }
        }

        public DataTable ThongKeVeTheoPhim()
        {
            try
            {
                return _veDal.ThongKeVeTheoPhim();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thống kê vé: {ex.Message}");
            }
        }
    }
}
