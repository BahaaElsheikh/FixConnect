using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;
// أضف في الأعلى
using FixConnect.BLL.DTOs;

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
                .Include(w => w.Specialty)
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
     string? bio, int? specialtyId,              // ← بدل string? specialty
     AvailabilityStatus status,
     List<int> regionIds, string? photoUrl)
        {
            var worker = _context.Workers
                .Include(w => w.User)
                .Include(w => w.WorksAt)
                .FirstOrDefault(w => w.UserId == userId);

            if (worker == null) return;

            worker.User.FullName = fullName;
            worker.User.Phone = phone;
            worker.Bio = bio;
            worker.SpecialtyId = specialtyId;        // ← int? بدل string
            worker.AvailabilityStatus = status;

            if (photoUrl != null)
                worker.PhotoUrl = photoUrl;

            var existing = _context.WorksAt.Where(wa => wa.UserId == userId).ToList();
            _context.WorksAt.RemoveRange(existing);

            foreach (var regionId in regionIds)
            {
                _context.WorksAt.Add(new WorksAt
                {
                    UserId = userId,
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
                existing.IdBackImagePath = backPath;
                existing.Status = "Pending";
                existing.SubmittedAt = DateTime.Now;
                existing.ReviewedAt = null;
            }
            else
            {
                _context.WorkerVerifications.Add(new WorkerVerification
                {
                    WorkerId = workerId,
                    IdFrontImagePath = frontPath,
                    IdBackImagePath = backPath,
                    Status = "Pending",
                    SubmittedAt = DateTime.Now
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



        // Search Regions (مش موجودة عند الـ Worker بالفعل)
        public List<object> SearchRegions(int workerId, string query)
        {
            var workerRegionIds = _context.WorksAt
                .Where(wa => wa.UserId == workerId)
                .Select(wa => wa.RegionId)
                .ToList();

            return _context.Regions
                .Where(r => r.RegionName.Contains(query) && !workerRegionIds.Contains(r.RegionId))
                .Take(5)
                .Select(r => new { r.RegionId, r.RegionName })
                .ToList<object>();
        }

        // Add Region
        public void AddRegion(int workerId, int regionId)
        {
            var exists = _context.WorksAt
                .Any(wa => wa.UserId == workerId && wa.RegionId == regionId);

            if (exists) return;

            _context.WorksAt.Add(new WorksAt
            {
                UserId = workerId,
                RegionId = regionId
            });
            _context.SaveChanges();
        }

        // Remove Region
        public void RemoveRegion(int workerId, string regionName)
        {
            var region = _context.Regions
                .FirstOrDefault(r => r.RegionName == regionName);
            if (region == null) return;

            var entry = _context.WorksAt
                .FirstOrDefault(wa => wa.UserId == workerId && wa.RegionId == region.RegionId);
            if (entry == null) return;

            _context.WorksAt.Remove(entry);
            _context.SaveChanges();
        }


        public PublicWorkerProfileViewModel? GetPublicProfile(int workerId)
        {
            var worker = _context.Workers
                .Include(w => w.User)
                .Include(w => w.Specialty)
                .Include(w => w.WorksAt).ThenInclude(wa => wa.Region)
                .Include(w => w.PortfolioItems)
                .Include(w => w.Reviews).ThenInclude(r => r.Customer).ThenInclude(c => c.User)
                .Include(w => w.Verification)
                .FirstOrDefault(w => w.UserId == workerId);

            if (worker == null) return null;

            return new PublicWorkerProfileViewModel
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
                }).ToList(),
                Verification = worker.Verification == null ? null : new VerificationViewModel
                {
                    WorkerId = worker.UserId,
                    IdFrontImagePath = worker.Verification.IdFrontImagePath,
                    IdBackImagePath = worker.Verification.IdBackImagePath,
                    Status = worker.Verification.Status,
                    SubmittedAt = worker.Verification.SubmittedAt
                }
            };
        }

        public List<Specialty> GetAllSpecialties()
    => _context.Specialties.OrderBy(s => s.SpecialtyName).ToList();

    }


}