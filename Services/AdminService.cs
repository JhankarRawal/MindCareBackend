using MentalHealthApis.Data;
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MentalHealthApis.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentService _appointmentService;

        public AdminService(ApplicationDbContext context, IAppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
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
                Role = u.Role
            }).ToListAsync();
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, UserRole newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Role = newRole;
            _context.Entry(user).State = EntityState.Modified;
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
                UserId = d.UserId
            }).ToListAsync();
        }

        // --- Appointments ---
        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            return await _appointmentService.GetAllAppointmentsAsync();
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
                    Status = a.Status // assuming AppointmentDto.Status is enum too
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
       



    }
}
