PHẦN 5 - BÁO CÁO ĐỒ ÁN CHUẨN TRƯỜNG (BẢN DÙNG NGAY)
Nội dung dưới đây đã bám theo mẫu bìa + quy định trình bày bạn gửi (UTT), viết theo văn phong học thuật và để trống các thông tin cá nhân để bạn chỉnh nhanh.

1 Quy chuẩn định dạng khi dàn trang Word
Khổ giấy: A4, in 1 mặt, đóng bìa mềm.
Lề trang: Trên 2.5 cm, Dưới 2.5 cm, Trái 3 cm, Phải 2 cm.
Đánh số trang: Giữa phía trên đầu trang.
Font toàn văn: Times New Roman, 13pt, giãn dòng 1.2, căn đều hai bên, thụt đầu dòng 1 cm.
Chương: 18pt, Bold, căn giữa, Before 0pt, After 12pt.
Mục cấp 2 (1.1, 1.2…): 16pt, Bold, căn trái, Before 6pt, After 6pt.
Tiểu mục cấp 3 (1.1.1…): 14pt, Bold, căn trái, Before 6pt, After 6pt.
Tên bảng: đặt phía trên bảng; tên hình: đặt phía dưới hình; đánh số theo chương (Bảng 3.1, Hình 2.2…).

2) Mẫu nội dung hoàn chỉnh báo cáo
TRANG BÌA (căn giữa)
TRƯỜNG ĐẠI HỌC GIAO THÔNG VẬN TẢI
KHOA CÔNG NGHỆ THÔNG TIN

ĐỒ ÁN TỐT NGHIỆP

ĐỀ TÀI
XÂY DỰNG HỆ THỐNG QUẢN LÝ VÀ ĐẶT XE TRỰC TUYẾN
(VEHICLE BOOKING SYSTEM)

Giảng viên hướng dẫn: ………………………
Sinh viên thực hiện: ………………………
Lớp: ………………………
Mã sinh viên: ………………………

Hà Nội - 2026

LỜI CAM ĐOAN
Tôi xin cam đoan nội dung đồ án tốt nghiệp với đề tài “Xây dựng hệ thống quản lý và đặt xe trực tuyến” là kết quả nghiên cứu, thiết kế và triển khai của cá nhân/nhóm chúng tôi dưới sự hướng dẫn của giảng viên hướng dẫn.
Các số liệu, kết quả, hình ảnh và nhận xét trong báo cáo là trung thực, được tổng hợp từ quá trình thực hiện thực tế và có trích dẫn nguồn rõ ràng đối với tài liệu tham khảo.
Tôi xin hoàn toàn chịu trách nhiệm trước Nhà trường về tính trung thực và tính pháp lý của nội dung báo cáo này.

Hà Nội, ngày … tháng … năm 2026
Sinh viên thực hiện
(Ký và ghi rõ họ tên)

LỜI CẢM ƠN
Trước hết, chúng tôi xin gửi lời cảm ơn chân thành đến Ban Giám hiệu Trường Đại học Giao thông Vận tải, quý thầy cô Khoa Công nghệ Thông tin đã trang bị cho chúng tôi nền tảng kiến thức và môi trường học tập nghiêm túc trong suốt quá trình học tập.
Đặc biệt, chúng tôi xin trân trọng cảm ơn giảng viên hướng dẫn đã tận tình định hướng đề tài, góp ý chuyên môn và hỗ trợ chúng tôi trong suốt quá trình triển khai đồ án.
Chúng tôi cũng xin cảm ơn gia đình, bạn bè đã động viên, chia sẻ và hỗ trợ để nhóm hoàn thành đề tài đúng tiến độ.
Do giới hạn thời gian và kinh nghiệm thực tế, báo cáo khó tránh khỏi thiếu sót. Chúng tôi rất mong nhận được ý kiến đóng góp của quý thầy cô để tiếp tục hoàn thiện sản phẩm.

Nhóm sinh viên thực hiện

