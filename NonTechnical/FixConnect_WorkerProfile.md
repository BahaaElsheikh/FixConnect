# FixConnect — Worker Profile & Portfolio (Full Guide)

---

## 📁 Files You Will Create / Edit in This Phase

```
FixConnect.DAL/
├── Models/
│   ├── Worker.cs                        ← Edit: Add PhotoUrl
│   └── WorkerVerification.cs            ← New
├── Context/
│   └── AppDbContext.cs                  ← Edit: Add WorkerVerifications DbSet + Config

FixConnect.BLL/
└── Services/
    ├── WorkerService.cs                 ← New
    └── PortfolioService.cs              ← New

FixConnect.PL/
├── Controllers/
│   └── WorkerController.cs             ← New
├── ViewModels/
│   ├── WorkerProfileViewModel.cs       ← New
│   ├── EditWorkerProfileViewModel.cs   ← New
│   └── PortfolioItemViewModel.cs       ← New
└── Views/
    └── Worker/
        ├── Profile.cshtml              ← New
        ├── EditProfile.cshtml          ← New
        └── Portfolio.cshtml            ← New

wwwroot/uploads/
├── ProfilePictures/                    ← Create manually
├── PortfolioPictures/                  ← Create manually
└── VerificationDocs/                   ← Create manually (Admin only)
```

---

## 🗄️ Step 1 — Update Worker Model

### File: `FixConnect.DAL/Models/Worker.cs`
```csharp
using FixConnect.DAL.Data.Enums;

namespace FixConnect.DAL.Models
{
    public class Worker
    {
        public int UserId { get; set; }
        public string? Specialty { get; set; }
        public string? Bio { get; set; }
        public bool IsVerified { get; set; } = false;
        public decimal AvgRating { get; set; } = 0;
        public AvailabilityStatus AvailabilityStatus { get; set; }
        public string? PhotoUrl { get; set; }              // ← NEW

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<WorksAt> WorksAt { get; set; } = new List<WorksAt>();
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public Wallet? Wallet { get; set; }
        public ICollection<Request> TargetedRequests { get; set; } = new List<Request>();
        public WorkerVerification? Verification { get; set; }  // ← NEW
    }
}
```

---

## 🗄️ Step 2 — New Entity: WorkerVerification

### File: `FixConnect.DAL/Models/WorkerVerification.cs`
```csharp
namespace FixConnect.DAL.Models
{
    public class WorkerVerification
    {
        public int VerificationId { get; set; }
        public int WorkerId { get; set; }
        public string IdFrontImagePath { get; set; } = null!;
        public string IdBackImagePath { get; set; } = null!;
        public string Status { get; set; } = "Pending";   // Pending / Approved / Rejected
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }

        // Navigation
        public Worker Worker { get; set; } = null!;
    }
}
```

---

## 🗄️ Step 3 — Update AppDbContext

### File: `FixConnect.DAL/Context/AppDbContext.cs`

#### أضف الـ DbSet:
```csharp
public DbSet<WorkerVerification> WorkerVerifications { get; set; }
```

#### أضف الـ Configuration في `OnModelCreating`:
```csharp
// ============================
// WorkerVerification
// ============================
modelBuilder.Entity<WorkerVerification>(entity =>
{
    entity.HasKey(v => v.VerificationId);
    entity.Property(v => v.Status).HasDefaultValue("Pending");
    entity.Property(v => v.SubmittedAt).HasDefaultValueSql("GETDATE()");

    entity.HasOne(v => v.Worker)
          .WithOne(w => w.Verification)
          .HasForeignKey<WorkerVerification>(v => v.WorkerId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

#### ثم Run Migration:
```bash
Add-Migration AddWorkerPhotoAndVerification -Project FixConnect.DAL -StartupProject FixConnect.PL
Update-Database -Project FixConnect.DAL -StartupProject FixConnect.PL
```

---

## 📋 Step 4 — ViewModels

### File: `FixConnect.PL/ViewModels/WorkerProfileViewModel.cs`
```csharp
namespace FixConnect.PL.ViewModels
{
    public class WorkerProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Bio { get; set; }
        public string? Specialty { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsVerified { get; set; }
        public string AvailabilityStatus { get; set; } = null!;
        public decimal AvgRating { get; set; }
        public List<string> WorkingRegions { get; set; } = new();
        public List<PortfolioItemViewModel> PortfolioItems { get; set; } = new();
        public List<ReviewItemViewModel> Reviews { get; set; } = new();
        public bool HasPendingVerification { get; set; }
    }

