using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET: api/doctors
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctors()
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();
            return Ok(doctors);
        }

        // GET: api/doctors/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        // POST: api/doctors (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DoctorDto>> CreateDoctor(CreateDoctorDto dto)
        {
            var doctor = await _doctorService.CreateDoctorAsync(dto);
            if (doctor == null) return BadRequest("User not found or not a Doctor.");
            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctor);
        }

        // PUT: api/doctors/{id} (Admin or self)
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DoctorDto>> UpdateDoctor(int id, UpdateDoctorDto dto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

            var doctor = await _doctorService.UpdateDoctorAsync(id, dto, currentUserId, currentUserRole);
            if (doctor == null) return Forbid();
            return Ok(doctor);
        }

        // DELETE: api/doctors/{id} (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var deleted = await _doctorService.DeleteDoctorAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // GET: api/doctors/{doctorId}/availability
        [HttpGet("{doctorId}/availability")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DoctorAvailabilityDto>>> GetDoctorAvailability(int doctorId, [FromQuery] DateTime? date)
        {
            var slots = await _doctorService.GetDoctorAvailabilityAsync(doctorId, date);
            return Ok(slots);
        }

        // POST: api/doctors/{doctorId}/availability
        [HttpPost("{doctorId}/availability")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DoctorAvailabilityDto>> SetDoctorAvailability(int doctorId, CreateDoctorAvailabilityDto dto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

            dto.DoctorId = doctorId; // ensure consistency

            var availability = await _doctorService.SetDoctorAvailabilityAsync(dto, currentUserId, currentUserRole);
            if (availability == null) return BadRequest("Invalid availability slot or not authorized.");
            return Ok(availability);
        }

        // DELETE: api/doctors/{doctorId}/availability/{availabilityId}
        [HttpDelete("{doctorId}/availability/{availabilityId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeleteDoctorAvailability(int doctorId, int availabilityId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

            var deleted = await _doctorService.DeleteDoctorAvailabilityAsync(doctorId, availabilityId, currentUserId, currentUserRole);
            if (!deleted) return BadRequest("Cannot delete this availability.");
            return NoContent();
        }
    }
}
