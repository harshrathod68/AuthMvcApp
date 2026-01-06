using System.Text.Json;
using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    /// <summary>
    /// Service for handling currency conversion using ExchangeRate-API
    /// This service fetches real-time exchange rates and performs currency conversions
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        
        // Base URL for the free ExchangeRate-API (no API key required)
        private const string BaseUrl = "https://api.exchangerate-api.com/v4/latest";

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="httpClient">HttpClient for making API requests</param>
        /// <param name="logger">Logger for logging information and errors</param>
        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Converts an amount from one currency to another using real-time exchange rates
        /// </summary>
        /// <param name="amount">The amount to convert</param>
        /// <param name="fromCurrency">Source currency code</param>
        /// <param name="toCurrency">Target currency code</param>
        /// <returns>CurrencyResultModel with conversion details</returns>
        public async Task<CurrencyResultModel?> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            try
            {
                // Validate input parameters
                if (amount <= 0)
                {
                    _logger.LogWarning("Invalid amount provided: {Amount}", amount);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
                {
                    _logger.LogWarning("Invalid currency codes provided");
                    return null;
                }

                // Normalize currency codes to uppercase
                fromCurrency = fromCurrency.ToUpper().Trim();
                toCurrency = toCurrency.ToUpper().Trim();

                _logger.LogInformation("Converting {Amount} {From} to {To}", amount, fromCurrency, toCurrency);

                // Build API URL and fetch exchange rates
                var apiUrl = $"{BaseUrl}/{fromCurrency}";
                var response = await _httpClient.GetAsync(apiUrl);

                // Check if API request was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API request failed with status: {StatusCode}", response.StatusCode);
                    return null;
                }

                // Parse JSON response
                var jsonContent = await response.Content.ReadAsStringAsync();
                var exchangeData = JsonDocument.Parse(jsonContent);
                var root = exchangeData.RootElement;

                // Extract exchange rate for target currency
                if (!root.GetProperty("rates").TryGetProperty(toCurrency, out var rateElement))
                {
                    _logger.LogWarning("Target currency {Currency} not found in rates", toCurrency);
                    return null;
                }

                var exchangeRate = rateElement.GetDecimal();
                var convertedAmount = amount * exchangeRate;

                // Parse last updated time
                var timeLastUpdated = root.GetProperty("time_last_updated").GetInt64();
                var lastUpdated = DateTimeOffset.FromUnixTimeSeconds(timeLastUpdated).LocalDateTime;

                // Build and return result model
                var result = new CurrencyResultModel
                {
                    OriginalAmount = amount,
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    ConvertedAmount = Math.Round(convertedAmount, 2),
                    ExchangeRate = Math.Round(exchangeRate, 6),
                    LastUpdated = lastUpdated
                };

                _logger.LogInformation("Conversion successful: {Amount} {From} = {Result} {To}", 
                    amount, fromCurrency, result.ConvertedAmount, toCurrency);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while fetching exchange rates");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing API response");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during currency conversion");
                return null;
            }
        }
    }
}
