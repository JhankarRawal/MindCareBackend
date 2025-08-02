using MentalHealthApis.Data;
using MentalHealthApis.DTOs;
using MentalHealthApis.Models;
using MentalHealthApis.Services.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MentalHealthApis.Services
{
    public class DoctorOnboardingService : IDoctorOnboardingService
    {
        private readonly ApplicationDbContext _context;

        public DoctorOnboardingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateDoctorApplicationAsync(DoctorApplicationDto applicationDto, int userId)
        {
            // Optional: Check if a doctor record already exists for this user
            var existingDoctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (existingDoctor != null)
            {
                // You can either return the existing ID or throw an exception
                // depending on your business rules (e.g., "Application already exists").
                return existingDoctor.Id;
            }

            // Create a new Doctor entity from the DTO
            var newDoctor = new Doctor
            {
                Name = applicationDto.Name,
                Specialization = applicationDto.Specialization,
                ContactInfo = applicationDto.ContactInfo,
                UserId = userId // CRITICAL: Link this new Doctor record to the logged-in User
            };

            // Add it to the database
            _context.Doctors.Add(newDoctor);
            await _context.SaveChangesAsync();

            // Return the ID of the record we just created
            return newDoctor.Id;
        }
    }
}