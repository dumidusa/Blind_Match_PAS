using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    private int GetStudentId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var studentId = GetStudentId();

        var project = await _context.Projects
            .Include(p => p.ResearchArea)
            .Include(p => p.MatchedSupervisor)
            .FirstOrDefaultAsync(p => p.StudentId == studentId
                                   && p.Status != "Withdrawn");

        return View(project); // project can be null (no submission yet)
    }

    public async Task<IActionResult> Submit()
    {
        var studentId = GetStudentId();

        // Block if student already has an active proposal
        var existing = await _context.Projects
            .AnyAsync(p => p.StudentId == studentId && p.Status != "Withdrawn");

        if (existing)
        {
            TempData["Error"] = "You already have an active proposal. Edit or withdraw it first.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new ProjectViewModel
        {
            ResearchAreas = await GetResearchAreasList()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ProjectViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.ResearchAreas = await GetResearchAreasList();
            return View(vm);
        }

        var studentId = GetStudentId();

        var project = new Project
        {
            Title          = vm.Title,
            Abstract       = vm.Abstract,
            TechStack      = vm.TechStack,
            ResearchAreaId = vm.ResearchAreaId,
            StudentId      = studentId,
            Status         = "Pending",
            SubmittedAt    = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Proposal submitted successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var studentId = GetStudentId();
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == studentId);

        if (project == null) return NotFound();

        if (project.Status is "Matched" or "UnderReview")
        {
            TempData["Error"] = "You cannot edit a proposal that is Under Review or Matched.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new ProjectViewModel
        {
            Id             = project.Id,
            Title          = project.Title,
            Abstract       = project.Abstract,
            TechStack      = project.TechStack,
            ResearchAreaId = project.ResearchAreaId,
            ResearchAreas  = await GetResearchAreasList()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.ResearchAreas = await GetResearchAreasList();
            return View(vm);
        }

        var studentId = GetStudentId();
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == vm.Id && p.StudentId == studentId);

        if (project == null) return NotFound();

        if (project.Status is "Matched" or "UnderReview")
        {
            TempData["Error"] = "Cannot edit this proposal at its current status.";
            return RedirectToAction(nameof(Index));
        }

        project.Title          = vm.Title;
        project.Abstract       = vm.Abstract;
        project.TechStack      = vm.TechStack;
        project.ResearchAreaId = vm.ResearchAreaId;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Proposal updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        var studentId = GetStudentId();
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == studentId);

        if (project == null) return NotFound();

        if (project.Status == "Matched")
        {
            TempData["Error"] = "You cannot withdraw a matched proposal.";
            return RedirectToAction(nameof(Index));
        }

        project.Status = "Withdrawn";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Proposal withdrawn.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetResearchAreasList()
        => await _context.ResearchAreas
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
            .ToListAsync();
}