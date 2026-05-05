using FixConnect.BLL.Services;
using FixConnect.DAL.Data.Enums;
using FixConnect.PL.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using FixConnect.BLL.DTOs;
namespace FixConnect.PL.Controllers
{
    public class AccountController : Controller
    {
        // ✅ DI: AuthService injected here
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // ─────────────────────────────
        // GET: /Account/Login
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Login() => View();

        // ─────────────────────────────
        // POST: /Account/Login
        // ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _authService.Login(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }
            if (user.IsActive == false)
            {
                ModelState.AddModelError("", "Account DeActivated Call The Admins ");
                return View(model);
            }

            await SignInUser(user.UserId.ToString(), user.FullName, user.Email, user.RoleType.ToString());
            return RedirectByRole(user.RoleType);
        }

        // ─────────────────────────────
        // GET: /Account/Register
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        // ─────────────────────────────
        // POST: /Account/Register
        // ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Validate specialty if Worker
            if (model.Role == RoleType.Worker && string.IsNullOrWhiteSpace(model.Specialty))
            {
                ModelState.AddModelError("Specialty", "Specialty is required for workers.");
                return View(model);
            }

            var (success, message) = _authService.Register(
                model.FullName, model.Email, model.Password,
                model.Phone, model.Role, model.Specialty);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            // Auto-login after register
            var user = _authService.FindByEmail(model.Email)!;
            await SignInUser(user.UserId.ToString(), user.FullName, user.Email, user.RoleType.ToString());
            return RedirectByRole(user.RoleType);
        }

        // ─────────────────────────────
        // GET: /Account/GoogleLogin
        // ─────────────────────────────
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Account", null, Request.Scheme)

            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        // ─────────────────────────────
        // GET: /Account/GoogleCallback
        // ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var email = result.Principal.FindFirstValue(ClaimTypes.Email)!;
            var name = result.Principal.FindFirstValue(ClaimTypes.Name)!;

            // Case A — existing user by GoogleId
            var user = _authService.FindByGoogleId(googleId);

            // Case A — existing user by Email (link their GoogleId)
            if (user == null)
            {
                var existingByEmail = _authService.FindByEmail(email);
                if (existingByEmail != null)
                {
                    _authService.LinkGoogleId(existingByEmail, googleId);
                    user = existingByEmail;
                }
            }

            if (user != null)
            {
                // Case A: found — sign in directly
                await SignInUser(user.UserId.ToString(), user.FullName, user.Email, user.RoleType.ToString());
                return RedirectByRole(user.RoleType);
            }

            // Case B: new user — redirect to complete profile
            var vm = new CompleteProfileViewModel
            {
                FullName = name,
                Email = email,
                GoogleId = googleId
            };
            return View("CompleteProfile", vm);
        }

        // ─────────────────────────────
        // POST: /Account/CompleteProfile
        // ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> CompleteProfile(CompleteProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.Role == RoleType.Worker && string.IsNullOrWhiteSpace(model.Specialty))
            {
                ModelState.AddModelError("Specialty", "Specialty is required for workers.");
                return View(model);
            }

            var (success, message) = _authService.RegisterGoogleUser(
                model.FullName, model.Email, model.GoogleId,
                model.Phone, model.Role, model.Specialty);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            var user = _authService.FindByEmail(model.Email)!;
            await SignInUser(user.UserId.ToString(), user.FullName, user.Email, user.RoleType.ToString());
            return RedirectByRole(user.RoleType);
        }

        // ─────────────────────────────
        // POST: /Account/Logout
        // ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ─────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────

        // Creates cookie claims and signs the user in
        private async Task SignInUser(string userId, string fullName, string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name,            fullName),
                new Claim(ClaimTypes.Email,           email),
                new Claim(ClaimTypes.Role,            role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });
        }

        // Redirects based on role claim
        private IActionResult RedirectByRole(RoleType role) => role switch
        {
            RoleType.Admin => RedirectToAction("Dashboard", "Admin"),
            RoleType.Worker => RedirectToAction("Profile", "Worker"),
            RoleType.Customer => RedirectToAction("Index", "Home"),
            _ => RedirectToAction("Login")
        };
    }
}