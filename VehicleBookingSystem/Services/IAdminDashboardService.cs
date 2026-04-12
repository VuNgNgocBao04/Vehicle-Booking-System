using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> BuildAsync(CancellationToken cancellationToken = default);
}