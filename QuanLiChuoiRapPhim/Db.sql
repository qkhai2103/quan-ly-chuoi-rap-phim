CREATE DATABASE CinemaDB;
GO

USE CinemaDB;
GO

-- ============================================================
-- 1. BẢNG: ChiNhanh (Chi nhánh)
-- ============================================================
CREATE TABLE ChiNhanh (
    MaChiNhanh INT PRIMARY KEY IDENTITY(1,1),
    TenChiNhanh NVARCHAR(200) NOT NULL,
    DiaChi NVARCHAR(500) NOT NULL,
    SoDienThoai VARCHAR(20),
    MaQuanLy INT NULL,
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE()
);

-- ============================================================
-- 2. BẢNG: NguoiDung (Người dùng)
-- ============================================================
CREATE TABLE NguoiDung (
    MaNguoiDung INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(200) NOT NULL,
    Email VARCHAR(100),
    SoDienThoai VARCHAR(20),
    VaiTro NVARCHAR(50) NOT NULL CHECK (VaiTro IN (N'Admin', N'QuanLy', N'NhanVien')),
    MaChiNhanh INT,
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh)
);

-- Cập nhật khóa ngoại cho ChiNhanh.MaQuanLy
ALTER TABLE ChiNhanh
ADD FOREIGN KEY (MaQuanLy) REFERENCES NguoiDung(MaNguoiDung);

-- ============================================================
-- 3. BẢNG: PhongChieu (Phòng chiếu)
-- ============================================================
CREATE TABLE PhongChieu (
    MaPhong INT PRIMARY KEY IDENTITY(1,1),
    TenPhong NVARCHAR(100) NOT NULL,
    MaChiNhanh INT NOT NULL,
    TongSoGhe INT NOT NULL,
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh)
);

-- ============================================================
-- 4. BẢNG: GheNgoi (Ghế ngồi)
-- ============================================================
CREATE TABLE GheNgoi (
    MaGhe INT PRIMARY KEY IDENTITY(1,1),
    MaPhong INT NOT NULL,
    SoGhe VARCHAR(10) NOT NULL,
    SoHang VARCHAR(5) NOT NULL,
    LoaiGhe NVARCHAR(20) NOT NULL CHECK (LoaiGhe IN (N'VIP', N'Thuong')),
    TrangThai BIT DEFAULT 1,
    FOREIGN KEY (MaPhong) REFERENCES PhongChieu(MaPhong),
    UNIQUE(MaPhong, SoGhe)
);

-- ============================================================
-- 5. BẢNG: Phim (Phim)
-- ============================================================
CREATE TABLE Phim (
    MaPhim INT PRIMARY KEY IDENTITY(1,1),
    TenPhim NVARCHAR(300) NOT NULL,
    TheLoai NVARCHAR(100),
    ThoiLuong INT NOT NULL, -- Thời lượng (phút)
    DaoDien NVARCHAR(200),
    DienVien NVARCHAR(500),
    MoTa NVARCHAR(MAX),
    HinhAnh VARCHAR(500),
    Trailer VARCHAR(500),
    DoTuoi VARCHAR(10) CHECK (DoTuoi IN ('P', 'C13', 'C16', 'C18')),
    NgayKhoiChieu DATE,
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE()
);

-- ============================================================
-- 6. BẢNG: SuatChieu (Suất chiếu)
-- ============================================================
CREATE TABLE SuatChieu (
    MaSuatChieu INT PRIMARY KEY IDENTITY(1,1),
    MaPhim INT NOT NULL,
    MaPhong INT NOT NULL,
    NgayChieu DATE NOT NULL,
    GioChieu TIME NOT NULL,
    GiaVe DECIMAL(10,2) NOT NULL,
    TrangThai NVARCHAR(50) DEFAULT N'SapChieu' CHECK (TrangThai IN (N'SapChieu', N'DangChieu', N'KetThuc')),
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaPhim) REFERENCES Phim(MaPhim),
    FOREIGN KEY (MaPhong) REFERENCES PhongChieu(MaPhong)
);

-- ============================================================
-- 7. BẢNG: KhachHang (Khách hàng) - TÙY CHỌN
-- ============================================================
CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(200) NOT NULL,
    SoDienThoai VARCHAR(20) UNIQUE NOT NULL,
    Email VARCHAR(100),
    NgaySinh DATE,
    DiemTichLuy INT DEFAULT 0,
    HangThanhVien NVARCHAR(20) DEFAULT N'Dong' CHECK (HangThanhVien IN (N'Dong', N'Bac', N'Vang')),
    NgayDangKy DATETIME DEFAULT GETDATE()
);

-- ============================================================
-- 8. BẢNG: Ve (Vé)
-- ============================================================
CREATE TABLE Ve (
    MaVe INT PRIMARY KEY IDENTITY(1,1),
    MaSuatChieu INT NOT NULL,
    MaGhe INT NOT NULL,
    MaKhachHang INT NULL,
    DoiTuongKhachHang NVARCHAR(50) DEFAULT N'NguoiLon' CHECK (DoiTuongKhachHang IN (N'NguoiLon', N'HocSinh', N'SinhVien', N'NguoiGia', N'TreEm')),
    GiaVe DECIMAL(10,2) NOT NULL,
    TrangThai NVARCHAR(50) DEFAULT N'DaBan' CHECK (TrangThai IN (N'DaBan', N'DaHuy')),
    NgayDat DATETIME DEFAULT GETDATE(),
    MaVeCode VARCHAR(50) UNIQUE NOT NULL,
    FOREIGN KEY (MaSuatChieu) REFERENCES SuatChieu(MaSuatChieu),
    FOREIGN KEY (MaGhe) REFERENCES GheNgoi(MaGhe),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    UNIQUE(MaSuatChieu, MaGhe) -- Mỗi ghế chỉ bán 1 lần trong 1 suất
);

-- ============================================================
-- 9. BẢNG: SanPham (Sản phẩm bắp nước)
-- ============================================================
CREATE TABLE SanPham (
    MaSanPham INT PRIMARY KEY IDENTITY(1,1),
    TenSanPham NVARCHAR(200) NOT NULL,
    LoaiSanPham NVARCHAR(50) CHECK (LoaiSanPham IN (N'Bap', N'Nuoc', N'Combo')),
    GiaBan DECIMAL(10,2) NOT NULL,
    MaChiNhanh INT NOT NULL,
    SoLuongTon INT DEFAULT 0,
    DonVi NVARCHAR(50),
    HinhAnh VARCHAR(500),
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh)
);

