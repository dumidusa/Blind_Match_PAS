using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
 
namespace BlindMatchPAS.Controllers;
 
[Authorize(Roles = "Supervisor")]
public class SupervisorController : Controller
{
    private readonly AppDbContext _context;
 
    public SupervisorController(AppDbContext context)
    {
        _context = context;
    }
 
    // ─── Helper ───────────────────────────────────────────────────────────────
    private int GetSupervisorId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    // ─── Dashboard ────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        int supervisorId = GetSupervisorId();
 
        ViewBag.MatchedCount = await _context.SupervisorInterests
            .CountAsync(si => si.SupervisorId == supervisorId && si.Status == "Confirmed");
 
        ViewBag.InterestedCount = await _context.SupervisorInterests
            .CountAsync(si => si.SupervisorId == supervisorId && si.Status == "Interested");
 
        var confirmedMatches = await _context.SupervisorInterests
            .Where(si => si.SupervisorId == supervisorId && si.Status == "Confirmed")
            .Include(si => si.Project).ThenInclude(p => p!.ResearchArea)
            .Include(si => si.Project).ThenInclude(p => p!.Student)
            .OrderByDescending(si => si.ConfirmedAt)
            .ToListAsync();
 
        ViewBag.ConfirmedMatches = confirmedMatches;
 
        var expertiseIds = await _context.SupervisorExpertises
            .Where(se => se.SupervisorId == supervisorId)
            .Select(se => se.ResearchAreaId).ToListAsync();
 
        ViewBag.ExpertiseIds = expertiseIds;
        ViewBag.ExpertiseAreas = await _context.SupervisorExpertises
            .Where(se => se.SupervisorId == supervisorId)
            .Include(se => se.ResearchArea)
            .Select(se => se.ResearchArea!.Name).ToListAsync();
 
