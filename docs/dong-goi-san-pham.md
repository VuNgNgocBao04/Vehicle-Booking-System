# HƯỚNG DẪN ĐÓNG GÓI SẢN PHẨM

## 1. Mục tiêu đóng gói

Đảm bảo khi giảng viên nhận file có thể:

- Dựng được CSDL từ file `.sql`.
- Chạy được backend ASP.NET Core MVC.
- Mở được giao diện frontend.
- Kiểm tra được các chức năng chính mà không cần dò lại mã nguồn.

## 2. Thành phần cần đóng gói

### 2.1 CSDL

- File script: `VehicleBookingSystem/Database/VehicleBookingSystem.sql`
- Đây là file bắt buộc nên có trong gói nộp.

### 2.2 Backend

Bao gồm toàn bộ source của project:

- `VehicleBookingSystem/`
- `VehicleBookingSystem.Tests/`
- `VehicleBookingSystem.sln`
- Các file cấu hình liên quan.
- Thư mục service layer nằm trong `VehicleBookingSystem/Services/` và phải được giữ nguyên khi đóng gói.

### 2.3 Frontend/template

Trong repo hiện tại, frontend nằm chung trong project backend:

- Razor Views.
- Bootstrap 5.
- jQuery.
- DataTables.
- CSS/JS trong `wwwroot`.

Nếu giảng viên yêu cầu tách riêng template, có thể nén thêm một bản phụ chứa:

- `wwwroot/`
- assets tùy chỉnh
- tài liệu mô tả theme/layout

## 3. Cách nén

### Phương án khuyến nghị

1. Copy toàn bộ source sạch ra một thư mục riêng.
2. Xóa các thư mục build nếu không cần:
   - `bin/`
   - `obj/`
3. Giữ lại:
   - source code
   - migrations
   - database script
   - docs
4. Nén thành `.zip`.
5. Đổi tên file theo quy tắc nhóm/lớp.

### Tên file đề xuất

- `VehicleBookingSystem_NhomX_LopY.zip`
- Nếu giảng viên bắt buộc đuôi `.zar`, đổi tên sau khi nén.

## 4. Kiểm tra trước khi nộp

Checklist:

- [ ] `dotnet build` pass.
- [ ] `dotnet test` pass.
- [ ] Script SQL chạy được trên SQL Server.
- [ ] Trang Admin xe hoạt động.
- [ ] Trang Admin booking hoạt động.
- [ ] Đổi ngôn ngữ hoạt động.
- [ ] Responsive trên mobile/tablet.
- [ ] Không còn dữ liệu demo nhạy cảm.
- [ ] API bookings trả `ApiResponse<T>` và lỗi trả ProblemDetails.

## 5. Đề xuất hoàn thiện thêm nếu còn thời gian

- Thêm ảnh chụp màn hình vào báo cáo.
- Chèn sơ đồ ERD/use-case/sitemap vào file báo cáo.
- Tối ưu cảnh báo package vulnerabilities.
- Tách API documentation ngắn gọn vào phụ lục.
- Viết thêm phần nêu rõ phân công của từng thành viên.

## 6. Gợi ý cấu trúc thư mục khi nén

```text
VehicleBookingSystem_NhomX_LopY/
├─ VehicleBookingSystem/
├─ VehicleBookingSystem.Tests/
├─ VehicleBookingSystem.sln
├─ Database/
│  └─ VehicleBookingSystem.sql
├─ docs/
│  ├─ bao-cao-tong-ket.md
│  ├─ huong-dan-cai-dat-va-cau-hinh.md
│  └─ dong-goi-san-pham.md
└─ README.md
```
