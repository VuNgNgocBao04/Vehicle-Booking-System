using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleBookingSystem.Extensions;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Services;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index([FromQuery] VehicleFilterViewModel filter)
    {
        SaveSearchHistory(filter);
        var history = HttpContext.Session.GetObject<List<VehicleSearchHistoryItem>>(SessionKeys.VehicleSearchHistory) ?? [];
        var model = await _customerService.BuildIndexAsync(filter, history);

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_VehicleList", model);
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        var model = await _customerService.BuildDetailsAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchSuggestions([FromQuery] string? term)
    {
        var suggestions = await _customerService.SearchSuggestionsAsync(term);

        return Json(suggestions);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VehicleComments(Guid vehicleId)
    {
        if (!await _customerService.VehicleExistsAsync(vehicleId))
        {
            return NotFound();
        }

        return PartialView("_VehicleComments", Array.Empty<string>());
    }

    private void SaveSearchHistory(VehicleFilterViewModel filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SearchTerm) &&
            !filter.VehicleCategoryId.HasValue &&
            string.IsNullOrWhiteSpace(filter.Brand) &&
            !filter.MinDailyRate.HasValue &&
            !filter.MaxDailyRate.HasValue &&
            !filter.SeatCount.HasValue)
        {
            return;
        }

        var history = HttpContext.Session.GetObject<List<VehicleSearchHistoryItem>>(SessionKeys.VehicleSearchHistory) ?? [];
        history.Insert(0, new VehicleSearchHistoryItem
        {
            At = DateTime.UtcNow,
            Keyword = filter.SearchTerm ?? string.Empty,
            FilterSummary = $"Category: {(filter.VehicleCategoryId?.ToString() ?? "Any")}, Brand: {(filter.Brand ?? "Any")}, Seats: {(filter.SeatCount?.ToString() ?? "Any")}, Rate: {(filter.MinDailyRate?.ToString() ?? "0")} - {(filter.MaxDailyRate?.ToString() ?? "Any")}" 
        });

        if (history.Count > 10)
        {
            history = history.Take(10).ToList();
        }

        HttpContext.Session.SetObject(SessionKeys.VehicleSearchHistory, history);
    }

    private static string NormalizeSort(string? sortBy)
    {
        return sortBy switch
        {
            "priceDesc" => "priceDesc",
            "newest" => "newest",
            "popular" => "popular",
            _ => "priceAsc"
        };
    }

    private static string NormalizeViewMode(string? viewMode)
    {
        return string.Equals(viewMode, "list", StringComparison.OrdinalIgnoreCase) ? "list" : "grid";
    }
}
