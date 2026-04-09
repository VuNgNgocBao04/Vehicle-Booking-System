CREATE DATABASE QuanLyDatXeNew;
GO

USE QuanLyDatXeNew;
GO

-- ==========================================
-- PHẦN 1: TẠO BẢNG & RÀNG BUỘC (CONSTRAINTS)
-- ==========================================

-- 1. Bảng NguoiDung
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(15) NOT NULL UNIQUE,
    DiaChi NVARCHAR(255),
    GioiTinh NVARCHAR(10),
    NgaySinh DATE
);

-- 2. Bảng QuanTriVien
CREATE TABLE QuanTriVien (
    MaQuanTriVien INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10),
    NgaySinh DATE
);

-- 3. Bảng TaiKhoan (Sửa lỗi: 1 tài khoản chỉ thuộc về 1 User HOẶC 1 Admin)
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    TenTaiKhoan VARCHAR(50) NOT NULL UNIQUE,
    TrangThaiTaiKhoan NVARCHAR(50) DEFAULT N'Hoạt động',
    MatKhau VARCHAR(255) NOT NULL, -- Thực tế sẽ lưu chuỗi Hashed (Bcrypt/Argon2)
    MaNguoiDung INT NULL,
    MaQuanTriVien INT NULL,
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (MaQuanTriVien) REFERENCES QuanTriVien(MaQuanTriVien),
    -- Ràng buộc: Không được phép trống cả 2, cũng không được phép có cả 2
    CONSTRAINT CHK_PhanQuyenTaiKhoan CHECK (
        (MaNguoiDung IS NOT NULL AND MaQuanTriVien IS NULL) OR 
        (MaNguoiDung IS NULL AND MaQuanTriVien IS NOT NULL)
    )
);

-- 4. Bảng Xe (MỚI: Bắt buộc phải có để quản lý phương tiện)
CREATE TABLE Xe (
    MaXe INT IDENTITY(1,1) PRIMARY KEY,
    TenXe NVARCHAR(100) NOT NULL,
    BienSo VARCHAR(20) NOT NULL UNIQUE,
    LoaiXe NVARCHAR(50), -- Ví dụ: 4 chỗ, 7 chỗ
    TrangThai NVARCHAR(50) DEFAULT N'Trống' -- Trống, Đang di chuyển, Đang bảo trì
);

-- 5. Bảng UuDai
CREATE TABLE UuDai (
    MaUuDai INT IDENTITY(1,1) PRIMARY KEY,
    TenUuDai NVARCHAR(150) NOT NULL,
    MucGiam DECIMAL(18,2) NOT NULL, -- Giá trị giảm trực tiếp
    DieuKien NVARCHAR(255),
    HanSuDung DATE NOT NULL,
    SoLuong INT DEFAULT 0,
    MaQuanTriVien INT NOT NULL,
    FOREIGN KEY (MaQuanTriVien) REFERENCES QuanTriVien(MaQuanTriVien)
);

-- 6. Bảng ChuyenXe (Đã nâng cấp: Đóng vai trò vừa là Đơn đặt xe vừa là Chuyến đi)
CREATE TABLE ChuyenXe (
    MaChuyenXe INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT NOT NULL, -- Bắt buộc: Biết ai đặt
    MaXe INT NOT NULL,        -- Bắt buộc: Biết xe nào chạy
    MaUuDai INT NULL,
    ThoiGianDat DATETIME DEFAULT GETDATE(),
    ThoiGianBatDau DATETIME NOT NULL,
    ThoiGianKetThuc DATETIME NULL,
    DiemDon NVARCHAR(255) NOT NULL,
    DiemDen NVARCHAR(255) NOT NULL,
    GiaGoc DECIMAL(18,2) NOT NULL, -- Giá cước chưa trừ khuyến mãi
    TrangThai NVARCHAR(50) DEFAULT N'Chờ xử lý', -- Chờ xử lý, Đang chạy, Hoàn thành, Đã hủy
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    FOREIGN KEY (MaXe) REFERENCES Xe(MaXe),
    FOREIGN KEY (MaUuDai) REFERENCES UuDai(MaUuDai)
);

