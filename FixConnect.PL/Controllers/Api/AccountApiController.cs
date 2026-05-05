// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 📁 FILE: FixConnect.PL/Controllers/Api/AccountApiController.cs
//     انشئ فولدر Api جوه Controllers وحط الفايل فيه
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using FixConnect.BLL.Services;
using FixConnect.DAL.Data.Enums;
using FixConnect.PL.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixConnect.PL.Controllers.Api
{
    [ApiController]
    [Route("api/account")]
    public class AccountApiController : ControllerBase
    {
        // ✅ DI: نفس الـ AuthService بتاعت الـ MVC Controller
        private readonly AuthService _authService;

        public AccountApiController(AuthService authService)
        {
            _authService = authService;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST: api/account/login
        // Body: { "email": "", "password": "" }
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _authService.Login(model.Email, model.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            if (user.IsActive == false)
                return Unauthorized(new { message = "Account deactivated. Contact admins." });

            await SignInUser(
                user.UserId.ToString(),
                user.FullName,
                user.Email,
                user.RoleType.ToString());

            return Ok(new
            {
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                role = user.RoleType.ToString()
            });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST: api/account/register
        // Body: { "fullName": "", "email": "", ... }
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Role == RoleType.Worker && string.IsNullOrWhiteSpace(model.Specialty))
                return BadRequest(new { message = "Specialty is required for workers." });

            var (success, message) = _authService.Register(
                model.FullName, model.Email, model.Password,
                model.Phone, model.Role, model.Specialty);

            if (!success)
                return BadRequest(new { message });

            var user = _authService.FindByEmail(model.Email)!;
            await SignInUser(
                user.UserId.ToString(),
                user.FullName,
                user.Email,
                user.RoleType.ToString());

            return Ok(new
            {
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                role = user.RoleType.ToString()
            });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST: api/account/logout
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully." });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // GET: api/account/me
        // بيرجع بيانات الـ User الحالي من الـ Cookie
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                fullName = User.FindFirstValue(ClaimTypes.Name),
                email = User.FindFirstValue(ClaimTypes.Email),
                role = User.FindFirstValue(ClaimTypes.Role)
            });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // PRIVATE HELPER
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        private async Task SignInUser(string userId, string fullName, string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name,           fullName),
                new Claim(ClaimTypes.Email,          email),
                new Claim(ClaimTypes.Role,           role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });
        }
    }
}