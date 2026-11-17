using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace sistema.Controllers;
[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();
    [AllowAnonymous]
    public IActionResult Error() => View(new sistema.Models.ErrorViewModel
    {
        RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}