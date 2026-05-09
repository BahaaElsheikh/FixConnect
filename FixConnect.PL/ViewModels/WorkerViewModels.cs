using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class WorkerDashboardViewModel
    {
        public List<RequestFeedItemViewModel> PublicRequests { get; set; } = new();
        public List<SpecialtyOption> Specialties { get; set; } = new();
        public List<RegionOption> Regions { get; set; } = new();
    }

    public class RequestFeedItemViewModel
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string CustomerName { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string? SpecialtyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ImageCount { get; set; }
        public bool AlreadyBid { get; set; }
        public List<string> ImagePaths { get; set; } = new();
    }

    // ─────────────────────────────
    // Request Detail (Worker View)
    // ─────────────────────────────
    public class RequestDetailViewModel
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string CustomerName { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string? SpecialtyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ImagePaths { get; set; } = new();
        public bool AlreadyBid { get; set; }
        public ExistingProposalViewModel? ExistingProposal { get; set; }
    }

    public class ExistingProposalViewModel
    {
        public int ProposalId { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public int? DurationEstimate { get; set; }
        public string Status { get; set; } = null!;
    }

    // ─────────────────────────────
    // Submit / Edit Proposal
    // ─────────────────────────────
    public class SubmitProposalViewModel
    {
        public int RequestId { get; set; }
        public string RequestTitle { get; set; } = null!;
        public int? ProposalId { get; set; }   // null = new, has value = edit

        [Required]
        public decimal LaborCost { get; set; }

        //[Required]
        public decimal MaterialCost { get; set; }

        public int DurationEstimate { get; set; }

        [Required]
        public string? Notes { get; set; }
    }

    // ─────────────────────────────
    // My Proposals (Worker)
    // ─────────────────────────────
    public class MyProposalsViewModel
    {
        public List<WorkerProposalRowViewModel> Proposals { get; set; } = new();
    }

    public class WorkerProposalRowViewModel
    {
        public int ProposalId { get; set; }
        public int RequestId { get; set; }
        public string RequestTitle { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public int? DurationEstimate { get; set; }
        public string Status { get; set; } = null!;
    }
}

