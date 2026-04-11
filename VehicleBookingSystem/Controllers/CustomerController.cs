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
            query = query.Where(vehicle => vehicle.Brand == filter.Brand);
        }

        if (filter.MinDailyRate.HasValue)
        {
            query = query.Where(vehicle => vehicle.DailyRate >= filter.MinDailyRate.Value);
        }

        if (filter.MaxDailyRate.HasValue)
        {
            query = query.Where(vehicle => vehicle.DailyRate <= filter.MaxDailyRate.Value);
        }

        var totalItems = await query.CountAsync();
        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize <= 0 ? 8 : filter.PageSize;

        var vehicles = await query
            .OrderBy(vehicle => vehicle.Brand)
            .ThenBy(vehicle => vehicle.Model)
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
            !filter.MaxDailyRate.HasValue)
        {
            return;
        }

        var history = HttpContext.Session.GetObject<List<VehicleSearchHistoryItem>>(SessionKeys.VehicleSearchHistory) ?? [];
        history.Insert(0, new VehicleSearchHistoryItem
        {
            At = DateTime.UtcNow,
            Keyword = filter.SearchTerm ?? string.Empty,
            FilterSummary = $"Category: {(filter.VehicleCategoryId?.ToString() ?? "Any")}, Brand: {(filter.Brand ?? "Any")}, Rate: {(filter.MinDailyRate?.ToString() ?? "0")} - {(filter.MaxDailyRate?.ToString() ?? "Any")}" 
        });

        if (history.Count > 10)
        {
            history = history.Take(10).ToList();
        }

        HttpContext.Session.SetObject(SessionKeys.VehicleSearchHistory, history);
    }
}
