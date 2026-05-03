using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Data.Enums
{
    public enum JobStatus
    {
        Active = 1,        // شغال دلوقتى
        Completed = 2,     // خلص وفيه Review
        Disputed = 3       // فيه خلاف
    }
}
