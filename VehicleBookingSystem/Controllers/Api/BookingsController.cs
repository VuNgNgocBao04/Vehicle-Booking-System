using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VehicleBookingSystem.Contracts.Bookings;
using VehicleBookingSystem.Contracts.Common;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Services;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Controllers.Api;

[ApiController]
[Route("api/bookings")]
[Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ApiProblemDetailsFactory _problemDetailsFactory;

    public BookingsController(IBookingService bookingService, ApiProblemDetailsFactory problemDetailsFactory)
    {
        _bookingService = bookingService;
        _problemDetailsFactory = problemDetailsFactory;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<BookingResponse>>>> GetBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!TryGetCurrentUserId(out var userId) && !User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, _problemDetailsFactory.Create(StatusCodes.Status403Forbidden, "Forbidden", "Không có quyền truy cập."));
        }

        var bookings = await _bookingService.GetApiBookingsAsync(userId, User.IsInRole("Admin"), page, pageSize);
        var items = bookings.Items.Select(MapBookingResponse).ToList();
        var response = new PagedResponse<BookingResponse>
        {
            Items = items,
            Page = bookings.Page,
            PageSize = bookings.PageSize,
            TotalItems = bookings.TotalItems
        };

        return Ok(ApiResponse<PagedResponse<BookingResponse>>.Ok(response));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> GetById(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId) && !User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, _problemDetailsFactory.Create(StatusCodes.Status403Forbidden, "Forbidden", "Không có quyền truy cập."));
        }

        var booking = await _bookingService.GetApiBookingAsync(id, userId, User.IsInRole("Admin"));
        if (booking is null)
        {
            return NotFound(_problemDetailsFactory.Create(StatusCodes.Status404NotFound, "Not Found", "Không tìm thấy đơn đặt xe."));
        }

        return Ok(ApiResponse<BookingResponse>.Ok(MapBookingResponse(booking)));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Create([FromBody] BookingCreateRequest request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, _problemDetailsFactory.Create(StatusCodes.Status403Forbidden, "Forbidden", "Không có quyền truy cập."));
        }

        var createModel = new BookingCreateViewModel
        {
            VehicleId = request.VehicleId,
            PickupLocation = request.PickupLocation,
            DropoffLocation = request.DropoffLocation,
            PickupDateTime = request.PickupDateTime,
            ReturnDateTime = request.ReturnDateTime,
            PaymentMethod = request.PaymentMethod
        };

        var result = await _bookingService.CreateAsync(createModel, userId, User.Identity?.Name);
        if (!result.Succeeded || result.BookingId is null)
        {
            var statusCode = result.ErrorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, _problemDetailsFactory.Create(statusCode, statusCode == StatusCodes.Status404NotFound ? "Not Found" : "Bad Request", result.ErrorMessage ?? "Không thể tạo đơn đặt xe."));
        }

        var booking = await _bookingService.GetApiBookingAsync(result.BookingId.Value, userId, true);
        if (booking is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, _problemDetailsFactory.Create(StatusCodes.Status500InternalServerError, "Internal Server Error", "Không đọc được dữ liệu booking vừa tạo."));
        }

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, ApiResponse<BookingResponse>.Ok(MapBookingResponse(booking), "Booking created successfully."));
    }

    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId) && !User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, _problemDetailsFactory.Create(StatusCodes.Status403Forbidden, "Forbidden", "Không có quyền truy cập."));
        }

        var result = await _bookingService.CancelApiAsync(id, userId, User.IsInRole("Admin"), User.Identity?.Name);
        if (!result.Succeeded)
        {
            var statusCode = result.ErrorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, _problemDetailsFactory.Create(statusCode, statusCode == StatusCodes.Status404NotFound ? "Not Found" : "Bad Request", result.ErrorMessage ?? "Không thể hủy booking."));
        }

        return Ok(ApiResponse<string>.Ok("Đã hủy đơn đặt xe."));
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
