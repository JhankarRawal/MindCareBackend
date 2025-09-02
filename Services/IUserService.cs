// In: MentalHealthApis.Services/IUserService.cs

using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllUsersAsync(); // For Admin
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto, int currentUserId);
        Task<bool> UpdateUserRoleAsync(int id, UserRole newRole); // For Admin

        // New Methods for Doctors
        Task<IEnumerable<UserDto>> GetDoctorsAsync();
        Task<UserDto?> GetDoctorByIdAsync(int id);


        int GetCurrentUserId(ClaimsPrincipal user);
        Task<bool> CanAccessUserDataAsync(int requesterId, int targetUserId);
        string? GetUserRole(ClaimsPrincipal user);
    }
}