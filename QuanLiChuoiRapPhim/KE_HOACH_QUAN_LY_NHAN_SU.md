# KỈ HOẠCH CODE: QUẢN LÝ NHÂN SỰ & CA LÀM

## 📋 TỔNG QUAN

Dự án: Quản Lý Chuỗi Rạp Phim (QuanLiChuoiRapPhim)
Mô đun: Quản Lý Nhân Sự & Ca Làm (cho vai trò Manager)
Phạm vi: Quản lý nhân viên trong chi nhánh/cụm rạp của mình

---

## 🗂️ CẤU TRÚC DỰ ÁN HIỆN TẠI

### Stack Công Nghệ
- **Language**: C# (.NET Framework)
- **Database**: SQL Server (site4now.net)
- **UI**: Windows Forms
- **Architecture**: 3-Tier (GUI - BLL - DAL)

### Cấu Trúc Thư Mục
```
QuanLiChuoiRapPhim/
├── GUI/
│   ├── frmLogin.cs                 # Form đăng nhập
│   ├── frmMain.cs                  # Form chính (với sidebar)
│   ├── UC_Admin.cs                 # Quản lý người dùng
│   ├── UC_Movies.cs                # Quản lý phim
│   ├── UC_NhanSu.cs                # ⚠️ TẠO MỚI - Quản lý nhân sự
│   ├── UC_Reports.cs               # Báo cáo
│   └── frmUserDetail.cs            # Chi tiết người dùng
│
├── BLL/
│   ├── AdminBLL.cs                 # Logic Admin
│   ├── UserBLL.cs                  # Logic User chung
│   ├── ReportBLL.cs                # Logic Báo cáo
│   └── ManagerBLL.cs               # ⚠️ TẠO MỚI - Logic quản lý nhân sự
│
├── DAL/
│   ├── DatabaseConfig.cs           # Cấu hình kết nối DB
│   ├── AdminDAL.cs                 # Truy vấn Admin
│   ├── UserDAL.cs                  # Truy vấn User chung
│   ├── ReportDAL.cs                # Truy vấn Báo cáo
│   └── ManagerDAL.cs               # ⚠️ TẠO MỚI - Truy vấn quản lý nhân sự
│
└── Properties/
    └── AssemblyInfo.cs
```

---

## 📊 THIẾT KẾ DATABASE

### Bảng Mở Rộng Cần Tạo

#### 1. **NhanVienChiTiet** - Thông tin chi tiết nhân viên
```sql
CREATE TABLE NhanVienChiTiet (
    MaNhanVien INT PRIMARY KEY IDENTITY(1,1),
    MaNguoiDung INT UNIQUE NOT NULL,  -- FK từ NguoiDung
    MaChiNhanh INT NOT NULL,           -- FK từ ChiNhanh
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),             -- 'Nam' / 'Nữ'
    QueQuan NVARCHAR(500),
    NoiCap NVARCHAR(100),
    NoiCapCMND NVARCHAR(100),
    SoTaiKhoanNH NVARCHAR(50),
    NganHang NVARCHAR(100),
    HoTenThuHuong NVARCHAR(100),
    HopDongLamViec NVARCHAR(500),      -- Đường dẫn file
    NgayKyHopDong DATE,
    NgayHetHopDong DATE,
    TrangThai NVARCHAR(50),            -- 'CoHieuLuc' / 'HetHieuLuc' / 'TamDung'
    GhiChu NVARCHAR(500),
    NgayTao DATETIME DEFAULT GETDATE(),
    NguoiTao INT,
    NgaySua DATETIME,
    NguoiSua INT,
    
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh)
);
```

#### 2. **LichLamViec** - Lịch làm việc của nhân viên
```sql
CREATE TABLE LichLamViec (
    MaLichLam INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT NOT NULL,
    MaChiNhanh INT NOT NULL,
    NgayLam DATE NOT NULL,
    Ca NVARCHAR(50),                   -- 'SangP', 'ChieuP', 'ToiP', 'CaHanhTrinh'
    GioVao TIME,
    GioRa TIME,
    ViTri NVARCHAR(100),               -- 'BanVe' / 'SoatVe' / 'QuayBapNuoc' / 'Khac'
    TrangThai NVARCHAR(50),            -- 'DuKienLam' / 'XacNhan' / 'HuyCa' / 'OffDay'
    GhiChu NVARCHAR(500),
    NgayTao DATETIME DEFAULT GETDATE(),
    NguoiTao INT,
    NgaySua DATETIME,
    NguoiSua INT,
    
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVienChiTiet(MaNhanVien),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh),
    UNIQUE (MaNhanVien, NgayLam)
);
```

