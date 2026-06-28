using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}