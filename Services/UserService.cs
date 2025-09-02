// In: MentalHealthApis.Services/UserService.cs

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

        // New: Get all users with Role.Doctor
        public async Task<IEnumerable<UserDto>> GetDoctorsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Doctor)
                .Select(user => MapUserToDto(user))
                .ToListAsync();
        }

        // New: Get a single doctor by ID
        public async Task<UserDto?> GetDoctorByIdAsync(int id)
        {
            var doctor = await _context.Users
                .Where(u => u.Id == id && u.Role == UserRole.Doctor)
                .FirstOrDefaultAsync();

            if (doctor == null) return null;

            return MapUserToDto(doctor);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto, int currentUserId)
        {
            if (id != currentUserId)
            {
                // Optionally add role-based authorization here if you want admins to update others.
                // For now, assume this is for self-update or specific admin methods will handle it.
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

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return MapUserToDto(user);
        }

        public async Task<bool> UpdateUserRoleAsync(int id, UserRole newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Role = newRole;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public int GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdValue = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdValue, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID claim (NameIdentifier) is missing or not in a valid integer format.");
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