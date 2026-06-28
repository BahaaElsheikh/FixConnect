using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.BLL.DTOs
{
    public class NotificationBadgeViewModel
    {
        public int DirectRequests { get; set; }
        public int Proposals { get; set; }
        public int Jobs { get; set; }
        public int Wallet { get; set; }
    }
}
