using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.Contracts.Vehicles;

public sealed class VehicleResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public Guid VehicleCategoryId { get; init; }
    public string? VehicleCategoryName { get; init; }
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string LicensePlate { get; init; } = string.Empty;
    public int SeatCount { get; init; }
    public string Color { get; init; } = string.Empty;
    public string Transmission { get; init; } = string.Empty;
    public string FuelType { get; init; } = string.Empty;
    public decimal DailyRate { get; init; }
    public VehicleStatus Status { get; init; }
    public string? ImageUrl { get; init; }
    public IReadOnlyList<string> GalleryUrls { get; init; } = [];
    public string? Description { get; init; }
    public string? BookingPolicyHtml { get; init; }
}
