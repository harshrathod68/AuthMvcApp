namespace MyApps.Models
{
    /// <summary>
    /// Time Track Entry Model
    /// </summary>
    public class TimeTrackEntry
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string WorkName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan TotalTime { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
