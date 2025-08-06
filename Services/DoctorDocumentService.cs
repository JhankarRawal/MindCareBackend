using MentalHealthApis.Data;
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

public class DoctorDocumentService : IDoctorDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public DoctorDocumentService(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<bool> ProcessAndSaveDocumentsAsync(DoctorDocumentUploadDto dto)
    {
        var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
        if (doctor == null) return false; // Doctor not found

        // Helper to save a file and create a DB record for it
        async Task CreateDocumentRecord(IFormFile file, string documentType)
        {
            if (file == null || file.Length == 0) return;

            // 1. Define where to save the file
            var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "documents", "doctors", dto.DoctorId.ToString());
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // 2. Create a unique filename
            var uniqueFileName = $"{documentType}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 3. Save the file to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Create the database entity
            var document = new DoctorDocument
            {
                DoctorId = dto.DoctorId,
                DocumentType = documentType,
                FilePath = $"/documents/doctors/{dto.DoctorId}/{uniqueFileName}", // Web-accessible path
            };
            _context.DoctorDocuments.Add(document);
        }

        // Process each document from the DTO
        await CreateDocumentRecord(dto.PasswordSizedPhoto, "PasswordSizedPhoto");
        await CreateDocumentRecord(dto.CitizenshipFront, "CitizenshipFront");
        await CreateDocumentRecord(dto.CitizenshipBack, "CitizenshipBack");

        if (dto.Certificates != null)
        {
            foreach (var certFile in dto.Certificates)
            {
                await CreateDocumentRecord(certFile, "Certificate");
            }
        }
        
        doctor.HasAcceptedTerms = dto.HasAcceptedTerms;

        // *** THIS IS THE FIX: Set the application status to "Pending" ***
        doctor.ApplicationStatus = "Pending";


        // Save everything to the database at once
        return await _context.SaveChangesAsync() > 0;
    }
}