using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Services;

public sealed class AccountService : IAccountService
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public AccountService(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IFileStorageService fileStorageService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
    }

    public async Task<AccountOperationResult> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            return new AccountOperationResult(false, null, "Email đã được đăng ký.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            DateOfBirth = model.DateOfBirth,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            var messages = string.Join("; ", createResult.Errors.Select(error => error.Description));
            return new AccountOperationResult(false, null, messages);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return new AccountOperationResult(false, null, "Đăng ký thất bại, vui lòng thử lại.");
        }

        try
        {
            var avatarUrl = await _fileStorageService.SaveAvatarAsync(user.Id, model.AvatarFile, cancellationToken);
            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                user.AvatarUrl = avatarUrl;
                await _userManager.UpdateAsync(user);
            }
        }
        catch (InvalidOperationException ex)
        {
            await _userManager.DeleteAsync(user);
            return new AccountOperationResult(false, null, ex.Message);
        }

        return new AccountOperationResult(true, user);
    }

    public async Task<AccountLoginResult> LoginAsync(LoginViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return new AccountLoginResult(false, null, ErrorMessage: "Email hoặc mật khẩu không đúng.");
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            return new AccountLoginResult(false, user, IsLockedOut: true, ErrorMessage: "Tài khoản đã bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau 5 phút.");
        }

        if (result.IsNotAllowed)
        {
            return new AccountLoginResult(false, user, IsNotAllowed: true, ErrorMessage: "Tài khoản chưa được xác thực.");
        }

        if (!result.Succeeded)
        {
            return new AccountLoginResult(false, user, ErrorMessage: "Email hoặc mật khẩu không đúng.");
        }

        return new AccountLoginResult(true, user);
    }

    public async Task<ProfileViewModel?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return new ProfileViewModel
        {
            Update = new ProfileUpdateViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                CurrentAvatarUrl = user.AvatarUrl
            }
        };
    }

    public async Task<AccountOperationResult> UpdateProfileAsync(Guid userId, ProfileUpdateViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return new AccountOperationResult(false, null, "User not found.");
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber?.Trim();
        user.Address = model.Address?.Trim();
        user.DateOfBirth = model.DateOfBirth;

        if (model.AvatarFile is not null)
        {
            try
            {
                user.AvatarUrl = await _fileStorageService.SaveAvatarAsync(user.Id, model.AvatarFile, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return new AccountOperationResult(false, user, ex.Message);
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var messages = string.Join("; ", result.Errors.Select(error => error.Description));
            return new AccountOperationResult(false, user, messages);
        }

        return new AccountOperationResult(true, user);
    }

    public async Task<AccountOperationResult> ChangePasswordAsync(Guid userId, ChangePasswordViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return new AccountOperationResult(false, null, "User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            var messages = string.Join("; ", result.Errors.Select(error => error.Description));
            return new AccountOperationResult(false, user, messages);
        }

        await _signInManager.RefreshSignInAsync(user);
        return new AccountOperationResult(true, user);
    }
}