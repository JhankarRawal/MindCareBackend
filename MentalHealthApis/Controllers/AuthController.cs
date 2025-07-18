using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            // Restrict admin registration
            if (registerDto.Role == Models.UserRole.Admin)
            {
                // Consider adding a restriction for admin role, e.g., only existing admins can register a new admin.
            }

            var user = await _authService.RegisterAsync(registerDto);
            if (user == null)
            {
                return BadRequest("Email already exists or invalid data.");
            }

            // Return the registered user's basic info
            return Ok(new UserDto { Id = user.Id, Name = user.Name, Email = user.Email, Role = user.Role });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            // Validate login and retrieve token
            var token = await _authService.LoginAsync(loginDto);
            if (token == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            // Parse the token to get user info from claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);
            User loggedInUser = _authService.GetUserById(userId); // Assume this is an async call

            if (loggedInUser == null)
            {
                return Unauthorized("User not found.");
            }

            // Create response DTO with token and user info
            var response = new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = loggedInUser.Id,
                    Name = loggedInUser.Name,
                    Email = loggedInUser.Email,
                    Role = loggedInUser.Role
                }
            };

            return Ok(response);
        }
    }
}
