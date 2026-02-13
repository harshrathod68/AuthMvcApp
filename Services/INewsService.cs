using MyApps.Models;

namespace MyApps.Services
{
    public interface INewsService
    {
        Task<List<NewsArticle>> GetTopHeadlinesAsync(string country, string? category = null, string language = "en", int page = 1);
        Task<List<NewsArticle>> SearchNewsAsync(string query, string language = "en", int page = 1);
    }
}
