using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VehicleController : Controller
{
    private readonly ApplicationDbContext _context;

    public VehicleController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] VehicleFilterViewModel filter)
    {
        var query = _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.VehicleCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var keyword = filter.SearchTerm.Trim();
            query = query.Where(vehicle =>
                (vehicle.Brand + " " + vehicle.Model).Contains(keyword) ||
                vehicle.LicensePlate.Contains(keyword) ||
                vehicle.Code.Contains(keyword));
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

        if (filter.Status.HasValue)
        {
            query = query.Where(vehicle => vehicle.Status == filter.Status.Value);
        }

        var totalItems = await query.CountAsync();
        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var vehicles = await query
            .OrderBy(vehicle => vehicle.Brand)
            .ThenBy(vehicle => vehicle.Model)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new AdminVehicleIndexViewModel
        {
            Filter = filter,
            Vehicles = new PagedResult<Vehicle>
            {
                Items = vehicles,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            },
            CategoryOptions = await BuildCategoryOptionsAsync(filter.VehicleCategoryId),
            StatusOptions = BuildStatusOptions(filter.Status)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadLookupAsync();
        return View(new VehicleFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookupAsync(form.VehicleCategoryId);
            return View(form);
        }

        if (await _context.Vehicles.AnyAsync(vehicle => vehicle.Code == form.Code || vehicle.LicensePlate == form.LicensePlate))
        {
            ModelState.AddModelError(string.Empty, "Vehicle code or license plate already exists.");
            await LoadLookupAsync(form.VehicleCategoryId);
            return View(form);
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            VehicleCategoryId = form.VehicleCategoryId,
            Code = form.Code.Trim(),
            Brand = form.Brand.Trim(),
            Model = form.Model.Trim(),
            LicensePlate = form.LicensePlate.Trim(),
            SeatCount = form.SeatCount,
            Color = form.Color.Trim(),
            Transmission = form.Transmission.Trim(),
            FuelType = form.FuelType.Trim(),
            DailyRate = form.DailyRate,
            Status = form.Status,
            ImageUrl = form.ImageUrl,
            Description = form.Description
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Vehicle created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        var model = new VehicleFormViewModel
        {
            Id = vehicle.Id,
            VehicleCategoryId = vehicle.VehicleCategoryId,
            Code = vehicle.Code,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            LicensePlate = vehicle.LicensePlate,
            SeatCount = vehicle.SeatCount,
            Color = vehicle.Color,
            Transmission = vehicle.Transmission,
            FuelType = vehicle.FuelType,
            DailyRate = vehicle.DailyRate,
            Status = vehicle.Status,
            ImageUrl = vehicle.ImageUrl,
            Description = vehicle.Description
        };

        await LoadLookupAsync(vehicle.VehicleCategoryId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, VehicleFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadLookupAsync(form.VehicleCategoryId);
            return View(form);
        }

        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        var duplicateExists = await _context.Vehicles.AnyAsync(item =>
            item.Id != id &&
            (item.Code == form.Code || item.LicensePlate == form.LicensePlate));

        if (duplicateExists)
        {
            ModelState.AddModelError(string.Empty, "Vehicle code or license plate already exists.");
            await LoadLookupAsync(form.VehicleCategoryId);
            return View(form);
        }

        vehicle.VehicleCategoryId = form.VehicleCategoryId;
        vehicle.Code = form.Code.Trim();
        vehicle.Brand = form.Brand.Trim();
        vehicle.Model = form.Model.Trim();
        vehicle.LicensePlate = form.LicensePlate.Trim();
        vehicle.SeatCount = form.SeatCount;
        vehicle.Color = form.Color.Trim();
        vehicle.Transmission = form.Transmission.Trim();
        vehicle.FuelType = form.FuelType.Trim();
        vehicle.DailyRate = form.DailyRate;
        vehicle.Status = form.Status;
        vehicle.ImageUrl = form.ImageUrl;
        vehicle.Description = form.Description;

        await _context.SaveChangesAsync();
        TempData["Message"] = "Vehicle updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(item => item.VehicleCategory)
            .FirstOrDefaultAsync(item => item.Id == id);

        return vehicle is null ? NotFound() : View(vehicle);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(item => item.VehicleCategory)
            .FirstOrDefaultAsync(item => item.Id == id);

        return vehicle is null ? NotFound() : View(vehicle);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        var hasActiveBookings = await _context.Bookings.AnyAsync(booking =>
            booking.VehicleId == id &&
            (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed) &&
            booking.ReturnDateTime >= DateTime.UtcNow);

        if (hasActiveBookings)
        {
            TempData["Error"] = "Cannot delete this vehicle because it has active bookings.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Vehicle deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookupAsync(Guid? selectedCategoryId = null)
    {
        ViewBag.CategoryOptions = await BuildCategoryOptionsAsync(selectedCategoryId);
        ViewBag.StatusOptions = BuildStatusOptions(null);
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildCategoryOptionsAsync(Guid? selectedCategoryId)
    {
        return await _context.VehicleCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new SelectListItem
            {
                Text = category.Name,
                Value = category.Id.ToString(),
                Selected = selectedCategoryId == category.Id
            })
            .ToListAsync();
    }

    private static IReadOnlyList<SelectListItem> BuildStatusOptions(VehicleStatus? selectedStatus)
    {
        return Enum.GetValues<VehicleStatus>()
            .Select(status => new SelectListItem
            {
                Text = status.ToString(),
                Value = ((int)status).ToString(),
                Selected = selectedStatus == status
            })
            .ToList();
    }
}
