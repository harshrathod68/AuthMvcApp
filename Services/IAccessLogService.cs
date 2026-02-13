using MyApps.Models;

namespace MyApps.Services
{
    public interface IAccessLogService
    {
        Task LogAccessAsync(int userId, string userName, string userEmail);
        Task<List<AccessLogModel>> GetAllAccessLogsAsync();
        Task<List<AccessLogModel>> GetUserAccessLogsAsync(int userId);
        Task<List<DailyAccessSummary>> GetDailyAccessSummaryAsync();
        Task<int> GetTodayAccessCountAsync(int userId);
    }
}
