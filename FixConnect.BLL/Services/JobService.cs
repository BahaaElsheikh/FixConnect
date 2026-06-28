using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FixConnect.BLL.Services
{
    public class JobService
    {
        private readonly AppDbContext _context;

        public JobService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Get Job with Full Details
        // ─────────────────────────────
        public Job? GetJob(int jobId)
        {
            return _context.Jobs
                .Include(j => j.Proposal)
                    .ThenInclude(p => p.Request)
                .Include(j => j.Proposal)
                    .ThenInclude(p => p.Worker)
                        .ThenInclude(w => w.User)
                .Include(j => j.Proposal)
                    .ThenInclude(p => p.Customer)
                        .ThenInclude(c => c.User)
                .Include(j => j.InvoiceItems)
                .FirstOrDefault(j => j.JobId == jobId);
        }

        // ─────────────────────────────
        // Get Worker Jobs
        // ─────────────────────────────
        public List<Job> GetWorkerJobs(int workerId)
        {
            return _context.Jobs
                .Include(j => j.Proposal)
                    .ThenInclude(p => p.Request)
                .Include(j => j.Proposal)
                    .ThenInclude(p => p.Customer)
                        .ThenInclude(c => c.User)
                .Include(j => j.InvoiceItems)
                .Where(j => j.Proposal.WorkerId == workerId)
                .OrderByDescending(j => j.JobId)
                .ToList();
        }

        // ─────────────────────────────
        // Get Customer Jobs
        // ─────────────────────────────
        public List<Job> GetCustomerJobs(int customerId)
        {
            return _context.Jobs
                .Include(j => j.Proposal)
                    .ThenInclude(p => p.Request)
                .Include(j => j.Proposal)
                    .ThenInclude(p => p.Worker)
                        .ThenInclude(w => w.User)
                .Include(j => j.InvoiceItems)
                .Where(j => j.Proposal.UserId == customerId)
                .OrderByDescending(j => j.JobId)
                .ToList();
        }

        // ─────────────────────────────
        // Start Job (Worker)
        // ─────────────────────────────
        public (bool Success, string Message) StartJob(int jobId, int workerId)
        {
            var job = GetJob(jobId);
            if (job == null || job.Proposal.WorkerId != workerId)
                return (false, "Job not found.");

            if (job.ActualStartDate.HasValue)
                return (false, "Job already started.");

            job.ActualStartDate = DateTime.Now;
            job.StartDate = DateOnly.FromDateTime(DateTime.Now);
            job.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return (true, "Job started successfully.");
        }

        // ─────────────────────────────
        // Cancel Job (Worker — before start)
        // ─────────────────────────────
        public (bool Success, string Message) CancelJob(int jobId, int workerId)
        {
            var job = GetJob(jobId);
            if (job == null || job.Proposal.WorkerId != workerId)
                return (false, "Job not found.");

            if (job.ActualStartDate.HasValue)
                return (false, "Cannot cancel a job that has already started.");

            job.Status =JobStatus.Disputed;
            job.UpdatedAt = DateTime.Now;
            // Revert proposal & request
            job.Proposal.Status = ProposalStatus.Rejected;
            job.Proposal.Request.Status = (int)RequestStatus.Pending;
            job.Proposal.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return (true, "Job cancelled.");
        }

        // ─────────────────────────────
        // Add Invoice Item (Worker)
        // ─────────────────────────────
        public (bool Success, string Message) AddInvoiceItem(
            int jobId, int workerId, string description, decimal cost)
        {
            var job = GetJob(jobId);
            if (job == null || job.Proposal.WorkerId != workerId)
                return (false, "Job not found.");

            if (job.Status != JobStatus.Active)
                return (false, "Job is not active.");

            _context.JobInvoiceItems.Add(new JobInvoiceItem
            {
                JobId = jobId,
                Description = description,
                Cost = cost,
                AddedAt = DateTime.Now
            });

            // Update total
            job.LiveInvoiceTotal += cost;
            job.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return (true, "Invoice item added.");
        }

        // ─────────────────────────────
        // Mark As Finished (Worker)
        // ─────────────────────────────
        public (bool Success, string Message) MarkAsFinished(int jobId, int workerId)
        {
            var job = GetJob(jobId);
            if (job == null || job.Proposal.WorkerId != workerId)
                return (false, "Job not found.");

            if (!job.ActualStartDate.HasValue)
                return (false, "Job has not started yet.");

            if (job.Status == JobStatus.Completed)
                return (false, "Job already completed.");

            // Use Disputed temporarily to mean "Waiting for Customer Confirmation"
            job.Status = JobStatus.Disputed;
            job.EndDate = DateOnly.FromDateTime(DateTime.Now);
            job.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return (true, "Marked as finished. Waiting for customer confirmation.");
        }

        // ─────────────────────────────
        // Confirm Completion (Customer)
        // ─────────────────────────────
        public (bool Success, string Message) ConfirmCompletion(
    int jobId, int customerId,
    WalletService walletService)
        {
            var job = GetJob(jobId);
            if (job == null || job.Proposal.UserId != customerId)
                return (false, "Job not found.");

            job.Status = JobStatus.Completed;
            job.UpdatedAt = DateTime.Now;

            job.Proposal.Request.Status = (int)RequestStatus.Completed;

            // Update Worker CompletedJobsCount
            var worker = _context.Workers.Find(job.Proposal.WorkerId);
            if (worker != null) worker.CompletedJobsCount++;

            _context.SaveChanges();

            // Deduct 10% from Job LaborCost (not Proposal)
            if (job.LaborCost.HasValue && job.LaborCost > 0)
                walletService.DeductCommission(job.Proposal.WorkerId, job.LaborCost.Value);

            return (true, "Job confirmed as completed.");
        }




        // Edit Invoice Item
        public (bool Success, string Message) EditInvoiceItem(
            int itemId, int jobId, int workerId,
            string description, decimal cost)
        {
            var job = GetJob(jobId);
            if (job == null || job.Proposal.WorkerId != workerId)
                return (false, "Job not found.");

            var item = job.InvoiceItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null) return (false, "Item not found.");

            // Adjust total
            job.LiveInvoiceTotal = (job.LiveInvoiceTotal ?? 0) - item.Cost + cost;

            item.Description = description;
            item.Cost = cost;

            job.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return (true, "Item updated.");
        }

        // Delete Invoice Item
        public (bool Success, string Message) DeleteInvoiceItem(
            int itemId, int jobId, int workerId)
        {
            var job = GetJob(jobId);
            if (job == null || job.Proposal.WorkerId != workerId)
                return (false, "Job not found.");

            var item = job.InvoiceItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null) return (false, "Item not found.");

            job.LiveInvoiceTotal = (job.LiveInvoiceTotal ?? 0) - item.Cost;
            _context.JobInvoiceItems.Remove(item);
            job.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return (true, "Item deleted.");
        }

        public (bool Success, string Message) SetJobLaborCost(
    int jobId, int workerId, decimal laborCost)
        {
            var job = GetJob(jobId);
            if (job == null || job.Proposal.WorkerId != workerId)
                return (false, "Job not found.");

            job.LaborCost = laborCost;
            _context.SaveChanges();
            return (true, "Labor cost updated.");
        }


        public int GetJobIdByProposal(int proposalId)
        {
            return _context.Jobs.Where(j => j.ProposalId == proposalId).Select(j => j.JobId).FirstOrDefault();
        }


    }
}