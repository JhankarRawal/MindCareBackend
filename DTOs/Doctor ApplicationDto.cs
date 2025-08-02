using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.DTOs
{
    public class DoctorApplicationDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; }

        [MaxLength(200)]
        public string? ContactInfo { get; set; }
    }
}