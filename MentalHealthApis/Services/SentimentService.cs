using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using   MentalHealthApis.Models;

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
            var request = new { content = content };
            var response = await _httpClient.PostAsJsonAsync("https://recommendation-system-vfxt.onrender.com/predict", request);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to call sentiment API.");

            return await response.Content.ReadFromJsonAsync<SentimentResult>();
        }
    }
}
