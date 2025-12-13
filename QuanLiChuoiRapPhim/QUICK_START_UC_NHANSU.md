# 🚀 QUICK START: UC_NhanSu Implementation

## 📦 Files Created

```
✅ GUI/UC_NhanSu.cs           (400 lines)   - Main UserControl
✅ GUI/UC_NhanSu.Designer.cs  (30 lines)    - Designer boilerplate
✅ BLL/ManagerBLL.cs          (350 lines)   - Business Logic (20+ methods)
✅ DAL/ManagerDAL.cs          (450 lines)   - Data Access (12 SQL queries)
```

## 🎨 UI Layout

```
╔════════════════════════════════════════════════════════════════════╗
║  QUẢN LÝ NHÂN VIÊN CHI NHÁNH                                       ║
╠════════════════════════════════════════════════════════════════════╣
║  🔍 Tìm kiếm: [_____]  📊 Trạng thái: [Tất cả▼]  🔄 🟢➕ 🟡✏️ 🔴🗑️  ║
╠════════════════════════════════════════════════════════════════════╣
║  ID │ Họ Tên        │ Email          │ SĐT    │ Vị Trí      │ TT  ║
╠════════════════════════════════════════════════════════════════════╣
║  1  │ Nguyễn A      │ a@mail.com     │ 123456 │ BanVe       │ OK  ║
║  2  │ Trần B        │ b@mail.com     │ 234567 │ SoatVe      │ OK  ║
║  3  │ Lê C          │ c@mail.com     │ 345678 │ QuayBapNuoc  │ Tạm ║
╠════════════════════════════════════════════════════════════════════╣
║  Tổng cộng: 3 nhân viên                                            ║
╚════════════════════════════════════════════════════════════════════╝
```

## 🔧 Core Methods

### UC_NhanSu.cs

```csharp
// Constructor
public UC_NhanSu(int managerId, int branchId, string userRole)

// Load data
private void LoadNhanVien()
private void UpdateFooter(string message)

// Search & Filter
private void TxtSearch_TextChanged(object sender, EventArgs e)
private void CboStatus_SelectedIndexChanged(object sender, EventArgs e)

// Button events
private void BtnRefresh_Click(object sender, EventArgs e)
private void BtnAdd_Click(object sender, EventArgs e)
private void BtnEdit_Click(object sender, EventArgs e)
private void BtnDelete_Click(object sender, EventArgs e)

// Detail view
private void DgvNhanVien_DoubleClick(object sender, EventArgs e)
private void OpenDetailForm(int nhanVienId, bool isEdit)
```

### ManagerBLL.cs

```csharp
// Employee Management
GetBranchEmployees(managerId, branchId)
GetNhanVienDetail(nhanVienId)
UpdateNhanVien(nhanVienInfo, out errorMessage)

// Shift Management
AssignShift(nhanVienId, ngayLam, ca, viTri, out errorMessage)
CheckShiftConflict(nhanVienId, ngayLam)
GetNhanVienSchedule(nhanVienId, days = 30)

// Leave Request
GetPendingLeaveRequests(branchId)
ApproveLeaveRequest(donId, managerId, ghiChu, out errorMessage)
ValidateLeaveRequest(nhanVienId, ngayBatDau, ngayKetThuc, out errorMessage)

// Performance
GetPerformanceByMonth(branchId, thangNam)
SavePerformanceScore(hieuSuatInfo, out errorMessage)
CalculateAverageScore(thaiDo, nangSuat, kyLuat)

// Utility
HasPermission(managerId, branchId)
GetWorkHistory(nhanVienId)
```

### ManagerDAL.cs

```csharp
// Employee Queries
GetNhanVienByBranch(branchId)
GetNhanVienDetail(nhanVienId)

// Shift Queries
GetLichLamByNhanVien(nhanVienId, fromDate, toDate)
GetLichLamByBranch(branchId, fromDate, toDate)
LichLamExists(nhanVienId, ngayLam)

// Leave Request Queries
GetDonXinNghiPending(branchId)
GetDonXinNghiByNhanVien(nhanVienId)

// Performance Queries
GetHieuSuatByMonth(branchId, thangNam)
GetHieuSuatByNhanVien(nhanVienId, year)

// History Queries
GetLichSuLamViec(nhanVienId)
```

## 🎯 Usage Example

