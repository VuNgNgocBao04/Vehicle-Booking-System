using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Validation;
using VehicleBookingSystem.Resources;

namespace VehicleBookingSystem.Contracts.Bookings;

[EndDateAfterStartDate(nameof(PickupDateTime), nameof(ReturnDateTime))]
public sealed class BookingCreateRequest
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.VehicleRequired))]
    public Guid VehicleId { get; init; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.PickupLocationRequired))]
    [StringLength(200, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength200))]
    public string PickupLocation { get; init; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.DropoffLocationRequired))]
    [StringLength(200, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.StringLength200))]
    public string DropoffLocation { get; init; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.Required))]
    [DataType(DataType.DateTime)]
    public DateTime PickupDateTime { get; init; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.Required))]
    [DataType(DataType.DateTime)]
    public DateTime ReturnDateTime { get; init; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.PaymentMethodRequired))]
    public PaymentMethod PaymentMethod { get; init; }
}
