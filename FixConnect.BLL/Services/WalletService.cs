using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;


using Microsoft.EntityFrameworkCore;

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
        // Commission = 10% من LaborCost في الـ Job (مش الـ Proposal)
        public void DeductCommission(int workerId, decimal jobLaborCost)
        {
            var wallet = _context.Wallets
                .FirstOrDefault(w => w.WorkerId == workerId);

            if (wallet == null) return;

            var commission = Math.Round(jobLaborCost * CommissionRate, 2);

            // Negative balance allowed
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
        // Recharge Wallet
        // ─────────────────────────────
        public void Recharge(int workerId, decimal amount)
        {
            var wallet = _context.Wallets
                .FirstOrDefault(w => w.WorkerId == workerId);

            if (wallet == null) return;

            wallet.Balance += amount;

            _context.Transactions.Add(new Transaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                Type = 1,  // Credit
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();
        }

        // ─────────────────────────────
        // Get Wallet with Transactions
        // ─────────────────────────────
        public (decimal Balance, decimal PendingPayouts, decimal TotalEarnings, List<Transaction> Transactions) GetWalletDetails(int workerId)
        {
            var wallet = _context.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefault(w => w.WorkerId == workerId);

            if (wallet == null) return (0, 0, 0, new());

            // 1. حساب الـ Pending: كل الـ Jobs اللي لسه مخلصتش (مش Completed) للـ Worker ده
            // ملحوظة: غير اسم الحالة (مثلاً "Completed") بناءً على الـ Enum أو الـ String عندك في الـ DB
            decimal pendingPayouts = _context.Jobs
            .Where(j => j.Proposal.WorkerId == workerId &&
                 j.Status != JobStatus.Completed)
             .Sum(j => j.LaborCost ?? 0m);

            decimal totalEarnings = _context.Jobs
                .Where(j => j.Proposal.WorkerId == workerId &&
                            j.Status == JobStatus.Completed)
                .Sum(j => j.LaborCost ?? 0m);
            var transactions = wallet.Transactions.OrderByDescending(t => t.CreatedAt).ToList();

           
            return (wallet.Balance, pendingPayouts, totalEarnings, transactions);
        }


    }
}