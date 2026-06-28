using FixConnect.BLL.Services;
using FixConnect.DAL.Context;
using FixConnect.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FixConnect.PL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // ✅ DI: Services injected
        private readonly AdminService _adminService;
        private readonly WorkerService _workerService;
        private readonly AppDbContext _context;


        private const int PageSize = 10;   


        public AdminController(AdminService adminService,
            WorkerService workerService,
            AppDbContext context)
        {
            _adminService = adminService;
            _workerService = workerService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Users");
        }

        // ─────────────────────────────
        // GET: /Admin/Users
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Users(string? search, string? roleFilter, string? sortOrder, int page = 1)
        {
            var users = _adminService.GetUsers(search, roleFilter);




            if (sortOrder == "date_asc")
            {
                users = users.OrderBy(u => u.CreatedAt).ToList();
            }
            else
            {
                // الديفولت أو الـ "date_desc" يكون تنازلي (الأحدث أولاً)
                users = users.OrderByDescending(u => u.CreatedAt).ToList();
                sortOrder = "date_desc";
            }


            var rows = users.Select(u => new UserRowViewModel
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive,
                IsVerified = u.IsVerified,
                CreatedAt = u.CreatedAt
            }).ToList();

            var vm = new UsersListViewModel
            {
                SearchQuery = search,
                RoleFilter = roleFilter,
                Users = PaginatedList<UserRowViewModel>.Create(rows, page, PageSize)
            };

            return View(vm);
        }
        // ─────────────────────────────
        // POST: /Admin/ToggleActive
        // ─────────────────────────────
        [HttpPost]
        public IActionResult ToggleActive(int userId, int isActive)
        {
            _adminService.SetUserActive(userId, isActive == 1);
            return RedirectToAction("Users");
        }

        // ─────────────────────────────
        // POST: /Admin/VerifyWorker
        // ─────────────────────────────
        [HttpPost]
        public IActionResult VerifyWorker(int workerId)
        {
            _adminService.VerifyWorker(workerId);
            return RedirectToAction("Users");
        }

        // ─────────────────────────────
        // GET: /Admin/WorkerProfile/5
        // ─────────────────────────────
        [HttpGet]
        [HttpGet]
        public IActionResult WorkerProfile(int id)
        {
            var worker = _workerService.GetWorkerProfile(id);
            if (worker == null)
                return NotFound();

            int totalReviewsCount = worker.Reviews?.Count ?? 0;

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
                WorkingRegions = worker.WorksAt.Select(x => x.Region.RegionName).ToList(),
                IsActive = worker.User.IsActive,
                CompletedJobsCount = worker.CompletedJobsCount,

                AvgAccuracyRating = totalReviewsCount > 0
                    ? Math.Round(worker.Reviews.Average(r => (decimal)r.AccuracyRating), 1)
                    : 0,

                AvgCommitmentRating = totalReviewsCount > 0
                    ? Math.Round(worker.Reviews.Average(r => (decimal)r.CommitmentRating), 1)
                    : 0,

                AvgPriceRating = totalReviewsCount > 0
                    ? Math.Round(worker.Reviews.Average(r => (decimal)r.PriceRating), 1)
                    : 0,

                PortfolioItems = worker.PortfolioItems.Select(p => new PortfolioItemViewModel
                {
                    ItemId = p.ItemId,
                    Title = p.Title ?? "",
                    Description = p.Description,
                    ImageUrl = p.ImageUrl
                }).ToList(),

                Reviews = worker.Reviews.Select(r => new ReviewDisplayViewModel
                {
                    CustomerName = r.Customer.User.FullName,
                    AccuracyRating = r.AccuracyRating,
                    CommitmentRating = r.CommitmentRating,
                    PriceRating = r.PriceRating,
                    AvgRating = Math.Round((r.AccuracyRating + r.CommitmentRating + r.PriceRating) / 3m, 1),
                    SuggestWorker = r.SuggestWorker,
                    Comment = r.Comment
                }).ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Admin/ApproveVerification
        // ─────────────────────────────
        [HttpPost]
        public IActionResult ApproveVerification(int workerId)
        {
            var verification = _context.WorkerVerifications
                .FirstOrDefault(v => v.WorkerId == workerId);

            if (verification != null)
            {
                verification.Status = "Approved";
                verification.ReviewedAt = DateTime.Now;

                var worker = _context.Workers.Find(workerId);
                if (worker != null) worker.IsVerified = true;

                _context.SaveChanges();
            }

            return RedirectToAction("WorkerProfile", new { id = workerId });
        }

        // ─────────────────────────────
        // POST: /Admin/RejectVerification
        // ─────────────────────────────
        [HttpPost]
        public IActionResult RejectVerification(int workerId)
        {
            var verification = _context.WorkerVerifications
                .FirstOrDefault(v => v.WorkerId == workerId);

            if (verification != null)
            {
                verification.Status = "Rejected";
                verification.ReviewedAt = DateTime.Now;
                _context.SaveChanges();
            }

            return RedirectToAction("WorkerProfile", new { id = workerId });
        }

        // ─────────────────────────────
        // GET: /Admin/Requests
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Requests(int page = 1)
        {
            var requests = _adminService.GetAllRequests();

            var rows = requests.Select(r => new RequestRowViewModel
            {
                RequestId = r.RequestId,
                Title = r.Title,
                CustomerName = r.CustomerName,
                RegionName = r.RegionName,
                Status = r.Status,
                RequestType = r.RequestType,
                CreatedAt = r.CreatedAt
            }).ToList();

            var vm = new RequestsListViewModel
            {
                Requests = PaginatedList<RequestRowViewModel>.Create(rows, page, PageSize)
            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Admin/DeleteRequest
        // ─────────────────────────────
        [HttpPost]
        public IActionResult DeleteRequest(int requestId)
        {
            _adminService.DeleteRequest(requestId);
            return RedirectToAction("Requests");
        }

        // ─────────────────────────────
        // GET: /Admin/Proposals
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Proposals(int page = 1)
        {
            var proposals = _adminService.GetAllProposals();

            var rows = proposals.Select(p => new ProposalRowViewModel
            {
                ProposalId = p.ProposalId,
                WorkerName = p.WorkerName,
                CustomerName = p.CustomerName,
                RequestTitle = p.RequestTitle,
                LaborCost = p.LaborCost,
                MaterialCost = p.MaterialCost,
                DurationEstimate = p.DurationEstimate,
                Status = p.Status
            }).ToList();

            var vm = new ProposalsListViewModel
            {
                Proposals = PaginatedList<ProposalRowViewModel>.Create(rows, page, PageSize)
            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Admin/DeleteProposal
        // ─────────────────────────────
        [HttpPost]
        public IActionResult DeleteProposal(int proposalId)
        {
            _adminService.DeleteProposal(proposalId);
            return RedirectToAction("Proposals");
        }


        // ─────────────────────────────
        // GET: /Admin/Jobs
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Jobs(int page = 1)
        {
            var jobsData = _adminService.GetAllJobs();

            var rows = jobsData.Select(j => new AdminJobRowViewModel
            {
                JobId = j.JobId,
                RequestTitle = j.Proposal.Request.Title,
                WorkerName = j.Proposal.Worker.User.FullName,
                CustomerName = j.Proposal.Request.Customer.User.FullName,
                Status = j.Status.ToString(),
                TotalInvoice = j.LiveInvoiceTotal ?? 0,
                CreatedAt = j.CreatedAt,

                // داتا الـ Request
                RequestId = j.Proposal.Request.RequestId,
                RequestType = j.Proposal.Request.RequestType.ToString(),
                RegionName = j.Proposal.Request.Region?.RegionName ?? "—",

                // داتا الـ Proposal
                ProposalId = j.ProposalId,
                LaborCost = j.LaborCost,
                MaterialCost = j.Proposal.MaterialCost,
                DurationEstimate = j.Proposal.DurationEstimate,

                // داتا الفاتورة والتواصل
                CustomerAddress = j.CustomerExactAddress ?? "—",
                CustomerPhone = j.CustomerContactNumber ?? "—",
                InvoiceItems = j.InvoiceItems.Select(i => new AdminInvoiceItemViewModel
                {
                    Description = i.Description,
                    Cost = i.Cost
                }).ToList()
            }).ToList();

            var vm = new AdminJobsListViewModel
            {
                Jobs = PaginatedList<AdminJobRowViewModel>.Create(rows, page, PageSize)
            };

            return View(vm);
        }


    }
}