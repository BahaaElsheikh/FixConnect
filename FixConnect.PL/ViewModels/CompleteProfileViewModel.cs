using System.ComponentModel.DataAnnotations;
using FixConnect.DAL.Data.Enums;

namespace FixConnect.PL.ViewModels
{
    // Used for Google OAuth new users (Case B)
    public class CompleteProfileViewModel
    {
        public string FullName { get; set; } = null!;   // pre-filled from Google
        public string Email { get; set; } = null!;       // pre-filled from Google
        public string GoogleId { get; set; } = null!;    // hidden field

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone must be 11 digits.")]
        public string Phone { get; set; } = null!;

        [Required]
        public RoleType Role { get; set; }

        public string? Specialty { get; set; }
    }
}