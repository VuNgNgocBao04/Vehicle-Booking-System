# Vehicle-Booking-System

Hệ thống Quản lý và Đặt xe trực tuyến (Vehicle Booking Management System).

## Kiến trúc

Project được dựng bằng ASP.NET Core MVC + Entity Framework Core Code First + SQL Server. `Users` và `Roles` dùng ASP.NET Core Identity, còn `Vehicles`, `Bookings`, `Payments` và `VehicleCategories` là các bảng nghiệp vụ. Cấu hình hiện tại trỏ vào SQL Server Express theo datasource bạn cung cấp và database `QuanLyDatXeNew`.

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

## Chạy ứng dụng

```bash
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run --project VehicleBookingSystem
```

Connection string:

`Data Source=DESKTOP-4E5I8PE\SQLEXPRESS;Initial Catalog=QuanLyDatXeNew;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False`

## Seed dữ liệu mẫu

- Tài khoản admin: `admin@vehiclebooking.local`
- Mật khẩu admin: `Admin123!`
- Role: `Admin`, `Customer`
- Danh mục xe: `Sedan`, `SUV`, `MPV`, `Hatchback`
- Xe mẫu đã được tạo sẵn khi ứng dụng khởi động lần đầu
