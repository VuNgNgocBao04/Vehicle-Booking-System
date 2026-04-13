using System.Globalization;

namespace VehicleBookingSystem.Resources;

public static class ValidationMessages
{
    private static bool IsEnglish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);

    public static string Required => IsEnglish ? "The {0} field is required." : "{0} là bắt buộc.";
    public static string EmailInvalid => IsEnglish ? "The {0} field must be a valid email address." : "{0} không đúng định dạng.";
    public static string PhoneInvalid => IsEnglish ? "The {0} field must be a valid phone number." : "{0} không hợp lệ.";
    public static string StringLength100 => IsEnglish ? "The {0} field must be at most 100 characters." : "{0} tối đa 100 ký tự.";
    public static string StringLength20 => IsEnglish ? "The {0} field must be at most 20 characters." : "{0} tối đa 20 ký tự.";
    public static string StringLength50 => IsEnglish ? "The {0} field must be at most 50 characters." : "{0} tối đa 50 ký tự.";
    public static string StringLength200 => IsEnglish ? "The {0} field must be at most 200 characters." : "{0} tối đa 200 ký tự.";
    public static string StringLength300 => IsEnglish ? "The {0} field must be at most 300 characters." : "{0} tối đa 300 ký tự.";
    public static string StringLength500 => IsEnglish ? "The {0} field must be at most 500 characters." : "{0} tối đa 500 ký tự.";
    public static string StringLength2000 => IsEnglish ? "The {0} field must be at most 2000 characters." : "{0} tối đa 2000 ký tự.";
    public static string StringLength6000 => IsEnglish ? "The {0} field must be at most 6000 characters." : "{0} tối đa 6000 ký tự.";
    public static string PasswordMin8 => IsEnglish ? "The password must be at least 8 characters." : "Mật khẩu phải có ít nhất 8 ký tự.";
    public static string PasswordComplexity => IsEnglish ? "The password must contain uppercase, lowercase, and a number." : "Mật khẩu cần có chữ hoa, chữ thường và số.";
    public static string ConfirmPasswordRequired => IsEnglish ? "Please confirm the password." : "Vui lòng nhập lại mật khẩu.";
    public static string PasswordMismatch => IsEnglish ? "The confirmation password does not match." : "Mật khẩu xác nhận không khớp.";
    public static string AgeMinimum18 => IsEnglish ? "The user must be at least 18 years old." : "Người dùng phải từ 18 tuổi trở lên.";
    public static string CodeRequired => IsEnglish ? "The vehicle code is required." : "Mã xe là bắt buộc.";
    public static string CodePattern => IsEnglish ? "The vehicle code may only contain uppercase letters, numbers, and hyphens." : "Mã xe chỉ gồm chữ in hoa, số hoặc dấu gạch ngang.";
    public static string LicensePlatePattern => IsEnglish ? "The license plate format is invalid (e.g. 30A-123.45)." : "Biển số không đúng định dạng (VD: 30A-123.45).";
    public static string SeatRange => IsEnglish ? "The seat count must be between 2 and 60." : "Số chỗ phải từ 2 đến 60.";
    public static string DailyRateRange => IsEnglish ? "The daily rate is invalid." : "Giá thuê/ngày không hợp lệ.";
    public static string DateRangeInvalid => IsEnglish ? "The end date must be after the start date." : "Ngày kết thúc phải sau ngày bắt đầu.";
    public static string DateRangeConfigInvalid => IsEnglish ? "Invalid date validation configuration." : "Cấu hình xác thực ngày không hợp lệ.";
    public static string BookingPickupDateInvalid => IsEnglish ? "The pickup date is invalid." : "Ngày nhận xe không hợp lệ.";
    public static string BookingReturnAfterPickup => IsEnglish ? "The return date must be after the pickup date." : "Ngày trả xe phải sau ngày nhận xe.";
    public static string PickupLocationRequired => IsEnglish ? "Pickup location is required." : "Điểm nhận xe là bắt buộc.";
    public static string DropoffLocationRequired => IsEnglish ? "Dropoff location is required." : "Điểm trả xe là bắt buộc.";
    public static string VehicleRequired => IsEnglish ? "A vehicle is required." : "Xe là bắt buộc.";
    public static string PaymentMethodRequired => IsEnglish ? "Payment method is required." : "Phương thức thanh toán là bắt buộc.";
    public static string VehicleCategoryRequired => IsEnglish ? "Vehicle category is required." : "Danh mục xe là bắt buộc.";
    public static string BrandRequired => IsEnglish ? "Vehicle brand is required." : "Hãng xe là bắt buộc.";
    public static string ModelRequired => IsEnglish ? "Vehicle model is required." : "Mẫu xe là bắt buộc.";
    public static string LicensePlateRequired => IsEnglish ? "License plate is required." : "Biển số xe là bắt buộc.";
    public static string ColorRequired => IsEnglish ? "Vehicle color is required." : "Màu xe là bắt buộc.";
    public static string TransmissionRequired => IsEnglish ? "Transmission type is required." : "Loại hộp số là bắt buộc.";
    public static string FuelTypeRequired => IsEnglish ? "Fuel type is required." : "Loại nhiên liệu là bắt buộc.";
    public static string PickUpReturnDateRequired => IsEnglish ? "Dates are required." : "Ngày nhận/trả xe là bắt buộc.";
}