using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();
        Task<DoctorDto?> GetDoctorByIdAsync(int id);
        Task<DoctorDto?> CreateDoctorAsync(CreateDoctorDto createDoctorDto); // Admin
        Task<DoctorDto?> UpdateDoctorAsync(int id, UpdateDoctorDto updateDoctorDto, int currentUserId, UserRole currentUserRole); // Admin or self
        Task<bool> DeleteDoctorAsync(int id); // Admin

        Task<IEnumerable<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(int doctorId, DateTime? date);
        Task<DoctorAvailabilityDto?> SetDoctorAvailabilityAsync(CreateDoctorAvailabilityDto createDto, int currentUserId, UserRole currentUserRole);
        Task<bool> DeleteDoctorAvailabilityAsync(int doctorId, int availabilityId, int currentUserId, UserRole currentUserRole);
        Task<int?> GetDoctorProfileIdByUserIdAsync(int userId); // Helper
    }
}