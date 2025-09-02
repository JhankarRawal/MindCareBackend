// In: MentalHealthApis.Controllers/UsersController.cs (or rename to DoctorsController or extend existing)

using MentalHealthApis.Data;
using MentalHealthApis.Models;
using MentalHealthApis.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using MentalHealthApis.Services; // Add this using statement

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _hasher;
        private readonly IUserService _userService; // Inject IUserService

        public UsersController(ApplicationDbContext context, IConfiguration configuration, IUserService userService) // Add IUserService
        {
            _context = context;
            _configuration = configuration;
            _hasher = new PasswordHasher<User>();
            _userService = userService; // Initialize IUserService
        }

        // ... (Existing Register, Login, GetCurrentUser, UpdateCurrentUser methods remain) ...

        // GET: api/users/doctors
        // Consider making this [Authorize(Roles = "User,Admin,Doctor")] if you want specific roles to view doctors
        [HttpGet("doctors")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetDoctors()
        {
            var doctors = await _userService.GetDoctorsAsync();
            return Ok(doctors);
        }

        // GET: api/users/doctors/{id}
        // Consider making this [Authorize(Roles = "User,Admin,Doctor")]
        [HttpGet("doctors/{id}")]
        public async Task<ActionResult<UserDto>> GetDoctorById(int id)
        {
            var doctor = await _userService.GetDoctorByIdAsync(id);

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            return Ok(doctor);
        }

        // Helper to generate JWT (remains the same)
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}