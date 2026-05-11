using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FixConnect.BLL.Services
{
    public class RequestService
    {
        private readonly AppDbContext _context;

        public RequestService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Create Request
        // ─────────────────────────────
        public int CreateRequest(int customerId, string title, string? description,
            int regionId, int? specialtyId, int? targetWorkerId,
            bool isPrivate, List<string> imagePaths)
        {
            var request = new Request
            {
                Title = title,
                Description = description,
                RegionId = regionId,
                SpecialtyId = specialtyId,
                TargetWorkerId = isPrivate ? targetWorkerId : null,
                RequestType = isPrivate
                    ? (int)RequestType.Targeted
                    : (int)RequestType.Open,
                Status = (int)RequestStatus.Pending,
                UserId = customerId,
                CreatedAt = DateTime.Now
            };

            _context.Requests.Add(request);
            _context.SaveChanges();

            foreach (var path in imagePaths)
            {
                _context.RequestImages.Add(new RequestImage
                {
                    RequestId = request.RequestId,
                    ImagePath = path
                });
            }

            // Update Customer TotalRequests
            var customer = _context.Customers.Find(customerId);
            if (customer != null) customer.TotalRequests++;

            _context.SaveChanges();
            return request.RequestId;
        }

        // ─────────────────────────────
        // Get Customer Requests
        // ─────────────────────────────
        public List<Request> GetCustomerRequests(int customerId)
        {
            return _context.Requests
                .Include(r => r.Region)
                .Include(r => r.Specialty)
                .Include(r => r.Images)
                .Include(r => r.Proposals)
                .Where(r => r.UserId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        // ─────────────────────────────
        // Get Single Request
        // ─────────────────────────────
        public Request? GetRequest(int requestId)
        {
            return _context.Requests
                .Include(r => r.Region)
                .Include(r => r.Specialty)
                .Include(r => r.Images)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .FirstOrDefault(r => r.RequestId == requestId);
        }

        // ─────────────────────────────
        // Edit Request
        // ─────────────────────────────
        public void UpdateRequest(int requestId, string title, string? description,
            int regionId, int? specialtyId,
            List<string> newImagePaths, List<int> deleteImageIds)
        {
            var request = _context.Requests
                .Include(r => r.Images)
                .FirstOrDefault(r => r.RequestId == requestId);

            if (request == null) return;

            request.Title = title;
            request.Description = description;
            request.RegionId = regionId;
            request.SpecialtyId = specialtyId;

            // Delete selected images
            var toDelete = request.Images
                .Where(i => deleteImageIds.Contains(i.ImageId)).ToList();
            foreach (var img in toDelete)
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot",
                    img.ImagePath.TrimStart('/'));
                if (File.Exists(fullPath)) File.Delete(fullPath);
                _context.RequestImages.Remove(img);
            }

            // Add new images
            foreach (var path in newImagePaths)
            {
                _context.RequestImages.Add(new RequestImage
                {
                    RequestId = requestId,
                    ImagePath = path
                });
            }

            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Get Public Feed for Worker
        // ─────────────────────────────
        public List<Request> GetPublicFeed(int workerId, string? search = null, string? regionSearch = null)
        {
            var worker = _context.Workers
                .Include(w => w.WorksAt)
                .FirstOrDefault(w => w.UserId == workerId);

            if (worker == null) return new();

            var workerRegionIds = worker.WorksAt.Select(wa => wa.RegionId).ToList();

            var query = _context.Requests
                .Include(r => r.Region)
                .Include(r => r.Specialty)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Images)
                .Where(r => r.RequestType == (int)RequestType.Open
                         && r.Status == (int)RequestStatus.Pending)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r => r.Title.Contains(search)
                                      || r.Description!.Contains(search));

            if (!string.IsNullOrWhiteSpace(regionSearch))
                query = query.Where(r => r.Region.RegionName.Contains(regionSearch));

            return query
                .OrderByDescending(r => workerRegionIds.Contains(r.RegionId))
                .ThenByDescending(r => r.SpecialtyId == worker.SpecialtyId)
                .ThenByDescending(r => r.CreatedAt)
                .ToList();
        }

        // ─────────────────────────────
        // Get Direct Requests for Worker
        // ─────────────────────────────
        public List<Request> GetDirectRequests(int workerId)
        {
            return _context.Requests
                .Include(r => r.Region)
                .Include(r => r.Specialty)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Images)
                .Where(r => r.TargetWorkerId == workerId
                         && r.RequestType == (int)RequestType.Targeted)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        // ─────────────────────────────
        // Get Received Proposals for Customer
        // ─────────────────────────────
        public List<Proposal> GetReceivedProposals(int customerId)
        {
            return _context.Proposals
                .Include(p => p.Request)
                .Include(p => p.Worker)
                    .ThenInclude(w => w.User)
                .Include(p => p.Worker)
                    .ThenInclude(w => w.Specialty)
                .Include(p => p.Worker)
                    .ThenInclude(w => w.PortfolioItems)
                .Include(p => p.Worker)
                    .ThenInclude(w => w.Reviews)
                        .ThenInclude(r => r.Customer)
                            .ThenInclude(c => c.User)
                .Where(p => p.UserId == customerId)
                .OrderByDescending(p => p.ProposalId)
                .ToList();
        }

        // ─────────────────────────────
        // Get Workers (Home Filter)
        // ─────────────────────────────
        public List<Worker> GetFilteredWorkers(
            string? search, int? specialtyId, string? regionSearch)
        {
            var query = _context.Workers
                .Include(w => w.User)
                .Include(w => w.Specialty)
                .Include(w => w.WorksAt).ThenInclude(wa => wa.Region)
               .Where(w => w.User.IsActive && w.User.RoleType == RoleType.Worker)
                .AsQueryable();

            if (specialtyId.HasValue)
                query = query.Where(w => w.SpecialtyId == specialtyId);

            if (!string.IsNullOrWhiteSpace(regionSearch))
                query = query.Where(w =>
                    w.WorksAt.Any(wa => wa.Region.RegionName.Contains(regionSearch)));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(w =>
                    w.User.FullName.Contains(search));

            return query
                .OrderByDescending(w => w.AvgRating)
                .ToList();
        }

        public void DeleteRequest(int requestId)
        {
            var request = _context.Requests.Find(requestId);
            if (request == null) return;
            _context.Requests.Remove(request);
            _context.SaveChanges();
        }

        public List<Proposal> GetProposalsByRequest(int requestId)
        {
            return _context.Proposals
                .Include(p => p.Request)
                .Include(p => p.Worker)
                    .ThenInclude(w => w.User)
                .Include(p => p.Worker)
                    .ThenInclude(w => w.Specialty)
                .Include(p => p.Worker)
                    .ThenInclude(w => w.PortfolioItems)
                .Include(p => p.Worker)
                    .ThenInclude(w => w.Reviews)
                        .ThenInclude(r => r.Customer)
                            .ThenInclude(c => c.User)
                .Where(p => p.RequestId == requestId)
                .OrderByDescending(p => p.ProposalId)
                .ToList();
        }

    }
}