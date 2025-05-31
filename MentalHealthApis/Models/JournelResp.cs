namespace MentalHealthApis.Models
{
    public class JournalEntryResponse
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Mood { get; set; }
        public DateTime Date { get; set; }

        public string Prediction { get; set; }
        public Dictionary<string, double> Scores { get; set; }
    }
}
