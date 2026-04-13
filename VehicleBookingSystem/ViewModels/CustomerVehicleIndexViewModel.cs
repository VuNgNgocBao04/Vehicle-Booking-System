using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class CustomerVehicleIndexViewModel
{
    public VehicleFilterViewModel Filter { get; set; } = new();
    public PagedResult<Vehicle> Vehicles { get; set; } = new();
    public IReadOnlyList<SelectListItem> CategoryOptions { get; set; } = [];
    public IReadOnlyList<string> BrandOptions { get; set; } = [];
    public decimal PriceRangeMin { get; set; }
    public decimal PriceRangeMax { get; set; }
    public IReadOnlyList<VehicleSearchHistoryItem> SearchHistory { get; set; } = [];
}
