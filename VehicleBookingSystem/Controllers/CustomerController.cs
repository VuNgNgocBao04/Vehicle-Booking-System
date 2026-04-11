using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VehicleBookingSystem.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
