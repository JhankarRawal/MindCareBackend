// Controllers/JournelController.cs
using Microsoft.AspNetCore.Mvc;
using MentalHealthApis.Models;
using MentalHealthApis.Services;

namespace MentalHealthApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournelController : ControllerBase
    {
        private readonly SentimentService _sentimentService;

        public JournelController(SentimentService sentimentService)
        {
            _sentimentService = sentimentService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeJournalEntry([FromBody] JournalEntryRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return BadRequest("Journal content cannot be empty");
                }

                // Call sentiment analysis service
                var sentimentResult = await _sentimentService.AnalyzeAsync(request.Content);

                // Map SentimentResult to JournalEntryResponse
                var response = new JournalEntryResponse
                {
                    Title = request.Title ?? "Journal Entry",
                    Content = request.Content,
                    Mood = request.Mood,
                    Date = DateTime.UtcNow,
                    Prediction = sentimentResult.Sentiment, // Use Sentiment instead of Prediction
                    Scores = new Dictionary<string, double> // Map confidence_scores to Scores
                    {
                        ["positive"] = sentimentResult.Confidence_Scores?.Positive ?? 0.0,
                        ["negative"] = sentimentResult.Confidence_Scores?.Negative ?? 0.0,
                        ["neutral"] = sentimentResult.Confidence_Scores?.Neutral ?? 0.0
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error analyzing journal entry: {ex.Message}");
            }
        }
    }

    // Request model for the journal entry
}