    public class ReviewItemViewModel
    {
        public string CustomerName { get; set; } = null!;
        public int RatingValue { get; set; }
        public string? Comment { get; set; }
    }
}
```

---

### File: `FixConnect.PL/ViewModels/EditWorkerProfileViewModel.cs`
```csharp
using FixConnect.DAL.Data.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class EditWorkerProfileViewModel
    {
        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        public string? Bio { get; set; }
        public string? Specialty { get; set; }
        public AvailabilityStatus AvailabilityStatus { get; set; }
        public List<int> SelectedRegionIds { get; set; } = new();
        public List<RegionOption> AllRegions { get; set; } = new();

        // Photo Upload
        public IFormFile? PhotoFile { get; set; }
        public string? CurrentPhotoUrl { get; set; }

        // Verification Upload
        public IFormFile? IdFrontImage { get; set; }
        public IFormFile? IdBackImage { get; set; }
        public bool HasPendingVerification { get; set; }
    }

    public class RegionOption
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; } = null!;
    }
}
```

---

### File: `FixConnect.PL/ViewModels/PortfolioItemViewModel.cs`
```csharp
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class PortfolioItemViewModel
    {
        public int ItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = null!;

        [StringLength(300)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
```

---

## ⚙️ Step 5 — Worker Service (BLL)

### File: `FixConnect.BLL/Services/WorkerService.cs`
```csharp
using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FixConnect.BLL.Services
{
    public class WorkerService
    {
        // ✅ DI: AppDbContext injected
        private readonly AppDbContext _context;

        public WorkerService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Get Full Worker Profile
        // ─────────────────────────────
        public Worker? GetWorkerProfile(int userId)
        {
            return _context.Workers
                .Include(w => w.User)
                .Include(w => w.WorksAt).ThenInclude(wa => wa.Region)
                .Include(w => w.PortfolioItems)
                .Include(w => w.Reviews).ThenInclude(r => r.Customer).ThenInclude(c => c.User)
                .Include(w => w.Verification)
                .FirstOrDefault(w => w.UserId == userId);
        }

        // ─────────────────────────────
        // Update Profile Info
        // ─────────────────────────────
        public void UpdateProfile(int userId, string fullName, string phone,
            string? bio, string? specialty, AvailabilityStatus status,
            List<int> regionIds, string? photoUrl)
        {
            var worker = _context.Workers
                .Include(w => w.User)
                .Include(w => w.WorksAt)
                .FirstOrDefault(w => w.UserId == userId);

            if (worker == null) return;

            // Update User base info
            worker.User.FullName = fullName;
            worker.User.Phone    = phone;

            // Update Worker info
            worker.Bio                = bio;
            worker.Specialty          = specialty;
            worker.AvailabilityStatus = status;

            if (photoUrl != null)
                worker.PhotoUrl = photoUrl;

            // Update Regions
            var existing = _context.WorksAt.Where(wa => wa.UserId == userId).ToList();
            _context.WorksAt.RemoveRange(existing);

            foreach (var regionId in regionIds)
            {
                _context.WorksAt.Add(new WorksAt
                {
                    UserId   = userId,
                    RegionId = regionId
                });
            }

            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Toggle Availability
        // ─────────────────────────────
        public void UpdateAvailability(int userId, AvailabilityStatus status)
        {
            var worker = _context.Workers.Find(userId);
            if (worker == null) return;

            worker.AvailabilityStatus = status;
            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Submit Verification
        // ─────────────────────────────
        public (bool Success, string Message) SubmitVerification(
            int workerId, string frontPath, string backPath)
        {
            var existing = _context.WorkerVerifications
                .FirstOrDefault(v => v.WorkerId == workerId);

            if (existing != null && existing.Status == "Pending")
                return (false, "You already have a pending verification request.");

            if (existing != null)
            {
                existing.IdFrontImagePath = frontPath;
                existing.IdBackImagePath  = backPath;
                existing.Status           = "Pending";
                existing.SubmittedAt      = DateTime.Now;
                existing.ReviewedAt       = null;
            }
            else
            {
                _context.WorkerVerifications.Add(new WorkerVerification
                {
                    WorkerId          = workerId,
                    IdFrontImagePath  = frontPath,
                    IdBackImagePath   = backPath,
                    Status            = "Pending",
                    SubmittedAt       = DateTime.Now
                });
            }

            _context.SaveChanges();
            return (true, "Verification submitted successfully.");
        }

        // ─────────────────────────────
        // Get All Regions
        // ─────────────────────────────
        public List<DAL.Models.Region> GetAllRegions()
            => _context.Regions.ToList();
    }
}
```

---

## ⚙️ Step 6 — Portfolio Service (BLL)

### File: `FixConnect.BLL/Services/PortfolioService.cs`
```csharp
using FixConnect.DAL.Context;
using FixConnect.DAL.Models;

namespace FixConnect.BLL.Services
{
    public class PortfolioService
    {
        // ✅ DI: AppDbContext injected
        private readonly AppDbContext _context;
        private const int MaxItems = 10;

        public PortfolioService(AppDbContext context)
        {
            _context = context;
        }

        public List<PortfolioItem> GetItems(int workerId)
            => _context.PortfolioItems
                .Where(p => p.UserId == workerId)
                .ToList();

        public (bool Success, string Message) AddItem(
            int workerId, string title, string? description, string? imageUrl)
        {
            var count = _context.PortfolioItems.Count(p => p.UserId == workerId);
            if (count >= MaxItems)
                return (false, $"Maximum {MaxItems} portfolio items allowed.");

            _context.PortfolioItems.Add(new PortfolioItem
            {
                UserId      = workerId,
                Title       = title,
                Description = description,
                ImageUrl    = imageUrl
            });

            _context.SaveChanges();
            return (true, "Item added successfully.");
        }

        public (bool Success, string Message) UpdateItem(
            int itemId, int workerId, string title, string? description, string? imageUrl)
        {
            var item = _context.PortfolioItems
                .FirstOrDefault(p => p.ItemId == itemId && p.UserId == workerId);

            if (item == null) return (false, "Item not found.");

            item.Title       = title;
            item.Description = description;

            if (imageUrl != null)
                item.ImageUrl = imageUrl;

            _context.SaveChanges();
            return (true, "Item updated.");
        }

        public (bool Success, string Message) DeleteItem(int itemId, int workerId)
        {
            var item = _context.PortfolioItems
                .FirstOrDefault(p => p.ItemId == itemId && p.UserId == workerId);

            if (item == null) return (false, "Item not found.");

            // Delete image file from disk
            if (!string.IsNullOrEmpty(item.ImageUrl))
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", item.ImageUrl.TrimStart('/'));
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }

            _context.PortfolioItems.Remove(item);
            _context.SaveChanges();
            return (true, "Item deleted.");
        }

        public PortfolioItem? GetItem(int itemId, int workerId)
            => _context.PortfolioItems
                .FirstOrDefault(p => p.ItemId == itemId && p.UserId == workerId);
    }
}
```

---

## 🎮 Step 7 — Worker Controller

### File: `FixConnect.PL/Controllers/WorkerController.cs`
```csharp
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
            _workerService    = workerService;
            _portfolioService = portfolioService;
            _env              = env;
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
                UserId             = worker.UserId,
                FullName           = worker.User.FullName,
                Email              = worker.User.Email,
                Phone              = worker.User.Phone,
                Bio                = worker.Bio,
                Specialty          = worker.Specialty,
                PhotoUrl           = worker.PhotoUrl,
                IsVerified         = worker.IsVerified,
                AvailabilityStatus = worker.AvailabilityStatus.ToString(),
                AvgRating          = worker.AvgRating,
                WorkingRegions     = worker.WorksAt.Select(wa => wa.Region.RegionName).ToList(),
                HasPendingVerification = worker.Verification?.Status == "Pending",
                PortfolioItems     = worker.PortfolioItems.Select(p => new PortfolioItemViewModel
                {
                    ItemId      = p.ItemId,
                    Title       = p.Title ?? "",
                    Description = p.Description,
                    ImageUrl    = p.ImageUrl
                }).ToList(),
                Reviews = worker.Reviews.Select(r => new ReviewItemViewModel
                {
                    CustomerName = r.Customer.User.FullName,
                    RatingValue  = r.RatingValue,
                    Comment      = r.Comment
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
                FullName           = worker.User.FullName,
                Phone              = worker.User.Phone,
                Bio                = worker.Bio,
                Specialty          = worker.Specialty,
                AvailabilityStatus = worker.AvailabilityStatus,
                CurrentPhotoUrl    = worker.PhotoUrl,
                SelectedRegionIds  = worker.WorksAt.Select(wa => wa.RegionId).ToList(),
                AllRegions         = _workerService.GetAllRegions()
                    .Select(r => new RegionOption
                    {
                        RegionId   = r.RegionId,
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

            // Handle Photo Upload
            string? photoUrl = null;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                photoUrl = await SaveFile(model.PhotoFile, "ProfilePictures");
            }

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
                var backPath  = await SaveFile(model.IdBackImage,  "VerificationDocs");
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

            var fileName  = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath  = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }
    }
}
```

---

## ⚙️ Step 8 — Register Services in Program.cs

### أضف في `FixConnect.PL/Program.cs` بعد `AddScoped<AuthService>()`:
```csharp
// ✅ DI: Worker Phase Services
builder.Services.AddScoped<WorkerService>();
builder.Services.AddScoped<PortfolioService>();
```

---

## 🎨 Step 9 — Views

### File: `FixConnect.PL/Views/Worker/Profile.cshtml`
```html
@model FixConnect.PL.ViewModels.WorkerProfileViewModel
@{
    ViewData["Title"] = "My Profile";
}

<div class="container py-4">

    @if (TempData["Error"] != null)
    {
        <div class="alert alert-danger">@TempData["Error"]</div>
    }

    <div class="row g-4">

        {{!-- Left: Profile Card --}}
        <div class="col-md-4">
            <div class="card shadow-sm text-center p-4">
                <img src="@(Model.PhotoUrl ?? "/uploads/ProfilePictures/default.png")"
                     class="rounded-circle mx-auto mb-3"
                     style="width:110px;height:110px;object-fit:cover;" />

                <h5 class="fw-bold mb-1">
                    @Model.FullName
                    @if (Model.IsVerified)
                    {
                        <span class="badge bg-primary ms-1" title="Verified">✔ Verified</span>
                    }
                </h5>

                <p class="text-muted small mb-1">@Model.Specialty</p>
                <p class="text-muted small mb-2">@Model.Email</p>

                {{!-- Star Rating --}}
                <div class="mb-2">
                    @for (int i = 1; i <= 5; i++)
                    {
                        <span style="color:@(i <= (int)Model.AvgRating ? "#f5a623" : "#ccc")">★</span>
                    }
                    <small class="text-muted">(@Model.AvgRating.ToString("0.0"))</small>
                </div>

                {{!-- Availability Badge --}}
                <span class="badge @(Model.AvailabilityStatus == "Available" ? "bg-success" :
                                      Model.AvailabilityStatus == "Busy"      ? "bg-warning text-dark" :
                                                                                 "bg-secondary")">
                    @Model.AvailabilityStatus
                </span>

                <hr />

                {{!-- Regions --}}
                <p class="small fw-semibold mb-1">Working Areas</p>
                @foreach (var region in Model.WorkingRegions)
                {
                    <span class="badge bg-light text-dark border me-1">@region</span>
                }

                <div class="mt-3">
                    <a asp-action="EditProfile" class="btn btn-outline-dark btn-sm w-100">
                        Edit Profile
                    </a>
                </div>
            </div>
        </div>

        {{!-- Right: Bio + Portfolio + Reviews --}}
        <div class="col-md-8">

            {{!-- Bio --}}
            @if (!string.IsNullOrEmpty(Model.Bio))
            {
                <div class="card shadow-sm p-4 mb-4">
                    <h6 class="fw-bold mb-2">About Me</h6>
                    <p class="text-muted mb-0">@Model.Bio</p>
                </div>
            }

            {{!-- Verification Banner --}}
            @if (!Model.IsVerified)
            {
                <div class="alert @(Model.HasPendingVerification ? "alert-warning" : "alert-info")">
                    @if (Model.HasPendingVerification)
                    {
                        <span>⏳ Your verification is under review.</span>
                    }
                    else
                    {
                        <span>
                            🔒 You are not verified yet.
                            <a asp-action="EditProfile">Upload your ID to get verified.</a>
                        </span>
                    }
                </div>
            }

            {{!-- Portfolio --}}
            <div class="card shadow-sm p-4 mb-4">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h6 class="fw-bold mb-0">Portfolio (@Model.PortfolioItems.Count / 10)</h6>
                    @if (Model.PortfolioItems.Count < 10)
                    {
                        <button class="btn btn-sm btn-dark"
                                data-bs-toggle="modal" data-bs-target="#addPortfolioModal">
                            + Add Item
                        </button>
                    }
                </div>

                @if (!Model.PortfolioItems.Any())
                {
                    <p class="text-muted small">No portfolio items yet.</p>
                }
                else
                {
                    <div class="row g-3">
                        @foreach (var item in Model.PortfolioItems)
                        {
                            <div class="col-6 col-md-4">
                                <div class="card border-0 shadow-sm h-100">
                                    @if (!string.IsNullOrEmpty(item.ImageUrl))
                                    {
                                        <img src="@item.ImageUrl" class="card-img-top"
                                             style="height:140px;object-fit:cover;" />
                                    }
                                    <div class="card-body p-2">
                                        <p class="fw-semibold small mb-0">@item.Title</p>
                                        <p class="text-muted" style="font-size:0.75rem">@item.Description</p>
                                        <form asp-action="DeletePortfolioItem" method="post"
                                              onsubmit="return confirm('Delete this item?')">
                                            <input type="hidden" name="itemId" value="@item.ItemId" />
                                            <button type="submit"
                                                    class="btn btn-outline-danger btn-sm w-100">
                                                Delete
                                            </button>
                                        </form>
                                    </div>
                                </div>
                            </div>
                        }
                    </div>
                }
            </div>

            {{!-- Reviews --}}
            <div class="card shadow-sm p-4">
                <h6 class="fw-bold mb-3">Reviews</h6>

                @if (!Model.Reviews.Any())
                {
                    <p class="text-muted small">No reviews yet.</p>
                }
                else
                {
                    @foreach (var r in Model.Reviews)
                    {
                        <div class="border-bottom pb-3 mb-3">
                            <div class="d-flex justify-content-between">
                                <strong class="small">@r.CustomerName</strong>
                                <span>
                                    @for (int i = 1; i <= 5; i++)
                                    {
                                        <span style="color:@(i <= r.RatingValue ? "#f5a623" : "#ccc")">★</span>
                                    }
                                </span>
                            </div>
                            <p class="text-muted small mb-0">@r.Comment</p>
                        </div>
                    }
                }
            </div>
        </div>
    </div>
</div>

{{!-- Add Portfolio Modal --}}
<div class="modal fade" id="addPortfolioModal" tabindex="-1">
    <div class="modal-dialog">
        <form asp-action="AddPortfolioItem" method="post" enctype="multipart/form-data">
            <div class="modal-content">
                <div class="modal-header">
                    <h6 class="modal-title fw-bold">Add Portfolio Item</h6>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Title</label>
                        <input name="Title" class="form-control" required />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Description</label>
                        <textarea name="Description" class="form-control" rows="2"></textarea>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Image</label>
                        <input name="ImageFile" type="file" class="form-control" accept="image/*" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="submit" class="btn btn-dark">Add</button>
                </div>
            </div>
        </form>
    </div>
</div>
```

---

### File: `FixConnect.PL/Views/Worker/EditProfile.cshtml`
```html
@model FixConnect.PL.ViewModels.EditWorkerProfileViewModel
@using FixConnect.DAL.Data.Enums
@{
    ViewData["Title"] = "Edit Profile";
}

<div class="container py-4" style="max-width:680px">
    <h5 class="fw-bold mb-4">Edit Profile</h5>

    <form asp-action="EditProfile" method="post" enctype="multipart/form-data">

        {{!-- Basic Info --}}
        <div class="card shadow-sm p-4 mb-4">
            <h6 class="fw-semibold mb-3">Basic Information</h6>

            <div class="mb-3">
                <label class="form-label">Full Name</label>
                <input asp-for="FullName" class="form-control" />
                <span asp-validation-for="FullName" class="text-danger small"></span>
            </div>

            <div class="mb-3">
                <label class="form-label">Phone</label>
                <input asp-for="Phone" class="form-control" />
            </div>

            <div class="mb-3">
                <label class="form-label">Specialty</label>
                <input asp-for="Specialty" class="form-control" />
            </div>

            <div class="mb-3">
                <label class="form-label">Bio</label>
                <textarea asp-for="Bio" class="form-control" rows="3"></textarea>
            </div>

            {{!-- Profile Photo --}}
            <div class="mb-3">
                <label class="form-label">Profile Photo</label>
                @if (!string.IsNullOrEmpty(Model.CurrentPhotoUrl))
                {
                    <div class="mb-2">
                        <img src="@Model.CurrentPhotoUrl"
                             style="width:80px;height:80px;object-fit:cover;border-radius:50%;" />
                    </div>
                }
                <input asp-for="PhotoFile" type="file" class="form-control" accept="image/*" />
            </div>
        </div>

        {{!-- Availability --}}
        <div class="card shadow-sm p-4 mb-4">
            <h6 class="fw-semibold mb-3">Availability</h6>
            <select asp-for="AvailabilityStatus" class="form-select">
                <option value="@((int)AvailabilityStatus.Available)">Available</option>
                <option value="@((int)AvailabilityStatus.Busy)">Busy</option>
                <option value="@((int)AvailabilityStatus.Offline)">Offline</option>
            </select>
        </div>

        {{!-- Working Regions --}}
        <div class="card shadow-sm p-4 mb-4">
            <h6 class="fw-semibold mb-3">Working Areas</h6>
            <div class="row">
                @foreach (var region in Model.AllRegions)
                {
                    <div class="col-6 col-md-4">
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox"
                                   name="SelectedRegionIds" value="@region.RegionId"
                                   @(Model.SelectedRegionIds.Contains(region.RegionId) ? "checked" : "") />
                            <label class="form-check-label small">@region.RegionName</label>
                        </div>
                    </div>
                }
            </div>
        </div>

        {{!-- Verification --}}
        @if (!Model.HasPendingVerification)
        {
            <div class="card shadow-sm p-4 mb-4">
                <h6 class="fw-semibold mb-1">ID Verification</h6>
                <p class="text-muted small mb-3">
                    Upload your national ID to get a Verified badge.
                </p>

                <div class="mb-3">
                    <label class="form-label">ID Front</label>
                    <input asp-for="IdFrontImage" type="file" class="form-control" accept="image/*" />
                </div>
                <div class="mb-3">
                    <label class="form-label">ID Back</label>
                    <input asp-for="IdBackImage" type="file" class="form-control" accept="image/*" />
                </div>
            </div>
        }
        else
        {
            <div class="alert alert-warning">
                ⏳ Verification is under review. You cannot re-submit until it is processed.
            </div>
        }

        <button type="submit" class="btn btn-dark w-100">Save Changes</button>
        <a asp-action="Profile" class="btn btn-outline-secondary w-100 mt-2">Cancel</a>
    </form>
</div>

<partial name="_ValidationScriptsPartial" />
```

---

## ✅ Phase Checklist

- [ ] `Worker.cs` — أضف `PhotoUrl` و `Verification` navigation
- [ ] `WorkerVerification.cs` — Entity جديدة في DAL/Models
- [ ] `AppDbContext.cs` — أضف DbSet + Configuration
- [ ] Migration اتعملت وتشتغلت
- [ ] `WorkerService.cs` في BLL/Services
- [ ] `PortfolioService.cs` في BLL/Services
- [ ] ViewModels الـ 3 اتعملوا
- [ ] `WorkerController.cs` في PL/Controllers
- [ ] Views الـ 2 اتعملوا في Views/Worker/
- [ ] Services اتسجلوا في `Program.cs`
- [ ] Folders اتعملوا يدوي:
  ```
  wwwroot/uploads/ProfilePictures/
  wwwroot/uploads/PortfolioPictures/
  wwwroot/uploads/VerificationDocs/
  ```
