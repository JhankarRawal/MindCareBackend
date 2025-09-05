using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MentalHealthApis.Data;

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var user = await _authService.RegisterAsync(registerDto);
            if (user == null)
            {
                return BadRequest("Email already exists or invalid data.");
            }

            // This line will now work because DoctorInfo is optional in UserDto
            return Ok(new UserDto { Id = user.Id, Name = user.Name, Email = user.Email, Role = user.Role });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var token = await _authService.LoginAsync(loginDto);
            if (token == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            int userId = int.Parse(userIdClaim.Value);
            User loggedInUser = await _authService.GetUserByIdAsync(userId);
            if (loggedInUser == null)
            {
                return Unauthorized("User not found.");
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == loggedInUser.Id);

            // This block will also now work correctly
            var userDto = new UserDto
            {
                Id = loggedInUser.Id,
                Name = loggedInUser.Name,
                Email = loggedInUser.Email,
                Role = loggedInUser.Role,
                DoctorInfo = doctor != null ? new DoctorInfoResponseDto
                {
                    DoctorId = doctor.Id, 
                    ApplicationStatus = doctor.ApplicationStatus,
                    RejectionReason = doctor.AdminNotes
                } : null
            };

            var response = new AuthResponseDto
            {
                Token = token,
                User = userDto
            };

            return Ok(response);
        }
    }
}