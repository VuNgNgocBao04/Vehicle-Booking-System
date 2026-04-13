using VehicleBookingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VehicleBookingSystem.ViewModels;

public class AdminBookingIndexViewModel
{
    public string? SearchTerm { get; set; }
    public BookingStatus? Status { get; set; }
    public PagedResult<Booking> Bookings { get; set; } = new();
    public IReadOnlyList<SelectListItem> VehicleOptions { get; set; } = [];
}
