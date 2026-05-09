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

        public CustomerController(RequestService requestService,
            WorkerService workerService, IWebHostEnvironment env)
        {
            _requestService = requestService;
            _workerService = workerService;
            _env = env;
        }

        private int GetCurrentUserId()
            => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ─────────────────────────────
        // GET: /Customer/Index (Find Worker)
        // ─────────────────────────────
        [HttpGet]
        public IActionResult Index(string? search, int? specialtyId, int? regionId)
        {
            var workers = _requestService.GetFilteredWorkers(search, specialtyId, regionId);

            var vm = new CustomerHomeViewModel
            {
                SearchQuery = search,
                SelectedSpecialtyId = specialtyId,
                SelectedRegionId = regionId,
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
                ExistingImages = request.Images.Select(i => i.ImagePath).ToList(),
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
    }
}
//[HttpGet]
//public IActionResult WorkerProfile(int id)
//{
//    var vm = _workerService.GetPublicProfile(id);
//    if (vm == null) return NotFound();
//    return View(vm);
//}