MỤC LỤC
LỜI CAM ĐOAN
LỜI CẢM ƠN
DANH MỤC CÁC TỪ VIẾT TẮT
DANH MỤC BẢNG BIỂU
DANH MỤC HÌNH ẢNH
MỞ ĐẦU
CHƯƠNG 1. TỔNG QUAN CÔNG NGHỆ
1.1. Tổng quan đề tài
1.2. Công nghệ backend
1.3. Công nghệ frontend
1.4. Công nghệ cơ sở dữ liệu
1.5. Công nghệ kiểm thử
CHƯƠNG 2. PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG
2.1. Mô tả bài toán
2.2. Yêu cầu chức năng và phi chức năng
2.3. Phân tích tác nhân và Use Case
2.4. Thiết kế cơ sở dữ liệu
2.5. Thiết kế kiến trúc và luồng xử lý
CHƯƠNG 3. XÂY DỰNG VÀ CÀI ĐẶT
3.1. Môi trường phát triển
3.2. Cài đặt hệ thống
3.3. Xây dựng các chức năng chính
3.4. Giao diện và triển khai tính năng
CHƯƠNG 4. KIỂM THỬ VÀ ĐÁNH GIÁ
4.1. Mục tiêu kiểm thử
4.2. Kịch bản và dữ liệu kiểm thử
4.3. Kết quả kiểm thử
4.4. Đánh giá hệ thống
KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN
TÀI LIỆU THAM KHẢO
PHỤ LỤC

(Trong Word: dùng Heading 1/2/3 và Insert Table of Contents để tự cập nhật số trang)

DANH MỤC CÁC TỪ VIẾT TẮT
CNTT: Công nghệ thông tin
MVC: Model - View - Controller
API: Application Programming Interface
JWT: JSON Web Token
ORM: Object-Relational Mapping
CRUD: Create - Read - Update - Delete
DBMS: Database Management System
UI/UX: User Interface/User Experience
E2E: End-to-End

DANH MỤC BẢNG BIỂU
Bảng 2.1. Mô tả tác nhân hệ thống
Bảng 2.2. Yêu cầu chức năng chính
Bảng 2.3. Từ điển dữ liệu bảng Vehicles
Bảng 2.4. Từ điển dữ liệu bảng Bookings
Bảng 3.1. Cấu hình môi trường triển khai
Bảng 4.1. Danh sách test case chức năng
Bảng 4.2. Kết quả kiểm thử tổng hợp
DANH MỤC HÌNH ẢNH
Hình 1.1. Kiến trúc tổng thể hệ thống
Hình 2.1. Use Case tổng quát phía khách hàng
Hình 2.2. Use Case tổng quát phía quản trị viên
Hình 2.3. Sơ đồ ERD hệ thống
Hình 3.1. Giao diện trang chủ
Hình 3.2. Giao diện đăng ký/đăng nhập
Hình 3.3. Giao diện danh sách xe và bộ lọc
Hình 3.4. Giao diện tạo booking
Hình 3.5. Giao diện dashboard quản trị
Hình 4.1. Kết quả chạy kiểm thử tự động

MỞ ĐẦU
Trong bối cảnh chuyển đổi số diễn ra mạnh mẽ, nhu cầu đặt dịch vụ trực tuyến ngày càng phổ biến, trong đó có dịch vụ thuê xe ngắn hạn phục vụ công tác, du lịch và nhu cầu đi lại cá nhân. Việc quản lý thủ công bằng sổ sách hoặc công cụ rời rạc dễ gây sai sót trong theo dõi trạng thái xe, trùng lịch đặt và khó mở rộng vận hành.
Xuất phát từ thực tế đó, đề tài “Xây dựng hệ thống quản lý và đặt xe trực tuyến” được thực hiện nhằm xây dựng một hệ thống web cho phép khách hàng tìm kiếm xe, đặt xe theo thời gian, theo dõi trạng thái đơn đặt và hỗ trợ quản trị viên quản lý đội xe, duyệt đơn, theo dõi doanh thu.
Đồ án tập trung giải quyết các nhóm vấn đề: thiết kế mô hình dữ liệu phù hợp nghiệp vụ; xây dựng kiến trúc ứng dụng web theo mô hình MVC kết hợp API; đảm bảo kiểm tra dữ liệu đầu vào, phân quyền truy cập và khả năng kiểm thử hệ thống.
Báo cáo được tổ chức thành bốn chương: tổng quan công nghệ, phân tích thiết kế, xây dựng cài đặt và kiểm thử đánh giá; từ đó đưa ra kết luận và định hướng phát triển.

