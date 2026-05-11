using FixConnect.DAL.Context;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FixConnect.BLL.Services
{
    public class ReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Submit Review
        // ─────────────────────────────
        public (bool Success, string Message) SubmitReview(
            int jobId, int customerId,
            int accuracyRating, int commitmentRating, int priceRating,
            bool suggestWorker, string? comment)
        {
            var job = _context.Jobs
                .Include(j => j.Proposal)
                .FirstOrDefault(j => j.JobId == jobId
                                  && j.Proposal.UserId == customerId);

            if (job == null)
                return (false, "Job not found.");

            if (_context.Reviews.Any(r => r.JobId == jobId))
                return (false, "You already reviewed this job.");

            // Calculate AVG
            decimal avg = (accuracyRating + commitmentRating + priceRating) / 3m;
            avg = Math.Round(avg, 2);

            var review = new Review
            {
                JobId = jobId,
                UserId = customerId,
                WorkerId = job.Proposal.WorkerId,
                AccuracyRating = accuracyRating,
                CommitmentRating = commitmentRating,
                PriceRating = priceRating,
                RatingValue = (int)Math.Round(avg),
                SuggestWorker = suggestWorker,
                Comment = comment
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            // Update Worker AvgRating & CompletedJobsCount
            UpdateWorkerStats(job.Proposal.WorkerId);

            return (true, "Review submitted successfully.");
        }

        // ─────────────────────────────
        // Update Worker Stats
        // ─────────────────────────────
        private void UpdateWorkerStats(int workerId)
        {
            var reviews = _context.Reviews
                .Where(r => r.WorkerId == workerId)
                .ToList();

            if (!reviews.Any()) return;

            var worker = _context.Workers.Find(workerId);
            if (worker == null) return;

            // New AVG = average of all review AVGs
            decimal totalAvg = reviews.Average(r =>
                (r.AccuracyRating + r.CommitmentRating + r.PriceRating) / 3m);

            worker.AvgRating = Math.Round(totalAvg, 2);
            worker.CompletedJobsCount = reviews.Count;

            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Get Reviews for Worker
        // ─────────────────────────────
        public List<Review> GetWorkerReviews(int workerId)
        {
            return _context.Reviews
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Where(r => r.WorkerId == workerId)
                .OrderByDescending(r => r.ReviewId)
                .ToList();
        }

        // ─────────────────────────────
        // Check if Job has Review
        // ─────────────────────────────
        public bool JobHasReview(int jobId)
            => _context.Reviews.Any(r => r.JobId == jobId);
    }
}