using MentalHealthApis.Data;
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // For HttpContextAccessor

namespace MentalHealthApis.Services
{
    

    public class AppointmentService : IAppointmentService
    {
        public readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AppointmentService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AppointmentDto?> CreateAppointmentAsync(CreateAppointmentDto createDto, int bookingUserId)
        {
            // 1. Validate User: Check if user already has a pending appointment
            var hasPendingAppointment = await _context.Appointments
                .AnyAsync(a => a.UserId == bookingUserId && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));
            if (hasPendingAppointment)
            {
                // Optionally throw a custom exception or return a specific error code/message
                // For simplicity, returning null here. Controller should handle this.
                Console.WriteLine("User already has a pending or confirmed appointment.");
                return null;
            }

            // 2. Validate Doctor exists
            var doctor = await _context.Doctors.FindAsync(createDto.DoctorId);
            if (doctor == null)
            {
                Console.WriteLine($"Doctor with ID {createDto.DoctorId} not found.");
                return null;
            }

            // 3. Validate Availability
            // Option A: Check against DoctorAvailability slots
            DoctorAvailability? availabilitySlot = null;
            if (createDto.DoctorAvailabilitySlotId.HasValue)
            {
                availabilitySlot = await _context.DoctorAvailabilities
                    .FirstOrDefaultAsync(da => da.Id == createDto.DoctorAvailabilitySlotId.Value &&
                                                da.DoctorId == createDto.DoctorId &&
                                                !da.IsBooked &&
                                                da.StartTime == createDto.AppointmentDateTime); // Ensure DTO matches slot
                if (availabilitySlot == null)
                {
                    Console.WriteLine("Selected availability slot is not valid or already booked.");
                    return null;
                }
            }
            else // Option B: Dynamic check (if not using explicit slots)
            {
                var appointmentEndTime = createDto.AppointmentDateTime.AddMinutes(createDto.DurationMinutes);
                var isSlotTaken = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == createDto.DoctorId &&
                                   a.Status != AppointmentStatus.CancelledByDoctor &&
                                   a.Status != AppointmentStatus.CancelledByUser &&
                                   a.AppointmentDateTime < appointmentEndTime &&
                                   a.AppointmentDateTime.AddMinutes(a.DurationMinutes) > createDto.AppointmentDateTime);
                if (isSlotTaken)
                {
                    Console.WriteLine("Doctor is not available at the selected time.");
                    return null;
                }
            }

            var appointment = new Appointment
            {
                UserId = bookingUserId,
                DoctorId = createDto.DoctorId,
                AppointmentDateTime = createDto.AppointmentDateTime,
                DurationMinutes = createDto.DurationMinutes,
                UserNotes = createDto.UserNotes,
                Status = AppointmentStatus.Pending, // Or Confirmed if no further doctor action needed
                DoctorAvailabilitySlotId = availabilitySlot?.Id
            };

            _context.Appointments.Add(appointment);

            if (availabilitySlot != null)
            {
                availabilitySlot.IsBooked = true;
                _context.DoctorAvailabilities.Update(availabilitySlot);
            }

            await _context.SaveChangesAsync();

            // Fetch related data for DTO
            var user = await _context.Users.FindAsync(bookingUserId);
            // Doctor already fetched

