using MentalHealthApis.DTOs;
using MentalHealthApis.Models;

namespace MentalHealthApis.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterDto registerDto);
        Task<string?> LoginAsync(LoginDto loginDto); // Returns JWT token or null
        User? GetUserById(int id); // Helper to get user for token generation
        Task GetUserById(bool v);
        object GetUserById(string value);
        Task GetUserByEmail(string email);
    }
}
