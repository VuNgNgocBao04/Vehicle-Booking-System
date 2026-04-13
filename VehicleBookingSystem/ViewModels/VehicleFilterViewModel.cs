using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.ViewModels;

public class VehicleFilterViewModel
{
    [StringLength(100)]
    public string? SearchTerm { get; set; }
    [StringLength(200)]
    public string? DropoffLocation { get; set; }
    public Guid? VehicleCategoryId { get; set; }
    [StringLength(100)]
    public string? Brand { get; set; }
    [Range(0, 100000000)]
    public decimal? MinDailyRate { get; set; }
    [Range(0, 100000000)]
    public decimal? MaxDailyRate { get; set; }
    [Range(2, 60)]
    public int? SeatCount { get; set; }
    [StringLength(30)]
    public string SortBy { get; set; } = "priceAsc";
    [StringLength(10)]
    public string ViewMode { get; set; } = "grid";
    public VehicleStatus? Status { get; set; }
    [Range(1, 1000)]
    public int Page { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 8;
}
