using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public int Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int WalletId { get; set; }

        // Navigation
        public Wallet Wallet { get; set; } = null!;
    }
}
