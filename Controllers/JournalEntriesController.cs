using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MentalHealthApis.Services;
using System.Text;
using System.ComponentModel.DataAnnotations; // Add this for validation

namespace MentalHealthApis.Controllers
{
    // ========= ADD THIS DTO CLASS FOR THE REQUEST BODY =========
    /// <summary>
    /// Represents the expected data for creating a journal entry.
    /// </summary>
    public class CreateJournalRequest
    {
        [Required(ErrorMessage = "Content is required.")]
        [MinLength(10, ErrorMessage = "Journal content must be at least 10 characters.")]
        public string Content { get; set; } = string.Empty;
    }


    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JournalEntriesController : ControllerBase
    {
        private readonly IJournalEntriesService _journalService;
        private readonly IUserService _userService;

        public JournalEntriesController(IJournalEntriesService journalService, IUserService userService)
        {
            _journalService = journalService;
            _userService = userService;
        }

        // ========= THIS IS THE CORRECTED METHOD =========
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJournalRequest request)
        {
            // The framework automatically validates the request based on the DTO's attributes.
            // If validation fails, it will return a 400 Bad Request with details.

            var userId = _userService.GetCurrentUserId(User);

            // Pass the content from the request DTO to your service
            var result = await _journalService.CreateJournalAsync(userId, request.Content);
            
            return Ok(result);
        }

        // --- All other methods below this line remain unchanged ---

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = _userService.GetCurrentUserId(User);
            var result = await _journalService.GetByIdAsync(id, userId);
            return result != null ? Ok(result) : Forbid();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId, int page = 1, int pageSize = 10, string? search = null)
        {
            var requesterId = _userService.GetCurrentUserId(User);
            var result = await _journalService.GetByUserAsync(userId, requesterId, page, pageSize, search);
            return Ok(result);
        }

        // NOTE: This Update method should also be changed to use a DTO in the future,
        // but for now, we are only fixing the `Create` method.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] string content)
        {
            var userId = _userService.GetCurrentUserId(User);
            var success = await _journalService.UpdateJournalAsync(id, userId, content);
            return success ? Ok() : Forbid();
        }

        [HttpGet("user/{userId}/sentiment-history")]
        public async Task<IActionResult> GetSentimentHistory(int userId)
        {
            var requesterId = _userService.GetCurrentUserId(User);
            var result = await _journalService.GetSentimentHistoryAsync(userId, requesterId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userService.GetCurrentUserId(User);
            var success = await _journalService.DeleteJournalAsync(id, userId);
            return success ? Ok() : Forbid();
        }

        [HttpGet("user/{userId}/export")]
        public async Task<IActionResult> ExportCsv(int userId)
        {
            var requesterId = _userService.GetCurrentUserId(User);
            var csvData = await _journalService.ExportJournalsToCsvAsync(userId, requesterId);

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            return File(bytes, "text/csv", $"JournalEntries_{userId}_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}