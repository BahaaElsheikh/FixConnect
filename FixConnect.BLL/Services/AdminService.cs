using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using Microsoft.EntityFrameworkCore;
using FixConnect.BLL.DTOs;


namespace FixConnect.BLL.Services
{
    public class AdminService
    {
        // ✅ DI: AppDbContext injected
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Get All Users (with filter)
        // ─────────────────────────────
        public List<UserResult> GetUsers(string? search, string? roleFilter)
        {
            var query = _context.Users
     .Include(u => u.Worker).ThenInclude(w => w.Verification) // ← تأكد من عمل Include لجدول التحقق هنا
     .AsQueryable();

            // Filter by Role
            if (roleFilter == "Worker")
                query = query.Where(u => u.RoleType == RoleType.Worker);
            else if (roleFilter == "Customer")
                query = query.Where(u => u.RoleType == RoleType.Customer);
            else
                query = query.Where(u => u.RoleType != RoleType.Admin);

            // Search
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Phone.Contains(search));

            return query.Select(u => new UserResult
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.RoleType.ToString(),
                IsActive = u.IsActive,
                IsVerified = u.Worker != null && u.Worker.IsVerified,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        // ─────────────────────────────
        // Activate / Deactivate User
        // ─────────────────────────────
        public void SetUserActive(int userId, bool isActive)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;

            user.IsActive = isActive;
            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Verify Worker (from Users page)
        // ─────────────────────────────
        public void VerifyWorker(int workerId)
        {
            var worker = _context.Workers.Find(workerId);
            if (worker == null) return;

            worker.IsVerified = true;

            var verification = _context.WorkerVerifications
                .FirstOrDefault(v => v.WorkerId == workerId);
            if (verification != null)
            {
                verification.Status = "Approved";
                verification.ReviewedAt = DateTime.Now;
            }

            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Get All Requests
        // ─────────────────────────────
        public List<RequestResult> GetAllRequests()
        {
            return _context.Requests
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Region)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RequestResult
                {
                    RequestId = r.RequestId,
                    Title = r.Title,
                    CustomerName = r.Customer.User.FullName,
                    RegionName = r.Region.RegionName,
                    Status = r.Status.ToString(),
                    RequestType = r.RequestType.ToString(),
                    CreatedAt = r.CreatedAt
                }).ToList();
        }

        // ─────────────────────────────
        // Delete Request
        // ─────────────────────────────
        public void DeleteRequest(int requestId)
        {
            var request = _context.Requests.Find(requestId);
            if (request == null) return;

            _context.Requests.Remove(request);
            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Get All Proposals
        // ─────────────────────────────
        public List<ProposalResult> GetAllProposals()
        {
            return _context.Proposals
                .Include(p => p.Worker).ThenInclude(w => w.User)
                .Include(p => p.Customer).ThenInclude(c => c.User)
                .Include(p => p.Request)
                .OrderByDescending(p => p.ProposalId)
                .Select(p => new ProposalResult
                {
                    ProposalId = p.ProposalId,
                    WorkerName = p.Worker.User.FullName,
                    CustomerName = p.Customer.User.FullName,
                    RequestTitle = p.Request.Title,
                    LaborCost = p.LaborCost,
                    MaterialCost = p.MaterialCost,
                    DurationEstimate = p.DurationEstimate,
                    Status = p.Status.ToString()
                }).ToList();
        }

        // ─────────────────────────────
        // Delete Proposal
        // ─────────────────────────────
        public void DeleteProposal(int proposalId)
        {
            var proposal = _context.Proposals.Find(proposalId);
            if (proposal == null) return;

            _context.Proposals.Remove(proposal);
            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Get All Jobs (لأدمن النظام)
        // ─────────────────────────────
        public List<FixConnect.DAL.Models.Job> GetAllJobs()
        {
            return _context.Jobs
                .Include(j => j.Proposal).ThenInclude(p => p.Request).ThenInclude(r => r.Customer).ThenInclude(c => c.User)
                .Include(j => j.Proposal).ThenInclude(p => p.Worker).ThenInclude(w => w.User)
                .Include(j => j.InvoiceItems)
                .OrderByDescending(j => j.JobId)
                .ToList();
        }




        // ─────────────────────────────
        // Result Classes (internal DTOs)
        // ─────────────────────────────
        //public class UserResult
        //{
        //    public int UserId { get; set; }
        //    public string FullName { get; set; } = null!;
        //    public string Email { get; set; } = null!;
        //    public string Phone { get; set; } = null!;
        //    public string Role { get; set; } = null!;
        //    public bool IsActive { get; set; }
        //    public bool IsVerified { get; set; }
        //    public DateTime CreatedAt { get; set; }
        //}

        ////public class RequestResult
        ////{
        ////    public int RequestId { get; set; }
        ////    public string Title { get; set; } = null!;
        ////    public string CustomerName { get; set; } = null!;
        ////    public string RegionName { get; set; } = null!;
        ////    public string Status { get; set; } = null!;
        ////    public string RequestType { get; set; } = null!;
        ////    public DateTime CreatedAt { get; set; }
        ////}

        //public class ProposalResult
        //{
        //    public int ProposalId { get; set; }
        //    public string WorkerName { get; set; } = null!;
        //    public string CustomerName { get; set; } = null!;
        //    public string RequestTitle { get; set; } = null!;
        //    public decimal? LaborCost { get; set; }
        //    public decimal? MaterialCost { get; set; }
        //    public int? DurationEstimate { get; set; }
        //    public string Status { get; set; } = null!;
        //}
    }
}