namespace FixConnect.DAL.Models
{
    public class WorkerVerification
    {
        public int VerificationId { get; set; }
        public int WorkerId { get; set; }
        public string IdFrontImagePath { get; set; } = null!;
        public string IdBackImagePath { get; set; } = null!;
        public string Status { get; set; } = "Pending";   // Pending / Approved / Rejected
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }

        // Navigation
        public Worker Worker { get; set; } = null!;
    }
}