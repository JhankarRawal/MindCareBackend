using MentalHealthApis.DTOs;
using MentalHealthApis.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/doctor-applications")]
[Authorize] // IMPORTANT: Only logged-in users can apply
public class DoctorApplicationController : ControllerBase
{
    private readonly IDoctorOnboardingService _onboardingService;

    public DoctorApplicationController(IDoctorOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyToBeDoctor([FromBody] DoctorApplicationDto applicationDto)
    {
        // Get the logged-in user's ID from the token claims. This is secure.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID could not be determined from token.");
        }

        // Call the service to create the Doctor record
        var newDoctorId = await _onboardingService.CreateDoctorApplicationAsync(applicationDto, userId);

        if (newDoctorId == 0)
        {
            return BadRequest("Could not process application.");
        }

        // Return the new DoctorId to the frontend
        return Ok(new { message = "Profile created successfully. Please upload your documents.", doctorId = newDoctorId });
    }
}