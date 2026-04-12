using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Contracts.Bookings;
using VehicleBookingSystem.Contracts.Common;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;

namespace VehicleBookingSystem.Controllers.Api;

[ApiController]
[Route("api/bookings")]
[Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
public class BookingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<BookingResponse>>> GetBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Vehicle)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Forbid();
            }

            query = query.Where(booking => booking.UserId == userId);
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(booking => booking.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(booking => MapBookingResponse(booking))
            .ToListAsync();

        return Ok(new PagedResponse<BookingResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Vehicle)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (booking is null)
        {
            return NotFound(new { message = "Không tìm thấy đơn đặt xe." });
        }

        if (!User.IsInRole("Admin"))
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Forbid();
            }

            if (booking.UserId != userId)
            {
                return Forbid();
            }
        }

        return Ok(MapBookingResponse(booking));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] BookingCreateRequest request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.VehicleId && item.Status == VehicleStatus.Available);

        if (vehicle is null)
        {
            return NotFound(new { message = "Không tìm thấy xe khả dụng." });
        }

        var isAvailable = !await _context.Bookings.AnyAsync(booking =>
            booking.VehicleId == request.VehicleId &&
            booking.Status != BookingStatus.Cancelled &&
            booking.Status != BookingStatus.Rejected &&
            booking.Status != BookingStatus.Completed &&
            request.PickupDateTime < booking.ReturnDateTime &&
            request.ReturnDateTime > booking.PickupDateTime);

        if (!isAvailable)
        {
            return BadRequest(new { message = "Xe không còn trống trong khoảng thời gian đã chọn." });
        }

        var totalAmount = CalculateTotal(vehicle.DailyRate, request.PickupDateTime, request.ReturnDateTime);

        var bookingEntity = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = $"BK-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            UserId = userId,
            VehicleId = request.VehicleId,
            PickupLocation = request.PickupLocation.Trim(),
            DropoffLocation = request.DropoffLocation.Trim(),
            PickupDateTime = request.PickupDateTime,
            ReturnDateTime = request.ReturnDateTime,
            TotalAmount = totalAmount,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentMethod = request.PaymentMethod,
                PaidAmount = totalAmount,
                Status = PaymentStatus.Pending
            }
        };

        _context.Bookings.Add(bookingEntity);
        await _context.SaveChangesAsync();

        var response = await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Vehicle)
            .Where(item => item.Id == bookingEntity.Id)
            .Select(item => MapBookingResponse(item))
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = bookingEntity.Id }, response);
    }

    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(item => item.Id == id);
        if (booking is null)
        {
            return NotFound(new { message = "Không tìm thấy đơn đặt xe." });
        }

        if (!User.IsInRole("Admin"))
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Forbid();
            }

            if (booking.UserId != userId)
            {
                return Forbid();
            }
        }

        if (booking.PickupDateTime <= DateTime.UtcNow.AddHours(6))
        {
            return BadRequest(new { message = "Chỉ được hủy trước giờ nhận xe tối thiểu 6 tiếng." });
        }

        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã hủy đơn đặt xe." });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userIdText) && Guid.TryParse(userIdText, out userId);
    }

    private static decimal CalculateTotal(decimal dailyRate, DateTime pickupDateTime, DateTime returnDateTime)
    {
        var days = Math.Max((returnDateTime.Date - pickupDateTime.Date).Days, 1);
        return days * dailyRate;
    }

    private static BookingResponse MapBookingResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            UserId = booking.UserId,
            VehicleId = booking.VehicleId,
            VehicleDisplayName = booking.Vehicle == null ? null : $"{booking.Vehicle.Brand} {booking.Vehicle.Model}",
            PickupDateTime = booking.PickupDateTime,
            ReturnDateTime = booking.ReturnDateTime,
            PickupLocation = booking.PickupLocation,
            DropoffLocation = booking.DropoffLocation,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt
        };
    }
}
