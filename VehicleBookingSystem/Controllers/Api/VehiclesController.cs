using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Contracts.Common;
using VehicleBookingSystem.Contracts.Vehicles;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Services;

namespace VehicleBookingSystem.Controllers.Api;

[ApiController]
[Route("api/vehicles")]
[Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
public class VehiclesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IWebHostEnvironment _environment;
<<<<<<< HEAD
    private readonly ApiProblemDetailsFactory _problemDetailsFactory;

    public VehiclesController(ApplicationDbContext context, IHtmlSanitizerService htmlSanitizer, IWebHostEnvironment environment, ApiProblemDetailsFactory problemDetailsFactory)
=======

    public VehiclesController(ApplicationDbContext context, IHtmlSanitizerService htmlSanitizer, IWebHostEnvironment environment)
>>>>>>> origin/dev
    {
        _context = context;
        _htmlSanitizer = htmlSanitizer;
        _environment = environment;
<<<<<<< HEAD
        _problemDetailsFactory = problemDetailsFactory;
=======
>>>>>>> origin/dev
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<VehicleResponse>>>> GetVehicles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.VehicleCategory)
            .AsQueryable();

        if (category.HasValue)
        {
            query = query.Where(vehicle => vehicle.VehicleCategoryId == category.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(vehicle => vehicle.DailyRate >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(vehicle => vehicle.DailyRate <= maxPrice.Value);
        }

        var totalItems = await query.CountAsync();
        var vehicles = await query
            .OrderBy(vehicle => vehicle.Brand)
            .ThenBy(vehicle => vehicle.Model)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

<<<<<<< HEAD
        return Ok(ApiResponse<PagedResponse<VehicleResponse>>.Ok(new PagedResponse<VehicleResponse>
=======
        var items = vehicles
            .Select(vehicle => MapVehicleResponse(vehicle, BuildGalleryUrls(vehicle.Id)))
            .ToList();

        return Ok(new PagedResponse<VehicleResponse>
>>>>>>> origin/dev
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        }));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VehicleResponse>>> GetById(Guid id)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(item => item.VehicleCategory)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (vehicle is null)
        {
            return NotFound(_problemDetailsFactory.Create(StatusCodes.Status404NotFound, "Not Found", "Không tìm thấy xe."));
        }

        return Ok(ApiResponse<VehicleResponse>.Ok(MapVehicleResponse(vehicle, BuildGalleryUrls(vehicle.Id))));
    }

    [HttpGet("{id:guid}/availability")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VehicleAvailabilityResponse>>> CheckAvailability(Guid id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (endDate <= startDate)
        {
            return BadRequest(_problemDetailsFactory.Create(StatusCodes.Status400BadRequest, "Bad Request", "Ngày kết thúc phải sau ngày bắt đầu."));
        }

        var vehicleExists = await _context.Vehicles.AsNoTracking().AnyAsync(item => item.Id == id);
        if (!vehicleExists)
        {
            return NotFound(_problemDetailsFactory.Create(StatusCodes.Status404NotFound, "Not Found", "Không tìm thấy xe."));
        }

        var isAvailable = !await _context.Bookings.AnyAsync(booking =>
            booking.VehicleId == id &&
            booking.Status != BookingStatus.Cancelled &&
            booking.Status != BookingStatus.Rejected &&
            booking.Status != BookingStatus.Completed &&
            startDate < booking.ReturnDateTime &&
            endDate > booking.PickupDateTime);

        return Ok(ApiResponse<VehicleAvailabilityResponse>.Ok(new VehicleAvailabilityResponse
        {
            VehicleId = id,
            StartDate = startDate,
            EndDate = endDate,
            IsAvailable = isAvailable,
            Message = isAvailable ? "Xe đang trống trong khoảng thời gian đã chọn." : "Xe đã được đặt trong khoảng thời gian này."
        }));
    }

    [HttpPost]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Identity.Application,Bearer")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<VehicleResponse>>> Create([FromBody] VehicleUpsertRequest request)
    {
        if (await _context.Vehicles.AnyAsync(vehicle => vehicle.Code == request.Code || vehicle.LicensePlate == request.LicensePlate))
        {
            ModelState.AddModelError(nameof(request.Code), "Mã xe hoặc biển số đã tồn tại.");
            return ValidationProblem(ModelState);
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            VehicleCategoryId = request.VehicleCategoryId,
            Code = request.Code.Trim(),
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            LicensePlate = request.LicensePlate.Trim(),
            SeatCount = request.SeatCount,
            Color = request.Color.Trim(),
            Transmission = request.Transmission.Trim(),
            FuelType = request.FuelType.Trim(),
            DailyRate = request.DailyRate,
            Status = VehicleStatus.Available,
            Description = _htmlSanitizer.Sanitize(request.Description),
            BookingPolicyHtml = _htmlSanitizer.Sanitize(request.BookingPolicyHtml)
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var response = MapVehicleResponse(vehicle, []);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, ApiResponse<VehicleResponse>.Ok(response, "Vehicle created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Identity.Application,Bearer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VehicleResponse>>> Update(Guid id, [FromBody] VehicleUpsertRequest request)
    {
        var vehicle = await _context.Vehicles
            .Include(item => item.VehicleCategory)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (vehicle is null)
        {
            return NotFound(_problemDetailsFactory.Create(StatusCodes.Status404NotFound, "Not Found", "Không tìm thấy xe."));
        }

        var duplicateExists = await _context.Vehicles.AnyAsync(item =>
            item.Id != id &&
            (item.Code == request.Code || item.LicensePlate == request.LicensePlate));

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(request.Code), "Mã xe hoặc biển số đã tồn tại.");
            return ValidationProblem(ModelState);
        }

        vehicle.VehicleCategoryId = request.VehicleCategoryId;
        vehicle.Code = request.Code.Trim();
        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.LicensePlate = request.LicensePlate.Trim();
        vehicle.SeatCount = request.SeatCount;
        vehicle.Color = request.Color.Trim();
        vehicle.Transmission = request.Transmission.Trim();
        vehicle.FuelType = request.FuelType.Trim();
        vehicle.DailyRate = request.DailyRate;
        vehicle.Description = _htmlSanitizer.Sanitize(request.Description);
        vehicle.BookingPolicyHtml = _htmlSanitizer.Sanitize(request.BookingPolicyHtml);

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<VehicleResponse>.Ok(MapVehicleResponse(vehicle, BuildGalleryUrls(vehicle.Id)), "Vehicle updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Identity.Application,Bearer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(item => item.Id == id);
        if (vehicle is null)
        {
            return NotFound(_problemDetailsFactory.Create(StatusCodes.Status404NotFound, "Not Found", "Không tìm thấy xe."));
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Đã xóa xe thành công."));
    }

    private VehicleResponse MapVehicleResponse(Vehicle vehicle, IReadOnlyList<string> galleryUrls)
    {
        return new VehicleResponse
        {
            Id = vehicle.Id,
            Code = vehicle.Code,
            VehicleCategoryId = vehicle.VehicleCategoryId,
            VehicleCategoryName = vehicle.VehicleCategory?.Name,
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
            GalleryUrls = galleryUrls,
            Description = _htmlSanitizer.Sanitize(vehicle.Description),
            BookingPolicyHtml = _htmlSanitizer.Sanitize(vehicle.BookingPolicyHtml)
        };
    }

    private IReadOnlyList<string> BuildGalleryUrls(Guid vehicleId)
    {
<<<<<<< HEAD
        var relativeFolder = Path.Combine("Content", "Images", "Vehicles", vehicleId.ToString("N"));
        var webRoot = Path.Combine(_environment.WebRootPath, relativeFolder);
=======
        var webRoot = Path.Combine(_environment.WebRootPath, "Content", "Images", "Vehicles", vehicleId.ToString("N"));
>>>>>>> origin/dev
        if (!Directory.Exists(webRoot))
        {
            return [];
        }

        return Directory
            .GetFiles(webRoot)
            .OrderBy(path => path)
            .Select(path => $"/Content/Images/Vehicles/{vehicleId:N}/{Path.GetFileName(path)}")
            .ToList();
    }
}
