using System.Security.Claims;
using VehicleBookingSystem.Contracts.Bookings;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public sealed record BookingCommandResult(bool Succeeded, string? ErrorMessage = null, Guid? BookingId = null);

public interface IBookingService
{
    Task<BookingCreateViewModel?> BuildCreateViewModelAsync(Guid vehicleId, BookingDraftSessionModel? draft, AppUser? currentUser, CancellationToken cancellationToken = default);
    Task<BookingCommandResult> CreateAsync(BookingCreateViewModel model, Guid userId, string? changedBy, CancellationToken cancellationToken = default);
    Task<Booking?> GetSuccessAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default);
    Task<BookingHistoryIndexViewModel> BuildHistoryAsync(Guid userId, string? searchTerm, BookingStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Booking?> GetDetailsAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default);
    Task<BookingCommandResult> CancelAsync(Guid bookingId, Guid userId, string? changedBy, CancellationToken cancellationToken = default);
    Task<(bool IsAvailable, string Message, decimal TotalAmount)> CheckAvailabilityAsync(Guid vehicleId, DateTime pickupDateTime, DateTime returnDateTime, CancellationToken cancellationToken = default);
    Task<PagedResult<Booking>> GetApiBookingsAsync(Guid? userId, bool isAdmin, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Booking?> GetApiBookingAsync(Guid bookingId, Guid? userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<BookingCommandResult> CancelApiAsync(Guid bookingId, Guid? userId, bool isAdmin, string? changedBy, CancellationToken cancellationToken = default);
}