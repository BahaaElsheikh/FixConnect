using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Admin
    {
        public int UserId { get; set; }
        public int? PermissionsLevel { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
