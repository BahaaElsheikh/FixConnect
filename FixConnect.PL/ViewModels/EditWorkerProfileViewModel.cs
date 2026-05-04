using FixConnect.DAL.Data.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class EditWorkerProfileViewModel
    {
        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        public string? Bio { get; set; }
        public string? Specialty { get; set; }
        public AvailabilityStatus AvailabilityStatus { get; set; }
        public List<int> SelectedRegionIds { get; set; } = new();
        public List<RegionOption> AllRegions { get; set; } = new();

        // Photo Upload
        public IFormFile? PhotoFile { get; set; }
        public string? CurrentPhotoUrl { get; set; }

        // Verification Upload
        public IFormFile? IdFrontImage { get; set; }
        public IFormFile? IdBackImage { get; set; }
        public bool HasPendingVerification { get; set; }
    }

    public class RegionOption
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; } = null!;
    }
}