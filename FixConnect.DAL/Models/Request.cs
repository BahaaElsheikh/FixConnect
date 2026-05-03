using FixConnect.DAL.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Request
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public RequestType RequestType { get; set; }
        public RequestStatus Status { get; set; }
        public int? TargetWorkerId { get; set; }

       
        public int UserId { get; set; }         // Customer
        public int RegionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Customer Customer { get; set; } = null!;
        public Region Region { get; set; } = null!;
        public Worker? TargetWorker { get; set; }
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    }
}