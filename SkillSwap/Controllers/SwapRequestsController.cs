using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Controllers
{
    [Authorize]
    public class SwapRequestsController : Controller
    {
        private readonly SkillSwapDbContext _context;

        public SwapRequestsController(SkillSwapDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // REQUEST LIST
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _context.SwapRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Include(r => r.OfferedSkill)
                .Include(r => r.RequestedSkill)
                .Where(r =>
                    r.SenderId == userId.Value ||
                    r.ReceiverId == userId.Value
                )
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // ==========================================
        // DETAILS
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.SwapRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Include(r => r.OfferedSkill)
                .Include(r => r.RequestedSkill)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    (
                        r.SenderId == userId.Value ||
                        r.ReceiverId == userId.Value
                    )
                );

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // ==========================================
        // CREATE - GET
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create(int receiverId)
        {
            var senderId = GetCurrentUserId();

            if (senderId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (senderId.Value == receiverId)
            {
                TempData["ErrorMessage"] =
                    "You cannot send a skill exchange request to yourself.";

                return RedirectToAction("Index", "Members");
            }

            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == receiverId);

            if (receiver == null)
            {
                return NotFound();
            }

            ViewBag.Receiver = receiver;

            ViewBag.Skills = await _context.Skills
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View();
        }

        // ==========================================
        // CREATE - POST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int receiverId,
            int offeredSkillId,
            int requestedSkillId,
            string message)
        {
            var senderId = GetCurrentUserId();

            if (senderId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (senderId.Value == receiverId)
            {
                ModelState.AddModelError(
                    "",
                    "You cannot send a request to yourself."
                );
            }

            if (offeredSkillId == requestedSkillId)
            {
                ModelState.AddModelError(
                    "",
                    "Offered skill and requested skill must be different."
                );
            }

            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == receiverId);

            if (receiver == null)
            {
                ModelState.AddModelError(
                    "",
                    "Receiver account does not exist."
                );
            }

            var offeredSkill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == offeredSkillId);

            if (offeredSkill == null)
            {
                ModelState.AddModelError(
                    "",
                    "Selected offered skill does not exist."
                );
            }

            var requestedSkill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == requestedSkillId);

            if (requestedSkill == null)
            {
                ModelState.AddModelError(
                    "",
                    "Selected requested skill does not exist."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Receiver = receiver;

                ViewBag.Skills = await _context.Skills
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                return View();
            }

            // ==========================================
            // CREATE REQUEST
            // ==========================================

            var swapRequest = new SwapRequest
            {
                SenderId = senderId.Value,
                ReceiverId = receiverId,

                OfferedSkillId = offeredSkillId,
                RequestedSkillId = requestedSkillId,

                Message = message?.Trim() ?? string.Empty,

                Status = "Pending",

                CreatedAt = DateTime.UtcNow
            };

            _context.SwapRequests.Add(swapRequest);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill exchange request sent successfully!";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // ACCEPT REQUEST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.SwapRequests
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.ReceiverId == userId.Value
                );

            if (request == null)
            {
                return NotFound();
            }

            request.Status = "Accepted";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill exchange request accepted!";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // REJECT REQUEST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.SwapRequests
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.ReceiverId == userId.Value
                );

            if (request == null)
            {
                return NotFound();
            }

            request.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill exchange request rejected.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // GET CURRENT LOGGED-IN USER ID
        // ==========================================
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }
    }
}