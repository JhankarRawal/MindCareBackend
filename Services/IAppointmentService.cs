using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public interface IAppointmentService
    {
        Task<AppointmentDto?> CreateAppointmentAsync(CreateAppointmentDto createDto, int bookingUserId);
        Task<IEnumerable<AppointmentDto>> GetUserAppointmentsAsync(int userId);
        Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(int doctorId);
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto?> GetAppointmentByIdAsync(int appointmentId);
        Task<bool> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentDto rescheduleDto, int currentUserId, UserRole currentUserRole);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus newStatus, string? notes, int currentUserId, UserRole currentUserRole);
        
        // New methods for accepting/rejecting appointments
        Task<bool> AcceptAppointmentAsync(int appointmentId, int currentUserId, UserRole currentUserRole);
        Task<bool> RejectAppointmentAsync(int appointmentId, string doctorNotes, int currentUserId, UserRole currentUserRole);
    }
}