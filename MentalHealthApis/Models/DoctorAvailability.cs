using System;
using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.Models
{
    public class DoctorAvailability
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public virtual Doctor? Doctor { get; set; }

        [Required]
        public DateTime StartTime { get; set; } // Specific date and time slot start

        [Required]
        public DateTime EndTime { get; set; }   // Specific date and time slot end

        public bool IsBooked { get; set; } = false; // Indicates if this specific slot is taken
    }
}