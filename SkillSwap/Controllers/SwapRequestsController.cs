using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Controllers
{
    public class SwapRequestsController : Controller
    {
        private readonly SkillSwapDbContext _context;

    public SwapRequestsController(SkillSwapDbContext context)
        {
            _context = context;
        }

        // GET: SwapRequests
        public async Task<IActionResult> Index()
        {
            var requests = await _context.SwapRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Include(r => r.OfferedSkill)
                .Include(r => r.RequestedSkill)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: SwapRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.SwapRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Include(r => r.OfferedSkill)
                .Include(r => r.RequestedSkill)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: SwapRequests/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Users = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Skills = await _context.Skills
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View();
        }

        // POST: SwapRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int senderId,
            int receiverId,
            int offeredSkillId,
            int requestedSkillId,
            string message)
        {
            if (senderId == receiverId)
            {
                ModelState.AddModelError(
                    "",
                    "Sender and receiver must be different."
                );
            }

            if (offeredSkillId == requestedSkillId)
            {
                ModelState.AddModelError(
                    "",
                    "Offered skill and requested skill must be different."
                );
            }

            var senderExists = await _context.Users
                .AnyAsync(u => u.Id == senderId);

            var receiverExists = await _context.Users
                .AnyAsync(u => u.Id == receiverId);

            var offeredSkillExists = await _context.Skills
                .AnyAsync(s => s.Id == offeredSkillId);

            var requestedSkillExists = await _context.Skills
                .AnyAsync(s => s.Id == requestedSkillId);

            if (!senderExists)
            {
                ModelState.AddModelError("", "Selected sender does not exist.");
            }

            if (!receiverExists)
            {
                ModelState.AddModelError("", "Selected receiver does not exist.");
            }

            if (!offeredSkillExists)
            {
                ModelState.AddModelError("", "Selected offered skill does not exist.");
            }

            if (!requestedSkillExists)
            {
                ModelState.AddModelError("", "Selected requested skill does not exist.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _context.Users
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                ViewBag.Skills = await _context.Skills
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                return View();
            }

            var swapRequest = new SwapRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                OfferedSkillId = offeredSkillId,
                RequestedSkillId = requestedSkillId,
                Message = message ?? string.Empty,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.SwapRequests.Add(swapRequest);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Swap request sent successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: SwapRequests/Accept/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var request = await _context.SwapRequests
                .FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = "Accepted";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Swap request accepted successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: SwapRequests/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.SwapRequests
                .FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Swap request rejected successfully!";

            return RedirectToAction(nameof(Index));
        }
    }


}
