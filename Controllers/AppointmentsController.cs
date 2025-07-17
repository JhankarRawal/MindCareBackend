using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Most appointment actions require login
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IHttpContextAccessor _httpContextAccessor; // To get current user

        public AppointmentsController(IAppointmentService appointmentService, IHttpContextAccessor httpContextAccessor)
        {
            _appointmentService = appointmentService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }
        private UserRole GetCurrentUserRole()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, out var role))
            {
                throw new UnauthorizedAccessException("User role not found in token.");
            }
            return role;
        }


        // POST: api/appointments (User books an appointment)
        [HttpPost]
        [Authorize(Roles = "User")] // Only regular users can book for themselves
        public async Task<ActionResult<AppointmentDto>> BookAppointment(CreateAppointmentDto createDto)
        {
            var currentUserId = GetCurrentUserId();
            if (createDto.UserId != currentUserId)
            {
                // User can only book for themselves using this endpoint.
                // Admins might use a different endpoint or have special logic.
                return Forbid("You can only book appointments for yourself.");
            }

            var appointment = await _appointmentService.CreateAppointmentAsync(createDto, currentUserId);
            if (appointment == null)
            {
                // Service layer should ideally return more specific errors
                return BadRequest("Could not create appointment. Slot may be unavailable, doctor not found, or user has existing pending appointment.");
            }
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id }, appointment);
        }

        // GET: api/appointments/{id} (User, Doctor associated, or Admin can get)
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointmentById(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Authorization: User can see their own, Doctor their own, Admin anyone's
            if (currentUserRole == UserRole.Admin ||
                (currentUserRole == UserRole.User && appointment.UserId == currentUserId) ||
                (currentUserRole == UserRole.Doctor && appointment.DoctorId == (await GetDoctorIdForCurrentUserAsync(currentUserId)))) // Assuming doctor role maps to a doctor entity
            {
                return Ok(appointment);
            }
            return Forbid();
        }

        // GET: api/appointments/user (current user's appointments)
        [HttpGet("user")] // No ID needed, takes from token
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetUserAppointments()
        {
            var userId = GetCurrentUserId();
            var appointments = await _appointmentService.GetUserAppointmentsAsync(userId);
            return Ok(appointments);
        }

        // GET: api/appointments/doctor (current doctor's appointments)
        [HttpGet("doctor")] // No ID needed, takes from token if doctor is a user
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetDoctorAppointments()
        {
            var doctorUserId = GetCurrentUserId();
            // Need to map doctorUserId (from User table) to DoctorId (from Doctor table)
            var doctorId = await GetDoctorIdForCurrentUserAsync(doctorUserId);
            if (!doctorId.HasValue) return NotFound("Doctor profile not found for current user.");

            var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId.Value);
            return Ok(appointments);
        }

        // Helper to get DoctorId from current logged-in User (if they are a doctor)
        // This would ideally be in a DoctorService or cached
        private async Task<int?> GetDoctorIdForCurrentUserAsync(int userId)
        {
            // Assuming IAppointmentService has access to DbContext or there's a dedicated DoctorService
            // This is a simplification; you might have a more direct way if DoctorService exists.
            var doctor = await ((AppointmentService)_appointmentService) // Casting for direct context access, not ideal
                               ._context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            return doctor?.Id;
        }


        // PUT: api/appointments/{id}/reschedule (User reschedules their appointment)
        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "User,Admin")] // User or Admin can reschedule
        public async Task<IActionResult> RescheduleAppointment(int id, RescheduleAppointmentDto rescheduleDto)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var success = await _appointmentService.RescheduleAppointmentAsync(id, rescheduleDto, currentUserId, currentUserRole);
            if (!success) return BadRequest("Failed to reschedule. Appointment not found, slot unavailable, or unauthorized.");
            return NoContent();
        }

        // PUT: api/appointments/{id}/cancel (User cancels their appointment)
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "User,Doctor,Admin")] // User, Doctor, or Admin can cancel
        public async Task<IActionResult> CancelAppointment(int id, [FromBody] UpdateAppointmentStatusDto? dto) // DTO for optional notes
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            AppointmentStatus statusToSet;

            switch (currentUserRole)
            {
                case UserRole.User:
                    statusToSet = AppointmentStatus.CancelledByUser;
                    break;
                case UserRole.Doctor:
                    statusToSet = AppointmentStatus.CancelledByDoctor;
                    break;
                case UserRole.Admin:
                    // Admin needs to specify which type of cancellation if not generic "Cancelled"
                    // For simplicity, assume admin uses a specific status or a generic one
                    // If dto.Status is provided by admin, use it. Otherwise, default.
                    statusToSet = dto?.Status == AppointmentStatus.CancelledByDoctor || dto?.Status == AppointmentStatus.CancelledByUser
                                  ? dto.Status
                                  : AppointmentStatus.CancelledByDoctor; // Default admin cancellation
                    break;
                default:
                    return Forbid();
            }

            var success = await _appointmentService.UpdateAppointmentStatusAsync(id, statusToSet, dto?.Notes, currentUserId, currentUserRole);
            if (!success) return BadRequest("Failed to cancel. Appointment not found or unauthorized.");
            return NoContent();
        }

        // PUT: api/appointments/{id}/complete (Doctor or Admin marks as completed)
        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> CompleteAppointment(int id, [FromBody] UpdateAppointmentStatusDto? dto) // DTO for doctor notes
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var success = await _appointmentService.UpdateAppointmentStatusAsync(id, AppointmentStatus.Completed, dto?.Notes, currentUserId, currentUserRole);
            if (!success) return BadRequest("Failed to complete. Appointment not found or unauthorized.");
            return NoContent();
        }
    }
}