CHƯƠNG 1: TỔNG QUAN CÔNG NGHỆ
1.1. Tổng quan đề tài
Hệ thống quản lý và đặt xe trực tuyến cho phép số hóa quy trình từ tra cứu xe đến tạo đơn đặt và quản lý vòng đời booking. Hệ thống hướng tới hai nhóm người dùng: khách hàng và quản trị viên.
Khách hàng có thể tìm kiếm, lọc xe theo tiêu chí, xem chi tiết xe, tạo booking và theo dõi lịch sử đơn đặt. Quản trị viên có thể quản lý dữ liệu xe, theo dõi và xử lý booking, đồng thời xem số liệu tổng quan vận hành.

1.2. Công nghệ backend
Backend sử dụng ASP.NET Core MVC, triển khai theo mô hình phân lớp: Controller, Service, Data Access.
Entity Framework Core được sử dụng theo hướng Code First để quản lý schema thông qua migration.
ASP.NET Core Identity hỗ trợ xác thực người dùng, quản lý vai trò và phân quyền truy cập.

1.3. Công nghệ frontend
Giao diện người dùng được xây dựng bằng Razor View, Bootstrap 5, jQuery và DataTables.
Thiết kế tập trung tính trực quan và khả năng thao tác nhanh, đồng thời hỗ trợ hiển thị tốt trên desktop và mobile.
Một số thành phần biểu đồ phục vụ dashboard quản trị được tích hợp để hỗ trợ phân tích dữ liệu vận hành.

1.4. Công nghệ cơ sở dữ liệu
Hệ quản trị cơ sở dữ liệu sử dụng SQL Server. Dữ liệu được tổ chức theo các thực thể nghiệp vụ chính: danh mục xe, xe, booking, thanh toán, lịch sử trạng thái booking, cùng các bảng Identity.
Các ràng buộc dữ liệu như khóa chính, khóa ngoại, unique index được áp dụng nhằm đảm bảo toàn vẹn dữ liệu và hiệu năng truy vấn cơ bản.

1.5. Công nghệ kiểm thử
Kiểm thử sử dụng xUnit cho unit/integration test và Playwright cho smoke test giao diện.
Cách tiếp cận này giúp phát hiện sớm lỗi ở tầng nghiệp vụ và xác nhận các luồng tương tác chính của người dùng.

CHƯƠNG 2: PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG
2.1. Mô tả bài toán
Bài toán đặt ra là xây dựng một hệ thống cho phép quản lý đội xe và xử lý đặt xe trực tuyến với các yêu cầu:

Tránh xung đột lịch đặt cùng một xe trong cùng khoảng thời gian.
Quản lý trạng thái booking xuyên suốt vòng đời xử lý.
Tách quyền rõ ràng giữa khách hàng và quản trị viên.
Cung cấp báo cáo tổng quan phục vụ vận hành.
2.2. Yêu cầu chức năng và phi chức năng
2.2.1. Yêu cầu chức năng
Đăng ký, đăng nhập, đăng xuất, quản lý hồ sơ cá nhân.
Tìm kiếm, lọc, phân trang danh sách xe.
Xem chi tiết xe và chính sách đặt xe.
Tạo booking, hủy booking theo điều kiện nghiệp vụ.
Quản trị viên quản lý xe (CRUD), duyệt/từ chối/hủy booking.
Hiển thị dashboard thống kê cơ bản.
2.2.2. Yêu cầu phi chức năng
Bảo mật ở mức cơ bản: phân quyền, chống giả mạo form, kiểm tra dữ liệu đầu vào.
Tính ổn định: xử lý lỗi và kiểm soát xung đột cập nhật booking.
Khả năng mở rộng: tách service để dễ bảo trì và bổ sung tính năng.
Tính khả dụng: giao diện thân thiện, hỗ trợ responsive.
2.3. Phân tích tác nhân và Use Case
Tác nhân gồm:

Khách truy cập (Guest): xem danh sách xe, xem chi tiết, đăng ký/đăng nhập.
Khách hàng (Customer): tạo booking, xem lịch sử booking, hủy booking.
Quản trị viên (Admin): quản lý xe, quản lý booking, xem dashboard.
(Chèn Hình 2.1 và Hình 2.2 Use Case theo mẫu trường)

