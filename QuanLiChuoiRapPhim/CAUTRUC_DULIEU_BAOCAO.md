# Cấu Trúc Dữ Liệu và Nguồn Dữ Liệu UC_BaoCaoChiNhanh

## 📊 Tổng Quan

UC_BaoCaoChiNhanh lấy dữ liệu từ **ReportDAL.cs** qua các stored procedure hoặc direct SQL queries. Dữ liệu sau đó được hiển thị trong TabControl với 5 tabs báo cáo khác nhau.

---

## 🗄️ Stored Procedures & Queries

### 1. **Doanh Thu Theo Ngày**
**Method**: `GetRevenueByDate(DateTime fromDate, DateTime toDate)`  
**Stored Procedure**: `sp_ThongKeDoanhThuTheoNgay`  
**Parameters**:
- `@TuNgay` - Ngày bắt đầu
- `@DenNgay` - Ngày kết thúc

**Return Fields**:
```
NgayThongKe | DoanhThuNgay | SoHoaDon | SoVeBan | SoSanPhamBan
```

**Sử Dụng**: Tab "DOANH THU"

---

### 2. **Phim Bán Chạy**
**Method**: `GetTopMovies(DateTime fromDate, DateTime toDate, int top = 10)`  
**Stored Procedure**: `sp_ThongKePhimBanChay`  
**Parameters**:
- `@Top` - Số phim top (mặc định 10)

**Return Fields**:
```
STT | TenPhim | SoVeBan | DoanhThu | TyLeDoang | NamPhatHanh | DienVien
```

**Sử Dụng**: Tab "PHIM"

---

### 3. **Doanh Thu Theo Chi Nhánh**
**Method**: `GetRevenueByBranch(DateTime fromDate, DateTime toDate)`  
**Stored Procedure**: `sp_BaoCaoDoanhThuChiNhanh`  
**Parameters**:
- `@TuNgay` - Ngày bắt đầu
- `@DenNgay` - Ngày kết thúc

**Return Fields**:
```
MaChiNhanh | TenChiNhanh | DoanhThuThang | SoHoaDon | SoNhanVien | TyLeThanhCong
```

**Sử Dụng**: Tab "TỔNG QUAN", Panel Nổi Bật

---

### 4. **Vé Bán Theo Suất Chiếu**
**Method**: `GetTicketSales(DateTime fromDate, DateTime toDate, string branchName = null)`  
**Query Type**: Direct SQL  
**Parameters**:
- `@FromDate` - Ngày bắt đầu
- `@ToDate` - Ngày kết thúc
- `@BranchName` - Tên chi nhánh (nullable)

**Return Fields**:
```
TenPhim | SoVeBan | DoanhThuVe | NgayChieu | GioChieu | TenChiNhanh | TenPhong
```

**Sử Dụng**: Tab "TỔNG QUAN" - DataGridView

---

### 5. **Sản Phẩm Bán Chạy**
**Method**: Được implement trong UC_BaoCaoChiNhanh  
**Query Type**: Cần implement hoặc fetch từ DAL

**Return Fields** (dự kiến):
```
MaSanPham | TenSanPham | SoLuongBan | DoanhThu | TyLeLaiBuan | TonKho
```

**Sử Dụng**: Tab "SẢN PHẨM"

---

### 6. **Hiệu Suất Nhân Viên**
**Method**: Được implement trong UC_BaoCaoChiNhanh  
**Query Type**: Cần implement hoặc fetch từ DAL

**Return Fields** (dự kiến):
```
MaNhanVien | TenNhanVien | SoHoaDon | DoanhThu | SoPhutLamViec | DanhGia
```

**Sử Dụng**: Tab "NHÂN VIÊN"

---

## 🔗 Mối Quan Hệ Bảng

