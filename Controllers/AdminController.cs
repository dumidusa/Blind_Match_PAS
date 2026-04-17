using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;  
        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalUsers    = await _context.Users.CountAsync(),
                TotalProjects = await _context.Projects.CountAsync()
            };

            return View(vm);
        }

        public IActionResult Environment()
        {
            var vm = new EnvironmentViewModel
            {
                DotNetVersion   = System.Environment.Version.ToString(),
                OS              = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                EnvironmentName = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                DatabaseConnected = _context.Database.CanConnect()
            };

            return View(vm);
        }

        public IActionResult Migrations()
        {
            var vm = new MigrationsViewModel
            {
                AppliedMigrations = _context.Database.GetAppliedMigrations().ToList(),
                PendingMigrations = _context.Database.GetPendingMigrations().ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> RbacAudit()
        {
            var vm = await _context.Users
                .Select(u => new RbacUserRow { Email = u.Email, Role = u.Role })
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> Users(string? search, string? role)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.Email.Contains(search));

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role);

            return View(await query.ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateUser() => View(new AdminCreateUserViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                FullName     = model.Email,          
                Email        = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),  
                Role         = model.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {model.Email} created successfully.";
            return RedirectToAction(nameof(Users));
        }
    }
}
