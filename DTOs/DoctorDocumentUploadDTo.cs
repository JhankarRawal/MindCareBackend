using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MentalHealthApis.DTOs
{
    public class DoctorDocumentUploadDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "A passport-sized photo is required.")]
        public IFormFile PasswordSizedPhoto { get; set; }

        [Required(ErrorMessage = "The front of your citizenship is required.")]
        public IFormFile CitizenshipFront { get; set; }

        [Required(ErrorMessage = "The back of your citizenship is required.")]
        public IFormFile CitizenshipBack { get; set; }

        // This allows for uploading zero or more optional certificates
        public List<IFormFile>? Certificates { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions.")]
        public bool HasAcceptedTerms { get; set; }
    }
}