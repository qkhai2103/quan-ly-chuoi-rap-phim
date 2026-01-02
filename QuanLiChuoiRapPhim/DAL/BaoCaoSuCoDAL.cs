// File: BaoCaoSuCoDAL.cs
// DAL layer for Room Issue Reports (Báo cáo sự cố phòng)

using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    public class BaoCaoSuCoDAL
    {
        private string _connectionString;

        public BaoCaoSuCoDAL()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        /// <summary>
        /// Lấy danh sách báo cáo sự cố theo chi nhánh
        /// </summary>
        public DataTable LayBaoCaoSuCoTheoChiNhanh(int maChiNhanh)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        bc.MaBaoCao,
                        p.TenPhong,
                        bc.LoaiSuCo,
                        bc.MoTa,
                        CASE bc.TrangThai 
                            WHEN N'ChoXuLy' THEN N'Chờ xử lý'
                            WHEN N'DangXuLy' THEN N'Đang sửa'
                            WHEN N'DaXong' THEN N'Đã xong'
                            WHEN N'KhongXuLy' THEN N'Không xử lý'
                            ELSE bc.TrangThai
                        END AS TrangThai,
                        CASE bc.MucDoUuTien
                            WHEN N'KhanCap' THEN N'Khẩn cấp'
                            WHEN N'Cao' THEN N'Cao'
                            WHEN N'BinhThuong' THEN N'Trung bình'
                            WHEN N'Thap' THEN N'Thấp'
                            ELSE bc.MucDoUuTien
                        END AS DoUuTien,
                        bc.NgayBaoCao AS NgayBao,
                        nd.HoTen AS NguoiBao,
                        bc.GhiChuXuLy
                    FROM BaoCaoSuCo bc
                    INNER JOIN NguoiDung nd ON bc.MaNguoiDung = nd.MaNguoiDung
                    LEFT JOIN PhongChieu p ON bc.MaPhong = p.MaPhong
                    WHERE p.MaChiNhanh = @MaChiNhanh OR p.MaChiNhanh IS NULL
                    ORDER BY bc.NgayBaoCao DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Lấy tất cả báo cáo sự cố (cho Admin xem tất cả)
        /// </summary>
        public DataTable LayTatCaBaoCaoSuCo()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        bc.MaBaoCao,
                        cn.TenChiNhanh,
                        p.TenPhong,
                        bc.LoaiSuCo,
                        bc.MoTa,
                        CASE bc.TrangThai 
                            WHEN N'ChoXuLy' THEN N'Chờ xử lý'
                            WHEN N'DangXuLy' THEN N'Đang sửa'
                            WHEN N'DaXong' THEN N'Đã xong'
                            WHEN N'KhongXuLy' THEN N'Không xử lý'
                            ELSE bc.TrangThai
                        END AS TrangThai,
                        CASE bc.MucDoUuTien
                            WHEN N'KhanCap' THEN N'Khẩn cấp'
                            WHEN N'Cao' THEN N'Cao'
                            WHEN N'BinhThuong' THEN N'Trung bình'
                            WHEN N'Thap' THEN N'Thấp'
                            ELSE bc.MucDoUuTien
                        END AS DoUuTien,
                        bc.NgayBaoCao AS NgayBao,
                        nd.HoTen AS NguoiBao,
                        bc.GhiChuXuLy
                    FROM BaoCaoSuCo bc
                    INNER JOIN NguoiDung nd ON bc.MaNguoiDung = nd.MaNguoiDung
                    LEFT JOIN PhongChieu p ON bc.MaPhong = p.MaPhong
                    LEFT JOIN ChiNhanh cn ON p.MaChiNhanh = cn.MaChiNhanh
                    ORDER BY bc.NgayBaoCao DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Thêm báo cáo sự cố mới
        /// </summary>
        public int ThemBaoCaoSuCo(int maNguoiDung, int maPhong, int? maGhe, string loaiSuCo, string moTa, string mucDoUuTien)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO BaoCaoSuCo (MaNguoiDung, MaPhong, MaGhe, LoaiSuCo, MoTa, MucDoUuTien, TrangThai, NgayBaoCao)
                    VALUES (@MaNguoiDung, @MaPhong, @MaGhe, @LoaiSuCo, @MoTa, @MucDoUuTien, N'ChoXuLy', GETDATE());
                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                    cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                    cmd.Parameters.AddWithValue("@MaGhe", (object)maGhe ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LoaiSuCo", loaiSuCo);
                    cmd.Parameters.AddWithValue("@MoTa", moTa);
                    cmd.Parameters.AddWithValue("@MucDoUuTien", mucDoUuTien);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái báo cáo sự cố
        /// </summary>
        public bool CapNhatTrangThai(int maBaoCao, string trangThai, int nguoiXuLy, string ghiChu)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE BaoCaoSuCo 
                    SET TrangThai = @TrangThai,
                        NguoiXuLy = @NguoiXuLy,
                        NgayXuLy = CASE WHEN @TrangThai IN (N'DaXong', N'KhongXuLy') THEN GETDATE() ELSE NgayXuLy END,
                        GhiChuXuLy = @GhiChu
                    WHERE MaBaoCao = @MaBaoCao";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaBaoCao", maBaoCao);
                    cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                    cmd.Parameters.AddWithValue("@NguoiXuLy", nguoiXuLy);
                    cmd.Parameters.AddWithValue("@GhiChu", ghiChu ?? string.Empty);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Lấy thống kê báo cáo sự cố theo chi nhánh
        /// </summary>
        public DataTable LayThongKeSuCo(int maChiNhanh)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        COUNT(*) AS TongSo,
                        SUM(CASE WHEN bc.TrangThai = N'ChoXuLy' THEN 1 ELSE 0 END) AS ChoXuLy,
                        SUM(CASE WHEN bc.TrangThai = N'DangXuLy' THEN 1 ELSE 0 END) AS DangXuLy,
                        SUM(CASE WHEN bc.TrangThai = N'DaXong' THEN 1 ELSE 0 END) AS DaXong
                    FROM BaoCaoSuCo bc
                    LEFT JOIN PhongChieu p ON bc.MaPhong = p.MaPhong
                    WHERE p.MaChiNhanh = @MaChiNhanh OR @MaChiNhanh = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Lấy danh sách phòng chiếu theo chi nhánh
        /// </summary>
        public DataTable LayPhongChieuTheoChiNhanh(int maChiNhanh)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT MaPhong, TenPhong 
                    FROM PhongChieu 
                    WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                    ORDER BY TenPhong";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
    }
}
