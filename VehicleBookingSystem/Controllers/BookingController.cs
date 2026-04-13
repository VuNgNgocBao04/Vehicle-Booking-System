using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleBookingSystem.Extensions;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Services;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Controllers;

public class BookingController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IBookingService _bookingService;

    public BookingController(UserManager<AppUser> userManager, IBookingService bookingService)
    {
        _userManager = userManager;
        _bookingService = bookingService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Create(Guid vehicleId)
    {
        var draft = HttpContext.Session.GetObject<BookingDraftSessionModel>(SessionKeys.PendingBookingDraft);
        var currentUser = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
        var model = await _bookingService.BuildCreateViewModelAsync(vehicleId, draft, currentUser);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (model.ReturnDateTime <= model.PickupDateTime)
        {
            ModelState.AddModelError(nameof(model.ReturnDateTime), "Return date must be later than pickup date.");
        }

        var availability = await _bookingService.CheckAvailabilityAsync(model.VehicleId, model.PickupDateTime, model.ReturnDateTime);
        var available = availability.IsAvailable;
        if (!available)
        {
            ModelState.AddModelError(string.Empty, "Vehicle is not available in the selected period.");
        }

        var currentUser = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
        var baseModel = await _bookingService.BuildCreateViewModelAsync(model.VehicleId, null, currentUser);
        if (baseModel is null)
        {
            return NotFound();
        }

        baseModel.PickupLocation = model.PickupLocation;
        baseModel.DropoffLocation = model.DropoffLocation;
        baseModel.PickupDateTime = model.PickupDateTime;
        baseModel.ReturnDateTime = model.ReturnDateTime;
        baseModel.PaymentMethod = model.PaymentMethod;
        baseModel.EstimatedTotalAmount = CalculateTotal(baseModel.DailyRate, model.PickupDateTime, model.ReturnDateTime);
        model.PaymentMethodOptions = BuildPaymentMethodOptions(model.PaymentMethod);

        if (!ModelState.IsValid)
        {
            baseModel.PaymentMethodOptions = BuildPaymentMethodOptions(model.PaymentMethod);
            return View(baseModel);
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            HttpContext.Session.SetObject(SessionKeys.PendingBookingDraft, new BookingDraftSessionModel
            {
                VehicleId = model.VehicleId,
                PickupLocation = model.PickupLocation,
                DropoffLocation = model.DropoffLocation,
                PickupDateTime = model.PickupDateTime,
                ReturnDateTime = model.ReturnDateTime,
                PaymentMethod = model.PaymentMethod
            });

            TempData["Message"] = "Please login to complete your booking. Draft has been saved.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Create), new { vehicleId = model.VehicleId }) });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Forbid();
        }

        var createResult = await _bookingService.CreateAsync(model, parsedUserId, User.Identity?.Name);
        if (!createResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, createResult.ErrorMessage ?? "Unable to create booking right now. Please try again.");
            baseModel.PaymentMethodOptions = BuildPaymentMethodOptions(model.PaymentMethod);
            return View(baseModel);
        }

        HttpContext.Session.Remove(SessionKeys.PendingBookingDraft);
        TempData["Message"] = "Booking created successfully.";
        return RedirectToAction(nameof(Success), new { id = createResult.BookingId!.Value });
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Success(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Forbid();
        }

        var booking = await _bookingService.GetSuccessAsync(id, parsedUserId);

        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyBookings(string? searchTerm, BookingStatus? status, int page = 1, int pageSize = 8)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Forbid();
        }

        var model = await _bookingService.BuildHistoryAsync(parsedUserId, searchTerm, status, page, pageSize);

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Forbid();
        }

        var booking = await _bookingService.GetDetailsAsync(id, parsedUserId);

        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Forbid();
        }

        var result = await _bookingService.CancelAsync(id, parsedUserId, User.Identity?.Name);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage ?? "Unable to cancel booking.";
            return RedirectToAction(nameof(MyBookings));
        }

        TempData["Message"] = "Booking cancelled successfully.";
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability(Guid vehicleId, DateTime pickupDateTime, DateTime returnDateTime)
    {
        var availability = await _bookingService.CheckAvailabilityAsync(vehicleId, pickupDateTime, returnDateTime);
        return Json(new
        {
            available = availability.IsAvailable,
            message = availability.Message,
            totalAmount = availability.TotalAmount
        });
    }

    private static decimal CalculateTotal(decimal dailyRate, DateTime pickupDateTime, DateTime returnDateTime)
    {
        var days = Math.Max((returnDateTime.Date - pickupDateTime.Date).Days, 1);
        return days * dailyRate;
    }

    private static IReadOnlyList<SelectListItem> BuildPaymentMethodOptions(PaymentMethod selected)
    {
        return Enum.GetValues<PaymentMethod>()
            .Select(method => new SelectListItem
            {
                Text = method.ToString(),
                Value = ((int)method).ToString(),
                Selected = method == selected
            })
            .ToList();
    }
}
