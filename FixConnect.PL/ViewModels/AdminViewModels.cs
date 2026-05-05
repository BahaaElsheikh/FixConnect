namespace FixConnect.PL.ViewModels
{
    // ─────────────────────────────
    // Users List
    // ─────────────────────────────
    public class UsersListViewModel
    {
        public List<UserRowViewModel> Users { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? RoleFilter { get; set; }   // "Worker" | "Customer" | null
    }

    public class UserRowViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }      // Workers only
        public DateTime CreatedAt { get; set; }
    }

    // ─────────────────────────────
    // Requests List
    // ─────────────────────────────
    public class RequestsListViewModel
    {
        public List<RequestRowViewModel> Requests { get; set; } = new();
    }

    public class RequestRowViewModel
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string RequestType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    // ─────────────────────────────
    // Proposals List
    // ─────────────────────────────
    public class ProposalsListViewModel
    {
        public List<ProposalRowViewModel> Proposals { get; set; } = new();
    }

    public class ProposalRowViewModel
    {
        public int ProposalId { get; set; }
        public string WorkerName { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string RequestTitle { get; set; } = null!;
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public int? DurationEstimate { get; set; }
        public string Status { get; set; } = null!;
    }
}