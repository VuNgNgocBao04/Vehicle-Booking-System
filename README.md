# Vehicle Booking System

Hệ thống quản lý và đặt xe trực tuyến, xây dựng trên ASP.NET Core MVC.

Mục tiêu của dự án là số hóa quy trình tìm xe, đặt xe, theo dõi booking cho khách hàng và quản trị đội xe cho quản trị viên, đồng thời đảm bảo tính ổn định qua bộ kiểm thử tự động.

## 1. Tổng quan chức năng

### 1.1 Chức năng phía khách hàng

- Đăng ký, đăng nhập, quản lý tài khoản.
- Duyệt danh sách xe và tìm kiếm theo từ khóa.
- Lọc xe theo danh mục, hãng xe, số chỗ, khoảng giá, sắp xếp.
- Xem chi tiết xe và lịch khả dụng theo ngày.
- Tạo booking và theo dõi lịch sử booking cá nhân.
- Hủy booking theo điều kiện nghiệp vụ.

### 1.2 Chức năng phía quản trị viên

- Dashboard tổng quan booking, doanh thu, trạng thái.
- Quản lý danh mục xe.
- Quản lý xe, ảnh xe, chính sách đặt xe.
- Quản lý booking: duyệt, từ chối, hủy, theo dõi lịch sử trạng thái.
- Làm việc với API cho các luồng nghiệp vụ chính.

## 2. Kiến trúc và công nghệ

- Backend: ASP.NET Core MVC (.NET 9).
- ORM: Entity Framework Core Code First + SQL Server.
- AuthN/AuthZ: ASP.NET Core Identity + Role-based authorization.
- API: Chuẩn hóa phản hồi thành công bằng ApiResponse kiểu generic, lỗi bằng ProblemDetails.
- Validation: Data Annotations + FluentValidation.
- Frontend: Razor Views, Bootstrap 5, jQuery, DataTables.
- Bảo mật bổ sung: JWT cho API, rate limit cho login, session/cookie cấu hình an toàn.
- I18N: vi-VN và en-US qua Resource files + cookie culture.

## 3. Cấu trúc thư mục chính

```text
Vehicle-Booking-System/
├─ VehicleBookingSystem/               # Ứng dụng web chính
│  ├─ Areas/Admin/                     # Khu vực quản trị
│  ├─ Controllers/                     # MVC + API controllers
│  ├─ Data/                            # DbContext, seed, cấu hình EF
│  ├─ Migrations/                      # EF Core migrations
│  ├─ Models/                          # Entity models
│  ├─ Services/                        # Service layer nghiệp vụ
│  ├─ ViewModels/                      # View models
│  ├─ Views/                           # Razor views
│  └─ wwwroot/                         # Static assets
├─ VehicleBookingSystem.Tests/         # Bộ kiểm thử xUnit + Playwright
├─ docs/                               # Tài liệu báo cáo, cài đặt, đóng gói, kiểm thử
└─ VehicleBookingSystem.sln
```

## 4. Yêu cầu môi trường

- Windows + SQL Server/SQL Server Express.
- .NET SDK 9.x.
- dotnet-ef (nếu cần thao tác migration từ CLI).

Cài dotnet-ef nếu máy chưa có:

```bash
dotnet tool install --global dotnet-ef
```

## 5. Cài đặt và chạy nhanh

### 5.1 Restore và build

```bash
dotnet restore VehicleBookingSystem.sln
dotnet build VehicleBookingSystem.sln -c Debug
```

### 5.2 Cấu hình database

- Chuỗi kết nối nằm ở VehicleBookingSystem/appsettings.Development.json.
- Tạo/cập nhật schema bằng migration:

```bash
dotnet ef database update --project VehicleBookingSystem
```

### 5.3 Cấu hình JWT local (khuyến nghị)

```bash
set Jwt__Issuer=VehicleBookingSystem
set Jwt__Audience=VehicleBookingSystem.Client
set Jwt__ExpireMinutes=60
set Jwt__Key=dev-local-jwt-key-1234567890-abcdef
```

### 5.4 Chạy ứng dụng

```bash
dotnet run --project VehicleBookingSystem --launch-profile https
```

URL mặc định:

- [https://localhost:7291](https://localhost:7291)
- [http://localhost:5294](http://localhost:5294)

Nếu máy chưa trust chứng chỉ local:

```bash
dotnet dev-certs https --trust
```

## 6. Kiểm thử

### 6.1 Bộ test hiện có

- API tests: xác minh contract và hành vi API.
- Controller tests: xác minh hành vi controller theo trạng thái dữ liệu.
- Service tests: kiểm thử xử lý file và ngoại lệ.
- Validation tests: kiểm thử custom attributes/ràng buộc dữ liệu.
- UI smoke tests (Playwright): kiểm tra nhanh các luồng giao diện trọng yếu.

### 6.2 Lệnh chạy test

Chạy toàn bộ test backend (không gồm E2E):

```bash
dotnet test VehicleBookingSystem.sln --filter "Category!=E2E"
```

Chạy full suite (bao gồm E2E, có thể skip nếu chưa bật biến môi trường):

```bash
dotnet test VehicleBookingSystem.sln
```

Chạy E2E smoke tests:

```bash
set RUN_UI_SMOKE=true
set UI_BASE_URL=https://localhost:7291
set UI_ADMIN_EMAIL=admin@vehiclebooking.local
set UI_ADMIN_PASSWORD=<admin-password>
dotnet test VehicleBookingSystem.Tests/VehicleBookingSystem.Tests.csproj --filter "Category=E2E"
```

## 7. Dữ liệu seed và tài khoản mẫu

- Email admin mặc định: [admin@vehiclebooking.local](mailto:admin@vehiclebooking.local).
- Mật khẩu admin: thiết lập qua user-secrets, không commit vào source.

Ví dụ:

```bash
dotnet user-secrets set "SeedData:AdminPassword" "<your-secure-password>" --project VehicleBookingSystem
```

## 8. Tài liệu dự án

- Báo cáo tổng kết: docs/bao-cao-tong-ket.md
- Hướng dẫn cài đặt và cấu hình: docs/huong-dan-cai-dat-va-cau-hinh.md
- Hướng dẫn đóng gói sản phẩm: docs/dong-goi-san-pham.md
- Tài liệu kiểm thử chi tiết theo mẫu báo cáo: docs/testing.md

## 9. Ghi chú chất lượng và phạm vi

- Bộ lọc danh sách xe hiện bám sát dữ liệu bảng Vehicle (không giữ tiêu chí nửa vời không có tác dụng lọc).
- Hệ thống đã tách service layer để giảm phụ thuộc trực tiếp giữa controller và data layer.
- Một số hạng mục nâng cao như thanh toán online, kiểm thử hiệu năng tải lớn, giám sát production-level nằm trong kế hoạch mở rộng.
