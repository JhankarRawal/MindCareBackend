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
        private readonly IDoctorService _doctorService; // Inject DoctorService for mapping User to Doctor ID

        public AppointmentsController(IAppointmentService appointmentService, IHttpContextAccessor httpContextAccessor, IDoctorService doctorService)
        {
            _appointmentService = appointmentService;
            _httpContextAccessor = httpContextAccessor;
            _doctorService = doctorService; // Initialize DoctorService
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
            // Removed createDto.UserId check here as the service layer implicitly uses currentUserId
            // If you want to allow a user to book for someone else (e.g., admin), this endpoint needs modification
            // For now, it's assumed current user is booking for themselves.

            var appointment = await _appointmentService.CreateAppointmentAsync(createDto, currentUserId);
            if (appointment == null)
            {
                // Service layer should ideally return more specific errors
                return BadRequest("Could not create appointment. Slot may be unavailable, doctor not found, or user has existing pending/confirmed appointment.");
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

            // Get the Doctor ID for the current user if they are a Doctor
            int? currentDoctorId = null;
            if (currentUserRole == UserRole.Doctor)
            {
                currentDoctorId = await _doctorService.GetDoctorProfileIdByUserIdAsync(currentUserId);
            }

            // Authorization: User can see their own, Doctor their own, Admin anyone's
            if (currentUserRole == UserRole.Admin ||
                (currentUserRole == UserRole.User && appointment.UserId == currentUserId) ||
                (currentUserRole == UserRole.Doctor && appointment.DoctorId == currentDoctorId))
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
            // Use the DoctorService to get the DoctorId
            var doctorId = await _doctorService.GetDoctorProfileIdByUserIdAsync(doctorUserId);
            if (!doctorId.HasValue) return NotFound("Doctor profile not found for current user.");

            var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId.Value);
            return Ok(appointments);
        }

        // Helper to get DoctorId from current logged-in User (if they are a doctor)
        // This helper is now simpler as it uses the injected IDoctorService
        // private async Task<int?> GetDoctorIdForCurrentUserAsync(int userId)
        // {
        //     return await _doctorService.GetDoctorProfileIdByUserIdAsync(userId);
        // }


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

        // New: PUT: api/appointments/{id}/accept (Doctor accepts a pending appointment)
        [HttpPut("{id}/accept")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> AcceptAppointment(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var success = await _appointmentService.AcceptAppointmentAsync(id, currentUserId, currentUserRole);
            if (!success) return BadRequest("Failed to accept appointment. Appointment not found, not pending, or unauthorized.");
            return NoContent();
        }

        // New: PUT: api/appointments/{id}/reject (Doctor rejects a pending appointment)
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> RejectAppointment(int id, [FromBody] RejectAppointmentDto rejectDto) // DTO to capture doctor's notes
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var success = await _appointmentService.RejectAppointmentAsync(id, rejectDto.DoctorNotes, currentUserId, currentUserRole);
            if (!success) return BadRequest("Failed to reject appointment. Appointment not found, not pending, or unauthorized.");
            return NoContent();
        }
    }

    // New DTO for RejectAppointment to get Doctor's notes
    public class RejectAppointmentDto
    {
        public string DoctorNotes { get; set; } = string.Empty;
    }
}