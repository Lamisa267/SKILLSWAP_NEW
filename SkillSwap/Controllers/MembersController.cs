using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly SkillSwapDbContext _context;

        public MembersController(SkillSwapDbContext context)
        {
            _context = context;
        }

        // =========================
        // MEMBERS LIST
        // =========================
        public async Task<IActionResult> Index()
        {
            var members = await _context.Members
                .OrderByDescending(m => m.JoinedDate)
                .ToListAsync();

            return View(members);
        }

        // =========================
        // MEMBER DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
                return NotFound();

            return View(member);
        }

        // =========================
        // CREATE - ADMIN ONLY
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member)
        {
            if (!ModelState.IsValid)
                return View(member);

            try
            {
                member.JoinedDate = DateTime.UtcNow;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Email.ToLower() == member.Email.ToLower());

                if (user != null)
                    member.UserId = user.Id;

                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Member added successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(
                    "CREATE DATABASE ERROR:\n\n" +
                    ex.Message +
                    "\n\nINNER ERROR:\n\n" +
                    ex.InnerException?.Message);
            }
        }

        // =========================
        // EDIT - ADMIN ONLY
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.Members.FindAsync(id);

            if (member == null)
                return NotFound();

            return View(member);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {
            if (id != member.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(member);

            try
            {
                var existingMember = await _context.Members.FindAsync(id);

                if (existingMember == null)
                    return NotFound();

                existingMember.Name = member.Name;
                existingMember.Email = member.Email;
                existingMember.PhoneNumber = member.PhoneNumber;
                existingMember.Bio = member.Bio;
                existingMember.SkillToOffer = member.SkillToOffer;
                existingMember.SkillToLearn = member.SkillToLearn;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Email.ToLower() == member.Email.ToLower());

                if (user != null)
                    existingMember.UserId = user.Id;
                else
                    existingMember.UserId = null;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Member updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(
                    "EDIT DATABASE ERROR:\n\n" +
                    ex.Message +
                    "\n\nINNER ERROR:\n\n" +
                    ex.InnerException?.Message);
            }
        }

        // =========================
        // DELETE - ADMIN ONLY
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
                return NotFound();

            return View(member);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var member = await _context.Members.FindAsync(id);

                if (member == null)
                    return NotFound();

                _context.Members.Remove(member);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Member deleted successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content(
                    "DELETE DATABASE ERROR:\n\n" +
                    ex.Message +
                    "\n\nINNER ERROR:\n\n" +
                    ex.InnerException?.Message);
            }
        }
    }
}