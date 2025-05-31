using Microsoft.AspNetCore.Mvc;
using MentalHealthApis.Models;
using MentalHealthApis.Services;
namespace MentalHealthApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournalController : ControllerBase
    {
        private readonly SentimentService _sentimentService;

        public JournalController(SentimentService sentimentService)
        {
            _sentimentService = sentimentService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateJournal([FromBody] JournalEntryRequest entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Content))
                return BadRequest("Content is required.");

            var sentiment = await _sentimentService.AnalyzeAsync(entry.Content);

            var response = new JournalEntryResponse
            {
                Title = entry.Title,
                Content = entry.Content,
                Mood = entry.Mood,
                Date = entry.Date,
                Prediction = sentiment.Prediction,
                Scores = sentiment.Scores
            };

            return Ok(response);
        }
    }
}