-- ============================================================
-- 10. BẢNG: HoaDon (Hóa đơn)
-- ============================================================
CREATE TABLE HoaDon (
    MaHoaDon INT PRIMARY KEY IDENTITY(1,1),
    MaNguoiDung INT NOT NULL,
    MaKhachHang INT NULL,
    TongTien DECIMAL(10,2) NOT NULL,
    GiamGia DECIMAL(10,2) DEFAULT 0,
    ThanhTien DECIMAL(10,2) NOT NULL,
    PhuongThucThanhToan NVARCHAR(50) CHECK (PhuongThucThanhToan IN (N'TienMat', N'ChuyenKhoan')),
    TrangThaiThanhToan NVARCHAR(50) DEFAULT N'DaThanhToan' CHECK (TrangThaiThanhToan IN (N'DaThanhToan', N'ChuaThanhToan')),
    NgayLap DATETIME DEFAULT GETDATE(),
    GhiChu NVARCHAR(500),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang)
);

-- ============================================================
-- 11. BẢNG: ChiTietHoaDon (Chi tiết hóa đơn)
-- ============================================================
CREATE TABLE ChiTietHoaDon (
    MaChiTiet INT PRIMARY KEY IDENTITY(1,1),
    MaHoaDon INT NOT NULL,
    MaVe INT NULL,
    MaSanPham INT NULL,
    SoLuong INT DEFAULT 1,
    DonGia DECIMAL(10,2) NOT NULL,
    ThanhTien DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
    FOREIGN KEY (MaVe) REFERENCES Ve(MaVe),
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham),
    CHECK (MaVe IS NOT NULL OR MaSanPham IS NOT NULL) -- Ít nhất 1 trong 2 phải có
);

-- ============================================================
-- 12. BẢNG: KhuyenMai (Khuyến mãi)
-- ============================================================
CREATE TABLE KhuyenMai (
    MaKhuyenMai INT PRIMARY KEY IDENTITY(1,1),
    TenKhuyenMai NVARCHAR(200) NOT NULL,
    MaCode VARCHAR(50) UNIQUE NOT NULL,
    LoaiGiamGia NVARCHAR(20) CHECK (LoaiGiamGia IN (N'PhanTram', N'TienMat')),
    GiaTriGiamGia DECIMAL(10,2) NOT NULL,
    GiamToiDa DECIMAL(10,2) NULL, -- Giảm tối đa (với % giảm)
    DieuKienToiThieu DECIMAL(10,2) DEFAULT 0, -- Đơn hàng tối thiểu
    SoLuongMa INT DEFAULT 0, -- Số lượng mã có thể dùng
    SoLuongDaSuDung INT DEFAULT 0,
    NgayBatDau DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL,
    MoTa NVARCHAR(500),
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE()
);

-- ============================================================
-- 13. BẢNG: CauHinhGiaVe (Cấu hình giá vé)
-- ============================================================
CREATE TABLE CauHinhGiaVe (
    MaCauHinh INT PRIMARY KEY IDENTITY(1,1),
    LoaiGhe NVARCHAR(20) NOT NULL CHECK (LoaiGhe IN (N'VIP', N'Thuong')),
    LoaiNgay NVARCHAR(20) NOT NULL CHECK (LoaiNgay IN (N'ThuThuong', N'CuoiTuan', N'LeHoi')),
    DoiTuongKhachHang NVARCHAR(50) NOT NULL CHECK (DoiTuongKhachHang IN (N'NguoiLon', N'HocSinh', N'SinhVien', N'NguoiGia', N'TreEm')),
    GiaGoc DECIMAL(10,2) NOT NULL,
    PhuThu DECIMAL(10,2) DEFAULT 0, -- Có thể âm (giảm giá) hoặc dương (phụ thu)
    GhiChu NVARCHAR(200),
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE()
);

-- ============================================================
-- 14. BẢNG: CaLamViec (Ca làm việc)
-- ============================================================
CREATE TABLE CaLamViec (
    MaCa INT PRIMARY KEY IDENTITY(1,1),
    TenCa NVARCHAR(50) NOT NULL,
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    MoTa NVARCHAR(200),
    TrangThai BIT DEFAULT 1
);

-- ============================================================
-- 15. BẢNG: PhanCongCa (Phân công ca làm việc)
-- ============================================================
CREATE TABLE PhanCongCa (
    MaPhanCong INT PRIMARY KEY IDENTITY(1,1),
    MaNguoiDung INT NOT NULL,
    MaCa INT NOT NULL,
    NgayLamViec DATE NOT NULL,
    TienDauCa DECIMAL(10,2) DEFAULT 0,
    TienCuoiCa DECIMAL(10,2) DEFAULT 0,
    TienThucTe DECIMAL(10,2) DEFAULT 0,
    ChenhLech DECIMAL(10,2) DEFAULT 0, -- TienThucTe - (TienCuoiCa - TienDauCa)
    TrangThai NVARCHAR(20) DEFAULT N'ChuaBatDau' CHECK (TrangThai IN (N'ChuaBatDau', N'DangLam', N'DaKetThuc')),
    GhiChu NVARCHAR(500),
    ThoiGianBatDau DATETIME,
    ThoiGianKetThuc DATETIME,
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (MaCa) REFERENCES CaLamViec(MaCa)
);

-- ============================================================
-- 16. BẢNG: LichSuHoatDong (Lịch sử hoạt động)
-- ============================================================
CREATE TABLE LichSuHoatDong (
    MaLichSu INT PRIMARY KEY IDENTITY(1,1),
    MaNguoiDung INT NOT NULL,
    HanhDong NVARCHAR(200) NOT NULL, -- 'Hủy vé', 'Sửa giá sản phẩm', 'Xóa phim'
    BangLienQuan NVARCHAR(50), -- 'Ve', 'SanPham', 'Phim', 'NguoiDung'
    MaBanGhi INT, -- ID của bản ghi bị thay đổi
    DuLieuCu NVARCHAR(MAX), -- Dữ liệu trước khi thay đổi (JSON hoặc text)
    DuLieuMoi NVARCHAR(MAX), -- Dữ liệu sau khi thay đổi
    LyDo NVARCHAR(500), -- Lý do thực hiện hành động
    ThoiGian DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

-- ============================================================
-- 17. BẢNG: BaoCaoSuCo (Báo cáo sự cố)
-- ============================================================
CREATE TABLE BaoCaoSuCo (
    MaBaoCao INT PRIMARY KEY IDENTITY(1,1),
    MaNguoiDung INT NOT NULL, -- Người báo cáo
    MaPhong INT NULL,
    MaGhe INT NULL,
    LoaiSuCo NVARCHAR(100) NOT NULL, -- 'Ghế hỏng', 'Máy chiếu lỗi', 'Âm thanh', 'Điều hòa'
    MoTa NVARCHAR(MAX),
    MucDoUuTien NVARCHAR(20) DEFAULT N'BinhThuong' CHECK (MucDoUuTien IN (N'Thap', N'BinhThuong', N'Cao', N'KhanCap')),
    TrangThai NVARCHAR(50) DEFAULT N'ChoXuLy' CHECK (TrangThai IN (N'ChoXuLy', N'DangXuLy', N'DaXong', N'KhongXuLy')),
    NguoiXuLy INT NULL, -- Người được giao xử lý
    NgayBaoCao DATETIME DEFAULT GETDATE(),
    NgayXuLy DATETIME NULL,
    GhiChuXuLy NVARCHAR(500),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiXuLy) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (MaPhong) REFERENCES PhongChieu(MaPhong),
    FOREIGN KEY (MaGhe) REFERENCES GheNgoi(MaGhe)
);

