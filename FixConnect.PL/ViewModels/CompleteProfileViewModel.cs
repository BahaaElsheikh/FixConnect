using FixConnect.DAL.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class CompleteProfileViewModel
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string GoogleId { get; set; } = null!;

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone must be 11 digits.")]
        public string Phone { get; set; } = null!;

        [Required]
        public RoleType Role { get; set; }

        // Worker Only
        public int? SpecialtyId { get; set; }
        public List<SpecialtyOption> Specialties { get; set; } = new();
    }
}