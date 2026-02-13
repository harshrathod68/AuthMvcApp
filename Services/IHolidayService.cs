using MyApps.Models;

namespace MyApps.Services
{
    public interface IHolidayService
    {
        Task<List<PublicHoliday>> GetPublicHolidaysAsync(string countryCode, int year);
        Task<List<AvailableCountry>> GetAvailableCountriesAsync();
    }
}
