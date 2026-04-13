namespace VehicleBookingSystem.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int? StatusCode { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
