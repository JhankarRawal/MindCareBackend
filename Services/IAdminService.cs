using MentalHealthApis.DTOs;
using MentalHealthApis.Models; // Ensure this is included for UserRole enum

namespace MentalHealthApis.Services.Interfaces
{
    public interface IAdminService
    {
        // Users
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(int userId, UserRole newRole);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ReactivateUserAsync(int userId);

        // Doctors
        Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();
        Task<bool> UpdateDoctorApplicationStatusAsync(int doctorId, string status, string? notes);
        // Add the new method to the interface
        Task<bool> ApproveDoctorApplicationAndPromoteUserAsync(int doctorId);


        // Appointments
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByStatusAsync(string status);
        Task<bool> CancelAppointmentAsync(int appointmentId);


        // Doctor Availability
        Task<DoctorAvailabilityDto?> SetDoctorAvailabilityAsync(CreateDoctorAvailabilityDto dto);
        Task<bool> DeleteDoctorAvailabilityAsync(int doctorId, int availabilityId);

        // Blog Posts
        Task<IEnumerable<BlogPostAdminDto>> GetAllBlogPostsAsync();
        Task<bool> ApproveBlogPostAsync(int postId);
        Task<bool> RejectBlogPostAsync(int postId);

        // Doctor Document Management
        Task<IEnumerable<DoctorDocumentAdminViewDto>> GetPendingDoctorDocumentsAsync();
        Task<IEnumerable<DoctorDocumentAdminViewDto>> GetDocumentsByDoctorIdAsync(int doctorId);
        Task<bool> VerifyDoctorDocumentAsync(int documentId);
        Task<bool> RejectDoctorDocumentAsync(int documentId, string adminNotes);
    }
}