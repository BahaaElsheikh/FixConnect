using System.ComponentModel.DataAnnotations;
using FixConnect.DAL.Data.Enums;

namespace FixConnect.PL.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = null!;

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone must be 11 digits.")]
        public string Phone { get; set; } = null!;

        [Required]
        public RoleType Role { get; set; }

        // Only required if Role == Worker
        public string? Specialty { get; set; }
    }
}