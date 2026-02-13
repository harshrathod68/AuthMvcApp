using MyApps.Models;
using System.Text.Json;

namespace MyApps.Services
{
    /// <summary>
    /// Complete Skill Tracker Service Implementation
    /// Handles skills, daily progress, streaks, charts, and motivation
    /// </summary>
    public class SkillTrackerService : ISkillTrackerService
    {
        private readonly string _skillsFile = "Data/skills.json";
        private readonly string _progressFile = "Data/dailyprogress.json";
        private readonly ILogger<SkillTrackerService> _logger;

        public SkillTrackerService(ILogger<SkillTrackerService> logger)
        {
            _logger = logger;
            EnsureFilesExist();
        }

        private void EnsureFilesExist()
        {
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            if (!File.Exists(_skillsFile))
                File.WriteAllText(_skillsFile, "[]");

            if (!File.Exists(_progressFile))
                File.WriteAllText(_progressFile, "[]");
        }

        #region Skill Management

        public List<SkillMasteryModel> GetAllSkills(string userEmail)
        {
            try
            {
                var skills = LoadSkills();
                return skills.Where(s => s.UserEmail == userEmail)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skills");
                return new List<SkillMasteryModel>();
            }
        }

        public SkillMasteryModel? GetSkillById(int id)
        {
            try
            {
                var skills = LoadSkills();
                return skills.FirstOrDefault(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skill by id");
                return null;
            }
        }

        public int CreateSkill(SkillMasteryModel skill)
        {
            try
            {
                var skills = LoadSkills();
                skill.Id = skills.Any() ? skills.Max(s => s.Id) + 1 : 1;
                skill.CreatedAt = DateTime.Now;
                skill.CompletedDays = 0;
                skill.MissedDays = 0;
                skill.CurrentStreak = 0;
                skill.LongestStreak = 0;
                skill.TotalMinutesSpent = 0;

                skills.Add(skill);
                SaveSkills(skills);

                _logger.LogInformation("Created skill: {Id} - {Name}", skill.Id, skill.SkillName);
                return skill.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating skill");
                return 0;
            }
        }

        public bool UpdateSkill(SkillMasteryModel skill)
        {
            try
            {
                var skills = LoadSkills();
                var existing = skills.FirstOrDefault(s => s.Id == skill.Id);
                
                if (existing == null) return false;

                existing.SkillName = skill.SkillName;
                existing.TotalDays = skill.TotalDays;
                existing.DailyMinutes = skill.DailyMinutes;
                existing.StartDate = skill.StartDate;
                existing.GoalLevel = skill.GoalLevel;

                SaveSkills(skills);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill");
                return false;
            }
        }

        public bool DeleteSkill(int id)
        {
            try
            {
                var skills = LoadSkills();
                var skill = skills.FirstOrDefault(s => s.Id == id);
                
                if (skill == null) return false;

                skills.Remove(skill);
                SaveSkills(skills);

                // Delete associated progress
                var progress = LoadProgress();
                progress.RemoveAll(p => p.SkillId == id);
                SaveProgress(progress);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skill");
                return false;
            }
        }

        #endregion

        #region Daily Progress Management

        public List<DailyProgressModel> GetProgressForSkill(int skillId)
        {
            try
            {
                var progress = LoadProgress();
                return progress.Where(p => p.SkillId == skillId)
                    .OrderBy(p => p.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress for skill");
                return new List<DailyProgressModel>();
            }
        }

        public DailyProgressModel? GetProgressForDate(int skillId, DateTime date)
        {
            try
            {
                var progress = LoadProgress();
                return progress.FirstOrDefault(p => 
                    p.SkillId == skillId && 
                    p.Date.Date == date.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress for date");
                return null;
            }
        }

        public bool AddOrUpdateProgress(DailyProgressModel progress)
        {
            try
            {
                var allProgress = LoadProgress();
                var existing = allProgress.FirstOrDefault(p => 
                    p.SkillId == progress.SkillId && 
                    p.Date.Date == progress.Date.Date);

                if (existing != null)
                {
                    existing.IsCompleted = progress.IsCompleted;
                    existing.MinutesSpent = progress.MinutesSpent;
                    existing.Notes = progress.Notes;
                    existing.LearningHighlight = progress.LearningHighlight;
                    existing.CompletedAt = progress.IsCompleted ? DateTime.Now : null;
                }
                else
                {
                    progress.Id = allProgress.Any() ? allProgress.Max(p => p.Id) + 1 : 1;
                    allProgress.Add(progress);
                }

                SaveProgress(allProgress);
                RecalculateProgress(progress.SkillId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating progress");
                return false;
            }
        }

        public bool MarkDayComplete(int skillId, DateTime date, int minutesSpent, string? notes)
        {
            try
            {
                var skill = GetSkillById(skillId);
                if (skill == null) return false;

                var dayNumber = (date.Date - skill.StartDate.Date).Days + 1;
                
                var progress = new DailyProgressModel
                {
                    SkillId = skillId,
                    Date = date.Date,
                    DayNumber = dayNumber,
                    IsCompleted = true,
                    MinutesSpent = minutesSpent,
                    Notes = notes,
                    CompletedAt = DateTime.Now
                };

                return AddOrUpdateProgress(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking day complete");
                return false;
            }
        }

        #endregion

        #region Progress Calculation

        public void RecalculateProgress(int skillId)
        {
            try
            {
                var skills = LoadSkills();
                var skill = skills.FirstOrDefault(s => s.Id == skillId);
                
                if (skill == null) return;

                var progress = GetProgressForSkill(skillId);
                
                skill.CompletedDays = progress.Count(p => p.IsCompleted);
                skill.TotalMinutesSpent = progress.Sum(p => p.MinutesSpent);
                skill.CurrentStreak = CalculateStreak(skillId);
                
                if (skill.CurrentStreak > skill.LongestStreak)
                    skill.LongestStreak = skill.CurrentStreak;

                // Calculate missed days
                var daysPassed = skill.DaysPassed;
                var expectedCompletedDays = Math.Min(daysPassed, skill.TotalDays);
                skill.MissedDays = Math.Max(0, expectedCompletedDays - skill.CompletedDays);

                SaveSkills(skills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating progress");
            }
        }

        public int CalculateStreak(int skillId)
        {
            try
            {
                var progress = GetProgressForSkill(skillId)
                    .Where(p => p.IsCompleted)
                    .OrderByDescending(p => p.Date)
                    .ToList();

                if (!progress.Any()) return 0;

                int streak = 0;
                DateTime expectedDate = DateTime.Today;

                foreach (var p in progress)
                {
                    if (p.Date.Date == expectedDate.Date)
                    {
                        streak++;
                        expectedDate = expectedDate.AddDays(-1);
                    }
                    else if (p.Date.Date < expectedDate.Date)
                    {
                        break;
                    }
                }

                return streak;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating streak");
                return 0;
            }
        }

        public double GetProgressPercentage(int skillId)
        {
            var skill = GetSkillById(skillId);
            return skill?.ProgressPercentage ?? 0;
        }

        public double GetConsistencyScore(int skillId)
        {
            var skill = GetSkillById(skillId);
            return skill?.ConsistencyScore ?? 0;
        }

        #endregion

        #region Dashboard Data

        public SkillDashboardModel GetDashboard(int skillId)
        {
            try
            {
                var skill = GetSkillById(skillId);
                if (skill == null)
                    return new SkillDashboardModel();

                var allProgress = GetProgressForSkill(skillId);
                var todayProgress = GetProgressForDate(skillId, DateTime.Today);
                var recentProgress = allProgress.OrderByDescending(p => p.Date).Take(7).ToList();

                // Weekly minutes
                var weeklyMinutes = new Dictionary<DateTime, int>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    var minutes = allProgress
                        .Where(p => p.Date.Date == date.Date)
                        .Sum(p => p.MinutesSpent);
                    weeklyMinutes[date] = minutes;
                }

                // Last 7 days streak
                var last7Days = new List<int>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    var completed = allProgress.Any(p => p.Date.Date == date.Date && p.IsCompleted);
                    last7Days.Add(completed ? 1 : 0);
                }

                // Current stage topic
                var currentDay = skill.DaysPassed;
                var currentStage = skill.Stages.FirstOrDefault(s => 
                    currentDay >= s.StartDay && currentDay <= s.EndDay);

                return new SkillDashboardModel
                {
                    Skill = skill,
                    TodayProgress = todayProgress,
                    RecentProgress = recentProgress,
                    WeeklyMinutes = weeklyMinutes,
                    Last7DaysStreak = last7Days,
                    CurrentStageTopic = currentStage?.Topic ?? "Getting Started"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard");
                return new SkillDashboardModel();
            }
        }

        public List<DailyProgressModel> GetTodaysTasks(string userEmail)
        {
            try
            {
                var skills = GetAllSkills(userEmail);
                var tasks = new List<DailyProgressModel>();

                foreach (var skill in skills.Where(s => s.IsActive))
                {
                    var todayProgress = GetProgressForDate(skill.Id, DateTime.Today);
                    
                    if (todayProgress == null)
                    {
                        // Create placeholder for today
                        var dayNumber = (DateTime.Today - skill.StartDate.Date).Days + 1;
                        todayProgress = new DailyProgressModel
                        {
                            SkillId = skill.Id,
                            Date = DateTime.Today,
                            DayNumber = dayNumber,
                            IsCompleted = false,
                            MinutesSpent = 0
                        };
                    }

                    tasks.Add(todayProgress);
                }

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's tasks");
                return new List<DailyProgressModel>();
            }
        }

        public Dictionary<string, int> GetOverallStats(string userEmail)
        {
            try
            {
                var skills = GetAllSkills(userEmail);
                var stats = new Dictionary<string, int>
                {
                    ["TotalSkills"] = skills.Count,
                    ["ActiveSkills"] = skills.Count(s => s.IsActive),
                    ["CompletedSkills"] = skills.Count(s => s.IsCompleted),
                    ["TotalCompletedDays"] = skills.Sum(s => s.CompletedDays),
                    ["TotalMinutesSpent"] = skills.Sum(s => s.TotalMinutesSpent),
                    ["HighestStreak"] = skills.Any() ? skills.Max(s => s.LongestStreak) : 0
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overall stats");
                return new Dictionary<string, int>();
            }
        }

        #endregion

        #region Chart Data

        public ProgressChartData GetProgressChartData(int skillId)
        {
            try
            {
                var skill = GetSkillById(skillId);
                if (skill == null)
                    return new ProgressChartData();

                var progress = GetProgressForSkill(skillId);
                var chartData = new ProgressChartData();

                var completedProgress = progress.Where(p => p.IsCompleted).OrderBy(p => p.Date).ToList();

                foreach (var p in completedProgress)
                {
                    chartData.Labels.Add($"Day {p.DayNumber}");
                    chartData.ProgressData.Add(Math.Round((p.DayNumber * 100.0 / skill.TotalDays), 2));
                    chartData.TimeData.Add(p.MinutesSpent);
                }

                return chartData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress chart data");
                return new ProgressChartData();
            }
        }

        public ProgressChartData GetWeeklyChartData(int skillId)
        {
            try
            {
                var chartData = new ProgressChartData();
                var progress = GetProgressForSkill(skillId);

                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    var dayProgress = progress.FirstOrDefault(p => p.Date.Date == date.Date);

                    chartData.Labels.Add(date.ToString("ddd"));
                    chartData.TimeData.Add(dayProgress?.MinutesSpent ?? 0);
                    chartData.StreakData.Add(dayProgress?.IsCompleted == true ? 1 : 0);
                }

                return chartData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly chart data");
                return new ProgressChartData();
            }
        }

        #endregion

        #region Motivational Messages

        public string GetMotivationalMessage(int streak)
        {
            return streak switch
            {
                0 => "🌟 Start your journey today!",
                1 => "🎉 Great start! Keep going!",
                2 => "💪 Two days in a row! Building momentum!",
                3 => "🔥 3 days streak! You're on fire!",
                5 => "⭐ 5 days! Consistency is key!",
                7 => "🏆 One week streak! Amazing dedication!",
                14 => "💎 Two weeks! You're unstoppable!",
                21 => "🚀 21 days! Habit formed!",
                30 => "👑 30 days! You're a champion!",
                60 => "🌟 60 days! Master level achieved!",
                100 => "🏅 100 days! Legendary status!",
                _ => streak > 0 ? $"🔥 {streak} days streak! Keep crushing it!" : "Start today!"
            };
        }

        public string GetDailyReminder(SkillMasteryModel skill)
        {
            var dayNumber = skill.DaysPassed;
            var progress = skill.ProgressPercentage;

            if (progress < 25)
                return $"📚 Day {dayNumber}: Let's build the foundation!";
            else if (progress < 50)
                return $"🎯 Day {dayNumber}: You're making great progress!";
            else if (progress < 75)
                return $"💪 Day {dayNumber}: More than halfway there!";
            else if (progress < 100)
                return $"🚀 Day {dayNumber}: Almost there! Final push!";
            else
                return $"🏆 Congratulations! You've mastered {skill.SkillName}!";
        }

        #endregion

        #region Helper Methods

        private List<SkillMasteryModel> LoadSkills()
        {
            try
            {
                var json = File.ReadAllText(_skillsFile);
                return JsonSerializer.Deserialize<List<SkillMasteryModel>>(json) ?? new List<SkillMasteryModel>();
            }
            catch
            {
                return new List<SkillMasteryModel>();
            }
        }

        private void SaveSkills(List<SkillMasteryModel> skills)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(skills, options);
            File.WriteAllText(_skillsFile, json);
        }

        private List<DailyProgressModel> LoadProgress()
        {
            try
            {
                var json = File.ReadAllText(_progressFile);
                return JsonSerializer.Deserialize<List<DailyProgressModel>>(json) ?? new List<DailyProgressModel>();
            }
            catch
            {
                return new List<DailyProgressModel>();
            }
        }

        private void SaveProgress(List<DailyProgressModel> progress)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(progress, options);
            File.WriteAllText(_progressFile, json);
        }

        #endregion
    }
}
