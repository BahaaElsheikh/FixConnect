using FixConnect.DAL.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Job
    {
        public int JobId { get; set; }
        public decimal? LiveInvoiceTotal { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public JobStatus Status { get; set; }
        public int ProposalId { get; set; }



        public string? CustomerExactAddress { get; set; }
        public string? CustomerContactNumber { get; set; }
        public DateTime? EstimatedStartTime { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public decimal? LaborCost { get; set; }   // ← سعر الخدمة (مختلف عن الخامات)

        // Navigation
        public Proposal Proposal { get; set; } = null!;
        public Review? Review { get; set; }
        public ICollection<JobInvoiceItem> InvoiceItems { get; set; } = new List<JobInvoiceItem>();

    }
}