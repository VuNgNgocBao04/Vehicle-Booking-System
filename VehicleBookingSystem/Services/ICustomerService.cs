using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public interface ICustomerService
{
    Task<CustomerVehicleIndexViewModel> BuildIndexAsync(VehicleFilterViewModel filter, IReadOnlyList<VehicleSearchHistoryItem> history, CancellationToken cancellationToken = default);
    Task<VehicleDetailsViewModel?> BuildDetailsAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> SearchSuggestionsAsync(string? term, CancellationToken cancellationToken = default);
    Task<bool> VehicleExistsAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<string?> GetVehicleCategoryNameAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
