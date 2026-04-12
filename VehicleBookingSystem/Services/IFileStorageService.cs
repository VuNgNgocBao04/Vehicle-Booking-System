using Microsoft.AspNetCore.Http;

namespace VehicleBookingSystem.Services;

public interface IFileStorageService
{
    Task<IReadOnlyList<string>> SaveVehicleGalleryAsync(Guid vehicleId, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken = default);
    Task<string?> SaveAvatarAsync(Guid userId, IFormFile? file, CancellationToken cancellationToken = default);
    Task DeleteVehicleGalleryAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
