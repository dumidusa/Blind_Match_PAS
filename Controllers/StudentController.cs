using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BLIND_MATCH_PAS.Data;
using BLIND_MATCH_PAS.Models;

namespace BLIND_MATCH_PAS.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // VIEW all my proposals
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var projects = await _context.StudentProjects
                .Where(p => p.StudentId == userId)
                .ToListAsync();
            return View(projects);
        }

        // CREATE - form
        public IActionResult Create()
        {
            return View();
        }

        // CREATE - save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentProject project)
        {
            if (ModelState.IsValid)
            {
                project.StudentId = _userManager.GetUserId(User);
                project.Status = ProjectStatus.Pending;
                _context.StudentProjects.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // EDIT - form
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.StudentProjects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);

            if (project == null || project.Status == ProjectStatus.Matched)
                return Forbid();

            return View(project);
        }

        // EDIT - save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentProject updated)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.StudentProjects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);

            if (project == null || project.Status == ProjectStatus.Matched)
                return Forbid();

            project.Title = updated.Title;
            project.Abstract = updated.Abstract;
            project.TechnicalStack = updated.TechnicalStack;
            project.ResearchArea = updated.ResearchArea;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // WITHDRAW
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.StudentProjects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);

            if (project == null || project.Status == ProjectStatus.Matched)
                return Forbid();

            _context.StudentProjects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}