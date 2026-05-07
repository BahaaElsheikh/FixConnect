using FixConnect.BLL.Services;
using FixConnect.DAL.Data.Enums;
using FixConnect.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixConnect.PL.Controllers
{
    [Authorize(Roles = "Worker")]
    public class WorkerController : Controller
    {
        // ✅ DI: Services injected
        private readonly WorkerService _workerService;
        private readonly PortfolioService _portfolioService;
        private readonly IWebHostEnvironment _env;

        public WorkerController(WorkerService workerService,
            PortfolioService portfolioService,
            IWebHostEnvironment env)
        {
            _workerService = workerService;
            _portfolioService = portfolioService;
            _env = env;
        }

        private int GetCurrentUserId()
            => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ─────────────────────────────
        // GET: /Worker/Profile
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Profile()
        {
            var worker = _workerService.GetWorkerProfile(GetCurrentUserId());
            if (worker == null) return NotFound();

            var vm = new WorkerProfileViewModel
            {
                UserId = worker.UserId,
                FullName = worker.User.FullName,
                Email = worker.User.Email,
                Phone = worker.User.Phone,
                Bio = worker.Bio,
                SpecialtyName = worker.Specialty?.SpecialtyName,

                PhotoUrl = worker.PhotoUrl,
                IsVerified = worker.IsVerified,
                AvailabilityStatus = worker.AvailabilityStatus.ToString(),
                AvgRating = worker.AvgRating,
                WorkingRegions = worker.WorksAt.Select(wa => wa.Region.RegionName).ToList(),
                HasPendingVerification = worker.Verification?.Status == "Pending",
                PortfolioItems = worker.PortfolioItems.Select(p => new PortfolioItemViewModel
                {
                    ItemId = p.ItemId,
                    Title = p.Title ?? "",
                    Description = p.Description,
                    ImageUrl = p.ImageUrl
                }).ToList(),
                Reviews = worker.Reviews.Select(r => new ReviewItemViewModel
                {
                    CustomerName = r.Customer.User.FullName,
                    RatingValue = r.RatingValue,
                    Comment = r.Comment
                }).ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────
        // GET: /Worker/EditProfile
        // ─────────────────────────────
        [HttpGet]
        public IActionResult EditProfile()
        {
            var worker = _workerService.GetWorkerProfile(GetCurrentUserId());
            if (worker == null) return NotFound();

            var vm = new EditWorkerProfileViewModel
            {
                FullName = worker.User.FullName,
                Phone = worker.User.Phone,
                Bio = worker.Bio,
                SpecialtyId = worker.SpecialtyId,       // ← int?
                AllSpecialties = _workerService.GetAllSpecialties()
                    .Select(s => new SpecialtyOption
                    {
                        SpecialtyId = s.SpecialtyId,
                        SpecialtyName = s.SpecialtyName
                    }).ToList(),
                AvailabilityStatus = worker.AvailabilityStatus,
                CurrentPhotoUrl = worker.PhotoUrl,
                SelectedRegionIds = worker.WorksAt.Select(wa => wa.RegionId).ToList(),
                AllRegions = _workerService.GetAllRegions()
                    .Select(r => new RegionOption
                    {
                        RegionId = r.RegionId,
                        RegionName = r.RegionName
                    }).ToList(),
                HasPendingVerification = worker.Verification?.Status == "Pending"
            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Worker/EditProfile
        // ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditWorkerProfileViewModel model)
        {
            int userId = GetCurrentUserId();

            string? photoUrl = null;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                photoUrl = await SaveFile(model.PhotoFile, "ProfilePictures");

            _workerService.UpdateProfile(
                userId,
                model.FullName,
                model.Phone,
                model.Bio,
                model.SpecialtyId,            // ← int? بدل string
                model.AvailabilityStatus,
                model.SelectedRegionIds,
                photoUrl);

            if (model.IdFrontImage != null && model.IdBackImage != null)
            {
                var frontPath = await SaveFile(model.IdFrontImage, "VerificationDocs");
                var backPath = await SaveFile(model.IdBackImage, "VerificationDocs");
                _workerService.SubmitVerification(userId, frontPath, backPath);
            }

            return RedirectToAction("Profile");
        }

        // ─────────────────────────────
        // POST: /Worker/ToggleAvailability
        // ─────────────────────────────
        [HttpPost]
        public IActionResult ToggleAvailability(int status)
        {
            _workerService.UpdateAvailability(GetCurrentUserId(), (AvailabilityStatus)status);
            return RedirectToAction("Profile");
        }

        // ─────────────────────────────
        // Portfolio CRUD
        // ─────────────────────────────

        // POST: /Worker/AddPortfolioItem
        [HttpPost]
        public async Task<IActionResult> AddPortfolioItem(PortfolioItemViewModel model)
        {
            string? imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
                imageUrl = await SaveFile(model.ImageFile, "PortfolioPictures");

            var (success, message) = _portfolioService.AddItem(
                GetCurrentUserId(), model.Title, model.Description, imageUrl);

            if (!success) TempData["Error"] = message;

            return RedirectToAction("Profile");
        }

        // POST: /Worker/DeletePortfolioItem
        [HttpPost]
        public IActionResult DeletePortfolioItem(int itemId)
        {
            _portfolioService.DeleteItem(itemId, GetCurrentUserId());
            return RedirectToAction("Profile");
        }

        // ─────────────────────────────
        // PRIVATE: Save File Helper
        // ─────────────────────────────
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




        // ─────────────────────────────
        // GET: /Worker/SearchRegions?q=cairo
        // ─────────────────────────────
        [HttpGet]
        public IActionResult SearchRegions(string q)
        {
            var results = _workerService.SearchRegions(GetCurrentUserId(), q);
            return Json(results);
        }

        // ─────────────────────────────
        // POST: /Worker/AddRegion
        // ─────────────────────────────
        [HttpPost]
        public IActionResult AddRegion([FromBody] AddRegionRequest request)
        {
            _workerService.AddRegion(GetCurrentUserId(), request.RegionId);
            return Ok();
        }

        // ─────────────────────────────
        // POST: /Worker/RemoveRegion
        // ─────────────────────────────
        [HttpPost]
        public IActionResult RemoveRegion(string regionName)
        {
            _workerService.RemoveRegion(GetCurrentUserId(), regionName);
            return RedirectToAction("Profile");
        }

        // Helper class للـ JSON body
        public class AddRegionRequest
        {
            public int RegionId { get; set; }
        }


        // POST: /Worker/UpdatePortfolioItem
        [HttpPost]
        public async Task<IActionResult> UpdatePortfolioItem(PortfolioItemViewModel model)
        {
            string? imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
                imageUrl = await SaveFile(model.ImageFile, "PortfolioPictures");

            _portfolioService.UpdateItem(
                model.ItemId, GetCurrentUserId(),
                model.Title, model.Description, imageUrl);

            return RedirectToAction("Profile");
        }

    }
}