```
┌──────────────┐
│   CHINHANH   │
│──────────────│
│ MaChiNhanh   │◄─────────┐
│ TenChiNhanh  │          │
│ DiaChi       │          │
│ TrangThai    │          │
└──────────────┘          │
        ▲                  │
        │                  │
┌───────┴───────┐  ┌──────┴──────────┐
│   PHONGCHIEU  │  │   NHANVIEN      │
│───────────────│  │──────────────────│
│ MaPhong       │  │ MaNhanVien       │
│ TenPhong      │  │ TenNhanVien      │
│ MaChiNhanh    │  │ MaChiNhanh       │
└─────┬─────────┘  │ ChucVu           │
      │            │ TrangThai        │
      │            └──────────────────┘
      │                    │
      │            ┌───────┴────────┐
┌─────┴─────────┐  │   PHIEUBAOCAO  │
│  SUATCHIEU    │  │────────────────│
│───────────────│  │ MaPhieuBaoCao  │
│ MaSuatChieu   │  │ MaNhanVien      │
│ MaPhim        │  │ NoiDung        │
│ MaPhong       │  │ NgayTao        │
│ NgayChieu     │  └────────────────┘
│ GioChieu      │
└─────┬─────────┘
      │
┌─────┴──────────┐
│      VE        │
│────────────────│
│ MaVe           │
│ MaSuatChieu    │
│ SoGhe          │
│ GiaVe          │
│ TrangThai      │
│ NgayDat        │
└────────────────┘
      │
      └────────────────┐
                       │
               ┌───────┴────────┐
               │    PHIM        │
               │────────────────│
               │ MaPhim         │
               │ TenPhim        │
               │ DienVien       │
               │ NamPhatHanh    │
               │ DanhGia        │
               └────────────────┘
```

---

## 📈 Dữ Liệu Thống Kê (Quick Stats)

### Tổng Doanh Thu
```sql
SELECT SUM(v.GiaVe) as TongDoanhThu
FROM Ve v
WHERE v.TrangThai = 'DaBan'
  AND v.NgayDat BETWEEN @TuNgay AND @DenNgay
```

### Tổng Hóa Đơn
```sql
SELECT COUNT(DISTINCT hd.MaHoaDon) as TongHoaDon
FROM HoaDon hd
WHERE hd.NgayTao BETWEEN @TuNgay AND @DenNgay
```

### Khách Hàng TB
```sql
SELECT COUNT(DISTINCT v.MaKhachHang) / 
       DATEDIFF(day, @TuNgay, @DenNgay) as KhachHangTB
FROM Ve v
WHERE v.NgayDat BETWEEN @TuNgay AND @DenNgay
```

### Doanh Thu TB
```sql
SELECT SUM(v.GiaVe) / DATEDIFF(day, @TuNgay, @DenNgay) as DoanhThuTB
FROM Ve v
WHERE v.TrangThai = 'DaBan'
  AND v.NgayDat BETWEEN @TuNgay AND @DenNgay
```

---

## 📂 Layer Architecture

```
┌─────────────────────────┐
│     UI Layer (GUI)      │
│  frmMain.cs             │
│  ↓                      │
│  UC_BaoCaoChiNhanh.cs   │ ← TabControl, Charts, DataGridViews
└────────┬────────────────┘
         │
         │ (Calls Methods)
         ↓
┌────────────────────────────────┐
│   Business Logic Layer (BLL)   │
│   ReportBLL.cs                 │
│   ├─ ValidateReportParams()    │
│   ├─ CalculateStats()          │
│   └─ FormatChartData()         │
└────────┬─────────────────────────┘
         │
         │ (Calls Methods)
         ↓
┌────────────────────────────────┐
│  Data Access Layer (DAL)       │
│  ReportDAL.cs                  │
│  ├─ GetRevenueByDate()         │
│  ├─ GetTopMovies()             │
│  ├─ GetRevenueByBranch()       │
│  └─ GetTicketSales()           │
└────────┬─────────────────────────┘
         │
         │ (Executes SQL)
         ↓
┌────────────────────────────────┐
│   Database Layer (SQL Server)  │
│   Stored Procedures            │
│   └─ sp_ThongKeDoanhThuTheoNgay│
│   └─ sp_ThongKePhimBanChay     │
│   └─ sp_BaoCaoDoanhThuChiNhanh │
└────────────────────────────────┘
```

---

## 🔄 Luồng Dữ Liệu Chi Tiết

### Khởi Tạo (Load Mặc Định)

