// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 📁 FILE: FixConnect.PL/Controllers/Api/WorkerApiController.cs
//     انشئ فولدر Api جوه Controllers وحط الفايل فيه
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using FixConnect.BLL.Services;
using FixConnect.DAL.Data.Enums;
using FixConnect.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixConnect.PL.Controllers.Api
{
    [ApiController]
    [Route("api/worker")]
    [Authorize(Roles = "Worker")]
    public class WorkerApiController : ControllerBase
    {
        // ✅ DI: نفس الـ Services بتاعت الـ MVC Controller
        private readonly WorkerService _workerService;
        private readonly PortfolioService _portfolioService;
        private readonly IWebHostEnvironment _env;

        public WorkerApiController(
            WorkerService workerService,
            PortfolioService portfolioService,
            IWebHostEnvironment env)
        {
            _workerService = workerService;
            _portfolioService = portfolioService;
            _env = env;
        }

        private int GetCurrentUserId()
            => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // GET: api/worker/profile
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var worker = _workerService.GetWorkerProfile(GetCurrentUserId());
            if (worker == null) return NotFound(new { message = "Worker not found." });

            var vm = new WorkerProfileViewModel
            {
                UserId = worker.UserId,
                FullName = worker.User.FullName,
                Email = worker.User.Email,
                Phone = worker.User.Phone,
                Bio = worker.Bio,
                Specialty = worker.Specialty,
                PhotoUrl = worker.PhotoUrl,
                IsVerified = worker.IsVerified,
                AvailabilityStatus = worker.AvailabilityStatus.ToString(),
                AvgRating = worker.AvgRating,
                HasPendingVerification = worker.Verification?.Status == "Pending",

                WorkingRegions = worker.WorksAt
                    .Select(wa => wa.Region.RegionName).ToList(),

                PortfolioItems = worker.PortfolioItems
                    .Select(p => new PortfolioItemViewModel
                    {
                        ItemId = p.ItemId,
                        Title = p.Title ?? "",
                        Description = p.Description,
                        ImageUrl = p.ImageUrl
                    }).ToList(),

                Reviews = worker.Reviews
                    .Select(r => new ReviewItemViewModel
                    {
                        CustomerName = r.Customer.User.FullName,
                        RatingValue = r.RatingValue,
                        Comment = r.Comment
                    }).ToList()
            };

            return Ok(vm);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // GET: api/worker/edit-profile
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpGet("edit-profile")]
        public IActionResult GetEditProfile()
        {
            var worker = _workerService.GetWorkerProfile(GetCurrentUserId());
            if (worker == null) return NotFound(new { message = "Worker not found." });

            var vm = new EditWorkerProfileViewModel
            {
                FullName = worker.User.FullName,
                Phone = worker.User.Phone,
                Bio = worker.Bio,
                Specialty = worker.Specialty,
                AvailabilityStatus = worker.AvailabilityStatus,
                CurrentPhotoUrl = worker.PhotoUrl,
                HasPendingVerification = worker.Verification?.Status == "Pending",

                SelectedRegionIds = worker.WorksAt
                    .Select(wa => wa.RegionId).ToList(),

                AllRegions = _workerService.GetAllRegions()
                    .Select(r => new RegionOption
                    {
                        RegionId = r.RegionId,
                        RegionName = r.RegionName
                    }).ToList()
            };

            return Ok(vm);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST: api/worker/edit-profile
        // Content-Type: multipart/form-data
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpPost("edit-profile")]
        public async Task<IActionResult> EditProfile([FromForm] EditWorkerProfileViewModel model)
        {
            int userId = GetCurrentUserId();

            // Handle Photo Upload
            string? photoUrl = null;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                photoUrl = await SaveFile(model.PhotoFile, "ProfilePictures");

            _workerService.UpdateProfile(
                userId,
                model.FullName,
                model.Phone,
                model.Bio,
                model.Specialty,
                model.AvailabilityStatus,
                model.SelectedRegionIds,
                photoUrl);

            // Handle Verification Upload
            if (model.IdFrontImage != null && model.IdBackImage != null)
            {
                var frontPath = await SaveFile(model.IdFrontImage, "VerificationDocs");
                var backPath = await SaveFile(model.IdBackImage, "VerificationDocs");
                _workerService.SubmitVerification(userId, frontPath, backPath);
            }

            return Ok(new { message = "Profile updated successfully." });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST: api/worker/toggle-availability
        // Body: { "status": 0 }
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpPost("toggle-availability")]
        public IActionResult ToggleAvailability([FromBody] ToggleAvailabilityRequest request)
        {
            _workerService.UpdateAvailability(GetCurrentUserId(), (AvailabilityStatus)request.Status);
            return Ok(new { message = "Availability updated." });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST: api/worker/portfolio
        // Content-Type: multipart/form-data
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpPost("portfolio")]
        public async Task<IActionResult> AddPortfolioItem([FromForm] PortfolioItemViewModel model)
        {
            string? imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
                imageUrl = await SaveFile(model.ImageFile, "PortfolioPictures");

            var (success, message) = _portfolioService.AddItem(
                GetCurrentUserId(), model.Title, model.Description, imageUrl);

            if (!success) return BadRequest(new { message });

            return Ok(new { message });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // DELETE: api/worker/portfolio/{itemId}
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpDelete("portfolio/{itemId}")]
        public IActionResult DeletePortfolioItem(int itemId)
        {
            var (success, message) = _portfolioService.DeleteItem(itemId, GetCurrentUserId());
            if (!success) return NotFound(new { message });

            return Ok(new { message });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // PUT: api/worker/portfolio/{itemId}
        // Content-Type: multipart/form-data
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpPut("portfolio/{itemId}")]
        public async Task<IActionResult> UpdatePortfolioItem(int itemId, [FromForm] PortfolioItemViewModel model)
        {
            string? imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
                imageUrl = await SaveFile(model.ImageFile, "PortfolioPictures");

            var (success, message) = _portfolioService.UpdateItem(
                itemId, GetCurrentUserId(), model.Title, model.Description, imageUrl);

            if (!success) return NotFound(new { message });

            return Ok(new { message });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // GET: api/worker/regions/search?q=cairo
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpGet("regions/search")]
        public IActionResult SearchRegions([FromQuery] string q)
        {
            var results = _workerService.SearchRegions(GetCurrentUserId(), q);
            return Ok(results);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // POST: api/worker/regions
        // Body: { "regionId": 1 }
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpPost("regions")]
        public IActionResult AddRegion([FromBody] AddRegionRequest request)
        {
            _workerService.AddRegion(GetCurrentUserId(), request.RegionId);
            return Ok(new { message = "Region added." });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // DELETE: api/worker/regions/{regionName}
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        [HttpDelete("regions/{regionName}")]
        public IActionResult RemoveRegion(string regionName)
        {
            _workerService.RemoveRegion(GetCurrentUserId(), regionName);
            return Ok(new { message = "Region removed." });
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // PRIVATE: Save File Helper
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // Request Models
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public class AddRegionRequest
        {
            public int RegionId { get; set; }
        }

        public class ToggleAvailabilityRequest
        {
            public int Status { get; set; }
        }
    }
}