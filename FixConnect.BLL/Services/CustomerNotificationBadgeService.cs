using System;
using System.Linq;
using FixConnect.BLL.DTOs;
using FixConnect.DAL.Context;
using FixConnect.DAL.Models;

namespace FixConnect.BLL.Services
{
    public class CustomerNotificationBadgeService
    {
        private readonly AppDbContext _context;

        public CustomerNotificationBadgeService(AppDbContext context)
        {
            _context = context;
        }

        private CustomerNotificationState GetOrCreateState(int customerId)
        {
            var state = _context.Set<CustomerNotificationState>().FirstOrDefault(s => s.CustomerId == customerId);
            if (state == null)
            {
                state = new CustomerNotificationState
                {
                    CustomerId = customerId,
                    LastSeenProposalsReceived = DateTime.Now,
                    LastSeenJobs = DateTime.Now,
                    LastSeenRequests = DateTime.Now
                };
                _context.Set<CustomerNotificationState>().Add(state);
                _context.SaveChanges();
            }
            return state;
        }

        public CustomerNotificationBadgeViewModel GetBadgeCounts(int customerId)
        {
            var state = GetOrCreateState(customerId);

            // 1. حساب طلبات العميل الجديدة
            int requestsCount = _context.Requests
                .Count(r => r.UserId == customerId && r.CreatedAt > state.LastSeenRequests);

            // 2. حساب العروض (Proposals) الجديدة المقدمة على طلبات هذا العميل
            int proposalsCount = _context.Proposals
                .Count(p => p.Request.UserId == customerId && p.CreatedAt > state.LastSeenProposalsReceived);

            // 3. حساب الشغل (Jobs) اللي حصل فيه تحديث يخص العميل
            int jobsCount = _context.Jobs
                .Count(j => j.Proposal.UserId == customerId && j.UpdatedAt > state.LastSeenJobs);

            return new CustomerNotificationBadgeViewModel
            {
                Requests = requestsCount,
                ProposalsReceived = proposalsCount,
                Jobs = jobsCount
            };
        }

        public void MarkRequestsSeen(int customerId)
        {
            var state = GetOrCreateState(customerId);
            state.LastSeenRequests = DateTime.Now;
            _context.SaveChanges();
        }

        public void MarkProposalsReceivedSeen(int customerId)
        {
            var state = GetOrCreateState(customerId);
            state.LastSeenProposalsReceived = DateTime.Now;
            _context.SaveChanges();
        }

        public void MarkJobsSeen(int customerId)
        {
            var state = GetOrCreateState(customerId);
            state.LastSeenJobs = DateTime.Now;
            _context.SaveChanges();
        }
    }
}