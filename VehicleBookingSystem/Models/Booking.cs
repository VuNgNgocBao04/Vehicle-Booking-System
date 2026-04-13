using System.ComponentModel.DataAnnotations;

namespace VehicleBookingSystem.Models;

public class Booking
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DropoffLocation { get; set; } = string.Empty;
    public DateTime PickupDateTime { get; set; }
    public DateTime ReturnDateTime { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Payment? Payment { get; set; }
    public ICollection<BookingStatusHistory> StatusHistories { get; set; } = new List<BookingStatusHistory>();
}

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    Rejected = 4,
    Completed = 5
}

public class BookingStatusHistory
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Booking? Booking { get; set; }
    public BookingStatus? FromStatus { get; set; }
    public BookingStatus ToStatus { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public string ChangedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}