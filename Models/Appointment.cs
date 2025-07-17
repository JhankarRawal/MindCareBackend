using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealthApis.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [Required]
        public int DoctorId { get; set; }
        public virtual Doctor? Doctor { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        // Store duration if it can vary, or assume a fixed duration (e.g., 1 hour)
        public int DurationMinutes { get; set; } = 60; // Default to 60 minutes

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [MaxLength(500)]
        public string? UserNotes { get; set; } // Notes from user when booking

        [MaxLength(500)]
        public string? DoctorNotes { get; set; } // Notes from doctor after session

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // If using the DoctorAvailability.IsBooked approach
        public int? DoctorAvailabilitySlotId { get; set; }
        [ForeignKey("DoctorAvailabilitySlotId")]
        public virtual DoctorAvailability? DoctorAvailabilitySlot { get; set; }
    }
}