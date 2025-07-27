using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MentalHealthApis.Models;
using MentalHealthApis.Data;
using System.Security.Claims;
using System.Net.Http.Json;
using MentalHealthApis.DTOs;
using System.Text;
using System.Linq; // Required for OrderByDescending
using System.Threading.Tasks; // Required for Task
using System.Collections.Generic; // Required for List

namespace MentalHealthApis.Services
{
    public class JournalEntriesService : IJournalEntriesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _httpFactory;

        public JournalEntriesService(ApplicationDbContext db, IUserService userService, IHttpClientFactory httpFactory)
        {
            _context = db;
            _userService = userService;
            _httpFactory = httpFactory;
        }

        // ... (All your existing methods like CreateJournalAsync, GetByIdAsync, etc. remain unchanged)
        #region Existing Methods
        public async Task<JournalEntryDto> CreateJournalAsync(int userId, string content)
        {
            var client = _httpFactory.CreateClient("SentimentApi"); 
              var sentimentResponse = await client.PostAsJsonAsync("/predict", new { text = content });
               sentimentResponse.EnsureSuccessStatusCode(); 
            var sentimentJson = await sentimentResponse.Content.ReadAsStringAsync();

            var entry = new JournalEntry
            {
                UserId = userId,
                EntryDate = DateTime.UtcNow.Date,
                Content = content,
                SentimentJson = sentimentJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            return new JournalEntryDto
            {
                JournalId = entry.JournalId,
                UserId = entry.UserId,
                EntryDate = entry.EntryDate,
                Content = entry.Content,
                Sentiments = JsonSerializer.Deserialize<SentimentFlags>(sentimentJson)
            };
        }

        public async Task<JournalEntryDto?> GetByIdAsync(int journalId, int requesterId)
        {
            var entry = await _context.JournalEntries.FindAsync(journalId);
            if (entry == null || !await _userService.CanAccessUserDataAsync(requesterId, entry.UserId))
                return null;

            return new JournalEntryDto
            {
                JournalId = entry.JournalId,
                UserId = entry.UserId,
                EntryDate = entry.EntryDate,
                Content = entry.Content,
                Sentiments = JsonSerializer.Deserialize<SentimentFlags>(entry.SentimentJson)
            };
        }

        public async Task<List<JournalEntryDto>> GetByUserAsync(int userId, int requesterId, int page = 1, int pageSize = 10, string? search = null)
        {
            if (!await _userService.CanAccessUserDataAsync(requesterId, userId))
                return new List<JournalEntryDto>();

            var query = _context.JournalEntries
                .Where(j => j.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(j => j.Content.ToLower().Contains(search.ToLower()));

            var entries = await query
                .OrderByDescending(j => j.EntryDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return entries.Select(entry => new JournalEntryDto
            {
                JournalId = entry.JournalId,
                UserId = entry.UserId,
                EntryDate = entry.EntryDate,
                Content = entry.Content,
                Sentiments = JsonSerializer.Deserialize<SentimentFlags>(entry.SentimentJson)
            }).ToList();
        }

        public async Task<bool> UpdateJournalAsync(int journalId, int requesterId, string updatedContent)
        {
            var entry = await _context.JournalEntries.FindAsync(journalId);
            if (entry == null || (entry.UserId != requesterId)) return false;

            entry.Content = updatedContent;
            entry.UpdatedAt = DateTime.UtcNow;

            _context.JournalEntries.Update(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteJournalAsync(int journalId, int requesterId)
        {
            var entry = await _context.JournalEntries.FindAsync(journalId);
            if (entry == null || (entry.UserId != requesterId)) return false;

            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<SentimentHistoryPoint>> GetSentimentHistoryAsync(int userId, int requesterId)
        {
            if (!await _userService.CanAccessUserDataAsync(requesterId, userId))
                return new List<SentimentHistoryPoint>();

            var entries = await _context.JournalEntries
                .Where(j => j.UserId == userId)
                .OrderBy(j => j.EntryDate)
                .ToListAsync();

            return entries.Select(e => new SentimentHistoryPoint
            {
                Date = e.EntryDate,
                Sentiment = JsonSerializer.Deserialize<SentimentFlags>(e.SentimentJson) ?? new SentimentFlags()
            }).ToList();
        }
           

            private string GenerateCsv(List<JournalEntryDto> entries)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Date,Content,Anxiety,Depression,Stress,Bipolar,Normal,Suicidal,PersonalityDisorder");

                foreach (var e in entries)
                {
                    var s = e.Sentiments!;
                    var safeContent = e.Content.Replace("\"", "\"\"");
                    sb.AppendLine($"\"{e.EntryDate:yyyy-MM-dd}\",\"{safeContent}\",{s.Anxiety},{s.Depression},{s.Stress},{s.Bipolar},{s.Normal},{s.Suicidal},{s.PersonalityDisorder}");
                }

                return sb.ToString();
            }

            public async Task<string> ExportJournalsToCsvAsync(int userId, int requesterId)
            {
                var journals = await GetByUserAsync(userId, requesterId, page: 1, pageSize: int.MaxValue);
                return GenerateCsv(journals);
            }
         #endregion

        // ========= ADD THIS ENTIRE NEW METHOD =========
        public async Task<SentimentFlags?> GetLatestSentimentForUserAsync(int userId)
        {
            var latestEntry = await _context.JournalEntries
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.EntryDate)   // Get the newest entry by date
                .ThenByDescending(j => j.CreatedAt) // If dates are the same, get the one created last
                .FirstOrDefaultAsync();

            if (latestEntry == null || string.IsNullOrEmpty(latestEntry.SentimentJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<SentimentFlags>(latestEntry.SentimentJson);
        }
    }
}