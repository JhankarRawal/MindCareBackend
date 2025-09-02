// This single class will represent EVERY document: photo, citizenship, license, etc.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealthApis.Models
{
    public class DoctorDocument
    {
        public int Id { get; set; }

        // Foreign Key to link back to the Doctor
        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }

        // This tells us what kind of document it is.
        // Examples: "PasswordSizedPhoto", "CitizenshipFront", "MedicalLicense"
        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; }

        // The path to where the file is stored on your server
        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        // An optional field for the admin to leave notes (e.g., "Photo is blurry")
        [MaxLength(500)]
        public string? AdminNotes { get; set; }
    }
}