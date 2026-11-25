using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ec.Edu.Sigh.Gr04.Controllers;

[Authorize]
public class HomeController : Controller
{
    [HttpGet("inicio")]
    public IActionResult Inicio() => View();
}
