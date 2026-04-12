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

    public IActionResult Index()
    {
        return View(BuildModel());
    }

    private AdminDashboardViewModel BuildModel()
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var totalVehicles = _context.Vehicles.Count();
        var bookingsToday = _context.Bookings.Count(item => item.CreatedAt >= today);
        var revenueThisMonth = _context.Payments
            .Where(item => item.Status == PaymentStatus.Paid && item.PaidAt.HasValue && item.PaidAt.Value >= monthStart)
            .Select(item => (decimal?)item.PaidAmount)
            .Sum() ?? 0;
        var monthlyBookings = _context.Bookings
            .AsNoTracking()
            .Where(item => item.CreatedAt >= monthStart)
            .Select(item => new { item.UserId, item.CreatedAt })
            .ToList();

        var historicalUsers = _context.Bookings
            .AsNoTracking()
            .Where(item => item.CreatedAt < monthStart)
            .Select(item => item.UserId)
            .Distinct()
            .ToHashSet();

        var newCustomersThisMonth = monthlyBookings
            .Select(item => item.UserId)
            .Distinct()
            .Count(userId => !historicalUsers.Contains(userId));

        var revenueByMonth = Enumerable.Range(0, 6)
            .Select(offset => monthStart.AddMonths(-5 + offset))
            .Select(start => new AdminMetricPointViewModel
            {
                Label = start.ToString("MM/yyyy"),
                Value = _context.Payments
                    .Where(item => item.Status == PaymentStatus.Paid &&
                                   item.PaidAt.HasValue &&
                                   item.PaidAt.Value.Year == start.Year &&
                                   item.PaidAt.Value.Month == start.Month)
                    .Select(item => (decimal?)item.PaidAmount)
                    .Sum() ?? 0
            })
            .ToList();

        var bookingByCategory = _context.Bookings
            .AsNoTracking()
            .Include(item => item.Vehicle)
            .ThenInclude(item => item!.VehicleCategory)
            .Where(item => item.CreatedAt >= monthStart)
            .AsEnumerable()
            .GroupBy(item => item.Vehicle?.VehicleCategory?.Name ?? "Unknown")
            .Select(group => new AdminPiePointViewModel
            {
                Label = group.Key,
                Value = group.Count()
            })
            .OrderByDescending(item => item.Value)
            .Take(6)
            .ToList();

        var recentPending = _context.Bookings
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
            .ToList();

        var maintenanceVehicles = _context.Vehicles
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
            .ToList();

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