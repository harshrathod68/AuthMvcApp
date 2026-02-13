using MyApps.Models;
using System.Text.Json;

namespace MyApps.Services
{
    public class TimeTrackService : ITimeTrackService
    {
        private readonly string _dataPath = "Data/timetrack.json";

        public async Task<List<TimeTrackEntry>> GetAllEntriesAsync(string userId)
        {
            var entries = await LoadEntriesAsync();
            return entries.Where(e => e.UserId == userId).OrderByDescending(e => e.Date).ThenByDescending(e => e.StartTime).ToList();
        }

        public async Task<TimeTrackEntry?> GetByIdAsync(int id, string userId)
        {
            var entries = await LoadEntriesAsync();
            return entries.FirstOrDefault(e => e.Id == id && e.UserId == userId);
        }

        public async Task<TimeTrackEntry> AddEntryAsync(string userId, string workName, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var entries = await LoadEntriesAsync();
            
            // Calculate total time
            var totalTime = endTime - startTime;
            if (totalTime < TimeSpan.Zero)
            {
                totalTime = totalTime.Add(TimeSpan.FromDays(1)); // Handle overnight work
            }

            var newEntry = new TimeTrackEntry
            {
                Id = entries.Any() ? entries.Max(e => e.Id) + 1 : 1,
                UserId = userId,
                WorkName = workName,
                Date = date.Date,
                StartTime = startTime,
                EndTime = endTime,
                TotalTime = totalTime,
                CreatedDate = DateTime.Now
            };

            entries.Add(newEntry);
            await SaveEntriesAsync(entries);
            return newEntry;
        }

        public async Task<bool> UpdateEntryAsync(TimeTrackEntry entry)
        {
            var entries = await LoadEntriesAsync();
            var existing = entries.FirstOrDefault(e => e.Id == entry.Id && e.UserId == entry.UserId);
            
            if (existing != null)
            {
                // Recalculate total time
                var totalTime = entry.EndTime - entry.StartTime;
                if (totalTime < TimeSpan.Zero)
                {
                    totalTime = totalTime.Add(TimeSpan.FromDays(1));
                }

                existing.WorkName = entry.WorkName;
                existing.Date = entry.Date.Date;
                existing.StartTime = entry.StartTime;
                existing.EndTime = entry.EndTime;
                existing.TotalTime = totalTime;
                
                await SaveEntriesAsync(entries);
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteEntryAsync(int id, string userId)
        {
            var entries = await LoadEntriesAsync();
            var entry = entries.FirstOrDefault(e => e.Id == id && e.UserId == userId);
            
            if (entry != null)
            {
                entries.Remove(entry);
                await SaveEntriesAsync(entries);
                return true;
            }
            return false;
        }

        private async Task<List<TimeTrackEntry>> LoadEntriesAsync()
        {
            if (!File.Exists(_dataPath))
                return new List<TimeTrackEntry>();

            var json = await File.ReadAllTextAsync(_dataPath);
            return JsonSerializer.Deserialize<List<TimeTrackEntry>>(json) ?? new List<TimeTrackEntry>();
        }

        private async Task SaveEntriesAsync(List<TimeTrackEntry> entries)
        {
            var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_dataPath, json);
        }
    }
}
