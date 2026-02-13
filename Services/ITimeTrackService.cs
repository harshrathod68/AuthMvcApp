using MyApps.Models;

namespace MyApps.Services
{
    public interface ITimeTrackService
    {
        Task<List<TimeTrackEntry>> GetAllEntriesAsync(string userId);
        Task<TimeTrackEntry?> GetByIdAsync(int id, string userId);
        Task<TimeTrackEntry> AddEntryAsync(string userId, string workName, DateTime date, TimeSpan startTime, TimeSpan endTime);
        Task<bool> UpdateEntryAsync(TimeTrackEntry entry);
        Task<bool> DeleteEntryAsync(int id, string userId);
    }
}
