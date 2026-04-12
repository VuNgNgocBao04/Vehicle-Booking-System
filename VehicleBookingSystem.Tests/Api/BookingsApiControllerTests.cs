using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleBookingSystem.Contracts.Bookings;
using VehicleBookingSystem.Contracts.Common;
using VehicleBookingSystem.Controllers.Api;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Services;

namespace VehicleBookingSystem.Tests.Api;

public class BookingsApiControllerTests
{
    [Fact]
    public async Task GetById_ReturnsNotFound_WhenBookingMissing()
    {
        await using var context = BuildContext(nameof(GetById_ReturnsNotFound_WhenBookingMissing));
        var controller = BuildController(context);

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithCalculatedTotalAmount()
    {
        await using var context = BuildContext(nameof(Create_ReturnsCreated_WithCalculatedTotalAmount));

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            VehicleCategoryId = Guid.NewGuid(),
            Code = "VH-001",
            Brand = "Toyota",
            Model = "Vios",
            LicensePlate = "30A-123.45",
            SeatCount = 5,
            Color = "White",
            Transmission = "AT",
            FuelType = "Gasoline",
            DailyRate = 500000,
            Status = VehicleStatus.Available
        };

        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var controller = BuildController(context);
        var request = new BookingCreateRequest
        {
            VehicleId = vehicle.Id,
            PickupLocation = "Ha Noi",
            DropoffLocation = "Ha Noi",
            PickupDateTime = DateTime.UtcNow.Date.AddDays(1),
            ReturnDateTime = DateTime.UtcNow.Date.AddDays(3),
            PaymentMethod = PaymentMethod.Cash
        };

        var action = await controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(action.Result);
        var response = Assert.IsType<ApiResponse<BookingResponse>>(created.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1000000, response.Data!.TotalAmount);
        Assert.Equal(BookingStatus.Pending, response.Data.Status);
    }

    private static ApplicationDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static BookingsController BuildController(ApplicationDbContext context)
    {
        var bookingService = new BookingService(context);
        var controller = new BookingsController(bookingService, new ApiProblemDetailsFactory())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, "Customer")
                    ], "TestAuth"))
                }
            }
        };

        return controller;
    }
}
