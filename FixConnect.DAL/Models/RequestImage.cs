namespace FixConnect.DAL.Models
{
    public class RequestImage
    {
        public int ImageId { get; set; }
        public int RequestId { get; set; }
        public string ImagePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation
        public Request Request { get; set; } = null!;
    }
}