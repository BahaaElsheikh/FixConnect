namespace FixConnect.BLL.DTOs
{
    public class PublicWorkerProfileViewModel
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

        public int CompletedJobsCount { get; set; }

        public List<PortfolioItemViewModel> PortfolioItems { get; set; } = new();
        public List<ReviewItemViewModel> Reviews { get; set; } = new();
        public VerificationViewModel? Verification { get; set; }
    }

    public class PortfolioItemViewModel
    {
        public int ItemId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ReviewItemViewModel
    {
        public string CustomerName { get; set; } = null!;
        public int RatingValue { get; set; }
        public string? Comment { get; set; }

        // New
        public int AccuracyRating { get; set; }
        public int CommitmentRating { get; set; }
        public int PriceRating { get; set; }
        public decimal AvgRating { get; set; }
        public bool SuggestWorker { get; set; }
    }

    public class VerificationViewModel
    {
        public int WorkerId { get; set; }
        public string IdFrontImagePath { get; set; } = null!;
        public string IdBackImagePath { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
    }
}