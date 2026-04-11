using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class AdminBookingIndexViewModel
{
    public string? SearchTerm { get; set; }
    public BookingStatus? Status { get; set; }
    public PagedResult<Booking> Bookings { get; set; } = new();
}
