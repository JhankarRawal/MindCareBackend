using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty; // Used as username

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}