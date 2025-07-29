// Models/DoctorCertificate.cs - New File

using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.Models
{
    public class DoctorCertificate
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; } // Path to the saved certificate file

        // Foreign Key to link this certificate back to a Doctor
        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }
    }
}