using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class VehicleFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "Vehicle Category")]
    public Guid VehicleCategoryId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 3)]
    [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "Code must contain uppercase letters, numbers, or dash only.")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [RegularExpression("^[A-Za-z0-9.-]+$", ErrorMessage = "License plate format is invalid.")]
    [Display(Name = "License Plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(2, 60)]
    [Display(Name = "Seat Count")]
    public int SeatCount { get; set; }

    [Required]
    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Transmission { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Fuel Type")]
    public string FuelType { get; set; } = string.Empty;

    [Range(100000, 100000000)]
    [Display(Name = "Daily Rate")]
    public decimal DailyRate { get; set; }

    [Display(Name = "Status")]
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    [Url]
    [StringLength(500)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
