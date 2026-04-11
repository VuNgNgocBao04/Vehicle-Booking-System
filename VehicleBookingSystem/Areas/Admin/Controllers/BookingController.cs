using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> Index(string? searchTerm, BookingStatus? status, int page = 1, int pageSize = 10)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Vehicle)
            .Include(booking => booking.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var keyword = searchTerm.Trim();
            query = query.Where(booking =>
                booking.BookingCode.Contains(keyword) ||
                (booking.Vehicle != null && booking.Vehicle.LicensePlate.Contains(keyword)) ||
                (booking.User != null && booking.User.FullName.Contains(keyword)) ||
                (booking.User != null && booking.User.Email != null && booking.User.Email.Contains(keyword)));
        }

        if (status.HasValue)
        {
            query = query.Where(booking => booking.Status == status.Value);
        }

        var totalItems = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var items = await query
            .OrderByDescending(booking => booking.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new AdminBookingIndexViewModel
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        return await UpdateStatusAsync(id, BookingStatus.Confirmed, "Booking approved.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        return await UpdateStatusAsync(id, BookingStatus.Cancelled, "Booking rejected.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return await UpdateStatusAsync(id, BookingStatus.Cancelled, "Booking cancelled.");
    }

    private async Task<IActionResult> UpdateStatusAsync(Guid id, BookingStatus status, string message)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        booking.Status = status;
        await _context.SaveChangesAsync();
        TempData["Message"] = message;
        return RedirectToAction(nameof(Index));
    }
}
