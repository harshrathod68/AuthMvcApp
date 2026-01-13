using System.Text.Json.Serialization;

namespace AuthMvcApp.Models
{
    public class NewsSearchModel
    {
        public string Country { get; set; } = "India";
        public string Category { get; set; } = "";
        public string SearchQuery { get; set; } = "";
        public string Language { get; set; } = "en";
        public int Page { get; set; } = 1;
    }

    public class NewsArticle
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Author { get; set; } = "";
        public string Url { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public DateTime? PublishedAt { get; set; }
        public string SourceName { get; set; } = "";
    }

    // GNews API Response Models
    public class GNewsApiResponse
    {
        [JsonPropertyName("totalArticles")]
        public int TotalArticles { get; set; }

        [JsonPropertyName("articles")]
        public List<GNewsArticle> Articles { get; set; } = new();
    }

    public class GNewsArticle
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("publishedAt")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("source")]
        public GNewsSource? Source { get; set; }
    }

    public class GNewsSource
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
