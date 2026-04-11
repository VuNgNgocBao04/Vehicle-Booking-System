using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class VehicleFilterViewModel
{
    public string? SearchTerm { get; set; }
    public Guid? VehicleCategoryId { get; set; }
    public string? Brand { get; set; }
    public decimal? MinDailyRate { get; set; }
    public decimal? MaxDailyRate { get; set; }
    public VehicleStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 8;
}
