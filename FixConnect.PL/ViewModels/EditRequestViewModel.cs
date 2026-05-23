using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class EditRequestViewModel
    {
        public int RequestId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = null!;

        [StringLength(300)]
        public string? Description { get; set; }

        public int RegionId { get; set; }
        public int? SpecialtyId { get; set; }
        public List<IFormFile>? NewImages { get; set; }
        public List<ExistingImageItem> ExistingImages { get; set; } = new();
        public List<int> DeleteImageIds { get; set; } = new();
        public List<SpecialtyOption> Specialties { get; set; } = new();
        public List<RegionOption> Regions { get; set; } = new();
    }

    public class ExistingImageItem
    {
        public int ImageId { get; set; }
        public string ImagePath { get; set; } = null!;
    }
}