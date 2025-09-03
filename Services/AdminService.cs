using MentalHealthApis.Data;
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics; // Added for Debug.WriteLine (alternative to Console.WriteLine)

namespace MentalHealthApis.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationDbContext context, IAppointmentService appointmentService, ILogger<AdminService> logger)
        {
            _context = context;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        // --- Users ---
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
            }).ToListAsync();
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, UserRole newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Role = newRole;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Doctors ---
        public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync()
        {
            return await _context.Doctors.Select(d => new DoctorDto
            {
                Id = d.Id,
                Name = d.Name,
                Specialization = d.Specialization,
                ContactInfo = d.ContactInfo,
                UserId = d.UserId,
                ApplicationStatus = d.ApplicationStatus
            }).ToListAsync();
        }

        // --- NEW: Approve Doctor Application and Promote User Role (Scenario 2) ---
        public async Task<bool> ApproveDoctorApplicationAndPromoteUserAsync(int doctorId)
        {
            // Eagerly load the associated User to avoid a separate query later
            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null)
            {
                _logger.LogWarning($"ApproveDoctorApplicationAndPromoteUserAsync: Doctor not found for doctorId: {doctorId}");
                return false;
            }
            if (doctor.User == null)
            {
                _logger.LogError($"ApproveDoctorApplicationAndPromoteUserAsync: User associated with doctorId: {doctorId} not found.");
                return false;
            }

            // Update doctor's application status
            doctor.ApplicationStatus = "Approved"; // Assuming "Approved" is the desired string status
            doctor.AdminNotes = "Doctor application approved by admin.";

            // Promote user role if they are not already a doctor
            if (doctor.User.Role != UserRole.Doctor)
            {
                _logger.LogInformation($"ApproveDoctorApplicationAndPromoteUserAsync: Promoting user {doctor.User.Id} (Doctor {doctorId}) from role {doctor.User.Role} to {UserRole.Doctor}.");
                doctor.User.Role = UserRole.Doctor;
            }
            else
            {
                _logger.LogInformation($"ApproveDoctorApplicationAndPromoteUserAsync: User {doctor.User.Id} (Doctor {doctorId}) is already a Doctor. Role not changed.");
            }
            
            // --- START DEBUGGING / LOGGING CODE (similar to what you had) ---
            _logger.LogDebug("--- Entity Framework Change Tracker State before SaveChangesAsync for ApproveDoctorApplicationAndPromoteUserAsync ---");
            var entries = _context.ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                _logger.LogDebug($"  Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                if (entry.State == EntityState.Modified)
                {
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified)
                        {
                            _logger.LogDebug($"    Property: {prop.Metadata.Name}, Original: {prop.OriginalValue}, Current: {prop.CurrentValue}");
                        }
                    }
                }
            }
            _logger.LogDebug("---------------------------------------------------------------------------------------------------------");
            // --- END DEBUGGING / LOGGING CODE ---

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"ApproveDoctorApplicationAndPromoteUserAsync: Successfully approved doctor application {doctorId} and ensured user {doctor.User.Id} has role {UserRole.Doctor}.");
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"ApproveDoctorApplicationAndPromoteUserAsync: Concurrency error updating doctor {doctorId} or user {doctor.User.Id}.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ApproveDoctorApplicationAndPromoteUserAsync: General error saving changes for doctor {doctorId} or user {doctor.User.Id}.");
                return false;
            }
        }

        public async Task<bool> UpdateDoctorApplicationStatusAsync(int doctorId, string status, string? notes)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null)
            {
                return false;
            }

            doctor.ApplicationStatus = status;
            doctor.AdminNotes = notes;

            await _context.SaveChangesAsync();
            return true;
        }


        // --- Appointments ---
        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            return await _appointmentService.GetAllAppointmentsAsync();
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByStatusAsync(string status)
        {
            if (!Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            {
                return new List<AppointmentDto>();
            }

            return await _context.Appointments
                .Where(a => a.Status == statusEnum)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    DoctorId = a.DoctorId,
                    AppointmentDateTime = a.AppointmentDateTime,
                    Status = a.Status
                }).ToListAsync();
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return false;
            appointment.Status = AppointmentStatus.CancelledByAdmin;
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Doctor Availability ---
        public async Task<DoctorAvailabilityDto?> SetDoctorAvailabilityAsync(CreateDoctorAvailabilityDto dto)
        {
            var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
            if (doctor == null) return null;

            if (dto.StartTime >= dto.EndTime || dto.StartTime <= DateTime.UtcNow)
                return null;

            var hasOverlap = await _context.DoctorAvailabilities.AnyAsync(da =>
                da.DoctorId == dto.DoctorId &&
                da.StartTime < dto.EndTime &&
                da.EndTime > dto.StartTime);

            if (hasOverlap) return null;

            var availability = new DoctorAvailability
            {
                DoctorId = dto.DoctorId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            _context.DoctorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            return new DoctorAvailabilityDto
            {
                Id = availability.Id,
                DoctorId = availability.DoctorId,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                IsBooked = availability.IsBooked
            };
        }

        public async Task<bool> DeleteDoctorAvailabilityAsync(int doctorId, int availabilityId)
        {
            var availability = await _context.DoctorAvailabilities.FindAsync(availabilityId);
            if (availability == null || availability.DoctorId != doctorId) return false;
            if (availability.IsBooked) return false;

            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Blog Posts ---
        public async Task<IEnumerable<BlogPostAdminDto>> GetAllBlogPostsAsync()
        {
            return await _context.BlogPosts
                .Select(p => new BlogPostAdminDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    ContentPreview = p.Content.Substring(0, Math.Min(p.Content.Length, 100)),
                    Status = p.Status,
                    AuthorId = p.AuthorId
                }).ToListAsync();
        }

        public async Task<bool> ApproveBlogPostAsync(int postId)
        {
            var post = await _context.BlogPosts.FindAsync(postId);
            if (post == null) return false;
            post.Status = PostStatus.Published;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectBlogPostAsync(int postId)
        {
            var post = await _context.BlogPosts.FindAsync(postId);
            if (post == null) return false;
            post.Status = PostStatus.Rejected;
            await _context.SaveChangesAsync();
            return true;
        }

        // --- DOCTOR DOCUMENT MANAGEMENT ---

        public async Task<IEnumerable<DoctorDocumentAdminViewDto>> GetPendingDoctorDocumentsAsync()
        {
            return await _context.DoctorDocuments
                .Where(d => d.Status == DocumentStatus.Pending)
                .Include(d => d.Doctor)
                .Select(d => new DoctorDocumentAdminViewDto
                {
                    Id = d.Id,
                    DoctorId = d.DoctorId,
                    DoctorName = d.Doctor.Name,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath,
                    Status = d.Status.ToString()
                }).ToListAsync();
        }

        public async Task<IEnumerable<DoctorDocumentAdminViewDto>> GetDocumentsByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorDocuments
                .Where(d => d.DoctorId == doctorId)
                .Include(d => d.Doctor)
                .Select(d => new DoctorDocumentAdminViewDto
                {
                    Id = d.Id,
                    DoctorId = d.DoctorId,
                    DoctorName = d.Doctor.Name,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath,
                    Status = d.Status.ToString()
                }).ToListAsync();
        }

        public async Task<bool> VerifyDoctorDocumentAsync(int documentId)
        {
            var document = await _context.DoctorDocuments.FindAsync(documentId);
            if (document == null) return false;

            // Mark document as verified
            document.Status = DocumentStatus.Verified;
            document.AdminNotes = null;

            // Find the user associated with this doctor document
            // This method *only* verifies the document, not promotes the role.
            // Role promotion is now handled by ApproveDoctorApplicationAndPromoteUserAsync
            
            // The original logic to promote user role here has been commented out
            // because the new method (ApproveDoctorApplicationAndPromoteUserAsync)
            // is designed to handle that. If you need a document verification
            // to also promote the role, you'd integrate the role change here again
            // or ensure this is called before the main approval.
            
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"VerifyDoctorDocumentAsync: Successfully updated document {document.Id} to {document.Status}.");
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"VerifyDoctorDocumentAsync: Concurrency error updating document {document.Id}.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"VerifyDoctorDocumentAsync: General error saving changes for document {document.Id}.");
                return false;
            }
        }

        public async Task<bool> RejectDoctorDocumentAsync(int documentId, string adminNotes)
        {
            var document = await _context.DoctorDocuments.FindAsync(documentId);
            if (document == null) return false;

            document.Status = DocumentStatus.Rejected;
            document.AdminNotes = adminNotes;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}