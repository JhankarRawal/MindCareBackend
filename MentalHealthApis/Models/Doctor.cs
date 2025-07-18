using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ContactInfo { get; set; } // e.g., office phone or email

        // For login, if doctors are also users
        public int? UserId { get; set; } // Nullable if doctor is not a user (e.g. managed by admin only)
        public virtual User? User { get; set; }


        public virtual ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}