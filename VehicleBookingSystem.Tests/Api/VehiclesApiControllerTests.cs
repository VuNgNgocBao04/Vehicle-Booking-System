using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehicleBookingSystem.Contracts.Common;
using VehicleBookingSystem.Contracts.Vehicles;
using VehicleBookingSystem.Controllers.Api;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Services;

namespace VehicleBookingSystem.Tests.Api;

public class VehiclesApiControllerTests
{
    [Fact]
    public async Task GetVehicles_ReturnsPagedData()
    {
        await using var context = BuildContext(nameof(GetVehicles_ReturnsPagedData));
        context.Vehicles.AddRange(
            BuildVehicle("VH-001", "30A-123.45", 400000),
            BuildVehicle("VH-002", "30A-123.46", 700000));
        await context.SaveChangesAsync();

        var controller = new VehiclesController(context, new HtmlSanitizerService(), BuildEnvironment());

        var action = await controller.GetVehicles(page: 1, pageSize: 1, category: null, minPrice: null, maxPrice: null);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var payload = Assert.IsType<PagedResponse<VehicleResponse>>(ok.Value);
        Assert.Equal(2, payload.TotalItems);
        Assert.Equal(1, payload.Page);
        Assert.Equal(1, payload.PageSize);
    }

    [Fact]
    public async Task CheckAvailability_ReturnsBadRequest_WhenEndDateBeforeStartDate()
    {
        await using var context = BuildContext(nameof(CheckAvailability_ReturnsBadRequest_WhenEndDateBeforeStartDate));
        var controller = new VehiclesController(context, new HtmlSanitizerService(), BuildEnvironment());

        var action = await controller.CheckAvailability(Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(2), DateTime.UtcNow.Date.AddDays(1));

        Assert.IsType<BadRequestObjectResult>(action.Result);
    }

    private static ApplicationDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Vehicle BuildVehicle(string code, string plate, decimal dailyRate)
    {
        return new Vehicle
        {
            Id = Guid.NewGuid(),
            VehicleCategoryId = Guid.NewGuid(),
            Code = code,
            Brand = "Toyota",
            Model = "Vios",
            LicensePlate = plate,
            SeatCount = 5,
            Color = "White",
            Transmission = "AT",
            FuelType = "Gasoline",
            DailyRate = dailyRate,
            Status = VehicleStatus.Available
        };
    }

    private static IWebHostEnvironment BuildEnvironment()
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.WebRootPath).Returns(Path.GetTempPath());
        return environment.Object;
    }
}
