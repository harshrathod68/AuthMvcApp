using MyApps.Models;

namespace MyApps.Services
{
    /// <summary>
    /// Service for handling time zone conversions
    /// Uses .NET's built-in TimeZoneInfo for accurate conversions
    /// </summary>
    public class TimeZoneService : ITimeZoneService
    {
        private readonly ILogger<TimeZoneService> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="logger">Logger for logging information and errors</param>
        public TimeZoneService(ILogger<TimeZoneService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Converts a datetime from one timezone to another
        /// </summary>
        /// <param name="dateTime">The datetime to convert</param>
        /// <param name="fromTimeZoneId">Source timezone ID (Windows timezone ID)</param>
        /// <param name="toTimeZoneId">Target timezone ID (Windows timezone ID)</param>
        /// <returns>TimeZoneResultModel with conversion details</returns>
        public TimeZoneResultModel? ConvertTime(DateTime dateTime, string fromTimeZoneId, string toTimeZoneId)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(fromTimeZoneId) || string.IsNullOrWhiteSpace(toTimeZoneId))
                {
                    _logger.LogWarning("Invalid timezone IDs provided");
                    return null;
                }

                _logger.LogInformation("Converting time from {From} to {To}", fromTimeZoneId, toTimeZoneId);

                // Get TimeZoneInfo objects for both timezones
                TimeZoneInfo fromTimeZone;
                TimeZoneInfo toTimeZone;

                try
                {
                    fromTimeZone = TimeZoneInfo.FindSystemTimeZoneById(fromTimeZoneId);
                    toTimeZone = TimeZoneInfo.FindSystemTimeZoneById(toTimeZoneId);
                }
                catch (TimeZoneNotFoundException ex)
                {
                    _logger.LogError(ex, "Timezone not found: {From} or {To}", fromTimeZoneId, toTimeZoneId);
                    return null;
                }

                // Create DateTime with the source timezone
                var sourceDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
                
                // Convert to UTC first, then to target timezone
                var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(sourceDateTime, fromTimeZone);
                var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, toTimeZone);

                // Calculate time difference
                var fromOffset = fromTimeZone.GetUtcOffset(sourceDateTime);
                var toOffset = toTimeZone.GetUtcOffset(convertedDateTime);
                var timeDifference = toOffset - fromOffset;

                // Get display info for both timezones
                var fromInfo = SupportedTimeZones.TimeZones.GetValueOrDefault(fromTimeZoneId);
                var toInfo = SupportedTimeZones.TimeZones.GetValueOrDefault(toTimeZoneId);

                // Build result model
                var result = new TimeZoneResultModel
                {
                    OriginalDateTime = sourceDateTime,
                    ConvertedDateTime = convertedDateTime,
                    FromTimeZoneName = fromInfo?.Location ?? fromTimeZone.DisplayName,
                    ToTimeZoneName = toInfo?.Location ?? toTimeZone.DisplayName,
                    FromAbbreviation = fromInfo?.Abbreviation ?? GetAbbreviation(fromTimeZone),
                    ToAbbreviation = toInfo?.Abbreviation ?? GetAbbreviation(toTimeZone),
                    TimeDifference = timeDifference
                };

                _logger.LogInformation("Conversion successful: {Original} {From} = {Converted} {To}",
                    sourceDateTime, fromTimeZoneId, convertedDateTime, toTimeZoneId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during time zone conversion");
                return null;
            }
        }

        /// <summary>
        /// Gets the current time in a specific timezone
        /// </summary>
        /// <param name="timeZoneId">The timezone ID</param>
        /// <returns>Current DateTime in the specified timezone</returns>
        public DateTime GetCurrentTimeInZone(string timeZoneId)
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current time for timezone: {TimeZone}", timeZoneId);
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Validates if a timezone ID is valid
        /// </summary>
        /// <param name="timeZoneId">The timezone ID to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidTimeZone(string timeZoneId)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts abbreviation from TimeZoneInfo display name
        /// </summary>
        /// <param name="timeZone">The TimeZoneInfo object</param>
        /// <returns>Abbreviated timezone name</returns>
        private static string GetAbbreviation(TimeZoneInfo timeZone)
        {
            // Try to extract abbreviation from display name
            var displayName = timeZone.DisplayName;
            if (displayName.Contains("(") && displayName.Contains(")"))
            {
                var start = displayName.IndexOf("(") + 1;
                var end = displayName.IndexOf(")");
                if (end > start)
                {
                    return displayName.Substring(start, end - start);
                }
            }
            return timeZone.StandardName;
        }
    }
}
