using System.ComponentModel.DataAnnotations;
using MentalHealthApis.Models; 

namespace MentalHealthApis.DTOs
{
    // This defines the nested "doctorInfo" object
    public class DoctorInfoResponseDto
    {
        public string ApplicationStatus { get; set; }
        public string? RejectionReason { get; set; }
    }

    // This is your SINGLE definition for UserDto
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; } // Added from your other UserDto definition for completeness
        public UserRole Role { get; set; }
        public DoctorInfoResponseDto? DoctorInfo { get; set; } 
    }

    // The rest of your related DTOs live here
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; } = UserRole.User; 
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }
}