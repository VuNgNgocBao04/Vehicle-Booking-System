using System.ComponentModel.DataAnnotations;

namespace VehicleBookingSystem.Models;

public class Vehicle
{
    public Guid Id { get; set; }

    [Required]
    public Guid VehicleCategoryId { get; set; }
    public VehicleCategory? VehicleCategory { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(2, 60)]
    public int SeatCount { get; set; }

    [Required]
    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Transmission { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FuelType { get; set; } = string.Empty;

    [Range(100000, 100000000)]
    [DataType(DataType.Currency)]
    public decimal DailyRate { get; set; }
    public VehicleStatus Status { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(6000)]
    public string? BookingPolicyHtml { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public enum VehicleStatus
{
    Available = 1,
    Reserved = 2,
    InUse = 3,
    Maintenance = 4,
    Disabled = 5
}