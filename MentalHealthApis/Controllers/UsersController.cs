using MentalHealthApis.DTOs;
// using MentalHealthApis.Services; // If you create a UserService
using MentalHealthApis.Data; // For direct context use if not using UserService
using MentalHealthApis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MentalHealthApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All user endpoints require authentication
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Replace with IUserService later

        public UsersController(ApplicationDbContext context) // Inject IUserService
        {
            _context = context;
        }

        // GET: api/users/me (Get current logged-in user info)
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            return Ok(new UserDto { Id = user.Id, Name = user.Name, Email = user.Email, PhoneNumber = user.PhoneNumber, Role = user.Role });
        }

        // GET: api/users/{id} (Admin can get any user)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new UserDto { Id = user.Id, Name = user.Name, Email = user.Email, PhoneNumber = user.PhoneNumber, Role = user.Role });
        }

        // PUT: api/users/me (Update current logged-in user's info)
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser(UpdateUserDto updateUserDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(updateUserDto.Name)) user.Name = updateUserDto.Name;
            if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber)) user.PhoneNumber = updateUserDto.PhoneNumber;
            // Add more updatable fields as needed

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}