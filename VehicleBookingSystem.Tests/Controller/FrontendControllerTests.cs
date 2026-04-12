using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehicleBookingSystem.Areas.Admin.Controllers;
using VehicleBookingSystem.Controllers;
using VehicleBookingSystem.Data;
using VehicleBookingSystem.Models;
using VehicleBookingSystem.Services;
using VehicleBookingSystem.ViewModels;

namespace VehicleBookingSystem.Tests.Controller;

public class FrontendControllerTests
{
    [Fact]
    public async Task Customer_SearchSuggestions_ReturnsEmpty_WhenTermTooShort()
    {
        await using var context = BuildContext(nameof(Customer_SearchSuggestions_ReturnsEmpty_WhenTermTooShort));
        var controller = new CustomerController(new CustomerService(context, BuildEnvironment()));

        var result = await controller.SearchSuggestions("a");

        var json = Assert.IsType<JsonResult>(result);
        var suggestions = Assert.IsAssignableFrom<IEnumerable<string>>(json.Value);
        Assert.Empty(suggestions);
    }

    [Fact]
    public async Task Customer_Details_ReturnsNotFound_WhenVehicleMissing()
    {
        await using var context = BuildContext(nameof(Customer_Details_ReturnsNotFound_WhenVehicleMissing));
        var controller = new CustomerController(new CustomerService(context, BuildEnvironment()));

        var result = await controller.Details(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Admin_Dashboard_Index_ReturnsDashboardModel()
    {
        using var context = BuildContext(nameof(Admin_Dashboard_Index_ReturnsDashboardModel));
        var controller = new DashboardController(context);

        var result = controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<AdminDashboardViewModel>(view.Model);
    }

    private static ApplicationDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IWebHostEnvironment BuildEnvironment()
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.WebRootPath).Returns(Path.GetTempPath());
        return environment.Object;
    }
}
