using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class BookingDraftSessionModel
{
    public Guid VehicleId { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DropoffLocation { get; set; } = string.Empty;
    public DateTime PickupDateTime { get; set; }
    public DateTime ReturnDateTime { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}
