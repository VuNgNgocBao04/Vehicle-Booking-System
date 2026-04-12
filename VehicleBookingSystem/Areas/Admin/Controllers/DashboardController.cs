using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await BuildModelAsync());
    }

    private async Task<AdminDashboardViewModel> BuildModelAsync()
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var sixMonthsStart = monthStart.AddMonths(-5);

        var totalVehicles = await _context.Vehicles.CountAsync();
        var bookingsToday = await _context.Bookings.CountAsync(item => item.CreatedAt >= today);
        var revenueThisMonth = await _context.Payments
            .Where(item => item.Status == PaymentStatus.Paid && item.PaidAt.HasValue && item.PaidAt.Value >= monthStart)
            .Select(item => (decimal?)item.PaidAmount)
            .SumAsync() ?? 0;

        var monthlyBookings = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.CreatedAt >= monthStart)
            .Select(item => item.UserId)
            .Distinct()
            .ToListAsync();

        var historicalUsers = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.CreatedAt < monthStart)
            .Select(item => item.UserId)
            .Distinct()
            .ToHashSetAsync();

        var newCustomersThisMonth = monthlyBookings.Count(userId => !historicalUsers.Contains(userId));

        var revenueByMonthRaw = await _context.Payments
            .AsNoTracking()
            .Where(item => item.Status == PaymentStatus.Paid && item.PaidAt.HasValue && item.PaidAt.Value >= sixMonthsStart)
            .GroupBy(item => new { item.PaidAt!.Value.Year, item.PaidAt!.Value.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Value = group.Sum(item => item.PaidAmount)
            })
            .ToDictionaryAsync(item => (item.Year, item.Month), item => item.Value);

        var revenueByMonth = Enumerable.Range(0, 6)
            .Select(offset => sixMonthsStart.AddMonths(offset))
            .Select(start => new AdminMetricPointViewModel
            {
                Label = start.ToString("MM/yyyy"),
                Value = revenueByMonthRaw.TryGetValue((start.Year, start.Month), out var value) ? value : 0
            })
            .ToList();

        var bookingByCategory = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.CreatedAt >= monthStart)
            .GroupBy(item => item.Vehicle!.VehicleCategory!.Name ?? "Unknown")
            .Select(group => new AdminPiePointViewModel
            {
                Label = group.Key,
                Value = group.Count()
            })
            .OrderByDescending(item => item.Value)
            .Take(6)
            .ToListAsync();

        var recentPending = await _context.Bookings
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.Vehicle)
            .Where(item => item.Status == BookingStatus.Pending)
            .OrderByDescending(item => item.CreatedAt)
            .Take(6)
            .Select(item => new AdminRecentBookingItemViewModel
            {
                BookingId = item.Id,
                BookingCode = item.BookingCode,
                CustomerName = item.User == null ? "Unknown" : item.User.FullName,
                VehicleName = item.Vehicle == null ? "Unknown" : $"{item.Vehicle.Brand} {item.Vehicle.Model}",
                PickupDateTime = item.PickupDateTime,
                TotalAmount = item.TotalAmount
            })
            .ToListAsync();

        var maintenanceVehicles = await _context.Vehicles
            .AsNoTracking()
            .Where(item => item.Status == VehicleStatus.Maintenance)
            .OrderBy(item => item.Brand)
            .ThenBy(item => item.Model)
            .Take(6)
            .Select(item => new AdminMaintenanceVehicleItemViewModel
            {
                VehicleId = item.Id,
                VehicleName = $"{item.Brand} {item.Model}",
                LicensePlate = item.LicensePlate
            })
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            TotalVehicles = totalVehicles,
            BookingsToday = bookingsToday,
            RevenueThisMonth = revenueThisMonth,
            NewCustomersThisMonth = newCustomersThisMonth,
            RevenueByMonth = revenueByMonth,
            BookingByCategory = bookingByCategory,
            RecentPendingBookings = recentPending,
            MaintenanceVehicles = maintenanceVehicles
        };
    }
}
