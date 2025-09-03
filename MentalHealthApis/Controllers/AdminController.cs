using MentalHealthApis.Data; // Or relevant services
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services; // For IAppointmentService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // All admin endpoints require Admin role
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Direct context for simplicity here
        private readonly IAppointmentService _appointmentService;

        public AdminController(ApplicationDbContext context, IAppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
        }

        // --- User Management ---
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            return await _context.Users
                .Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email, PhoneNumber = u.PhoneNumber, Role = u.Role })
                .ToListAsync();
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UserRole newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found.");

            // Prevent admin from accidentally removing their own admin role if they are the last one (complex logic, omit for now)
            // Be careful with this endpoint.
            user.Role = newRole;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }


        // --- Doctor Management (some might be in DoctorsController with Admin role check) ---
        [HttpGet("doctors")]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAllDoctors()
        {
            return await _context.Doctors
               .Select(d => new DoctorDto
               {
                   Id = d.Id,
                   Name = d.Name,
                   Specialization = d.Specialization,
                   ContactInfo = d.ContactInfo,
                   UserId = d.UserId
               }).ToListAsync();
        }


        // --- Appointment Management ---
        [HttpGet("appointments")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAllAppointments()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            return Ok(appointments);
        }

        // Admin can manage Doctor Availability (could also be in DoctorsController with Admin checks)
        // POST: api/admin/doctors/{doctorId}/availability
        [HttpPost("doctors/{doctorId}/availability")]
        public async Task<ActionResult<DoctorAvailabilityDto>> AdminSetDoctorAvailability(int doctorId, CreateDoctorAvailabilityDto createDto)
        {
            if (doctorId != createDto.DoctorId)
                return BadRequest("Doctor ID mismatch.");

            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return NotFound("Doctor not found.");

            if (createDto.StartTime >= createDto.EndTime || createDto.StartTime <= DateTime.UtcNow)
                return BadRequest("Invalid start or end time.");

            var overlaps = await _context.DoctorAvailabilities
                .AnyAsync(da => da.DoctorId == doctorId &&
                                da.StartTime < createDto.EndTime &&
                                da.EndTime > createDto.StartTime);
            if (overlaps) return Conflict("The proposed availability slot overlaps with an existing one.");

            var availability = new DoctorAvailability
            {
                DoctorId = createDto.DoctorId,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime
            };
            _context.DoctorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            return Ok(new DoctorAvailabilityDto { Id = availability.Id, DoctorId = availability.DoctorId, StartTime = availability.StartTime, EndTime = availability.EndTime, IsBooked = availability.IsBooked });
        }

        // DELETE: api/admin/doctors/{doctorId}/availability/{availabilityId}
        [HttpDelete("doctors/{doctorId}/availability/{availabilityId}")]
        public async Task<IActionResult> AdminDeleteDoctorAvailability(int doctorId, int availabilityId)
        {
            var availability = await _context.DoctorAvailabilities.FindAsync(availabilityId);
            if (availability == null || availability.DoctorId != doctorId) return NotFound();

            if (availability.IsBooked)
            {
                return BadRequest("Cannot delete a booked slot. Cancel the appointment first.");
            }

            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}