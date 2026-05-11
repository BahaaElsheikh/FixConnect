namespace FixConnect.PL.ViewModels
{
    public class WorkerProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Bio { get; set; }
        public string? SpecialtyName { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsVerified { get; set; }
        public string AvailabilityStatus { get; set; } = null!;
        public decimal AvgRating { get; set; }
        public List<string> WorkingRegions { get; set; } = new();
        public List<PortfolioItemViewModel> PortfolioItems { get; set; } = new();
        public List<ReviewDisplayViewModel> Reviews { get; set; } = new();
        public bool HasPendingVerification { get; set; }

        public int CompletedJobsCount { get; set; }


    }

    public class ReviewItemViewModel
    {
        public string CustomerName { get; set; } = null!;
        public int RatingValue { get; set; }
        public string? Comment { get; set; }
    }
}