```
1. UC_BaoCaoChiNhanh Constructor
   ├─ ThietLapGiaoDien() - Create UI
   └─ TaiBaoCaoMacDinh() - Load default data
   
2. TaiBaoCaoMacDinh()
   ├─ Call ReportDAL.GetRevenueByDate()
   │  └─ Execute sp_ThongKeDoanhThuTheoNgay
   │  └─ Return DataTable
   │  └─ Bind to dgvTongQuan
   │
   ├─ Call ReportDAL.GetTopMovies()
   │  └─ Execute sp_ThongKePhimBanChay
   │  └─ Return DataTable
   │  └─ Bind to dgvPhim & chartPhim
   │
   ├─ Call ReportDAL.GetRevenueByBranch()
   │  └─ Execute sp_BaoCaoDoanhThuChiNhanh
   │  └─ Return DataTable
   │  └─ Update Quick Stats Labels
   │
   ├─ Create Charts
   │  ├─ chartDoanhThu (Line/Column)
   │  ├─ chartSanPham (Pie/Bar)
   │  ├─ chartPhim (Bar/Column)
   │  └─ chartNhanVien (Gauge/Bar)
   │
   └─ Populate DataGridViews
      ├─ dgvTongQuan
      ├─ dgvDoanhThu
      ├─ dgvSanPham
      ├─ dgvPhim
      └─ dgvNhanVien
```

### Refresh Dữ Liệu (Filter)

```
1. User Click "NÚT TẠO BÁO CÁO"
   ├─ Get DateTimePicker values
   ├─ Get ComboBox selection
   └─ Call CboLoaiBaoCao_SelectedIndexChanged()

2. CboLoaiBaoCao_SelectedIndexChanged()
   ├─ Based on selected report type
   ├─ Call appropriate DAL methods
   │  ├─ RevenueByDate
   │  ├─ TopMovies
   │  ├─ RevenueByBranch
   │  └─ TicketSales
   │
   └─ Update UI Components
      ├─ Refresh DataGridViews
      ├─ Redraw Charts
      └─ Update Stats Labels
```

---

## 💾 Caching Strategy

- **Không có caching** - Dữ liệu luôn được load từ database
- **Performance**: Dữ liệu được load async hoặc sync tùy theo size
- **Updates**: Dữ liệu được refresh khi user click button hoặc thay đổi filter

---

## 🔐 SQL Injection Prevention

- ✅ Sử dụng Stored Procedures
- ✅ Sử dụng SqlParameter với @ParameterName
- ✅ Không concat string vào query

---

## 📋 Các Tab Sử Dụng Dữ Liệu Nào

| Tab | DataTable | Chart | DataGridView | Quick Stats |
|-----|-----------|-------|--------------|-------------|
| **TỔNG QUAN** | GetRevenueByDate | - | dgvTongQuan | ✓ (4 labels) |
| **DOANH THU** | GetRevenueByDate | chartDoanhThu | dgvDoanhThu | - |
| **SẢN PHẨM** | GetTopProducts | chartSanPham | dgvSanPham | - |
| **PHIM** | GetTopMovies | chartPhim | dgvPhim | - |
| **NHÂN VIÊN** | GetEmployeePerformance | chartNhanVien | dgvNhanVien | - |

---

## 🧪 Test Data

Để test, bạn có thể:

1. Insert test data vào bảng Ve, HoaDon, SanPham
2. Chạy stored procedures trực tiếp trong SQL Server Management Studio
3. Xem kết quả trước khi test UI

Ví dụ:
```sql
-- Test GetRevenueByDate
EXEC sp_ThongKeDoanhThuTheoNgay 
    @TuNgay = '2025-01-01',
    @DenNgay = '2025-01-31'
```

---

## 📝 Lưu Ý

- Nếu không có dữ liệu, các tab vẫn hiển thị nhưng DataGridViews/Charts sẽ trống
- Các stored procedures phải tồn tại trong database
- DatabaseConfig.ConnectionString phải cấu hình đúng
- Nếu connection fail, exception sẽ được catch trong frmMain.LoadReports()

---

**Ngày tạo**: 2025-12-15  
**Tình trạng**: ✅ Hoàn tất
