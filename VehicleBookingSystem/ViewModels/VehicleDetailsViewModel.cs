using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class VehicleDetailsViewModel
{
    public Vehicle Vehicle { get; set; } = new();
    public IReadOnlyList<string> GalleryUrls { get; set; } = [];
    public IReadOnlyList<VehicleAvailabilitySlotViewModel> AvailabilitySlots { get; set; } = [];
}

public class VehicleAvailabilitySlotViewModel
{
    public DateOnly Date { get; set; }
    public bool IsAvailable { get; set; }
}
