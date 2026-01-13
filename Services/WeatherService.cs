/*
 * =====================================================
 * WeatherService.cs - Weather API Service
 * =====================================================
 * 
 * Ye service OpenWeatherMap API se weather data fetch karti hai.
 * 
 * API: https://openweathermap.org/api
 * Free Plan: 1000 calls/day
 * 
 * How it works:
 * 1. User city name enter karta hai
 * 2. Ye service API ko call karti hai
 * 3. JSON response aata hai
 * 4. JSON ko C# object mein convert karte hain
 * 5. Controller ko return karte hain
 * 
 * =====================================================
 */

using System.Text.Json;
using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    public class WeatherService : IWeatherService
    {
        // HttpClient - API calls karne ke liye
        private readonly HttpClient _httpClient;
        
        // Configuration - appsettings.json se values lene ke liye
        private readonly IConfiguration _configuration;
        
        // Logger - debugging ke liye (console mein print hota hai)
        private readonly ILogger<WeatherService> _logger;

        // Constructor - Dependencies inject hoti hain
        public WeatherService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// City ka weather data fetch karta hai
        /// </summary>
        /// <param name="city">City ka naam (e.g., "Mumbai", "Delhi")</param>
        /// <returns>WeatherModel with all weather data, or null if city not found</returns>
        public async Task<WeatherModel?> GetWeatherAsync(string city)
        {
            try
            {
                // ===== STEP 1: API URL banao =====
                // appsettings.json se API key aur base URL lo
                var apiKey = _configuration["WeatherApi:ApiKey"];
                var baseUrl = _configuration["WeatherApi:BaseUrl"];
                
                // Complete URL: baseUrl?q=Mumbai&appid=xxx&units=metric
                // units=metric means temperature in Celsius
                var url = $"{baseUrl}?q={city}&appid={apiKey}&units=metric";
                
                // Log karo (debugging ke liye)
                _logger.LogInformation($"Fetching weather for: {city}");
                
                // ===== STEP 2: API call karo =====
                // GetAsync = HTTP GET request bhejta hai
                // await = response aane tak wait karo
                var response = await _httpClient.GetAsync(url);
                
                // ===== STEP 3: Response check karo =====
                // IsSuccessStatusCode = 200 OK means success
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Weather API returned: {response.StatusCode}");
                    return null; // City not found ya API error
                }

                // ===== STEP 4: JSON response read karo =====
                var json = await response.Content.ReadAsStringAsync();
                
                // JSON parse karo
                var data = JsonDocument.Parse(json);
                var root = data.RootElement;

                // ===== STEP 5: JSON se data nikalo aur WeatherModel banao =====
                /*
                 * API Response Example:
                 * {
                 *   "name": "Mumbai",
                 *   "main": { "temp": 30.5, "humidity": 80 },
                 *   "weather": [{ "description": "clear sky", "icon": "01d" }],
                 *   "wind": { "speed": 5.2 }
                 * }
                 */
                
                var weather = new WeatherModel
                {
                    // Basic info
                    CityName = root.GetProperty("name").GetString() ?? city,
                    Country = root.GetProperty("sys").GetProperty("country").GetString() ?? "",
                    
                    // Temperature (Celsius mein)
                    Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
                    FeelsLike = root.GetProperty("main").GetProperty("feels_like").GetDouble(),
                    TempMin = root.GetProperty("main").GetProperty("temp_min").GetDouble(),
                    TempMax = root.GetProperty("main").GetProperty("temp_max").GetDouble(),
                    
                    // Other data
                    Humidity = root.GetProperty("main").GetProperty("humidity").GetInt32(),
                    Pressure = root.GetProperty("main").GetProperty("pressure").GetInt32(),
                    WindSpeed = root.GetProperty("wind").GetProperty("speed").GetDouble(),
                    WindDegree = root.GetProperty("wind").TryGetProperty("deg", out var deg) ? deg.GetInt32() : 0,
                    
                    // Weather description (e.g., "clear sky", "light rain")
                    Description = root.GetProperty("weather")[0].GetProperty("description").GetString() ?? "",
                    Icon = root.GetProperty("weather")[0].GetProperty("icon").GetString() ?? "",
                    
                    // Visibility (km mein convert)
                    Visibility = root.TryGetProperty("visibility", out var vis) ? vis.GetInt32() / 1000 : 0,
                    Clouds = root.GetProperty("clouds").GetProperty("all").GetInt32(),
                    
                    // Sunrise/Sunset (Unix timestamp se DateTime mein convert)
                    Sunrise = DateTimeOffset.FromUnixTimeSeconds(
                        root.GetProperty("sys").GetProperty("sunrise").GetInt64()
                    ).LocalDateTime,
                    Sunset = DateTimeOffset.FromUnixTimeSeconds(
                        root.GetProperty("sys").GetProperty("sunset").GetInt64()
                    ).LocalDateTime
                };

                return weather;
            }
            catch (Exception ex)
            {
                // Koi error aaya - log karo aur null return karo
                _logger.LogError($"Error fetching weather: {ex.Message}");
                return null;
            }
        }
    }
}
