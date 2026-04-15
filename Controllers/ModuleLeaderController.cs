using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = "ModuleLeader")]
public class ModuleLeaderController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}