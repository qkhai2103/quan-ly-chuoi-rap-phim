using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace QuanLiChuoiRapPhim.BLL
{
    /// <summary>
    /// Helper class for exporting data to various formats (CSV, Excel-compatible)
    /// </summary>
    public static class ExportHelper
    {
        /// <summary>
        /// Export DataTable to CSV file
        /// </summary>
        /// <param name="dt">DataTable to export</param>
        /// <param name="filePath">Target file path</param>
        /// <param name="includeHeaders">Include column headers</param>
        public static void ExportToCsv(DataTable dt, string filePath, bool includeHeaders = true)
        {
            StringBuilder sb = new StringBuilder();

            // Headers
            if (includeHeaders)
            {
                string[] columnNames = new string[dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    columnNames[i] = EscapeCsvValue(dt.Columns[i].ColumnName);
                }
                sb.AppendLine(string.Join(",", columnNames));
            }

            // Data rows
            foreach (DataRow row in dt.Rows)
            {
                string[] values = new string[dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    values[i] = EscapeCsvValue(row[i]?.ToString() ?? "");
                }
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Export DataGridView to CSV file
        /// </summary>
        public static void ExportDataGridViewToCsv(DataGridView dgv, string filePath)
        {
            StringBuilder sb = new StringBuilder();

            // Headers
            string[] headers = new string[dgv.Columns.Count];
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                headers[i] = EscapeCsvValue(dgv.Columns[i].HeaderText);
            }
            sb.AppendLine(string.Join(",", headers));

            // Data rows
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                
                string[] values = new string[dgv.Columns.Count];
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    values[i] = EscapeCsvValue(row.Cells[i].Value?.ToString() ?? "");
                }
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Export DataTable to Excel XML format (can be opened by Excel)
        /// </summary>
        public static void ExportToExcelXml(DataTable dt, string filePath, string sheetName = "Sheet1")
        {
            StringBuilder sb = new StringBuilder();

            // XML header
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");

            // Styles
            sb.AppendLine("<Styles>");
            sb.AppendLine("<Style ss:ID=\"Header\">");
            sb.AppendLine("<Font ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/>");
            sb.AppendLine("<Interior ss:Color=\"#E21A3C\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("<Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>");
            sb.AppendLine("</Style>");
            sb.AppendLine("<Style ss:ID=\"Data\">");
            sb.AppendLine("<Alignment ss:Vertical=\"Center\"/>");
            sb.AppendLine("</Style>");
            sb.AppendLine("<Style ss:ID=\"Number\">");
            sb.AppendLine("<NumberFormat ss:Format=\"#,##0\"/>");
            sb.AppendLine("<Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
            sb.AppendLine("</Style>");
            sb.AppendLine("<Style ss:ID=\"Date\">");
            sb.AppendLine("<NumberFormat ss:Format=\"dd/mm/yyyy\"/>");
            sb.AppendLine("<Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>");
            sb.AppendLine("</Style>");
            sb.AppendLine("</Styles>");

            // Worksheet
            sb.AppendLine($"<Worksheet ss:Name=\"{EscapeXmlValue(sheetName)}\">");
            sb.AppendLine("<Table>");

            // Column widths
            foreach (DataColumn col in dt.Columns)
            {
                int width = Math.Max(col.ColumnName.Length * 10, 80);
                sb.AppendLine($"<Column ss:AutoFitWidth=\"1\" ss:Width=\"{width}\"/>");
            }

            // Header row
            sb.AppendLine("<Row ss:Height=\"25\">");
            foreach (DataColumn col in dt.Columns)
            {
                sb.AppendLine($"<Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">{EscapeXmlValue(col.ColumnName)}</Data></Cell>");
            }
            sb.AppendLine("</Row>");

            // Data rows
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine("<Row ss:Height=\"20\">");
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    object value = row[i];
                    string dataType = "String";
                    string styleId = "Data";
                    string cellValue = "";

                    if (value == null || value == DBNull.Value)
                    {
                        cellValue = "";
                    }
                    else if (value is DateTime dt2)
                    {
                        dataType = "DateTime";
                        styleId = "Date";
                        cellValue = dt2.ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                    else if (value is decimal || value is int || value is long || value is double || value is float)
                    {
                        dataType = "Number";
                        styleId = "Number";
                        cellValue = Convert.ToString(value);
                    }
                    else
                    {
                        cellValue = EscapeXmlValue(value.ToString());
                    }

                    sb.AppendLine($"<Cell ss:StyleID=\"{styleId}\"><Data ss:Type=\"{dataType}\">{cellValue}</Data></Cell>");
                }
                sb.AppendLine("</Row>");
            }

            sb.AppendLine("</Table>");
            sb.AppendLine("</Worksheet>");
            sb.AppendLine("</Workbook>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Show SaveFileDialog and export DataTable
        /// </summary>
        public static bool ExportWithDialog(DataTable dt, string defaultFileName, IWin32Window owner = null)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files (*.xlsx)|*.xml|CSV Files (*.csv)|*.csv";
                sfd.FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                sfd.Title = "Xuất dữ liệu";

                if (sfd.ShowDialog(owner) == DialogResult.OK)
                {
                    try
                    {
                        if (sfd.FilterIndex == 1) // Excel XML
                        {
                            ExportToExcelXml(dt, sfd.FileName, defaultFileName);
                        }
                        else // CSV
                        {
                            ExportToCsv(dt, sfd.FileName);
                        }

                        MessageBox.Show($"Xuất file thành công!\n{sfd.FileName}", 
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xuất file: {ex.Message}", 
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Show SaveFileDialog and export DataGridView
        /// </summary>
        public static bool ExportDataGridViewWithDialog(DataGridView dgv, string defaultFileName, IWin32Window owner = null)
        {
            // Convert DataGridView to DataTable first
            DataTable dt = new DataTable();
            
            // Add columns
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Visible)
                    dt.Columns.Add(col.HeaderText);
            }

            // Add rows
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                
                DataRow dr = dt.NewRow();
                int colIndex = 0;
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Visible)
                    {
                        dr[colIndex] = row.Cells[col.Index].Value?.ToString() ?? "";
                        colIndex++;
                    }
                }
                dt.Rows.Add(dr);
            }

            return ExportWithDialog(dt, defaultFileName, owner);
        }

        /// <summary>
        /// Escape special characters for CSV
        /// </summary>
        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }

        /// <summary>
        /// Escape special characters for XML
        /// </summary>
        private static string EscapeXmlValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}
