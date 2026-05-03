using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Data.Enums
{
    public enum RequestStatus
    {
        Pending = 1,       // تم النشر، لسه فيه تلقى عروض
        InProgress = 2,    // اتقبل عرض وبدأ الشغل
        Completed = 3,     // الشغل خلص
        Cancelled = 4,     // اتلغى
        Closed = 5         // مفيش عروض اتقبلت وانتهى
    }
}