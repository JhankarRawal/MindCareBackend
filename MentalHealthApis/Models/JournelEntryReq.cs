using System.Text.Json.Serialization;

namespace MentalHealthApis.Models
{
    public class JournalEntryRequest
    {
        public string Title { get; set; }

        [JsonPropertyName("text")]
        public string Content { get; set; }

        public string Mood { get; set; }
        public DateTime Date { get; set; }
    }
}