2.4. Thiết kế cơ sở dữ liệu
2.4.1. Các thực thể chính
VehicleCategories: thông tin danh mục xe.
Vehicles: thông tin xe, trạng thái và giá thuê theo ngày.
Bookings: thông tin đơn đặt xe.
Payments: thông tin thanh toán gắn với booking.
BookingStatusHistories: lịch sử thay đổi trạng thái đơn đặt.
2.4.2. Quan hệ dữ liệu
Một danh mục có nhiều xe.
Một xe có nhiều booking theo thời gian.
Một booking gắn một bản ghi thanh toán.
Một booking có nhiều bản ghi lịch sử trạng thái.
(Chèn Hình 2.3 ERD)

2.5. Thiết kế kiến trúc và luồng xử lý
Kiến trúc hệ thống gồm:

Tầng trình bày: Razor View + Controller.
Tầng nghiệp vụ: Service xử lý logic đặt xe, quản lý xe, tài khoản.
Tầng dữ liệu: DbContext + SQL Server.
Luồng tạo booking:

Người dùng nhập thông tin đặt xe.
Hệ thống kiểm tra hợp lệ dữ liệu và kiểm tra trùng lịch.
Tạo booking ở trạng thái chờ xử lý.
Ghi lịch sử trạng thái và thông tin thanh toán ban đầu.
Trả kết quả cho người dùng.
CHƯƠNG 3: XÂY DỰNG VÀ CÀI ĐẶT
3.1. Môi trường phát triển
Hệ điều hành: Windows
Nền tảng: .NET 9
Cơ sở dữ liệu: SQL Server
IDE: Visual Studio/VS Code
Quản lý mã nguồn: Git
3.2. Cài đặt hệ thống
Quy trình cài đặt:

Khôi phục package và build solution.
Cấu hình chuỗi kết nối và tham số môi trường.
Chạy migration hoặc script SQL để khởi tạo cơ sở dữ liệu.
Chạy ứng dụng bằng profile HTTPS.
Kiểm tra các tài khoản và dữ liệu mẫu.
3.3. Xây dựng các chức năng chính
3.3.1. Chức năng tài khoản
Đăng ký tài khoản khách hàng.
Đăng nhập và phân luồng theo vai trò.
Quản lý hồ sơ người dùng và đổi mật khẩu.
3.3.2. Chức năng khách hàng
Tìm kiếm và lọc xe theo nhiều tiêu chí.
Xem chi tiết xe và trạng thái khả dụng theo thời gian.
Tạo booking và xem lịch sử booking cá nhân.
Hủy booking theo chính sách thời gian.
3.3.3. Chức năng quản trị
Quản lý xe: thêm, sửa, xóa, xem chi tiết, xuất dữ liệu CSV.
Quản lý booking: lọc dữ liệu, xem chi tiết, duyệt, từ chối, hủy.
Theo dõi dashboard: số lượng xe, booking, doanh thu theo tháng.
3.4. Giao diện và triển khai tính năng
Hệ thống sử dụng thiết kế responsive, bố cục rõ ràng theo vai trò người dùng.
Các giao diện chính cần minh họa trong báo cáo:

Trang chủ và danh sách xe.
Trang đăng ký/đăng nhập.
Trang đặt xe và lịch sử booking.
Dashboard và trang quản trị booking/xe.
(Chèn các hình từ Hình 3.1 đến Hình 3.5)

CHƯƠNG 4: KIỂM THỬ VÀ ĐÁNH GIÁ
4.1. Mục tiêu kiểm thử
Đảm bảo các chức năng cốt lõi hoạt động đúng yêu cầu.
Xác nhận các ràng buộc nghiệp vụ quan trọng: kiểm tra trùng lịch, điều kiện hủy booking, phân quyền truy cập.
Đánh giá mức độ ổn định trước khi bảo vệ.
4.2. Kịch bản và dữ liệu kiểm thử
Nhóm kiểm thử theo hai hướng:

Kiểm thử tự động với unit/integration test cho tầng API và service.
Smoke test giao diện cho các luồng quan trọng (lọc danh sách, tạo booking, dashboard admin).
Ví dụ test case:

