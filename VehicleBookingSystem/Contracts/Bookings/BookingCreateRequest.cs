using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Validation;

namespace VehicleBookingSystem.Contracts.Bookings;

[EndDateAfterStartDate(nameof(PickupDateTime), nameof(ReturnDateTime))]
public sealed class BookingCreateRequest
{
    [Required(ErrorMessage = "Mã xe là bắt buộc.")]
    public Guid VehicleId { get; init; }

    [Required(ErrorMessage = "Điểm nhận xe là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Điểm nhận xe tối đa 200 ký tự.")]
    public string PickupLocation { get; init; } = string.Empty;

    [Required(ErrorMessage = "Điểm trả xe là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Điểm trả xe tối đa 200 ký tự.")]
    public string DropoffLocation { get; init; } = string.Empty;

    [Required(ErrorMessage = "Ngày nhận xe là bắt buộc.")]
    [DataType(DataType.DateTime)]
    public DateTime PickupDateTime { get; init; }

    [Required(ErrorMessage = "Ngày trả xe là bắt buộc.")]
    [DataType(DataType.DateTime)]
    public DateTime ReturnDateTime { get; init; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
    public PaymentMethod PaymentMethod { get; init; }
}
