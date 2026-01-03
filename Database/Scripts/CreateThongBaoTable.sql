-- Script: Create ThongBao (Notification) Table
-- Purpose: Store notifications for users when proposals are approved/rejected, leave requests are processed, etc.

-- Check if table exists and create if not
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThongBao')
BEGIN
    CREATE TABLE ThongBao (
        MaThongBao INT IDENTITY(1,1) PRIMARY KEY,
        MaNguoiNhan INT NOT NULL,
        TieuDe NVARCHAR(200) NOT NULL,
        NoiDung NVARCHAR(500),
        LoaiThongBao NVARCHAR(50) NOT NULL, -- DeXuatDaDuyet, DeXuatBiTuChoi, YeuCauNghiDuyet, YeuCauNghiBiTuChoi, BaoCaoSuCo, etc.
        DaDoc BIT DEFAULT 0,
        NgayTao DATETIME DEFAULT GETDATE(),
        LienKet NVARCHAR(100), -- e.g., "DeXuat:15", "YeuCauNghi:8" to link to related entity
        MaNguoiGui INT,
        CONSTRAINT FK_ThongBao_NguoiNhan FOREIGN KEY (MaNguoiNhan) REFERENCES NguoiDung(MaNguoiDung),
        CONSTRAINT FK_ThongBao_NguoiGui FOREIGN KEY (MaNguoiGui) REFERENCES NguoiDung(MaNguoiDung)
    );

    -- Index for fast lookup by recipient and unread status
    CREATE INDEX IX_ThongBao_NguoiNhan ON ThongBao(MaNguoiNhan, DaDoc);
    CREATE INDEX IX_ThongBao_NgayTao ON ThongBao(NgayTao DESC);

    PRINT N'Table ThongBao created successfully!';
END
ELSE
BEGIN
    PRINT N'Table ThongBao already exists.';
END
GO

-- Insert sample notifications for testing (optional - can be removed in production)
-- These will be created automatically by the system when proposals are approved/rejected
/*
INSERT INTO ThongBao (MaNguoiNhan, TieuDe, NoiDung, LoaiThongBao, DaDoc, LienKet, MaNguoiGui)
VALUES 
(1, N'Đề xuất đã được duyệt', N'Đề xuất giảm suất chiếu phim "Mai" đã được Admin duyệt.', 'DeXuatDaDuyet', 0, 'DeXuat:1', 2),
(1, N'Đề xuất bị từ chối', N'Đề xuất xóa phim "Deadpool" bị từ chối. Lý do: Phim vẫn còn tiềm năng.', 'DeXuatBiTuChoi', 0, 'DeXuat:2', 2),
(3, N'Yêu cầu nghỉ phép đã duyệt', N'Yêu cầu nghỉ phép ngày 05/01 - 07/01 đã được quản lý duyệt.', 'YeuCauNghiDuyet', 0, 'YeuCauNghi:5', 1);
*/
