using System;
using System.Data;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class PhongChieuBLL
    {
        private readonly PhongChieuDAL _phongChieuDal;

        public PhongChieuBLL()
        {
            _phongChieuDal = new PhongChieuDAL();
        }

        public DataTable LayTatCaPhongChieu()
        {
            try
            {
                return _phongChieuDal.LayTatCaPhongChieu();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách phòng chiếu: {ex.Message}");
            }
        }

        public bool ThemPhongChieu(string tenPhong, int maChiNhanh, int tongSoGhe)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenPhong))
                    throw new Exception("Tên phòng không được để trống!");
                if (tongSoGhe <= 0)
                    throw new Exception("Số ghế phải lớn hơn 0!");

                return _phongChieuDal.ThemPhongChieu(tenPhong, maChiNhanh, tongSoGhe);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thêm phòng chiếu: {ex.Message}");
            }
        }

        public bool CapNhatPhongChieu(int maPhong, string tenPhong, int tongSoGhe)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenPhong))
                    throw new Exception("Tên phòng không được để trống!");
                if (tongSoGhe <= 0)
                    throw new Exception("Số ghế phải lớn hơn 0!");

                return _phongChieuDal.CapNhatPhongChieu(maPhong, tenPhong, tongSoGhe);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi cập nhật phòng chiếu: {ex.Message}");
            }
        }

        public bool XoaPhongChieu(int maPhong)
        {
            try
            {
                return _phongChieuDal.XoaPhongChieu(maPhong);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xóa phòng chiếu: {ex.Message}");
            }
        }

        public DataTable LayGheTrong(int maSuatChieu)
        {
            try
            {
                return _phongChieuDal.LayGheTrong(maSuatChieu);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy ghế trống: {ex.Message}");
            }
        }
    }
}
