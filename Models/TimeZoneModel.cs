using System.ComponentModel.DataAnnotations;

namespace AuthMvcApp.Models
{
    /// <summary>
    /// Model for time zone conversion input
    /// Contains the source time, date, and timezone information
    /// </summary>
    public class TimeZoneConversionModel
    {
        [Required(ErrorMessage = "Please select a time")]
        [Display(Name = "Time")]
        public string Time { get; set; } = DateTime.Now.ToString("HH:mm");

        [Required(ErrorMessage = "Please select a date")]
        [Display(Name = "Date")]
        public string Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        [Required(ErrorMessage = "Please select source timezone")]
        [Display(Name = "From Timezone")]
        public string FromTimeZone { get; set; } = "India Standard Time";

        [Required(ErrorMessage = "Please select target timezone")]
        [Display(Name = "To Timezone")]
        public string ToTimeZone { get; set; } = "Eastern Standard Time";
    }

    /// <summary>
    /// Model for time zone conversion result
    /// Contains both original and converted time details
    /// </summary>
    public class TimeZoneResultModel
    {
        public DateTime OriginalDateTime { get; set; }
        public DateTime ConvertedDateTime { get; set; }
        public string FromTimeZoneName { get; set; } = string.Empty;
        public string ToTimeZoneName { get; set; } = string.Empty;
        public string FromAbbreviation { get; set; } = string.Empty;
        public string ToAbbreviation { get; set; } = string.Empty;
        public TimeSpan TimeDifference { get; set; }
    }

    /// <summary>
    /// Static class containing supported time zones with their details
    /// Includes major time zones from around the world
    /// </summary>
    public static class SupportedTimeZones
    {
        /// <summary>
        /// Dictionary of supported time zones with display information
        /// Key: Windows TimeZone ID, Value: TimeZoneDisplayInfo
        /// </summary>
        public static readonly Dictionary<string, TimeZoneDisplayInfo> TimeZones = new()
        {
            // Asia
            { "India Standard Time", new TimeZoneDisplayInfo("India Standard Time", "IST", "🇮🇳", "India", "+05:30") },
            { "China Standard Time", new TimeZoneDisplayInfo("China Standard Time", "CST", "🇨🇳", "China", "+08:00") },
            { "Tokyo Standard Time", new TimeZoneDisplayInfo("Tokyo Standard Time", "JST", "🇯🇵", "Japan", "+09:00") },
            { "Singapore Standard Time", new TimeZoneDisplayInfo("Singapore Standard Time", "SGT", "🇸🇬", "Singapore", "+08:00") },
            { "Arabian Standard Time", new TimeZoneDisplayInfo("Arabian Standard Time", "AST", "🇦🇪", "Dubai/UAE", "+04:00") },
            { "Korea Standard Time", new TimeZoneDisplayInfo("Korea Standard Time", "KST", "🇰🇷", "South Korea", "+09:00") },
            
            // Americas
            { "Eastern Standard Time", new TimeZoneDisplayInfo("Eastern Standard Time", "EST", "🇺🇸", "USA (New York)", "-05:00") },
            { "Central Standard Time", new TimeZoneDisplayInfo("Central Standard Time", "CST", "🇺🇸", "USA (Chicago)", "-06:00") },
            { "Mountain Standard Time", new TimeZoneDisplayInfo("Mountain Standard Time", "MST", "🇺🇸", "USA (Denver)", "-07:00") },
            { "Pacific Standard Time", new TimeZoneDisplayInfo("Pacific Standard Time", "PST", "🇺🇸", "USA (Los Angeles)", "-08:00") },
            { "E. South America Standard Time", new TimeZoneDisplayInfo("E. South America Standard Time", "BRT", "🇧🇷", "Brazil", "-03:00") },
            
            // Europe
            { "GMT Standard Time", new TimeZoneDisplayInfo("GMT Standard Time", "GMT", "🇬🇧", "UK (London)", "+00:00") },
            { "Central European Standard Time", new TimeZoneDisplayInfo("Central European Standard Time", "CET", "🇩🇪", "Germany/France", "+01:00") },
            { "Russian Standard Time", new TimeZoneDisplayInfo("Russian Standard Time", "MSK", "🇷🇺", "Russia (Moscow)", "+03:00") },
            
            // Oceania
            { "AUS Eastern Standard Time", new TimeZoneDisplayInfo("AUS Eastern Standard Time", "AEST", "🇦🇺", "Australia (Sydney)", "+10:00") },
            { "New Zealand Standard Time", new TimeZoneDisplayInfo("New Zealand Standard Time", "NZST", "🇳🇿", "New Zealand", "+12:00") },
            
            // UTC
            { "UTC", new TimeZoneDisplayInfo("UTC", "UTC", "🌍", "Coordinated Universal Time", "+00:00") }
        };
    }

    /// <summary>
    /// Display information for a time zone
    /// </summary>
    public class TimeZoneDisplayInfo
    {
        public string Id { get; set; }
        public string Abbreviation { get; set; }
        public string Flag { get; set; }
        public string Location { get; set; }
        public string Offset { get; set; }

        public TimeZoneDisplayInfo(string id, string abbreviation, string flag, string location, string offset)
        {
            Id = id;
            Abbreviation = abbreviation;
            Flag = flag;
            Location = location;
            Offset = offset;
        }
    }
}
