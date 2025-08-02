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
        [HttpGet("documents/pending")]
public async Task<ActionResult<IEnumerable<DoctorDocumentAdminViewDto>>> GetPendingDocuments()
{
    var documents = await _adminService.GetPendingDoctorDocumentsAsync();
    return Ok(documents);
}

[HttpGet("doctors/{doctorId}/documents")]
public async Task<ActionResult<IEnumerable<DoctorDocumentAdminViewDto>>> GetDocumentsForDoctor(int doctorId)
{
    var documents = await _adminService.GetDocumentsByDoctorIdAsync(doctorId);
    return Ok(documents);
}

        [HttpPut("documents/{documentId}/verify")]
        public async Task<IActionResult> VerifyDocument(int documentId)
        {
            var success = await _adminService.VerifyDoctorDocumentAsync(documentId);
            return success ? NoContent() : NotFound(new { message = "Document not found." });
        }
[HttpPatch("doctors/{doctorId}/approve")]
public async Task<IActionResult> ApproveDoctorApplication(int doctorId)
{
    var success = await _adminService.UpdateDoctorApplicationStatusAsync(doctorId, "Approved", null);
    if (!success) return NotFound(new { message = "Doctor not found." });

    // Optional but recommended: Update the user's role to grant access.
    // await _adminService.ActivateDoctorRoleForUser(doctorId);

    return Ok(new { message = "Doctor application approved successfully." });
}

public class RejectionPayload
{
    public string Notes { get; set; }
}

// This endpoint rejects the entire application for a doctor.
[HttpPatch("doctors/{doctorId}/reject")]
public async Task<IActionResult> RejectDoctorApplication(int doctorId, [FromBody] RejectionPayload payload)
{
    if (string.IsNullOrWhiteSpace(payload?.Notes))
    {
        return BadRequest(new { message = "Rejection notes are required." });
    }

    var success = await _adminService.UpdateDoctorApplicationStatusAsync(doctorId, "Rejected", payload.Notes);
    if (!success) return NotFound(new { message = "Doctor not found." });

    return Ok(new { message = "Doctor application rejected successfully." });
}

// For this endpoint, the admin UI would send a simple JSON body like: { "notes": "Image is blurry." }
public class RejectionDto { public string Notes { get; set; } }

[HttpPut("documents/{documentId}/reject")]
public async Task<IActionResult> RejectDocument(int documentId, [FromBody] RejectionDto rejection)
{
    if (string.IsNullOrWhiteSpace(rejection?.Notes))
    {
        return BadRequest(new { message = "Rejection notes are required." });
    }
    var success = await _adminService.RejectDoctorDocumentAsync(documentId, rejection.Notes);
    return success ? NoContent() : NotFound(new { message = "Document not found." });
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
