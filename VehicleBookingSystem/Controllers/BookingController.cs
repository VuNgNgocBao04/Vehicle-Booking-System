using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Extensions;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Controllers;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Create(Guid vehicleId)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == vehicleId && item.Status == VehicleStatus.Available);

        if (vehicle is null)
        {
            return NotFound();
        }

        var draft = HttpContext.Session.GetObject<BookingDraftSessionModel>(SessionKeys.PendingBookingDraft);
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

        model.EstimatedTotalAmount = CalculateTotal(vehicle.DailyRate, model.PickupDateTime, model.ReturnDateTime);
        model.PaymentMethodOptions = BuildPaymentMethodOptions(model.PaymentMethod);

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == model.VehicleId);

        if (vehicle is null)
        {
            return NotFound();
        }

        if (model.ReturnDateTime <= model.PickupDateTime)
        {
            ModelState.AddModelError(nameof(model.ReturnDateTime), "Return date must be later than pickup date.");
        }

        var available = await IsVehicleAvailableAsync(model.VehicleId, model.PickupDateTime, model.ReturnDateTime);
        if (!available)
        {
            ModelState.AddModelError(string.Empty, "Vehicle is not available in the selected period.");
        }

        model.VehicleName = $"{vehicle.Brand} {vehicle.Model}";
        model.DailyRate = vehicle.DailyRate;
        model.EstimatedTotalAmount = CalculateTotal(vehicle.DailyRate, model.PickupDateTime, model.ReturnDateTime);
        model.PaymentMethodOptions = BuildPaymentMethodOptions(model.PaymentMethod);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            HttpContext.Session.SetObject(SessionKeys.PendingBookingDraft, new BookingDraftSessionModel
            {
                VehicleId = model.VehicleId,
                PickupLocation = model.PickupLocation,
                DropoffLocation = model.DropoffLocation,
                PickupDateTime = model.PickupDateTime,
                ReturnDateTime = model.ReturnDateTime,
                PaymentMethod = model.PaymentMethod
            });

            TempData["Message"] = "Please login to complete your booking. Draft has been saved.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Create), new { vehicleId = model.VehicleId }) });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Forbid();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (!await IsVehicleAvailableAsync(model.VehicleId, model.PickupDateTime, model.ReturnDateTime))
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Vehicle is not available in the selected period.");
            return View(model);
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = GenerateBookingCode(),
            UserId = parsedUserId,
            VehicleId = model.VehicleId,
            PickupLocation = model.PickupLocation.Trim(),
            DropoffLocation = model.DropoffLocation.Trim(),
            PickupDateTime = model.PickupDateTime,
            ReturnDateTime = model.ReturnDateTime,
            TotalAmount = model.EstimatedTotalAmount,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentMethod = model.PaymentMethod,
                Status = PaymentStatus.Pending,
                PaidAmount = model.EstimatedTotalAmount
            }
        };

        _context.Bookings.Add(booking);

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Unable to create booking right now. Please try again.");
            return View(model);
        }

        HttpContext.Session.Remove(SessionKeys.PendingBookingDraft);
        TempData["Message"] = "Booking created successfully.";
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyBookings(string? searchTerm, BookingStatus? status, int page = 1, int pageSize = 8)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Forbid();
        }

        var query = _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Vehicle)
            .Where(booking => booking.UserId == parsedUserId)
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

        var totalItems = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = pageSize <= 0 ? 8 : pageSize;

        var items = await query
            .OrderByDescending(booking => booking.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new BookingHistoryIndexViewModel
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

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Forbid();
        }

        var booking = await _context.Bookings.FirstOrDefaultAsync(item => item.Id == id && item.UserId == parsedUserId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.PickupDateTime <= DateTime.UtcNow.AddHours(6))
        {
            TempData["Error"] = "Booking can only be cancelled at least 6 hours before pickup time.";
            return RedirectToAction(nameof(MyBookings));
        }

        booking.Status = BookingStatus.Cancelled;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Booking was updated by another request. Please refresh and try again.";
            return RedirectToAction(nameof(MyBookings));
        }

        TempData["Message"] = "Booking cancelled successfully.";
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability(Guid vehicleId, DateTime pickupDateTime, DateTime returnDateTime)
    {
        if (returnDateTime <= pickupDateTime)
        {
            return Json(new { available = false, message = "Return date must be later than pickup date." });
        }

        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == vehicleId);

        if (vehicle is null)
        {
            return Json(new { available = false, message = "Vehicle not found." });
        }

        var available = await IsVehicleAvailableAsync(vehicleId, pickupDateTime, returnDateTime);
        var total = CalculateTotal(vehicle.DailyRate, pickupDateTime, returnDateTime);
        return Json(new
        {
            available,
            message = available ? "Vehicle is available." : "Vehicle is already booked in this range.",
            totalAmount = total
        });
    }

    private async Task<bool> IsVehicleAvailableAsync(Guid vehicleId, DateTime pickupDateTime, DateTime returnDateTime)
    {
        return !await _context.Bookings.AnyAsync(booking =>
            booking.VehicleId == vehicleId &&
            booking.Status != BookingStatus.Cancelled &&
            booking.Status != BookingStatus.Rejected &&
            booking.Status != BookingStatus.Completed &&
            pickupDateTime < booking.ReturnDateTime &&
            returnDateTime > booking.PickupDateTime);
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
