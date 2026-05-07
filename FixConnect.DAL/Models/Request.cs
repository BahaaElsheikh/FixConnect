namespace FixConnect.DAL.Models
{
    public class Request
    {
        public int RequestId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int RequestType { get; set; }
        public int? TargetWorkerId { get; set; }
        public int? SpecialtyId { get; set; }        // ← جديد
        public int Status { get; set; }
        public int UserId { get; set; }
        public int RegionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Customer Customer { get; set; } = null!;
        public Region Region { get; set; } = null!;
        public Worker? TargetWorker { get; set; }
        public Specialty? Specialty { get; set; }    // ← جديد
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public ICollection<RequestImage> Images { get; set; } = new List<RequestImage>();
    }
}