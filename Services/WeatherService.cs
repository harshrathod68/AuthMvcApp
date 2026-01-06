using System.Text.Json;
using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<WeatherModel?> GetWeatherAsync(string city)
        {
            try
            {
                var apiKey = _configuration["WeatherApi:ApiKey"];
                var baseUrl = _configuration["WeatherApi:BaseUrl"];
                
                var url = $"{baseUrl}?q={city}&appid={apiKey}&units=metric";
                
                _logger.LogInformation($"Fetching weather for: {city}");
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Weather API returned: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(json);
                var root = data.RootElement;

                var weather = new WeatherModel
                {
                    CityName = root.GetProperty("name").GetString() ?? city,
                    Country = root.GetProperty("sys").GetProperty("country").GetString() ?? "",
                    Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
                    FeelsLike = root.GetProperty("main").GetProperty("feels_like").GetDouble(),
                    TempMin = root.GetProperty("main").GetProperty("temp_min").GetDouble(),
                    TempMax = root.GetProperty("main").GetProperty("temp_max").GetDouble(),
                    Humidity = root.GetProperty("main").GetProperty("humidity").GetInt32(),
                    Pressure = root.GetProperty("main").GetProperty("pressure").GetInt32(),
                    WindSpeed = root.GetProperty("wind").GetProperty("speed").GetDouble(),
                    WindDegree = root.GetProperty("wind").TryGetProperty("deg", out var deg) ? deg.GetInt32() : 0,
                    Description = root.GetProperty("weather")[0].GetProperty("description").GetString() ?? "",
                    Icon = root.GetProperty("weather")[0].GetProperty("icon").GetString() ?? "",
                    Visibility = root.TryGetProperty("visibility", out var vis) ? vis.GetInt32() / 1000 : 0,
                    Clouds = root.GetProperty("clouds").GetProperty("all").GetInt32(),
                    Sunrise = DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("sys").GetProperty("sunrise").GetInt64()).LocalDateTime,
                    Sunset = DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("sys").GetProperty("sunset").GetInt64()).LocalDateTime
                };

                return weather;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching weather: {ex.Message}");
                return null;
            }
        }
    }
}
