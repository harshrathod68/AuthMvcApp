using MyApps.Models;
using System.Text.Json;

namespace MyApps.Services
{
    /// <summary>
    /// Emergency Numbers Service using Emergency Number API
    /// </summary>
    public class EmergencyService : IEmergencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmergencyService> _logger;
        private const string BASE_URL = "https://emergencynumberapi.com/api/country";

        // Country name to ISO code mapping
        private readonly Dictionary<string, string> _countryISOCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "India", "IN" }, { "USA", "US" }, { "United States", "US" },
            { "UK", "GB" }, { "United Kingdom", "GB" }, { "Canada", "CA" },
            { "Australia", "AU" }, { "Germany", "DE" }, { "France", "FR" },
            { "Italy", "IT" }, { "Spain", "ES" }, { "Japan", "JP" },
            { "China", "CN" }, { "Brazil", "BR" }, { "Mexico", "MX" },
            { "Russia", "RU" }, { "South Korea", "KR" }, { "Netherlands", "NL" },
            { "Switzerland", "CH" }, { "Sweden", "SE" }, { "Norway", "NO" },
            { "Denmark", "DK" }, { "Finland", "FI" }, { "Poland", "PL" },
            { "Belgium", "BE" }, { "Austria", "AT" }, { "Ireland", "IE" },
            { "Portugal", "PT" }, { "Greece", "GR" }, { "Czech Republic", "CZ" },
            { "New Zealand", "NZ" }, { "Singapore", "SG" }, { "Malaysia", "MY" },
            { "Thailand", "TH" }, { "Indonesia", "ID" }, { "Philippines", "PH" },
            { "Vietnam", "VN" }, { "Pakistan", "PK" }, { "Bangladesh", "BD" },
            { "Sri Lanka", "LK" }, { "Nepal", "NP" }, { "Afghanistan", "AF" },
            { "Saudi Arabia", "SA" }, { "UAE", "AE" }, { "Turkey", "TR" },
            { "Egypt", "EG" }, { "South Africa", "ZA" }, { "Nigeria", "NG" },
            { "Kenya", "KE" }, { "Argentina", "AR" }, { "Chile", "CL" },
            { "Colombia", "CO" }, { "Peru", "PE" }, { "Venezuela", "VE" }
        };

        public EmergencyService(HttpClient httpClient, ILogger<EmergencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Get emergency numbers for a country
        /// </summary>
        public async Task<EmergencyNumbersModel?> GetEmergencyNumbersAsync(string country)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(country))
                    return null;

                // Convert country name to ISO code
                string isoCode;
                if (country.Length == 2)
                {
                    isoCode = country.ToUpper();
                }
                else if (!_countryISOCodes.TryGetValue(country, out isoCode))
                {
                    return null;
                }

                var url = $"{BASE_URL}/{isoCode}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(content) || content == "[]" || content == "{}")
                    return null;
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var apiResponse = JsonSerializer.Deserialize<EmergencyApiResponse>(content, options);
                
                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching emergency numbers: {ex.Message}");
                return null;
            }
        }
    }
}
