using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Extensions;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public sealed class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BookingCreateViewModel?> BuildCreateViewModelAsync(Guid vehicleId, BookingDraftSessionModel? draft, AppUser? currentUser, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == vehicleId && item.Status == VehicleStatus.Available, cancellationToken);

        if (vehicle is null)
        {
            return null;
        }

        var model = new BookingCreateViewModel
        {
            VehicleId = vehicle.Id,
            VehicleName = $"{vehicle.Brand} {vehicle.Model}",
            DailyRate = vehicle.DailyRate,
            PickupLocation = draft?.PickupLocation ?? string.Empty,
            DropoffLocation = draft?.DropoffLocation ?? string.Empty,
            PickupDateTime = draft?.PickupDateTime.Date > DateTime.UtcNow.Date ? draft.PickupDateTime.Date : DateTime.UtcNow.Date.AddDays(1),
            ReturnDateTime = draft?.ReturnDateTime.Date > DateTime.UtcNow.Date ? draft.ReturnDateTime.Date : DateTime.UtcNow.Date.AddDays(2),
            PaymentMethod = draft?.PaymentMethod ?? PaymentMethod.Cash,
            PaymentMethodOptions = BuildPaymentMethodOptions(PaymentMethod.Cash)
        };

        if (currentUser is not null)
        {
            if (string.IsNullOrWhiteSpace(model.PickupLocation) && !string.IsNullOrWhiteSpace(currentUser.Address))
            {
                model.PickupLocation = currentUser.Address;
            }

            if (string.IsNullOrWhiteSpace(model.DropoffLocation) && !string.IsNullOrWhiteSpace(currentUser.Address))
            {
                model.DropoffLocation = currentUser.Address;
            }
        }

        model.EstimatedTotalAmount = CalculateTotal(vehicle.DailyRate, model.PickupDateTime, model.ReturnDateTime);
        model.PaymentMethodOptions = BuildPaymentMethodOptions(model.PaymentMethod);
        return model;
    }

    public async Task<BookingCommandResult> CreateAsync(BookingCreateViewModel model, Guid userId, string? changedBy, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == model.VehicleId && item.Status == VehicleStatus.Available, cancellationToken);

        if (vehicle is null)
        {
            return new BookingCommandResult(false, "Vehicle not found.");
        }

        if (model.ReturnDateTime <= model.PickupDateTime)
        {
            return new BookingCommandResult(false, "Return date must be later than pickup date.");
        }

        if (!await IsVehicleAvailableAsync(model.VehicleId, model.PickupDateTime, model.ReturnDateTime, cancellationToken))
        {
            return new BookingCommandResult(false, "Vehicle is not available in the selected period.");
        }

        var totalAmount = CalculateTotal(vehicle.DailyRate, model.PickupDateTime, model.ReturnDateTime);

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        if (!await IsVehicleAvailableAsync(model.VehicleId, model.PickupDateTime, model.ReturnDateTime, cancellationToken))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new BookingCommandResult(false, "Vehicle is not available in the selected period.");
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = GenerateBookingCode(),
            UserId = userId,
            VehicleId = model.VehicleId,
            PickupLocation = model.PickupLocation.Trim(),
            DropoffLocation = model.DropoffLocation.Trim(),
            PickupDateTime = model.PickupDateTime,
            ReturnDateTime = model.ReturnDateTime,
            TotalAmount = totalAmount,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentMethod = model.PaymentMethod,
                Status = PaymentStatus.Pending,
                PaidAmount = totalAmount
            },
            StatusHistories =
            [
                new BookingStatusHistory
                {
                    Id = Guid.NewGuid(),
                    FromStatus = null,
                    ToStatus = BookingStatus.Pending,
                    ChangedAtUtc = DateTime.UtcNow,
                    ChangedBy = changedBy ?? "Customer",
                    Note = "Booking created"
                }
            ]
        };

        _context.Bookings.Add(booking);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new BookingCommandResult(false, "Unable to create booking right now. Please try again.");
        }

        return new BookingCommandResult(true, null, booking.Id);
    }

    public async Task<Booking?> GetSuccessAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Vehicle)
            .FirstOrDefaultAsync(item => item.Id == bookingId && item.UserId == userId, cancellationToken);
    }

    public async Task<BookingHistoryIndexViewModel> BuildHistoryAsync(Guid userId, string? searchTerm, BookingStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Vehicle)
            .Where(booking => booking.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var keyword = searchTerm.Trim();
            query = query.Where(booking =>
                booking.BookingCode.Contains(keyword) ||
                (booking.Vehicle != null && booking.Vehicle.LicensePlate.Contains(keyword)));
        }

        if (status.HasValue)
        {
            query = query.Where(booking => booking.Status == status.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        page = Math.Max(1, page);
        pageSize = pageSize <= 0 ? 8 : pageSize;

        var items = await query
            .OrderByDescending(booking => booking.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new BookingHistoryIndexViewModel
        {
            SearchTerm = searchTerm,
            Status = status,
            Bookings = new PagedResult<Booking>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            }
        };
    }

    public async Task<Booking?> GetDetailsAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Vehicle)
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == bookingId && item.UserId == userId, cancellationToken);
    }

    public async Task<BookingCommandResult> CancelAsync(Guid bookingId, Guid userId, string? changedBy, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .Include(item => item.StatusHistories)
            .FirstOrDefaultAsync(item => item.Id == bookingId && item.UserId == userId, cancellationToken);

        if (booking is null)
        {
            return new BookingCommandResult(false, "Booking not found.");
        }

        if (booking.PickupDateTime <= DateTime.UtcNow.AddHours(6))
        {
            return new BookingCommandResult(false, "Booking can only be cancelled at least 6 hours before pickup time.");
        }

        var fromStatus = booking.Status;
        booking.Status = BookingStatus.Cancelled;
        booking.StatusHistories.Add(new BookingStatusHistory
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            FromStatus = fromStatus,
            ToStatus = BookingStatus.Cancelled,
            ChangedAtUtc = DateTime.UtcNow,
            ChangedBy = changedBy ?? "Customer",
            Note = "Customer cancelled booking"
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return new BookingCommandResult(true);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new BookingCommandResult(false, "Booking was updated by another request. Please refresh and try again.");
        }
    }

    public async Task<(bool IsAvailable, string Message, decimal TotalAmount)> CheckAvailabilityAsync(Guid vehicleId, DateTime pickupDateTime, DateTime returnDateTime, CancellationToken cancellationToken = default)
    {
        if (returnDateTime <= pickupDateTime)
        {
            return (false, "Return date must be later than pickup date.", 0);
        }

        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return (false, "Vehicle not found.", 0);
        }

        var available = await IsVehicleAvailableAsync(vehicleId, pickupDateTime, returnDateTime, cancellationToken);
        var totalAmount = CalculateTotal(vehicle.DailyRate, pickupDateTime, returnDateTime);

        return available
            ? (true, "Vehicle is available.", totalAmount)
            : (false, "Vehicle is already booked in this range.", totalAmount);
    }

    private async Task<bool> IsVehicleAvailableAsync(Guid vehicleId, DateTime pickupDateTime, DateTime returnDateTime, CancellationToken cancellationToken)
    {
        return !await _context.Bookings.AnyAsync(booking =>
            booking.VehicleId == vehicleId &&
            booking.Status != BookingStatus.Cancelled &&
            booking.Status != BookingStatus.Rejected &&
            booking.Status != BookingStatus.Completed &&
            pickupDateTime < booking.ReturnDateTime &&
            returnDateTime > booking.PickupDateTime, cancellationToken);
    }

    private static string GenerateBookingCode()
    {
        return $"BK-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    private static decimal CalculateTotal(decimal dailyRate, DateTime pickupDateTime, DateTime returnDateTime)
    {
        var days = Math.Max((returnDateTime.Date - pickupDateTime.Date).Days, 1);
        return days * dailyRate;
    }

    private static IReadOnlyList<SelectListItem> BuildPaymentMethodOptions(PaymentMethod selected)
    {
        return Enum.GetValues<PaymentMethod>()
            .Select(method => new SelectListItem
            {
                Text = method.ToString(),
                Value = ((int)method).ToString(),
                Selected = method == selected
            })
            .ToList();
    }
}