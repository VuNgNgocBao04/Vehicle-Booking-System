# HƯỚNG DẪN CÀI ĐẶT VÀ CẤU HÌNH

## 1. Yêu cầu môi trường

- .NET 9 SDK.
- SQL Server 2019/2022 hoặc SQL Server Express.
- Visual Studio 2022 hoặc VS Code.
- Node/browser để kiểm tra UI nếu cần.

## 2. Cấu trúc gói nộp bài

Gói nộp bài nên gồm 3 phần:

1. **CSDL**: file `.sql` script.
2. **Frontend template**: nếu tách riêng asset hoặc theme.
3. **Backend chương trình**: toàn bộ source code web app.

Tên gói theo quy tắc:

- `TenCSDL_NhomX_LopY.zip`
- Ví dụ: `VehicleBookingSystem_Nhom6_N01.zip`

Nếu giảng viên yêu cầu đuôi `.zar`, có thể đổi tên file zip sang `.zar` sau khi nén xong.

## 3. Chạy database bằng script SQL

File script hiện có:

- [Database/VehicleBookingSystem.sql](../VehicleBookingSystem/Database/VehicleBookingSystem.sql)

Thực hiện:

1. Mở SQL Server Management Studio.
2. Tạo database trống.
3. Mở file `.sql`.
4. Chạy toàn bộ script.
5. Kiểm tra bảng `Bookings`, `BookingStatusHistories`, `Vehicles`, `Payments` và các bảng Identity.

## 4. Cấu hình ứng dụng

### 4.1 Appsettings

Các cấu hình chính nằm trong:

- `VehicleBookingSystem/appsettings.json`
- `VehicleBookingSystem/appsettings.Development.json`

Các mục cần lưu ý:

- Connection string.
- Jwt options.
- Session options.
- Startup options.
- Seed data.

### 4.2 Cấu hình seed admin

Nếu muốn tự tạo dữ liệu mẫu khi chạy lần đầu:

```bash
dotnet user-secrets set "SeedData:AdminPassword" "<mat-khau-manh>" --project VehicleBookingSystem
```

Có thể bật thêm:

- `StartupOptions:ApplyMigrationsOnStartup = true`
- `StartupOptions:SeedOnStartup = true`

Trong môi trường demo/nộp bài, nên giữ mật khẩu admin trong User Secrets, không commit vào repo.

## 5. Chạy ứng dụng

Từ thư mục gốc repository:

```bash
dotnet restore
dotnet build VehicleBookingSystem.sln
dotnet test VehicleBookingSystem.sln
dotnet run --project VehicleBookingSystem
```

## 6. Tài khoản mặc định

Khi bật seed:

- Email admin: `admin@vehiclebooking.local`
- Role: `Admin`
- Role khách hàng: `Customer`

## 7. Kiểm tra chức năng sau khi cài đặt

Nên kiểm tra tối thiểu các luồng sau:

- Đăng nhập admin.
- Mở trang quản trị xe.
- Mở trang quản trị booking.
- Đổi ngôn ngữ vi/en.
- Tạo booking từ phía khách.
- Hủy booking.
- Xem lịch sử trạng thái booking.

## 8. Lưu ý bảo mật khi triển khai

- Không commit connection string thật.
- Không commit mật khẩu admin.
- Giữ cookie bảo mật ở môi trường production.
- Bật HTTPS.
- Rà soát lại package vulnerabilities trước khi đóng gói.

## 9. Gợi ý nộp bài

Nên nộp kèm:

- Báo cáo tổng kết.
- Hướng dẫn cài đặt/cấu hình này.
- File SQL script.
- Mã nguồn backend/frontend đã build ổn định.
