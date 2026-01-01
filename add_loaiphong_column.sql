-- Add LoaiPhong column to PhongChieu table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PhongChieu' AND COLUMN_NAME = 'LoaiPhong')
BEGIN
    ALTER TABLE PhongChieu ADD LoaiPhong NVARCHAR(20) NULL DEFAULT N'2D';
    UPDATE PhongChieu SET LoaiPhong = N'2D' WHERE LoaiPhong IS NULL;
    PRINT 'Column LoaiPhong added successfully';
END
ELSE
BEGIN
    PRINT 'Column LoaiPhong already exists';
END
GO
