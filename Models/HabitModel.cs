using System.ComponentModel.DataAnnotations;

namespace AuthMvcApp.Models
{
    public class HabitModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        public string Name { get; set; } = "";
        
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "🎯";
        public string Color { get; set; } = "#667eea";
        public string Category { get; set; } = "custom";
        public List<HabitLog> Logs { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }

    public class HabitLog
    {
        public string Date { get; set; } = ""; // yyyy-MM-dd format
        public bool IsCompleted { get; set; }
    }

    public class HabitCreateModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        public string Name { get; set; } = "";
        
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "🎯";
        public string Color { get; set; } = "#667eea";
        public string Category { get; set; } = "custom";
    }

    public class HabitStats
    {
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public int TotalCompleted { get; set; }
        public double CompletionRate { get; set; }
        public Dictionary<string, bool> WeeklyStatus { get; set; } = new();
    }

    public class CalendarData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public int FirstDayOfWeek { get; set; }
        public int TotalDays { get; set; }
        public Dictionary<int, int> DayStatus { get; set; } = new(); // 0=none, 1=some, 2=all
    }

    public class ProgressData
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Values { get; set; } = new();
    }
}
