-- Migration: Thêm bảng DeXuatLichChieu và cập nhật cấu trúc hỗ trợ quản lý chi nhánh
-- Date: 2026-01-02

USE CinemaDB;
GO

-- ============================================================
-- BẢNG: DeXuatLichChieu (Đề xuất lịch chiếu từ Quản lý chi nhánh)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DeXuatLichChieu' AND xtype='U')
BEGIN
    CREATE TABLE DeXuatLichChieu (
        MaDeXuat INT PRIMARY KEY IDENTITY(1,1),
        MaNguoiDeXuat INT NOT NULL,            -- Quản lý chi nhánh đề xuất
        MaPhim INT NOT NULL,                    -- Phim cần điều chỉnh
        MaChiNhanh INT NOT NULL,                -- Chi nhánh đề xuất
        LoaiDeXuat NVARCHAR(50) NOT NULL,       -- 'GiamSuatChieu', 'TangSuatChieu', 'XoaPhim', 'ThemPhim'
        LyDo NVARCHAR(MAX),                     -- Lý do đề xuất
        ThongTinBoSung NVARCHAR(MAX),           -- Thông tin bổ sung (JSON format)
        TrangThai NVARCHAR(50) DEFAULT N'ChoDuyet' CHECK (TrangThai IN (N'ChoDuyet', N'DaDuyet', N'TuChoi', N'DaHuy')),
        NguoiDuyet INT NULL,                    -- Admin duyệt
        NgayDuyet DATETIME NULL,
        GhiChuDuyet NVARCHAR(500),              -- Ghi chú từ Admin
        NgayTao DATETIME DEFAULT GETDATE(),
        NgayCapNhat DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (MaNguoiDeXuat) REFERENCES NguoiDung(MaNguoiDung),
        FOREIGN KEY (MaPhim) REFERENCES Phim(MaPhim),
        FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh),
        FOREIGN KEY (NguoiDuyet) REFERENCES NguoiDung(MaNguoiDung)
    );
    PRINT N'Created table DeXuatLichChieu';
END
GO

-- ============================================================
-- INDEX cho tối ưu truy vấn
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DeXuatLichChieu_TrangThai')
BEGIN
    CREATE INDEX IX_DeXuatLichChieu_TrangThai ON DeXuatLichChieu(TrangThai);
    PRINT N'Created index IX_DeXuatLichChieu_TrangThai';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DeXuatLichChieu_MaChiNhanh')
BEGIN
    CREATE INDEX IX_DeXuatLichChieu_MaChiNhanh ON DeXuatLichChieu(MaChiNhanh);
    PRINT N'Created index IX_DeXuatLichChieu_MaChiNhanh';
END
GO

-- ============================================================
-- Stored Procedure: Lấy đề xuất theo trạng thái
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_LayDeXuatLichChieu')
    DROP PROCEDURE sp_LayDeXuatLichChieu;
GO

CREATE PROCEDURE sp_LayDeXuatLichChieu
    @MaChiNhanh INT = NULL,
    @TrangThai NVARCHAR(50) = NULL
AS
BEGIN
    SELECT 
        dx.MaDeXuat,
        dx.MaPhim,
        p.TenPhim,
        p.TheLoai,
        dx.MaChiNhanh,
        cn.TenChiNhanh,
        dx.LoaiDeXuat,
        dx.LyDo,
        dx.ThongTinBoSung,
        dx.TrangThai,
        nd.HoTen AS NguoiDeXuat,
        dx.NgayTao,
        ISNULL(admin.HoTen, '') AS NguoiDuyet,
        dx.NgayDuyet,
        dx.GhiChuDuyet
    FROM DeXuatLichChieu dx
    INNER JOIN Phim p ON dx.MaPhim = p.MaPhim
    INNER JOIN ChiNhanh cn ON dx.MaChiNhanh = cn.MaChiNhanh
    INNER JOIN NguoiDung nd ON dx.MaNguoiDeXuat = nd.MaNguoiDung
    LEFT JOIN NguoiDung admin ON dx.NguoiDuyet = admin.MaNguoiDung
    WHERE (@MaChiNhanh IS NULL OR dx.MaChiNhanh = @MaChiNhanh)
      AND (@TrangThai IS NULL OR dx.TrangThai = @TrangThai)
    ORDER BY dx.NgayTao DESC;
END
GO

-- ============================================================
-- Stored Procedure: Thêm đề xuất mới
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ThemDeXuatLichChieu')
    DROP PROCEDURE sp_ThemDeXuatLichChieu;
GO

CREATE PROCEDURE sp_ThemDeXuatLichChieu
    @MaNguoiDeXuat INT,
    @MaPhim INT,
    @MaChiNhanh INT,
    @LoaiDeXuat NVARCHAR(50),
    @LyDo NVARCHAR(MAX),
    @ThongTinBoSung NVARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO DeXuatLichChieu (MaNguoiDeXuat, MaPhim, MaChiNhanh, LoaiDeXuat, LyDo, ThongTinBoSung)
    VALUES (@MaNguoiDeXuat, @MaPhim, @MaChiNhanh, @LoaiDeXuat, @LyDo, @ThongTinBoSung);
    
    SELECT SCOPE_IDENTITY() AS MaDeXuat;
END
GO

-- ============================================================
-- Stored Procedure: Duyệt/Từ chối đề xuất
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_DuyetDeXuat')
    DROP PROCEDURE sp_DuyetDeXuat;
GO

CREATE PROCEDURE sp_DuyetDeXuat
    @MaDeXuat INT,
    @TrangThai NVARCHAR(50),
    @NguoiDuyet INT,
    @GhiChuDuyet NVARCHAR(500) = NULL
AS
BEGIN
    UPDATE DeXuatLichChieu
    SET TrangThai = @TrangThai,
        NguoiDuyet = @NguoiDuyet,
        NgayDuyet = GETDATE(),
        GhiChuDuyet = @GhiChuDuyet,
        NgayCapNhat = GETDATE()
    WHERE MaDeXuat = @MaDeXuat;
    
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

PRINT N'Migration completed: DeXuatLichChieu table and procedures created';
GO
