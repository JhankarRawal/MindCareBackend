using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MentalHealthApis.Models;

namespace MentalHealthApis.Services
{
    public class SentimentService
    {
        private readonly HttpClient _httpClient;

        public SentimentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SentimentResult> AnalyzeAsync(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new ArgumentException("Content cannot be null or empty", nameof(content));
                }

                // Send { "text": "..." } to Flask API
                var request = new { text = content };

                var response = await _httpClient.PostAsJsonAsync("http://192.168.18.139:5000/predict", request);
                Console.WriteLine($"[SentimentService] Response status code: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SentimentService] Error response: {errorContent}");
                    throw new Exception($"Failed to call sentiment API. Status: {response.StatusCode}, Content: {errorContent}");
                }

                // Read and log the raw response for debugging
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SentimentService] Raw response: {rawResponse}");

                // Parse the JSON response
                var result = await response.Content.ReadFromJsonAsync<SentimentResult>();

                if (result == null)
                {
                    throw new Exception("Received null response from sentiment API");
                }

                Console.WriteLine($"[SentimentService] Parsed sentiment: {result.Sentiment}");
                Console.WriteLine($"[SentimentService] Confidence scores - Positive: {result.Confidence_Scores?.Positive}, Negative: {result.Confidence_Scores?.Negative}, Neutral: {result.Confidence_Scores?.Neutral}");

                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[SentimentService] HTTP request failed: {ex.Message}");
                throw new Exception($"Network error when calling sentiment API: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[SentimentService] Request timeout: {ex.Message}");
                throw new Exception("Timeout when calling sentiment API", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SentimentService] Unexpected error: {ex.Message}");
                throw;
            }
        }
    }
}