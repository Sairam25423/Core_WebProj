using System.ComponentModel.DataAnnotations;

namespace Core_WebProj.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(100, ErrorMessage = "Email must be between 5 and 100 characters.", MinimumLength = 5)]
        public required string Email { get; set; }
    }
}