```csharp
// In frmMain or wherever you want to use UC_NhanSu:

int managerId = 1;      // Current user's ID
int branchId = 5;       // Manager's branch
string userRole = "QuanLy";

UC_NhanSu ucNhanSu = new UC_NhanSu(managerId, branchId, userRole);
_mainContentPanel.Controls.Clear();
_mainContentPanel.Controls.Add(ucNhanSu);
```

## ⚙️ How It Works

### Step 1: Load Data
```
UC_NhanSu Constructor
  ↓
SetupUI() creates layout
  ↓
LoadNhanVien()
  ↓
ManagerBLL.GetBranchEmployees(managerId, branchId)
  ↓
ManagerDAL.GetNhanVienByBranch(branchId)
  ↓
SQL Query → DataTable
  ↓
Bind to DataGridView
```

### Step 2: Search
```
User types in TextBox
  ↓
TxtSearch_TextChanged()
  ↓
DataView.RowFilter with LIKE
  ↓
Update DataGridView
```

### Step 3: Filter by Status
```
User selects from ComboBox
  ↓
CboStatus_SelectedIndexChanged()
  ↓
DataView.RowFilter with WHERE
  ↓
Update DataGridView
```

### Step 4: Open Detail
```
User double-clicks row OR clicks Edit
  ↓
Get selected MaNhanVien
  ↓
OpenDetailForm(nhanVienId, isEdit: true/false)
  ↓
TODO: Open frmNhanVienDetail (next step)
```

## 🔐 Permission Check

```csharp
// Automatic in LoadNhanVien()
if (_currentRole != "QuanLy")
{
    MessageBox.Show("Bạn không có quyền!", "...");
    return;
}

// Also check in ManagerBLL.HasPermission()
if (!HasPermission(managerId, branchId))
{
    // Deny access
}
```

## 📊 Data Flow

```
Database (5 tables needed)
    ↑↓
ManagerDAL.cs (SQL queries)
    ↑↓
ManagerBLL.cs (Business logic)
    ↑↓
UC_NhanSu.cs (UI binding)
    ↑↓
User (see & interact)
```

## 🚨 Important Notes

1. **Database Tables**: Need to create 5 tables:
   - NhanVienChiTiet
   - LichLamViec
   - DonXinNghi
   - HieuSuat
   - LichSuLamViec

2. **Connection String**: Uses `DatabaseConfig.ConnectionString` from existing code

3. **Error Handling**: All methods wrap in try-catch and return meaningful messages

4. **Parameterized Queries**: All DAL queries use `@Parameter` syntax to prevent SQL injection

5. **Permission Model**: Manager can only see employees of their assigned branch

## ✅ Testing Checklist

```
□ UI displays correctly with all buttons
□ Search works (type name, see results update)
□ Filter works (select status, see results update)
□ Refresh clears search and reloads data
□ Double-click opens detail form (TODO)
□ Permission check works (non-managers denied)
□ Error messages display properly
□ Footer updates with count
□ No SQL injection vulnerability
```

## 📋 What's Next?

### Immediate:
1. Create SQL tables (copy from KE_HOACH_QUAN_LY_NHAN_SU.md)
2. Add INSERT/UPDATE/DELETE methods to ManagerDAL.cs

### Soon:
3. Create frmNhanVienDetail.cs (6 tabs form)
4. Create UC_LichLamViec.cs (shift assignment)
5. Create UC_DonXinNghi.cs (leave approval)
6. Create UC_HieuSuat.cs (performance)
7. Update frmMain.cs sidebar menu

### Finally:
8. Integration testing
9. User acceptance testing
10. Production deployment

## 🔗 Related Files

- Plan: [KE_HOACH_QUAN_LY_NHAN_SU.md](KE_HOACH_QUAN_LY_NHAN_SU.md)
- Summary: [CODE_SUMMARY_UC_NHANSU.md](CODE_SUMMARY_UC_NHANSU.md)
- Main Form: [GUI/frmMain.cs](GUI/frmMain.cs)
- Admin UC: [GUI/UC_Admin.cs](GUI/UC_Admin.cs) (reference)

---

**Ready to build?** 🚀

Choose your next task:
1. Create SQL tables
2. Code frmNhanVienDetail.cs
3. Code UC_LichLamViec.cs
4. Integrate into frmMain sidebar
