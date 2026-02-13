namespace MyApps.Models
{
    /// <summary>
    /// Model for tracking user access/login history
    /// </summary>
    public class AccessLogModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime AccessTime { get; set; }
        public string AccessDate { get; set; } = string.Empty; // For grouping by date
    }

    /// <summary>
    /// Model for daily access summary
    /// </summary>
    public class DailyAccessSummary
    {
        public string Date { get; set; } = string.Empty;
        public int TotalAccess { get; set; }
        public List<AccessLogModel> AccessLogs { get; set; } = new List<AccessLogModel>();
    }
}
