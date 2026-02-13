using MyApps.Models;

namespace MyApps.Services
{
    /// <summary>
    /// Interface for Emergency Numbers Service
    /// </summary>
    public interface IEmergencyService
    {
        /// <summary>
        /// Get emergency numbers for a country
        /// </summary>
        Task<EmergencyNumbersModel?> GetEmergencyNumbersAsync(string country);
    }
}
