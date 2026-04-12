using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class BookingCreateViewModel
{
    [Required]
    public Guid VehicleId { get; set; }

    public string VehicleName { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Pickup Location")]
    public string PickupLocation { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Dropoff Location")]
    public string DropoffLocation { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Pickup Date")]
    [DataType(DataType.Date)]
    public DateTime PickupDateTime { get; set; }

    [Required]
    [Display(Name = "Return Date")]
    [DataType(DataType.Date)]
    public DateTime ReturnDateTime { get; set; }

    [Required]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public decimal EstimatedTotalAmount { get; set; }

    public IReadOnlyList<SelectListItem> PaymentMethodOptions { get; set; } = [];
}