            return new AppointmentDto
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                UserName = user?.Name ?? "N/A",
                DoctorId = appointment.DoctorId,
                DoctorName = doctor.Name,
                AppointmentDateTime = appointment.AppointmentDateTime,
                DurationMinutes = appointment.DurationMinutes,
                Status = appointment.Status,
                UserNotes = appointment.UserNotes,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }

        public async Task<IEnumerable<AppointmentDto>> GetUserAppointmentsAsync(int userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Doctor) // Include doctor details
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User!.Name, // User will be loaded by EF due to UserId filter or an Include(a => a.User)
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor!.Name,
                    AppointmentDateTime = a.AppointmentDateTime,
                    DurationMinutes = a.DurationMinutes,
                    Status = a.Status,
                    UserNotes = a.UserNotes,
                    DoctorNotes = a.DoctorNotes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(int doctorId)
        {
            return await _context.Appointments
               .Where(a => a.DoctorId == doctorId)
               .Include(a => a.User)
               .Select(a => new AppointmentDto
               {
                   Id = a.Id,
                   UserId = a.UserId,
                   UserName = a.User!.Name,
                   DoctorId = a.DoctorId,
                   DoctorName = a.Doctor!.Name,  // Could get from Doctor directly if we also .Include(a => a.Doctor)
                   AppointmentDateTime = a.AppointmentDateTime,
                   DurationMinutes = a.DurationMinutes,
                   Status = a.Status,
                   UserNotes = a.UserNotes,
                   DoctorNotes = a.DoctorNotes,
                   CreatedAt = a.CreatedAt,
                   UpdatedAt = a.UpdatedAt
               })
               .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync() // Admin
        {
            return await _context.Appointments
               .Include(a => a.User)
               .Include(a => a.Doctor)
               .Select(a => new AppointmentDto
               {
                   Id = a.Id,
                   UserId = a.UserId,
                   UserName = a.User!.Name,
                   DoctorId = a.DoctorId,
                   DoctorName = a.Doctor!.Name,
                   AppointmentDateTime = a.AppointmentDateTime,
                   DurationMinutes = a.DurationMinutes,
                   Status = a.Status,
                   UserNotes = a.UserNotes,
                   DoctorNotes = a.DoctorNotes,
                   CreatedAt = a.CreatedAt,
                   UpdatedAt = a.UpdatedAt
               })
               .ToListAsync();
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return null;

            return new AppointmentDto
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                UserName = appointment.User!.Name,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor!.Name,
                AppointmentDateTime = appointment.AppointmentDateTime,
                DurationMinutes = appointment.DurationMinutes,
                Status = appointment.Status,
                UserNotes = appointment.UserNotes,
                DoctorNotes = appointment.DoctorNotes,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }


        public async Task<bool> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentDto rescheduleDto, int currentUserId, UserRole currentUserRole)
        {
            var appointment = await _context.Appointments
                                    .Include(a => a.DoctorAvailabilitySlot) // Include the old slot
                                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return false;

            // Authorization: User can only reschedule their own appointments. Admin can reschedule any.
            if (currentUserRole != UserRole.Admin && appointment.UserId != currentUserId)
            {
                return false; // Unauthorized
            }

            // Cannot reschedule completed or cancelled appointments
            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.CancelledByUser ||
                appointment.Status == AppointmentStatus.CancelledByDoctor)
            {
                return false; // Invalid state
            }

            // Validate new slot (similar to CreateAppointmentAsync logic)
            // For brevity, assuming new slot is validated outside or simplified check here
            var newAppointmentEndTime = rescheduleDto.NewAppointmentDateTime.AddMinutes(appointment.DurationMinutes);
            var isNewSlotTaken = await _context.Appointments
                .AnyAsync(a => a.Id != appointmentId && // Exclude current appointment
                               a.DoctorId == appointment.DoctorId &&
                               a.Status != AppointmentStatus.CancelledByDoctor &&
                               a.Status != AppointmentStatus.CancelledByUser &&
                               a.AppointmentDateTime < newAppointmentEndTime &&
                               a.AppointmentDateTime.AddMinutes(a.DurationMinutes) > rescheduleDto.NewAppointmentDateTime);

            if (isNewSlotTaken) return false; // New slot conflict

            // If using DoctorAvailability slots:
            DoctorAvailability? newAvailabilitySlot = null;
            if (rescheduleDto.NewDoctorAvailabilitySlotId.HasValue)
            {
                newAvailabilitySlot = await _context.DoctorAvailabilities
                    .FirstOrDefaultAsync(da => da.Id == rescheduleDto.NewDoctorAvailabilitySlotId.Value &&
                                                da.DoctorId == appointment.DoctorId &&
                                                !da.IsBooked &&
                                                da.StartTime == rescheduleDto.NewAppointmentDateTime);
                if (newAvailabilitySlot == null) return false; // New slot not found or booked
            }


            // Free up old slot if it was linked
            if (appointment.DoctorAvailabilitySlot != null)
            {
                appointment.DoctorAvailabilitySlot.IsBooked = false;
                _context.DoctorAvailabilities.Update(appointment.DoctorAvailabilitySlot);
            }

            // Update appointment
            appointment.AppointmentDateTime = rescheduleDto.NewAppointmentDateTime;
            appointment.Status = AppointmentStatus.Rescheduled; // Or Pending/Confirmed
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.DoctorAvailabilitySlotId = newAvailabilitySlot?.Id; // Link to new slot

            if (newAvailabilitySlot != null)
            {
                newAvailabilitySlot.IsBooked = true;
                _context.DoctorAvailabilities.Update(newAvailabilitySlot);
            }

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus newStatus, string? notes, int currentUserId, UserRole currentUserRole)
        {
            var appointment = await _context.Appointments
                                    .Include(a => a.DoctorAvailabilitySlot)
                                    .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) return false;

            // Authorization checks:
            // User can cancel their own appointments (if status is Pending/Confirmed/Rescheduled)
            // Doctor can cancel/complete appointments assigned to them
            // Admin can do anything
            bool isAuthorized = currentUserRole == UserRole.Admin ||
                               (currentUserRole == UserRole.User && appointment.UserId == currentUserId &&
                                (newStatus == AppointmentStatus.CancelledByUser) &&
                                (appointment.Status == AppointmentStatus.Pending || appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Rescheduled)) ||
                               (currentUserRole == UserRole.Doctor && appointment.Doctor != null && appointment.Doctor.UserId == currentUserId && // Assuming Doctor has a UserId link
                                (newStatus == AppointmentStatus.Completed || newStatus == AppointmentStatus.CancelledByDoctor));

            if (!isAuthorized) return false;

            // Logic for freeing up slot on cancellation
            if (newStatus == AppointmentStatus.CancelledByUser || newStatus == AppointmentStatus.CancelledByDoctor)
            {
                if (appointment.DoctorAvailabilitySlot != null)
                {
                    appointment.DoctorAvailabilitySlot.IsBooked = false;
                    _context.DoctorAvailabilities.Update(appointment.DoctorAvailabilitySlot);
                }
            }

            appointment.Status = newStatus;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (newStatus == AppointmentStatus.Completed && currentUserRole == UserRole.Doctor && !string.IsNullOrWhiteSpace(notes))
            {
                appointment.DoctorNotes = notes;
            }
            else if ((newStatus == AppointmentStatus.CancelledByUser || newStatus == AppointmentStatus.CancelledByDoctor) && !string.IsNullOrWhiteSpace(notes))
            {
                // Could have a generic "CancellationReason" field or use UserNotes/DoctorNotes
                if (currentUserRole == UserRole.User) appointment.UserNotes = $"Cancellation: {notes}";
                else appointment.DoctorNotes = $"Cancellation: {notes}";
            }

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}