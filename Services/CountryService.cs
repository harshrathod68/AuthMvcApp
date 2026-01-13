using System.Text.Json;
using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    /// <summary>
    /// Service for fetching country information from REST Countries API
    /// API: https://restcountries.com/v3.1/name/{country}
    /// </summary>
    public class CountryService : ICountryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CountryService> _logger;
        private const string BaseUrl = "https://restcountries.com/v3.1";

        /// <summary>
        /// Constructor with HttpClient injection
        /// </summary>
        public CountryService(HttpClient httpClient, ILogger<CountryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Gets country information by name from REST Countries API
        /// Uses fullText=true for exact match first, then falls back to partial search
        /// </summary>
        public async Task<CountryInfoModel?> GetCountryInfoAsync(string countryName)
        {
            try
            {
                // First try exact match with fullText=true
                var exactUrl = $"{BaseUrl}/name/{Uri.EscapeDataString(countryName)}?fullText=true";
                var response = await _httpClient.GetAsync(exactUrl);

                JsonElement[]? countries = null;

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    countries = JsonSerializer.Deserialize<JsonElement[]>(json);
                }

                // If exact match not found, try partial search
                if (countries == null || countries.Length == 0)
                {
                    var partialUrl = $"{BaseUrl}/name/{Uri.EscapeDataString(countryName)}";
                    response = await _httpClient.GetAsync(partialUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Country not found: {Country}", countryName);
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    countries = JsonSerializer.Deserialize<JsonElement[]>(json);
                }

                if (countries == null || countries.Length == 0)
                {
                    return null;
                }

                // If multiple results, select the most relevant one (highest population)
                // This ensures "India" returns India, not "British Indian Ocean Territory"
                var country = countries[0];
                if (countries.Length > 1)
                {
                    long maxPopulation = 0;
                    foreach (var c in countries)
                    {
                        if (c.TryGetProperty("population", out var pop))
                        {
                            var population = pop.GetInt64();
                            if (population > maxPopulation)
                            {
                                maxPopulation = population;
                                country = c;
                            }
                        }
                    }
                }

                return ParseCountryData(country);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching country info for: {Country}", countryName);
                return null;
            }
        }

        /// <summary>
        /// Parses JSON response into CountryInfoModel
        /// </summary>
        private CountryInfoModel ParseCountryData(JsonElement country)
        {
            var model = new CountryInfoModel();

            // Basic info
            if (country.TryGetProperty("name", out var name))
            {
                model.Name = GetStringValue(name, "common");
                model.OfficialName = GetStringValue(name, "official");
            }

            // Capital
            if (country.TryGetProperty("capital", out var capital) && capital.ValueKind == JsonValueKind.Array)
            {
                var capitals = capital.EnumerateArray().Select(c => c.GetString()).Where(c => c != null);
                model.Capital = string.Join(", ", capitals);
            }

            // Region & Subregion
            model.Region = GetStringValue(country, "region");
            model.SubRegion = GetStringValue(country, "subregion");

            // Population & Area
            if (country.TryGetProperty("population", out var pop))
                model.Population = pop.GetInt64();
            if (country.TryGetProperty("area", out var area))
                model.Area = area.GetDouble();

            // Flag
            if (country.TryGetProperty("flags", out var flags))
            {
                model.FlagUrl = GetStringValue(flags, "svg");
                model.FlagAlt = GetStringValue(flags, "alt");
            }

            // Coat of Arms
            if (country.TryGetProperty("coatOfArms", out var coa))
            {
                model.CoatOfArmsUrl = GetStringValue(coa, "svg");
            }

            // Languages
            if (country.TryGetProperty("languages", out var languages) && languages.ValueKind == JsonValueKind.Object)
            {
                model.Languages = languages.EnumerateObject()
                    .Select(l => l.Value.GetString() ?? "")
                    .Where(l => !string.IsNullOrEmpty(l))
                    .ToList();
            }

            // Currencies
            if (country.TryGetProperty("currencies", out var currencies) && currencies.ValueKind == JsonValueKind.Object)
            {
                model.Currencies = currencies.EnumerateObject()
                    .Select(c => new CurrencyInfoDetail
                    {
                        Code = c.Name,
                        Name = GetStringValue(c.Value, "name"),
                        Symbol = GetStringValue(c.Value, "symbol")
                    })
                    .ToList();
            }

            // Timezones
            if (country.TryGetProperty("timezones", out var timezones) && timezones.ValueKind == JsonValueKind.Array)
            {
                model.Timezones = timezones.EnumerateArray()
                    .Select(t => t.GetString() ?? "")
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }

            // Borders
            if (country.TryGetProperty("borders", out var borders) && borders.ValueKind == JsonValueKind.Array)
            {
                model.Borders = borders.EnumerateArray()
                    .Select(b => b.GetString() ?? "")
                    .Where(b => !string.IsNullOrEmpty(b))
                    .ToList();
            }

            // Maps
            if (country.TryGetProperty("maps", out var maps))
            {
                model.GoogleMapsUrl = GetStringValue(maps, "googleMaps");
            }

            // Coordinates
            if (country.TryGetProperty("latlng", out var latlng) && latlng.ValueKind == JsonValueKind.Array)
            {
                var coords = latlng.EnumerateArray().ToArray();
                if (coords.Length >= 2)
                {
                    model.Latitude = coords[0].GetDouble();
                    model.Longitude = coords[1].GetDouble();
                }
            }

            // Boolean properties
            if (country.TryGetProperty("landlocked", out var landlocked))
                model.Landlocked = landlocked.GetBoolean();
            if (country.TryGetProperty("unMember", out var unMember))
                model.UnMember = unMember.GetBoolean();
            if (country.TryGetProperty("independent", out var independent))
                model.Independent = independent.GetBoolean();

            // Car driving side
            if (country.TryGetProperty("car", out var car))
            {
                model.DrivingSide = GetStringValue(car, "side");
            }

            // Phone code (IDD)
            if (country.TryGetProperty("idd", out var idd))
            {
                var root = GetStringValue(idd, "root");
                if (idd.TryGetProperty("suffixes", out var suffixes) && suffixes.ValueKind == JsonValueKind.Array)
                {
                    var suffix = suffixes.EnumerateArray().FirstOrDefault().GetString() ?? "";
                    model.PhoneCode = root + suffix;
                }
            }

            // Top Level Domain
            if (country.TryGetProperty("tld", out var tld) && tld.ValueKind == JsonValueKind.Array)
            {
                model.TopLevelDomain = tld.EnumerateArray().FirstOrDefault().GetString() ?? "";
            }

            // Country codes
            model.CountryCode2 = GetStringValue(country, "cca2");
            model.CountryCode3 = GetStringValue(country, "cca3");
            model.FifaCode = GetStringValue(country, "fifa");
            model.StartOfWeek = GetStringValue(country, "startOfWeek");

            return model;
        }

        /// <summary>
        /// Helper to safely get string value from JSON
        /// </summary>
        private string GetStringValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? "";
            }
            return "";
        }
    }
}
