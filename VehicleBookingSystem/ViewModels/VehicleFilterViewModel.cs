using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class VehicleFilterViewModel
{
    [StringLength(100)]
    public string? SearchTerm { get; set; }
    public Guid? VehicleCategoryId { get; set; }
    [StringLength(100)]
    public string? Brand { get; set; }
    [Range(0, 100000000)]
    public decimal? MinDailyRate { get; set; }
    [Range(0, 100000000)]
    public decimal? MaxDailyRate { get; set; }
    public VehicleStatus? Status { get; set; }
    [Range(1, 1000)]
    public int Page { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 8;
}
