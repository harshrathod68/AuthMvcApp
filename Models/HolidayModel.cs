namespace MyApps.Models
{
    /// <summary>
    /// Model for Holiday search
    /// </summary>
    public class HolidaySearchModel
    {
        public string Country { get; set; } = "India";
        public int Year { get; set; } = DateTime.Now.Year;
    }

    /// <summary>
    /// Public Holiday model
    /// </summary>
    public class PublicHoliday
    {
        public string? Date { get; set; }
        public string? LocalName { get; set; }
        public string? Name { get; set; }
        public string? CountryCode { get; set; }
        public bool Fixed { get; set; }
        public bool Global { get; set; }
        public List<string>? Types { get; set; }
    }

    /// <summary>
    /// Available country model
    /// </summary>
    public class AvailableCountry
    {
        public string? CountryCode { get; set; }
        public string? Name { get; set; }
    }
}
