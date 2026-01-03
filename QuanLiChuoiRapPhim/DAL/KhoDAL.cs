using System;
using System.Data;
using System.Data.SqlClient;

namespace QuanLiChuoiRapPhim.DAL
{
    /// <summary>
    /// Data Access Layer cho Quản lý Kho
    /// Bao gồm: Sản phẩm, Nhập kho, Xuất kho, Nhà cung cấp, Kiểm kê
    /// </summary>
    public class KhoDAL
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        #region === SẢN PHẨM ===

        /// <summary>
        /// Lấy danh sách sản phẩm theo chi nhánh
        /// </summary>
        public DataTable GetSanPhamByChiNhanh(int maChiNhanh)
        {
            string query = @"
                SELECT 
                    MaSanPham, TenSanPham, LoaiSanPham, GiaBan, DonVi,
                    SoLuongTon, HinhAnh, TrangThai, NgayTao
                FROM SanPham
                WHERE MaChiNhanh = @MaChiNhanh
                ORDER BY LoaiSanPham, TenSanPham";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm đang hoạt động
        /// </summary>
        public DataTable GetSanPhamActive(int maChiNhanh)
        {
            string query = @"
                SELECT 
                    MaSanPham, TenSanPham, LoaiSanPham, GiaBan, DonVi,
                    SoLuongTon, HinhAnh
                FROM SanPham
                WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                ORDER BY LoaiSanPham, TenSanPham";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Thêm sản phẩm mới
        /// </summary>
        public int ThemSanPham(string tenSP, string loaiSP, decimal giaBan, string donVi, int soLuongTon, int maChiNhanh, string hinhAnh = null)
        {
            string query = @"
                INSERT INTO SanPham (TenSanPham, LoaiSanPham, GiaBan, DonVi, SoLuongTon, MaChiNhanh, HinhAnh)
                VALUES (@TenSanPham, @LoaiSanPham, @GiaBan, @DonVi, @SoLuongTon, @MaChiNhanh, @HinhAnh);
                SELECT SCOPE_IDENTITY();";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenSanPham", tenSP);
                    cmd.Parameters.AddWithValue("@LoaiSanPham", loaiSP);
                    cmd.Parameters.AddWithValue("@GiaBan", giaBan);
                    cmd.Parameters.AddWithValue("@DonVi", donVi ?? "");
                    cmd.Parameters.AddWithValue("@SoLuongTon", soLuongTon);
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@HinhAnh", (object)hinhAnh ?? DBNull.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        public bool CapNhatSanPham(int maSP, string tenSP, string loaiSP, decimal giaBan, string donVi, string hinhAnh = null)
        {
            string query = @"
                UPDATE SanPham 
                SET TenSanPham = @TenSanPham, LoaiSanPham = @LoaiSanPham, 
                    GiaBan = @GiaBan, DonVi = @DonVi, HinhAnh = @HinhAnh
                WHERE MaSanPham = @MaSanPham";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSanPham", maSP);
                    cmd.Parameters.AddWithValue("@TenSanPham", tenSP);
                    cmd.Parameters.AddWithValue("@LoaiSanPham", loaiSP);
                    cmd.Parameters.AddWithValue("@GiaBan", giaBan);
                    cmd.Parameters.AddWithValue("@DonVi", donVi ?? "");
                    cmd.Parameters.AddWithValue("@HinhAnh", (object)hinhAnh ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Cập nhật số lượng tồn kho
        /// </summary>
        public bool CapNhatSoLuongTon(int maSP, int soLuongMoi)
        {
            string query = "UPDATE SanPham SET SoLuongTon = @SoLuong WHERE MaSanPham = @MaSanPham";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SoLuong", soLuongMoi);
                    cmd.Parameters.AddWithValue("@MaSanPham", maSP);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Tăng/Giảm số lượng tồn kho
        /// </summary>
        public bool DieuChinhSoLuong(int maSP, int soLuongThayDoi)
        {
            string query = "UPDATE SanPham SET SoLuongTon = SoLuongTon + @SoLuong WHERE MaSanPham = @MaSanPham";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SoLuong", soLuongThayDoi);
                    cmd.Parameters.AddWithValue("@MaSanPham", maSP);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Xóa sản phẩm (soft delete)
        /// </summary>
        public bool XoaSanPham(int maSP)
        {
            string query = "UPDATE SanPham SET TrangThai = 0 WHERE MaSanPham = @MaSanPham";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSanPham", maSP);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Lấy sản phẩm sắp hết hàng
        /// </summary>
        public DataTable GetSanPhamSapHet(int maChiNhanh, int nguong = 10)
        {
            string query = @"
                SELECT MaSanPham, TenSanPham, LoaiSanPham, SoLuongTon, DonVi
                FROM SanPham
                WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1 AND SoLuongTon <= @Nguong
                ORDER BY SoLuongTon ASC";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@Nguong", nguong);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        #endregion

        #region === NHÀ CUNG CẤP ===

        /// <summary>
        /// Kiểm tra bảng NhaCungCap có tồn tại không
        /// </summary>
        public bool CheckNhaCungCapTableExists()
        {
            string query = "SELECT COUNT(*) FROM sys.tables WHERE name = 'NhaCungCap'";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        /// <summary>
        /// Lấy danh sách nhà cung cấp
        /// </summary>
        public DataTable GetNhaCungCap()
        {
            string query = @"
                SELECT MaNhaCungCap, TenNhaCungCap, DiaChi, SoDienThoai, Email, NguoiLienHe, GhiChu
                FROM NhaCungCap
                WHERE TrangThai = 1
                ORDER BY TenNhaCungCap";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Thêm nhà cung cấp mới
        /// </summary>
        public int ThemNhaCungCap(string tenNCC, string diaChi, string sdt, string email, string nguoiLienHe, string ghiChu)
        {
            string query = @"
                INSERT INTO NhaCungCap (TenNhaCungCap, DiaChi, SoDienThoai, Email, NguoiLienHe, GhiChu)
                VALUES (@TenNhaCungCap, @DiaChi, @SoDienThoai, @Email, @NguoiLienHe, @GhiChu);
                SELECT SCOPE_IDENTITY();";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenNhaCungCap", tenNCC);
                    cmd.Parameters.AddWithValue("@DiaChi", diaChi ?? "");
                    cmd.Parameters.AddWithValue("@SoDienThoai", sdt ?? "");
                    cmd.Parameters.AddWithValue("@Email", email ?? "");
                    cmd.Parameters.AddWithValue("@NguoiLienHe", nguoiLienHe ?? "");
                    cmd.Parameters.AddWithValue("@GhiChu", ghiChu ?? "");
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        #endregion

        #region === NHẬP KHO ===

        /// <summary>
        /// Kiểm tra bảng NhapKho có tồn tại không
        /// </summary>
        public bool CheckNhapKhoTableExists()
        {
            string query = "SELECT COUNT(*) FROM sys.tables WHERE name = 'NhapKho'";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        /// <summary>
        /// Lấy danh sách phiếu nhập kho theo chi nhánh
        /// </summary>
        public DataTable GetPhieuNhapKho(int maChiNhanh, DateTime tuNgay, DateTime denNgay)
        {
            string query = @"
                SELECT 
                    n.MaNhapKho,
                    n.NgayNhap,
                    n.TongTien,
                    ISNULL(ncc.TenNhaCungCap, N'Không xác định') AS NhaCungCap,
                    n.SoHoaDon,
                    n.GhiChu,
                    nd.HoTen AS NguoiNhap,
                    (SELECT COUNT(*) FROM ChiTietNhapKho WHERE MaNhapKho = n.MaNhapKho) AS SoLoaiSP,
                    n.TrangThai
                FROM NhapKho n
                LEFT JOIN NhaCungCap ncc ON n.MaNhaCungCap = ncc.MaNhaCungCap
                LEFT JOIN NguoiDung nd ON n.MaNguoiNhap = nd.MaNguoiDung
                WHERE n.MaChiNhanh = @MaChiNhanh
                  AND CAST(n.NgayNhap AS DATE) BETWEEN @TuNgay AND @DenNgay
                ORDER BY n.NgayNhap DESC";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Date);
                    cmd.Parameters.AddWithValue("@DenNgay", denNgay.Date);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Tạo phiếu nhập kho mới
        /// </summary>
        public int TaoPhieuNhapKho(int maChiNhanh, int? maNhaCungCap, int maNguoiNhap, string soHoaDon, string ghiChu, DataTable chiTiet)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Tính tổng tiền
                    decimal tongTien = 0;
                    foreach (DataRow row in chiTiet.Rows)
                    {
                        tongTien += Convert.ToDecimal(row["ThanhTien"]);
                    }

                    // Tạo phiếu nhập
                    string queryNhap = @"
                        INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, TongTien, SoHoaDon, GhiChu)
                        VALUES (@MaChiNhanh, @MaNhaCungCap, @MaNguoiNhap, @TongTien, @SoHoaDon, @GhiChu);
                        SELECT SCOPE_IDENTITY();";

                    int maNhapKho;
                    using (SqlCommand cmd = new SqlCommand(queryNhap, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                        cmd.Parameters.AddWithValue("@MaNhaCungCap", (object)maNhaCungCap ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaNguoiNhap", maNguoiNhap);
                        cmd.Parameters.AddWithValue("@TongTien", tongTien);
                        cmd.Parameters.AddWithValue("@SoHoaDon", soHoaDon ?? "");
                        cmd.Parameters.AddWithValue("@GhiChu", ghiChu ?? "");
                        maNhapKho = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Thêm chi tiết và cập nhật tồn kho
                    foreach (DataRow row in chiTiet.Rows)
                    {
                        int maSP = Convert.ToInt32(row["MaSanPham"]);
                        int soLuong = Convert.ToInt32(row["SoLuong"]);
                        decimal donGia = Convert.ToDecimal(row["DonGia"]);
                        decimal thanhTien = Convert.ToDecimal(row["ThanhTien"]);

                        // Thêm chi tiết
                        string queryChiTiet = @"
                            INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
                            VALUES (@MaNhapKho, @MaSanPham, @SoLuongNhap, @DonGiaNhap, @ThanhTien)";

                        using (SqlCommand cmdCT = new SqlCommand(queryChiTiet, conn, transaction))
                        {
                            cmdCT.Parameters.AddWithValue("@MaNhapKho", maNhapKho);
                            cmdCT.Parameters.AddWithValue("@MaSanPham", maSP);
                            cmdCT.Parameters.AddWithValue("@SoLuongNhap", soLuong);
                            cmdCT.Parameters.AddWithValue("@DonGiaNhap", donGia);
                            cmdCT.Parameters.AddWithValue("@ThanhTien", thanhTien);
                            cmdCT.ExecuteNonQuery();
                        }

                        // Cập nhật tồn kho
                        string queryTon = "UPDATE SanPham SET SoLuongTon = SoLuongTon + @SoLuong WHERE MaSanPham = @MaSanPham";
                        using (SqlCommand cmdTon = new SqlCommand(queryTon, conn, transaction))
                        {
                            cmdTon.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmdTon.Parameters.AddWithValue("@MaSanPham", maSP);
                            cmdTon.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return maNhapKho;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Lấy chi tiết phiếu nhập kho
        /// </summary>
        public DataTable GetChiTietNhapKho(int maNhapKho)
        {
            string query = @"
                SELECT 
                    ct.MaSanPham,
                    sp.TenSanPham,
                    ct.SoLuongNhap AS SoLuong,
                    ct.DonGiaNhap AS DonGia,
                    ct.ThanhTien,
                    sp.DonVi
                FROM ChiTietNhapKho ct
                INNER JOIN SanPham sp ON ct.MaSanPham = sp.MaSanPham
                WHERE ct.MaNhapKho = @MaNhapKho";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaNhapKho", maNhapKho);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        #endregion

        #region === XUẤT KHO ===

        /// <summary>
        /// Kiểm tra bảng XuatKho có tồn tại không
        /// </summary>
        public bool CheckXuatKhoTableExists()
        {
            string query = "SELECT COUNT(*) FROM sys.tables WHERE name = 'XuatKho'";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        /// <summary>
        /// Lấy danh sách phiếu xuất kho
        /// </summary>
        public DataTable GetPhieuXuatKho(int maChiNhanh, string trangThai = null)
        {
            string query = @"
                SELECT 
                    x.MaXuatKho,
                    x.NgayXuat,
                    cnXuat.TenChiNhanh AS ChiNhanhXuat,
                    ISNULL(cnNhan.TenChiNhanh, N'Xuất bán') AS ChiNhanhNhan,
                    x.TongTien,
                    x.LoaiXuat,
                    x.LyDoXuat,
                    x.TrangThai,
                    ndXuat.HoTen AS NguoiXuat,
                    ISNULL(ndXacNhan.HoTen, N'') AS NguoiXacNhan,
                    (SELECT COUNT(*) FROM ChiTietXuatKho WHERE MaXuatKho = x.MaXuatKho) AS SoLoaiSP
                FROM XuatKho x
                INNER JOIN ChiNhanh cnXuat ON x.MaChiNhanhXuat = cnXuat.MaChiNhanh
                LEFT JOIN ChiNhanh cnNhan ON x.MaChiNhanhNhan = cnNhan.MaChiNhanh
                INNER JOIN NguoiDung ndXuat ON x.MaNguoiXuat = ndXuat.MaNguoiDung
                LEFT JOIN NguoiDung ndXacNhan ON x.MaNguoiXacNhan = ndXacNhan.MaNguoiDung
                WHERE (x.MaChiNhanhXuat = @MaChiNhanh OR x.MaChiNhanhNhan = @MaChiNhanh)
                  AND (@TrangThai IS NULL OR x.TrangThai = @TrangThai)
                ORDER BY x.NgayXuat DESC";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@TrangThai", (object)trangThai ?? DBNull.Value);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Tạo phiếu xuất kho
        /// </summary>
        public int TaoPhieuXuatKho(int maChiNhanhXuat, int? maChiNhanhNhan, int maNguoiXuat, string loaiXuat, string lyDo, string ghiChu, DataTable chiTiet)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Tính tổng tiền
                    decimal tongTien = 0;
                    foreach (DataRow row in chiTiet.Rows)
                    {
                        tongTien += Convert.ToDecimal(row["ThanhTien"]);
                    }

                    // Tạo phiếu xuất
                    string queryXuat = @"
                        INSERT INTO XuatKho (MaChiNhanhXuat, MaChiNhanhNhan, MaNguoiXuat, TongTien, LoaiXuat, LyDoXuat, GhiChu)
                        VALUES (@MaChiNhanhXuat, @MaChiNhanhNhan, @MaNguoiXuat, @TongTien, @LoaiXuat, @LyDoXuat, @GhiChu);
                        SELECT SCOPE_IDENTITY();";

                    int maXuatKho;
                    using (SqlCommand cmd = new SqlCommand(queryXuat, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanhXuat", maChiNhanhXuat);
                        cmd.Parameters.AddWithValue("@MaChiNhanhNhan", (object)maChiNhanhNhan ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaNguoiXuat", maNguoiXuat);
                        cmd.Parameters.AddWithValue("@TongTien", tongTien);
                        cmd.Parameters.AddWithValue("@LoaiXuat", loaiXuat);
                        cmd.Parameters.AddWithValue("@LyDoXuat", lyDo ?? "");
                        cmd.Parameters.AddWithValue("@GhiChu", ghiChu ?? "");
                        maXuatKho = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Thêm chi tiết và trừ tồn kho
                    foreach (DataRow row in chiTiet.Rows)
                    {
                        int maSP = Convert.ToInt32(row["MaSanPham"]);
                        int soLuong = Convert.ToInt32(row["SoLuong"]);
                        decimal donGia = Convert.ToDecimal(row["DonGia"]);
                        decimal thanhTien = Convert.ToDecimal(row["ThanhTien"]);

                        // Thêm chi tiết
                        string queryChiTiet = @"
                            INSERT INTO ChiTietXuatKho (MaXuatKho, MaSanPham, SoLuongXuat, DonGiaXuat, ThanhTien)
                            VALUES (@MaXuatKho, @MaSanPham, @SoLuongXuat, @DonGiaXuat, @ThanhTien)";

                        using (SqlCommand cmdCT = new SqlCommand(queryChiTiet, conn, transaction))
                        {
                            cmdCT.Parameters.AddWithValue("@MaXuatKho", maXuatKho);
                            cmdCT.Parameters.AddWithValue("@MaSanPham", maSP);
                            cmdCT.Parameters.AddWithValue("@SoLuongXuat", soLuong);
                            cmdCT.Parameters.AddWithValue("@DonGiaXuat", donGia);
                            cmdCT.Parameters.AddWithValue("@ThanhTien", thanhTien);
                            cmdCT.ExecuteNonQuery();
                        }

                        // Trừ tồn kho chi nhánh xuất
                        string queryTon = "UPDATE SanPham SET SoLuongTon = SoLuongTon - @SoLuong WHERE MaSanPham = @MaSanPham";
                        using (SqlCommand cmdTon = new SqlCommand(queryTon, conn, transaction))
                        {
                            cmdTon.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmdTon.Parameters.AddWithValue("@MaSanPham", maSP);
                            cmdTon.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return maXuatKho;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Lấy chi tiết phiếu xuất kho
        /// </summary>
        public DataTable GetChiTietXuatKho(int maXuatKho)
        {
            string query = @"
                SELECT 
                    ct.MaSanPham,
                    sp.TenSanPham,
                    ct.SoLuongXuat AS SoLuong,
                    ct.DonGiaXuat AS DonGia,
                    ct.ThanhTien,
                    sp.DonVi
                FROM ChiTietXuatKho ct
                INNER JOIN SanPham sp ON ct.MaSanPham = sp.MaSanPham
                WHERE ct.MaXuatKho = @MaXuatKho";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaXuatKho", maXuatKho);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Xác nhận nhận hàng (chuyển kho)
        /// </summary>
        public bool XacNhanNhanHang(int maXuatKho, int maNguoiXacNhan, int maChiNhanhNhan)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Cập nhật trạng thái phiếu xuất
                    string queryUpdate = @"
                        UPDATE XuatKho 
                        SET TrangThai = N'DaNhan', MaNguoiXacNhan = @MaNguoiXacNhan, NgayXacNhan = GETDATE()
                        WHERE MaXuatKho = @MaXuatKho AND MaChiNhanhNhan = @MaChiNhanh";

                    using (SqlCommand cmd = new SqlCommand(queryUpdate, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@MaNguoiXacNhan", maNguoiXacNhan);
                        cmd.Parameters.AddWithValue("@MaXuatKho", maXuatKho);
                        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanhNhan);
                        
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    // Lấy chi tiết và cập nhật tồn kho chi nhánh nhận
                    string queryChiTiet = "SELECT MaSanPham, SoLuongXuat FROM ChiTietXuatKho WHERE MaXuatKho = @MaXuatKho";
                    DataTable dtChiTiet = new DataTable();
                    
                    using (SqlCommand cmd = new SqlCommand(queryChiTiet, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@MaXuatKho", maXuatKho);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dtChiTiet);
                    }

                    foreach (DataRow row in dtChiTiet.Rows)
                    {
                        int maSP = Convert.ToInt32(row["MaSanPham"]);
                        int soLuong = Convert.ToInt32(row["SoLuongXuat"]);

                        // Kiểm tra sản phẩm tương ứng ở chi nhánh nhận
                        string queryCheckSP = @"
                            SELECT MaSanPham FROM SanPham 
                            WHERE MaChiNhanh = @MaChiNhanh 
                              AND TenSanPham = (SELECT TenSanPham FROM SanPham WHERE MaSanPham = @MaSanPhamGoc)";

                        int? maSPNhan = null;
                        using (SqlCommand cmdCheck = new SqlCommand(queryCheckSP, conn, transaction))
                        {
                            cmdCheck.Parameters.AddWithValue("@MaChiNhanh", maChiNhanhNhan);
                            cmdCheck.Parameters.AddWithValue("@MaSanPhamGoc", maSP);
                            var result = cmdCheck.ExecuteScalar();
                            if (result != null)
                                maSPNhan = Convert.ToInt32(result);
                        }

                        if (maSPNhan.HasValue)
                        {
                            // Cộng thêm vào sản phẩm đã có
                            string queryTon = "UPDATE SanPham SET SoLuongTon = SoLuongTon + @SoLuong WHERE MaSanPham = @MaSanPham";
                            using (SqlCommand cmdTon = new SqlCommand(queryTon, conn, transaction))
                            {
                                cmdTon.Parameters.AddWithValue("@SoLuong", soLuong);
                                cmdTon.Parameters.AddWithValue("@MaSanPham", maSPNhan.Value);
                                cmdTon.ExecuteNonQuery();
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

        #endregion

        #region === BÁO CÁO ===

        /// <summary>
        /// Lấy thống kê tổng quan kho
        /// </summary>
        public DataTable GetThongKeKho(int maChiNhanh)
        {
            string query = @"
                SELECT 
                    N'Tổng số loại sản phẩm' AS ChiTieu,
                    COUNT(*) AS GiaTri,
                    N'' AS DonVi
                FROM SanPham WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                
                UNION ALL
                
                SELECT N'Tổng số lượng tồn kho', SUM(SoLuongTon), N'đơn vị'
                FROM SanPham WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                
                UNION ALL
                
                SELECT N'Giá trị tồn kho', SUM(GiaBan * SoLuongTon), N'VNĐ'
                FROM SanPham WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1
                
                UNION ALL
                
                SELECT N'Sản phẩm sắp hết (<=10)', COUNT(*), N'loại'
                FROM SanPham WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1 AND SoLuongTon <= 10
                
                UNION ALL
                
                SELECT N'Sản phẩm hết hàng', COUNT(*), N'loại'
                FROM SanPham WHERE MaChiNhanh = @MaChiNhanh AND TrangThai = 1 AND SoLuongTon = 0";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Lấy thống kê nhập kho theo tháng
        /// </summary>
        public DataTable GetThongKeNhapTheoThang(int maChiNhanh, int nam)
        {
            string query = @"
                SELECT 
                    MONTH(NgayNhap) AS Thang,
                    COUNT(*) AS SoPhieu,
                    SUM(TongTien) AS TongTien
                FROM NhapKho
                WHERE MaChiNhanh = @MaChiNhanh AND YEAR(NgayNhap) = @Nam AND TrangThai = N'DaNhap'
                GROUP BY MONTH(NgayNhap)
                ORDER BY Thang";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@Nam", nam);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        #endregion

        #region === HELPER ===

        /// <summary>
        /// Chạy migration script để tạo các bảng cần thiết
        /// </summary>
        public void RunMigration()
        {
            // Đọc file migration
            string migrationPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "Migrations", 
                "Kho_Tables_Migration.sql");

            if (!System.IO.File.Exists(migrationPath))
            {
                throw new System.IO.FileNotFoundException($"Không tìm thấy file migration: {migrationPath}");
            }

            string script = System.IO.File.ReadAllText(migrationPath);
            
            // Split by GO statements
            string[] batches = script.Split(new[] { "\r\nGO\r\n", "\nGO\n", "\r\nGO", "GO\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                foreach (string batch in batches)
                {
                    if (!string.IsNullOrWhiteSpace(batch))
                    {
                        using (SqlCommand cmd = new SqlCommand(batch, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Insert mock data cho tab Nhập/Xuất kho để demo
        /// </summary>
        public void InsertMockInventoryData(int maChiNhanh, int maNguoiDung)
        {
            try
            {
                // Check if already has data
                string checkQuery = "SELECT COUNT(*) FROM NhapKho WHERE MaChiNhanh = @MaChiNhanh";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Mock data already exists - skipping");
                            return;
                        }
                    }
                }

                // Get sample products
                var dtProducts = GetSanPhamByChiNhanh(maChiNhanh);
                if (dtProducts.Rows.Count < 2)
                {
                    System.Diagnostics.Debug.WriteLine("Need at least 2 products to insert mock data");
                    return;
                }

                int maBap = Convert.ToInt32(dtProducts.Rows[0]["MaSanPham"]);
                int maNuoc = dtProducts.Rows.Count > 1 ? Convert.ToInt32(dtProducts.Rows[1]["MaSanPham"]) : maBap;

                // Insert 5 phiếu nhập
                for (int i = 0; i < 5; i++)
                {
                    int daysAgo = 40 - (i * 8);
                    DataTable chiTietNhap = new DataTable();
                    chiTietNhap.Columns.Add("MaSanPham", typeof(int));
                    chiTietNhap.Columns.Add("SoLuong", typeof(int));
                    chiTietNhap.Columns.Add("DonGia", typeof(decimal));
                    chiTietNhap.Columns.Add("ThanhTien", typeof(decimal));

                    chiTietNhap.Rows.Add(maBap, 300 + i * 50, 25000, (300 + i * 50) * 25000);
                    chiTietNhap.Rows.Add(maNuoc, 150 + i * 30, 15000, (150 + i * 30) * 15000);

                    TaoPhieuNhapKho(
                        maChiNhanh,
                        1 + (i % 3), // Rotate suppliers
                        maNguoiDung,
                        $"HD-MOCK-{i + 1:000}",
                        $"Mock data - Phiếu nhập #{i + 1}",
                        chiTietNhap
                    );
                }

                // Insert 4 phiếu xuất
                for (int i = 0; i < 4; i++)
                {
                    int daysAgo = 35 - (i * 7);
                    DataTable chiTietXuat = new DataTable();
                    chiTietXuat.Columns.Add("MaSanPham", typeof(int));
                    chiTietXuat.Columns.Add("SoLuong", typeof(int));
                    chiTietXuat.Columns.Add("DonGia", typeof(decimal));
                    chiTietXuat.Columns.Add("ThanhTien", typeof(decimal));

                    chiTietXuat.Rows.Add(maBap, 100 + i * 20, 50000, (100 + i * 20) * 50000);
                    chiTietXuat.Rows.Add(maNuoc, 80 + i * 15, 20000, (80 + i * 15) * 20000);

                    TaoPhieuXuatKho(
                        maChiNhanh,
                        null, // Xuất bán
                        maNguoiDung,
                        "XuatBan",
                        $"Mock data - Xuất bán #{i + 1}",
                        "",
                        chiTietXuat
                    );
                }

                System.Diagnostics.Debug.WriteLine($"Inserted mock inventory data for branch {maChiNhanh}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InsertMockInventoryData error: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}