-- 7. Bảng DanhGia (Thêm ràng buộc chặt chẽ số sao 1-5)
CREATE TABLE DanhGia (
    MaDanhGia INT IDENTITY(1,1) PRIMARY KEY,
    MaChuyenXe INT NOT NULL UNIQUE, -- 1 chuyến chỉ đánh giá 1 lần
    SoSao INT NOT NULL,
    PhanHoi NVARCHAR(MAX),
    FOREIGN KEY (MaChuyenXe) REFERENCES ChuyenXe(MaChuyenXe),
    CONSTRAINT CHK_SoSao CHECK (SoSao BETWEEN 1 AND 5)
);

-- 8. Bảng ThanhToan (Logic tính tiền rõ ràng hơn)
CREATE TABLE ThanhToan (
    MaThanhToan INT IDENTITY(1,1) PRIMARY KEY,
    MaChuyenXe INT NOT NULL UNIQUE,
    PhuongThucThanhToan NVARCHAR(50) NOT NULL,
    SoTienGiam DECIMAL(18,2) DEFAULT 0,
    TongTienThanhToan DECIMAL(18,2) NOT NULL, -- TongTien = GiaGoc - SoTienGiam (Tính từ Tầng Logic)
    TrangThaiThanhToan NVARCHAR(50) DEFAULT N'Đã thanh toán',
    FOREIGN KEY (MaChuyenXe) REFERENCES ChuyenXe(MaChuyenXe),
    CONSTRAINT CHK_TongTien CHECK (TongTienThanhToan >= 0) -- Không được phép âm tiền
);
GO

-- ==========================================
-- PHẦN 2: THÊM DỮ LIỆU MẪU CHUẨN XÁC
-- ==========================================

-- Thêm 20 NguoiDung
INSERT INTO NguoiDung (HoTen, SoDienThoai, DiaChi, GioiTinh, NgaySinh) VALUES
(N'Nguyễn Văn An', '0901234567', N'Hà Nội', N'Nam', '1995-05-12'),
(N'Trần Thị Bình', '0912345678', N'TP.HCM', N'Nữ', '1998-08-22'),
(N'Lê Văn Cường', '0923456789', N'Đà Nẵng', N'Nam', '1990-11-05'),
(N'Phạm Thị Dung', '0934567890', N'Hải Phòng', N'Nữ', '2000-02-14'),
(N'Hoàng Văn Em', '0945678901', N'Cần Thơ', N'Nam', '1985-07-30'),
(N'Vũ Thị Phương', '0956789012', N'Nha Trang', N'Nữ', '1999-09-09'),
(N'Đặng Văn Giáp', '0967890123', N'Huế', N'Nam', '1992-04-18'),
(N'Bùi Thị Hoa', '0978901234', N'Hạ Long', N'Nữ', '1997-12-01'),
(N'Đỗ Văn Inh', '0989012345', N'Vinh', N'Nam', '1994-06-25'),
(N'Ngô Thị Kim', '0990123456', N'Đà Lạt', N'Nữ', '2001-03-08'),
(N'Dương Văn Linh', '0909876543', N'Hà Nội', N'Nam', '1988-10-20'),
(N'Lý Thị Mai', '0918765432', N'TP.HCM', N'Nữ', '1996-01-15'),
(N'Đào Văn Nam', '0927654321', N'Đà Nẵng', N'Nam', '1993-08-08'),
(N'Đoàn Thị Oanh', '0936543210', N'Hải Phòng', N'Nữ', '1991-05-19'),
(N'Trịnh Văn Phúc', '0945432109', N'Cần Thơ', N'Nam', '1989-11-11'),
(N'Nguyễn Thị Quỳnh', '0954321098', N'Nha Trang', N'Nữ', '2002-07-07'),
(N'Lê Văn Sang', '0963210987', N'Huế', N'Nam', '1995-02-28'),
(N'Trần Thị Thu', '0972109876', N'Hạ Long', N'Nữ', '1998-04-30'),
(N'Phạm Văn Uy', '0981098765', N'Vinh', N'Nam', '1990-09-02'),
(N'Hoàng Thị Vy', '0990987654', N'Đà Lạt', N'Nữ', '2000-12-24');

