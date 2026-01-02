using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    /// <summary>
    /// Data Access Layer cho bảng GheNgoi
    /// </summary>
    public class GheDAL
    {
        /// <summary>
        /// Lấy danh sách ghế theo phòng chiếu
        /// </summary>
        public DataTable LayGheTheoPhong(int maPhong)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT MaGhe, MaPhong, SoGhe, SoHang, LoaiGhe, TrangThai
                FROM GheNgoi 
                WHERE MaPhong = @MaPhong
                ORDER BY SoHang, SoGhe";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Thêm ghế mới vào phòng
        /// </summary>
        public bool ThemGhe(int maPhong, string soGhe, string soHang, string loaiGhe)
        {
            string query = @"
                INSERT INTO GheNgoi (MaPhong, SoGhe, SoHang, LoaiGhe, TrangThai)
                VALUES (@MaPhong, @SoGhe, @SoHang, @LoaiGhe, 1)";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                    cmd.Parameters.AddWithValue("@SoGhe", soGhe);
                    cmd.Parameters.AddWithValue("@SoHang", soHang);
                    cmd.Parameters.AddWithValue("@LoaiGhe", loaiGhe);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Cập nhật loại ghế và trạng thái
        /// </summary>
        public bool CapNhatGhe(int maGhe, string loaiGhe, bool trangThai)
        {
            string query = @"
                UPDATE GheNgoi 
                SET LoaiGhe = @LoaiGhe, TrangThai = @TrangThai
                WHERE MaGhe = @MaGhe";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaGhe", maGhe);
                    cmd.Parameters.AddWithValue("@LoaiGhe", loaiGhe);
                    cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Xóa tất cả ghế của một phòng (dùng khi redesign)
        /// </summary>
        public bool XoaGheTheoPhong(int maPhong)
        {
            string query = "DELETE FROM GheNgoi WHERE MaPhong = @MaPhong";

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                    return cmd.ExecuteNonQuery() >= 0; // Có thể không có ghế nào
                }
            }
        }

        /// <summary>
        /// Lưu toàn bộ sơ đồ ghế cho phòng (xóa cũ và thêm mới)
        /// </summary>
        public bool LuuSoDoGhe(int maPhong, DataTable gheData)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Xóa ghế cũ (chỉ nếu không có vé nào đã bán)
                        string checkQuery = @"
                            SELECT COUNT(*) FROM Ve v 
                            INNER JOIN GheNgoi g ON v.MaGhe = g.MaGhe 
                            WHERE g.MaPhong = @MaPhong";
                        
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@MaPhong", maPhong);
                            int veCount = (int)checkCmd.ExecuteScalar();
                            
                            if (veCount > 0)
                            {
                                // Chỉ update loại ghế, không xóa
                                foreach (DataRow row in gheData.Rows)
                                {
                                    string updateQuery = @"
                                        UPDATE GheNgoi SET LoaiGhe = @LoaiGhe, TrangThai = @TrangThai
                                        WHERE MaPhong = @MaPhong AND SoGhe = @SoGhe";
                                    
                                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction))
                                    {
                                        updateCmd.Parameters.AddWithValue("@MaPhong", maPhong);
                                        updateCmd.Parameters.AddWithValue("@SoGhe", row["SoGhe"]);
                                        updateCmd.Parameters.AddWithValue("@LoaiGhe", row["LoaiGhe"]);
                                        updateCmd.Parameters.AddWithValue("@TrangThai", row["TrangThai"]);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            else
                            {
                                // Xóa và thêm mới
                                string deleteQuery = "DELETE FROM GheNgoi WHERE MaPhong = @MaPhong";
                                using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn, transaction))
                                {
                                    deleteCmd.Parameters.AddWithValue("@MaPhong", maPhong);
                                    deleteCmd.ExecuteNonQuery();
                                }
                                
                                // Thêm ghế mới
                                foreach (DataRow row in gheData.Rows)
                                {
                                    string insertQuery = @"
                                        INSERT INTO GheNgoi (MaPhong, SoGhe, SoHang, LoaiGhe, TrangThai)
                                        VALUES (@MaPhong, @SoGhe, @SoHang, @LoaiGhe, @TrangThai)";
                                    
                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@MaPhong", maPhong);
                                        insertCmd.Parameters.AddWithValue("@SoGhe", row["SoGhe"]);
                                        insertCmd.Parameters.AddWithValue("@SoHang", row["SoHang"]);
                                        insertCmd.Parameters.AddWithValue("@LoaiGhe", row["LoaiGhe"]);
                                        insertCmd.Parameters.AddWithValue("@TrangThai", row["TrangThai"]);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}

