using MentalHealthApis.Data;
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return MapUserToDto(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            return MapUserToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(user => MapUserToDto(user))
                .ToListAsync();
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto, int currentUserId)
        {
            // Ensure user is updating themselves or an admin is doing it.
            // This check is usually better handled by controller authorization,
            // but can be an additional layer here.
            if (id != currentUserId)
            {
                // Here you might check if currentUserId has Admin role if you want Admin to update any user.
                // For now, let's assume only self-update is handled by this specific method call pattern.
                // The controller would differentiate.
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(updateUserDto.Name))
            {
                user.Name = updateUserDto.Name;
            }
            if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
            {
                user.PhoneNumber = updateUserDto.PhoneNumber;
            }
            // Password updates should be handled via a separate, more secure flow (e.g., ChangePasswordDto)

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return MapUserToDto(user);
        }

        public async Task<bool> UpdateUserRoleAsync(int id, UserRole newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Add any business logic for role changes (e.g., ensuring not last admin)
            user.Role = newRole;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public int GetCurrentUserId(ClaimsPrincipal user)
        {
            var id = user.FindFirst("user_id")?.Value;
            return id != null ? int.Parse(id) : throw new UnauthorizedAccessException();
        }

        public async Task<bool> CanAccessUserDataAsync(int requesterId, int targetUserId)
        {
            if (requesterId == targetUserId) return true;

            var role = await _context.Users
                .Where(u => u.Id == requesterId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            if (role == UserRole.Admin) return true;

            return await _context.Appointments.AnyAsync(a => a.DoctorId == requesterId && a.UserId == targetUserId);
        }

        public string? GetUserRole(ClaimsPrincipal user)
        {
            return user.FindFirst("role")?.Value;
        }
    


// Private helper for mapping
private static UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            };
        }
    }
}