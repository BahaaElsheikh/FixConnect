using System;

namespace FixConnect.DAL.Models
{
    public class CustomerNotificationState
    {
        public int CustomerId { get; set; }
        public DateTime LastSeenProposalsReceived { get; set; } = DateTime.MinValue;
        public DateTime LastSeenJobs { get; set; } = DateTime.MinValue;
        public DateTime LastSeenRequests { get; set; } = DateTime.MinValue;

        // Navigation property للـ User العام أو الـ Customer المعتمد عندك
        public User Customer { get; set; } = null!;
    }
}