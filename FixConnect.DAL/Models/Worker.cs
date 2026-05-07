using FixConnect.DAL.Data.Enums;

namespace FixConnect.DAL.Models
{
    public class Worker
    {
        public int UserId { get; set; }
        public int? SpecialtyId { get; set; }        // ← بدل string Specialty
        public string? Bio { get; set; }
        public bool IsVerified { get; set; } = false;
        public decimal AvgRating { get; set; } = 0;
        public AvailabilityStatus AvailabilityStatus { get; set; }
        public string? PhotoUrl { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Specialty? Specialty { get; set; }    // ← Navigation
        public ICollection<WorksAt> WorksAt { get; set; } = new List<WorksAt>();
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public Wallet? Wallet { get; set; }
        public ICollection<Request> TargetedRequests { get; set; } = new List<Request>();
        public WorkerVerification? Verification { get; set; }
    }
}