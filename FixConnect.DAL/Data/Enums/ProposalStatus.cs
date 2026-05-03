using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Data.Enums
{
    public enum ProposalStatus
    {
        Pending = 1,       // لسه ما اتردش عليه
        Accepted = 2,      // اتقبل
        Rejected = 3,      // اترفض من الـ Customer
        AutoRejected = 4   // اترفض تلقائياً لما اتقبل عرض تاني
    }
}