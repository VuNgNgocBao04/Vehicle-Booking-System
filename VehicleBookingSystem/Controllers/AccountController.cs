using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VehicleBookingSystem.Extensions;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Services;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IFileStorageService fileStorageService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel
        {
            DateOfBirth = DateTime.Today.AddYears(-18)
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã được đăng ký.");
            return View(model);
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
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty, "Đăng ký thất bại, vui lòng thử lại.");
            return View(model);
        }

        try
        {
            var avatarUrl = await _fileStorageService.SaveAvatarAsync(user.Id, model.AvatarFile);
            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                user.AvatarUrl = avatarUrl;
                await _userManager.UpdateAsync(user);
            }
        }
        catch (InvalidOperationException ex)
        {
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(nameof(model.AvatarFile), ex.Message);
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        await StoreUserSessionAsync(user);

        return RedirectToAction("Index", "Customer");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau 5 phút.");
            return View(model);
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản chưa được xác thực.");
            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        await StoreUserSessionAsync(user);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        return RedirectToAction("Index", "Customer");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var model = new ProfileViewModel
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

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (!TryValidateModel(model.Update, nameof(ProfileViewModel.Update)))
        {
            model.Update.CurrentAvatarUrl = user.AvatarUrl;
            return View(model);
        }

        user.FullName = model.Update.FullName.Trim();
        user.PhoneNumber = model.Update.PhoneNumber?.Trim();
        user.Address = model.Update.Address?.Trim();
        user.DateOfBirth = model.Update.DateOfBirth;

        if (model.Update.AvatarFile is not null)
        {
            try
            {
                user.AvatarUrl = await _fileStorageService.SaveAvatarAsync(user.Id, model.Update.AvatarFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(ProfileViewModel.Update) + ".AvatarFile", ex.Message);
                model.Update.CurrentAvatarUrl = user.AvatarUrl;
                return View(model);
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Update.CurrentAvatarUrl = user.AvatarUrl;
            return View(model);
        }

        await StoreUserSessionAsync(user);
        TempData["Message"] = "Cập nhật hồ sơ thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        model.Update = new ProfileUpdateViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            DateOfBirth = user.DateOfBirth,
            CurrentAvatarUrl = user.AvatarUrl
        };

        if (!TryValidateModel(model.ChangePassword, nameof(ProfileViewModel.ChangePassword)))
        {
            return View("Profile", model);
        }

        var result = await _userManager.ChangePasswordAsync(user, model.ChangePassword.CurrentPassword, model.ChangePassword.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Profile", model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Message"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile));
    }

    private async Task StoreUserSessionAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var profile = new SessionUserProfile
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "Customer",
            Avatar = user.AvatarUrl ?? BuildAvatar(user.FullName)
        };

        HttpContext.Session.SetObject(SessionKeys.UserProfile, profile);
    }

    private static string BuildAvatar(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "U";
        }

        var initials = string.Concat(fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));

        return string.IsNullOrWhiteSpace(initials) ? "U" : initials;
    }
}
