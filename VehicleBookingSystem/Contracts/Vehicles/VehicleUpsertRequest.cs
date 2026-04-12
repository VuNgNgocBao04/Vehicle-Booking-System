using System.ComponentModel.DataAnnotations;

namespace VehicleBookingSystem.Contracts.Vehicles;

public sealed class VehicleUpsertRequest
{
    [Required(ErrorMessage = "Mã xe là bắt buộc.")]
    [StringLength(20, MinimumLength = 3)]
    [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "Mã xe chỉ gồm chữ in hoa, số và dấu gạch ngang.")]
    public string Code { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mã danh mục xe là bắt buộc.")]
    public Guid VehicleCategoryId { get; init; }

    [Required(ErrorMessage = "Hãng xe là bắt buộc.")]
    [StringLength(100)]
    public string Brand { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mẫu xe là bắt buộc.")]
    [StringLength(100)]
    public string Model { get; init; } = string.Empty;

    [Required(ErrorMessage = "Biển số là bắt buộc.")]
    [StringLength(20)]
    [RegularExpression("^[0-9]{2}[A-Z]-[0-9]{3}\\.[0-9]{2}$", ErrorMessage = "Biển số không đúng định dạng (VD: 30A-123.45).")]
    public string LicensePlate { get; init; } = string.Empty;

    [Range(2, 60, ErrorMessage = "Số chỗ phải từ 2 đến 60.")]
    public int SeatCount { get; init; }

    [Required(ErrorMessage = "Màu xe là bắt buộc.")]
    [StringLength(50)]
    public string Color { get; init; } = string.Empty;

    [Required(ErrorMessage = "Loại hộp số là bắt buộc.")]
    [StringLength(50)]
    public string Transmission { get; init; } = string.Empty;

    [Required(ErrorMessage = "Loại nhiên liệu là bắt buộc.")]
    [StringLength(50)]
    public string FuelType { get; init; } = string.Empty;

    [Range(100000, 100000000, ErrorMessage = "Giá thuê/ngày không hợp lệ.")]
    public decimal DailyRate { get; init; }

    public string? Description { get; init; }
    public string? BookingPolicyHtml { get; init; }
}
