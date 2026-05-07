namespace FixConnect.DAL.Models
{
    public class Specialty
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = null!;

        public ICollection<Worker> Workers { get; set; } = new List<Worker>();
        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}