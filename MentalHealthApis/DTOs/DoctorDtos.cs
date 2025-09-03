using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.DTOs
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
        public int? UserId { get; set; } // If doctor is linked to a user account
    }

    public class CreateDoctorDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Specialization { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
        public int? UserId { get; set; } // Optional: Link to an existing User account (that user should have Doctor role)
    }

    public class UpdateDoctorDto
    {
        public string? Name { get; set; }
        public string? Specialization { get; set; }
        public string? ContactInfo { get; set; }
    }

    public class DoctorAvailabilityDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsBooked { get; set; }
    }

    public class CreateDoctorAvailabilityDto
    {
        [Required]
        public int DoctorId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
    }
}