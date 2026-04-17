using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Models;
using BlindMatchPAS.Data;
using Microsoft.AspNetCore.Authorization;

namespace BlindMatchPAS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

  /* [AllowAnonymous]
   public IActionResult SeedUser()
    {
        var user = new User
        {
            FullName = "Test Student",
            Email = "test@student.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            Role = "Student"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Content("User created");
    }*/
   [AllowAnonymous]
    public IActionResult FixPassword()
 {
    var user = _context.Users.FirstOrDefault(x => x.Email == "test@student.com");

    if (user == null)
        return Content("User not found");

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234");

    _context.SaveChanges();

    return Content("Password fixed");
 }
}
