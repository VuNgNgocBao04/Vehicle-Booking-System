namespace VehicleBookingSystem.Models;

public class VehicleCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}