using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace VehicleBookingSystem.Controllers;

public class LocalizationController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl = null)
    {
        var supportedCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "vi-VN",
            "en-US"
        };

        var selectedCulture = supportedCultures.Contains(culture) ? culture : "vi-VN";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(selectedCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
