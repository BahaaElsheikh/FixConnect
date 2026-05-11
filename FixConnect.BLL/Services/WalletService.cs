using FixConnect.DAL.Context;
using FixConnect.DAL.Models;

namespace FixConnect.BLL.Services
{
    public class WalletService
    {
        private readonly AppDbContext _context;
        private const decimal CommissionRate = 0.10m;  // 10% commission

        public WalletService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Deduct Commission after Job
        // ─────────────────────────────
        public void DeductCommission(int workerId, decimal jobTotal)
        {
            var wallet = _context.Wallets
                .FirstOrDefault(w => w.WorkerId == workerId);

            if (wallet == null) return;

            var commission = jobTotal * CommissionRate;
            wallet.Balance -= commission;

            _context.Transactions.Add(new Transaction
            {
                WalletId = wallet.WalletId,
                Amount = commission,
                Type = 2,  // Debit
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Get Wallet Balance
        // ─────────────────────────────
        public decimal GetBalance(int workerId)
        {
            return _context.Wallets
                .Where(w => w.WorkerId == workerId)
                .Select(w => w.Balance)
                .FirstOrDefault();
        }
    }
}