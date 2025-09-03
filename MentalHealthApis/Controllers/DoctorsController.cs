using MentalHealthApis.Data; // Or IDoctorService
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Replace with IDoctorService

        public DoctorsController(ApplicationDbContext context) // Inject IDoctorService
        {
            _context = context;
        }

        // GET: api/doctors
        [HttpGet]
        [AllowAnonymous] // Publicly list doctors
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctors()
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

        // GET: api/doctors/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            return new DoctorDto { Id = doctor.Id, Name = doctor.Name, Specialization = doctor.Specialization, ContactInfo = doctor.ContactInfo, UserId = doctor.UserId };
        }

        // POST: api/doctors (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DoctorDto>> CreateDoctor(CreateDoctorDto createDoctorDto)
        {
            // Optional: Check if UserId exists and has Doctor role
            if (createDoctorDto.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(createDoctorDto.UserId.Value);
                if (user == null || user.Role != UserRole.Doctor)
                {
                    return BadRequest("Associated user not found or is not a Doctor role.");
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

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id },
                new DoctorDto { Id = doctor.Id, Name = doctor.Name, Specialization = doctor.Specialization, ContactInfo = doctor.ContactInfo, UserId = doctor.UserId });
        }

        // --- Availability Endpoints ---

        // GET: api/doctors/{doctorId}/availability (For users to see available slots)
        [HttpGet("{doctorId}/availability")]
        [AllowAnonymous] // Or [Authorize] if only logged-in users can see
        public async Task<ActionResult<IEnumerable<DoctorAvailabilityDto>>> GetDoctorAvailability(int doctorId, [FromQuery] DateTime? date)
        {
            var query = _context.DoctorAvailabilities
                .Where(da => da.DoctorId == doctorId && !da.IsBooked);

            if (date.HasValue)
            {
                // Filter by specific date, ignoring time part for the whole day's slots
                query = query.Where(da => da.StartTime.Date == date.Value.Date);
            }
            else
            {
                // Default to future availability if no date specified
                query = query.Where(da => da.StartTime >= DateTime.UtcNow);
            }


            return await query.Select(da => new DoctorAvailabilityDto
            {
                Id = da.Id,
                DoctorId = da.DoctorId,
                StartTime = da.StartTime,
                EndTime = da.EndTime,
                IsBooked = da.IsBooked
            })
                .OrderBy(da => da.StartTime)
                .ToListAsync();
        }

        // POST: api/doctors/{doctorId}/availability (Doctor or Admin to set availability)
        [HttpPost("{doctorId}/availability")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DoctorAvailabilityDto>> SetDoctorAvailability(int doctorId, CreateDoctorAvailabilityDto createDto)
        {
            if (doctorId != createDto.DoctorId)
                return BadRequest("Doctor ID mismatch.");

            // Authorization: Ensure the logged-in doctor is setting their own availability, or it's an admin
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return NotFound("Doctor not found.");

            if (currentUserRole != UserRole.Admin.ToString() && (!doctor.UserId.HasValue || doctor.UserId.Value != currentUserId))
            {
                return Forbid("You are not authorized to set availability for this doctor.");
            }

            // Validate StartTime < EndTime and StartTime is in the future
            if (createDto.StartTime >= createDto.EndTime || createDto.StartTime <= DateTime.UtcNow)
            {
                return BadRequest("Invalid start or end time for availability slot.");
            }

            // Check for overlapping availability slots for the same doctor
            var overlaps = await _context.DoctorAvailabilities
                .AnyAsync(da => da.DoctorId == doctorId &&
                                da.StartTime < createDto.EndTime &&
                                da.EndTime > createDto.StartTime);
            if (overlaps)
            {
                return Conflict("The proposed availability slot overlaps with an existing one.");
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

            return CreatedAtAction(nameof(GetDoctorAvailability), new { doctorId = availability.DoctorId },
                new DoctorAvailabilityDto { Id = availability.Id, DoctorId = availability.DoctorId, StartTime = availability.StartTime, EndTime = availability.EndTime, IsBooked = availability.IsBooked });
        }

        // DELETE: api/doctors/{doctorId}/availability/{availabilityId} (Doctor or Admin to remove a slot)
        [HttpDelete("{doctorId}/availability/{availabilityId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeleteDoctorAvailability(int doctorId, int availabilityId)
        {
            var availability = await _context.DoctorAvailabilities.FindAsync(availabilityId);
            if (availability == null || availability.DoctorId != doctorId)
            {
                return NotFound("Availability slot not found for this doctor.");
            }

            if (availability.IsBooked)
            {
                return BadRequest("Cannot delete an availability slot that is already booked. Cancel the appointment first.");
            }

            // Authorization: Ensure doctor is deleting their own, or admin
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return NotFound("Doctor not found.");


            if (currentUserRole != UserRole.Admin.ToString() && (!doctor.UserId.HasValue || doctor.UserId.Value != currentUserId))
            {
                return Forbid("You are not authorized to delete this availability slot.");
            }

            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}