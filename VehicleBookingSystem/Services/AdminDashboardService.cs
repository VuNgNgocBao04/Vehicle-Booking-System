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

        // Calculate revenue for 6 months in a single query instead of 6 separate queries
        var sixMonthsAgo = monthStart.AddMonths(-5);
        var revenueData = await _context.Payments
            .AsNoTracking()
            .Where(item => item.Status == PaymentStatus.Paid &&
                           item.PaidAt.HasValue &&
                           item.PaidAt.Value >= sixMonthsAgo &&
                           item.PaidAt.Value < monthStart.AddMonths(1))
            .GroupBy(item => new { item.PaidAt!.Value.Year, item.PaidAt!.Value.Month })
            .Select(group => new
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                Amount = group.Sum(item => (decimal?)item.PaidAmount) ?? 0
            })
            .OrderBy(item => item.Year)
            .ThenBy(item => item.Month)
            .ToListAsync(cancellationToken);

        var revenueByMonth = new List<AdminMetricPointViewModel>();
        foreach (var start in Enumerable.Range(0, 6).Select(offset => monthStart.AddMonths(-5 + offset)))
        {
            var amount = revenueData
                .Where(item => item.Year == start.Year && item.Month == start.Month)
                .Select(item => item.Amount)
                .FirstOrDefault();

            revenueByMonth.Add(new AdminMetricPointViewModel
            {
                Label = start.ToString("MM/yyyy"),
                Value = amount
            });
        }

        var bookingByCategory = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.CreatedAt >= monthStart)
            .Select(item => item.Vehicle != null && item.Vehicle.VehicleCategory != null
                ? item.Vehicle.VehicleCategory.Name
                : "Unknown")
            .GroupBy(item => item)
            .Select(group => new AdminPiePointViewModel
            {
                Label = group.Key,
                Value = group.Count()
            })
            .OrderByDescending(item => item.Value)
            .Take(6)
            .ToListAsync(cancellationToken);

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