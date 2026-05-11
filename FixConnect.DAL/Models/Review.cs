namespace FixConnect.DAL.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        // Old field — keep for backward compat
        public int RatingValue { get; set; }

        // New — 3 rating dimensions
        public int AccuracyRating { get; set; }      // دقة وجودة
        public int CommitmentRating { get; set; }    // التزام
        public int PriceRating { get; set; }         // السعر

        public bool SuggestWorker { get; set; } = false;
        public string? Comment { get; set; }

        public int UserId { get; set; }    // Customer
        public int JobId { get; set; }
        public int WorkerId { get; set; }

        // Navigation
        public Customer Customer { get; set; } = null!;
        public Job Job { get; set; } = null!;
        public Worker Worker { get; set; } = null!;
    }
}