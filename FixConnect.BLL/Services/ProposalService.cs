using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;
using static Azure.Core.HttpHeader;

namespace FixConnect.BLL.Services
{
    public class ProposalService
    {
        private readonly AppDbContext _context;

        public ProposalService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Submit Proposal
        // ─────────────────────────────
        public (bool Success, string Message) SubmitProposal(
            int workerId, int requestId,
            decimal laborCost, decimal materialCost, int duration,string notes, DateTime estimatedStartTime)
        {
            // Check duplicate
            var exists = _context.Proposals.Any(p =>
                p.WorkerId == workerId && p.RequestId == requestId);
            if (exists)
                return (false, "You already submitted a proposal for this request.");

            // Get CustomerId from Request
            var request = _context.Requests.Find(requestId);
            if (request == null) return (false, "Request not found.");

            _context.Proposals.Add(new Proposal
            {
                WorkerId = workerId,
                RequestId = requestId,
                UserId = request.UserId,
                LaborCost = laborCost,
                MaterialCost = materialCost,
                DurationEstimate = duration,
                Notes = notes,
                EstimatedStartTime = estimatedStartTime,  // ← أضف
                Status = ProposalStatus.Pending , 
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();
            return (true, "Proposal submitted successfully.");
        }

        // ─────────────────────────────
        // Edit Proposal
        // ─────────────────────────────
        public (bool Success, string Message) EditProposal(
            int proposalId, int workerId,
            decimal laborCost, decimal materialCost, int duration , string notes, DateTime estimatedStartTime)
        {
            var proposal = _context.Proposals
                .FirstOrDefault(p => p.ProposalId == proposalId
                                  && p.WorkerId == workerId);

            if (proposal == null) return (false, "Proposal not found.");

            if (proposal.Status != ProposalStatus.Pending)
                return (false, "Cannot edit a proposal that has already been reviewed.");

            proposal.LaborCost = laborCost;
            proposal.MaterialCost = materialCost;
            proposal.DurationEstimate = duration;
            proposal.Notes = notes;
            proposal.EstimatedStartTime = estimatedStartTime;
            proposal.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return (true, "Proposal updated.");
        }

        // ─────────────────────────────
        // Get Worker Proposals
        // ─────────────────────────────
        public List<Proposal> GetWorkerProposals(int workerId)
        {
            return _context.Proposals
                .Include(p => p.Request)
                .Include(p => p.Customer).ThenInclude(c => c.User)
                .Where(p => p.WorkerId == workerId)
                .OrderByDescending(p => p.ProposalId)
                .ToList();
        }

        // ─────────────────────────────
        // Get Existing Proposal
        // ─────────────────────────────
        public Proposal? GetWorkerProposalForRequest(int workerId, int requestId)
        {
            return _context.Proposals
                .FirstOrDefault(p => p.WorkerId == workerId
                                  && p.RequestId == requestId);
        }


        public (bool Success, string Message, int JobId) AcceptProposal(
    int proposalId, int customerId,
    string exactAddress, string contactNumber)
        {
            var proposal = _context.Proposals
                .Include(p => p.Request)
                .FirstOrDefault(p => p.ProposalId == proposalId
                                  && p.UserId == customerId);

            if (proposal == null)
                return (false, "Proposal not found.", 0);

            if (proposal.Status != ProposalStatus.Pending)
                return (false, "Proposal is no longer pending.", 0);

            // Accept this proposal
            proposal.Status = ProposalStatus.Accepted;

            proposal.UpdatedAt = DateTime.Now;

            // Auto-reject all other proposals on the same request
            var otherProposals = _context.Proposals
                .Where(p => p.RequestId == proposal.RequestId
                         && p.ProposalId != proposalId
                         && p.Status == ProposalStatus.Pending)
                .ToList();

            foreach (var p in otherProposals)
            {
                p.Status = ProposalStatus.AutoRejected;
                p.UpdatedAt = DateTime.Now;
            }

            // Update Request Status to InProgress
            var request = proposal.Request;
            request.Status = (int)RequestStatus.InProgress;

            // Create Job
            var job = new Job
            {
                ProposalId = proposalId,
                Status = JobStatus.Active,
                LiveInvoiceTotal = 0,
                CustomerExactAddress = exactAddress,
                CustomerContactNumber = contactNumber,
                EstimatedStartTime = proposal.EstimatedStartTime,  // 
                ActualStartDate = null,
                CreatedAt = DateTime.Now,
            };

            _context.Jobs.Add(job);
            _context.SaveChanges();

            return (true, "Proposal accepted. Job created.", job.JobId);
        }

        public Proposal? GetProposalById(int proposalId)
        {
            return _context.Proposals
                .Include(p => p.Worker).ThenInclude(w => w.User)
                .Include(p => p.Request)
                .Include(p => p.Customer).ThenInclude(c => c.User)
                .FirstOrDefault(p => p.ProposalId == proposalId);
        }
    }
}