#### 3. **DonXinNghi** - Đơn xin nghỉ phép
```sql
CREATE TABLE DonXinNghi (
    MaDonXinNghi INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT NOT NULL,
    MaChiNhanh INT NOT NULL,
    LoaiNghi NVARCHAR(50),             -- 'Phep' / 'Khong' / 'Hy' / 'OmBenh' / 'KhongPhepODai'
    NgayBatDau DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    SoNgay INT,
    LyDo NVARCHAR(500),
    DiaDiem NVARCHAR(500),
    NguoiThayThe INT,                  -- FK MaNhanVien thay thế
    TrangThai NVARCHAR(50),            -- 'Cho' / 'DuaThao' / 'DuocPheDuyet' / 'TuChoi'
    NguoiPD INT,                       -- FK người phê duyệt (Quản Lý)
    GhiChuPD NVARCHAR(500),            -- Ghi chú phê duyệt
    NgayGui DATETIME DEFAULT GETDATE(),
    NgayPD DATETIME,
    
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVienChiTiet(MaNhanVien),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh),
    FOREIGN KEY (NguoiThayThe) REFERENCES NhanVienChiTiet(MaNhanVien),
    FOREIGN KEY (NguoiPD) REFERENCES NguoiDung(MaNguoiDung)
);
```

#### 4. **HieuSuat** - Đánh giá hiệu suất nhân viên
```sql
CREATE TABLE HieuSuat (
    MaHieuSuat INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT NOT NULL,
    MaChiNhanh INT NOT NULL,
    ThangNam VARCHAR(7),               -- 'YYYY-MM' ví dụ '2024-12'
    DiemThaiDo FLOAT,                  -- 0-10
    DiemNangSuat FLOAT,                -- 0-10
    DiemKyLuat FLOAT,                  -- 0-10
    DiemTrungBinh FLOAT,               -- Trung bình 3 điểm trên
    DanhGia NVARCHAR(1000),            -- Nhận xét chi tiết
    KeNghiThuong NVARCHAR(500),
    KeNghiPhat NVARCHAR(500),
    HanhDong NVARCHAR(500),            -- 'Thuong' / 'Phat' / 'DaoTao' / 'Khong'
    TrangThai NVARCHAR(50),            -- 'Nhap' / 'DaGui' / 'DuocPheDuyet'
    NguoiDanhGia INT,                  -- FK Quản Lý
    NgayDanhGia DATETIME DEFAULT GETDATE(),
    NguoiPheDuyet INT,                 -- FK Admin
    NgayPheDuyet DATETIME,
    
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVienChiTiet(MaNhanVien),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh),
    FOREIGN KEY (NguoiDanhGia) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (NguoiPheDuyet) REFERENCES NguoiDung(MaNguoiDung),
    UNIQUE (MaNhanVien, ThangNam)
);
```

#### 5. **LichSuLamViec** - Lịch sử công tác của nhân viên
```sql
CREATE TABLE LichSuLamViec (
    MaLichSu INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT NOT NULL,
    MaChiNhanh INT NOT NULL,
    HanhDong NVARCHAR(500),            -- 'TuyenDung' / 'DieuDo' / 'HuongDan' / 'ThoiViec'
    NgayHanhDong DATE NOT NULL,
    GhiChu NVARCHAR(500),
    NgayTao DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVienChiTiet(MaNhanVien),
    FOREIGN KEY (MaChiNhanh) REFERENCES ChiNhanh(MaChiNhanh)
);
```

---

## 🔄 FLOW CHỨC NĂNG

### 1️⃣ **Xem Danh Sách Nhân Viên Chi Nhánh**
```
[Manager] → [UC_NhanSu] 
→ Load list từ NhanVienChiTiet + NguoiDung (Chi nhánh của Manager)
→ DataGridView: MaNV, HoTen, Email, SĐT, ViTri, TrangThai
→ Tìm kiếm, Lọc theo trạng thái
→ [Xem Chi Tiết] → [frmNhanVienDetail]
```

### 2️⃣ **Phân Công Ca Làm Việc**
```
[Manager] → [UC_LichLamViec]
→ Calendar view hoặc table theo ngày
→ Chọn nhân viên & ngày
→ Chọn Ca (Sáng/Chiều/Tối) & Vị trí
→ Save → LichLamViec
→ Nhân viên có thể xem lịch (read-only)
```

