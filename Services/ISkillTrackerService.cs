using MyApps.Models;

namespace MyApps.Services
{
    /// <summary>
    /// Interface for Skill Tracker Service
    /// Complete skill mastery tracking with daily tasks, progress, and charts
    /// </summary>
    public interface ISkillTrackerService
    {
        // ===== Skill Management =====
        List<SkillMasteryModel> GetAllSkills(string userEmail);
        SkillMasteryModel? GetSkillById(int id);
        int CreateSkill(SkillMasteryModel skill);
        bool UpdateSkill(SkillMasteryModel skill);
        bool DeleteSkill(int id);
        
        // ===== Daily Progress Management =====
        List<DailyProgressModel> GetProgressForSkill(int skillId);
        DailyProgressModel? GetProgressForDate(int skillId, DateTime date);
        bool AddOrUpdateProgress(DailyProgressModel progress);
        bool MarkDayComplete(int skillId, DateTime date, int minutesSpent, string? notes);
        
        // ===== Progress Calculation =====
        void RecalculateProgress(int skillId);
        int CalculateStreak(int skillId);
        double GetProgressPercentage(int skillId);
        double GetConsistencyScore(int skillId);
        
        // ===== Dashboard Data =====
        SkillDashboardModel GetDashboard(int skillId);
        List<DailyProgressModel> GetTodaysTasks(string userEmail);
        Dictionary<string, int> GetOverallStats(string userEmail);
        
        // ===== Chart Data =====
        ProgressChartData GetProgressChartData(int skillId);
        ProgressChartData GetWeeklyChartData(int skillId);
        
        // ===== Motivational Messages =====
        string GetMotivationalMessage(int streak);
        string GetDailyReminder(SkillMasteryModel skill);
    }
}
