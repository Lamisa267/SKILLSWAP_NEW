using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Controllers
{
    public class AccountController : Controller
    {
        private readonly SkillSwapDbContext _context;


    public AccountController(SkillSwapDbContext context)
        {
            _context = context;
        }

        // =========================
        // REGISTER - GET
        // =========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // =========================
        // REGISTER - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim().ToLower();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists."
                );

                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = email,

                // Password is stored as a BCrypt hash
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    model.Password
                ),

                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Account created successfully! Please login.";

            return RedirectToAction(nameof(Login));
        }

        // =========================
        // LOGIN - GET
        // =========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =========================
        // LOGIN - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Email and password are required.";

                return View();
            }

            var normalizedEmail = email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email.ToLower() == normalizedEmail
                );

            if (user == null)
            {
                ViewBag.Error =
                    "Invalid email or password.";

                return View();
            }

            bool passwordCorrect;

            try
            {
                passwordCorrect = BCrypt.Net.BCrypt.Verify(
                    password,
                    user.PasswordHash
                );
            }
            catch (BCrypt.Net.SaltParseException)
            {
                ViewBag.Error =
                    "This account uses an old password format. Please create a new account.";

                return View();
            }

            if (!passwordCorrect)
            {
                ViewBag.Error =
                    "Invalid email or password.";

                return View();
            }

            // =========================
            // CUSTOM SESSION
            // =========================

            HttpContext.Session.SetInt32(
                "UserId",
                user.Id
            );

            HttpContext.Session.SetString(
                "UserName",
                user.FullName
            );

            HttpContext.Session.SetString(
                "UserRole",
                user.Role
            );

            // =========================
            // COOKIE AUTHENTICATION
            // =========================

            var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            new Claim(
                ClaimTypes.Name,
                user.FullName
            ),

            new Claim(
                ClaimTypes.Email,
                user.Email
            ),

            new Claim(
                ClaimTypes.Role,
                user.Role
            )
        };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        // =========================
        // LOGOUT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        // =========================
        // ACCESS DENIED
        // =========================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }

}
