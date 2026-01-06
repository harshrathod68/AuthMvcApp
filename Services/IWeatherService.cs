using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    public interface IWeatherService
    {
        Task<WeatherModel?> GetWeatherAsync(string city);
    }
}
