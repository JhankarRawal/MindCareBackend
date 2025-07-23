namespace MentalHealthApis.Models
{
    public class SentimentHistoryPoint
    {
        public DateTime Date { get; set; }
        public SentimentFlags Sentiment { get; set; }
    }
}