-- ============================================================
-- TẠO INDEX ĐỂ TỐI ƯU TRUY VẤN
-- ============================================================
CREATE INDEX IX_NguoiDung_TenDangNhap ON NguoiDung(TenDangNhap);
CREATE INDEX IX_SuatChieu_NgayChieu ON SuatChieu(NgayChieu);
CREATE INDEX IX_Ve_MaSuatChieu ON Ve(MaSuatChieu);
CREATE INDEX IX_Ve_TrangThai ON Ve(TrangThai);
CREATE INDEX IX_HoaDon_NgayLap ON HoaDon(NgayLap);
CREATE INDEX IX_ChiTietHoaDon_MaHoaDon ON ChiTietHoaDon(MaHoaDon);

GO

-- ============================================================
-- DỮ LIỆU MẪU (Demo - Dữ liệu thật tại Cần Thơ)
-- ============================================================

-- ========== 1. Thêm Chi nhánh ==========
INSERT INTO ChiNhanh (TenChiNhanh, DiaChi, SoDienThoai) VALUES
(N'CGV Vincom Xuân Khánh', N'Tầng 5, Vincom Plaza Xuân Khánh, 209 Đường 30 Tháng 4, Phường Xuân Khánh, Quận Ninh Kiều, TP. Cần Thơ', '0292-3821-888'),
(N'CGV Sense City Cần Thơ', N'Tầng 4, Sense City, 1A Đường 30 Tháng 4, Phường An Phú, Quận Ninh Kiều, TP. Cần Thơ', '0292-3888-999'),
(N'"CGV Vincom Hùng Vương"', N'Tầng 5, Vincom Plaza Hùng Vương, 17 Hùng Vương, Phường An Hòa, Quận Ninh Kiều, TP. Cần Thơ', '1900-6595');

-- ========== 2. Thêm Người dùng (Admin, Quản lý và Nhân viên) - Cập nhật 2025 ==========
INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, VaiTro, MaChiNhanh) VALUES
-- Admin
('admin', '123456', N'Nguyễn Thị Tuyết', 'nguyenthituyet@cgvcinema.vn', '0913456789', N'Admin', NULL),

-- Quản lý Chi nhánh
('ql_cn1', '123456', N'Phạm Thị Trúc Mỹ', 'phamthitrucmy@cgvcinema.vn', '0909123456', N'QuanLy', 1),
('ql_cn2', '123456', N'Trần Quốc Khái', 'tranquockhai@cgvcinema.vn', '0908765432', N'QuanLy', 2),
('ql_cn3', '123456', N'Phạm Thảo My', 'phamthaomy@cgvcinema.vn', '0919876543', N'QuanLy', 3),

-- Nhân viên Chi nhánh 1 (Vincom Xuân Khánh)
('nv_ve01', '123456', N'Nguyễn Phạm Thùy Dương', 'nguyenphamthuyduong@cgvcinema.vn', '0987654321', N'NhanVien', 1),
('nv_ve02', '123456', N'Trần Bội Anh', 'tranboianh@cgvcinema.vn', '0978123456', N'NhanVien', 1),

-- Nhân viên Chi nhánh 2 (Sense City)
('nv_ve03', '123456', N'Đặng Thành Dỉ', 'dangthanhdi@cgvcinema.vn', '0912345678', N'NhanVien', 2),
('nv_ve04', '123456', N'Kiến Nguyễn Trường Giang', 'kiennguyentruonggiang@cgvcinema.vn', '0934567890', N'NhanVien', 2),

-- Nhân viên Chi nhánh 3 (Lotte Cinema)
('nv_ve05', '123456', N'Trần Thị Thủy Tiên', 'tranthithuytien@cgvcinema.vn', '0923456789', N'NhanVien', 3),
('nv_ve06', '123456', N'Trần Chí Vỹ', 'tranchivy@cgvcinema.vn', '0945678901', N'NhanVien', 3);

-- Cập nhật Manager cho Chi nhánh
UPDATE ChiNhanh SET MaQuanLy = 2 WHERE MaChiNhanh = 1; -- Nguyễn Thị Hương quản lý CN1
UPDATE ChiNhanh SET MaQuanLy = 3 WHERE MaChiNhanh = 2; -- Lê Văn Quang quản lý CN2
UPDATE ChiNhanh SET MaQuanLy = 4 WHERE MaChiNhanh = 3; -- Phạm Thị Loan quản lý CN3

-- ========== 3. Thêm Phòng chiếu ==========
INSERT INTO PhongChieu (TenPhong, MaChiNhanh, TongSoGhe) VALUES
-- Chi nhánh 1: Vincom Xuân Khánh
(N'Phòng 1', 1, 100),
(N'Phòng 2', 1, 80),
(N'Phòng 3', 1, 120),

-- Chi nhánh 2: Sense City
(N'Phòng 1', 2, 150),
(N'Phòng 2', 2, 100),

-- Chi nhánh 3: Lotte Cinema
(N'Phòng 1', 3, 180),
(N'Phòng 2', 3, 120),
(N'Phòng 3', 3, 100);

-- ========== 4. Thêm Ghế (Phòng 1 - Chi nhánh 1: 10 hàng x 10 ghế = 100 ghế) ==========
DECLARE @MaPhong INT = 1;
DECLARE @Hang CHAR(1);
DECLARE @SoGhe INT;
DECLARE @i INT = 65; -- Mã ASCII của 'A'

