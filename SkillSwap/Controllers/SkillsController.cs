using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Controllers
{
    [Authorize]
    public class SkillsController : Controller
    {
        private readonly SkillSwapDbContext _context;

        public SkillsController(SkillSwapDbContext context)
        {
            _context = context;
        }

        // Show all skills
        public async Task<IActionResult> Index()
        {
            var skills = await _context.Skills
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(skills);
        }

        // Show skill details
        public async Task<IActionResult> Details(int id)
        {
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == id);

            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // Show create form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Save new skill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Skill skill)
        {
            if (!ModelState.IsValid)
            {
                return View(skill);
            }

            skill.Name = skill.Name.Trim();
            skill.Description = skill.Description?.Trim() ?? string.Empty;
            skill.CreatedAt = DateTime.UtcNow;

            _context.Skills.Add(skill);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Skill added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Show edit form
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var skill = await _context.Skills.FindAsync(id);

            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // Save edited skill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Skill skill)
        {
            if (id != skill.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(skill);
            }

            var existingSkill = await _context.Skills.FindAsync(id);

            if (existingSkill == null)
            {
                return NotFound();
            }

            existingSkill.Name = skill.Name.Trim();

            existingSkill.Description =
                skill.Description?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Show delete confirmation
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == id);

            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // Delete skill
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skill = await _context.Skills.FindAsync(id);

            if (skill == null)
            {
                return NotFound();
            }

            _context.Skills.Remove(skill);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}