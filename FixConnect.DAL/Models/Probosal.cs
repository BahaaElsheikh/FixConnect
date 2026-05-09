using FixConnect.DAL.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Proposal
    {
        public int ProposalId { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public int? DurationEstimate { get; set; }   // days


        public ProposalStatus Status { get; set; }
        public int UserId { get; set; }      // Customer
        public int RequestId { get; set; }
        public int WorkerId { get; set; }

        public string? Notes { get; set; }
        // Navigation
        public Customer Customer { get; set; } = null!;
        public Request Request { get; set; } = null!;
        public Worker Worker { get; set; } = null!;
        public Job? Job { get; set; }
    }
}