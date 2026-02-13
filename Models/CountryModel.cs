namespace MyApps.Models
{
    /// <summary>
    /// Model for country search input
    /// </summary>
    public class CountrySearchModel
    {
        public string Country { get; set; } = string.Empty;
    }

    /// <summary>
    /// Model for country information from REST Countries API
    /// </summary>
    public class CountryInfoModel
    {
        public string Name { get; set; } = string.Empty;
        public string OfficialName { get; set; } = string.Empty;
        public string Capital { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string SubRegion { get; set; } = string.Empty;
        public long Population { get; set; }
        public double Area { get; set; }
        public string FlagUrl { get; set; } = string.Empty;
        public string FlagAlt { get; set; } = string.Empty;
        public string CoatOfArmsUrl { get; set; } = string.Empty;
        public List<string> Languages { get; set; } = new();
        public List<CurrencyInfoDetail> Currencies { get; set; } = new();
        public List<string> Timezones { get; set; } = new();
        public List<string> Borders { get; set; } = new();
        public string GoogleMapsUrl { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Landlocked { get; set; }
        public bool UnMember { get; set; }
        public bool Independent { get; set; }
        public string DrivingSide { get; set; } = string.Empty;
        public string PhoneCode { get; set; } = string.Empty;
        public string TopLevelDomain { get; set; } = string.Empty;
        public string CountryCode2 { get; set; } = string.Empty;
        public string CountryCode3 { get; set; } = string.Empty;
        public string FifaCode { get; set; } = string.Empty;
        public string StartOfWeek { get; set; } = string.Empty;
    }

    /// <summary>
    /// Currency detail model
    /// </summary>
    public class CurrencyInfoDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
    }
}
