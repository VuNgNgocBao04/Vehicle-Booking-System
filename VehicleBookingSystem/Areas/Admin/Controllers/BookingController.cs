using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Globalization;
using VehicleBookingSystem.Contracts.Common;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new AdminBookingIndexViewModel
        {
            VehicleOptions = await _context.Vehicles
                .AsNoTracking()
                .OrderBy(item => item.Brand)
                .ThenBy(item => item.Model)
                .Select(item => new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = $"{item.Brand} {item.Model} - {item.LicensePlate}"
                })
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TableData()
    {
        var form = Request.Form;
        var draw = ParseInt(form["draw"], 1);
        var start = Math.Max(ParseInt(form["start"], 0), 0);
        var length = Math.Clamp(ParseInt(form["length"], 10), 5, 100);
        var search = form["search[value]"].ToString().Trim();
        var statusRaw = form["status"].ToString();
        var vehicleIdRaw = form["vehicleId"].ToString();
        var fromDateRaw = form["fromDate"].ToString();
        var toDateRaw = form["toDate"].ToString();

        var status = Enum.TryParse<BookingStatus>(statusRaw, true, out var parsedStatus) ? (BookingStatus?)parsedStatus : null;
        var vehicleId = Guid.TryParse(vehicleIdRaw, out var parsedVehicleId) ? (Guid?)parsedVehicleId : null;
        var fromDate = DateTime.TryParse(fromDateRaw, out var parsedFromDate) ? (DateTime?)parsedFromDate.Date : null;
        var toDate = DateTime.TryParse(toDateRaw, out var parsedToDate) ? (DateTime?)parsedToDate.Date : null;

        var query = _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Vehicle)
            .Include(booking => booking.User)
            .AsQueryable();

        var recordsTotal = await _context.Bookings.CountAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(booking =>
                booking.BookingCode.Contains(search) ||
                (booking.Vehicle != null && booking.Vehicle.LicensePlate.Contains(search)) ||
                (booking.User != null && booking.User.FullName.Contains(search)) ||
                (booking.User != null && booking.User.Email != null && booking.User.Email.Contains(search)));
        }

        if (status.HasValue)
        {
            query = query.Where(booking => booking.Status == status.Value);
        }

        if (vehicleId.HasValue)
        {
            query = query.Where(booking => booking.VehicleId == vehicleId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(booking => booking.PickupDateTime.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(booking => booking.PickupDateTime.Date <= toDate.Value);
        }

        var recordsFiltered = await query.CountAsync();
        var culture = CultureInfo.CurrentCulture;

        var data = await query
            .OrderByDescending(booking => booking.CreatedAt)
            .Skip(start)
            .Take(length)
            .Select(booking => new
            {
                id = booking.Id,
                bookingCode = booking.BookingCode,
                customer = booking.User != null ? booking.User.FullName : "-",
                email = booking.User != null ? booking.User.Email : "-",
                vehicle = booking.Vehicle != null ? booking.Vehicle.Brand + " " + booking.Vehicle.Model : "-",
                plate = booking.Vehicle != null ? booking.Vehicle.LicensePlate : "-",
                period = booking.PickupDateTime.ToString("d", culture) + " - " + booking.ReturnDateTime.ToString("d", culture),
                totalAmount = booking.TotalAmount.ToString("C0", culture),
                status = booking.Status.ToString(),
                statusClass = GetBookingStatusBadge(booking.Status)
            })
            .ToListAsync();

        return Json(new
        {
            draw,
            recordsTotal,
            recordsFiltered,
            data
        });
    }

    [HttpGet]
    public async Task<IActionResult> DetailData(Guid id)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Vehicle)
            .Include(item => item.User)
            .Include(item => item.Payment)
            .Include(item => item.StatusHistories)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (booking is null)
        {
            return NotFound(ApiResponse<object>.Fail("Booking not found."));
        }

        var history = booking.StatusHistories
            .OrderBy(item => item.ChangedAtUtc)
            .Select(item => new
            {
                at = item.ChangedAtUtc.ToString("g", CultureInfo.CurrentCulture),
                status = item.FromStatus.HasValue ? $"{item.FromStatus} -> {item.ToStatus}" : item.ToStatus.ToString(),
                actor = item.ChangedBy,
                note = item.Note
            })
            .ToList();

        return Json(ApiResponse<object>.Ok(new
        {
            id = booking.Id,
            bookingCode = booking.BookingCode,
            customerName = booking.User?.FullName,
            customerEmail = booking.User?.Email,
            customerPhone = booking.User?.PhoneNumber,
            vehicleName = booking.Vehicle != null ? booking.Vehicle.Brand + " " + booking.Vehicle.Model : "-",
            plate = booking.Vehicle?.LicensePlate,
            pickupLocation = booking.PickupLocation,
            dropoffLocation = booking.DropoffLocation,
            pickupDate = booking.PickupDateTime.ToString("g", CultureInfo.CurrentCulture),
            returnDate = booking.ReturnDateTime.ToString("g", CultureInfo.CurrentCulture),
            totalAmount = booking.TotalAmount.ToString("C0", CultureInfo.CurrentCulture),
            status = booking.Status.ToString(),
            paymentStatus = booking.Payment?.Status.ToString() ?? "N/A",
            history
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        return await ApproveInternalAsync(id, false);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveAjax(Guid id)
    {
        return await ApproveInternalAsync(id, true);
    }

    private async Task<IActionResult> ApproveInternalAsync(Guid id, bool ajax)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var booking = await _context.Bookings.FirstOrDefaultAsync(item => item.Id == id);
        if (booking is null)
        {
            await transaction.RollbackAsync();
            return AjaxOrRedirect(ajax, false, "Booking not found.");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            await transaction.RollbackAsync();
            return AjaxOrRedirect(ajax, false, "Only pending bookings can be approved.");
        }

        var fromStatus = booking.Status;

        var hasConflict = await _context.Bookings.AnyAsync(item =>
            item.Id != id &&
            item.VehicleId == booking.VehicleId &&
            (item.Status == BookingStatus.Pending || item.Status == BookingStatus.Confirmed) &&
            booking.PickupDateTime < item.ReturnDateTime &&
            booking.ReturnDateTime > item.PickupDateTime);

        if (hasConflict)
        {
            await transaction.RollbackAsync();
            return AjaxOrRedirect(ajax, false, "Cannot approve booking because another pending/confirmed booking already overlaps this period.");
        }

        booking.Status = BookingStatus.Confirmed;
        booking.StatusHistories.Add(new BookingStatusHistory
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            FromStatus = fromStatus,
            ToStatus = BookingStatus.Confirmed,
            ChangedAtUtc = DateTime.UtcNow,
            ChangedBy = User.Identity?.Name ?? "Admin",
            Note = "Admin approved booking"
        });

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return AjaxOrRedirect(ajax, false, "Booking was updated by another request. Please refresh and try again.");
        }

        return AjaxOrRedirect(ajax, true, "Booking approved.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        return await UpdateStatusAsync(id, BookingStatus.Rejected, "Booking rejected.", false, BookingStatus.Pending);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectAjax(Guid id)
    {
        return await UpdateStatusAsync(id, BookingStatus.Rejected, "Booking rejected.", true, BookingStatus.Pending);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return await UpdateStatusAsync(id, BookingStatus.Cancelled, "Booking cancelled.", false, BookingStatus.Pending, BookingStatus.Confirmed);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAjax(Guid id)
    {
        return await UpdateStatusAsync(id, BookingStatus.Cancelled, "Booking cancelled.", true, BookingStatus.Pending, BookingStatus.Confirmed);
    }

    private async Task<IActionResult> UpdateStatusAsync(Guid id, BookingStatus newStatus, string message, bool ajax, params BookingStatus[] allowedFromStatuses)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var booking = await _context.Bookings.FirstOrDefaultAsync(item => item.Id == id);
        if (booking is null)
        {
            await transaction.RollbackAsync();
            return AjaxOrRedirect(ajax, false, "Booking not found.");
        }

        if (allowedFromStatuses.Length > 0 && !allowedFromStatuses.Contains(booking.Status))
        {
            await transaction.RollbackAsync();
            return AjaxOrRedirect(ajax, false, $"Cannot change booking status from {booking.Status}.");
        }

        var fromStatus = booking.Status;

        booking.Status = newStatus;
        booking.StatusHistories.Add(new BookingStatusHistory
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            FromStatus = fromStatus,
            ToStatus = newStatus,
            ChangedAtUtc = DateTime.UtcNow,
            ChangedBy = User.Identity?.Name ?? "Admin",
            Note = message
        });

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return AjaxOrRedirect(ajax, false, "Booking was updated by another request. Please refresh and try again.");
        }

        return AjaxOrRedirect(ajax, true, message);
    }

    private IActionResult AjaxOrRedirect(bool ajax, bool success, string message)
    {
        if (ajax)
        {
            if (!success)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Json(ApiResponse<string>.Fail(message));
            }

            return Json(ApiResponse<string>.Ok(message));
        }

        if (success)
        {
            TempData["Message"] = message;
        }
        else
        {
            TempData["Error"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    private static int ParseInt(string? raw, int fallback)
    {
        return int.TryParse(raw, out var value) ? value : fallback;
    }

    private static string GetBookingStatusBadge(BookingStatus status)
    {
        return status switch
        {
            BookingStatus.Pending => "warning",
            BookingStatus.Confirmed => "success",
            BookingStatus.Cancelled => "secondary",
            BookingStatus.Rejected => "danger",
            BookingStatus.Completed => "primary",
            _ => "secondary"
        };
    }
}
