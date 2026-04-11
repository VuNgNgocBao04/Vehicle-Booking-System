using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class AdminVehicleIndexViewModel
{
    public VehicleFilterViewModel Filter { get; set; } = new();
    public PagedResult<Vehicle> Vehicles { get; set; } = new();
    public IReadOnlyList<SelectListItem> CategoryOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];
}
