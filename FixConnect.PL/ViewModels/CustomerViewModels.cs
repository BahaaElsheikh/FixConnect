using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{


    // ─────────────────────────────
    // Worker Card (Home/Discovery)
    // ─────────────────────────────
    public class WorkerCardViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string? SpecialtyName { get; set; }
        public decimal AvgRating { get; set; }
        public bool IsVerified { get; set; }
        public string AvailabilityStatus { get; set; } = null!;
        public List<string> Regions { get; set; } = new();
    }

    // ─────────────────────────────
    // Customer Home (Find Worker)
    // ─────────────────────────────
    public class CustomerHomeViewModel
    {
        public List<WorkerCardViewModel> Workers { get; set; } = new();
        public List<SpecialtyOption> Specialties { get; set; } = new();
        public List<RegionOption> Regions { get; set; } = new();
        public int? SelectedSpecialtyId { get; set; }
        public string? RegionSearch { get; set; }
        public string? SearchQuery { get; set; }

        public CustomerJobRowViewModel? LatestActiveJob { get; set; }
        public List<MyRequestRowViewModel> PendingRequests { get; set; } = new();
    }

    // ─────────────────────────────
    // Create Request
    // ─────────────────────────────
    public class CreateRequestViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = null!;

        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        public int RegionId { get; set; }

        public int? SpecialtyId { get; set; }

        // Scenario B — from Worker Profile
        public int? TargetWorkerId { get; set; }
        public bool IsPrivate { get; set; } = false;

        // Images
        public List<IFormFile>? Images { get; set; }

        // Dropdowns
        public List<SpecialtyOption> Specialties { get; set; } = new();
        public List<RegionOption> Regions { get; set; } = new();
        public string? TargetWorkerName { get; set; }
    }

    // ─────────────────────────────
    // My Requests (Customer)
    // ─────────────────────────────
    public class MyRequestsViewModel
    {
        public List<MyRequestRowViewModel> Requests { get; set; } = new();
    }

    public class MyRequestRowViewModel
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public string RequestType { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string? SpecialtyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProposalCount { get; set; }
        public List<string> ImagePaths { get; set; } = new();
    }

    // ─────────────────────────────
    // Edit Request
    // ─────────────────────────────
    
    // ─────────────────────────────
    // Received Proposals (Customer)
    // ─────────────────────────────
    public class ReceivedProposalsViewModel
    {
        public List<ProposalDetailViewModel> Proposals { get; set; } = new();
    }

    public class ProposalDetailViewModel
    {
        // Proposal Info
        public int ProposalId { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public int? DurationEstimate { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime? EstimatedStartTime { get; set; }


        // Request Info
        public int RequestId { get; set; }
        public string RequestTitle { get; set; } = null!;

        // Worker Info
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = null!;
        public string? WorkerPhoto { get; set; }
        public decimal WorkerRating { get; set; }
        public bool WorkerIsVerified { get; set; }
        public string? WorkerSpecialty { get; set; }
        public List<PortfolioItemViewModel> Portfolio { get; set; } = new();
        public List<ReviewItemViewModel> Reviews { get; set; } = new();
    }
}
