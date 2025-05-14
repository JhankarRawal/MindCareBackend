using MentalHealthApis.Models; // For UserRole

namespace MentalHealthApis.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
    }

    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }
}