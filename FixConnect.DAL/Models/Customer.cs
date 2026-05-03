using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Customer
    {
        public int UserId { get; set; }
        public string? Address { get; set; }
        public int TotalRequests { get; set; } = 0;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Request> Requests { get; set; } = new List<Request>();
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}