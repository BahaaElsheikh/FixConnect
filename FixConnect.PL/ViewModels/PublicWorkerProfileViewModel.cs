namespace FixConnect.PL.ViewModels
{
    public class PublicWorkerProfileViewModel
    {
        // Basic Info
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Bio { get; set; }
        public string? Specialty { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsVerified { get; set; }
        public string AvailabilityStatus { get; set; } = null!;
        public decimal AvgRating { get; set; }

        // أضف هذه الحقول داخل كلاس PublicWorkerProfileViewModel تحت الـ AvgRating القديم
        public decimal AvgAccuracyRating { get; set; }   // متوسط الدقة
        public decimal AvgCommitmentRating { get; set; } // متوسط الالتزام
        public decimal AvgPriceRating { get; set; }      // متوسط السعر

        public List<string> WorkingRegions { get; set; } = new();

        // Portfolio
        public List<PortfolioItemViewModel> PortfolioItems { get; set; } = new();

        // Reviews
        public List<ReviewItemViewModel> Reviews { get; set; } = new();

        // Verification (Admin Only)
        public VerificationViewModel? Verification { get; set; }
    }

    public class VerificationViewModel
    {
        public string IdFrontImagePath { get; set; } = null!;
        public string IdBackImagePath { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public int WorkerId { get; set; }
    }
}