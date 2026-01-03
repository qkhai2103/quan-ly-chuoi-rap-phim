-- Script: Migrate existing ThongBao timestamps from local time to UTC
-- Purpose: Convert all existing NgayTao values to UTC for consistency
-- Run this ONCE after deploying the UTC changes

-- IMPORTANT: Adjust the offset based on your server's current timezone
-- If server is in Vietnam (UTC+7), we subtract 7 hours to get UTC
-- If server is already UTC, skip this migration

BEGIN TRANSACTION;

-- Backup existing data (optional but recommended)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThongBao_Backup')
BEGIN
    SELECT * INTO ThongBao_Backup FROM ThongBao;
    PRINT N'✅ Backup created: ThongBao_Backup';
END

-- Check current timezone offset
DECLARE @CurrentOffset INT = DATEPART(TZ, SYSDATETIMEOFFSET());
PRINT N'Current server timezone offset: ' + CAST(@CurrentOffset AS NVARCHAR(10)) + ' minutes';

-- Convert NgayTao from local time to UTC
-- Assuming server is in UTC+7 (Vietnam), subtract 7 hours
UPDATE ThongBao
SET NgayTao = DATEADD(HOUR, -7, NgayTao)
WHERE NgayTao IS NOT NULL;

DECLARE @RowsAffected INT = @@ROWCOUNT;
PRINT N'✅ Updated ' + CAST(@RowsAffected AS NVARCHAR(10)) + ' records to UTC';

-- Verify the migration
SELECT TOP 5
    MaThongBao,
    TieuDe,
    NgayTao AS NgayTao_UTC,
    DATEADD(HOUR, 7, NgayTao) AS NgayTao_LocalPreview
FROM ThongBao
ORDER BY MaThongBao DESC;

PRINT N'📋 Sample of migrated data shown above';
PRINT N'⚠️  Review the data before committing!';
PRINT N'Run COMMIT to apply changes, or ROLLBACK to cancel';

-- Uncomment one of the following:
-- COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION;
