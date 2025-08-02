using MentalHealthApis.DTOs;
using System.Threading.Tasks;

namespace MentalHealthApis.Services.Interfaces
{
    public interface IDoctorOnboardingService
    {
        // This method will take the application and the LOGGED-IN user's ID
        // It will return the ID of the newly created Doctor record.
        Task<int> CreateDoctorApplicationAsync(DoctorApplicationDto applicationDto, int userId);
    }
}