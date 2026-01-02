-- ============================================================
-- MIGRATION: Tạo các bảng quản lý kho
-- Ngày tạo: 02/01/2026
-- Mô tả: Bổ sung các bảng NhaCungCap, NhapKho, XuatKho, ChiTietNhapKho, ChiTietXuatKho
-- ============================================================

-- 1. BẢNG: NhaCungCap (Nhà cung cấp)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NhaCungCap')
BEGIN
    CREATE TABLE NhaCungCap (
        MaNhaCungCap INT PRIMARY KEY IDENTITY(1,1),
        TenNhaCungCap NVARCHAR(200) NOT NULL,
        DiaChi NVARCHAR(500),
        SoDienThoai VARCHAR(20),
        Email VARCHAR(100),
        NguoiLienHe NVARCHAR(100),
        GhiChu NVARCHAR(500),
        TrangThai BIT DEFAULT 1,
        NgayTao DATETIME DEFAULT GETDATE()
    );
    PRINT N'Đã tạo bảng NhaCungCap';
END
GO

-- 2. BẢNG: NhapKho (Phiếu nhập kho)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NhapKho')
BEGIN
    CREATE TABLE NhapKho (
        MaNhapKho INT PRIMARY KEY IDENTITY(1,1),
        MaChiNhanh INT NOT NULL,
        MaNhaCungCap INT NULL,
        MaNguoiNhap INT NOT NULL,
        NgayNhap DATETIME DEFAULT GETDATE(),
        TongTien DECIMAL(15,2) DEFAULT 0,
        SoHoaDon NVARCHAR(100),
        GhiChu NVARCHAR(500),
        TrangThai NVARCHAR(50) DEFAULT N'DaNhap' CHECK (TrangThai IN (N'DaNhap', N'DaHuy')),
        FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh),
        FOREIGN KEY (MaNhaCungCap) REFERENCES NhaCungCap(MaNhaCungCap),
        FOREIGN KEY (MaNguoiNhap) REFERENCES NguoiDung(MaNguoiDung)
    );
    PRINT N'Đã tạo bảng NhapKho';
END
GO

-- 3. BẢNG: ChiTietNhapKho (Chi tiết phiếu nhập)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChiTietNhapKho')
BEGIN
    CREATE TABLE ChiTietNhapKho (
        MaChiTietNhap INT PRIMARY KEY IDENTITY(1,1),
        MaNhapKho INT NOT NULL,
        MaSanPham INT NOT NULL,
        SoLuongNhap INT NOT NULL,
        DonGiaNhap DECIMAL(10,2) NOT NULL,
        ThanhTien DECIMAL(15,2) NOT NULL,
        GhiChu NVARCHAR(200),
        FOREIGN KEY (MaNhapKho) REFERENCES NhapKho(MaNhapKho),
        FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
    );
    PRINT N'Đã tạo bảng ChiTietNhapKho';
END
GO

-- 4. BẢNG: XuatKho (Phiếu xuất kho - chuyển kho giữa các chi nhánh hoặc xuất bán)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'XuatKho')
BEGIN
    CREATE TABLE XuatKho (
        MaXuatKho INT PRIMARY KEY IDENTITY(1,1),
        MaChiNhanhXuat INT NOT NULL,
        MaChiNhanhNhan INT NULL, -- NULL nếu xuất bán, có giá trị nếu chuyển kho
        MaNguoiXuat INT NOT NULL,
        MaNguoiXacNhan INT NULL,
        NgayXuat DATETIME DEFAULT GETDATE(),
        NgayXacNhan DATETIME NULL,
        TongTien DECIMAL(15,2) DEFAULT 0,
        LoaiXuat NVARCHAR(50) DEFAULT N'XuatBan' CHECK (LoaiXuat IN (N'XuatBan', N'ChuyenKho', N'Huy', N'KhuyenMai')),
        LyDoXuat NVARCHAR(500),
        TrangThai NVARCHAR(50) DEFAULT N'ChoXacNhan' CHECK (TrangThai IN (N'ChoXacNhan', N'DaXacNhan', N'DaNhan', N'DaHuy')),
        GhiChu NVARCHAR(500),
        FOREIGN KEY (MaChiNhanhXuat) REFERENCES ChiNhanh(MaChiNhanh),
        FOREIGN KEY (MaChiNhanhNhan) REFERENCES ChiNhanh(MaChiNhanh),
        FOREIGN KEY (MaNguoiXuat) REFERENCES NguoiDung(MaNguoiDung),
        FOREIGN KEY (MaNguoiXacNhan) REFERENCES NguoiDung(MaNguoiDung)
    );
    PRINT N'Đã tạo bảng XuatKho';
END
GO

-- 5. BẢNG: ChiTietXuatKho (Chi tiết phiếu xuất)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChiTietXuatKho')
BEGIN
    CREATE TABLE ChiTietXuatKho (
        MaChiTietXuat INT PRIMARY KEY IDENTITY(1,1),
        MaXuatKho INT NOT NULL,
        MaSanPham INT NOT NULL,
        SoLuongXuat INT NOT NULL,
        DonGiaXuat DECIMAL(10,2) NOT NULL,
        ThanhTien DECIMAL(15,2) NOT NULL,
        GhiChu NVARCHAR(200),
        FOREIGN KEY (MaXuatKho) REFERENCES XuatKho(MaXuatKho),
        FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
    );
    PRINT N'Đã tạo bảng ChiTietXuatKho';
END
GO

