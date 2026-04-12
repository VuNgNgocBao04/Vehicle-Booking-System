using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Validation;

namespace VehicleBookingSystem.ViewModels;

[EndDateAfterStartDate(nameof(PickupDateTime), nameof(ReturnDateTime))]
public class BookingCreateViewModel
{
    [Required(ErrorMessage = "Xe là bắt buộc.")]
    public Guid VehicleId { get; set; }

    public string VehicleName { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }

    [Required(ErrorMessage = "Điểm nhận xe là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Điểm nhận xe tối đa 200 ký tự.")]
    [Display(Name = "Điểm nhận xe")]
    public string PickupLocation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Điểm trả xe là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Điểm trả xe tối đa 200 ký tự.")]
    [Display(Name = "Điểm trả xe")]
    public string DropoffLocation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày nhận xe là bắt buộc.")]
    [Display(Name = "Ngày nhận xe")]
    [DataType(DataType.Date)]
    public DateTime PickupDateTime { get; set; }

    [Required(ErrorMessage = "Ngày trả xe là bắt buộc.")]
    [Display(Name = "Ngày trả xe")]
    [DataType(DataType.Date)]
    public DateTime ReturnDateTime { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
    [Display(Name = "Phương thức thanh toán")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public decimal EstimatedTotalAmount { get; set; }

    public IReadOnlyList<SelectListItem> PaymentMethodOptions { get; set; } = [];
}
