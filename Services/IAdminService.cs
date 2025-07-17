using MentalHealthApis.DTOs;
using MentalHealthApis.Models;

namespace MentalHealthApis.Services.Interfaces
{
    public interface IAdminService
    {
        // User
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(int userId, UserRole newRole);

        // Doctor
        Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();

        // Appointment
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();

        // Doctor Availability
        Task<DoctorAvailabilityDto?> SetDoctorAvailabilityAsync(CreateDoctorAvailabilityDto dto);
        Task<bool> DeleteDoctorAvailabilityAsync(int doctorId, int availabilityId);
        Task<IEnumerable<BlogPostAdminDto>> GetAllBlogPostsAsync();
        Task<bool> ApproveBlogPostAsync(int postId);
        Task<bool> RejectBlogPostAsync(int postId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ReactivateUserAsync(int userId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByStatusAsync(string status);
        Task<bool> CancelAppointmentAsync(int appointmentId);



    }
}
