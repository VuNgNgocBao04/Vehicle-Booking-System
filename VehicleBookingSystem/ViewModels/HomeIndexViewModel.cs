namespace VehicleBookingSystem.ViewModels;

public class HomeIndexViewModel
{
    public IReadOnlyList<HomeFeaturedVehicleViewModel> FeaturedVehicles { get; set; } = [];
    public IReadOnlyList<HomeCategoryViewModel> Categories { get; set; } = [];
}

public class HomeFeaturedVehicleViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal DailyRate { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class HomeCategoryViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AvailableCount { get; set; }
}
