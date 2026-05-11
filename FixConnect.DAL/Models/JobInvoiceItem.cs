namespace FixConnect.DAL.Models
{
    public class JobInvoiceItem
    {
        public int ItemId { get; set; }
        public int JobId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Cost { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Navigation
        public Job Job { get; set; } = null!;
    }
}