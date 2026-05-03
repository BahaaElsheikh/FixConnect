using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class WorksAt
    {
        public int UserId { get; set; }
        public int RegionId { get; set; }

        // Navigation
        public Worker Worker { get; set; } = null!;
        public Region Region { get; set; } = null!;
    }
}