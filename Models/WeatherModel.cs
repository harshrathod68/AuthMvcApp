namespace MyApps.Models
{
    public class WeatherModel
    {
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
        public double WindSpeed { get; set; }
        public int WindDegree { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Visibility { get; set; }
        public int Clouds { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
    }

    public class WeatherSearchModel
    {
        public string City { get; set; } = string.Empty;
    }
}
