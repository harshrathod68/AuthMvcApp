using MyApps.Models;

namespace MyApps.Services
{
    /// <summary>
    /// Interface for country information service
    /// Uses REST Countries API to fetch country details
    /// </summary>
    public interface ICountryService
    {
        /// <summary>
        /// Gets country information by country name
        /// </summary>
        /// <param name="countryName">Name of the country to search</param>
        /// <returns>Country information or null if not found</returns>
        Task<CountryInfoModel?> GetCountryInfoAsync(string countryName);
    }
}