### 3️⃣ **Duyệt Đơn Xin Nghỉ**
```
[Manager] → [UC_DonXinNghi]
→ Danh sách đơn: Trạng thái = 'Cho'
→ [Chi Tiết Đơn]:
   - Thông tin nhân viên
   - Loại nghỉ, Ngày, Lý do
   - Chọn người thay thế
→ [Duyệt] → Cập nhật TrangThai = 'DuocPheDuyet' + tự động điều chỉnh LichLamViec
→ [Từ Chối] → GhiChuPD & gửi thông báo
```

### 4️⃣ **Theo Dõi Hiệu Suất Nhân Viên**
```
[Manager] → [UC_HieuSuat]
→ Chọn tháng & nhân viên
→ Nhập/Sửa điểm: Thái độ, Năng suất, Kỷ luật (0-10)
→ Nhập nhận xét chi tiết
→ Chọn hành động: Thưởng/Phạt/Đào tạo
→ [Gửi phê duyệt] → Admin duyệt
→ Lưu vào HieuSuat + LichSuLamViec
```

---

## 💻 CODE STRUCTURE

### **Tầng DAL: ManagerDAL.cs**
```csharp
namespace QuanLiChuoiRapPhim.DAL
{
    internal class ManagerDAL
    {
        // 1. Nhân Viên
        - GetNhanVienByBranch(int branchId)
        - GetNhanVienDetail(int nhanVienId)
        - AddNhanVien(NhanVienDTO)
        - UpdateNhanVien(NhanVienDTO)
        - DeleteNhanVien(int nhanVienId)
        - SearchNhanVien(string keyword, int branchId)

        // 2. Lịch Làm Việc
        - GetLichLamByBranch(DateTime fromDate, DateTime toDate, int branchId)
        - GetLichLamByNhanVien(int nhanVienId, DateTime fromDate, DateTime toDate)
        - AddLichLam(LichLamDTO)
        - UpdateLichLam(LichLamDTO)
        - DeleteLichLam(int lichLamId)

        // 3. Đơn Xin Nghỉ
        - GetDonXinNghiPending(int branchId)
        - GetDonXinNghiByNhanVien(int nhanVienId)
        - ApproveDonXinNghi(int donId, int managerId, string ghiChu)
        - RejectDonXinNghi(int donId, int managerId, string lyDo)

        // 4. Hiệu Suất
        - GetHieuSuatByMonth(int branchId, string thangNam)
        - GetHieuSuatByNhanVien(int nhanVienId, int year)
        - SaveHieuSuat(HieuSuatDTO)
        - SubmitHieuSuatForApproval(int hieuSuatId)

        // 5. Lịch Sử
        - GetLichSuLamViec(int nhanVienId)
        - AddLichSuLamViec(LichSuLamViecDTO)
    }
}
```

### **Tầng BLL: ManagerBLL.cs**
```csharp
namespace QuanLiChuoiRapPhim.BLL
{
    internal class ManagerBLL
    {
        private ManagerDAL _managerDAL = new ManagerDAL();
        
        // 1. Nhân Viên - CRUD & Logic
        public DataTable GetBranchEmployees(int managerId, int branchId)
        public bool AddEmployee(EmployeeInfo info, out string error)
        public bool UpdateEmployee(EmployeeInfo info, out string error)
        public bool DeleteEmployee(int employeeId, out string error)
        public DataRow GetEmployeeDetail(int employeeId)
        
        // 2. Lịch Làm Việc - Phân Công
        public DataTable GetBranchSchedule(int branchId, DateTime from, DateTime to)
        public bool AssignShift(int employeeId, DateTime date, string shift, out string error)
        public bool CheckShiftConflict(int employeeId, DateTime date)
        public DataTable GetEmployeeSchedule(int employeeId, int days = 30)
        
        // 3. Đơn Xin Nghỉ - Duyệt
        public DataTable GetPendingLeaveRequests(int branchId)
        public bool ApproveLeaveRequest(int requestId, int managerId, string notes)
        public bool RejectLeaveRequest(int requestId, int managerId, string reason)
        public bool ValidateLeaveRequest(int employeeId, DateTime from, DateTime to)
        
        // 4. Hiệu Suất - Đánh Giá
        public DataTable GetPerformanceByMonth(int branchId, string monthYear)
        public bool SavePerformanceScore(PerformanceInfo info, out string error)
        public float CalculateAverageScore(float attitude, float productivity, float discipline)
        public bool SubmitPerformanceForApproval(int performanceId)
        
        // 5. Utility
        public DataTable GetBranchInfo(int managerId)
        public bool HasPermission(int managerId, int branchId)
        public DataTable GetAvailableReplacements(int branchId, DateTime date)
    }
}
```

