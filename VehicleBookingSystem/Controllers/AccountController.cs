using System.Security.Claims;
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
    private readonly IAccountService _accountService;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IAccountService accountService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _accountService = accountService;
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

        var result = await _accountService.RegisterAsync(model);
        if (!result.Succeeded || result.User is null)
        {
            ModelState.AddModelError(string.IsNullOrWhiteSpace(result.ErrorMessage) ? nameof(model.Email) : string.Empty, result.ErrorMessage ?? "Đăng ký thất bại, vui lòng thử lại.");
            return View(model);
        }

<<<<<<< HEAD
        await _signInManager.SignInAsync(result.User, isPersistent: false);
        await StoreUserSessionAsync(result.User);
=======
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
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError(string.Empty, "Không thể cập nhật hồ sơ tài khoản.");
                    return View(model);
                }
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
>>>>>>> origin/dev

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

        var result = await _accountService.LoginAsync(model);
        if (!result.Succeeded || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        await StoreUserSessionAsync(result.User);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        if (await _userManager.IsInRoleAsync(result.User, "Admin"))
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Challenge();
        }

        var model = await _accountService.GetProfileAsync(parsedUserId);
        if (model is null)
        {
            return Challenge();
        }

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Challenge();
        }

        if (!TryValidateModel(model.Update, nameof(ProfileViewModel.Update)))
        {
            return View(model);
        }

        var result = await _accountService.UpdateProfileAsync(parsedUserId, model.Update);
        if (!result.Succeeded || result.User is null)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                ModelState.AddModelError(nameof(ProfileViewModel.Update) + ".AvatarFile", result.ErrorMessage);
            }

            model.Update.CurrentAvatarUrl = result.User?.AvatarUrl;
            return View(model);
        }

        await StoreUserSessionAsync(result.User);
        TempData["Message"] = "Cập nhật hồ sơ thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ProfileViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Challenge();
        }

        var profile = await _accountService.GetProfileAsync(parsedUserId);
        if (profile is null)
        {
            return Challenge();
        }

        model.Update = profile.Update;

        if (!TryValidateModel(model.ChangePassword, nameof(ProfileViewModel.ChangePassword)))
        {
            return View("Profile", model);
        }

        var result = await _accountService.ChangePasswordAsync(parsedUserId, model.ChangePassword);
        if (!result.Succeeded)
        {
            foreach (var error in (result.ErrorMessage ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View("Profile", model);
        }
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
