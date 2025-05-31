namespace MentalHealthApis.Models
{
    public class JournalEntryRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Mood { get; set; }
        public DateTime Date { get; set; }
    }
}
