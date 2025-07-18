using MentalHealthApis.Models;

namespace MentalHealthApis.DTOs
{
    // Used when returning user information to the client
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
    }

    // Used when registering a new user
    public class RegisterUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }

    // Used when logging in
    public class LoginUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Used when updating the current user
    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
