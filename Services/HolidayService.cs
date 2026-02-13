using MyApps.Models;
using System.Text.Json;

namespace MyApps.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HolidayService> _logger;
        private const string BASE_URL = "https://calendarific.com/api/v2";
        private const string API_KEY = "J1PDTx04961Mk2fJtxXmbF2jcLQccHkj";

        private readonly Dictionary<string, string> _countryMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
            { "Portugal", "PT" }, { "Greece", "GR" }, { "New Zealand", "NZ" },
            { "Singapore", "SG" }, { "Malaysia", "MY" }, { "Thailand", "TH" },
            { "Indonesia", "ID" }, { "Philippines", "PH" }, { "Vietnam", "VN" },
            { "Pakistan", "PK" }, { "Bangladesh", "BD" }, { "Sri Lanka", "LK" },
            { "Nepal", "NP" }, { "Saudi Arabia", "SA" }, { "UAE", "AE" },
            { "Turkey", "TR" }, { "Egypt", "EG" }, { "South Africa", "ZA" },
            { "Nigeria", "NG" }, { "Kenya", "KE" }, { "Argentina", "AR" },
            { "Chile", "CL" }, { "Colombia", "CO" }, { "Peru", "PE" }
        };

        public HolidayService(HttpClient httpClient, ILogger<HolidayService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<PublicHoliday>> GetPublicHolidaysAsync(string countryCode, int year)
        {
            try
            {
                // Convert country name to code if needed
                if (countryCode.Length > 2 && _countryMapping.TryGetValue(countryCode, out var code))
                    countryCode = code;
                else if (countryCode.Length > 2)
                    return new List<PublicHoliday>();

                var url = $"{BASE_URL}/holidays?api_key={API_KEY}&country={countryCode.ToUpper()}&year={year}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<PublicHoliday>();

                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(content))
                    return new List<PublicHoliday>();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<CalendarificResponse>(content, options);
                
                if (apiResponse?.Response?.Holidays == null)
                    return new List<PublicHoliday>();

                // Convert Calendarific format to our format
                return apiResponse.Response.Holidays.Select(h => new PublicHoliday
                {
                    Date = h.Date?.Iso,
                    Name = h.Name,
                    LocalName = h.Description,
                    CountryCode = countryCode.ToUpper(),
                    Fixed = true,
                    Global = h.Type?.Contains("National") ?? false,
                    Types = h.Type ?? new List<string>()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching holidays: {ex.Message}");
                return new List<PublicHoliday>();
            }
        }

        public async Task<List<AvailableCountry>> GetAvailableCountriesAsync()
        {
            try
            {
                var url = $"{BASE_URL}/countries?api_key={API_KEY}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<AvailableCountry>();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<CalendarificCountriesResponse>(content, options);
                
                if (apiResponse?.Response?.Countries == null)
                    return new List<AvailableCountry>();

                return apiResponse.Response.Countries.Select(c => new AvailableCountry
                {
                    CountryCode = c.Iso3166,
                    Name = c.CountryName
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching countries: {ex.Message}");
                return new List<AvailableCountry>();
            }
        }
    }

    // Calendarific API response models
    public class CalendarificResponse
    {
        public CalendarificMeta? Meta { get; set; }
        public CalendarificData? Response { get; set; }
    }

    public class CalendarificMeta
    {
        public int Code { get; set; }
    }

    public class CalendarificData
    {
        public List<CalendarificHoliday>? Holidays { get; set; }
    }

    public class CalendarificHoliday
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public CalendarificDate? Date { get; set; }
        public List<string>? Type { get; set; }
    }

    public class CalendarificDate
    {
        public string? Iso { get; set; }
    }

    public class CalendarificCountriesResponse
    {
        public CalendarificMeta? Meta { get; set; }
        public CalendarificCountriesData? Response { get; set; }
    }

    public class CalendarificCountriesData
    {
        public List<CalendarificCountry>? Countries { get; set; }
    }

    public class CalendarificCountry
    {
        public string? CountryName { get; set; }
        public string? Iso3166 { get; set; }
    }
}
