using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehicleBookingSystem.Areas.Admin.Controllers;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Services;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Tests.Controller;

public class AdminVehicleControllerTests
{
    [Fact]
    public async Task Create_ReturnsView_WhenModelInvalid()
    {
        await using var context = BuildContext(nameof(Create_ReturnsView_WhenModelInvalid));
        var fileStorageMock = new Mock<IFileStorageService>();
        var controller = new VehicleController(context, fileStorageMock.Object, new HtmlSanitizerService());
        controller.ModelState.AddModelError("Code", "invalid");

        var result = await controller.Create(new VehicleFormViewModel());

        Assert.IsType<ViewResult>(result);
    }

    private static ApplicationDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
