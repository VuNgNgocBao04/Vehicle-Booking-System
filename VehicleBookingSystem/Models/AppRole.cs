using Microsoft.AspNetCore.Identity;

namespace VehicleBookingSystem.Models;

public class AppRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}