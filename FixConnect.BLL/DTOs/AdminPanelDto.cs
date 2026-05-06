using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.BLL.DTOs
{
    
        public class UserResult
        {
            public int UserId { get; set; }
            public string FullName { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string Phone { get; set; } = null!;
            public string Role { get; set; } = null!;
            public bool IsActive { get; set; }
            public bool IsVerified { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class RequestResult
        {
            public int RequestId { get; set; }
            public string Title { get; set; } = null!;
            public string CustomerName { get; set; } = null!;
            public string RegionName { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string RequestType { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
        }

        public class ProposalResult
        {
            public int ProposalId { get; set; }
            public string WorkerName { get; set; } = null!;
            public string CustomerName { get; set; } = null!;
            public string RequestTitle { get; set; } = null!;
            public decimal? LaborCost { get; set; }
            public decimal? MaterialCost { get; set; }
            public int? DurationEstimate { get; set; }
            public string Status { get; set; } = null!;
        }
    }

