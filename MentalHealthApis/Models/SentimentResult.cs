namespace MentalHealthApis.Models
{
    public class SentimentResult
    {
        public string Prediction { get; set; }
        public Dictionary<string, double> Scores { get; set; }
    }
}
