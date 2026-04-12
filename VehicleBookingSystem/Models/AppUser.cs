using Microsoft.AspNetCore.Identity;

namespace VehicleBookingSystem.Models;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}