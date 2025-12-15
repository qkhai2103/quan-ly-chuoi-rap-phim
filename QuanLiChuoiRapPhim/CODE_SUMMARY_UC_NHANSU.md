# 📝 CODE SUMMARY: UC_NhanSu.cs & ManagerBLL.cs & ManagerDAL.cs

## ✅ ĐÃ HOÀN THÀNH (3 Files)

### 1️⃣ **UC_NhanSu.cs** - UserControl Quản Lý Danh Sách Nhân Viên
**Vị trí**: [GUI/UC_NhanSu.cs](GUI/UC_NhanSu.cs)  
**Dòng code**: ~400 dòng

#### 🎨 Giao Diện (4 Khu Vực):

**Header** (Màu xanh dương - #00AAFF)
- Tiêu đề: "QUẢN LÝ NHÂN VIÊN CHI NHÁNH"

**Filter Panel** (Màu xám #F0F0F0)
- 🔍 Ô Tìm Kiếm: Tìm theo Họ Tên, Email, SĐT
- 📊 Dropdown Lọc Trạng Thái: "Tất cả", "CoHieuLuc", "HetHieuLuc", "TamDung"
- 🔄 Nút Làm Mới (Button)
- ➕ Nút Thêm (Button) - Thông báo thêm qua Admin
- ✏️ Nút Sửa (Button) - Mở form chi tiết
- 🗑️ Nút Xóa (Button) - Thông báo xóa qua Admin

**DataGridView** (Danh Sách Nhân Viên)
- Cột: MaNV | HoTen | Email | SĐT | ViTri | TrangThai | NgayTao
- Cho phép double-click để xem chi tiết
- Chế độ read-only (không chỉnh sửa trực tiếp)
- Hàng xen kẽ màu (alternating rows)

**Footer Panel**
- Hiển thị: "Tổng cộng: X nhân viên"

#### 🔧 Chức Năng Chính:

| Phương Thức | Mô Tả |
|---|---|
| `LoadNhanVien()` | Tải danh sách nhân viên từ ManagerBLL |
| `TxtSearch_TextChanged()` | Tìm kiếm theo Họ Tên, Email, SĐT (real-time) |
| `CboStatus_SelectedIndexChanged()` | Lọc theo trạng thái |
| `BtnRefresh_Click()` | Làm mới dữ liệu |
| `BtnEdit_Click()` | Mở form chi tiết ở chế độ sửa |
| `DgvNhanVien_DoubleClick()` | Mở form chi tiết ở chế độ xem |
| `OpenDetailForm()` | Mở frmNhanVienDetail (TODO) |
| `UpdateFooter()` | Cập nhật thông tin footer |

#### 🔐 Kiểm Tra Quyền:
- Chỉ vai trò **QuanLy** mới có quyền truy cập
- Chỉ xem nhân viên của **chi nhánh của mình**

---

### 2️⃣ **ManagerBLL.cs** - Business Logic Layer
**Vị trí**: [BLL/ManagerBLL.cs](BLL/ManagerBLL.cs)  
**Dòng code**: ~350 dòng

#### 📋 Các Phương Thức (20+ methods):

**Nhân Viên (Employee)**
```csharp
✓ GetBranchEmployees()       - Lấy danh sách nhân viên chi nhánh
✓ GetNhanVienDetail()         - Lấy chi tiết nhân viên
✓ AddNhanVien()               - Thêm NV (chỉ Admin)
✓ UpdateNhanVien()            - Cập nhật NV (Manager)
✓ DeleteNhanVien()            - Xóa NV (chỉ Admin)
```

**Lịch Làm Việc (Shift)**
```csharp
✓ AssignShift()               - Phân công ca làm
✓ CheckShiftConflict()        - Kiểm tra xung đột ca
✓ GetNhanVienSchedule()       - Lấy lịch NV (30 ngày)
✓ GetBranchSchedule()         - Lấy lịch chi nhánh (khoảng thời gian)
```

**Đơn Xin Nghỉ (Leave Request)**
```csharp
✓ GetPendingLeaveRequests()   - Lấy đơn chờ duyệt
✓ ApproveLeaveRequest()       - Duyệt đơn (auto update lịch)
✓ RejectLeaveRequest()        - Từ chối đơn
✓ ValidateLeaveRequest()      - Kiểm tra hợp lệ
✓ GetAvailableReplacements()  - Lấy người thay thế
```

**Hiệu Suất (Performance)**
```csharp
✓ GetPerformanceByMonth()     - Lấy hiệu suất theo tháng
✓ SavePerformanceScore()      - Lưu đánh giá
✓ CalculateAverageScore()     - Tính điểm TB
✓ SubmitPerformanceForApproval() - Gửi phê duyệt
```

**Utility (Tiện Ích)**
```csharp
✓ GetBranchInfo()             - Lấy info chi nhánh
✓ HasPermission()             - Kiểm tra quyền
✓ GetWorkHistory()            - Lấy lịch sử công tác
✓ AddWorkHistory()            - Ghi lại sự kiện
```

#### ✨ Đặc Điểm:
- ✅ Validation dữ liệu
- ✅ Xử lý quyền hạn (Manager chỉ xem chi nhánh mình)
- ✅ Error handling chi tiết
- ✅ Tính toán tự động (điểm trung bình)
- ⏳ TODO: Gọi ManagerDAL (chưa có DB tables)

---

### 3️⃣ **ManagerDAL.cs** - Data Access Layer
**Vị trí**: [DAL/ManagerDAL.cs](DAL/ManagerDAL.cs)  
**Dòng code**: ~450 dòng

#### 🗂️ Các Phương Thức SQL:

**Nhân Viên (SELECT)**
```csharp
✓ GetNhanVienByBranch(branchId)
  → SELECT từ NhanVienChiTiet + NguoiDung
  → WHERE MaChiNhanh = @BranchId

✓ GetNhanVienDetail(nhanVienId)
  → SELECT chi tiết: cơ bản, hợp đồng, tài khoản ngân hàng
  → JOIN với ChiNhanh
```

**Lịch Làm (SELECT)**
```csharp
✓ GetLichLamByNhanVien(nhanVienId, fromDate, toDate)
  → SELECT lịch làm nhân viên trong khoảng thời gian

✓ GetLichLamByBranch(branchId, fromDate, toDate)
  → SELECT lịch làm cả chi nhánh

✓ LichLamExists(nhanVienId, ngayLam)
  → Kiểm tra nhân viên đã có ca làm ngày đó chưa
```

**Đơn Xin Nghỉ (SELECT)**
```csharp
✓ GetDonXinNghiPending(branchId)
  → Lấy đơn TrangThai = 'Cho'

✓ GetDonXinNghiByNhanVien(nhanVienId)
  → Lấy toàn bộ đơn của nhân viên
```

**Hiệu Suất (SELECT)**
```csharp
✓ GetHieuSuatByMonth(branchId, thangNam)
  → Lấy hiệu suất theo tháng 'YYYY-MM'

✓ GetHieuSuatByNhanVien(nhanVienId, year)
  → Lấy lịch sử hiệu suất năm
```

**Lịch Sử (SELECT)**
```csharp
✓ GetLichSuLamViec(nhanVienId)
  → Lấy lịch sử công tác nhân viên
```

#### 🔍 SQL Patterns:
- ✅ Parameterized Queries (chống SQL injection)
- ✅ JOIN tables (NhanVienChiTiet + NguoiDung + ChiNhanh)
- ✅ WHERE filtering
- ✅ ORDER BY sorting
- ✅ Exception handling

#### ⏳ TODO (Cần viết sau):
- `Insert` LichLamViec
- `Update` LichLamViec
- `Delete` LichLamViec
- `ApproveDonXinNghi()` - UPDATE + cập nhật LichLamViec
- `RejectDonXinNghi()` - UPDATE
- `SaveHieuSuat()` - INSERT/UPDATE
- `AddLichSuLamViec()` - INSERT

---

## 📊 KIẾN TRÚC 3-TIER

```
┌─────────────────────────────────┐
│   GUI: UC_NhanSu.cs            │ UserControl (400 dòng)
│   - DataGridView               │ - Tìm kiếm, lọc
│   - Button events              │ - CRUD buttons
└────────────┬────────────────────┘
             │ Gọi
             ▼
┌─────────────────────────────────┐
│   BLL: ManagerBLL.cs           │ Business Logic (350 dòng)
│   - Validation                 │ - Error handling
│   - Permission check           │ - Auto calculation
│   - Business logic             │
└────────────┬────────────────────┘
             │ Gọi
             ▼
┌─────────────────────────────────┐
│   DAL: ManagerDAL.cs           │ Data Access (450 dòng)
│   - SQL Queries                │ - Parameter binding
│   - DataTable result           │ - Exception handling
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│   DATABASE: SQL Server         │
│   - NhanVienChiTiet            │
│   - NguoiDung                  │
│   - ChiNhanh                   │
│   - LichLamViec (TODO)         │
│   - DonXinNghi (TODO)          │
│   - HieuSuat (TODO)            │
└─────────────────────────────────┘
```

---

## 🎯 FLOW HOẠT ĐỘNG

### 1. Khi mở UC_NhanSu:
```
1. Constructor nhận: managerId, branchId, userRole
2. Kiểm tra role == "QuanLy"
3. SetupUI() → Tạo giao diện
4. LoadNhanVien() → Gọi ManagerBLL.GetBranchEmployees()
5. ManagerBLL → Gọi ManagerDAL.GetNhanVienByBranch()
6. ManagerDAL → Query SQL → Trả DataTable
7. Bind DataTable vào DataGridView
```

### 2. Khi tìm kiếm:
```
TextBox_TextChanged
  → Lọc DataTable với LIKE '%keyword%'
  → DataView.RowFilter
  → Bind vào DataGridView
```

### 3. Khi lọc theo trạng thái:
```
ComboBox_SelectedIndexChanged
  → DataView.RowFilter = "TrangThai = 'CoHieuLuc'"
  → Bind vào DataGridView
```

### 4. Khi double-click / Sửa:
```
Lấy MaNhanVien từ row được chọn
  → TODO: Mở frmNhanVienDetail(nhanVienId)
```

---

## 🚀 NEXT STEPS (SẮP LÀMTIẾP)

### ✅ ĐÃ XONG:
- [x] UC_NhanSu.cs + Designer
- [x] ManagerBLL.cs (20+ methods)
- [x] ManagerDAL.cs (SELECT queries)

### ⏳ CẦN LÀM:
1. **SQL Tables** - Chạy lệnh CREATE TABLE (5 bảng)
2. **ManagerDAL** - Viết INSERT/UPDATE/DELETE methods
3. **frmNhanVienDetail.cs** - Form chi tiết 6 tabs
4. **UC_LichLamViec.cs** - Phân công ca làm
5. **UC_DonXinNghi.cs** - Duyệt đơn xin nghỉ
6. **UC_HieuSuat.cs** - Đánh giá hiệu suất
7. **frmMain.cs** - Thêm menu sidebar
8. **Test & Debug** - Toàn bộ chức năng

---

## 💡 LƯU Ý THIẾT KẾ

### 🔐 Bảo Mật:
- Tất cả queries dùng **SqlParameter** (chống SQL injection)
- Kiểm tra **HasPermission()** trước mỗi thao tác
- Manager chỉ xem dữ liệu **chi nhánh của mình**

### 🎨 UI/UX:
- Màu sắc nhất quán: Xanh dương (#00AAFF), Xanh lá (#28A745), Vàng (#FFC107), Đỏ (#DC3545)
- Button icons: ➕ ✏️ 🗑️ 🔄 🔍
- Alternating row colors cho dễ đọc
- Double-click để xem chi tiết

### 🔄 Data Flow:
- GUI → BLL (validation) → DAL (SQL) → Database
- Trả về **DataTable** cho binding
- Exception handling ở mọi tầng

### ⚠️ Kiểm Tra Lỗi:
- Không có data → Hiển thị message
- SQL error → Hiển thị lỗi rõ ràng
- Permission denied → Thông báo không quyền

---

## 📈 THỐNG KÊ CODE

| File | Dòng | Phương Thức | Chức Năng |
|---|---|---|---|
| UC_NhanSu.cs | ~400 | 12 | UI + Events |
| ManagerBLL.cs | ~350 | 20+ | Business Logic |
| ManagerDAL.cs | ~450 | 12 | SQL Queries |
| **Tổng** | **~1200** | **45+** | |

---

## 🔗 Liên Kết File

- [UC_NhanSu.cs](GUI/UC_NhanSu.cs) - UserControl quản lý nhân viên
- [UC_NhanSu.Designer.cs](GUI/UC_NhanSu.Designer.cs) - Designer (empty)
- [ManagerBLL.cs](BLL/ManagerBLL.cs) - Logic layer
- [ManagerDAL.cs](DAL/ManagerDAL.cs) - Data access layer
- [KE_HOACH_QUAN_LY_NHAN_SU.md](KE_HOACH_QUAN_LY_NHAN_SU.md) - Full plan with SQL

---

**Created**: 2024-12-13  
**Status**: ✅ UC_NhanSu + BLL + DAL hoàn thành, chờ SQL tables + frmDetail
