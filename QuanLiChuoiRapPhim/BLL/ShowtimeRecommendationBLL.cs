using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLiChuoiRapPhim.DAL;

namespace QuanLiChuoiRapPhim.BLL
{
    /// <summary>
    /// ShowtimeRecommendationBLL - Đề xuất suất chiếu thông minh
    /// Phân tích dữ liệu lịch sử để đề xuất khung giờ tối ưu
    /// </summary>
    public class ShowtimeRecommendationBLL
    {
        #region Models

        public class ShowtimeRecommendation
        {
            public TimeSpan RecommendedTime { get; set; }
            public string TimeSlot { get; set; }        // "Sáng", "Chiều", "Tối"
            public double PredictedOccupancy { get; set; }  // % dự đoán
            public double ConfidenceScore { get; set; }     // Độ tin cậy 0-100%
            public string Reasoning { get; set; }           // Giải thích
            public int Priority { get; set; }               // 1 = cao nhất
        }

        public class TimeSlotStats
        {
            public string TimeSlot { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int TotalShowtimes { get; set; }
            public int TotalTicketsSold { get; set; }
            public int TotalSeats { get; set; }
            public double AverageOccupancy { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal AverageRevenuePerShowtime { get; set; }
        }

        public class MoviePerformance
        {
            public int MaPhim { get; set; }
            public string TenPhim { get; set; }
            public string TheLoai { get; set; }
            public double AverageOccupancy { get; set; }
            public string BestTimeSlot { get; set; }
            public TimeSpan BestShowtime { get; set; }
        }

        public class DayOfWeekStats
        {
            public DayOfWeek Day { get; set; }
            public string DayName { get; set; }
            public double AverageOccupancy { get; set; }
            public int TotalShowtimes { get; set; }
            public string PeakTimeSlot { get; set; }
        }

        #endregion

        #region Main Recommendation Methods

        /// <summary>
        /// Đề xuất suất chiếu tối ưu cho phim mới
        /// </summary>
        public List<ShowtimeRecommendation> GetRecommendations(int maChiNhanh, int maPhim, DateTime ngayChieu, int maPhong)
        {
            var recommendations = new List<ShowtimeRecommendation>();

            try
            {
                // 1. Phân tích dữ liệu lịch sử theo khung giờ
                var timeSlotStats = GetTimeSlotStatistics(maChiNhanh, 90); // 90 ngày gần nhất

                // 2. Phân tích ngày trong tuần
                var dayStats = GetDayOfWeekStatistics(maChiNhanh, 90);
                var targetDayStats = dayStats.Find(d => d.Day == ngayChieu.DayOfWeek);

                // 3. Phân tích hiệu suất phim theo thể loại
                var moviePerf = GetMoviePerformanceByGenre(maPhim);

                // 4. Kiểm tra phòng đã có suất chiếu nào chưa
                var existingShowtimes = GetExistingShowtimes(maPhong, ngayChieu);

                // 5. Tạo đề xuất
                recommendations = GenerateRecommendations(
                    timeSlotStats, 
                    targetDayStats, 
                    moviePerf, 
                    existingShowtimes,
                    ngayChieu);
            }
            catch (Exception ex)
            {
                // Log error and return default recommendations
                System.Diagnostics.Debug.WriteLine($"Error in GetRecommendations: {ex.Message}");
                recommendations = GetDefaultRecommendations(ngayChieu);
            }

            return recommendations;
        }

        /// <summary>
        /// Phân tích thống kê theo khung giờ
        /// </summary>
        public List<TimeSlotStats> GetTimeSlotStatistics(int maChiNhanh, int daysBack = 90)
        {
            var stats = new List<TimeSlotStats>();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = @"
                    WITH ShowtimeData AS (
                        SELECT 
                            sc.MaSuatChieu,
                            sc.GioBatDau,
                            pc.SoGhe,
                            ISNULL(COUNT(v.MaVe), 0) as TicketsSold,
                            CASE 
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 9 AND 11 THEN 'Sáng'
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 12 AND 17 THEN 'Chiều'
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 18 AND 21 THEN 'Tối'
                                ELSE 'Khuya'
                            END as TimeSlot
                        FROM SuatChieu sc
                        INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                        LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu 
                            AND v.TrangThai IN ('Đã bán', 'Đã thanh toán')
                        WHERE pc.MaChiNhanh = @MaChiNhanh
                            AND sc.NgayChieu >= DATEADD(DAY, -@DaysBack, GETDATE())
                            AND sc.TrangThai = 'Đã chiếu'
                        GROUP BY sc.MaSuatChieu, sc.GioBatDau, pc.SoGhe
                    )
                    SELECT 
                        TimeSlot,
                        COUNT(*) as TotalShowtimes,
                        SUM(TicketsSold) as TotalTicketsSold,
                        SUM(SoGhe) as TotalSeats,
                        CAST(SUM(TicketsSold) AS FLOAT) / NULLIF(SUM(SoGhe), 0) * 100 as AvgOccupancy,
                        CASE TimeSlot
                            WHEN 'Sáng' THEN '09:00'
                            WHEN 'Chiều' THEN '14:00'
                            WHEN 'Tối' THEN '19:00'
                            ELSE '22:00'
                        END as StartTimeStr,
                        CASE TimeSlot
                            WHEN 'Sáng' THEN '12:00'
                            WHEN 'Chiều' THEN '18:00'
                            WHEN 'Tối' THEN '22:00'
                            ELSE '01:00'
                        END as EndTimeStr
                    FROM ShowtimeData
                    GROUP BY TimeSlot
                    ORDER BY 
                        CASE TimeSlot
                            WHEN 'Sáng' THEN 1
                            WHEN 'Chiều' THEN 2
                            WHEN 'Tối' THEN 3
                            ELSE 4
                        END";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@DaysBack", daysBack);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats.Add(new TimeSlotStats
                            {
                                TimeSlot = reader["TimeSlot"].ToString(),
                                TotalShowtimes = Convert.ToInt32(reader["TotalShowtimes"]),
                                TotalTicketsSold = Convert.ToInt32(reader["TotalTicketsSold"]),
                                TotalSeats = Convert.ToInt32(reader["TotalSeats"]),
                                AverageOccupancy = reader["AvgOccupancy"] != DBNull.Value 
                                    ? Convert.ToDouble(reader["AvgOccupancy"]) : 0,
                                StartTime = TimeSpan.Parse(reader["StartTimeStr"].ToString()),
                                EndTime = TimeSpan.Parse(reader["EndTimeStr"].ToString())
                            });
                        }
                    }
                }
            }

            // If no data, return default stats
            if (stats.Count == 0)
            {
                stats = GetDefaultTimeSlotStats();
            }

            return stats;
        }

        /// <summary>
        /// Phân tích thống kê theo ngày trong tuần
        /// </summary>
        public List<DayOfWeekStats> GetDayOfWeekStatistics(int maChiNhanh, int daysBack = 90)
        {
            var stats = new List<DayOfWeekStats>();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = @"
                    WITH DailyData AS (
                        SELECT 
                            DATEPART(WEEKDAY, sc.NgayChieu) as DayNum,
                            sc.MaSuatChieu,
                            pc.SoGhe,
                            ISNULL(COUNT(v.MaVe), 0) as TicketsSold,
                            CASE 
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 9 AND 11 THEN 'Sáng'
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 12 AND 17 THEN 'Chiều'
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 18 AND 21 THEN 'Tối'
                                ELSE 'Khuya'
                            END as TimeSlot
                        FROM SuatChieu sc
                        INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                        LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu 
                            AND v.TrangThai IN ('Đã bán', 'Đã thanh toán')
                        WHERE pc.MaChiNhanh = @MaChiNhanh
                            AND sc.NgayChieu >= DATEADD(DAY, -@DaysBack, GETDATE())
                            AND sc.TrangThai = 'Đã chiếu'
                        GROUP BY DATEPART(WEEKDAY, sc.NgayChieu), sc.MaSuatChieu, pc.SoGhe, sc.GioBatDau
                    ),
                    DayStats AS (
                        SELECT 
                            DayNum,
                            COUNT(*) as TotalShowtimes,
                            CAST(SUM(TicketsSold) AS FLOAT) / NULLIF(SUM(SoGhe), 0) * 100 as AvgOccupancy
                        FROM DailyData
                        GROUP BY DayNum
                    ),
                    PeakSlot AS (
                        SELECT 
                            DayNum,
                            TimeSlot,
                            CAST(SUM(TicketsSold) AS FLOAT) / NULLIF(SUM(SoGhe), 0) * 100 as SlotOccupancy,
                            ROW_NUMBER() OVER (PARTITION BY DayNum ORDER BY 
                                CAST(SUM(TicketsSold) AS FLOAT) / NULLIF(SUM(SoGhe), 0) DESC) as rn
                        FROM DailyData
                        GROUP BY DayNum, TimeSlot
                    )
                    SELECT 
                        ds.DayNum,
                        ds.TotalShowtimes,
                        ds.AvgOccupancy,
                        ps.TimeSlot as PeakTimeSlot
                    FROM DayStats ds
                    LEFT JOIN PeakSlot ps ON ds.DayNum = ps.DayNum AND ps.rn = 1
                    ORDER BY ds.DayNum";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    cmd.Parameters.AddWithValue("@DaysBack", daysBack);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int dayNum = Convert.ToInt32(reader["DayNum"]);
                            // SQL Server: 1=Sunday, 2=Monday... .NET: 0=Sunday, 1=Monday...
                            DayOfWeek dow = (DayOfWeek)(dayNum - 1);

                            stats.Add(new DayOfWeekStats
                            {
                                Day = dow,
                                DayName = GetVietnameseDayName(dow),
                                TotalShowtimes = Convert.ToInt32(reader["TotalShowtimes"]),
                                AverageOccupancy = reader["AvgOccupancy"] != DBNull.Value 
                                    ? Convert.ToDouble(reader["AvgOccupancy"]) : 0,
                                PeakTimeSlot = reader["PeakTimeSlot"]?.ToString() ?? "Tối"
                            });
                        }
                    }
                }
            }

            // Fill in missing days with defaults
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek dow = (DayOfWeek)i;
                if (!stats.Exists(s => s.Day == dow))
                {
                    stats.Add(new DayOfWeekStats
                    {
                        Day = dow,
                        DayName = GetVietnameseDayName(dow),
                        TotalShowtimes = 0,
                        AverageOccupancy = GetDefaultOccupancyForDay(dow),
                        PeakTimeSlot = IsWeekend(dow) ? "Chiều" : "Tối"
                    });
                }
            }

            return stats;
        }

        /// <summary>
        /// Phân tích hiệu suất phim theo thể loại
        /// </summary>
        public MoviePerformance GetMoviePerformanceByGenre(int maPhim)
        {
            var perf = new MoviePerformance();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = @"
                    -- Lấy thông tin phim
                    SELECT p.MaPhim, p.TenPhim, p.TheLoai
                    FROM Phim p
                    WHERE p.MaPhim = @MaPhim;

                    -- Phân tích hiệu suất theo thể loại
                    WITH GenreData AS (
                        SELECT 
                            sc.MaSuatChieu,
                            pc.SoGhe,
                            ISNULL(COUNT(v.MaVe), 0) as TicketsSold,
                            CASE 
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 9 AND 11 THEN 'Sáng'
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 12 AND 17 THEN 'Chiều'
                                WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 18 AND 21 THEN 'Tối'
                                ELSE 'Khuya'
                            END as TimeSlot,
                            sc.GioBatDau
                        FROM Phim p
                        INNER JOIN Phim p2 ON p.TheLoai = p2.TheLoai
                        INNER JOIN SuatChieu sc ON p2.MaPhim = sc.MaPhim
                        INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                        LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu 
                            AND v.TrangThai IN ('Đã bán', 'Đã thanh toán')
                        WHERE p.MaPhim = @MaPhim
                            AND sc.TrangThai = 'Đã chiếu'
                        GROUP BY sc.MaSuatChieu, pc.SoGhe, sc.GioBatDau
                    )
                    SELECT 
                        TimeSlot,
                        CAST(SUM(TicketsSold) AS FLOAT) / NULLIF(SUM(SoGhe), 0) * 100 as AvgOccupancy,
                        COUNT(*) as ShowCount
                    FROM GenreData
                    GROUP BY TimeSlot
                    ORDER BY AvgOccupancy DESC";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhim", maPhim);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // First result set: Movie info
                        if (reader.Read())
                        {
                            perf.MaPhim = Convert.ToInt32(reader["MaPhim"]);
                            perf.TenPhim = reader["TenPhim"].ToString();
                            perf.TheLoai = reader["TheLoai"]?.ToString() ?? "Phim";
                        }

                        // Move to second result set: Time slot performance
                        if (reader.NextResult() && reader.Read())
                        {
                            perf.BestTimeSlot = reader["TimeSlot"].ToString();
                            perf.AverageOccupancy = reader["AvgOccupancy"] != DBNull.Value 
                                ? Convert.ToDouble(reader["AvgOccupancy"]) : 50;

                            // Set recommended showtime based on best slot
                            perf.BestShowtime = GetDefaultTimeForSlot(perf.BestTimeSlot);
                        }
                    }
                }
            }

            // Set defaults if no data
            if (string.IsNullOrEmpty(perf.BestTimeSlot))
            {
                perf.BestTimeSlot = "Tối";
                perf.BestShowtime = new TimeSpan(19, 30, 0);
                perf.AverageOccupancy = 65;
            }

            return perf;
        }

        #endregion

        #region Helper Methods

        private List<ShowtimeRecommendation> GenerateRecommendations(
            List<TimeSlotStats> timeSlotStats,
            DayOfWeekStats dayStats,
            MoviePerformance moviePerf,
            List<TimeSpan> existingShowtimes,
            DateTime targetDate)
        {
            var recommendations = new List<ShowtimeRecommendation>();

            // Các khung giờ phổ biến
            var timeSlots = new List<(string Name, TimeSpan[] Times)>
            {
                ("Sáng", new[] { new TimeSpan(9, 30, 0), new TimeSpan(10, 30, 0), new TimeSpan(11, 30, 0) }),
                ("Chiều", new[] { new TimeSpan(14, 0, 0), new TimeSpan(15, 30, 0), new TimeSpan(16, 30, 0) }),
                ("Tối", new[] { new TimeSpan(18, 30, 0), new TimeSpan(19, 30, 0), new TimeSpan(20, 30, 0), new TimeSpan(21, 30, 0) })
            };

            int priority = 1;
            bool isWeekend = IsWeekend(targetDate.DayOfWeek);

            foreach (var slot in timeSlots)
            {
                var slotStats = timeSlotStats.Find(s => s.TimeSlot == slot.Name);
                double baseOccupancy = slotStats?.AverageOccupancy ?? 50;

                // Điều chỉnh theo ngày trong tuần
                if (isWeekend && (slot.Name == "Chiều" || slot.Name == "Tối"))
                {
                    baseOccupancy *= 1.2; // +20% cuối tuần
                }

                // Điều chỉnh theo thể loại phim
                if (moviePerf.BestTimeSlot == slot.Name)
                {
                    baseOccupancy *= 1.15; // +15% nếu khớp thể loại
                }

                foreach (var time in slot.Times)
                {
                    // Kiểm tra xung đột với suất chiếu hiện có
                    bool hasConflict = existingShowtimes.Exists(e => 
                        Math.Abs((e - time).TotalMinutes) < 150); // 2.5 giờ buffer

                    if (hasConflict) continue;

                    double confidence = CalculateConfidence(slotStats, dayStats, isWeekend);
                    string reasoning = GenerateReasoning(slot.Name, baseOccupancy, isWeekend, moviePerf);

                    recommendations.Add(new ShowtimeRecommendation
                    {
                        RecommendedTime = time,
                        TimeSlot = slot.Name,
                        PredictedOccupancy = Math.Min(baseOccupancy, 100),
                        ConfidenceScore = confidence,
                        Reasoning = reasoning,
                        Priority = priority++
                    });
                }
            }

            // Sắp xếp theo mức độ ưu tiên (dự đoán cao nhất)
            recommendations.Sort((a, b) => 
                b.PredictedOccupancy.CompareTo(a.PredictedOccupancy));

            // Cập nhật priority sau khi sort
            for (int i = 0; i < recommendations.Count; i++)
            {
                recommendations[i].Priority = i + 1;
            }

            return recommendations;
        }

        private double CalculateConfidence(TimeSlotStats slotStats, DayOfWeekStats dayStats, bool isWeekend)
        {
            double confidence = 50; // Base

            if (slotStats != null && slotStats.TotalShowtimes >= 10)
            {
                confidence += 20; // Có nhiều dữ liệu
            }

            if (dayStats != null && dayStats.TotalShowtimes >= 5)
            {
                confidence += 15; // Có dữ liệu ngày
            }

            if (isWeekend)
            {
                confidence += 10; // Cuối tuần dễ dự đoán hơn
            }

            return Math.Min(confidence, 95); // Max 95%
        }

        private string GenerateReasoning(string timeSlot, double occupancy, bool isWeekend, MoviePerformance moviePerf)
        {
            var reasons = new List<string>();

            // Khung giờ
            switch (timeSlot)
            {
                case "Sáng":
                    reasons.Add("Khung giờ sáng phù hợp cho gia đình và người lớn tuổi");
                    break;
                case "Chiều":
                    reasons.Add("Khung giờ chiều thu hút sinh viên và giới trẻ");
                    if (isWeekend) reasons.Add("Cuối tuần tăng 20% lượng khách");
                    break;
                case "Tối":
                    reasons.Add("Khung giờ vàng với lượng khách cao nhất");
                    reasons.Add("Phù hợp cho khách đi làm");
                    break;
            }

            // Thể loại phim
            if (moviePerf != null && moviePerf.BestTimeSlot == timeSlot)
            {
                reasons.Add($"Phim {moviePerf.TheLoai} thường bán tốt vào {timeSlot.ToLower()}");
            }

            // Dự đoán
            if (occupancy >= 70)
            {
                reasons.Add($"Dự đoán tỷ lệ lấp đầy cao ({occupancy:F0}%)");
            }

            return string.Join(". ", reasons) + ".";
        }

        private List<TimeSpan> GetExistingShowtimes(int maPhong, DateTime ngayChieu)
        {
            var showtimes = new List<TimeSpan>();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = @"
                    SELECT GioBatDau 
                    FROM SuatChieu 
                    WHERE MaPhong = @MaPhong 
                        AND CONVERT(date, NgayChieu) = @NgayChieu
                        AND TrangThai != 'Đã hủy'";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                    cmd.Parameters.AddWithValue("@NgayChieu", ngayChieu.Date);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["GioBatDau"] != DBNull.Value)
                            {
                                var gioBatDau = reader["GioBatDau"];
                                if (gioBatDau is DateTime dt)
                                {
                                    showtimes.Add(dt.TimeOfDay);
                                }
                                else if (gioBatDau is TimeSpan ts)
                                {
                                    showtimes.Add(ts);
                                }
                            }
                        }
                    }
                }
            }

            return showtimes;
        }

        private List<ShowtimeRecommendation> GetDefaultRecommendations(DateTime targetDate)
        {
            bool isWeekend = IsWeekend(targetDate.DayOfWeek);

            return new List<ShowtimeRecommendation>
            {
                new ShowtimeRecommendation
                {
                    RecommendedTime = new TimeSpan(19, 30, 0),
                    TimeSlot = "Tối",
                    PredictedOccupancy = isWeekend ? 75 : 70,
                    ConfidenceScore = 60,
                    Reasoning = "Khung giờ vàng - phổ biến nhất với khách đi làm.",
                    Priority = 1
                },
                new ShowtimeRecommendation
                {
                    RecommendedTime = new TimeSpan(21, 0, 0),
                    TimeSlot = "Tối",
                    PredictedOccupancy = isWeekend ? 70 : 65,
                    ConfidenceScore = 55,
                    Reasoning = "Khung giờ tối muộn - phù hợp cho giới trẻ.",
                    Priority = 2
                },
                new ShowtimeRecommendation
                {
                    RecommendedTime = new TimeSpan(14, 30, 0),
                    TimeSlot = "Chiều",
                    PredictedOccupancy = isWeekend ? 65 : 45,
                    ConfidenceScore = 50,
                    Reasoning = isWeekend 
                        ? "Chiều cuối tuần thu hút gia đình và sinh viên." 
                        : "Khung giờ chiều - lượng khách trung bình.",
                    Priority = 3
                },
                new ShowtimeRecommendation
                {
                    RecommendedTime = new TimeSpan(10, 0, 0),
                    TimeSlot = "Sáng",
                    PredictedOccupancy = isWeekend ? 40 : 25,
                    ConfidenceScore = 45,
                    Reasoning = "Khung giờ sáng - ít khách, phù hợp cho suất chiếu đặc biệt.",
                    Priority = 4
                }
            };
        }

        private List<TimeSlotStats> GetDefaultTimeSlotStats()
        {
            return new List<TimeSlotStats>
            {
                new TimeSlotStats { TimeSlot = "Sáng", AverageOccupancy = 35, StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("12:00") },
                new TimeSlotStats { TimeSlot = "Chiều", AverageOccupancy = 55, StartTime = TimeSpan.Parse("14:00"), EndTime = TimeSpan.Parse("18:00") },
                new TimeSlotStats { TimeSlot = "Tối", AverageOccupancy = 72, StartTime = TimeSpan.Parse("19:00"), EndTime = TimeSpan.Parse("22:00") }
            };
        }

        private TimeSpan GetDefaultTimeForSlot(string slot)
        {
            switch (slot)
            {
                case "Sáng": return new TimeSpan(10, 0, 0);
                case "Chiều": return new TimeSpan(14, 30, 0);
                case "Tối": return new TimeSpan(19, 30, 0);
                default: return new TimeSpan(19, 30, 0);
            }
        }

        private double GetDefaultOccupancyForDay(DayOfWeek day)
        {
            return IsWeekend(day) ? 70 : 55;
        }

        private bool IsWeekend(DayOfWeek day)
        {
            return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
        }

        private string GetVietnameseDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Thứ Hai";
                case DayOfWeek.Tuesday: return "Thứ Ba";
                case DayOfWeek.Wednesday: return "Thứ Tư";
                case DayOfWeek.Thursday: return "Thứ Năm";
                case DayOfWeek.Friday: return "Thứ Sáu";
                case DayOfWeek.Saturday: return "Thứ Bảy";
                case DayOfWeek.Sunday: return "Chủ Nhật";
                default: return day.ToString();
            }
        }

        #endregion

        #region Analytics Dashboard Methods

        /// <summary>
        /// Lấy tổng quan hiệu suất suất chiếu cho dashboard
        /// </summary>
        public DataTable GetShowtimePerformanceSummary(int maChiNhanh, int daysBack = 30)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = @"
                    SELECT 
                        p.TenPhim,
                        CASE 
                            WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 9 AND 11 THEN 'Sáng'
                            WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 12 AND 17 THEN 'Chiều'
                            WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 18 AND 21 THEN 'Tối'
                            ELSE 'Khuya'
                        END as KhungGio,
                        COUNT(*) as SoSuatChieu,
                        SUM(ISNULL((SELECT COUNT(*) FROM Ve v WHERE v.MaSuatChieu = sc.MaSuatChieu 
                            AND v.TrangThai IN ('Đã bán', 'Đã thanh toán')), 0)) as TongVeBan,
                        SUM(pc.SoGhe) as TongGhe,
                        CAST(SUM(ISNULL((SELECT COUNT(*) FROM Ve v WHERE v.MaSuatChieu = sc.MaSuatChieu 
                            AND v.TrangThai IN ('Đã bán', 'Đã thanh toán')), 0)) AS FLOAT) / 
                            NULLIF(SUM(pc.SoGhe), 0) * 100 as TyLeLapDay
                    FROM SuatChieu sc
                    INNER JOIN Phim p ON sc.MaPhim = p.MaPhim
                    INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                    WHERE pc.MaChiNhanh = @MaChiNhanh
                        AND sc.NgayChieu >= DATEADD(DAY, -@DaysBack, GETDATE())
                        AND sc.TrangThai = 'Đã chiếu'
                    GROUP BY p.TenPhim, 
                        CASE 
                            WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 9 AND 11 THEN 'Sáng'
                            WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 12 AND 17 THEN 'Chiều'
                            WHEN DATEPART(HOUR, sc.GioBatDau) BETWEEN 18 AND 21 THEN 'Tối'
                            ELSE 'Khuya'
                        END
                    ORDER BY TyLeLapDay DESC";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    adapter.SelectCommand.Parameters.AddWithValue("@DaysBack", daysBack);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        /// <summary>
        /// Lấy xu hướng đặt vé theo giờ
        /// </summary>
        public DataTable GetHourlyBookingTrend(int maChiNhanh, int daysBack = 30)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                string query = @"
                    SELECT 
                        DATEPART(HOUR, sc.GioBatDau) as Gio,
                        COUNT(DISTINCT sc.MaSuatChieu) as SoSuatChieu,
                        COUNT(v.MaVe) as SoVeBan,
                        CAST(COUNT(v.MaVe) AS FLOAT) / NULLIF(COUNT(DISTINCT sc.MaSuatChieu), 0) as TrungBinhVe
                    FROM SuatChieu sc
                    INNER JOIN PhongChieu pc ON sc.MaPhong = pc.MaPhong
                    LEFT JOIN Ve v ON sc.MaSuatChieu = v.MaSuatChieu 
                        AND v.TrangThai IN ('Đã bán', 'Đã thanh toán')
                    WHERE pc.MaChiNhanh = @MaChiNhanh
                        AND sc.NgayChieu >= DATEADD(DAY, -@DaysBack, GETDATE())
                    GROUP BY DATEPART(HOUR, sc.GioBatDau)
                    ORDER BY Gio";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@MaChiNhanh", maChiNhanh);
                    adapter.SelectCommand.Parameters.AddWithValue("@DaysBack", daysBack);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        #endregion
    }
}
