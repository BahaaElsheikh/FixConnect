using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    // ─────────────────────────────
    // Accept Proposal (Customer)
    // ─────────────────────────────
    public class AcceptProposalViewModel
    {
        public int ProposalId { get; set; }
        public string WorkerName { get; set; } = null!;
        public string RequestTitle { get; set; } = null!;
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public int? DurationEstimate { get; set; }

        [Required]
        public string CustomerExactAddress { get; set; } = null!;

        [Required]
        public string CustomerContactNumber { get; set; } = null!;
    }

    // ─────────────────────────────
    // Customer Jobs List
    // ─────────────────────────────
    public class CustomerJobsViewModel
    {
        public List<CustomerJobRowViewModel> Jobs { get; set; } = new();
    }

    public class CustomerJobRowViewModel
    {
        public int JobId { get; set; }
        public string RequestTitle { get; set; } = null!;
        public string WorkerName { get; set; } = null!;
        public string? WorkerPhoto { get; set; }
        public decimal LiveInvoiceTotal { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? EstimatedStartTime { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public bool WorkerMarkedFinished { get; set; }
        public decimal? LaborCost { get; set; }
    }

    // ─────────────────────────────
    // Worker Jobs List
    // ─────────────────────────────
    public class WorkerJobsViewModel
    {
        public List<WorkerJobRowViewModel> Jobs { get; set; } = new();
    }

    public class WorkerJobRowViewModel
    {
        public int JobId { get; set; }
        public string RequestTitle { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerExactAddress { get; set; } = null!;
        public string CustomerContactNumber { get; set; } = null!;
        public DateTime? EstimatedStartTime { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public decimal LiveInvoiceTotal { get; set; }
        public string Status { get; set; } = null!;
        public bool CanStart { get; set; }
        public bool CanCancel { get; set; }
        public bool CanMarkFinished { get; set; }
    }

    // ─────────────────────────────
    // Job Detail (Worker)
    // ─────────────────────────────
    public class JobDetailViewModel
    {
        public int JobId { get; set; }
        public string RequestTitle { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerExactAddress { get; set; } = null!;
        public string CustomerContactNumber { get; set; } = null!;
        public DateTime? EstimatedStartTime { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public decimal LiveInvoiceTotal { get; set; }
        public string Status { get; set; } = null!;
        public bool CanStart { get; set; }
        public bool CanCancel { get; set; }
        public bool CanMarkFinished { get; set; }

        public decimal? LaborCost { get; set; }
        public List<InvoiceItemViewModel> InvoiceItems { get; set; } = new();
    }

    public class InvoiceItemViewModel
    {
        public int ItemId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Cost { get; set; }
        public DateTime AddedAt { get; set; }
    }

    // ─────────────────────────────
    // Add Invoice Item
    // ─────────────────────────────
    public class AddInvoiceItemViewModel
    {
        public int JobId { get; set; }

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public decimal Cost { get; set; }
    }
}