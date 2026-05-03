using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Data.Enums
{
    public enum AvailabilityStatus
    {
        Available = 1,     // متاح لاستقبال طلبات
        Busy = 2,          // مشغول بـ Job حالياً
        Offline = 3        // مش متاح
    }
}
