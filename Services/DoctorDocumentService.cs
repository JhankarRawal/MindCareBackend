// Services/DoctorDocumentService.cs - UPDATED

using MentalHealthApis.Data;     // Your DbContext namespace
using MentalHealthApis.Models;   // Your Models namespace
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore; // Required for FirstOrDefaultAsync
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

public class DoctorDocumentService : IDoctorDocumentService
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ApplicationDbContext _dbContext; // Assuming this is your DbContext name
    private readonly ILogger<DoctorDocumentService> _logger;

    public DoctorDocumentService(IWebHostEnvironment hostingEnvironment, ApplicationDbContext dbContext, ILogger<DoctorDocumentService> logger)
    {
        _hostingEnvironment = hostingEnvironment;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> ProcessAndSaveDocumentsAsync(DoctorDocumentUploadDto dto)
    {
        // The incoming 'clientId' is the USER's ID. Let's parse it.
        if (!int.TryParse(dto.ClientId, out int userId))
        {
            _logger.LogError($"Invalid UserId format received: '{dto.ClientId}'");
            return false;
        }

        // 1. VERIFY THE USER EXISTS
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogError($"No user found with ID '{userId}'. Cannot create a doctor profile.");
            return false;
        }
        
        // 2. PREVENT DUPLICATE SUBMISSIONS
        var existingDoctorProfile = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (existingDoctorProfile != null)
        {
            _logger.LogWarning($"A doctor application has already been submitted for user ID '{userId}'.");
            // You might want to return a specific message to the user here.
            return false; 
        }

        // 3. CREATE THE NEW DOCTOR ENTITY
        var newDoctor = new Doctor
        {
            UserId = userId,
            Name = user.Name, // Pre-populate the name from the User's profile
            ContactInfo = user.PhoneNumber, // Or other relevant info
            HasAcceptedTerms = dto.TermsAccepted,
            DocumentSubmissionStatus = "UnderReview",
            Specialization = "Pending Review" // A default value until admin approves
        };

        // 4. DEFINE FOLDER PATH AND SAVE FILES
        // Use the unique UserId for the folder to keep it consistent.
        var userFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "doctors", userId.ToString());
        Directory.CreateDirectory(userFolderPath);

        try
        {
            // 5. UPDATE THE NEW ENTITY WITH FILE PATHS
            newDoctor.PasswordSizedPhotoPath = await SaveFileAsync(dto.Passwordsizedphoto, userFolderPath, userId.ToString());
            newDoctor.CitizenshipFrontPath = await SaveFileAsync(dto.CitizenshipFront, userFolderPath, userId.ToString());
            newDoctor.CitizenshipBackPath = await SaveFileAsync(dto.CitizenshipBack, userFolderPath, userId.ToString());

            // 6. ADD NEW DOCTOR AND CERTIFICATES TO THE DATABASE
            _dbContext.Doctors.Add(newDoctor);

            if (dto.Certificates != null && dto.Certificates.Count > 0)
            {
                foreach (var certFile in dto.Certificates)
                {
                    var certPath = await SaveFileAsync(certFile, userFolderPath, userId.ToString());
                    // The DoctorId will be set by Entity Framework automatically
                    // when the parent newDoctor is saved.
                    newDoctor.Certificates.Add(new DoctorCertificate { FilePath = certPath });
                }
            }
            
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Successfully created new doctor profile for user ID '{userId}'. Status is 'UnderReview'.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating doctor profile for User ID: {userId}");
            // Optional: Cleanup partially saved files if something goes wrong.
            return false;
        }
    }

    private async Task<string> SaveFileAsync(IFormFile file, string folderPath, string subFolder)
    {
        if (file == null || file.Length == 0) return null;

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var dbPath = Path.Combine("uploads", "doctors", subFolder, uniqueFileName);
        return dbPath.Replace('\\', '/'); // Use forward slashes for web compatibility
    }
}