### **Tầng GUI: UserControl & Form**

#### **UC_NhanSu.cs** - Quản lý danh sách nhân viên
```
Layout:
├── Panel Header (Title + Buttons)
│   ├── Btn [Thêm Mới] [Sửa] [Xóa] [Làm Mới]
│   └── Tìm kiếm + Lọc theo TrangThai
│
├── DataGridView (Danh sách nhân viên)
│   ├── MaNV | HoTen | Email | SĐT | ViTri | TrangThai | [Chi Tiết]
│
└── Panel Footer (Tổng cộng, Status)
```

#### **UC_LichLamViec.cs** - Quản lý lịch làm
```
Layout (Tabs):
├── Tab 1: Lịch Theo Tuần (Calendar)
│   ├── Chọn tuần
│   ├── Grid: Nhân viên × Ngày
│   ├── Thêm ca, chỉnh sửa, xóa
│
├── Tab 2: Lịch Theo Nhân Viên
│   ├── Chọn nhân viên
│   ├── Table: Ngày | Ca | Vị Trí | Trạng Thái
│   └── Thêm/sửa/xóa lịch
│
└── Tab 3: Tính Năng Cao Cấp
    ├── Phân tích xung đột ca làm
    ├── Tự động phân ca (nếu có)
```

#### **UC_DonXinNghi.cs** - Duyệt đơn xin nghỉ
```
Layout:
├── Tabs: [Chờ Duyệt] [Đã Duyệt] [Đã Từ Chối]
│
├── DataGridView:
│   ├── MaĐơn | HoTen | LoaiNghi | Từ-Đến | LyDo | TrangThai
│   └── [Xem Chi Tiết]
│
├── Panel Chi Tiết Đơn:
│   ├── Thông tin nhân viên
│   ├── Loại nghỉ, ngày, lý do, địa điểm
│   ├── Chọn người thay thế
│   ├── Input ghi chú
│   └── [Duyệt] [Từ Chối]
```

#### **UC_HieuSuat.cs** - Đánh giá hiệu suất
```
Layout:
├── Chọn Tháng/Năm
├── Chọn Nhân Viên (ComboBox)
│
├── TabControl:
│   ├── Tab 1: Nhập Điểm
│   │   ├── Thái Độ: [NumericUpDown 0-10]
│   │   ├── Năng Suất: [NumericUpDown 0-10]
│   │   ├── Kỷ Luật: [NumericUpDown 0-10]
│   │   └── Điểm TB: [Label tự tính]
│   │
│   ├── Tab 2: Nhận Xét & Hành Động
│   │   ├── TextBox Nhận Xét (500 ký tự)
│   │   ├── RadioBtn: [Thưởng] [Phạt] [Đào Tạo] [Không]
│   │   ├── TextBox Ghi Chú
│   │   └── [Gửi Phê Duyệt]
│   │
│   └── Tab 3: Lịch Sử
│       └── DataGridView: Tháng | Điểm | Trạng Thái | Hành Động
│
└── [Lưu] [Gửi] [Đóng]
```

#### **frmNhanVienDetail.cs** - Form chi tiết nhân viên
```
Layout:
├── Panel Header (Tiêu đề + avatar)
│
├── TabControl:
│   ├── Tab 1: Thông Tin Cơ Bản
│   │   ├── Tên, SĐT, Email, Địa chỉ, Ngày sinh
│   │   ├── CMND, Nơi cấp, Ngày cấp
│   │   └── [Edit] [Save] [Cancel]
│   │
│   ├── Tab 2: Thông Tin Công Tác
│   │   ├── Vị trí, Ngày ký hợp đồng, Hạn HĐ
│   │   ├── Tài khoản ngân hàng, Người thụ hưởng
│   │   ├── Trạng thái (CoHieuLuc/HetHieuLuc/TamDung)
│   │   └── [Edit] [Save]
│   │
│   ├── Tab 3: Lịch Làm Việc (30 ngày gần nhất)
│   │   ├── DataGridView: Ngày | Ca | Vị Trí | Trạng Thái
│   │   └── [View All]
│   │
│   ├── Tab 4: Đơn Xin Nghỉ (2024)
│   │   ├── DataGridView: Loại | Từ-Đến | Lý Do | Trạng Thái
│   │   └── [View All]
│   │
│   ├── Tab 5: Hiệu Suất
│   │   ├── DataGridView: Tháng | ĐiểmTB | DanhGia | HanhDong
│   │   └── [View Chi Tiết]
│   │
│   └── Tab 6: Lịch Sử Công Tác
│       └── DataGridView: Ngày | Hành Động | Ghi Chú
│
└── [Đóng]
```

