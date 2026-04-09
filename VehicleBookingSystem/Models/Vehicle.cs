namespace VehicleBookingSystem.Models;

public class Vehicle
{
    public Guid Id { get; set; }
    public Guid VehicleCategoryId { get; set; }
    public VehicleCategory? VehicleCategory { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }
    public VehicleStatus Status { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
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