using MyApps.Models;
using System.Text.Json;

namespace MyApps.Services
{
    public class AccessLogService : IAccessLogService
    {
        private readonly string _filePath = "Data/accesslogs.json";
        private readonly ILogger<AccessLogService> _logger;

        public AccessLogService(ILogger<AccessLogService> logger)
        {
            _logger = logger;
            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        private async Task<List<AccessLogModel>> ReadLogsAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<List<AccessLogModel>>(json) ?? new List<AccessLogModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading access logs: {ex.Message}");
                return new List<AccessLogModel>();
            }
        }

        private async Task SaveLogsAsync(List<AccessLogModel> logs)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(logs, options);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving access logs: {ex.Message}");
            }
        }

        public async Task LogAccessAsync(int userId, string userName, string userEmail)
        {
            try
            {
                var logs = await ReadLogsAsync();
                var now = DateTime.Now;

                var newLog = new AccessLogModel
                {
                    Id = logs.Any() ? logs.Max(l => l.Id) + 1 : 1,
                    UserId = userId,
                    UserName = userName,
                    UserEmail = userEmail,
                    AccessTime = now,
                    AccessDate = now.ToString("yyyy-MM-dd")
                };

                logs.Add(newLog);
                await SaveLogsAsync(logs);

                _logger.LogInformation($"Access logged for user {userName} (ID: {userId})");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging access: {ex.Message}");
            }
        }

        public async Task<List<AccessLogModel>> GetAllAccessLogsAsync()
        {
            var logs = await ReadLogsAsync();
            return logs.OrderByDescending(l => l.AccessTime).ToList();
        }

        public async Task<List<AccessLogModel>> GetUserAccessLogsAsync(int userId)
        {
            var logs = await ReadLogsAsync();
            return logs.Where(l => l.UserId == userId)
                      .OrderByDescending(l => l.AccessTime)
                      .ToList();
        }

        public async Task<List<DailyAccessSummary>> GetDailyAccessSummaryAsync()
        {
            var logs = await ReadLogsAsync();
            
            var summary = logs.GroupBy(l => l.AccessDate)
                             .Select(g => new DailyAccessSummary
                             {
                                 Date = g.Key,
                                 TotalAccess = g.Count(),
                                 AccessLogs = g.OrderByDescending(l => l.AccessTime).ToList()
                             })
                             .OrderByDescending(s => s.Date)
                             .ToList();

            return summary;
        }

        public async Task<int> GetTodayAccessCountAsync(int userId)
        {
            var logs = await ReadLogsAsync();
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            
            return logs.Count(l => l.UserId == userId && l.AccessDate == today);
        }
    }
}
