using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    /// <summary>
    /// Data Access Layer for ThongBao (Notification) operations
    /// </summary>
    public class ThongBaoDAL
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        /// <summary>
        /// Thêm thông báo mới
        /// </summary>
        public int ThemThongBao(int nguoiNhan, string tieuDe, string noiDung, string loaiThongBao, string lienKet = null, int? nguoiGui = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO ThongBao (MaNguoiNhan, TieuDe, NoiDung, LoaiThongBao, LienKet, MaNguoiGui, DaDoc, NgayTao)
                        VALUES (@NguoiNhan, @TieuDe, @NoiDung, @LoaiThongBao, @LienKet, @NguoiGui, 0, GETUTCDATE());
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NguoiNhan", nguoiNhan);
                        cmd.Parameters.AddWithValue("@TieuDe", tieuDe);
                        cmd.Parameters.AddWithValue("@NoiDung", noiDung ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LoaiThongBao", loaiThongBao);
                        cmd.Parameters.AddWithValue("@LienKet", lienKet ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NguoiGui", nguoiGui.HasValue ? (object)nguoiGui.Value : DBNull.Value);

                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding notification: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Lấy danh sách thông báo của người dùng (mới nhất trước)
        /// </summary>
        public DataTable LayThongBao(int maNguoiDung, int soLuong = 50)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = $@"
                        SELECT TOP {soLuong}
                            tb.MaThongBao,
                            tb.TieuDe,
                            tb.NoiDung,
                            tb.LoaiThongBao,
                            tb.DaDoc,
                            tb.NgayTao,
                            tb.LienKet,
                            ng.HoTen AS NguoiGui
                        FROM ThongBao tb
                        LEFT JOIN NguoiDung ng ON tb.MaNguoiGui = ng.MaNguoiDung
                        WHERE tb.MaNguoiNhan = @MaNguoiDung
                        ORDER BY tb.NgayTao DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting notifications: {ex.Message}");
            }
            return dt;
        }

        /// <summary>
        /// Lấy thông báo chưa đọc
        /// </summary>
        public DataTable LayThongBaoChuaDoc(int maNguoiDung)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            tb.MaThongBao,
                            tb.TieuDe,
                            tb.NoiDung,
                            tb.LoaiThongBao,
                            tb.NgayTao,
                            tb.LienKet,
                            ng.HoTen AS NguoiGui
                        FROM ThongBao tb
                        LEFT JOIN NguoiDung ng ON tb.MaNguoiGui = ng.MaNguoiDung
                        WHERE tb.MaNguoiNhan = @MaNguoiDung AND tb.DaDoc = 0
                        ORDER BY tb.NgayTao DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting unread notifications: {ex.Message}");
            }
            return dt;
        }

        /// <summary>
        /// Đếm số thông báo chưa đọc (cho badge)
        /// </summary>
        public int DemThongBaoChuaDoc(int maNguoiDung)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM ThongBao WHERE MaNguoiNhan = @MaNguoiDung AND DaDoc = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting notifications: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Đánh dấu một thông báo đã đọc
        /// </summary>
        public bool DanhDauDaDoc(int maThongBao)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE ThongBao SET DaDoc = 1 WHERE MaThongBao = @MaThongBao";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaThongBao", maThongBao);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking notification as read: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo đã đọc
        /// </summary>
        public bool DanhDauTatCaDaDoc(int maNguoiDung)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE ThongBao SET DaDoc = 1 WHERE MaNguoiNhan = @MaNguoiDung AND DaDoc = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                        return cmd.ExecuteNonQuery() >= 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking all notifications as read: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa thông báo cũ (hơn 30 ngày)
        /// </summary>
        public int XoaThongBaoCu(int soNgay = 30)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM ThongBao WHERE NgayTao < DATEADD(DAY, -@SoNgay, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SoNgay", soNgay);
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting old notifications: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Kiểm tra table ThongBao đã tồn tại chưa
        /// </summary>
        public bool TableExists()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ThongBao'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tạo table ThongBao nếu chưa có
        /// </summary>
        public bool CreateTableIfNotExists()
        {
            if (TableExists()) return true;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        CREATE TABLE ThongBao (
                            MaThongBao INT IDENTITY(1,1) PRIMARY KEY,
                            MaNguoiNhan INT NOT NULL,
                            TieuDe NVARCHAR(200) NOT NULL,
                            NoiDung NVARCHAR(500),
                            LoaiThongBao NVARCHAR(50) NOT NULL,
                            DaDoc BIT DEFAULT 0,
                            NgayTao DATETIME DEFAULT GETUTCDATE(),
                            LienKet NVARCHAR(100),
                            MaNguoiGui INT,
                            CONSTRAINT FK_ThongBao_NguoiNhan FOREIGN KEY (MaNguoiNhan) REFERENCES NguoiDung(MaNguoiDung),
                            CONSTRAINT FK_ThongBao_NguoiGui FOREIGN KEY (MaNguoiGui) REFERENCES NguoiDung(MaNguoiDung)
                        );
                        CREATE INDEX IX_ThongBao_NguoiNhan ON ThongBao(MaNguoiNhan, DaDoc);
                        CREATE INDEX IX_ThongBao_NgayTao ON ThongBao(NgayTao DESC);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating ThongBao table: {ex.Message}");
                return false;
            }
        }
    }
}
