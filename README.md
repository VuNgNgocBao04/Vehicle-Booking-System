# Vehicle-Booking-System

Hệ thống Quản lý và Đặt xe trực tuyến (Vehicle Booking Management System).

## Kiến trúc

Project được dựng bằng ASP.NET Core MVC + Entity Framework Core Code First + SQL Server. `Users` và `Roles` dùng ASP.NET Core Identity, còn `Vehicles`, `Bookings`, `Payments` và `VehicleCategories` là các bảng nghiệp vụ. Phần nghiệp vụ chính đã được tách vào service layer riêng: `AccountService`, `BookingService`, `CustomerService`, `VehicleService`.

Ứng dụng đã được chỉnh để mở ổn trong Visual Studio 2022/2025 bằng launch profile trong `VehicleBookingSystem/Properties/launchSettings.json`. Ảnh gallery và avatar không còn phụ thuộc vào working directory hiện tại, nên chạy từ Visual Studio, `dotnet run`, hay IIS Express đều ổn định hơn.

API đã chuẩn hóa response theo `ApiResponse<T>` cho dữ liệu thành công và dùng `ProblemDetails` chung cho lỗi.

## ERD

```mermaid
erDiagram
  APPUSER ||--o{ BOOKING : makes
  VEHICLECATEGORY ||--o{ VEHICLE : contains
  VEHICLE ||--o{ BOOKING : booked_for
  BOOKING ||--o| PAYMENT : has

  APPUSER {
    guid Id PK
    string UserName
    string Email
    string FullName
  }

  APPROLE {
    guid Id PK
    string Name
    string Description
  }

  VEHICLECATEGORY {
    guid Id PK
    string Name
    string Description
  }

  VEHICLE {
    guid Id PK
    guid VehicleCategoryId FK
    string Code
    string Brand
    string Model
    string LicensePlate
    decimal DailyRate
    int Status
  }

  BOOKING {
    guid Id PK
    guid UserId FK
    guid VehicleId FK
    string BookingCode
    decimal TotalAmount
    int Status
  }

  PAYMENT {
    guid Id PK
    guid BookingId FK
    decimal PaidAmount
    int PaymentMethod
    int Status
  }
```

## Chạy ứng dụng (chi tiết build + mở browser)

### 1) Điều kiện trước khi chạy

- Windows + SQL Server/SQL Server Express đang chạy.
- .NET SDK 9.x (`dotnet --version`).
- EF Core CLI (`dotnet ef --version`). Nếu chưa có:

```bash
dotnet tool install --global dotnet-ef
```

Connection string cho môi trường development nằm trong `VehicleBookingSystem/appsettings.Development.json`. Nếu triển khai production, hãy đặt `ConnectionStrings__DefaultConnection`, `Jwt__Key`, và các biến cấu hình khác qua environment variables hoặc secret store của hạ tầng.

JWT signing key không còn nằm trực tiếp trong source. Ở Development, app sẽ tự sinh key tạm để demo chạy ổn định; ở môi trường khác, bạn phải cấu hình `Jwt__Key` qua user-secrets hoặc môi trường.

Hai cờ `StartupOptions:ApplyMigrationsOnStartup` và `StartupOptions:SeedOnStartup` mặc định tắt để app khởi động ổn định. Nếu cần seed dữ liệu demo, hãy bật lại thủ công và đặt `SeedData:AdminPassword` bằng user-secrets.

### 2) Restore package và build solution

Chạy tại thư mục root repository:

```bash
dotnet restore VehicleBookingSystem.sln
dotnet build VehicleBookingSystem.sln -c Debug
```

### 3) Cấu hình JWT cho môi trường local (bắt buộc)

App sẽ không khởi động nếu thiếu `Jwt:Key`. Thiết lập nhanh bằng biến môi trường (Command Prompt):

```bash
set Jwt__Issuer=VehicleBookingSystem
set Jwt__Audience=VehicleBookingSystem.Client
set Jwt__ExpireMinutes=60
set Jwt__Key=dev-local-jwt-key-1234567890-abcdef
```

