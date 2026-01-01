using System;
using System.Data.SqlClient;
using System.IO;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.Migrations
{
    public class MigrationRunner
    {
        public static void RunMigration()
        {
            try
            {
                string script = @"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PhongChieu' AND COLUMN_NAME = 'LoaiPhong')
BEGIN
    ALTER TABLE PhongChieu ADD LoaiPhong NVARCHAR(20) NULL DEFAULT N'2D';
    UPDATE PhongChieu SET LoaiPhong = N'2D' WHERE LoaiPhong IS NULL;
END
";

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(script, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("Migration completed successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Migration error: {ex.Message}");
                // Don't throw - let app continue even if migration fails
            }
        }
    }
}
