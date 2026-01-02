using System;
using System.Data;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    internal class PhimBLL
    {
        private readonly PhimDAL _phimDal;

        public PhimBLL()
        {
            _phimDal = new PhimDAL();
        }

        public DataTable LayTatCaPhim()
        {
            try
            {
                return _phimDal.GetAllPhim();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách phim: {ex.Message}");
            }
        }

        public bool ThemPhim(string tenPhim, string theLoai, int thoiLuong, string daoDien, 
            string dienVien, string moTa, string doTuoi, DateTime ngayKhoiChieu, string hinhAnh = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenPhim))
                    throw new Exception("Tên phim không được để trống!");
                if (thoiLuong <= 0)
                    throw new Exception("Thời lượng phim phải lớn hơn 0!");

                return _phimDal.ThemPhim(tenPhim, theLoai, thoiLuong, daoDien, dienVien, moTa, doTuoi, ngayKhoiChieu, hinhAnh);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thêm phim: {ex.Message}");
            }
        }

        public bool CapNhatPhim(int maPhim, string tenPhim, string theLoai, int thoiLuong, 
            string daoDien, string dienVien, string moTa, string doTuoi, DateTime ngayKhoiChieu)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenPhim))
                    throw new Exception("Tên phim không được để trống!");
                if (thoiLuong <= 0)
                    throw new Exception("Thời lượng phim phải lớn hơn 0!");

                return _phimDal.CapNhatPhim(maPhim, tenPhim, theLoai, thoiLuong, daoDien, dienVien, moTa, doTuoi, ngayKhoiChieu);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi cập nhật phim: {ex.Message}");
            }
        }

        public bool XoaPhim(int maPhim)
        {
            try
            {
                return _phimDal.XoaPhim(maPhim);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xóa phim: {ex.Message}");
            }
        }

        public DataTable TimKiemPhim(string tuKhoa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tuKhoa))
                    return LayTatCaPhim();

                return _phimDal.TimKiemPhim(tuKhoa);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tìm kiếm phim: {ex.Message}");
            }
        }
    }
}