WHILE @i < 75 -- A đến J (10 hàng)
BEGIN
    SET @Hang = CHAR(@i);
    SET @SoGhe = 1;
    WHILE @SoGhe <= 10
    BEGIN
        INSERT INTO GheNgoi (MaPhong, SoGhe, SoHang, LoaiGhe)
        VALUES (@MaPhong, @Hang + CAST(@SoGhe AS VARCHAR(2)), @Hang, 
                CASE WHEN @i >= 72 THEN N'VIP' ELSE N'Thuong' END); -- Hàng H,I,J là VIP
        SET @SoGhe = @SoGhe + 1;
    END
    SET @i = @i + 1;
END

-- Thêm ghế cho Phòng 2 (8 hàng x 10 ghế = 80 ghế)
SET @MaPhong = 2;
SET @i = 65;
WHILE @i < 73 -- A đến H (8 hàng)
BEGIN
    SET @Hang = CHAR(@i);
    SET @SoGhe = 1;
    WHILE @SoGhe <= 10
    BEGIN
        INSERT INTO GheNgoi (MaPhong, SoGhe, SoHang, LoaiGhe)
        VALUES (@MaPhong, @Hang + CAST(@SoGhe AS VARCHAR(2)), @Hang, 
                CASE WHEN @i >= 70 THEN N'VIP' ELSE N'Thuong' END); -- Hàng G,H là VIP
        SET @SoGhe = @SoGhe + 1;
    END
    SET @i = @i + 1;
END

-- ========== 5. Thêm Phim (Dữ liệu thật đang chiếu) ==========
INSERT INTO Phim (TenPhim, TheLoai, ThoiLuong, DaoDien, DienVien, MoTa, DoTuoi, NgayKhoiChieu) VALUES
(N'Mai', N'Tâm lý, Tình cảm', 131, N'Trấn Thành', N'Phương Anh Đào, Tuấn Trần, Hồng Đào', N'Câu chuyện về Mai - một cô gái bán hoa dạo với quá khứ đầy bí ẩn và hành trình tìm lại chính mình.', 'C16', '2024-02-10'),
(N'Lật Mặt 7: Một Điều Ước', N'Hài, Gia đình', 138, N'Lý Hải', N'Lý Hải, Minh Hà, Trương Minh Cường', N'Phần 7 của series Lật Mặt xoay quanh câu chuyện gia đình đầy cảm động và ý nghĩa.', 'P', '2024-04-26'),
(N'Đố Anh Còng Được Tôi', N'Hài, Hành động', 115, N'Võ Thanh Hòa', N'Trấn Thành, Ngô Kiến Huy, BB Trần', N'Cuộc rượt đuổi hài hước giữa cảnh sát và tên cướp ngớ ngẩn.', 'C13', '2024-03-15'),
(N'Kung Fu Panda 4', N'Hoạt hình, Hài, Phiêu lưu', 94, N'Mike Mitchell', N'Jack Black, Awkwafina, Viola Davis', N'Po tiếp tục hành trình trở thành Thủ lãnh Tâm linh của Thung lũng Bình Yên.', 'P', '2024-03-08'),
(N'Godzilla x Kong: The New Empire', N'Hành động, Khoa học viễn tưởng', 115, N'Adam Wingard', N'Rebecca Hall, Brian Tyree Henry, Dan Stevens', N'Godzilla và Kong liên minh chống lại một mối đe dọa mới từ lòng đất.', 'C13', '2024-03-29'),
(N'Tarot Tử Thần', N'Kinh dị, Bí ẩn', 92, N'Spenser Cohen', N'Harriet Slater, Adain Bradley', N'Nhóm bạn trẻ vô tình mở ra lời nguyền chết chóc từ bộ bài Tarot cổ.', 'C18', '2024-05-10'),
(N'Deadpool & Wolverine', N'Hành động, Hài, Siêu anh hùng', 128, N'Shawn Levy', N'Ryan Reynolds, Hugh Jackman', N'Deadpool và Wolverine hợp tác trong một nhiệm vụ điên rồ cứu vũ trụ.', 'C18', '2024-07-26'),
(N'Inside Out 2', N'Hoạt hình, Gia đình, Hài', 96, N'Kelsey Mann', N'Amy Poehler, Maya Hawke', N'Cuộc phiêu lưu mới của Riley và các cảm xúc trong giai đoạn tuổi teen.', 'P', '2024-06-14');

-- ========== 6. Thêm Suất chiếu (Lịch chiếu ngày 10/12/2024 và 11/12/2024) ==========
INSERT INTO SuatChieu (MaPhim, MaPhong, NgayChieu, GioChieu, GiaVe, TrangThai) VALUES
-- Ngày 10/12/2024
-- Phim "Mai" - Phòng 1
(1, 1, '2024-12-10', '09:30:00', 75000, N'DangChieu'),
(1, 1, '2024-12-10', '13:45:00', 85000, N'DangChieu'),
(1, 1, '2024-12-10', '18:00:00', 95000, N'DangChieu'),
(1, 1, '2024-12-10', '21:30:00', 100000, N'DangChieu'),

-- Phim "Lật Mặt 7" - Phòng 2
(2, 2, '2024-12-10', '10:00:00', 70000, N'DangChieu'),
(2, 2, '2024-12-10', '14:15:00', 80000, N'DangChieu'),
(2, 2, '2024-12-10', '19:00:00', 90000, N'DangChieu'),

-- Phim "Kung Fu Panda 4" - Phòng 3
(4, 3, '2024-12-10', '09:00:00', 70000, N'DangChieu'),
(4, 3, '2024-12-10', '11:30:00', 75000, N'DangChieu'),
(4, 3, '2024-12-10', '15:00:00', 80000, N'DangChieu'),

-- Phim "Deadpool & Wolverine" - Phòng 4 (Sense City)
(7, 4, '2024-12-10', '20:00:00', 110000, N'DangChieu'),
(7, 4, '2024-12-10', '22:30:00', 100000, N'DangChieu'),

-- Ngày 11/12/2024
(1, 1, '2024-12-11', '10:00:00', 75000, N'SapChieu'),
(1, 1, '2024-12-11', '14:30:00', 85000, N'SapChieu'),
(2, 2, '2024-12-11', '11:00:00', 70000, N'SapChieu'),
(5, 3, '2024-12-11', '19:30:00', 95000, N'SapChieu'),
(6, 4, '2024-12-11', '21:00:00', 90000, N'SapChieu');

