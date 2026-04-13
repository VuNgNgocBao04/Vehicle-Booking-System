using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var featured = await _context.Vehicles
            .AsNoTracking()
            .Include(item => item.VehicleCategory)
            .Where(item => item.Status == VehicleStatus.Available)
            .OrderBy(item => item.DailyRate)
            .Take(6)
            .Select(item => new HomeFeaturedVehicleViewModel
            {
                Id = item.Id,
                Name = $"{item.Brand} {item.Model}",
                ImageUrl = item.ImageUrl,
                DailyRate = item.DailyRate,
                Category = item.VehicleCategory == null ? "Unknown" : item.VehicleCategory.Name
            })
            .ToListAsync();

        var categories = await _context.VehicleCategories
            .AsNoTracking()
            .Select(category => new HomeCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                AvailableCount = category.Vehicles.Count(vehicle => vehicle.Status == VehicleStatus.Available)
            })
            .OrderByDescending(item => item.AvailableCount)
            .Take(6)
            .ToListAsync();

        return View(new HomeIndexViewModel
        {
            FeaturedVehicles = featured,
            Categories = categories
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var statusCode = HttpContext.Request.Query.TryGetValue("statusCode", out var statusCodeValues) && int.TryParse(statusCodeValues.ToString(), out var parsedStatusCode)
            ? parsedStatusCode
            : feature?.Error is UnauthorizedAccessException
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status500InternalServerError;

        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            _ => "An error occurred while processing your request."
        };

        var model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = statusCode,
            Title = title,
            Detail = feature?.Error.Message,
            Instance = feature?.Path
        };

        _logger.LogWarning(feature?.Error, "Error page requested. StatusCode: {StatusCode}, Path: {Path}, TraceId: {TraceId}", statusCode, feature?.Path, requestId);

        if (IsApiRequest(feature?.Path))
        {
            return Problem(detail: model.Detail, title: model.Title, statusCode: model.StatusCode, instance: model.Instance);
        }

        return View(model);
    }

    private static bool IsApiRequest(string? path)
    {
        return !string.IsNullOrWhiteSpace(path) && path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
    }
}
