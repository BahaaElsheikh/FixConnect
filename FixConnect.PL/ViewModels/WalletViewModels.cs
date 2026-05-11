using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class WalletViewModel
    {
        public decimal Balance { get; set; }
        public List<TransactionRowViewModel> Transactions { get; set; } = new();
    }

    public class TransactionRowViewModel
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = null!;   // Credit / Debit
        public DateTime CreatedAt { get; set; }
    }

    // ─────────────────────────────
    // Recharge
    // ─────────────────────────────
    public class RechargeViewModel
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = null!;
    }
}

