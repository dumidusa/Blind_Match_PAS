using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using BlindMatchPAS.Data;
using BlindMatchPAS.ViewModels;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
namespace BlindMatchPAS.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    //loging page ...
    [AllowAnonymous]
    public IActionResult Login()
    {
    return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
     // 1. Find user by email
     var user = await _context.Users
        .FirstOrDefaultAsync(x => x.Email == model.Email);

     if (user == null)
        {
        ViewBag.Error = "Email not found.";
        return View(model);
        }

         // 2. Check password against stored hash
      bool validPassword = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

     if (!validPassword)
      {
        ViewBag.Error = "Incorrect password.";
        return View(model);
     }

     // 3. Create claims (stores who this user is in the cookie)
     var claims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
     };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // 4. Sign in — creates the session cookie
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // 5. Redirect based on role
     return user.Role switch
     {
        "Student"    => RedirectToAction("Index", "Student"),
        "Supervisor" => RedirectToAction("Index", "Supervisor"),
        "Admin"      => RedirectToAction("Index", "Admin"),
        _            => RedirectToAction("Login")
     };
}

  

    // log out action method 
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }
    public IActionResult AccessDenied()
    {
    return View();
    }
}