-- Thêm 20 QuanTriVien
INSERT INTO QuanTriVien (HoTen, GioiTinh, NgaySinh) VALUES
(N'Admin Tuấn', N'Nam', '1985-01-01'), (N'Admin Lan', N'Nữ', '1988-02-02'),
(N'Admin Hùng', N'Nam', '1990-03-03'), (N'Admin Hương', N'Nữ', '1992-04-04'),
(N'Admin Minh', N'Nam', '1987-05-05'), (N'Admin Ngọc', N'Nữ', '1989-06-06'),
(N'Admin Sơn', N'Nam', '1991-07-07'), (N'Admin Trang', N'Nữ', '1993-08-08'),
(N'Admin Kiên', N'Nam', '1986-09-09'), (N'Admin Hạnh', N'Nữ', '1990-10-10'),
(N'Admin Đạt', N'Nam', '1988-11-11'), (N'Admin Yến', N'Nữ', '1994-12-12'),
(N'Admin Thành', N'Nam', '1985-01-15'), (N'Admin Thủy', N'Nữ', '1987-02-16'),
(N'Admin Phúc', N'Nam', '1992-03-17'), (N'Admin Thảo', N'Nữ', '1995-04-18'),
(N'Admin Cường', N'Nam', '1989-05-19'), (N'Admin My', N'Nữ', '1991-06-20'),
(N'Admin Khoa', N'Nam', '1993-07-21'), (N'Admin Linh', N'Nữ', '1986-08-22');

-- Thêm 20 TaiKhoan (10 User, 10 Admin - Thỏa mãn Check Constraint)
INSERT INTO TaiKhoan (TenTaiKhoan, MatKhau, MaNguoiDung, MaQuanTriVien) VALUES
('user_an', '$2a$12$hashedstring1', 1, NULL),
('user_binh', '$2a$12$hashedstring2', 2, NULL),
('user_cuong', '$2a$12$hashedstring3', 3, NULL),
('user_dung', '$2a$12$hashedstring4', 4, NULL),
('user_em', '$2a$12$hashedstring5', 5, NULL),
('user_phuong', '$2a$12$hashedstring6', 6, NULL),
('user_giap', '$2a$12$hashedstring7', 7, NULL),
('user_hoa', '$2a$12$hashedstring8', 8, NULL),
('user_inh', '$2a$12$hashedstring9', 9, NULL),
('user_kim', '$2a$12$hashedstring10', 10, NULL),
('admin_tuan', '$2a$12$adminhash1', NULL, 1),
('admin_lan', '$2a$12$adminhash2', NULL, 2),
('admin_hung', '$2a$12$adminhash3', NULL, 3),
('admin_huong', '$2a$12$adminhash4', NULL, 4),
('admin_minh', '$2a$12$adminhash5', NULL, 5),
('admin_ngoc', '$2a$12$adminhash6', NULL, 6),
('admin_son', '$2a$12$adminhash7', NULL, 7),
('admin_trang', '$2a$12$adminhash8', NULL, 8),
('admin_kien', '$2a$12$adminhash9', NULL, 9),
('admin_hanh', '$2a$12$adminhash10', NULL, 10);