TC01: Tạo booking hợp lệ.
TC02: Tạo booking với thời gian không hợp lệ.
TC03: Kiểm tra quyền truy cập admin/customer.
TC04: Duyệt booking đang chờ xử lý.
TC05: Hủy booking không đúng điều kiện thời gian.
4.3. Kết quả kiểm thử
Kết quả chạy test hiện tại cho bộ test backend không bao gồm E2E:

Tổng số test: 14
Thành công: 14
Thất bại: 0
Kết quả cho thấy hệ thống đáp ứng tốt các luồng nghiệp vụ chính và chưa phát hiện lỗi nghiêm trọng trong phạm vi test hiện có.
4.4. Đánh giá hệ thống
Ưu điểm:

Kiến trúc tách lớp rõ ràng, dễ bảo trì.
Chức năng chính hoàn chỉnh theo mục tiêu đề tài.
Có phân quyền và biện pháp bảo mật cơ bản.
Có bộ kiểm thử tự động hỗ trợ xác nhận chất lượng.
Hạn chế:

Chưa tích hợp luồng quên mật khẩu đầy đủ.
Chưa có kiểm thử tải hiệu năng quy mô lớn.
Chưa tích hợp cổng thanh toán trực tuyến thực tế.
KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN
Kết luận
Đồ án đã xây dựng thành công hệ thống quản lý và đặt xe trực tuyến, đáp ứng các yêu cầu cốt lõi về nghiệp vụ, kiến trúc và kiểm thử. Hệ thống cho phép khách hàng tìm kiếm và đặt xe thuận tiện, đồng thời hỗ trợ quản trị viên quản lý dữ liệu và theo dõi vận hành hiệu quả.

Hướng phát triển
Bổ sung chức năng quên mật khẩu và xác thực email/OTP.
Tích hợp cổng thanh toán trực tuyến.
Mở rộng báo cáo phân tích nâng cao theo thời gian thực.
Triển khai hệ thống trên hạ tầng cloud và bổ sung giám sát logging tập trung.
Nâng cao kiểm thử E2E và kiểm thử hiệu năng.
TÀI LIỆU THAM KHẢO
(Trình bày theo số thứ tự trích dẫn [1], [2]… theo hướng dẫn)
[1] Microsoft, “ASP.NET Core documentation,” https://learn.microsoft.com/aspnet/core
[2] Microsoft, “Entity Framework Core documentation,” https://learn.microsoft.com/ef/core
[3] Microsoft, “Introduction to Identity on ASP.NET Core,” https://learn.microsoft.com/aspnet/core/security/authentication/identity
[4] Bootstrap Team, “Bootstrap 5 Documentation,” https://getbootstrap.com/docs/5.0
[5] jQuery Foundation, “jQuery API Documentation,” https://api.jquery.com
[6] DataTables, “DataTables Manual,” https://datatables.net/manual
[7] xUnit, “xUnit.net Documentation,” https://xunit.net
[8] Microsoft Playwright, “Playwright for .NET,” https://playwright.dev/dotnet
[9] Six Labors, “ImageSharp Documentation,” https://docs.sixlabors.com
[10] OWASP Foundation, “OWASP Top 10,” https://owasp.org/www-project-top-ten

PHỤ LỤC
Phụ lục A. Danh sách endpoint API chính
POST /api/auth/token
GET /api/vehicles
GET /api/vehicles/{id}
GET /api/vehicles/{id}/availability
POST /api/vehicles
PUT /api/vehicles/{id}
DELETE /api/vehicles/{id}
GET /api/bookings
GET /api/bookings/{id}
POST /api/bookings
PUT /api/bookings/{id}/cancel
Phụ lục B. Mô tả dữ liệu mẫu demo
Danh mục xe: Sedan, SUV, MPV, Hatchback
Trạng thái booking: Pending, Confirmed, Cancelled, Rejected, Completed
Tài khoản demo: 01 Admin + nhiều Customer theo kịch bản kiểm thử
Phụ lục C. Kịch bản demo bảo vệ
Luồng khách: tìm xe → xem chi tiết → tạo booking → xem lịch sử booking
Luồng admin: đăng nhập → duyệt booking → xem dashboard → quản lý xe
Phụ lục D. Ảnh chụp màn hình minh họa
Ảnh trang chủ
Ảnh trang danh sách xe + bộ lọc
Ảnh trang booking
Ảnh dashboard admin
Ảnh kết quả test
PHẦN 6 - SLIDE BẢO VỆ 10-15 PHÚT (12 SLIDE, ÍT CHỮ, CHUẨN HỘI ĐỒNG)

