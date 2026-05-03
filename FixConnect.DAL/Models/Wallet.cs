using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class Wallet
    {
        public int WalletId { get; set; }
        public int WorkerId { get; set; }
        public decimal Balance { get; set; } = 0;

        // Navigation
        public Worker Worker { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