-- Thêm 20 Xe (Dữ liệu mới)
INSERT INTO Xe (TenXe, BienSo, LoaiXe, TrangThai) VALUES
(N'Toyota Vios', '29A-111.11', N'4 Chỗ', N'Trống'),
(N'Hyundai Accent', '29A-222.22', N'4 Chỗ', N'Trống'),
(N'Kia Cerato', '30A-333.33', N'4 Chỗ', N'Đang di chuyển'),
(N'Mazda 3', '30A-444.44', N'4 Chỗ', N'Trống'),
(N'Honda City', '30A-555.55', N'4 Chỗ', N'Đang bảo trì'),
(N'Toyota Innova', '51A-666.66', N'7 Chỗ', N'Trống'),
(N'Mitsubishi Xpander', '51A-777.77', N'7 Chỗ', N'Trống'),
(N'Ford Everest', '51A-888.88', N'7 Chỗ', N'Đang di chuyển'),
(N'Hyundai SantaFe', '51A-999.99', N'7 Chỗ', N'Trống'),
(N'Kia Sorento', '51A-101.01', N'7 Chỗ', N'Trống'),
(N'VinFast Fadil', '43A-121.21', N'4 Chỗ', N'Trống'),
(N'VinFast Lux A', '43A-232.32', N'4 Chỗ', N'Đang di chuyển'),
(N'Toyota Fortuner', '43A-343.43', N'7 Chỗ', N'Trống'),
(N'Honda CR-V', '43A-454.54', N'7 Chỗ', N'Trống'),
(N'Mazda CX-5', '43A-565.65', N'7 Chỗ', N'Đang bảo trì'),
(N'Suzuki Ertiga', '15A-676.76', N'7 Chỗ', N'Trống'),
(N'Peugeot 3008', '15A-787.87', N'7 Chỗ', N'Trống'),
(N'Hyundai Grand i10', '15A-898.98', N'4 Chỗ', N'Trống'),
(N'Kia Morning', '15A-909.09', N'4 Chỗ', N'Đang di chuyển'),
(N'Toyota Camry', '15A-112.12', N'4 Chỗ', N'Trống');

-- Thêm 20 UuDai 
INSERT INTO UuDai (TenUuDai, MucGiam, DieuKien, HanSuDung, SoLuong, MaQuanTriVien) VALUES
(N'Giảm giá Tân binh 1', 50000, N'Dành cho khách hàng mới', '2026-12-31', 100, 1),
(N'Khuyến mãi Hè', 30000, N'Áp dụng chuyến > 10km', '2026-08-31', 50, 2),
(N'Lễ tình nhân', 40000, N'Áp dụng ngày 14/2', '2027-02-14', 200, 3),
(N'Giảm giá Đêm', 20000, N'Đặt xe sau 22h', '2026-12-31', 500, 4),
(N'Khách VIP Đồng', 15000, N'Thành viên Đồng', '2026-12-31', 1000, 5),
(N'Khách VIP Bạc', 25000, N'Thành viên Bạc', '2026-12-31', 800, 6),
(N'Khách VIP Vàng', 50000, N'Thành viên Vàng', '2026-12-31', 300, 7),
(N'Khách VIP Kim Cương', 100000, N'Thành viên Kim Cương', '2026-12-31', 100, 8),
(N'Mưa tháng 7', 10000, N'Áp dụng mùa mưa', '2026-07-31', 2000, 9),
(N'Quốc Khánh 2/9', 50000, N'Áp dụng ngày 2/9', '2026-09-02', 500, 10),
(N'Tết Nguyên Đán 1', 100000, N'Dịp Tết', '2027-02-10', 1000, 11),
(N'Tết Nguyên Đán 2', 50000, N'Dịp Tết (chuyến ngắn)', '2027-02-10', 1000, 12),
(N'Giảm giá Sinh nhật', 50000, N'Tháng sinh nhật', '2026-12-31', 500, 13),
(N'Chuyến đi sân bay', 100000, N'Điểm đến/đón là Sân bay', '2026-12-31', 300, 14),
(N'Ưu đãi giờ vàng 1', 15000, N'Từ 9h - 11h', '2026-06-30', 400, 15),
(N'Ưu đãi giờ vàng 2', 20000, N'Từ 14h - 16h', '2026-06-30', 400, 16),
(N'Thanh toán thẻ VISA', 30000, N'Chỉ áp dụng thẻ VISA', '2026-12-31', 600, 17),
(N'Thanh toán Momo', 20000, N'Chỉ áp dụng ví Momo', '2026-12-31', 800, 18),
(N'Thanh toán ZaloPay', 25000, N'Chỉ áp dụng ví ZaloPay', '2026-12-31', 800, 19),
(N'Mã tri ân', 50000, N'Khách hàng thân thiết', '2026-12-31', 100, 20);

