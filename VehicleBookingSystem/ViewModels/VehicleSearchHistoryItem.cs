namespace VehicleBookingSystem.ViewModels;

public class VehicleSearchHistoryItem
{
    public DateTime At { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string FilterSummary { get; set; } = string.Empty;
}
