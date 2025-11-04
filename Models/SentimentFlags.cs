using System.Text.Json.Serialization;

namespace MentalHealthApis.Models
{
    public class SentimentFlags
    {
        public bool Anxiety { get; set; }
        public bool Bipolar { get; set; }
        public bool Depression { get; set; }
        public bool Normal { get; set; }

        [JsonPropertyName("Personality disorder")]
        public bool PersonalityDisorder { get; set; }

        public bool Stress { get; set; }
        public bool Suicidal { get; set; }
    }
}
