using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email); // Might be useful
        Task<IEnumerable<UserDto>> GetAllUsersAsync(); // For Admin
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto, int currentUserId);
        Task<bool> UpdateUserRoleAsync(int id, UserRole newRole); // For Admin
        // DeleteUserAsync might be needed, but handle cascading deletes carefully
    }
}