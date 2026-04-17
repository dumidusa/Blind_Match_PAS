using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = "ModuleLeader")]
public class ModuleLeaderController : Controller
{
    private readonly AppDbContext _context;

    public ModuleLeaderController(AppDbContext context)
    {
        _context = context;
    }

    // ─── Dashboard ────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalStudents = await _context.Users
            .CountAsync(u => u.Role == "Student");

        ViewBag.TotalSupervisors = await _context.Users
            .CountAsync(u => u.Role == "Supervisor");

        ViewBag.TotalMatched = await _context.Projects
            .CountAsync(p => p.Status == "Matched");

        ViewBag.TotalPending = await _context.Projects
            .CountAsync(p => p.Status == "Pending");

        ViewBag.TotalUnderReview = await _context.Projects
            .CountAsync(p => p.Status == "UnderReview");

        var recentMatches = await _context.Projects
            .Where(p => p.Status == "Matched")
            .Include(p => p.Student)
            .Include(p => p.MatchedSupervisor)
            .Include(p => p.ResearchArea)
            .OrderByDescending(p => p.SubmittedAt)
            .Take(10)
            .ToListAsync();

        return View(recentMatches);
    }

    // ─── Research Area Management ─────────────────────────────────────────────
    public async Task<IActionResult> ResearchAreas()
    {
        var areas = await _context.ResearchAreas
            .OrderBy(r => r.Name)
            .ToListAsync();
        return View(areas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResearchArea(ResearchAreaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid name. Use only letters, numbers, spaces, hyphens, slashes, or parentheses.";
            return RedirectToAction(nameof(ResearchAreas));
        }

        bool exists = await _context.ResearchAreas
            .AnyAsync(r => r.Name.ToLower() == model.Name.Trim().ToLower());

        if (exists)
        {
            TempData["Error"] = $"Research area \"{model.Name}\" already exists.";
            return RedirectToAction(nameof(ResearchAreas));
        }

        _context.ResearchAreas.Add(new ResearchArea { Name = model.Name.Trim() });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Research area \"{model.Name}\" added successfully.";
        return RedirectToAction(nameof(ResearchAreas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResearchArea(int id)
    {
        var area = await _context.ResearchAreas.FindAsync(id);
        if (area == null)
        {
            TempData["Error"] = "Research area not found.";
            return RedirectToAction(nameof(ResearchAreas));
        }

        bool inUse = await _context.Projects.AnyAsync(p => p.ResearchAreaId == id);
        if (inUse)
        {
            TempData["Error"] = $"Cannot delete \"{area.Name}\" — it is used by existing projects.";
            return RedirectToAction(nameof(ResearchAreas));
        }

        _context.ResearchAreas.Remove(area);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Research area \"{area.Name}\" deleted.";
        return RedirectToAction(nameof(ResearchAreas));
    }

    // ─── Allocation Oversight ─────────────────────────────────────────────────
    public async Task<IActionResult> AllAllocations()
    {
        var projects = await _context.Projects
            .Where(p => p.Status == "Matched")
            .Include(p => p.Student)
            .Include(p => p.MatchedSupervisor)
            .Include(p => p.ResearchArea)
            .OrderByDescending(p => p.SubmittedAt)
            .ToListAsync();

        ViewBag.Supervisors = await _context.Users
            .Where(u => u.Role == "Supervisor")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(projects);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnmatchProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            TempData["Error"] = "Project not found.";
            return RedirectToAction(nameof(AllAllocations));
        }

        // Clear the match
        project.MatchedSupervisorId = null;
        project.Status = "Pending";

        // Remove confirmed interest record so supervisor can rematch
        var confirmedInterest = await _context.SupervisorInterests
            .FirstOrDefaultAsync(si => si.ProjectId == id && si.Status == "Confirmed");

        if (confirmedInterest != null)
            _context.SupervisorInterests.Remove(confirmedInterest);

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Project \"{project.Title}\" has been unmatched and returned to Pending.";
        return RedirectToAction(nameof(AllAllocations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReassignProject(ReassignProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid reassignment data.";
            return RedirectToAction(nameof(AllAllocations));
        }

        var project = await _context.Projects.FindAsync(model.ProjectId);
        var newSupervisor = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == model.SupervisorId && u.Role == "Supervisor");

        if (project == null || newSupervisor == null)
        {
            TempData["Error"] = "Project or supervisor not found.";
            return RedirectToAction(nameof(AllAllocations));
        }

        // Remove old confirmed interest
        var oldInterest = await _context.SupervisorInterests
            .FirstOrDefaultAsync(si => si.ProjectId == model.ProjectId && si.Status == "Confirmed");
        if (oldInterest != null)
            _context.SupervisorInterests.Remove(oldInterest);

        // Create new confirmed interest for the new supervisor
        _context.SupervisorInterests.Add(new SupervisorInterest
        {
            SupervisorId = model.SupervisorId,
            ProjectId = model.ProjectId,
            Status = "Confirmed",
            ConfirmedAt = DateTime.UtcNow
        });

        project.MatchedSupervisorId = model.SupervisorId;
        project.Status = "Matched";

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Project \"{project.Title}\" reassigned to {newSupervisor.FullName}.";
        return RedirectToAction(nameof(AllAllocations));
    }

    // ─── User Management ──────────────────────────────────────────────────────
    public async Task<IActionResult> UserManagement(string? role, string? search)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        ViewBag.FilterRole = role ?? string.Empty;
        ViewBag.Search = search ?? string.Empty;

        var users = await query.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all fields correctly. Password must be at least 6 characters.";
            return RedirectToAction(nameof(UserManagement));
        }

        bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
        if (emailExists)
        {
            TempData["Error"] = $"A user with email \"{model.Email}\" already exists.";
            return RedirectToAction(nameof(UserManagement));
        }

        string[] validRoles = { "Student", "Supervisor", "ModuleLeader" };
        if (!validRoles.Contains(model.Role))
        {
            TempData["Error"] = "Invalid role selected.";
            return RedirectToAction(nameof(UserManagement));
        }

        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = model.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"User \"{model.FullName}\" ({model.Role}) created successfully.";
        return RedirectToAction(nameof(UserManagement));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(UserManagement));
        }

        bool hasProjects = await _context.Projects.AnyAsync(p => p.StudentId == id || p.MatchedSupervisorId == id);
        if (hasProjects)
        {
            TempData["Error"] = $"Cannot delete \"{user.FullName}\" — they have projects linked to their account.";
            return RedirectToAction(nameof(UserManagement));
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"User \"{user.FullName}\" deleted.";
        return RedirectToAction(nameof(UserManagement));
    }
}
