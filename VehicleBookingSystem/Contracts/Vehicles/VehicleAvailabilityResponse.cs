namespace VehicleBookingSystem.Contracts.Vehicles;

public sealed class VehicleAvailabilityResponse
{
    public Guid VehicleId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsAvailable { get; init; }
    public string Message { get; init; } = string.Empty;
}