        return View();
    }
 
    // ─── Expertise Management ─────────────────────────────────────────────────
    public async Task<IActionResult> ManageExpertise()
    {
        int supervisorId = GetSupervisorId();
        ViewBag.AllAreas = await _context.ResearchAreas.OrderBy(r => r.Name).ToListAsync();
        ViewBag.SelectedIds = await _context.SupervisorExpertises
            .Where(se => se.SupervisorId == supervisorId)
            .Select(se => se.ResearchAreaId).ToListAsync();
        return View();
    }
 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveExpertise(List<int> selectedAreaIds)
    {
        int supervisorId = GetSupervisorId();
        var existing = await _context.SupervisorExpertises
            .Where(se => se.SupervisorId == supervisorId).ToListAsync();
        _context.SupervisorExpertises.RemoveRange(existing);
 
        if (selectedAreaIds != null && selectedAreaIds.Any())
        {
            await _context.SupervisorExpertises.AddRangeAsync(
                selectedAreaIds.Select(id => new SupervisorExpertise
                {
                    SupervisorId = supervisorId,
                    ResearchAreaId = id
                }));
        }
 
        await _context.SaveChangesAsync();
        TempData["Success"] = "Your expertise areas have been updated.";
        return RedirectToAction(nameof(ManageExpertise));
    }
 
    // ─── Blind Review Dashboard ───────────────────────────────────────────────
    public async Task<IActionResult> BrowseProjects(int? areaId, string? search)
    {
        int supervisorId = GetSupervisorId();
 
        var expertiseIds = await _context.SupervisorExpertises
            .Where(se => se.SupervisorId == supervisorId)
            .Select(se => se.ResearchAreaId).ToListAsync();
 
        var interestedProjectIds = await _context.SupervisorInterests
            .Where(si => si.SupervisorId == supervisorId)
            .Select(si => si.ProjectId).ToListAsync();
 
        var query = _context.Projects
            .Include(p => p.ResearchArea)
            .Where(p => (p.Status == "Pending" || p.Status == "UnderReview")
                        && p.MatchedSupervisorId == null);
 
        if (areaId.HasValue && areaId.Value > 0)
            query = query.Where(p => p.ResearchAreaId == areaId.Value);
        else if (expertiseIds.Any())
            query = query.Where(p => expertiseIds.Contains(p.ResearchAreaId));
 
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Abstract.ToLower().Contains(term) ||
                p.TechStack.ToLower().Contains(term));
        }
 
        var projects = await query.OrderByDescending(p => p.SubmittedAt).ToListAsync();
 
        var interestCounts = await _context.SupervisorInterests
            .Where(si => projects.Select(p => p.Id).Contains(si.ProjectId))
            .GroupBy(si => si.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Count);
 
        ViewBag.Projects = projects;
        ViewBag.InterestedProjectIds = interestedProjectIds;
        ViewBag.InterestCounts = interestCounts;
        ViewBag.AllAreas = await _context.ResearchAreas.OrderBy(r => r.Name).ToListAsync();
        ViewBag.ExpertiseIds = expertiseIds;
        ViewBag.SelectedAreaId = areaId ?? 0;
        ViewBag.Search = search ?? string.Empty;
 
        return View();
    }
 
    // ─── Express Interest ─────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExpressInterest(int projectId)
    {
        int supervisorId = GetSupervisorId();
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null || project.MatchedSupervisorId != null)
        {
            TempData["Error"] = "This project is no longer available.";
            return RedirectToAction(nameof(BrowseProjects));
        }
 
        bool already = await _context.SupervisorInterests
            .AnyAsync(si => si.SupervisorId == supervisorId && si.ProjectId == projectId);
        if (!already)
        {
            _context.SupervisorInterests.Add(new SupervisorInterest
            {
                SupervisorId = supervisorId,
                ProjectId = projectId,
                Status = "Interested"
            });
            if (project.Status == "Pending") project.Status = "UnderReview";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Interest noted! Confirm the match from your Interested Projects list.";
        }
        else
        {
            TempData["Info"] = "You have already expressed interest in this project.";
        }
 
        return RedirectToAction(nameof(BrowseProjects));
    }
 
    // ─── Withdraw Interest ────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WithdrawInterest(int projectId)
    {
        int supervisorId = GetSupervisorId();
        var interest = await _context.SupervisorInterests
            .FirstOrDefaultAsync(si => si.SupervisorId == supervisorId
                && si.ProjectId == projectId && si.Status == "Interested");
 
        if (interest != null)
        {
            _context.SupervisorInterests.Remove(interest);
            bool othersInterested = await _context.SupervisorInterests
                .AnyAsync(si => si.ProjectId == projectId && si.SupervisorId != supervisorId);
            if (!othersInterested)
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project != null && project.Status == "UnderReview")
                    project.Status = "Pending";
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Interest withdrawn.";
        }
 
        return RedirectToAction(nameof(InterestedProjects));
    }
 
    // ─── Interested Projects List ─────────────────────────────────────────────
    public async Task<IActionResult> InterestedProjects()
    {
        int supervisorId = GetSupervisorId();
        ViewBag.Interests = await _context.SupervisorInterests
            .Where(si => si.SupervisorId == supervisorId && si.Status == "Interested")
            .Include(si => si.Project).ThenInclude(p => p!.ResearchArea)
            .OrderByDescending(si => si.CreatedAt).ToListAsync();
        return View();
    }
 
    // ─── Confirm Match (Identity Reveal) ──────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmMatch(int projectId)
    {
        int supervisorId = GetSupervisorId();
        var interest = await _context.SupervisorInterests
            .Include(si => si.Project)
            .FirstOrDefaultAsync(si =>
                si.SupervisorId == supervisorId &&
                si.ProjectId == projectId &&
                si.Status == "Interested");
 
        if (interest?.Project == null)
        {
            TempData["Error"] = "Interest record not found.";
            return RedirectToAction(nameof(InterestedProjects));
        }
        if (interest.Project.MatchedSupervisorId != null)
        {
            TempData["Error"] = "This project has already been matched.";
            return RedirectToAction(nameof(InterestedProjects));
        }
 
        interest.Status = "Confirmed";
        interest.ConfirmedAt = DateTime.UtcNow;
        interest.Project.Status = "Matched";
        interest.Project.MatchedSupervisorId = supervisorId;
 
        // Remove competing interests
        var others = await _context.SupervisorInterests
            .Where(si => si.ProjectId == projectId && si.SupervisorId != supervisorId && si.Status == "Interested")
            .ToListAsync();
        _context.SupervisorInterests.RemoveRange(others);
 
        await _context.SaveChangesAsync();
        TempData["Success"] = "Match confirmed! The student's identity has been revealed below.";
        return RedirectToAction(nameof(MatchedProjects));
    }
 
    // ─── Matched Projects (Identity Revealed) ─────────────────────────────────
    public async Task<IActionResult> MatchedProjects()
    {
        int supervisorId = GetSupervisorId();
        ViewBag.Matches = await _context.SupervisorInterests
            .Where(si => si.SupervisorId == supervisorId && si.Status == "Confirmed")
            .Include(si => si.Project).ThenInclude(p => p!.ResearchArea)
            .Include(si => si.Project).ThenInclude(p => p!.Student)
            .OrderByDescending(si => si.ConfirmedAt).ToListAsync();
        return View();
    }
}