-- ========== 7. Thêm Khách hàng - Cập nhật 2025 ==========
INSERT INTO KhachHang (HoTen, SoDienThoai, Email, NgaySinh, DiemTichLuy, HangThanhVien) VALUES
(N'Phạm Thị Trúc Mỹ', '0901234567', 'phamthitrucmy@gmail.com', '2005-05-15', 250, N'Bac'),
(N'Trần Quốc Khái', '0912345678', 'tranquockhai@gmail.com', '2005-08-20', 500, N'Vang'),
(N'Nguyễn Thị Tuyết', '0923456789', 'nguyenthituyet@gmail.com', '2005-03-10', 80, N'Dong'),
(N'Phạm Thảo My', '0934567890', 'phamthaomy@gmail.com', '2005-12-05', 320, N'Bac'),
(N'Nguyễn Phạm Thùy Dương', '0945678901', 'nguyenphamthuyduong@gmail.com', '2005-07-22', 150, N'Dong'),
(N'Trần Bội Anh', '0956789012', 'tranboianh@gmail.com', '2005-11-18', 420, N'Vang'),
(N'Đặng Thành Dỉ', '0967890123', 'dangthanhdi@gmail.com', '2005-02-14', 90, N'Dong'),
(N'Kiến Nguyễn Trường Giang', '0978901234', 'kiennguyentruonggiang@gmail.com', '2005-09-30', 180, N'Dong'),
(N'Trần Thị Thủy Tiên', '0989012345', 'tranthithuytien@gmail.com', '2005-04-25', 350, N'Bac'),
(N'Trần Chí Vỹ', '0990123456', 'tranchivy@gmail.com', '2005-06-08', 220, N'Dong');

-- ========== 8. Thêm Sản phẩm bắp nước (Theo chi nhánh) ==========
INSERT INTO SanPham (TenSanPham, LoaiSanPham, GiaBan, MaChiNhanh, SoLuongTon, DonVi) VALUES
-- Chi nhánh 1: Vincom Xuân Khánh
(N'Bắp rang bơ (Lớn)', N'Bap', 60000, 1, 150, N'Hộp'),
(N'Bắp rang bơ (Vừa)', N'Bap', 50000, 1, 200, N'Hộp'),
(N'Bắp rang bơ (Nhỏ)', N'Bap', 40000, 1, 180, N'Hộp'),
(N'Bắp phô mai (Lớn)', N'Bap', 65000, 1, 120, N'Hộp'),
(N'Coca Cola (Lớn)', N'Nuoc', 35000, 1, 250, N'Ly'),
(N'Coca Cola (Vừa)', N'Nuoc', 30000, 1, 300, N'Ly'),
(N'Pepsi (Lớn)', N'Nuoc', 35000, 1, 200, N'Ly'),
(N'7Up (Lớn)', N'Nuoc', 35000, 1, 180, N'Ly'),
(N'Nước suối Aquafina', N'Nuoc', 15000, 1, 400, N'Chai'),
(N'Combo Đơn (Bắp lớn + Nước lớn)', N'Combo', 85000, 1, 100, N'Suất'),
(N'Combo Đôi (2 Bắp lớn + 2 Nước lớn)', N'Combo', 160000, 1, 80, N'Suất'),
(N'Combo Gia đình (3 Bắp lớn + 3 Nước lớn)', N'Combo', 230000, 1, 50, N'Suất'),

-- Chi nhánh 2: Sense City
(N'Bắp rang bơ (Lớn)', N'Bap', 60000, 2, 180, N'Hộp'),
(N'Bắp rang bơ (Vừa)', N'Bap', 50000, 2, 220, N'Hộp'),
(N'Coca Cola (Lớn)', N'Nuoc', 35000, 2, 300, N'Ly'),
(N'Pepsi (Vừa)', N'Nuoc', 30000, 2, 250, N'Ly'),
(N'Combo Tiết kiệm (Bắp vừa + Nước vừa)', N'Combo', 70000, 2, 120, N'Suất'),

-- Chi nhánh 3: Lotte Cinema
(N'Bắp rang bơ (Lớn)', N'Bap', 65000, 3, 200, N'Hộp'),
(N'Bắp caramel (Lớn)', N'Bap', 70000, 3, 100, N'Hộp'),
(N'Coca Cola (Lớn)', N'Nuoc', 38000, 3, 280, N'Ly'),
(N'Combo Đôi (2 Bắp + 2 Nước)', N'Combo', 170000, 3, 90, N'Suất');

-- ========== 9. Thêm Vé đã bán (Mẫu) ==========
INSERT INTO Ve (MaSuatChieu, MaGhe, MaKhachHang, DoiTuongKhachHang, GiaVe, TrangThai, MaVeCode) VALUES
-- Suất chiếu 1: Phim Mai lúc 9:30
(1, 1, 1, N'NguoiLon', 75000, N'DaBan', 'VE001-20241210-001'),
(1, 2, 1, N'NguoiLon', 75000, N'DaBan', 'VE001-20241210-002'),
(1, 11, 2, N'NguoiLon', 75000, N'DaBan', 'VE001-20241210-003'),
(1, 12, 2, N'SinhVien', 65000, N'DaBan', 'VE001-20241210-004'), -- Giảm 10k cho SV
(1, 13, 2, N'NguoiLon', 75000, N'DaBan', 'VE001-20241210-005'),

-- Suất chiếu 2: Phim Mai lúc 13:45 (Ghế VIP)
(2, 81, 3, N'NguoiLon', 105000, N'DaBan', 'VE002-20241210-001'), -- Ghế H1 (VIP +30k)
(2, 82, 3, N'NguoiLon', 105000, N'DaBan', 'VE002-20241210-002'),

-- Suất chiếu 3: Phim Mai lúc 18:00
(3, 45, 4, N'NguoiLon', 95000, N'DaBan', 'VE003-20241210-001'),
(3, 46, 4, N'NguoiLon', 95000, N'DaBan', 'VE003-20241210-002'),
(3, 47, NULL, N'HocSinh', 80000, N'DaBan', 'VE003-20241210-003'), -- Khách vãng lai - học sinh

-- Suất chiếu 5: Lật Mặt 7 lúc 14:15
(5, 35, 5, N'NguoiLon', 80000, N'DaBan', 'VE005-20241210-001'),
(5, 36, 5, N'TreEm', 60000, N'DaBan', 'VE005-20241210-002'), -- Trẻ em giảm 20k
(5, 37, 5, N'NguoiLon', 80000, N'DaBan', 'VE005-20241210-003');

