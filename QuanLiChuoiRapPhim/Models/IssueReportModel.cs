using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLiChuoiRapPhim.Models
{
    /// <summary>
    /// Model cho việc validate và submit form báo cáo sự cố
    /// </summary>
    public class IssueReportModel
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; }
        public string LoaiSuCo { get; set; }
        public string MoTa { get; set; }
        public IssuePriority DoUuTien { get; set; }
        public List<string> HinhAnhFilePaths { get; set; } = new List<string>();

        /// <summary>
        /// Các loại sự cố hợp lệ
        /// </summary>
        public static readonly string[] ValidIssueTypes = new[]
        {
            "Máy chiếu",
            "Âm thanh",
            "Điều hòa",
            "Ghế",
            "Ánh sáng",
            "Khác"
        };

        /// <summary>
        /// Kiểm tra loại sự cố có hợp lệ không
        /// </summary>
        public bool IsValidIssueType()
        {
            return !string.IsNullOrWhiteSpace(LoaiSuCo) && 
                   ValidIssueTypes.Contains(LoaiSuCo);
        }

        /// <summary>
        /// Validate model (simple validation without FluentValidation)
        /// </summary>
        public Dictionary<string, string> Validate()
        {
            var errors = new Dictionary<string, string>();

            // Validate Room
            if (MaPhong <= 0)
                errors.Add(nameof(MaPhong), "Vui lòng chọn phòng chiếu");

            // Validate Issue Type
            if (string.IsNullOrWhiteSpace(LoaiSuCo))
                errors.Add(nameof(LoaiSuCo), "Vui lòng chọn loại sự cố");
            else if (!IsValidIssueType())
                errors.Add(nameof(LoaiSuCo), "Loại sự cố không hợp lệ");

            // Validate Description
            if (string.IsNullOrWhiteSpace(MoTa))
                errors.Add(nameof(MoTa), "Vui lòng nhập mô tả chi tiết");
            else if (MoTa.Length < 10)
                errors.Add(nameof(MoTa), "Mô tả phải có ít nhất 10 ký tự");
            else if (MoTa.Length > 500)
                errors.Add(nameof(MoTa), "Mô tả không được vượt quá 500 ký tự");

            // Validate Images (if any)
            if (HinhAnhFilePaths != null && HinhAnhFilePaths.Count > 0)
            {
                const long MAX_SIZE = 5 * 1024 * 1024; // 5MB
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                foreach (var path in HinhAnhFilePaths)
                {
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(path);
                        
                        if (fileInfo.Length > MAX_SIZE)
                        {
                            errors.Add($"Image_{fileInfo.Name}", 
                                $"File {fileInfo.Name} vượt quá 5MB");
                        }

                        if (!validExtensions.Contains(fileInfo.Extension.ToLower()))
                        {
                            errors.Add($"Image_{fileInfo.Name}", 
                                $"File {fileInfo.Name} không phải định dạng ảnh hợp lệ (jpg, png, gif)");
                        }
                    }
                    catch
                    {
                        errors.Add($"Image_{path}", "Không thể đọc file ảnh");
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Kiểm tra model có hợp lệ không
        /// </summary>
        public bool IsValid => Validate().Count == 0;
    }
}
