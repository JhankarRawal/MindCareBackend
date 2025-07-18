using MentalHealthApis.Models; // For AppointmentStatus
using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // For easier display
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty; // For easier display
        public DateTime AppointmentDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? UserNotes { get; set; }
        public string? DoctorNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateAppointmentDto
    {
        [Required]
        public int UserId { get; set; } // Will be taken from authenticated user usually
        [Required]
        public int DoctorId { get; set; }
        [Required]
        public DateTime AppointmentDateTime { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public string? UserNotes { get; set; }
        public int? DoctorAvailabilitySlotId { get; set; } // If booking against a specific slot
    }

    public class RescheduleAppointmentDto
    {
        [Required]
        public DateTime NewAppointmentDateTime { get; set; }
        public int? NewDoctorAvailabilitySlotId { get; set; } // If applicable
    }

    public class UpdateAppointmentStatusDto
    {
        [Required]
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; } // e.g., cancellation reason or completion notes
    }
}