using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MentalHealthApis.Models
{
    public class SentimentResult
    {
        [JsonPropertyName("sentiment")]
        public string Sentiment { get; set; }

        [JsonPropertyName("confidence_scores")]
        public ConfidenceScores Confidence_Scores { get; set; }

        [JsonPropertyName("context_adjustments_applied")]
        public ContextAdjustments Context_Adjustments_Applied { get; set; }

        [JsonPropertyName("processed_features_count")]
        public int Processed_Features_Count { get; set; }

        [JsonPropertyName("recommendations")]
        public List<Recommendation> Recommendations { get; set; }

        [JsonPropertyName("significant_contextual_features")]
        public List<string> Significant_Contextual_Features { get; set; }
    }

    public class ConfidenceScores
    {
        [JsonPropertyName("negative")]
        public double Negative { get; set; }

        [JsonPropertyName("neutral")]
        public double Neutral { get; set; }

        [JsonPropertyName("positive")]
        public double Positive { get; set; }
    }

    public class ContextAdjustments
    {
        [JsonPropertyName("positive")]
        public double Positive { get; set; }
    }

    public class Recommendation
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

}