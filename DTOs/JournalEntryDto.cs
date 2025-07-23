using MentalHealthApis.Models;

namespace MentalHealthApis.DTOs
{
    public class JournalEntryDto
    {
        public int? JournalId { get; set; }
        public int? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? EntryDate { get; set; }
        public SentimentFlags? Sentiments { get; set; }
    }
}
