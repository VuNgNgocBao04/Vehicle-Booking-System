using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public sealed record AccountOperationResult(bool Succeeded, AppUser? User = null, string? ErrorMessage = null);
public sealed record AccountLoginResult(bool Succeeded, AppUser? User = null, bool IsLockedOut = false, bool IsNotAllowed = false, string? ErrorMessage = null);

public interface IAccountService
{
    Task<AccountOperationResult> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken = default);
    Task<AccountLoginResult> LoginAsync(LoginViewModel model, CancellationToken cancellationToken = default);
    Task<ProfileViewModel?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> UpdateProfileAsync(Guid userId, ProfileUpdateViewModel model, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> ChangePasswordAsync(Guid userId, ChangePasswordViewModel model, CancellationToken cancellationToken = default);
}