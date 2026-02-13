using MyApps.Models;

namespace MyApps.Services
{
    public interface IWeatherService
    {
        Task<WeatherModel?> GetWeatherAsync(string city);
    }
}
