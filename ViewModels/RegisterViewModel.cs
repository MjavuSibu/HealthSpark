using System.ComponentModel.DataAnnotations;

namespace HealthSpark.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public string DateOfBirth { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "patient"; // patient | doctor | admin

        public string? ErrorMessage { get; set; }
    }
}