Gợi ý: dùng key dài tối thiểu 32 ký tự.

### 4) Apply migration để tạo/cập nhật database

```bash
dotnet ef database update --project VehicleBookingSystem
```

### 5) Chạy web app với profile HTTPS

```bash
dotnet run --project VehicleBookingSystem --launch-profile https
```

Khi chạy thành công, app lắng nghe tại:

- `https://localhost:7291`
- `http://localhost:5294`

Khuyến nghị mở bản HTTPS trên browser: `https://localhost:7291`.

Nếu browser cảnh báo chứng chỉ local lần đầu, chạy:

```bash
dotnet dev-certs https --trust
```

Sau đó restart app và mở lại URL HTTPS.

### 6) Dừng ứng dụng

Trong terminal đang chạy app, bấm `Ctrl + C`.

## Frontend UI Smoke Tests (Playwright)

Project test đã có các smoke tests cho 3 luồng critical:

- Listing filter
- Booking create (anonymous flow redirect login)
- Admin dashboard chart render

Các test UI đã được gắn category riêng: `Category=E2E`.

### Lệnh pipeline mặc định (nhanh, không chạy E2E)

```bash
dotnet test VehicleBookingSystem.Tests/VehicleBookingSystem.Tests.csproj --filter "Category!=E2E"
```

### Lệnh chạy riêng E2E smoke tests

```bash
dotnet test VehicleBookingSystem.Tests/VehicleBookingSystem.Tests.csproj --filter "Category=E2E"
```

Các test này chỉ chạy khi bật cờ môi trường, để tránh fail trên máy chưa chạy web app:

```bash
set RUN_UI_SMOKE=true
set UI_BASE_URL=https://localhost:7291
set UI_ADMIN_EMAIL=admin@vehiclebooking.local
set UI_ADMIN_PASSWORD=<your-admin-password>
dotnet test VehicleBookingSystem.Tests/VehicleBookingSystem.Tests.csproj
```

Lần đầu chạy cần cài browser cho Playwright:

```bash
pwsh VehicleBookingSystem.Tests/bin/Debug/net9.0/playwright.ps1 install
```

Lưu ý triển khai production: TinyMCE trong trang quản trị xe đang dùng CDN `no-api-key` cho mục đích development. Trước khi deploy, cần thay bằng API key hợp lệ (hoặc self-host) để tránh cảnh báo và giới hạn tính năng từ nhà cung cấp.

## Seed dữ liệu mẫu

- Tài khoản admin: `admin@vehiclebooking.local`
- Mật khẩu admin: đặt qua .NET User Secrets (không commit vào source control) khi bật `StartupOptions:SeedOnStartup`

  ```bash
  dotnet user-secrets set "SeedData:AdminPassword" "<your-secure-password>" --project VehicleBookingSystem
  ```

- Role: `Admin`, `Customer`
- Danh mục xe: `Sedan`, `SUV`, `MPV`, `Hatchback`
- Xe mẫu đã được tạo sẵn khi ứng dụng khởi động lần đầu

## Kiến trúc hiện tại

- MVC controllers chỉ còn vai trò orchestration.
- Service layer xử lý query/command cho account, booking, customer và vehicle.
- API bookings dùng `ApiResponse<T>` và `ApiProblemDetailsFactory` để giữ response nhất quán.
- Các lỗi runtime đi qua `HomeController.Error` để hiển thị HTML cho MVC hoặc ProblemDetails cho API.

## Tài liệu nộp bài

Các file Markdown phục vụ báo cáo và đóng gói được đặt trong thư mục `docs/`:

- [Báo cáo tổng kết](docs/bao-cao-tong-ket.md)
- [Hướng dẫn cài đặt và cấu hình](docs/huong-dan-cai-dat-va-cau-hinh.md)
- [Hướng dẫn đóng gói sản phẩm](docs/dong-goi-san-pham.md)

Khi hoàn thiện, hãy xuất báo cáo sang Word/PDF theo đúng định dạng yêu cầu của giảng viên và thêm ảnh minh họa thực tế vào các vị trí được đánh dấu trong báo cáo.
