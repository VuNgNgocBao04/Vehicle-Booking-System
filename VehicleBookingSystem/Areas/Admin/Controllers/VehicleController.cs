using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using VehicleBookingSystem.Contracts.Common;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.ViewModels;
using VehicleBookingSystem.Services;

namespace VehicleBookingSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VehicleController : Controller
{
    private readonly ApplicationDbContext _context;
<<<<<<< HEAD
    private readonly IVehicleService _vehicleService;

    public VehicleController(
        ApplicationDbContext context,
        IVehicleService vehicleService)
    {
        _context = context;
        _vehicleService = vehicleService;
=======
    private readonly IFileStorageService _fileStorageService;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IWebHostEnvironment _environment;

    public VehicleController(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        IHtmlSanitizerService htmlSanitizer,
        IWebHostEnvironment environment)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _htmlSanitizer = htmlSanitizer;
        _environment = environment;
>>>>>>> origin/dev
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new AdminVehicleIndexViewModel
        {
            CategoryOptions = await BuildCategoryOptionsAsync(null),
            StatusOptions = BuildStatusOptions(null)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TableData()
    {
        var form = Request.Form;
        var draw = ParseInt(form["draw"], 1);
        var start = Math.Max(ParseInt(form["start"], 0), 0);
        var length = Math.Clamp(ParseInt(form["length"], 10), 5, 100);
        var search = form["search[value]"].ToString().Trim();
        var categoryIdRaw = form["categoryId"].ToString();
        var statusRaw = form["status"].ToString();

        var categoryId = Guid.TryParse(categoryIdRaw, out var parsedCategoryId) ? (Guid?)parsedCategoryId : null;
        var status = Enum.TryParse<VehicleStatus>(statusRaw, true, out var parsedStatus) ? (VehicleStatus?)parsedStatus : null;

        var query = _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.VehicleCategory)
            .AsQueryable();

        var recordsTotal = await _context.Vehicles.CountAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(vehicle =>
                vehicle.Code.Contains(search) ||
                vehicle.Brand.Contains(search) ||
                vehicle.Model.Contains(search) ||
                vehicle.LicensePlate.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(vehicle => vehicle.VehicleCategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(vehicle => vehicle.Status == status.Value);
        }

        var recordsFiltered = await query.CountAsync();

        var orderColumn = ParseInt(form["order[0][column]"], 0);
        var orderDir = form["order[0][dir]"].ToString();
        var descending = string.Equals(orderDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = orderColumn switch
        {
            1 => descending ? query.OrderByDescending(vehicle => vehicle.Brand).ThenByDescending(vehicle => vehicle.Model) : query.OrderBy(vehicle => vehicle.Brand).ThenBy(vehicle => vehicle.Model),
            2 => descending ? query.OrderByDescending(vehicle => vehicle.VehicleCategory!.Name) : query.OrderBy(vehicle => vehicle.VehicleCategory!.Name),
            3 => descending ? query.OrderByDescending(vehicle => vehicle.LicensePlate) : query.OrderBy(vehicle => vehicle.LicensePlate),
            4 => descending ? query.OrderByDescending(vehicle => vehicle.DailyRate) : query.OrderBy(vehicle => vehicle.DailyRate),
            5 => descending ? query.OrderByDescending(vehicle => vehicle.Status) : query.OrderBy(vehicle => vehicle.Status),
            _ => descending ? query.OrderByDescending(vehicle => vehicle.Code) : query.OrderBy(vehicle => vehicle.Code)
        };

        var culture = CultureInfo.CurrentCulture;

        var data = await query
            .Skip(start)
            .Take(length)
            .Select(vehicle => new
            {
                id = vehicle.Id,
                code = vehicle.Code,
                vehicleName = vehicle.Brand + " " + vehicle.Model,
                category = vehicle.VehicleCategory != null ? vehicle.VehicleCategory.Name : "-",
                licensePlate = vehicle.LicensePlate,
                dailyRate = vehicle.DailyRate.ToString("C0", culture),
                status = GetVehicleStatusText(vehicle.Status),
                statusBadge = GetVehicleStatusBadge(vehicle.Status),
                actions = new
                {
                    detailUrl = Url.Action(nameof(Details), new { id = vehicle.Id }),
                    editUrl = Url.Action(nameof(Edit), new { id = vehicle.Id })
                }
            })
            .ToListAsync();

        return Json(new
        {
            draw,
            recordsTotal,
            recordsFiltered,
            data
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(Guid id)
    {
        var result = await _vehicleService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<string>.Fail(result.ErrorMessage ?? "Unable to delete vehicle."));
        }

        return Json(ApiResponse<string>.Ok("Vehicle deleted successfully."));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkDelete([FromForm] List<Guid> ids)
    {
        if (ids.Count == 0)
        {
            return BadRequest(ApiResponse<string>.Fail("No vehicles selected."));
        }

        var skipped = 0;

        foreach (var vehicleId in ids)
        {
            var result = await _vehicleService.DeleteAsync(vehicleId);
            if (!result.Succeeded)
            {
                skipped++;
            }
        }

        return Json(ApiResponse<string>.Ok(skipped > 0
            ? $"Deleted {ids.Count - skipped} vehicle(s). Skipped {skipped} active vehicle(s)."
            : $"Deleted {ids.Count} vehicle(s)."));
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(string? searchTerm, Guid? categoryId, VehicleStatus? status)
    {
        var query = _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.VehicleCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(vehicle =>
                vehicle.Code.Contains(searchTerm) ||
                vehicle.Brand.Contains(searchTerm) ||
                vehicle.Model.Contains(searchTerm) ||
                vehicle.LicensePlate.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(vehicle => vehicle.VehicleCategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(vehicle => vehicle.Status == status.Value);
        }

        var vehicles = await query.OrderBy(vehicle => vehicle.Brand).ThenBy(vehicle => vehicle.Model).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Code,Vehicle,Category,LicensePlate,DailyRate,Status");

        foreach (var vehicle in vehicles)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(vehicle.Code),
                EscapeCsv($"{vehicle.Brand} {vehicle.Model}"),
                EscapeCsv(vehicle.VehicleCategory?.Name ?? string.Empty),
                EscapeCsv(vehicle.LicensePlate),
                EscapeCsv(vehicle.DailyRate.ToString("0.##", CultureInfo.InvariantCulture)),
                EscapeCsv(GetVehicleStatusText(vehicle.Status))));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        var fileName = $"vehicles-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
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
        ValidateGalleryFiles(form.GalleryFiles);

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

        var createResult = await _vehicleService.CreateAsync(form);
        if (!createResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, createResult.ErrorMessage ?? "Unable to create vehicle.");
            await LoadLookupAsync(form.VehicleCategoryId);
            return View(form);
        }

        TempData["Message"] = "Vehicle created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await _vehicleService.BuildEditViewModelAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        await LoadLookupAsync(model.VehicleCategoryId);
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

        ValidateGalleryFiles(form.GalleryFiles);

        if (!ModelState.IsValid)
        {
            await LoadLookupAsync(form.VehicleCategoryId);
            return View(form);
        }

        var updateResult = await _vehicleService.UpdateAsync(id, form);
        if (!updateResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, updateResult.ErrorMessage ?? "Unable to update vehicle.");
            await LoadLookupAsync(form.VehicleCategoryId);
            return View(form);
        }
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
        var result = await _vehicleService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage ?? "Unable to delete vehicle.";
            return RedirectToAction(nameof(Delete), new { id });
        }

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

    private void ValidateGalleryFiles(IReadOnlyList<IFormFile>? files)
    {
        if (files is null || files.Count == 0)
        {
            return;
        }

        var invalid = files.Any(file =>
            file.Length <= 0 ||
            file.Length > 5 * 1024 * 1024 ||
            !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase));

        if (invalid)
        {
            ModelState.AddModelError(nameof(VehicleFormViewModel.GalleryFiles), "Chỉ chấp nhận ảnh jpg/png/webp và mỗi file tối đa 5MB.");
        }
    }

<<<<<<< HEAD
    private static int ParseInt(string? raw, int fallback)
    {
        return int.TryParse(raw, out var value) ? value : fallback;
    }

    private static string EscapeCsv(string input)
    {
        if (input.Contains(',') || input.Contains('"') || input.Contains('\n'))
=======
    private IReadOnlyList<string> BuildGalleryUrls(Guid vehicleId)
    {
        var root = Path.Combine(_environment.WebRootPath, "Content", "Images", "Vehicles", vehicleId.ToString("N"));
        if (!Directory.Exists(root))
>>>>>>> origin/dev
        {
            return $"\"{input.Replace("\"", "\"\"")}\"";
        }

        return input;
    }

    private static string GetVehicleStatusBadge(VehicleStatus status)
    {
        return status switch
        {
            VehicleStatus.Available => "success",
            VehicleStatus.InUse => "info",
            VehicleStatus.Maintenance => "warning",
            VehicleStatus.Reserved => "primary",
            VehicleStatus.Disabled => "secondary",
            _ => "secondary"
        };
    }

    private static string GetVehicleStatusText(VehicleStatus status)
    {
        var isEnglish = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);

        if (isEnglish)
        {
            return status switch
            {
                VehicleStatus.Available => "Available",
                VehicleStatus.InUse => "In Use",
                VehicleStatus.Maintenance => "Maintenance",
                VehicleStatus.Reserved => "Reserved",
                VehicleStatus.Disabled => "Disabled",
                _ => status.ToString()
            };
        }

        return status switch
        {
            VehicleStatus.Available => "Sẵn sàng",
            VehicleStatus.InUse => "Đang thuê",
            VehicleStatus.Maintenance => "Bảo dưỡng",
            VehicleStatus.Reserved => "Đã giữ chỗ",
            VehicleStatus.Disabled => "Ngưng hoạt động",
            _ => status.ToString()
        };
    }
}
