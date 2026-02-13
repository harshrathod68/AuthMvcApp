using System.Text.Json;
using MyApps.Models;

namespace MyApps.Services
{
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://gnews.io/api/v4";

        public NewsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["NewsApi:ApiKey"] ?? "";
        }

        public async Task<List<NewsArticle>> GetTopHeadlinesAsync(string country, string? category = null, string language = "en", int page = 1)
        {
            var url = $"{BaseUrl}/top-headlines?country={country}&lang={language}&apikey={_apiKey}&max=9&page={page}";
            if (!string.IsNullOrEmpty(category)) url += $"&topic={category}";

            return await FetchNewsAsync(url);
        }

        public async Task<List<NewsArticle>> SearchNewsAsync(string query, string language = "en", int page = 1)
        {
            var url = $"{BaseUrl}/search?q={Uri.EscapeDataString(query)}&lang={language}&apikey={_apiKey}&max=9&page={page}";
            return await FetchNewsAsync(url);
        }

        private async Task<List<NewsArticle>> FetchNewsAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<NewsArticle>();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<GNewsApiResponse>(json);

                if (apiResponse?.Articles == null) return new List<NewsArticle>();

                return apiResponse.Articles
                    .Where(a => !string.IsNullOrEmpty(a.Title))
                    .Select(a => new NewsArticle
                    {
                        Title = a.Title ?? "",
                        Description = a.Description ?? "",
                        Author = a.Source?.Name ?? "Unknown",
                        Url = a.Url ?? "",
                        ImageUrl = a.Image ?? "",
                        PublishedAt = a.PublishedAt,
                        SourceName = a.Source?.Name ?? "Unknown"
                    }).ToList();
            }
            catch { return new List<NewsArticle>(); }
        }
    }
}
