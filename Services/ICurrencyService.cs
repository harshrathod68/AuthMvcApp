using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    /// <summary>
    /// Interface for currency conversion service
    /// Defines the contract for fetching exchange rates and converting currencies
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Converts an amount from one currency to another
        /// </summary>
        /// <param name="amount">The amount to convert</param>
        /// <param name="fromCurrency">Source currency code (e.g., USD)</param>
        /// <param name="toCurrency">Target currency code (e.g., INR)</param>
        /// <returns>CurrencyResultModel with conversion details, or null if conversion fails</returns>
        Task<CurrencyResultModel?> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
