// File: DeXuatLichChieuDAL.cs
// DAL layer for Showtime Proposals (Đề xuất lịch chiếu từ Quản lý chi nhánh)

using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    public class DeXuatLichChieuDAL
    {
        private string _connectionString;

        public DeXuatLichChieuDAL()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        /// <summary>
        /// Lấy danh sách đề xuất theo chi nhánh và trạng thái
        /// </summary>
        public DataTable LayDeXuatTheoChiNhanh(int? maChiNhanh = null, string trangThai = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LayDeXuatLichChieu", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaChiNhanh", (object)maChiNhanh ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrangThai", (object)trangThai ?? DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Lấy tất cả đề xuất (cho Admin)
        /// </summary>
        public DataTable LayTatCaDeXuat(string trangThai = null)
        {
            return LayDeXuatTheoChiNhanh(null, trangThai);
        }

        /// <summary>
        /// Lấy tất cả đề xuất chờ duyệt (cho Admin) với thông tin chi tiết
        /// </summary>
        public DataTable LayTatCaDeXuatChoDuyet()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            dx.MaDeXuat,
                            dx.MaPhim,
                            p.TenPhim,
                            p.TheLoai,
                            dx.MaChiNhanh,
                            cn.TenChiNhanh,
                            dx.LoaiDeXuat,
                            CASE dx.LoaiDeXuat 
                                WHEN 'GiamSuatChieu' THEN N'Giảm suất chiếu'
                                WHEN 'XoaPhim' THEN N'Xóa phím'
                                ELSE dx.LoaiDeXuat 
                            END AS LoaiDeXuatText,
                            dx.LyDo,
                            dx.TrangThai,
                            dx.NgayTao,
                            dx.MaNguoiDeXuat,
                            nd.HoTen AS NguoiDeXuat
                        FROM DeXuatLichChieu dx
                        INNER JOIN Phim p ON dx.MaPhim = p.MaPhim
                        INNER JOIN ChiNhanh cn ON dx.MaChiNhanh = cn.MaChiNhanh
                        INNER JOIN NguoiDung nd ON dx.MaNguoiDeXuat = nd.MaNguoiDung
                        WHERE dx.TrangThai = N'ChoDuyet'
                        ORDER BY dx.NgayTao DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting pending proposals: {ex.Message}");
            }
            return dt;
        }

        /// <summary>
        /// Thêm đề xuất mới
        /// </summary>
        public int ThemDeXuat(int maNguoiDeXuat, int maPhim, int maChiNhanh, string loaiDeXuat, string lyDo, string thongTinBoSung = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Kiểm tra xem stored procedure có tồn tại không
                string checkSP = @"
                    IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ThemDeXuatLichChieu')
                        SELECT 1
                    ELSE
                        SELECT 0";
                
                conn.Open();
                using (SqlCommand checkCmd = new SqlCommand(checkSP, conn))
                {
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (exists == 0)
                    {
                        // Fallback: sử dụng INSERT trực tiếp
                        return ThemDeXuatDirect(conn, maNguoiDeXuat, maPhim, maChiNhanh, loaiDeXuat, lyDo, thongTinBoSung);
                    }
                }

                using (SqlCommand cmd = new SqlCommand("sp_ThemDeXuatLichChieu", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaNguoiDeXuat", maNguoiDeXuat);
                    cmd.Parameters.AddWithValue("@MaPhim", maPhim);
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@LoaiDeXuat", loaiDeXuat);
                    cmd.Parameters.AddWithValue("@LyDo", lyDo);
                    cmd.Parameters.AddWithValue("@ThongTinBoSung", (object)thongTinBoSung ?? DBNull.Value);

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private int ThemDeXuatDirect(SqlConnection conn, int maNguoiDeXuat, int maPhim, int maChiNhanh, string loaiDeXuat, string lyDo, string thongTinBoSung)
        {
            string query = @"
                INSERT INTO DeXuatLichChieu (MaNguoiDeXuat, MaPhim, MaChiNhanh, LoaiDeXuat, LyDo, ThongTinBoSung)
                VALUES (@MaNguoiDeXuat, @MaPhim, @MaChiNhanh, @LoaiDeXuat, @LyDo, @ThongTinBoSung);
                SELECT SCOPE_IDENTITY();";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaNguoiDeXuat", maNguoiDeXuat);
                cmd.Parameters.AddWithValue("@MaPhim", maPhim);
                cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                cmd.Parameters.AddWithValue("@LoaiDeXuat", loaiDeXuat);
                cmd.Parameters.AddWithValue("@LyDo", lyDo);
                cmd.Parameters.AddWithValue("@ThongTinBoSung", (object)thongTinBoSung ?? DBNull.Value);

                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        /// <summary>
        /// Duyệt hoặc từ chối đề xuất (dành cho Admin) - Tự động tạo thông báo
        /// </summary>
        public bool DuyetDeXuat(int maDeXuat, string trangThai, int nguoiDuyet, string ghiChuDuyet = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Lấy thông tin đề xuất trước khi cập nhật
                string getInfoQuery = @"
                    SELECT dx.MaNguoiDeXuat, p.TenPhim, dx.LoaiDeXuat 
                    FROM DeXuatLichChieu dx
                    INNER JOIN Phim p ON dx.MaPhim = p.MaPhim
                    WHERE dx.MaDeXuat = @MaDeXuat";
                
                int nguoiDeXuat = 0;
                string tenPhim = "";
                string loaiDeXuat = "";
                
                using (SqlCommand infoCmd = new SqlCommand(getInfoQuery, conn))
                {
                    infoCmd.Parameters.AddWithValue("@MaDeXuat", maDeXuat);
                    using (SqlDataReader reader = infoCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nguoiDeXuat = reader.GetInt32(0);
                            tenPhim = reader.GetString(1);
                            loaiDeXuat = reader.GetString(2);
                        }
                    }
                }
                
                // Kiểm tra stored procedure
                string checkSP = "SELECT COUNT(*) FROM sys.procedures WHERE name = 'sp_DuyetDeXuat'";
                bool success = false;
                
                using (SqlCommand checkCmd = new SqlCommand(checkSP, conn))
                {
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (exists == 0)
                    {
                        success = DuyetDeXuatDirect(conn, maDeXuat, trangThai, nguoiDuyet, ghiChuDuyet);
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_DuyetDeXuat", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaDeXuat", maDeXuat);
                            cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                            cmd.Parameters.AddWithValue("@NguoiDuyet", nguoiDuyet);
                            cmd.Parameters.AddWithValue("@GhiChuDuyet", (object)ghiChuDuyet ?? DBNull.Value);

                            cmd.ExecuteNonQuery();
                            success = true;
                        }
                    }
                }
                
                // Tạo thông báo cho người đề xuất
                if (success && nguoiDeXuat > 0)
                {
                    try
                    {
                        var thongBaoDAL = new ThongBaoDAL();
                        thongBaoDAL.CreateTableIfNotExists(); // Tự tạo table nếu chưa có
                        
                        string loaiThongBao = trangThai == "DaDuyet" ? "DeXuatDaDuyet" : "DeXuatBiTuChoi";
                        string loaiText = loaiDeXuat == "GiamSuatChieu" ? "giảm suất chiếu" : "xóa phím";
                        string tieuDe = trangThai == "DaDuyet" 
                            ? $"\u2705 Đề xuất đã được duyệt" 
                            : $"\u274c Đề xuất bị từ chối";
                        string noiDung = trangThai == "DaDuyet"
                            ? $"Đề xuất {loaiText} phím \"{tenPhim}\" đã được Admin duyệt."
                            : $"Đề xuất {loaiText} phím \"{tenPhim}\" bị từ chối.{(string.IsNullOrEmpty(ghiChuDuyet) ? "" : $" Lý do: {ghiChuDuyet}")}";
                        
                        thongBaoDAL.ThemThongBao(nguoiDeXuat, tieuDe, noiDung, loaiThongBao, $"DeXuat:{maDeXuat}", nguoiDuyet);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating notification: {ex.Message}");
                    }
                }
                
                return success;
            }
        }

        private bool DuyetDeXuatDirect(SqlConnection conn, int maDeXuat, string trangThai, int nguoiDuyet, string ghiChuDuyet)
        {
            string query = @"
                UPDATE DeXuatLichChieu
                SET TrangThai = @TrangThai,
                    NguoiDuyet = @NguoiDuyet,
                    NgayDuyet = GETDATE(),
                    GhiChuDuyet = @GhiChuDuyet,
                    NgayCapNhat = GETDATE()
                WHERE MaDeXuat = @MaDeXuat";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaDeXuat", maDeXuat);
                cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                cmd.Parameters.AddWithValue("@NguoiDuyet", nguoiDuyet);
                cmd.Parameters.AddWithValue("@GhiChuDuyet", (object)ghiChuDuyet ?? DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Lấy tất cả phim đang chiếu trong chi nhánh với thống kê doanh thu
        /// Sắp xếp theo doanh thu tăng dần để dễ nhận diện phim cần giảm suất
        /// </summary>
        public DataTable LayPhimDoanhThuThap(int maChiNhanh, int soNgayGanDay = 30)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        p.MaPhim,
                        p.TenPhim,
                        p.TheLoai,
                        @MaChiNhanh AS MaChiNhanh,
                        ISNULL(COUNT(DISTINCT sc.MaSuatChieu), 0) AS SoSuatChieu,
                        ISNULL(COUNT(v.MaVe), 0) AS SoVeBan,
                        ISNULL(SUM(v.GiaVe), 0) AS DoanhThu,
                        CASE 
                            WHEN ISNULL(COUNT(v.MaVe), 0) = 0 THEN 0
                            ELSE CAST(COUNT(v.MaVe) AS FLOAT) / NULLIF(SUM(pc.TongSoGhe), 1) * 100
                        END AS TyLeLapDay
                    FROM Phim p
                    LEFT JOIN SuatChieu sc ON p.MaPhim = sc.MaPhim 
                        AND sc.NgayChieu >= DATEADD(DAY, -@SoNgay, GETDATE())
                    LEFT JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong 
                        AND pc.MaChiNhanh = @MaChiNhanh
                    LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu AND v.TrangThai = N'DaBan'
                    WHERE p.TrangThai = 1
                    GROUP BY p.MaPhim, p.TenPhim, p.TheLoai
                    ORDER BY DoanhThu ASC, TyLeLapDay ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@SoNgay", soNgayGanDay);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Lấy thống kê đề xuất theo trạng thái
        /// </summary>
        public DataTable LayThongKeDeXuat(int? maChiNhanh = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        COUNT(*) AS TongSo,
                        SUM(CASE WHEN TrangThai = N'ChoDuyet' THEN 1 ELSE 0 END) AS ChoDuyet,
                        SUM(CASE WHEN TrangThai = N'DaDuyet' THEN 1 ELSE 0 END) AS DaDuyet,
                        SUM(CASE WHEN TrangThai = N'TuChoi' THEN 1 ELSE 0 END) AS TuChoi
                    FROM DeXuatLichChieu
                    WHERE (@MaChiNhanh IS NULL OR MaChiNhanh = @MaChiNhanh)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", (object)maChiNhanh ?? DBNull.Value);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Hủy đề xuất (dành cho Quản lý chi nhánh)
        /// </summary>
        public bool HuyDeXuat(int maDeXuat, int maNguoiHuy)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE DeXuatLichChieu
                    SET TrangThai = N'DaHuy',
                        NgayCapNhat = GETDATE()
                    WHERE MaDeXuat = @MaDeXuat 
                      AND MaNguoiDeXuat = @MaNguoiHuy
                      AND TrangThai = N'ChoDuyet'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaDeXuat", maDeXuat);
                    cmd.Parameters.AddWithValue("@MaNguoiHuy", maNguoiHuy);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
