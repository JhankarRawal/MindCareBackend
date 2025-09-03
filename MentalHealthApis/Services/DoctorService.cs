using MentalHealthApis.Data;
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;

        public DoctorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync()
        {
            return await _context.Doctors
                .Select(d => MapDoctorToDto(d))
                .ToListAsync();
        }

        public async Task<DoctorDto?> GetDoctorByIdAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            return doctor == null ? null : MapDoctorToDto(doctor);
        }

        public async Task<DoctorDto?> CreateDoctorAsync(CreateDoctorDto createDoctorDto)
        {
            // Optional: Check if UserId exists and has Doctor role
            if (createDoctorDto.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(createDoctorDto.UserId.Value);
                if (user == null || user.Role != UserRole.Doctor)
                {
                    // Consider throwing a specific exception or returning a result object
                    return null; // "Associated user not found or is not a Doctor role."
                }
            }

            var doctor = new Doctor
            {
                Name = createDoctorDto.Name,
                Specialization = createDoctorDto.Specialization,
                ContactInfo = createDoctorDto.ContactInfo,
                UserId = createDoctorDto.UserId
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return MapDoctorToDto(doctor);
        }

        public async Task<DoctorDto?> UpdateDoctorAsync(int id, UpdateDoctorDto updateDoctorDto, int currentUserId, UserRole currentUserRole)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return null;

            // Authorization: Admin can update any, Doctor can update their own profile
            bool isAuthorized = currentUserRole == UserRole.Admin ||
                                (currentUserRole == UserRole.Doctor && doctor.UserId.HasValue && doctor.UserId.Value == currentUserId);

            if (!isAuthorized)
            {
                // Or throw an UnauthorizedAccessException
                return null; // Not authorized
            }

            if (!string.IsNullOrEmpty(updateDoctorDto.Name)) doctor.Name = updateDoctorDto.Name;
            if (!string.IsNullOrEmpty(updateDoctorDto.Specialization)) doctor.Specialization = updateDoctorDto.Specialization;
            if (updateDoctorDto.ContactInfo != null) doctor.ContactInfo = updateDoctorDto.ContactInfo; // Allow clearing

            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();
            return MapDoctorToDto(doctor);
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            // This assumes Admin role. Controller should enforce this.
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

            // Consider implications: What happens to their appointments? Availability?
            // Might need more complex logic or disallow deletion if active associations exist.
            // For now, a simple delete:
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(int doctorId, DateTime? date)
        {
            var query = _context.DoctorAvailabilities
                .Where(da => da.DoctorId == doctorId && !da.IsBooked);

            if (date.HasValue)
            {
                query = query.Where(da => da.StartTime.Date == date.Value.Date);
            }
            else
            {
                query = query.Where(da => da.StartTime >= DateTime.UtcNow);
            }

            return await query
                .OrderBy(da => da.StartTime)
                .Select(da => MapAvailabilityToDto(da))
                .ToListAsync();
        }

        public async Task<DoctorAvailabilityDto?> SetDoctorAvailabilityAsync(CreateDoctorAvailabilityDto createDto, int currentUserId, UserRole currentUserRole)
        {
            var doctor = await _context.Doctors.FindAsync(createDto.DoctorId);
            if (doctor == null) return null; // "Doctor not found."

            // Authorization
            bool isAuthorized = currentUserRole == UserRole.Admin ||
                                (currentUserRole == UserRole.Doctor && doctor.UserId.HasValue && doctor.UserId.Value == currentUserId);
            if (!isAuthorized) return null; // "Not authorized to set availability for this doctor."

            if (createDto.StartTime >= createDto.EndTime || createDto.StartTime <= DateTime.UtcNow)
            {
                return null; // "Invalid start or end time."
            }

            var overlaps = await _context.DoctorAvailabilities
                .AnyAsync(da => da.DoctorId == createDto.DoctorId &&
                                da.StartTime < createDto.EndTime &&
                                da.EndTime > createDto.StartTime);
            if (overlaps)
            {
                return null; // "The proposed availability slot overlaps with an existing one."
            }

            var availability = new DoctorAvailability
            {
                DoctorId = createDto.DoctorId,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                IsBooked = false
            };

            _context.DoctorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();
            return MapAvailabilityToDto(availability);
        }

        public async Task<bool> DeleteDoctorAvailabilityAsync(int doctorId, int availabilityId, int currentUserId, UserRole currentUserRole)
        {
            var availability = await _context.DoctorAvailabilities
                                    .FirstOrDefaultAsync(da => da.Id == availabilityId && da.DoctorId == doctorId);
            if (availability == null) return false; // "Availability slot not found."

            var doctor = await _context.Doctors.FindAsync(doctorId); // Already know doctorId matches, but need for UserID check
            if (doctor == null) return false; // Should not happen if availability exists

            // Authorization
            bool isAuthorized = currentUserRole == UserRole.Admin ||
                                (currentUserRole == UserRole.Doctor && doctor.UserId.HasValue && doctor.UserId.Value == currentUserId);
            if (!isAuthorized) return false; // "Not authorized."

            if (availability.IsBooked)
            {
                return false; // "Cannot delete a booked slot. Cancel appointment first."
            }

            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetDoctorProfileIdByUserIdAsync(int userId)
        {
            var doctor = await _context.Doctors
                                .AsNoTracking() // No need to track for a simple lookup
                                .FirstOrDefaultAsync(d => d.UserId == userId);
            return doctor?.Id;
        }


        // Private mappers
        private static DoctorDto MapDoctorToDto(Doctor doctor)
        {
            return new DoctorDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Specialization = doctor.Specialization,
                ContactInfo = doctor.ContactInfo,
                UserId = doctor.UserId
            };
        }

        private static DoctorAvailabilityDto MapAvailabilityToDto(DoctorAvailability da)
        {
            return new DoctorAvailabilityDto
            {
                Id = da.Id,
                DoctorId = da.DoctorId,
                StartTime = da.StartTime,
                EndTime = da.EndTime,
                IsBooked = da.IsBooked
            };
        }
    }
}