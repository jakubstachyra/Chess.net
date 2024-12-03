using System.ComponentModel.DataAnnotations;

namespace Domain.AuthModels
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }

        [Required]
        public required string Username { get; set; }
    }

}
