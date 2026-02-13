using System.ComponentModel.DataAnnotations;

namespace MyApps.Models
{
    /// <summary>
    /// Enhanced Skill Mastery Model with complete tracking
    /// </summary>
    public class SkillMasteryModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Skill name is required")]
        [StringLength(100)]
        public string SkillName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total duration is required")]
        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days")]
        public int TotalDays { get; set; }

        [Required(ErrorMessage = "Daily time commitment is required")]
        [Range(15, 480, ErrorMessage = "Daily time must be between 15 and 480 minutes")]
        public int DailyMinutes { get; set; } // Daily time commitment

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Goal level is required")]
        public string GoalLevel { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced

        public string UserEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Progress tracking
        public int CompletedDays { get; set; }
        public int MissedDays { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalMinutesSpent { get; set; }

        // Calculated properties
        public double ProgressPercentage => TotalDays > 0 ? (CompletedDays * 100.0 / TotalDays) : 0;
        public double ConsistencyScore => DaysPassed > 0 ? (CompletedDays * 100.0 / DaysPassed) : 0;
        public int RemainingDays => TotalDays - CompletedDays;
        public int DaysPassed => (DateTime.Now.Date - StartDate.Date).Days + 1;
        public DateTime ExpectedEndDate => StartDate.AddDays(TotalDays);
        public bool IsCompleted => CompletedDays >= TotalDays;
        public bool IsActive => !IsCompleted && DateTime.Now.Date >= StartDate.Date;

        // Roadmap stages
        public List<SkillStageModel> Stages { get; set; } = new List<SkillStageModel>();
    }

    /// <summary>
    /// Skill learning stage (e.g., Basics, Intermediate, Advanced)
    /// </summary>
    public class SkillStageModel
    {
        public int Id { get; set; }
        public int SkillId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public int StartDay { get; set; }
        public int EndDay { get; set; }
        public string Topic { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

    /// <summary>
    /// Daily progress entry
    /// </summary>
    public class DailyProgressModel
    {
        public int Id { get; set; }
        public int SkillId { get; set; }
        public DateTime Date { get; set; }
        public int DayNumber { get; set; } // Day 1, Day 2, etc.
        
        public bool IsCompleted { get; set; }
        public int MinutesSpent { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        // What was learned today
        [StringLength(500)]
        public string? LearningHighlight { get; set; }
    }

    /// <summary>
    /// Model for creating a new skill
    /// </summary>
    public class CreateSkillMasteryModel
    {
        [Required(ErrorMessage = "Skill name is required")]
        [StringLength(100)]
        [Display(Name = "Skill Name")]
        public string SkillName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total duration is required")]
        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days")]
        [Display(Name = "Total Duration (Days)")]
        public int TotalDays { get; set; } = 60;

        [Required(ErrorMessage = "Daily time commitment is required")]
        [Range(15, 480, ErrorMessage = "Daily time must be between 15 and 480 minutes")]
        [Display(Name = "Daily Time Commitment (Minutes)")]
        public int DailyMinutes { get; set; } = 60;

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Goal level is required")]
        [Display(Name = "Goal Level")]
        public string GoalLevel { get; set; } = "Beginner";

        [Display(Name = "Use Predefined Roadmap")]
        public bool UsePredefinedRoadmap { get; set; } = true;
    }

    /// <summary>
    /// Dashboard view model
    /// </summary>
    public class SkillDashboardModel
    {
        public SkillMasteryModel Skill { get; set; } = new SkillMasteryModel();
        public DailyProgressModel? TodayProgress { get; set; }
        public List<DailyProgressModel> RecentProgress { get; set; } = new List<DailyProgressModel>();
        public Dictionary<DateTime, int> WeeklyMinutes { get; set; } = new Dictionary<DateTime, int>();
        public List<int> Last7DaysStreak { get; set; } = new List<int>();
        public string CurrentStageTopic { get; set; } = string.Empty;
    }

    /// <summary>
    /// Progress chart data
    /// </summary>
    public class ProgressChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<double> ProgressData { get; set; } = new List<double>();
        public List<int> TimeData { get; set; } = new List<int>();
        public List<int> StreakData { get; set; } = new List<int>();
    }
}
