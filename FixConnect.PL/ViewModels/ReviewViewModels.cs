using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class SubmitReviewViewModel
    {
        public int JobId { get; set; }
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = null!;
        public string? WorkerPhoto { get; set; }
        public string RequestTitle { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int AccuracyRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int CommitmentRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int PriceRating { get; set; }

        public bool SuggestWorker { get; set; } = false;

        [StringLength(300)]
        public string? Comment { get; set; }
    }

    // ─────────────────────────────
    // Review Display
    // ─────────────────────────────
    public class ReviewDisplayViewModel
    {
        public string CustomerName { get; set; } = null!;
        public int AccuracyRating { get; set; }
        public int CommitmentRating { get; set; }
        public int PriceRating { get; set; }
        public decimal AvgRating { get; set; }
        public bool SuggestWorker { get; set; }
        public string? Comment { get; set; }
    }
}