Slide 1: Giới thiệu đề tài
Xây dựng hệ thống quản lý và đặt xe trực tuyến
Sinh viên thực hiện: …
GVHD: …
Mục tiêu: số hóa quy trình đặt xe

Slide 2: Lý do chọn đề tài
Nhu cầu đặt xe online tăng
Quản lý thủ công dễ sai sót
Cần hệ thống minh bạch, dễ mở rộng

Slide 3: Mục tiêu
Quản lý xe và booking tập trung
Hỗ trợ khách hàng đặt xe nhanh
Phân quyền rõ Customer/Admin
Đảm bảo dữ liệu nhất quán

Slide 4: Công nghệ sử dụng
ASP.NET Core MVC, EF Core, SQL Server
Identity, JWT (API), FluentValidation
Bootstrap 5, jQuery, DataTables
xUnit, Playwright (smoke test)

Slide 5: Phân tích hệ thống
Tác nhân: Guest, Customer, Admin
Luồng chính: tìm xe, đặt xe, xử lý booking
Ràng buộc: không trùng lịch, chính sách hủy

Slide 6: Use Case / ERD
Use Case khách hàng
Use Case quản trị viên
ERD: Vehicles, Bookings, Payments, StatusHistory

Slide 7: Chức năng chính
Đăng ký/đăng nhập/đăng xuất
Tìm kiếm, lọc, phân trang xe
Tạo/hủy booking
Admin CRUD xe, duyệt/từ chối booking

Slide 8: Demo giao diện
Trang chủ + danh sách xe
Form tạo booking
Lịch sử booking cá nhân
Dashboard quản trị

Slide 9: Kết quả đạt được
Hoàn thành nghiệp vụ cốt lõi
Phân quyền và bảo mật cơ bản
Kiểm thử backend pass 14/14
Hệ thống chạy ổn định khi demo

Slide 10: Hạn chế
Chưa có quên mật khẩu đầy đủ
Chưa có payment gateway thực tế
Chưa kiểm thử tải chuyên sâu

Slide 11: Hướng phát triển
Quên mật khẩu + OTP/email verify
Tích hợp thanh toán online
Triển khai cloud + monitoring
Mở rộng báo cáo BI

Slide 12: Cảm ơn
Xin cảm ơn Hội đồng đã lắng nghe
Kính mời nhận xét và đặt câu hỏi

*ƯU TIÊN GẤP NẾU CÒN 3 NGÀY

Việc cần làm ngay
    -Chốt một kịch bản demo 8-10 phút không lỗi, có dữ liệu thật đa trạng thái booking.
    -Bổ sung bằng chứng deploy hoặc tối thiểu video demo full luồng có timestamp.
    -Hoàn thiện báo cáo đúng format trường và chèn ảnh thật từ hệ thống.
    -Chuẩn bị file “Q&A hội đồng” 1 trang để luyện trả lời.

Việc nên bỏ qua
    -Refactor kiến trúc lớn (đụng nhiều file, rủi ro cao).
    -Làm thêm chức năng lớn mới (payment gateway thật, chat realtime).
    -Tối ưu UI quá sâu về thẩm mỹ nếu chưa ảnh hưởng demo.

Việc tăng điểm nhanh nhất
    -Thêm luồng quên mật khẩu cơ bản hoặc ít nhất mô tả + mock màn hình + kế hoạch kỹ thuật rõ.
    -Tạo dữ liệu demo phong phú: pending/confirmed/cancelled/rejected, nhiều loại xe, nhiều user.
    -Bổ sung 5-8 test case âm trong báo cáo và quay video xử lý lỗi thực tế.
Việc hội đồng đánh giá cao nhất:
    -Trả lời chắc về quyết định kỹ thuật và trade-off.
    -Demo mượt, không lỗi, có xử lý tình huống xấu.
    -Chứng minh được tư duy kỹ sư: bảo mật, dữ liệu, kiểm thử, khả năng mở rộng.