-- Thêm 20 ChuyenXe (Đã liên kết NguoiDung và Xe)
INSERT INTO ChuyenXe (MaNguoiDung, MaXe, MaUuDai, ThoiGianBatDau, ThoiGianKetThuc, DiemDon, DiemDen, GiaGoc, TrangThai) VALUES
(1, 1, 1, '2026-04-01 08:00:00', '2026-04-01 08:30:00', N'Cầu Giấy, Hà Nội', N'Hoàn Kiếm, Hà Nội', 100000, N'Hoàn thành'),
(2, 2, 2, '2026-04-01 09:15:00', '2026-04-01 09:45:00', N'Quận 1, TP.HCM', N'Quận 7, TP.HCM', 150000, N'Hoàn thành'),
(3, 3, 3, '2026-04-02 10:00:00', '2026-04-02 10:20:00', N'Hải Châu, Đà Nẵng', N'Sơn Trà, Đà Nẵng', 80000, N'Hoàn thành'),
(4, 4, 4, '2026-04-02 22:30:00', '2026-04-02 23:00:00', N'Đống Đa, Hà Nội', N'Thanh Xuân, Hà Nội', 120000, N'Hoàn thành'),
(5, 5, 5, '2026-04-03 07:00:00', '2026-04-03 07:45:00', N'Quận 3, TP.HCM', N'Sân bay Tân Sơn Nhất', 200000, N'Hoàn thành'),
(6, 6, 6, '2026-04-03 14:00:00', '2026-04-03 14:15:00', N'Ninh Kiều, Cần Thơ', N'Bến Ninh Kiều', 50000, N'Hoàn thành'),
(7, 7, 7, '2026-04-04 18:00:00', '2026-04-04 18:50:00', N'Hai Bà Trưng, Hà Nội', N'Hà Đông, Hà Nội', 160000, N'Hoàn thành'),
(8, 8, 8, '2026-04-04 20:00:00', '2026-04-04 21:00:00', N'Tân Bình, TP.HCM', N'Thủ Đức, TP.HCM', 250000, N'Hoàn thành'),
(9, 9, 9, '2026-04-05 08:30:00', '2026-04-05 09:10:00', N'Sân bay Nội Bài', N'Cầu Giấy, Hà Nội', 300000, N'Hoàn thành'),
(10, 10, 10, '2026-04-05 15:00:00', '2026-04-05 15:20:00', N'Quận 10, TP.HCM', N'Quận 5, TP.HCM', 70000, N'Hoàn thành'),
(11, 11, 11, '2026-04-06 06:30:00', '2026-04-06 07:00:00', N'Ngũ Hành Sơn, Đà Nẵng', N'Hải Châu, Đà Nẵng', 90000, N'Hoàn thành'),
(12, 12, 12, '2026-04-06 17:15:00', '2026-04-06 18:00:00', N'Gò Vấp, TP.HCM', N'Bình Thạnh, TP.HCM', 110000, N'Hoàn thành'),
(13, 13, 13, '2026-04-07 19:00:00', '2026-04-07 19:25:00', N'Tây Hồ, Hà Nội', N'Ba Đình, Hà Nội', 85000, N'Hoàn thành'),
(14, 14, 14, '2026-04-07 11:00:00', '2026-04-07 11:40:00', N'Bến xe Miền Đông', N'Quận 1, TP.HCM', 130000, N'Hoàn thành'),
(15, 15, 15, '2026-04-08 09:00:00', '2026-04-08 09:15:00', N'Bãi Cháy, Hạ Long', N'Hòn Gai, Hạ Long', 60000, N'Hoàn thành'),
(16, 16, 16, '2026-04-08 14:30:00', '2026-04-08 14:45:00', N'Vinhomes Central Park', N'Landmark 81', 40000, N'Hoàn thành'),
(17, 17, 17, '2026-04-09 21:00:00', '2026-04-09 21:30:00', N'Phố cổ Hội An', N'Biển An Bàng', 100000, N'Hoàn thành'),
(18, 18, 18, '2026-04-09 13:00:00', '2026-04-09 13:20:00', N'Sân bay Cam Ranh', N'TP Nha Trang', 350000, N'Hoàn thành'),
(19, 19, 19, '2026-04-10 07:45:00', '2026-04-10 08:30:00', N'Thành phố Huế', N'Lăng Tự Đức', 140000, N'Hoàn thành'),
(20, 20, 20, '2026-04-10 16:00:00', '2026-04-10 16:30:00', N'Hồ Xuân Hương, Đà Lạt', N'Thung lũng Tình Yêu', 120000, N'Hoàn thành');

