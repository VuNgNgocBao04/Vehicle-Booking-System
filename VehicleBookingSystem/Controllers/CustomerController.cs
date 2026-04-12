using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Extensions;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomerController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index([FromQuery] VehicleFilterViewModel filter)
    {
        filter.Page = Math.Max(filter.Page, 1);
        filter.PageSize = filter.PageSize <= 0 ? 9 : Math.Min(filter.PageSize, 24);
        filter.SortBy = NormalizeSort(filter.SortBy);
        filter.ViewMode = NormalizeViewMode(filter.ViewMode);

        var query = _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.VehicleCategory)
            .Where(vehicle => vehicle.Status == VehicleStatus.Available)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var keyword = filter.SearchTerm.Trim();
            query = query.Where(vehicle =>
                (vehicle.Brand + " " + vehicle.Model).Contains(keyword) ||
                vehicle.LicensePlate.Contains(keyword));
        }

        if (filter.VehicleCategoryId.HasValue)
        {
            query = query.Where(vehicle => vehicle.VehicleCategoryId == filter.VehicleCategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            query = query.Where(vehicle => vehicle.Brand == filter.Brand.Trim());
        }

        if (filter.MinDailyRate.HasValue)
        {
            query = query.Where(vehicle => vehicle.DailyRate >= filter.MinDailyRate.Value);
        }

        if (filter.MaxDailyRate.HasValue)
        {
            query = query.Where(vehicle => vehicle.DailyRate <= filter.MaxDailyRate.Value);
        }

        if (filter.SeatCount.HasValue)
        {
            query = query.Where(vehicle => vehicle.SeatCount >= filter.SeatCount.Value);
        }

        query = filter.SortBy switch
        {
            "priceDesc" => query.OrderByDescending(vehicle => vehicle.DailyRate).ThenBy(vehicle => vehicle.Brand),
            "newest" => query.OrderByDescending(vehicle => vehicle.Id),
            "popular" => query.OrderByDescending(vehicle => vehicle.Bookings.Count).ThenBy(vehicle => vehicle.Brand),
            _ => query.OrderBy(vehicle => vehicle.DailyRate).ThenBy(vehicle => vehicle.Brand)
        };

        var totalItems = await query.CountAsync();
        var page = filter.Page;
        var pageSize = filter.PageSize;

        var vehicles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        SaveSearchHistory(filter);

        var model = new CustomerVehicleIndexViewModel
        {
            Filter = filter,
            Vehicles = new PagedResult<Vehicle>
            {
                Items = vehicles,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            },
            CategoryOptions = await _context.VehicleCategories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .Select(category => new SelectListItem
                {
                    Text = category.Name,
                    Value = category.Id.ToString(),
                    Selected = filter.VehicleCategoryId == category.Id
                })
                .ToListAsync(),
            BrandOptions = await _context.Vehicles
                .AsNoTracking()
                .Where(vehicle => vehicle.Status == VehicleStatus.Available)
                .Select(vehicle => vehicle.Brand)
                .Distinct()
                .OrderBy(item => item)
                .ToListAsync(),
            PriceRangeMin = await _context.Vehicles.AsNoTracking().MinAsync(item => (decimal?)item.DailyRate) ?? 0,
            PriceRangeMax = await _context.Vehicles.AsNoTracking().MaxAsync(item => (decimal?)item.DailyRate) ?? 0,
            SearchHistory = HttpContext.Session.GetObject<List<VehicleSearchHistoryItem>>(SessionKeys.VehicleSearchHistory) ?? []
        };

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
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(item => item.VehicleCategory)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (vehicle is null)
        {
            return NotFound();
        }

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var endDate = startDate.AddDays(20);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.VehicleId == id &&
                           item.Status != BookingStatus.Cancelled &&
                           item.Status != BookingStatus.Rejected &&
                           item.Status != BookingStatus.Completed &&
                           item.ReturnDateTime.Date >= startDate.ToDateTime(TimeOnly.MinValue) &&
                           item.PickupDateTime.Date <= endDate.ToDateTime(TimeOnly.MinValue))
            .Select(item => new { item.PickupDateTime, item.ReturnDateTime })
            .ToListAsync();

        var availability = Enumerable.Range(0, 21)
            .Select(offset => startDate.AddDays(offset))
            .Select(date => new VehicleAvailabilitySlotViewModel
            {
                Date = date,
                IsAvailable = bookings.All(item => !(date.ToDateTime(TimeOnly.MinValue) >= item.PickupDateTime.Date && date.ToDateTime(TimeOnly.MinValue) <= item.ReturnDateTime.Date))
            })
            .ToList();

        var model = new VehicleDetailsViewModel
        {
            Vehicle = vehicle,
            GalleryUrls = BuildGalleryUrls(vehicle),
            AvailabilitySlots = availability
        };

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchSuggestions([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
        {
            return Json(Array.Empty<string>());
        }

        var keyword = term.Trim();
        var suggestions = await _context.Vehicles
            .AsNoTracking()
            .Where(item => item.Status == VehicleStatus.Available &&
                           (item.Brand.Contains(keyword) ||
                            item.Model.Contains(keyword) ||
                            item.LicensePlate.Contains(keyword)))
            .OrderBy(item => item.Brand)
            .ThenBy(item => item.Model)
            .Select(item => $"{item.Brand} {item.Model} ({item.LicensePlate})")
            .Distinct()
            .Take(8)
            .ToListAsync();

        return Json(suggestions);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VehicleComments(Guid vehicleId)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == vehicleId);

        if (vehicle is null)
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

    private static IReadOnlyList<string> BuildGalleryUrls(Vehicle vehicle)
    {
        var urls = new List<string>();

        if (!string.IsNullOrWhiteSpace(vehicle.ImageUrl))
        {
            urls.Add(vehicle.ImageUrl);
        }

        var relativeFolder = Path.Combine("Content", "Images", "Vehicles", vehicle.Id.ToString("N"));
        var absoluteFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeFolder);
        if (Directory.Exists(absoluteFolder))
        {
            urls.AddRange(Directory
                .GetFiles(absoluteFolder)
                .OrderBy(path => path)
                .Select(path => $"/{relativeFolder.Replace("\\", "/")}/{Path.GetFileName(path)}"));
        }

        return urls.Distinct().Take(8).ToList();
    }
}
