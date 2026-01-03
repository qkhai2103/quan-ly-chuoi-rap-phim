-- ================================================
-- SCRIPT: INSERT MOCK DATA CHO MODULE KHO
-- Mục đích: Populate dữ liệu mẫu cho các tab Nhập/Xuất kho
-- ================================================

-- Kiểm tra và insert mock data chỉ khi chưa có
IF NOT EXISTS (SELECT TOP 1 1 FROM NhapKho)
BEGIN
    PRINT 'Inserting mock data for Inventory module...'

    -- ================================================
    -- 1. NHÀ CUNG CẤP (đã có từ migration, bổ sung thêm nếu cần)
    -- ================================================
    IF NOT EXISTS (SELECT 1 FROM NhaCungCap WHERE MaNhaCungCap = 4)
    BEGIN
        INSERT INTO NhaCungCap (TenNhaCungCap, DiaChi, SoDienThoai, NguoiLienHe, GhiChu) VALUES
        (N'Công ty TNHH Snack Việt', N'234 Võ Văn Ngân, Q.TĐ, TP.HCM', '028-3344556', N'Phạm Văn D', N'NCC snack, kẹo'),
        (N'Công ty Nước giải khát Tân Hiệp Phát', N'567 Quốc lộ 13, Bình Dương', '028-7788990', N'Nguyễn Thị E', N'NCC nước suối, trà');
    END

    -- ================================================
    -- 2. PHIẾU NHẬP KHO (7 phiếu - 2 tháng gần đây)
    -- ================================================
    DECLARE @MaNguoiDung INT = (SELECT TOP 1 MaNguoiDung FROM NguoiDung WHERE VaiTro = N'Manager')
    DECLARE @MaChiNhanh INT = (SELECT TOP 1 MaChiNhanh FROM ChiNhanh)
    DECLARE @MaBap INT = (SELECT TOP 1 MaSanPham FROM SanPham WHERE LoaiSanPham = 'Bap')
    DECLARE @MaNuoc INT = (SELECT TOP 1 MaSanPham FROM SanPham WHERE LoaiSanPham = 'Nuoc')

    IF @MaNguoiDung IS NULL OR @MaChiNhanh IS NULL
    BEGIN
        PRINT 'ERROR: Cannot find MaNguoiDung or MaChiNhanh - skipping mock data'
        RETURN
    END

    PRINT 'Using MaChiNhanh: ' + CAST(@MaChiNhanh AS VARCHAR)
    PRINT 'Using MaNguoiDung: ' + CAST(@MaNguoiDung AS VARCHAR)

    -- Phiếu nhập 1
    INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, NgayNhap, TongTien, SoHoaDon, GhiChu, TrangThai)
    VALUES (@MaChiNhanh, 1, @MaNguoiDung, DATEADD(DAY, -45, GETDATE()), 15500000, N'HD-NCC01-001', N'Nhập bắp ngô tháng 11', N'DaNhap')
    
    DECLARE @MaNhap1 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
    VALUES 
        (@MaNhap1, @MaBap, 500, 25000, 12500000),
        (@MaNhap1, @MaNuoc, 200, 15000, 3000000)

    -- Phiếu nhập 2
    INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, NgayNhap, TongTien, SoHoaDon, GhiChu, TrangThai)
    VALUES (@MaChiNhanh, 2, @MaNguoiDung, DATEADD(DAY, -38, GETDATE()), 8400000, N'HD-PEPSI-112', N'Nhập Pepsi các loại', N'DaNhap')
    
    DECLARE @MaNhap2 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
    VALUES (@MaNhap2, @MaNuoc, 600, 14000, 8400000)

    -- Phiếu nhập 3
    INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, NgayNhap, TongTien, SoHoaDon, GhiChu, TrangThai)
    VALUES (@MaChiNhanh, 3, @MaNguoiDung, DATEADD(DAY, -30, GETDATE()), 10200000, N'HD-COCA-223', N'Nhập Coca-Cola đầu tháng 12', N'DaNhap')
    
    DECLARE @MaNhap3 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
    VALUES (@MaNhap3, @MaNuoc, 680, 15000, 10200000)

    -- Phiếu nhập 4
    INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, NgayNhap, TongTien, SoHoaDon, GhiChu, TrangThai)
    VALUES (@MaChiNhanh, 1, @MaNguoiDung, DATEADD(DAY, -20, GETDATE()), 18750000, N'HD-NCC01-002', N'Nhập bắp combo tháng 12', N'DaNhap')
    
    DECLARE @MaNhap4 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
    VALUES 
        (@MaNhap4, @MaBap, 600, 26000, 15600000),
        (@MaNhap4, @MaNuoc, 210, 15000, 3150000)

    -- Phiếu nhập 5
    INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, NgayNhap, TongTien, SoHoaDon, GhiChu, TrangThai)
    VALUES (@MaChiNhanh, 4, @MaNguoiDung, DATEADD(DAY, -15, GETDATE()), 6500000, N'HD-SNACK-045', N'Nhập snack, kẹo', N'DaNhap')
    
    DECLARE @MaNhap5 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
    VALUES (@MaNhap5, @MaBap, 200, 32500, 6500000)

    -- Phiếu nhập 6
    INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, NgayNhap, TongTien, SoHoaDon, GhiChu, TrangThai)
    VALUES (@MaChiNhanh, 5, @MaNguoiDung, DATEADD(DAY, -8, GETDATE()), 9100000, N'HD-THP-334', N'Nhập nước suối, trà', N'DaNhap')
    
    DECLARE @MaNhap6 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
    VALUES (@MaNhap6, @MaNuoc, 650, 14000, 9100000)

    -- Phiếu nhập 7 (gần nhất)
    INSERT INTO NhapKho (MaChiNhanh, MaNhaCungCap, MaNguoiNhap, NgayNhap, TongTien, SoHoaDon, GhiChu, TrangThai)
    VALUES (@MaChiNhanh, 1, @MaNguoiDung, DATEADD(DAY, -3, GETDATE()), 12800000, N'HD-NCC01-003', N'Nhập bổ sung cuối tuần', N'DaNhap')
    
    DECLARE @MaNhap7 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietNhapKho (MaNhapKho, MaSanPham, SoLuongNhap, DonGiaNhap, ThanhTien)
    VALUES 
        (@MaNhap7, @MaBap, 400, 25500, 10200000),
        (@MaNhap7, @MaNuoc, 200, 13000, 2600000)

    -- ================================================
    -- 3. PHIẾU XUẤT KHO (6 phiếu - mix giữa bán và chuyển)
    -- ================================================
    
    -- Lấy chi nhánh khác để chuyển kho (nếu có)
    DECLARE @MaChiNhanhKhac INT = (
        SELECT TOP 1 MaChiNhanh 
        FROM ChiNhanh 
        WHERE MaChiNhanh != @MaChiNhanh
    )

    -- Phiếu xuất 1: Xuất bán
    INSERT INTO XuatKho (MaChiNhanhXuat, MaChiNhanhNhan, MaNguoiXuat, NgayXuat, TongTien, LoaiXuat, LyDoXuat, TrangThai)
    VALUES (@MaChiNhanh, NULL, @MaNguoiDung, DATEADD(DAY, -40, GETDATE()), 0, N'XuatBan', N'Bán lẻ cho khách hàng', N'DaXuat')
    
    DECLARE @MaXuat1 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietXuatKho (MaXuatKho, MaSanPham, SoLuongXuat, DonGiaXuat, ThanhTien)
    VALUES 
        (@MaXuat1, @MaBap, 150, 50000, 7500000),
        (@MaXuat1, @MaNuoc, 80, 20000, 1600000)
    UPDATE XuatKho SET TongTien = 9100000 WHERE MaXuatKho = @MaXuat1

    -- Phiếu xuất 2: Chuyển kho (nếu có chi nhánh khác)
    IF @MaChiNhanhKhac IS NOT NULL
    BEGIN
        INSERT INTO XuatKho (MaChiNhanhXuat, MaChiNhanhNhan, MaNguoiXuat, NgayXuat, TongTien, LoaiXuat, LyDoXuat, TrangThai)
        VALUES (@MaChiNhanh, @MaChiNhanhKhac, @MaNguoiDung, DATEADD(DAY, -35, GETDATE()), 0, N'ChuyenKho', N'Điều chuyển hỗ trợ chi nhánh khác', N'DaXacNhan')
        
        DECLARE @MaXuat2 INT = SCOPE_IDENTITY()
        INSERT INTO ChiTietXuatKho (MaXuatKho, MaSanPham, SoLuongXuat, DonGiaXuat, ThanhTien)
        VALUES (@MaXuat2, @MaBap, 200, 25000, 5000000)
        UPDATE XuatKho SET TongTien = 5000000 WHERE MaXuatKho = @MaXuat2
    END

    -- Phiếu xuất 3: Xuất bán
    INSERT INTO XuatKho (MaChiNhanhXuat, MaChiNhanhNhan, MaNguoiXuat, NgayXuat, TongTien, LoaiXuat, LyDoXuat, TrangThai)
    VALUES (@MaChiNhanh, NULL, @MaNguoiDung, DATEADD(DAY, -28, GETDATE()), 0, N'XuatBan', N'Bán cho sự kiện', N'DaXuat')
    
    DECLARE @MaXuat3 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietXuatKho (MaXuatKho, MaSanPham, SoLuongXuat, DonGiaXuat, ThanhTien)
    VALUES 
        (@MaXuat3, @MaBap, 180, 52000, 9360000),
        (@MaXuat3, @MaNuoc, 120, 21000, 2520000)
    UPDATE XuatKho SET TongTien = 11880000 WHERE MaXuatKho = @MaXuat3

    -- Phiếu xuất 4: Xuất hủy
    INSERT INTO XuatKho (MaChiNhanhXuat, MaChiNhanhNhan, MaNguoiXuat, NgayXuat, TongTien, LoaiXuat, LyDoXuat, TrangThai)
    VALUES (@MaChiNhanh, NULL, @MaNguoiDung, DATEADD(DAY, -22, GETDATE()), 0, N'XuatHuy', N'Hủy hàng hết hạn', N'DaXuat')
    
    DECLARE @MaXuat4 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietXuatKho (MaXuatKho, MaSanPham, SoLuongXuat, DonGiaXuat, ThanhTien)
    VALUES (@MaXuat4, @MaNuoc, 30, 15000, 450000)
    UPDATE XuatKho SET TongTien = 450000 WHERE MaXuatKho = @MaXuat4

    -- Phiếu xuất 5: Xuất bán
    INSERT INTO XuatKho (MaChiNhanhXuat, MaChiNhanhNhan, MaNguoiXuat, NgayXuat, TongTien, LoaiXuat, LyDoXuat, TrangThai)
    VALUES (@MaChiNhanh, NULL, @MaNguoiDung, DATEADD(DAY, -12, GETDATE()), 0, N'XuatBan', N'Bán lẻ', N'DaXuat')
    
    DECLARE @MaXuat5 INT = SCOPE_IDENTITY()
    INSERT INTO ChiTietXuatKho (MaXuatKho, MaSanPham, SoLuongXuat, DonGiaXuat, ThanhTien)
    VALUES 
        (@MaXuat5, @MaBap, 100, 50000, 5000000),
        (@MaXuat5, @MaNuoc, 90, 20000, 1800000)
    UPDATE XuatKho SET TongTien = 6800000 WHERE MaXuatKho = @MaXuat5

    -- Phiếu xuất 6: Chờ xác nhận (chuyển kho)
    IF @MaChiNhanhKhac IS NOT NULL
    BEGIN
        INSERT INTO XuatKho (MaChiNhanhXuat, MaChiNhanhNhan, MaNguoiXuat, NgayXuat, TongTien, LoaiXuat, LyDoXuat, TrangThai)
        VALUES (@MaChiNhanh, @MaChiNhanhKhac, @MaNguoiDung, DATEADD(DAY, -5, GETDATE()), 0, N'ChuyenKho', N'Đang chờ xác nhận nhận hàng', N'ChoXacNhan')
        
        DECLARE @MaXuat6 INT = SCOPE_IDENTITY()
        INSERT INTO ChiTietXuatKho (MaXuatKho, MaSanPham, SoLuongXuat, DonGiaXuat, ThanhTien)
        VALUES (@MaXuat6, @MaBap, 150, 26000, 3900000)
        UPDATE XuatKho SET TongTien = 3900000 WHERE MaXuatKho = @MaXuat6
    END

    PRINT 'Mock data insertion completed successfully!'
    PRINT 'Inserted 7 NhapKho records + details'
    PRINT 'Inserted 6 XuatKho records + details'
END
ELSE
BEGIN
    PRINT 'NhapKho table already has data - skipping mock data insertion'
END
GO
