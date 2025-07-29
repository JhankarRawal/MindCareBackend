// DTOs/DoctorDocumentUploadDto.cs

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class DoctorDocumentUploadDto
{
    [Required]
    public string ClientId { get; set; } // This will be the Doctor's ID

    [Required]
    public IFormFile Passwordsizedphoto { get; set; }

    [Required]
    public IFormFile CitizenshipFront { get; set; }

    [Required]
    public IFormFile CitizenshipBack { get; set; }

    // Optional professional certificates
    public List<IFormFile>? Certificates { get; set; }

    [Required]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions.")]
    public bool TermsAccepted { get; set; }
}