using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    /// <summary>
    /// Interface for time zone conversion service
    /// Defines the contract for converting time between different time zones
    /// </summary>
    public interface ITimeZoneService
    {
        /// <summary>
        /// Converts a datetime from one timezone to another
        /// </summary>
        /// <param name="dateTime">The datetime to convert</param>
        /// <param name="fromTimeZoneId">Source timezone ID</param>
        /// <param name="toTimeZoneId">Target timezone ID</param>
        /// <returns>TimeZoneResultModel with conversion details, or null if conversion fails</returns>
        TimeZoneResultModel? ConvertTime(DateTime dateTime, string fromTimeZoneId, string toTimeZoneId);

        /// <summary>
        /// Gets the current time in a specific timezone
        /// </summary>
        /// <param name="timeZoneId">The timezone ID</param>
        /// <returns>Current DateTime in the specified timezone</returns>
        DateTime GetCurrentTimeInZone(string timeZoneId);

        /// <summary>
        /// Validates if a timezone ID is valid
        /// </summary>
        /// <param name="timeZoneId">The timezone ID to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValidTimeZone(string timeZoneId);
    }
}
