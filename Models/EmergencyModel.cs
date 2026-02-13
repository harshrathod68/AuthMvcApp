namespace MyApps.Models
{
    /// <summary>
    /// Model for Emergency Numbers feature
    /// </summary>
    public class EmergencySearchModel
    {
        public string Country { get; set; } = "India";
    }

    /// <summary>
    /// API Response wrapper
    /// </summary>
    public class EmergencyApiResponse
    {
        public string? Disclaimer { get; set; }
        public string? Error { get; set; }
        public EmergencyNumbersModel? Data { get; set; }
    }

    /// <summary>
    /// Emergency numbers data model
    /// </summary>
    public class EmergencyNumbersModel
    {
        public CountryInfo? Country { get; set; }
        public EmergencyServiceNumbers? Ambulance { get; set; }
        public EmergencyServiceNumbers? Fire { get; set; }
        public EmergencyServiceNumbers? Police { get; set; }
        public EmergencyServiceNumbers? Dispatch { get; set; }
        public bool Member_112 { get; set; }
        public EmergencyServiceNumbers? Traffic { get; set; }
    }

    public class CountryInfo
    {
        public string? Name { get; set; }
        public string? ISOCode { get; set; }
        public string? ISONumeric { get; set; }
    }

    public class EmergencyServiceNumbers
    {
        public List<string>? All { get; set; }
    }
}
