using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public sealed record VehicleCommandResult(bool Succeeded, string? ErrorMessage = null);

public interface IVehicleService
{
    Task<VehicleFormViewModel?> BuildEditViewModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> BuildGalleryUrlsAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<VehicleCommandResult> CreateAsync(VehicleFormViewModel form, CancellationToken cancellationToken = default);
    Task<VehicleCommandResult> UpdateAsync(Guid id, VehicleFormViewModel form, CancellationToken cancellationToken = default);
    Task<VehicleCommandResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}