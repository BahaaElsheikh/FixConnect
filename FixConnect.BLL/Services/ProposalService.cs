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
            decimal laborCost, decimal materialCost, int duration,string notes)
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
                Status = ProposalStatus.Pending
            });

            _context.SaveChanges();
            return (true, "Proposal submitted successfully.");
        }

        // ─────────────────────────────
        // Edit Proposal
        // ─────────────────────────────
        public (bool Success, string Message) EditProposal(
            int proposalId, int workerId,
            decimal laborCost, decimal materialCost, int duration)
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
    }
}