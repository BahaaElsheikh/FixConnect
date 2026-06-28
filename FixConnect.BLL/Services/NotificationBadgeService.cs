using FixConnect.DAL.Context;
using FixConnect.DAL.Models;
using FixConnect.DAL.Data.Enums;
using FixConnect.BLL.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FixConnect.BLL.Services
{
    public class NotificationBadgeService
    {
        // ✅ DI: AppDbContext injected (same pattern as WorkerService)
        private readonly AppDbContext _context;

        public NotificationBadgeService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Get or Create State (auto-create on first use)
        // ─────────────────────────────
        private WorkerNotificationState GetOrCreateState(int workerId)
        {
            var state = _context.WorkerNotificationStates
                .FirstOrDefault(s => s.WorkerId == workerId);

            if (state == null)
            {
                state = new WorkerNotificationState
                {
                    WorkerId = workerId,
                    LastSeenDirectRequests = DateTime.MinValue,
                    LastSeenProposals = DateTime.MinValue,
                    LastSeenJobs = DateTime.MinValue,
                    LastSeenWallet = DateTime.MinValue
                };
                _context.WorkerNotificationStates.Add(state);
                _context.SaveChanges();
            }

            return state;
        }

        // ─────────────────────────────
        // Get Badge Counts (used by ViewComponent + Polling endpoint)
        // ─────────────────────────────
        public NotificationBadgeViewModel GetBadgeCounts(int workerId)
        {
            var state = GetOrCreateState(workerId);

            int directRequests = _context.Requests
                .Count(r => r.TargetWorkerId == workerId
                         && r.CreatedAt > state.LastSeenDirectRequests);

            int proposals = _context.Proposals
                .Count(p => p.WorkerId == workerId
                         && p.UpdatedAt.HasValue
                         && p.UpdatedAt.Value > state.LastSeenProposals);

            int jobs = _context.Jobs
                .Count(j => j.Proposal.WorkerId == workerId
                         && j.UpdatedAt.HasValue
                         && j.UpdatedAt.Value > state.LastSeenJobs);

            int wallet = _context.Transactions
                .Count(t => t.Wallet.WorkerId == workerId
                         && t.CreatedAt > state.LastSeenWallet);

            return new NotificationBadgeViewModel
            {
                DirectRequests = directRequests,
                Proposals = proposals,
                Jobs = jobs,
                Wallet = wallet
            };
        }

        // ─────────────────────────────
        // Mark As Seen — one method per section
        // ─────────────────────────────
        public void MarkDirectRequestsSeen(int workerId)
        {
            var state = GetOrCreateState(workerId);
            state.LastSeenDirectRequests = DateTime.Now;
            _context.SaveChanges();
        }

        public void MarkProposalsSeen(int workerId)
        {
            var state = GetOrCreateState(workerId);
            state.LastSeenProposals = DateTime.Now;
            _context.SaveChanges();
        }

        public void MarkJobsSeen(int workerId)
        {
            var state = GetOrCreateState(workerId);
            state.LastSeenJobs = DateTime.Now;
            _context.SaveChanges();
        }

        public void MarkWalletSeen(int workerId)
        {
            var state = GetOrCreateState(workerId);
            state.LastSeenWallet = DateTime.Now;
            _context.SaveChanges();
        }
    }
}