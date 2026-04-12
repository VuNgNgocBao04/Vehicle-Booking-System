using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardViewModel> BuildAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var totalVehicles = await _context.Vehicles.CountAsync(cancellationToken);
        var bookingsToday = await _context.Bookings.CountAsync(item => item.CreatedAt >= today, cancellationToken);
        var revenueThisMonth = await _context.Payments
            .Where(item => item.Status == PaymentStatus.Paid && item.PaidAt.HasValue && item.PaidAt.Value >= monthStart)
            .Select(item => (decimal?)item.PaidAmount)
            .SumAsync(cancellationToken) ?? 0;

        var monthlyBookings = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.CreatedAt >= monthStart)
            .Select(item => new { item.UserId, item.CreatedAt })
            .ToListAsync(cancellationToken);

        var historicalUsers = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.CreatedAt < monthStart)
            .Select(item => item.UserId)
            .Distinct()
            .ToHashSetAsync(cancellationToken);

        var newCustomersThisMonth = monthlyBookings
            .Select(item => item.UserId)
            .Distinct()
            .Count(userId => !historicalUsers.Contains(userId));

        var revenueByMonth = new List<AdminMetricPointViewModel>();
        foreach (var start in Enumerable.Range(0, 6).Select(offset => monthStart.AddMonths(-5 + offset)))
        {
            var amount = await _context.Payments
                .Where(item => item.Status == PaymentStatus.Paid &&
                               item.PaidAt.HasValue &&
                               item.PaidAt.Value.Year == start.Year &&
                               item.PaidAt.Value.Month == start.Month)
                .Select(item => (decimal?)item.PaidAmount)
                .SumAsync(cancellationToken) ?? 0;

            revenueByMonth.Add(new AdminMetricPointViewModel
            {
                Label = start.ToString("MM/yyyy"),
                Value = amount
            });
        }

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
            .ToListAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

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