---

## 📁 FILE & CODE SAMPLES

### **1. ManagerDAL.cs** - Mẫu truy vấn
```csharp
// Ví dụ GetNhanVienByBranch
public DataTable GetNhanVienByBranch(int branchId)
{
    DataTable dt = new DataTable();
    string query = @"
        SELECT 
            nv.MaNhanVien,
            nd.MaNguoiDung,
            nd.HoTen,
            nd.Email,
            nd.SoDienThoai,
            nv.ViTri,
            nv.TrangThai,
            nd.NgayTao,
            nv.NgayKyHopDong
        FROM NhanVienChiTiet nv
        INNER JOIN NguoiDung nd ON nv.MaNguoiDung = nd.MaNguoiDung
        WHERE nv.MaChiNhanh = @BranchId
        ORDER BY nd.HoTen";
    
    using (SqlConnection conn = new SqlConnection(DatabaseConfig.ConnectionString))
    {
        conn.Open();
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@BranchId", branchId);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
    }
    return dt;
}
```

---

## 🔐 PHÂN QUYỀN

| Chức Năng | Admin | Quản Lý | Nhân Viên |
|---|---|---|---|
| Xem danh sách nhân viên | ✅ Toàn hệ thống | ✅ Chi nhánh của mình | ❌ |
| Thêm/Sửa/Xóa nhân viên | ✅ | ❌ | ❌ |
| Phân công ca làm | ❌ | ✅ Chi nhánh | ❌ |
| Duyệt đơn xin nghỉ | ✅ | ✅ | ❌ |
| Đánh giá hiệu suất | ❌ | ✅ | ❌ |
| Phê duyệt hiệu suất | ✅ | ❌ | ❌ |
| Xem lịch làm của mình | ✅ | ✅ | ✅ |
| Xin nghỉ phép | ❌ | ❌ | ✅ |

---

## ✅ CHECKLIST TRIỂN KHAI

### Phase 1: Database & DAL
- [ ] Tạo 5 bảng mới trong SQL Server
- [ ] Viết SQL INSERT sample data (5-10 nhân viên/chi nhánh)
- [ ] Tạo ManagerDAL.cs với 15+ methods

### Phase 2: Business Logic
- [ ] Tạo ManagerBLL.cs với validation & business logic
- [ ] Xử lý phân quyền (chỉ xem dữ liệu chi nhánh của mình)
- [ ] Tính toán tự động (điểm trung bình, conflict detection)

### Phase 3: GUI - Nhân Viên
- [ ] Xây dựng UC_NhanSu.cs (danh sách + CRUD)
- [ ] Tạo frmNhanVienDetail.cs (chi tiết 6 tabs)
- [ ] Kết nối dữ liệu & test

### Phase 4: GUI - Lịch Làm & Đơn Xin Nghỉ
- [ ] Xây dựng UC_LichLamViec.cs
- [ ] Xây dựng UC_DonXinNghi.cs
- [ ] Auto-update lịch khi duyệt đơn xin nghỉ

### Phase 5: GUI - Hiệu Suất
- [ ] Xây dựng UC_HieuSuat.cs
- [ ] Tính điểm trung bình tự động
- [ ] Gửi phê duyệt cho Admin

### Phase 6: Integration & Test
- [ ] Cập nhật frmMain sidebar (thêm menu Nhân Sự)
- [ ] Test phân quyền & CRUD
- [ ] Test tính năng xung đột lịch
- [ ] Test tự động điều chỉnh lịch từ đơn xin nghỉ

---

## 📝 NOTES

1. **Database**: Hiện sử dụng SQL Server online (site4now.net). Cần kiểm tra quyền tạo bảng.
2. **Security**: Luôn kiểm tra MaChiNhanh của Manager trước khi lấy dữ liệu.
3. **DateTime**: Sử dụng format 'yyyy-MM-dd' cho consistency.
4. **Validation**: Kiểm tra overlap lịch làm, hạn hợp đồng, số ngày nghỉ còn lại.
5. **Notifications**: Sau này có thể thêm email/SMS khi duyệt đơn.

---

## 🚀 NEXT STEPS

1. **Ngay**: Tạo database tables
2. **Hôm sau**: Code ManagerDAL.cs
3. **Tiếp**: Code ManagerBLL.cs + UC_NhanSu.cs
4. **Cuối cùng**: Tích hợp & test toàn bộ

