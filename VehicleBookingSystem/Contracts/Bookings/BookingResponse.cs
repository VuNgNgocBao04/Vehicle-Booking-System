using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.Contracts.Bookings;

public sealed class BookingResponse
{
    public Guid Id { get; init; }
    public string BookingCode { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public Guid VehicleId { get; init; }
    public string? VehicleDisplayName { get; init; }
    public DateTime PickupDateTime { get; init; }
    public DateTime ReturnDateTime { get; init; }
    public string PickupLocation { get; init; } = string.Empty;
    public string DropoffLocation { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public BookingStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
