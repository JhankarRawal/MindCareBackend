using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // --- USER MANAGEMENT ---

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            return Ok(await _adminService.GetAllUsersAsync());
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UserRole newRole)
        {
            var result = await _adminService.UpdateUserRoleAsync(id, newRole);
            return result ? NoContent() : NotFound("User not found.");
        }

        [HttpPut("users/{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var result = await _adminService.DeactivateUserAsync(id);
            return result ? NoContent() : NotFound("User not found.");
        }

        [HttpPut("users/{id}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var result = await _adminService.ReactivateUserAsync(id);
            return result ? NoContent() : NotFound("User not found.");
        }

        // --- DOCTOR MANAGEMENT ---

        [HttpGet("doctors")]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAllDoctors()
        {
            return Ok(await _adminService.GetAllDoctorsAsync());
        }

        // --- APPOINTMENT MANAGEMENT ---

        [HttpGet("appointments")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAllAppointments()
        {
            return Ok(await _adminService.GetAllAppointmentsAsync());
        }

        [HttpGet("appointments/status/{status}")]
        public async Task<IActionResult> GetAppointmentsByStatus(string status)
        {
            var appointments = await _adminService.GetAppointmentsByStatusAsync(status);
            return Ok(appointments);
        }

        [HttpPut("appointments/{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var result = await _adminService.CancelAppointmentAsync(id);
            return result ? NoContent() : NotFound("Appointment not found.");
        }

        // --- DOCTOR AVAILABILITY ---

        [HttpPost("doctors/{doctorId}/availability")]
        public async Task<ActionResult<DoctorAvailabilityDto>> AdminSetDoctorAvailability(int doctorId, CreateDoctorAvailabilityDto createDto)
        {
            if (doctorId != createDto.DoctorId)
                return BadRequest("Doctor ID mismatch.");

            var result = await _adminService.SetDoctorAvailabilityAsync(createDto);
            return result == null
                ? Conflict("Invalid or overlapping availability slot.")
                : Ok(result);
        }

        [HttpDelete("doctors/{doctorId}/availability/{availabilityId}")]
        public async Task<IActionResult> AdminDeleteDoctorAvailability(int doctorId, int availabilityId)
        {
            var result = await _adminService.DeleteDoctorAvailabilityAsync(doctorId, availabilityId);
            return result ? NoContent() : BadRequest("Cannot delete or invalid slot.");
        }

        // --- BLOG MANAGEMENT ---

        [HttpGet("blog-posts")]
        public async Task<IActionResult> GetAllBlogPosts()
        {
            return Ok(await _adminService.GetAllBlogPostsAsync());
        }

        [HttpPut("blog-posts/{id}/approve")]
        public async Task<IActionResult> ApproveBlogPost(int id)
        {
            var result = await _adminService.ApproveBlogPostAsync(id);
            return result ? NoContent() : NotFound("Post not found.");
        }

        [HttpPut("blog-posts/{id}/reject")]
        public async Task<IActionResult> RejectBlogPost(int id)
        {
            var result = await _adminService.RejectBlogPostAsync(id);
            return result ? NoContent() : NotFound("Post not found.");
        }
  
    }
}
