using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Validation;
using VehicleBookingSystem.Resources;

namespace VehicleBookingSystem.ViewModels;

[EndDateAfterStartDate(nameof(PickupDateTime), nameof(ReturnDateTime))]
public class BookingCreateViewModel
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.VehicleRequired))]
    public Guid VehicleId { get; set; }

    public string VehicleName { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.PickupLocationRequired))]
    [StringLength(200, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength200))]
    [Display(Name = "Điểm nhận xe")]
    public string PickupLocation { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.DropoffLocationRequired))]
    [StringLength(200, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength200))]
    [Display(Name = "Điểm trả xe")]
    public string DropoffLocation { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.Required))]
    [Display(Name = "Ngày nhận xe")]
    [DataType(DataType.Date)]
    public DateTime PickupDateTime { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.Required))]
    [Display(Name = "Ngày trả xe")]
    [DataType(DataType.Date)]
    public DateTime ReturnDateTime { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.PaymentMethodRequired))]
    [Display(Name = "Phương thức thanh toán")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public decimal EstimatedTotalAmount { get; set; }

    public IReadOnlyList<SelectListItem> PaymentMethodOptions { get; set; } = [];
}
