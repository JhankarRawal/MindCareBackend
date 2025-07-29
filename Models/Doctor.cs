// Models/Doctor.cs - Updated

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

        // --- NEW PROPERTIES FOR DOCUMENT UPLOAD ---

        [MaxLength(255)]
        public string? PasswordSizedPhotoPath { get; set; }

        [MaxLength(255)]
        public string? CitizenshipFrontPath { get; set; }

        [MaxLength(255)]
        public string? CitizenshipBackPath { get; set; }
        
        public bool HasAcceptedTerms { get; set; }

        [MaxLength(50)]
        public string? DocumentSubmissionStatus { get; set; } // e.g., "Pending", "UnderReview", "Approved"
        
        // --- END NEW PROPERTIES ---

        public virtual ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        
        // Navigation property for the new certificates entity
        public virtual ICollection<DoctorCertificate> Certificates { get; set; } = new List<DoctorCertificate>();
    }
}