-- 6. BẢNG: KiemKe (Phiếu kiểm kê/điều chỉnh tồn kho)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'KiemKe')
BEGIN
    CREATE TABLE KiemKe (
        MaKiemKe INT PRIMARY KEY IDENTITY(1,1),
        MaChiNhanh INT NOT NULL,
        MaNguoiKiemKe INT NOT NULL,
        NgayKiemKe DATETIME DEFAULT GETDATE(),
        GhiChu NVARCHAR(500),
        TrangThai NVARCHAR(50) DEFAULT N'DaKiemKe',
        FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh),
        FOREIGN KEY (MaNguoiKiemKe) REFERENCES NguoiDung(MaNguoiDung)
    );
    PRINT N'Đã tạo bảng KiemKe';
END
GO

-- 7. BẢNG: ChiTietKiemKe (Chi tiết kiểm kê)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChiTietKiemKe')
BEGIN
    CREATE TABLE ChiTietKiemKe (
        MaChiTietKiemKe INT PRIMARY KEY IDENTITY(1,1),
        MaKiemKe INT NOT NULL,
        MaSanPham INT NOT NULL,
        SoLuongSoSach INT NOT NULL,     -- Số lượng theo sổ sách
        SoLuongThucTe INT NOT NULL,     -- Số lượng thực tế kiểm đếm
        ChenhLech INT NOT NULL,          -- Chênh lệch = Thực tế - Sổ sách
        LyDoChenhLech NVARCHAR(200),
        FOREIGN KEY (MaKiemKe) REFERENCES KiemKe(MaKiemKe),
        FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
    );
    PRINT N'Đã tạo bảng ChiTietKiemKe';
END
GO

-- ============================================================
-- DỮ LIỆU MẪU
-- ============================================================

-- Thêm nhà cung cấp mẫu
IF NOT EXISTS (SELECT TOP 1 1 FROM NhaCungCap)
BEGIN
    INSERT INTO NhaCungCap (TenNhaCungCap, DiaChi, SoDienThoai, Email, NguoiLienHe, GhiChu) VALUES
    (N'Công ty TNHH Bắp Ngô Việt Nam', N'123 Nguyễn Văn Linh, Q.7, TP.HCM', '028-1234567', 'contact@bapngovn.com', N'Nguyễn Văn A', N'NCC chính - Bắp rang'),
    (N'Pepsi Vietnam', N'456 Lê Văn Việt, Q.9, TP.HCM', '028-9876543', 'sales@pepsi.vn', N'Trần Thị B', N'NCC nước ngọt Pepsi'),
    (N'Coca-Cola Vietnam', N'789 Điện Biên Phủ, Q.3, TP.HCM', '028-5555555', 'b2b@cocacola.vn', N'Lê Văn C', N'NCC nước ngọt Coca'),
    (N'Công ty CP Thực phẩm Orion', N'321 Hoàng Văn Thụ, Q.Phú Nhuận, TP.HCM', '028-6666666', 'sales@orion.vn', N'Phạm Thị D', N'NCC bánh kẹo');
    PRINT N'Đã thêm dữ liệu mẫu NhaCungCap';
END
GO

-- Thêm phiếu nhập mẫu
IF NOT EXISTS (SELECT TOP 1 1 FROM NhapKho)
BEGIN
    -- Lấy mã chi nhánh và mã người dùng đầu tiên
    DECLARE @MaChiNhanh INT = (SELECT TOP 1 MaChiNhanh FROM ChiNhanh);
    DECLARE @MaNguoiDung INT = (SELECT TOP 1 MaNguoiDung FROM NguoiDung WHERE VaiTro IN (N'Admin', N'Manager'));
    
    IF @MaChiNhanh IS NOT NULL AND @MaNguoiDung IS NOT NULL
    BEGIN
        INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, NgayNhap, TongTien, SoHoaDon, GhiChu)
        VALUES 
        (@MaChiNhanh, 1, @MaNguoiDung, DATEADD(DAY, -7, GETDATE()), 5000000, N'HD-2025-001', N'Nhập hàng đợt 1 tháng 12'),
        (@MaChiNhanh, 2, @MaNguoiDung, DATEADD(DAY, -3, GETDATE()), 3500000, N'HD-2025-002', N'Nhập nước ngọt Pepsi'),
        (@MaChiNhanh, 1, @MaNguoiDung, GETDATE(), 2000000, N'HD-2025-003', N'Bổ sung bắp rang cuối tuần');
        
        -- Thêm chi tiết nhập kho
        DECLARE @MaSP1 INT = (SELECT TOP 1 MaSanPham FROM SanPham WHERE MaChiNhanh = @MaChiNhanh AND LoaiSanPham = N'Bap');
        DECLARE @MaSP2 INT = (SELECT TOP 1 MaSanPham FROM SanPham WHERE MaChiNhanh = @MaChiNhanh AND LoaiSanPham = N'Nuoc');
        
        IF @MaSP1 IS NOT NULL
        BEGIN
            INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
            VALUES 
            (1, @MaSP1, 100, 15000, 1500000),
            (3, @MaSP1, 50, 15000, 750000);
        END
        
        IF @MaSP2 IS NOT NULL
        BEGIN
            INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
            VALUES 
            (2, @MaSP2, 200, 8000, 1600000);
        END
        
        PRINT N'Đã thêm dữ liệu mẫu NhapKho và ChiTietNhapKho';
    END
END
GO

PRINT N'=== MIGRATION HOÀN TẤT ===';
