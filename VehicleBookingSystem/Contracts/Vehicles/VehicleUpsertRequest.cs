using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Resources;

namespace VehicleBookingSystem.Contracts.Vehicles;

public sealed class VehicleUpsertRequest
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.CodeRequired))]
    [StringLength(20, MinimumLength = 3)]
    [RegularExpression("^[A-Z0-9-]+$", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.CodePattern))]
    public string Code { get; init; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.VehicleCategoryRequired))]
    public Guid VehicleCategoryId { get; init; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.BrandRequired))]
    [StringLength(100)]
    public string Brand { get; init; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.ModelRequired))]
    [StringLength(100)]
    public string Model { get; init; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.LicensePlateRequired))]
    [StringLength(20)]
    [RegularExpression("^[0-9]{2}[A-Z]-[0-9]{3}\\.[0-9]{2}$", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.LicensePlatePattern))]
    public string LicensePlate { get; init; } = string.Empty;

    [Range(2, 60, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.SeatRange))]
    public int SeatCount { get; init; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.ColorRequired))]
    [StringLength(50)]
    public string Color { get; init; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.TransmissionRequired))]
    [StringLength(50)]
    public string Transmission { get; init; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.FuelTypeRequired))]
    [StringLength(50)]
    public string FuelType { get; init; } = string.Empty;

    [Range(100000, 100000000, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.DailyRateRange))]
    public decimal DailyRate { get; init; }

    public string? Description { get; init; }
    public string? BookingPolicyHtml { get; init; }
}
