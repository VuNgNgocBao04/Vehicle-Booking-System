namespace VehicleBookingSystem.Models;

public class Payment
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Booking? Booking { get; set; }
    public decimal PaidAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime? PaidAt { get; set; }
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    CreditCard = 3,
    EWallet = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}