-- ========== 10. Thêm Hóa đơn ==========
INSERT INTO HoaDon (MaNguoiDung, MaKhachHang, TongTien, GiamGia, ThanhTien, PhuongThucThanhToan, TrangThaiThanhToan, GhiChu) VALUES
-- Hóa đơn 1: Khách hàng 1 mua 2 vé + 1 combo đơn
(3, 1, 245000, 10000, 235000, N'ChuyenKhoan', N'DaThanhToan', N'Khách hàng hạng Bạc - Giảm giá 10k'),
-- Hóa đơn 2: Khách hàng 2 mua 3 vé + 1 combo đôi
(3, 2, 385000, 20000, 365000, N'ChuyenKhoan', N'DaThanhToan', N'Khách hàng hạng Vàng - Giảm giá 20k'),
-- Hóa đơn 3: Khách hàng 3 mua 2 vé VIP + bắp nước riêng lẻ
(4, 3, 305000, 0, 305000, N'TienMat', N'DaThanhToan', NULL),
-- Hóa đơn 4: Khách hàng 4 mua 2 vé + combo tiết kiệm
(3, 4, 260000, 0, 260000, N'TienMat', N'DaThanhToan', NULL),
-- Hóa đơn 5: Khách vãng lai mua 1 vé + bắp vừa + nước vừa
(4, NULL, 175000, 0, 175000, N'TienMat', N'DaThanhToan', N'Khách vãng lai'),
-- Hóa đơn 6: Khách hàng 5 mua 3 vé + combo gia đình
(4, 5, 470000, 15000, 455000, N'ChuyenKhoan', N'DaThanhToan', N'Tích điểm thành viên');

-- ========== 11. Thêm Chi tiết hóa đơn ==========
-- Hóa đơn 1: 2 vé + 1 combo đơn
INSERT INTO ChiTietHoaDon (MaHoaDon, MaVe, MaSanPham, SoLuong, DonGia, ThanhTien) VALUES
(1, 1, NULL, 1, 75000, 75000), -- Vé 1
(1, 2, NULL, 1, 75000, 75000), -- Vé 2
(1, NULL, 11, 1, 85000, 85000); -- Combo đơn

-- Hóa đơn 2: 3 vé + 1 combo đôi
INSERT INTO ChiTietHoaDon (MaHoaDon, MaVe, MaSanPham, SoLuong, DonGia, ThanhTien) VALUES
(2, 3, NULL, 1, 75000, 75000), -- Vé 3
(2, 4, NULL, 1, 75000, 75000), -- Vé 4
(2, 5, NULL, 1, 75000, 75000), -- Vé 5
(2, NULL, 12, 1, 160000, 160000); -- Combo đôi

-- Hóa đơn 3: 2 vé VIP + bắp phô mai lớn + 2 nước
INSERT INTO ChiTietHoaDon (MaHoaDon, MaVe, MaSanPham, SoLuong, DonGia, ThanhTien) VALUES
(3, 6, NULL, 1, 105000, 105000), -- Vé VIP 1
(3, 7, NULL, 1, 105000, 105000), -- Vé VIP 2
(3, NULL, 4, 1, 65000, 65000), -- Bắp phô mai lớn
(3, NULL, 5, 2, 35000, 70000); -- 2 Coca lớn

-- Hóa đơn 4: 2 vé + combo tiết kiệm
INSERT INTO ChiTietHoaDon (MaHoaDon, MaVe, MaSanPham, SoLuong, DonGia, ThanhTien) VALUES
(4, 8, NULL, 1, 95000, 95000), -- Vé 1
(4, 9, NULL, 1, 95000, 95000), -- Vé 2
(4, NULL, 14, 1, 70000, 70000); -- Combo tiết kiệm

-- Hóa đơn 5: 1 vé + bắp vừa + nước vừa (khách vãng lai)
INSERT INTO ChiTietHoaDon (MaHoaDon, MaVe, MaSanPham, SoLuong, DonGia, ThanhTien) VALUES
(5, 10, NULL, 1, 95000, 95000), -- Vé
(5, NULL, 2, 1, 50000, 50000), -- Bắp vừa
(5, NULL, 6, 1, 30000, 30000); -- Nước vừa

-- Hóa đơn 6: 3 vé + combo gia đình
INSERT INTO ChiTietHoaDon (MaHoaDon, MaVe, MaSanPham, SoLuong, DonGia, ThanhTien) VALUES
(6, 11, NULL, 1, 80000, 80000), -- Vé 1
(6, 12, NULL, 1, 80000, 80000), -- Vé 2
(6, 13, NULL, 1, 80000, 80000), -- Vé 3
(6, NULL, 13, 1, 230000, 230000); -- Combo gia đình

GO

-- ============================================================
-- STORED PROCEDURES (Hỗ trợ nghiệp vụ)
-- ============================================================

-- SP: Lấy danh sách ghế trống cho 1 suất chiếu
CREATE PROCEDURE sp_LayGheTrong
    @MaSuatChieu INT
AS
BEGIN
    SELECT g.MaGhe, g.SoGhe, g.SoHang, g.LoaiGhe
    FROM GheNgoi g
    INNER JOIN SuatChieu sc ON g.MaPhong = sc.MaPhong
    WHERE sc.MaSuatChieu = @MaSuatChieu
      AND g.MaGhe NOT IN (
          SELECT MaGhe FROM Ve 
          WHERE MaSuatChieu = @MaSuatChieu AND TrangThai = N'DaBan'
      )
      AND g.TrangThai = 1
    ORDER BY g.SoHang, g.SoGhe;
END
GO

