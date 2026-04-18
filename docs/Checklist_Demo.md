PHẦN 1 - KIỂM TRA TỔNG THỂ DỰ ÁN (100 điểm)

1. Ý tưởng đề tài phù hợp đồ án CNTT: 9/10
Hệ thống đặt xe trực tuyến là đề tài chuẩn, đủ nghiệp vụ người dùng và quản trị.
2. Độ khó kỹ thuật: 8/10
Có MVC + API + Identity + phân quyền + upload + sanitize + localization + concurrency.
3. Tính thực tế/ứng dụng: 9/10
Use case sát thực tế thuê xe.
4. Frontend hoàn thiện: 8/10
UI khá tốt, responsive và có dashboard, nhưng cần polish demo flow đồng nhất hơn.
5. Backend hoàn thiện: 8.5/10
Service layer rõ, API response chuẩn, xử lý lỗi khá ổn.
6. Database thiết kế chuẩn: 8/10
Có PK/FK/index/unique/rowversion; vẫn cần dữ liệu demo “đậm” hơn.
7. CRUD đầy đủ: 8/10
Vehicle và booking rất ổn; thiếu cảm giác “full” ở vài thực thể phụ.
8. Phân quyền User/Admin: 9/10
Rõ ràng trên MVC và API.
9. Bảo mật cơ bản: 8/10
Có AntiForgery, rate limit login, cookie secure, sanitize, lockout.
10. Deploy/demo chạy thật: 6.5/10
Hiện mới thấy profile local và hướng dẫn local, chưa có bằng chứng public deployment.

Tổng điểm: 82/100
Kết luận: Có thể bảo vệ, nhưng nên nâng lên khoảng 86-90 nếu xử lý nhanh mục deploy + dữ liệu demo + tài liệu kiểm thử.

PHẦN 2 - CHECKLIST DEMO BẢO VỆ

A. Trang chủ: ✔ Đã có
B. Đăng ký/đăng nhập: ✔ Đã có
C. Quên mật khẩu: ⚠ Thiếu (không bắt buộc nếu GV không yêu cầu, nhưng rất hay bị hỏi)
D. Dashboard: ✔ Đã có
E. CRUD dữ liệu chính: ✔ Đã có (xe, booking)
F. Tìm kiếm/lọc/phân trang: ✔ Đã có
G. Upload ảnh/file: ✔ Đã có
H. Phân quyền User/Admin: ✔ Đã có
I. Responsive mobile: ✔ Đã có
J. Báo lỗi validation: ✔ Đã có
K. Log out: ✔ Đã có
L. Demo database có dữ liệu thật: ⚠ Thiếu bằng chứng mạnh (seed có, nhưng SQL nộp chủ yếu là schema migration)
M. Có case test lỗi khi thao tác: ✔ Đã có (test âm + xử lý conflict)

Mục cần làm gấp trong checklist:
❌ Cần làm gấp: Có link demo public hoặc video demo chạy thực tế end-to-end.

PHẦN 3 - REVIEW CODE

1. Cấu trúc thư mục: Khá chuẩn cho đồ án MVC + API + Services + Tests.
2. Clean code: Khá ổn, tách service tốt, naming rõ.
3. Tách layer: Có tách Controller/Service/Data tốt, chưa thấy Repository riêng nhưng chấp nhận được ở đồ án.
4. Comment: Vừa đủ, không rối.
5. Lặp code: Có lặp nhỏ ở mapping/status message, chưa nghiêm trọng.
6. Hardcode nguy hiểm: Không thấy lộ secret cứng rõ ràng; có nhắc dùng secret/env đúng hướng.
7. Bug tiềm ẩn:
-Chưa có luồng quên mật khẩu, hội đồng hay hỏi “mất quyền truy cập thì sao”.
-Một số thao tác admin booking đặt transaction + concurrency tốt, nhưng nên demo rõ case tranh chấp để ghi điểm.
-Dễ mở rộng: Tương đối tốt, đặc biệt ở service layer và contract API.

PHẦN 4 - KIỂM TRA DATABASE

Bảng dữ liệu: Đủ cho core flow (Users/Roles/Vehicles/Bookings/Payments/StatusHistory).
PK/FK: Chuẩn, có ràng buộc rõ.
Quan hệ bảng: Đúng nghiệp vụ chính, có one-to-one Payment-Booking, one-to-many booking history.
Dữ liệu mẫu demo: Có seed bằng code, nhưng SQL script chưa thể hiện dữ liệu mẫu giàu kịch bản.
Dữ liệu fake hợp lý: Có dữ liệu xe mẫu cơ bản, nên thêm khách hàng/booking nhiều trạng thái.
Chuẩn hóa: Tốt ở mức đồ án, chưa thấy lỗi nghiêm trọng.
Thiếu bảng quan trọng: Không thiếu bảng cốt lõi, nhưng có thể bổ sung bảng audit/log hoạt động nếu muốn tăng điểm.
