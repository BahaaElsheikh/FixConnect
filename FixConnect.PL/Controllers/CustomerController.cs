using FixConnect.BLL.Services;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using FixConnect.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixConnect.PL.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly RequestService _requestService;
        private readonly WorkerService _workerService;
        private readonly IWebHostEnvironment _env;
        private readonly ProposalService _proposalService;
        private readonly JobService _jobService;
        private readonly WalletService _walletService;
        private readonly ReviewService _reviewService;



        public CustomerController(RequestService requestService,
            WorkerService workerService,
            ProposalService proposalService,
             JobService jobService,
             WalletService walletService,
              ReviewService reviewService,
            IWebHostEnvironment env)
        {
            _requestService = requestService;
            _workerService = workerService;
            _proposalService = proposalService;
            _jobService = jobService;
            _walletService = walletService;
            _reviewService = reviewService;
            _env = env;
        }

        private int GetCurrentUserId()
            => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ─────────────────────────────
        // GET: /Customer/Index (Find Worker)
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Index(string? search, int? specialtyId, string? regionSearch)
        {
            int customerId = GetCurrentUserId();
            var workers = _requestService.GetFilteredWorkers(search, specialtyId, regionSearch);
            var jobs = _jobService.GetCustomerJobs(customerId);
            var requests = _requestService.GetCustomerRequests(customerId);

            var latestJob = jobs.FirstOrDefault(j =>
                j.Status == JobStatus.Active || j.Status == JobStatus.Disputed);

            var pendingRequests = requests
                .Where(r => r.Status == (int)RequestStatus.Pending)
                .ToList();

            var vm = new CustomerHomeViewModel
            {
                SearchQuery = search,
                SelectedSpecialtyId = specialtyId,
                RegionSearch = regionSearch,
                Specialties = _workerService.GetAllSpecialties()
                    .Select(s => new SpecialtyOption
                    {
                        SpecialtyId = s.SpecialtyId,
                        SpecialtyName = s.SpecialtyName
                    }).ToList(),
                Regions = _workerService.GetAllRegions()
                    .Select(r => new RegionOption
                    {
                        RegionId = r.RegionId,
                        RegionName = r.RegionName
                    }).ToList(),
                Workers = workers.Select(w => new WorkerCardViewModel
                {
                    UserId = w.UserId,
                    FullName = w.User.FullName,
                    PhotoUrl = w.PhotoUrl,
                    SpecialtyName = w.Specialty?.SpecialtyName,
                    AvgRating = w.AvgRating,
                    IsVerified = w.IsVerified,
                    AvailabilityStatus = w.AvailabilityStatus.ToString(),
                    Regions = w.WorksAt.Select(wa => wa.Region.RegionName).ToList()
                }).ToList(),
                LatestActiveJob = latestJob == null ? null : new CustomerJobRowViewModel
                {
                    JobId = latestJob.JobId,
                    RequestTitle = latestJob.Proposal.Request.Title,
                    WorkerName = latestJob.Proposal.Worker.User.FullName,
                    WorkerPhoto = latestJob.Proposal.Worker.PhotoUrl,
                    LiveInvoiceTotal = latestJob.LiveInvoiceTotal ?? 0,
                    Status = ((JobStatus)latestJob.Status).ToString()
                },
                PendingRequests = pendingRequests.Select(r => new MyRequestRowViewModel
                {
                    RequestId = r.RequestId,
                    Title = r.Title,
                    RegionName = r.Region.RegionName,
                    SpecialtyName = r.Specialty?.SpecialtyName,
                    CreatedAt = r.CreatedAt,
                    ProposalCount = r.Proposals.Count
                }).ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────
        // GET: /Customer/MyRequests
        // ─────────────────────────────
        [HttpGet]
        public IActionResult MyRequests()
        {
            var requests = _requestService.GetCustomerRequests(GetCurrentUserId());

            var vm = new MyRequestsViewModel
            {
                Requests = requests.Select(r => new MyRequestRowViewModel
                {
                    RequestId = r.RequestId,
                    Title = r.Title,
                    Description = r.Description,
                    Status = ((RequestStatus)r.Status).ToString(),
                    RequestType = ((RequestType)r.RequestType).ToString(),
                    RegionName = r.Region.RegionName,
                    SpecialtyName = r.Specialty?.SpecialtyName,
                    CreatedAt = r.CreatedAt,
                    ProposalCount = r.Proposals.Count,
                    ImagePaths = r.Images.Select(i => i.ImagePath).ToList()
                }).ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────
        // GET: /Customer/CreateRequest
        // ─────────────────────────────
        [HttpGet]
        public IActionResult CreateRequest(int? workerId)
        {
            Worker? targetWorker = null;
            if (workerId.HasValue)
                targetWorker = _workerService.GetWorkerProfile(workerId.Value);

            var vm = new CreateRequestViewModel
            {
                TargetWorkerId = workerId,
                TargetWorkerName = targetWorker?.User?.FullName,
                IsPrivate = workerId.HasValue,
                Specialties = _workerService.GetAllSpecialties()
                    .Select(s => new SpecialtyOption
                    {
                        SpecialtyId = s.SpecialtyId,
                        SpecialtyName = s.SpecialtyName
                    }).ToList(),
                Regions = _workerService.GetAllRegions()
                    .Select(r => new RegionOption
                    {
                        RegionId = r.RegionId,
                        RegionName = r.RegionName
                    }).ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Customer/CreateRequest
        // ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> CreateRequest(CreateRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Specialties = _workerService.GetAllSpecialties()
                    .Select(s => new SpecialtyOption
                    {
                        SpecialtyId = s.SpecialtyId,
                        SpecialtyName = s.SpecialtyName
                    }).ToList();
                model.Regions = _workerService.GetAllRegions()
                    .Select(r => new RegionOption
                    {
                        RegionId = r.RegionId,
                        RegionName = r.RegionName
                    }).ToList();
                return View(model);
            }

            var imagePaths = new List<string>();
            if (model.Images != null)
            {
                foreach (var file in model.Images)
                {
                    if (file.Length > 0)
                        imagePaths.Add(await SaveFile(file, "RequestImages"));
                }
            }

            _requestService.CreateRequest(
                GetCurrentUserId(), model.Title, model.Description,
                model.RegionId, model.SpecialtyId, model.TargetWorkerId,
                model.IsPrivate, imagePaths);

            TempData["Success"] = "Request created successfully.";
            return RedirectToAction("MyRequests");
        }

        // ─────────────────────────────
        // GET: /Customer/EditRequest/5
        // ─────────────────────────────
        [HttpGet]
        public IActionResult EditRequest(int id)
        {
            var request = _requestService.GetRequest(id);
            if (request == null || request.UserId != GetCurrentUserId())
                return NotFound();

            var vm = new EditRequestViewModel
            {
                RequestId = request.RequestId,
                Title = request.Title,
                Description = request.Description,
                RegionId = request.RegionId,
                SpecialtyId = request.SpecialtyId,
                ExistingImages = request.Images.Select(i => new ExistingImageItem
                {
                    ImageId = i.ImageId,
                    ImagePath = i.ImagePath
                }).ToList(),
                Specialties = _workerService.GetAllSpecialties()
                    .Select(s => new SpecialtyOption
                    {
                        SpecialtyId = s.SpecialtyId,
                        SpecialtyName = s.SpecialtyName
                    }).ToList(),

             

                Regions = _workerService.GetAllRegions()
                    .Select(r => new RegionOption
                    {
                        RegionId = r.RegionId,
                        RegionName = r.RegionName
                    }).ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Customer/EditRequest
        // ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> EditRequest(EditRequestViewModel model)
        {
            var newPaths = new List<string>();
            if (model.NewImages != null)
            {
                foreach (var file in model.NewImages)
                {
                    if (file.Length > 0)
                        newPaths.Add(await SaveFile(file, "RequestImages"));
                }
            }

            _requestService.UpdateRequest(
                model.RequestId, model.Title, model.Description,
                model.RegionId, model.SpecialtyId,
                newPaths, model.DeleteImageIds);

            return RedirectToAction("MyRequests");
        }

        // ─────────────────────────────
        // GET: /Customer/ReceivedProposals
        // ─────────────────────────────
        [HttpGet]
        public IActionResult ReceivedProposals()
        {
            var proposals = _requestService.GetReceivedProposals(GetCurrentUserId());

            var vm = new ReceivedProposalsViewModel
            {
                Proposals = proposals.Select(p => new ProposalDetailViewModel
                {
                    ProposalId = p.ProposalId,
                    LaborCost = p.LaborCost,
                    MaterialCost = p.MaterialCost,
                    DurationEstimate = p.DurationEstimate,
                    Status = p.Status.ToString(),
                    RequestId = p.RequestId,
                    RequestTitle = p.Request.Title,
                    WorkerId = p.WorkerId,
                    WorkerName = p.Worker.User.FullName,
                    WorkerPhoto = p.Worker.PhotoUrl,
                    WorkerRating = p.Worker.AvgRating,
                    WorkerIsVerified = p.Worker.IsVerified,

                    Notes = p.Notes, //
                    EstimatedStartTime = p.EstimatedStartTime, //

                    WorkerSpecialty = p.Worker.Specialty?.SpecialtyName,
                    Portfolio = p.Worker.PortfolioItems.Select(pi => new PortfolioItemViewModel
                    {
                        ItemId = pi.ItemId,
                        Title = pi.Title ?? "",
                        Description = pi.Description,
                        ImageUrl = pi.ImageUrl
                    }).ToList(),
                    Reviews = p.Worker.Reviews.Select(r => new ReviewItemViewModel
                    {
                        CustomerName = r.Customer.User.FullName,
                        RatingValue = r.RatingValue,
                        Comment = r.Comment
                    }).ToList()
                

                }).ToList(),

            };

            return View(vm);
        }

        // ─────────────────────────────
        // Worker Profile View (Customer POV)
        // ─────────────────────────────
        [HttpGet]
        public IActionResult WorkerProfile(int id)
        {
            var vm = _workerService.GetPublicProfile(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            var path = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(path);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(path, fileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{folder}/{fileName}";
        }


        // ─────────────────────────────
        // GET: /Customer/AcceptProposal/5
        // ─────────────────────────────
        [HttpGet]
        public IActionResult AcceptProposal(int proposalId)
        {
            var proposal = _proposalService.GetProposalById(proposalId);
            if (proposal == null) return NotFound();

            var vm = new AcceptProposalViewModel
            {
                ProposalId = proposalId,
                WorkerName = proposal.Worker.User.FullName,
                RequestTitle = proposal.Request.Title,
                LaborCost = proposal.LaborCost,
                MaterialCost = proposal.MaterialCost,
                DurationEstimate = proposal.DurationEstimate,

            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Customer/AcceptProposal
        // ─────────────────────────────
        [HttpPost]
        public IActionResult AcceptProposal(AcceptProposalViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message, jobId) = _proposalService.AcceptProposal(
                model.ProposalId, GetCurrentUserId(),
                model.CustomerExactAddress, model.CustomerContactNumber);

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction("ReceivedProposals");
            }

            TempData["Success"] = "Proposal accepted! Job has been created.";
            return RedirectToAction("Jobs");
        }

        // ─────────────────────────────
        // GET: /Customer/Jobs
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Jobs()
        {
            var jobs = _jobService.GetCustomerJobs(GetCurrentUserId());

            var vm = new CustomerJobsViewModel
            {
                Jobs = jobs.Select(j => new CustomerJobRowViewModel
                {
                    JobId = j.JobId,
                    RequestTitle = j.Proposal.Request.Title,
                    WorkerName = j.Proposal.Worker.User.FullName,
                    WorkerPhoto = j.Proposal.Worker.PhotoUrl,
                    LiveInvoiceTotal = j.LiveInvoiceTotal ?? 0,
                    Status = ((JobStatus)j.Status).ToString(),
                    EstimatedStartTime = j.EstimatedStartTime,
                    ActualStartDate = j.ActualStartDate,
                    LaborCost = j.LaborCost,
                    WorkerMarkedFinished = j.Status == JobStatus.Disputed
                                        && j.ActualStartDate.HasValue
                }).ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Customer/ConfirmCompletion
        // ─────────────────────────────
        [HttpPost]
        public IActionResult ConfirmCompletion(int jobId)
        {
            var (success, message) = _jobService.ConfirmCompletion(
                jobId, GetCurrentUserId(), _walletService);

            if (success)
                return RedirectToAction("SubmitReview", new { jobId });

            TempData["Error"] = message;
            return RedirectToAction("Jobs");
        }



        // GET: /Customer/JobInvoice?jobId=5
        [HttpGet]
        public IActionResult JobInvoice(int jobId)
        {
            var job = _jobService.GetJob(jobId);
            if (job == null || job.Proposal.UserId != GetCurrentUserId())
                return Content("<p class='text-danger'>Job not found.</p>", "text/html");

            var invoiceTotal = job.InvoiceItems.Sum(i => i.Cost);
            var laborCost = job.LaborCost ?? 0;
            var grandTotal = laborCost + invoiceTotal;

            var html = new System.Text.StringBuilder();

            html.Append($@"
    <table class='table table-sm'>
        <thead class='table-light'>
            <tr>
                <th>Description</th>
                <th class='text-end'>Cost (EGP)</th>
                <th class='text-end'>Date</th>
            </tr>
        </thead>
        <tbody>
            <tr class='table-warning'>
                <td class='fw-semibold'>🔧 Service Cost</td>
                <td class='text-end fw-semibold'>{laborCost:0.00}</td>
                <td></td>
            </tr>");

            foreach (var item in job.InvoiceItems.OrderBy(i => i.AddedAt))
            {
                html.Append($@"
        <tr>
            <td class='small'>{item.Description}</td>
            <td class='text-end'>{item.Cost:0.00}</td>
            <td class='text-end text-muted' style='font-size:0.72rem;white-space:nowrap'>
                {item.AddedAt:dd MMM yyyy}
            </td>
        </tr>");
            }
            html.Append($@"
            </tbody>
            <tfoot>
                <tr class='border-top'>
                    <td class='small text-muted'>Materials Subtotal</td>
                    <td class='text-end small text-muted'>{invoiceTotal:0.00}</td>
                </tr>
                <tr class='table-dark'>
                    <td class='fw-bold'>Total</td>
                    <td class='text-end fw-bold'>{grandTotal:0.00} EGP</td>
                </tr>
            </tfoot>
        </table>");

            return Content(html.ToString(), "text/html");
        }

        [HttpPost]
        public IActionResult DeleteRequest(int requestId)
        {
            var request = _requestService.GetRequest(requestId);
            if (request == null || request.UserId != GetCurrentUserId())
                return NotFound();

            _requestService.DeleteRequest(requestId);
            TempData["Success"] = "Request deleted successfully.";
            return RedirectToAction("MyRequests");
        }

        [HttpGet]
        public IActionResult RequestProposals(int requestId)
        {
            var request = _requestService.GetRequest(requestId);
            if (request == null || request.UserId != GetCurrentUserId())
                return NotFound();

            var proposals = _requestService.GetProposalsByRequest(requestId);

            var vm = new ReceivedProposalsViewModel
            {
                Proposals = proposals.Select(p => new ProposalDetailViewModel
                {
                    ProposalId = p.ProposalId,
                    LaborCost = p.LaborCost,
                    MaterialCost = p.MaterialCost,
                    DurationEstimate = p.DurationEstimate,
                    Status = p.Status.ToString(),
                    Notes = p.Notes,
                    EstimatedStartTime = p.EstimatedStartTime,
                    RequestId = p.RequestId,
                    RequestTitle = p.Request.Title,
                    WorkerId = p.WorkerId,
                    WorkerName = p.Worker.User.FullName,
                    WorkerPhoto = p.Worker.PhotoUrl,
                    WorkerRating = p.Worker.AvgRating,
                    WorkerIsVerified = p.Worker.IsVerified,
                    WorkerSpecialty = p.Worker.Specialty?.SpecialtyName,
                    Portfolio = p.Worker.PortfolioItems.Select(pi => new PortfolioItemViewModel
                    {
                        ItemId = pi.ItemId,
                        Title = pi.Title ?? "",
                        Description = pi.Description,
                        ImageUrl = pi.ImageUrl
                    }).ToList(),
                    Reviews = p.Worker.Reviews.Select(r => new ReviewItemViewModel
                    {
                        CustomerName = r.Customer.User.FullName,
                        RatingValue = r.RatingValue,
                        Comment = r.Comment
                    }).ToList()
                }).ToList()
            };

            ViewBag.RequestTitle = request.Title;
            return View(vm);
        }



        [HttpGet]
        public IActionResult SubmitReview(int jobId)
        {
            var job = _jobService.GetJob(jobId);
            if (job == null || job.Proposal.UserId != GetCurrentUserId())
                return NotFound();

            if (_reviewService.JobHasReview(jobId))
            {
                TempData["Error"] = "You already reviewed this job.";
                return RedirectToAction("Jobs");
            }

            var vm = new SubmitReviewViewModel
            {
                JobId = jobId,
                WorkerId = job.Proposal.WorkerId,
                WorkerName = job.Proposal.Worker.User.FullName,
                WorkerPhoto = job.Proposal.Worker.PhotoUrl,
                RequestTitle = job.Proposal.Request.Title
            };

            return View(vm);
        }

        // ─────────────────────────────
        // POST: /Customer/SubmitReview
        // ─────────────────────────────
        [HttpPost]
        public IActionResult SubmitReview(SubmitReviewViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = _reviewService.SubmitReview(
                model.JobId, GetCurrentUserId(),
                model.AccuracyRating, model.CommitmentRating, model.PriceRating,
                model.SuggestWorker, model.Comment);

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Jobs");
        }


    }
}
//[HttpGet]
//public IActionResult WorkerProfile(int id)
//{
//    var vm = _workerService.GetPublicProfile(id);
//    if (vm == null) return NotFound();
//    return View(vm);
//}

