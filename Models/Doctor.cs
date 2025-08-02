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

        // --- NEW PROPERTIES FOR APPLICATION TRACKING ---

        [Required]
        [MaxLength(50)]
        public string ApplicationStatus { get; set; } = "Pending"; // Default status for new applications. Values: "Pending", "Approved", "Rejected"

        public string? AdminNotes { get; set; } // To store the reason for rejection or other admin notes.

        // --- COLLECTIONS ---
        
        public virtual ICollection<DoctorDocument> Documents { get; set; } = new List<DoctorDocument>();

        public virtual ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
        
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}