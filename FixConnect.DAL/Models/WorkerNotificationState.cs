using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class WorkerNotificationState
    {
        public int WorkerId { get; set; }

        public DateTime LastSeenDirectRequests { get; set; } = DateTime.MinValue;
        public DateTime LastSeenProposals { get; set; } = DateTime.MinValue;
        public DateTime LastSeenJobs { get; set; } = DateTime.MinValue;
        public DateTime LastSeenWallet { get; set; } = DateTime.MinValue;

        // Navigation
        public Worker Worker { get; set; } = null!;
    }
}
