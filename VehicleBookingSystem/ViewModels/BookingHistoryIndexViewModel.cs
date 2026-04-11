using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class BookingHistoryIndexViewModel
{
    public string? SearchTerm { get; set; }
    public BookingStatus? Status { get; set; }
    public PagedResult<Booking> Bookings { get; set; } = new();
}
