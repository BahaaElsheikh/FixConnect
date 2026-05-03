using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int RatingValue { get; set; }
        public string? Comment { get; set; }
        public int UserId { get; set; }    // Customer
        public int JobId { get; set; }
        public int WorkerId { get; set; }

        // Navigation
        public Customer Customer { get; set; } = null!;
        public Job Job { get; set; } = null!;
        public Worker Worker { get; set; } = null!;
    }
}