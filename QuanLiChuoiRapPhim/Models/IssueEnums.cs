using System;
using System.ComponentModel;

namespace QuanLiChuoiRapPhim.Models
{
    /// <summary>
    /// Độ ưu tiên của sự cố
    /// </summary>
    public enum IssuePriority
    {
        [Description("Thấp")]
        Low = 1,
        
        [Description("Trung bình")]
        Medium = 2,
        
        [Description("Cao")]
        High = 3
    }

    /// <summary>
    /// Trạng thái của báo cáo sự cố
    /// </summary>
    public enum IssueStatus
    {
        [Description("Chờ xử lý")]
        Pending = 1,
        
        [Description("Đang sửa")]
        InProgress = 2,
        
        [Description("Đã xong")]
        Resolved = 3
    }

    /// <summary>
    /// Extension methods cho Enum
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Lấy Description attribute value của enum
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();
            
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// Parse string thành enum theo Description
        /// </summary>
        public static T ParseByDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
            }
            
            // Fallback to standard parse
            return (T)Enum.Parse(typeof(T), description);
        }

        /// <summary>
        /// Chuyển enum value cũ (string) sang enum mới
        /// </summary>
        public static IssuePriority ParseLegacyPriority(string legacyValue)
        {
            return legacyValue?.ToLower() switch
            {
                "cao" => IssuePriority.High,
                "thap" => IssuePriority.Low,
                "binhthuong" => IssuePriority.Medium,
                "trung bình" => IssuePriority.Medium,
                _ => IssuePriority.Medium
            };
        }

        /// <summary>
        /// Chuyển enum thành database value (để backward compatibility)
        /// </summary>
        public static string ToLegacyValue(this IssuePriority priority)
        {
            return priority switch
            {
                IssuePriority.High => "Cao",
                IssuePriority.Low => "Thap",
                IssuePriority.Medium => "BinhThuong",
                _ => "BinhThuong"
            };
        }
    }
}
