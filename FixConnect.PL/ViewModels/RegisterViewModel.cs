using FixConnect.DAL.Data.Enums;
using System.ComponentModel.DataAnnotations;

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
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;


        [Required]
        public RoleType Role { get; set; }

        // Worker Only
        public int? SpecialtyId { get; set; }                    // ← بدل string
        public List<SpecialtyOption> Specialties { get; set; } = new();  // ← للـ Dropdown
    }

    public class SpecialtyOption
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = null!;
    }
}