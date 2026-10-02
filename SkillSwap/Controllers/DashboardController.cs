using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using System.Security.Claims;

namespace SkillSwap.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SkillSwapDbContext _context;

        public DashboardController(SkillSwapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdString);

            var user = await _context.Users
                .Include(u => u.UserSkills)
                .ThenInclude(us => us.Skill)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserName = user.FullName;

            ViewBag.MySkills = user.UserSkills
                .Select(us => us.Skill.Name)
                .ToList();

            ViewBag.SentCount = await _context.SwapRequests
                .CountAsync(r => r.SenderId == userId);

            ViewBag.ReceivedCount = await _context.SwapRequests
                .CountAsync(r => r.ReceiverId == userId);

            ViewBag.PendingCount = await _context.SwapRequests
                .CountAsync(r =>
                    (r.SenderId == userId || r.ReceiverId == userId)
                    && r.Status == "Pending");

            ViewBag.AcceptedCount = await _context.SwapRequests
                .CountAsync(r =>
                    (r.SenderId == userId || r.ReceiverId == userId)
                    && r.Status == "Accepted");

            return View();
        }
    }
}