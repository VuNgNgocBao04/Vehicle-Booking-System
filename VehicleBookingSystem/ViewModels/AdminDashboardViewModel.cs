namespace VehicleBookingSystem.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalVehicles { get; set; }
    public int BookingsToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public IReadOnlyList<AdminMetricPointViewModel> RevenueByMonth { get; set; } = [];
    public IReadOnlyList<AdminPiePointViewModel> BookingByCategory { get; set; } = [];
    public IReadOnlyList<AdminRecentBookingItemViewModel> RecentPendingBookings { get; set; } = [];
    public IReadOnlyList<AdminMaintenanceVehicleItemViewModel> MaintenanceVehicles { get; set; } = [];
}

public class AdminMetricPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class AdminPiePointViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class AdminRecentBookingItemViewModel
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public DateTime PickupDateTime { get; set; }
    public decimal TotalAmount { get; set; }
}

public class AdminMaintenanceVehicleItemViewModel
{
    public Guid VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
}
