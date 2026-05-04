using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class PortfolioItemViewModel
    {
        public int ItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = null!;

        [StringLength(300)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}