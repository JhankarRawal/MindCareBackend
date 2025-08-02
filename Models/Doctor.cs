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
        public string? ContactInfo { get; set; }

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public bool HasAcceptedTerms { get; set; }

        // --- WE ARE REMOVING ALL OF THESE ---
        // public string? PasswordSizedPhotoPath { get; set; }
        // public string? CitizenshipFrontPath { get; set; }
        // public string? CitizenshipBackPath { get; set; }
        // public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        // public virtual ICollection<DoctorCertificate> Certificates { get; set; } = new List<DoctorCertificate>();
        // --- END OF REMOVALS ---

        // --- ADD THIS SINGLE, UNIFIED COLLECTION ---
        public virtual ICollection<DoctorDocument> Documents { get; set; } = new List<DoctorDocument>();

        // These collections stay the same
        public virtual ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}