-- SP: Thống kê doanh thu theo ngày
CREATE PROCEDURE sp_ThongKeDoanhThuTheoNgay
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SELECT 
        CAST(NgayLap AS DATE) AS NgayBan,
        COUNT(MaHoaDon) AS SoHoaDon,
        SUM(ThanhTien) AS TongDoanhThu,
        SUM(GiamGia) AS TongGiamGia
    FROM HoaDon
    WHERE CAST(NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
      AND TrangThaiThanhToan = N'DaThanhToan'
    GROUP BY CAST(NgayLap AS DATE)
    ORDER BY NgayBan DESC;
END
GO

-- SP: Thống kê phim bán chạy
CREATE PROCEDURE sp_ThongKePhimBanChay
    @Top INT = 10
AS
BEGIN
    SELECT TOP (@Top)
        p.MaPhim,
        p.TenPhim,
        p.TheLoai,
        COUNT(v.MaVe) AS SoVeBan,
        SUM(v.GiaVe) AS DoanhThu
    FROM Phim p
    INNER JOIN SuatChieu sc ON p.MaPhim = sc.MaPhim
    INNER JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu
    WHERE v.TrangThai = N'DaBan'
    GROUP BY p.MaPhim, p.TenPhim, p.TheLoai
    ORDER BY SoVeBan DESC;
END
GO

-- SP: Tính điểm tích lũy cho khách hàng (10.000đ = 1 điểm)
CREATE PROCEDURE sp_TichDiemKhachHang
    @MaKhachHang INT,
    @SoTien DECIMAL(10,2)
AS
BEGIN
    DECLARE @DiemThem INT;
    SET @DiemThem = FLOOR(@SoTien / 10000); -- Mỗi 10k = 1 điểm
    
    UPDATE KhachHang 
    SET DiemTichLuy = DiemTichLuy + @DiemThem,
        HangThanhVien = CASE 
            WHEN DiemTichLuy + @DiemThem >= 500 THEN N'Vang'
            WHEN DiemTichLuy + @DiemThem >= 200 THEN N'Bac'
            ELSE N'Dong'
        END
    WHERE MaKhachHang = @MaKhachHang;
    
    SELECT DiemTichLuy, HangThanhVien FROM KhachHang WHERE MaKhachHang = @MaKhachHang;
END
GO

-- SP: Báo cáo doanh thu theo chi nhánh
CREATE PROCEDURE sp_BaoCaoDoanhThuChiNhanh
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SELECT 
        cn.MaChiNhanh,
        cn.TenChiNhanh,
        COUNT(hd.MaHoaDon) AS SoHoaDon,
        SUM(hd.ThanhTien) AS TongDoanhThu,
        AVG(hd.ThanhTien) AS DoanhThuTrungBinh
    FROM ChiNhanh cn
    LEFT JOIN NguoiDung nd ON cn.MaChiNhanh = nd.MaChiNhanh
    LEFT JOIN HoaDon hd ON nd.MaNguoiDung = hd.MaNguoiDung
    WHERE CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
      AND hd.TrangThaiThanhToan = N'DaThanhToan'
    GROUP BY cn.MaChiNhanh, cn.TenChiNhanh
    ORDER BY TongDoanhThu DESC;
END
GO

-- ============================================================
-- DỮ LIỆU MẪU CHO CÁC BẢNG MỚI
-- ============================================================

-- ========== 12. Thêm Khuyến mãi ==========
INSERT INTO KhuyenMai (TenKhuyenMai, MaCode, LoaiGiamGia, GiaTriGiamGia, GiamToiDa, DieuKienToiThieu, SoLuongMa, SoLuongDaSuDung, NgayBatDau, NgayKetThuc, MoTa) VALUES
(N'Giảm 20% cho thành viên mới', 'NEWMEMBER20', N'PhanTram', 20, 50000, 100000, 100, 15, '2024-12-01', '2024-12-31', N'Áp dụng cho hóa đơn từ 100k'),
(N'Giảm 50k cho combo gia đình', 'FAMILY50K', N'TienMat', 50000, NULL, 200000, 50, 8, '2024-12-01', '2024-12-31', N'Mua combo gia đình giảm ngay 50k'),
(N'Giảm 15% cuối tuần', 'WEEKEND15', N'PhanTram', 15, 40000, 150000, 200, 45, '2024-12-01', '2024-12-31', N'Áp dụng thứ 7, CN'),
(N'Giảm 30k sinh nhật', 'BDAY30K', N'TienMat', 30000, NULL, 0, 500, 12, '2024-01-01', '2024-12-31', N'Giảm giá trong tháng sinh nhật'),
(N'Voucher 100k', 'VIP100K', N'TienMat', 100000, NULL, 300000, 20, 3, '2024-12-10', '2024-12-20', N'Dành cho khách VIP');

-- ========== 13. Thêm Cấu hình giá vé ==========
INSERT INTO CauHinhGiaVe (LoaiGhe, LoaiNgay, DoiTuongKhachHang, GiaGoc, PhuThu, GhiChu) VALUES
-- Ghế thường - Thứ thường
(N'Thuong', N'ThuThuong', N'NguoiLon', 70000, 0, N'Giá chuẩn ngày thường'),
(N'Thuong', N'ThuThuong', N'HocSinh', 70000, -10000, N'Giảm 10k cho học sinh'),
(N'Thuong', N'ThuThuong', N'SinhVien', 70000, -10000, N'Giảm 10k cho sinh viên'),
(N'Thuong', N'ThuThuong', N'NguoiGia', 70000, -15000, N'Giảm 15k cho người già'),
(N'Thuong', N'ThuThuong', N'TreEm', 70000, -20000, N'Giảm 20k cho trẻ em'),

-- Ghế thường - Cuối tuần
(N'Thuong', N'CuoiTuan', N'NguoiLon', 85000, 0, N'Giá cuối tuần'),
(N'Thuong', N'CuoiTuan', N'HocSinh', 85000, -10000, N'Giảm 10k cho học sinh'),
(N'Thuong', N'CuoiTuan', N'SinhVien', 85000, -10000, N'Giảm 10k cho sinh viên'),
(N'Thuong', N'CuoiTuan', N'TreEm', 85000, -20000, N'Giảm 20k cho trẻ em'),

-- Ghế VIP - Thứ thường
(N'VIP', N'ThuThuong', N'NguoiLon', 70000, 30000, N'Phụ thu VIP 30k'),
(N'VIP', N'ThuThuong', N'HocSinh', 70000, 20000, N'Phụ thu VIP 20k cho học sinh'),
(N'VIP', N'ThuThuong', N'SinhVien', 70000, 20000, N'Phụ thu VIP 20k cho sinh viên'),

-- Ghế VIP - Cuối tuần
(N'VIP', N'CuoiTuan', N'NguoiLon', 85000, 35000, N'Phụ thu VIP 35k cuối tuần'),
(N'VIP', N'CuoiTuan', N'SinhVien', 85000, 25000, N'Phụ thu VIP 25k cho sinh viên'),

-- Lễ hội
(N'Thuong', N'LeHoi', N'NguoiLon', 100000, 0, N'Giá lễ Tết'),
(N'VIP', N'LeHoi', N'NguoiLon', 100000, 40000, N'VIP lễ Tết');

-- ========== 14. Thêm Ca làm việc ==========
INSERT INTO CaLamViec (TenCa, GioBatDau, GioKetThuc, MoTa) VALUES
(N'Ca Sáng', '07:00:00', '13:00:00', N'Ca làm việc buổi sáng'),
(N'Ca Chiều', '13:00:00', '19:00:00', N'Ca làm việc buổi chiều'),
(N'Ca Tối', '19:00:00', '23:30:00', N'Ca làm việc buổi tối');

-- ========== 15. Thêm Phân công ca ==========
INSERT INTO PhanCongCa (MaNguoiDung, MaCa, NgayLamViec, TienDauCa, TienCuoiCa, TienThucTe, ChenhLech, TrangThai, ThoiGianBatDau, ThoiGianKetThuc) VALUES
-- Ngày 10/12/2024
(5, 1, '2024-12-10', 500000, 2350000, 2350000, 0, N'DaKetThuc', '2024-12-10 07:00:00', '2024-12-10 13:00:00'), -- Lê Thị Thanh Tâm - Ca sáng
(6, 2, '2024-12-10', 2350000, 4780000, 4780000, 0, N'DaKetThuc', '2024-12-10 13:00:00', '2024-12-10 19:00:00'), -- Huỳnh Văn Đạt - Ca chiều
(5, 3, '2024-12-10', 4780000, 6240000, 6250000, 10000, N'DaKetThuc', '2024-12-10 19:00:00', '2024-12-10 23:30:00'), -- Lê Thị Thanh Tâm - Ca tối (thừa 10k)

-- Ngày 11/12/2024
(6, 1, '2024-12-11', 500000, 500000, 500000, 0, N'DangLam', '2024-12-11 07:00:00', NULL), -- Huỳnh Văn Đạt - Ca sáng (đang làm)
(8, 2, '2024-12-11', 0, 0, 0, 0, N'ChuaBatDau', NULL, NULL), -- Võ Thị Mai Anh - Ca chiều
(9, 3, '2024-12-11', 0, 0, 0, 0, N'ChuaBatDau', NULL, NULL); -- Trịnh Hoàng Nam - Ca tối

-- ========== 16. Thêm Lịch sử hoạt động ==========
INSERT INTO LichSuHoatDong (MaNguoiDung, HanhDong, BangLienQuan, MaBanGhi, DuLieuCu, DuLieuMoi, LyDo, ThoiGian) VALUES
(2, N'Tạo mới phim', 'Phim', 1, NULL, N'{"TenPhim":"Mai","TheLoai":"Tâm lý","ThoiLuong":131}', N'Thêm phim mới vào hệ thống', '2024-12-01 09:30:00'),
(5, N'Hủy vé', 'Ve', 15, N'{"TrangThai":"DaBan"}', N'{"TrangThai":"DaHuy"}', N'Khách yêu cầu hủy vé do có việc bận', '2024-12-10 10:15:00'),
(2, N'Sửa giá sản phẩm', 'SanPham', 1, N'{"GiaBan":55000}', N'{"GiaBan":60000}', N'Điều chỉnh giá theo thị trường', '2024-12-09 14:20:00'),
(1, N'Thêm nhân viên mới', 'NguoiDung', 10, NULL, N'{"HoTen":"Bùi Văn Hải","VaiTro":"NhanVien"}', N'Tuyển dụng nhân viên mới', '2024-11-25 08:00:00'),
(6, N'Cập nhật thông tin khách hàng', 'KhachHang', 2, N'{"DiemTichLuy":480}', N'{"DiemTichLuy":500}', N'Cộng điểm sau giao dịch', '2024-12-10 18:45:00');

-- ========== 17. Thêm Báo cáo sự cố ==========
INSERT INTO BaoCaoSuCo (MaNguoiDung, MaPhong, MaGhe, LoaiSuCo, MoTa, MucDoUuTien, TrangThai, NguoiXuLy, NgayBaoCao, NgayXuLy, GhiChuXuLy) VALUES
(5, 1, 45, N'Ghế hỏng', N'Ghế E5 bị gãy tay vịn bên phải, cần thay thế', N'Cao', N'DaXong', 2, '2024-12-09 14:30:00', '2024-12-09 16:00:00', N'Đã thay ghế mới'),
(6, 2, NULL, N'Máy chiếu lỗi', N'Máy chiếu phòng 2 bị mờ hình, cần kiểm tra đèn', N'KhanCap', N'DaXong', 2, '2024-12-08 19:00:00', '2024-12-08 19:45:00', N'Đã thay bóng đèn mới'),
(8, 4, NULL, N'Âm thanh', N'Loa phía sau phòng không hoạt động', N'BinhThuong', N'DangXuLy', 3, '2024-12-10 20:15:00', NULL, NULL),
(9, 3, 78, N'Ghế hỏng', N'Ghế H8 không ngả được', N'Thap', N'ChoXuLy', NULL, '2024-12-10 21:00:00', NULL, NULL),
(5, 1, NULL, N'Điều hòa', N'Điều hòa phòng 1 không đủ lạnh', N'Cao', N'DangXuLy', 2, '2024-12-10 15:30:00', NULL, NULL);

GO

-- ============================================================
-- HOÀN THÀNH
-- ============================================================
PRINT N'===================================================================';
PRINT N'✓ Tạo database CinemaDB thành công!';
PRINT N'✓ Tổng số bảng: 17 (đầy đủ chức năng)';
PRINT N'✓ Đã thêm dữ liệu mẫu thực tế tại Cần Thơ';
PRINT N'✓ Đã tạo 5 Stored Procedures hỗ trợ nghiệp vụ';
PRINT N'===================================================================';
PRINT N'Dữ liệu mẫu bao gồm:';
PRINT N'  - 3 Chi nhánh rạp tại Cần Thơ';
PRINT N'  - 10 Người dùng (1 Admin + 3 Quản lý + 6 Nhân viên)';
PRINT N'  - 8 Phòng chiếu với 180 ghế';
PRINT N'  - 8 Phim đang chiếu thực tế';
PRINT N'  - 17 Suất chiếu ngày 10-11/12/2024';
PRINT N'  - 8 Khách hàng thành viên';
PRINT N'  - 21 Sản phẩm bắp nước (phân theo chi nhánh)';
PRINT N'  - 16 Vé đã bán mẫu (có phân loại đối tượng)';
PRINT N'  - 6 Hóa đơn với chi tiết đầy đủ';
PRINT N'  - 5 Khuyến mãi/Voucher';
PRINT N'  - 16 Cấu hình giá vé (theo loại ghế, ngày, đối tượng)';
PRINT N'  - 3 Ca làm việc';
PRINT N'  - 6 Phân công ca mẫu';
PRINT N'  - 5 Lịch sử hoạt động';
PRINT N'  - 5 Báo cáo sự cố';
PRINT N'===================================================================';
PRINT N'Các bảng mới được thêm:';
PRINT N'  12. KhuyenMai - Quản lý voucher, mã giảm giá';
PRINT N'  13. CauHinhGiaVe - Cấu hình giá linh hoạt theo điều kiện';
PRINT N'  14. CaLamViec - Định nghĩa ca làm việc';
PRINT N'  15. PhanCongCa - Phân công và báo cáo giao ca';
PRINT N'  16. LichSuHoatDong - Log mọi thao tác quan trọng';
PRINT N'  17. BaoCaoSuCo - Quản lý sự cố rạp, thiết bị';
PRINT N'===================================================================';
GO