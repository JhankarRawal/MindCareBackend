using MentalHealthApis.DTOs;
using MentalHealthApis.Models; // Required for SentimentFlags
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MentalHealthApis.Services
{
    public interface IJournalEntriesService
    {
        Task<JournalEntryDto> CreateJournalAsync(int userId, string content);
        Task<JournalEntryDto?> GetByIdAsync(int journalId, int requesterId);
        Task<List<JournalEntryDto>> GetByUserAsync(int userId, int requesterId, int page = 1, int pageSize = 10, string? search = null);
        Task<bool> UpdateJournalAsync(int journalId, int requesterId, string updatedContent);
        Task<bool> DeleteJournalAsync(int journalId, int requesterId);
        Task<List<SentimentHistoryPoint>> GetSentimentHistoryAsync(int userId, int requesterId);
        Task<string> ExportJournalsToCsvAsync(int userId, int requesterId);

        // ========= ADD THIS NEW METHOD DEFINITION =========
        /// <summary>
        /// Gets the sentiment data from the most recent journal entry for a specific user.
        /// </summary>
        Task<SentimentFlags?> GetLatestSentimentForUserAsync(int userId);
    }
}