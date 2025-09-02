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

        

       // In MentalHealthApis.Services/DoctorService.cs

public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync()
{
    var doctors = await _context.Doctors
        .Include(d => d.Documents) // <--- Correct: Loads the documents
        .ToListAsync();

    return doctors.Select(d => MapDoctorToDto(d));
}

public async Task<DoctorDto?> GetDoctorByIdAsync(int id)
{
    var doctor = await _context.Doctors
        .Include(d => d.Documents) // <--- Correct: Loads the documents
        .FirstOrDefaultAsync(d => d.Id == id); // <--- Use FirstOrDefaultAsync with Include

    return doctor == null ? null : MapDoctorToDto(doctor);
}

public async Task<DoctorDto?> UpdateDoctorAsync(int id, UpdateDoctorDto updateDoctorDto, int currentUserId, UserRole currentUserRole)
{
    var doctor = await _context.Doctors
        .Include(d => d.Documents) // <--- Correct: Loads the documents for the updated DTO
        .FirstOrDefaultAsync(d => d.Id == id); // <--- Use FirstOrDefaultAsync with Include

    if (doctor == null) return null;
    // ... rest of the update logic ...
    _context.Doctors.Update(doctor);
    await _context.SaveChangesAsync();
    return MapDoctorToDto(doctor);
}

        public async Task<DoctorDto?> CreateDoctorAsync(CreateDoctorDto createDoctorDto)
        {
            if (createDoctorDto.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(createDoctorDto.UserId.Value);
                if (user == null || user.Role != UserRole.Doctor)
                {
                    return null; // "Associated user not found or is not a Doctor role."
                }
            }

            var doctor = new Doctor
            {
                Name = createDoctorDto.Name,
                Specialization = createDoctorDto.Specialization,
                ContactInfo = createDoctorDto.ContactInfo,
                UserId = createDoctorDto.UserId,
                ApplicationStatus = createDoctorDto.ApplicationStatus ?? "Pending", // Ensure status is set
                HasAcceptedTerms = false // Default for a new doctor
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // ProfileImageUrl will be null at this point as the photo is uploaded separately
            return MapDoctorToDto(doctor);
        }

       

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

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
            if (doctor == null) return null;

            bool isAuthorized = currentUserRole == UserRole.Admin ||
                                (currentUserRole == UserRole.Doctor && doctor.UserId.HasValue && doctor.UserId.Value == currentUserId);
            if (!isAuthorized) return null;

            if (createDto.StartTime >= createDto.EndTime || createDto.StartTime <= DateTime.UtcNow)
            {
                return null;
            }

            var overlaps = await _context.DoctorAvailabilities
                .AnyAsync(da => da.DoctorId == createDto.DoctorId &&
                                da.StartTime < createDto.EndTime &&
                                da.EndTime > createDto.StartTime);
            if (overlaps)
            {
                return null;
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
            if (availability == null) return false;

            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return false;

            bool isAuthorized = currentUserRole == UserRole.Admin ||
                                (currentUserRole == UserRole.Doctor && doctor.UserId.HasValue && doctor.UserId.Value == currentUserId);
            if (!isAuthorized) return false;

            if (availability.IsBooked)
            {
                return false;
            }

            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetDoctorProfileIdByUserIdAsync(int userId)
        {
            var doctor = await _context.Doctors
                                .AsNoTracking()
                                .FirstOrDefaultAsync(d => d.UserId == userId);
            return doctor?.Id;
        }

        // The single, comprehensive MapDoctorToDto method
       // In MentalHealthApis.Services/DoctorService.cs

private DoctorDto MapDoctorToDto(Doctor doctor)
{
    var profilePhotoPath = doctor.Documents?
                                .FirstOrDefault(doc => doc.DocumentType == "PasswordSizedPhoto")
                                ?.FilePath;

    return new DoctorDto
    {
        Id = doctor.Id,
        Name = doctor.Name,
        Specialization = doctor.Specialization,
        ContactInfo = doctor.ContactInfo,
        UserId = doctor.UserId,
        ApplicationStatus = doctor.ApplicationStatus, // <--- Make sure this line is there!
        ProfileImageUrl = profilePhotoPath
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