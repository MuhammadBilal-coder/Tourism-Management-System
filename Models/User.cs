using System.ComponentModel.DataAnnotations;

namespace TourismManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        // ✅ Optional:  Password reset token (for forgot password feature)
        [MaxLength(500)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }
    }
}