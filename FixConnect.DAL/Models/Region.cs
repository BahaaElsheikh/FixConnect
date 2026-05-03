using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Region
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; } = null!;

        // Navigation
        public ICollection<WorksAt> WorksAt { get; set; } = new List<WorksAt>();
        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}