-- Thêm 20 DanhGia
INSERT INTO DanhGia (MaChuyenXe, SoSao, PhanHoi) VALUES
(1, 5, N'Tài xế nhiệt tình, xe sạch sẽ'),
(2, 4, N'Đi đúng giờ, tuy nhiên điều hòa hơi yếu'),
(3, 5, N'Tuyệt vời'),
(4, 3, N'Tài xế đi hơi ẩu'),
(5, 5, N'Rất hài lòng, sẽ ủng hộ tiếp'),
(6, 4, N'Xe đến đón hơi muộn 5 phút'),
(7, 5, N'Chuyến đi êm ái, tài xế thân thiện'),
(8, 2, N'Xe có mùi thuốc lá'),
(9, 5, N'Rất tốt'),
(10, 4, N'Ổn trong tầm giá'),
(11, 5, N'Tài xế cẩn thận, đường đông nhưng xử lý tốt'),
(12, 5, N'Tuyệt vời!'),
(13, 3, N'Giá hơi cao so với ứng dụng khác'),
(14, 4, N'Tài xế ít nói nhưng lái cứng'),
(15, 5, N'Chuyến đi nhanh chóng'),
(16, 5, N'Rất tiện lợi'),
(17, 4, N'Tốt'),
(18, 5, N'Không có gì để chê'),
(19, 4, N'Hài lòng'),
(20, 5, N'Vote 5 sao cho chất lượng dịch vụ');

-- Thêm 20 ThanhToan 
-- (Lưu ý: TongTienThanhToan = GiaGoc - SoTienGiam. Nếu SoTienGiam > GiaGoc, TongTien = 0)
INSERT INTO ThanhToan (MaChuyenXe, PhuongThucThanhToan, SoTienGiam, TongTienThanhToan) VALUES
(1, N'Tiền mặt', 50000, 50000),      -- Gốc 100k, Giảm 50k
(2, N'Thẻ VISA', 30000, 120000),     -- Gốc 150k, Giảm 30k
(3, N'Momo', 40000, 40000),          -- Gốc 80k, Giảm 40k
(4, N'ZaloPay', 20000, 100000),      -- Gốc 120k, Giảm 20k
(5, N'Tiền mặt', 15000, 185000),     -- Gốc 200k, Giảm 15k
(6, N'VNPay', 25000, 25000),         -- Gốc 50k, Giảm 25k
(7, N'Tiền mặt', 50000, 110000),     -- Gốc 160k, Giảm 50k
(8, N'Thẻ ATM', 100000, 150000),     -- Gốc 250k, Giảm 100k
(9, N'Momo', 10000, 290000),         -- Gốc 300k, Giảm 10k
(10, N'Tiền mặt', 50000, 20000),     -- Gốc 70k, Giảm 50k
(11, N'ZaloPay', 90000, 0),          -- Gốc 90k, Giảm 100k -> Tổng = 0
(12, N'Tiền mặt', 50000, 60000),     -- Gốc 110k, Giảm 50k
(13, N'Thẻ VISA', 50000, 35000),     -- Gốc 85k, Giảm 50k
(14, N'Momo', 100000, 30000),        -- Gốc 130k, Giảm 100k
(15, N'VNPay', 15000, 45000),        -- Gốc 60k, Giảm 15k
(16, N'Tiền mặt', 20000, 20000),     -- Gốc 40k, Giảm 20k
(17, N'Thẻ VISA', 30000, 70000),     -- Gốc 100k, Giảm 30k
(18, N'Momo', 20000, 330000),        -- Gốc 350k, Giảm 20k
(19, N'Tiền mặt', 25000, 115000),    -- Gốc 140k, Giảm 25k
(20, N'ZaloPay', 50000, 70000);      -- Gốc 120k, Giảm 50k
GO