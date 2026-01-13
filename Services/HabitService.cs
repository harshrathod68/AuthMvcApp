/*
 * =====================================================
 * HabitService.cs - Habit Tracker Service
 * =====================================================
 * 
 * Ye service Habit Tracker ka data manage karti hai.
 * Data JSON file mein store hota hai: Data/habits.json
 * 
 * Features:
 * - Add/Edit/Delete habits
 * - Daily check-in (complete/incomplete mark karo)
 * - Streak tracking (kitne din lagatar kiya)
 * - Statistics (completion rate, best streak, etc.)
 * 
 * Important Concepts:
 * - Streak = Lagatar kitne din habit complete ki
 * - Log = Har din ka record (date + completed status)
 * 
 * Author: Harsh Rathod
 * =====================================================
 */

using System.Text.Json;
using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    public class HabitService : IHabitService
    {
        // JSON file ka path
        private readonly string _filePath;
        
        // Thread safety ke liye lock
        private readonly object _lock = new();

        // Constructor
        public HabitService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Data", "habits.json");
            EnsureFileExists();
        }

        /// <summary>
        /// File nahi hai to create karo
        /// </summary>
        private void EnsureFileExists()
        {
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        /// <summary>
        /// JSON file se habits read karo
        /// </summary>
        private List<HabitModel> ReadHabits()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<HabitModel>>(json) ?? new();
            }
        }

        /// <summary>
        /// Habits ko JSON file mein save karo
        /// </summary>
        private void SaveHabits(List<HabitModel> habits)
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(habits, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_filePath, json);
            }
        }

        // ========================================
        // READ - Habits List Get Karo
        // ========================================
        
        /// <summary>
        /// User ki sab active habits lo
        /// </summary>
        public List<HabitModel> GetUserHabits(string userId)
        {
            return ReadHabits()
                // Sirf is user ki habits
                .Where(h => h.UserId == userId && h.IsActive)
                // Purani habits pehle (creation order)
                .OrderBy(h => h.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Single habit ID se get karo
        /// </summary>
        public HabitModel? GetHabitById(int id, string userId)
        {
            return ReadHabits().FirstOrDefault(h => h.Id == id && h.UserId == userId);
        }

        // ========================================
        // CREATE - New Habit Add Karo
        // ========================================
        
        /// <summary>
        /// Nayi habit add karo
        /// </summary>
        public void AddHabit(HabitModel habit)
        {
            var habits = ReadHabits();
            
            // New ID generate karo
            habit.Id = habits.Any() ? habits.Max(h => h.Id) + 1 : 1;
            habit.CreatedAt = DateTime.Now;
            
            habits.Add(habit);
            SaveHabits(habits);
        }

        // ========================================
        // UPDATE - Habit Edit Karo
        // ========================================
        
        /// <summary>
        /// Habit details update karo (name, icon, color, etc.)
        /// </summary>
        public bool UpdateHabit(HabitModel habit)
        {
            var habits = ReadHabits();
            var existing = habits.FirstOrDefault(h => h.Id == habit.Id && h.UserId == habit.UserId);
            
            if (existing == null) 
                return false;

            // Details update karo
            existing.Name = habit.Name;
            existing.Description = habit.Description;
            existing.Icon = habit.Icon;
            existing.Color = habit.Color;
            existing.Category = habit.Category;

            SaveHabits(habits);
            return true;
        }

        // ========================================
        // DELETE - Habit Remove Karo (Soft Delete)
        // ========================================
        
        /// <summary>
        /// Habit delete karo (soft delete - IsActive = false)
        /// Soft delete = Data rehta hai par dikhta nahi
        /// </summary>
        public bool DeleteHabit(int id, string userId)
        {
            var habits = ReadHabits();
            var habit = habits.FirstOrDefault(h => h.Id == id && h.UserId == userId);
            
            if (habit == null) 
                return false;

            // Soft delete - sirf inactive mark karo
            // Isse purana data safe rehta hai
            habit.IsActive = false;
            
            SaveHabits(habits);
            return true;
        }

        // ========================================
        // TOGGLE LOG - Daily Check-in
        // ========================================
        
        /// <summary>
        /// Kisi date par habit complete/incomplete toggle karo
        /// </summary>
        /// <param name="id">Habit ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="date">Date (format: "yyyy-MM-dd")</param>
        public bool ToggleHabitLog(int id, string userId, string date)
        {
            var habits = ReadHabits();
            var habit = habits.FirstOrDefault(h => h.Id == id && h.UserId == userId);
            
            if (habit == null) 
                return false;

            // Check karo is date ka log already hai ya nahi
            var log = habit.Logs.FirstOrDefault(l => l.Date == date);
            
            if (log != null)
            {
                // Log hai - toggle karo (true ↔ false)
                log.IsCompleted = !log.IsCompleted;
            }
            else
            {
                // Log nahi hai - naya log add karo (completed = true)
                habit.Logs.Add(new HabitLog 
                { 
                    Date = date, 
                    IsCompleted = true 
                });
            }

            SaveHabits(habits);
            return true;
        }

        // ========================================
        // STATISTICS - Habit Stats Calculate Karo
        // ========================================
        
        /// <summary>
        /// Habit ki statistics calculate karo
        /// - Weekly status (last 7 days)
        /// - Total completed days
        /// - Completion rate (last 30 days)
        /// - Current streak
        /// - Best streak
        /// </summary>
        public HabitStats GetHabitStats(HabitModel habit)
        {
            var stats = new HabitStats();
            var today = DateTime.Today;
            
            // ===== 1. WEEKLY STATUS (Last 7 Days) =====
            // Dictionary: date → completed (true/false)
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i).ToString("yyyy-MM-dd");
                var log = habit.Logs.FirstOrDefault(l => l.Date == date);
                stats.WeeklyStatus[date] = log?.IsCompleted ?? false;
            }

            // ===== 2. TOTAL COMPLETED =====
            // Kitne din total complete kiya
            stats.TotalCompleted = habit.Logs.Count(l => l.IsCompleted);

            // ===== 3. COMPLETION RATE (Last 30 Days) =====
            // Last 30 days mein kitne % days complete kiya
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => today.AddDays(-i).ToString("yyyy-MM-dd"))
                .ToList();
            
            var completedIn30 = habit.Logs.Count(l => 
                last30Days.Contains(l.Date) && l.IsCompleted
            );
            
            // Percentage calculate karo (round to 1 decimal)
            stats.CompletionRate = Math.Round((double)completedIn30 / 30 * 100, 1);

            // ===== 4. CURRENT STREAK =====
            // Aaj se peeche jaake kitne din lagatar complete kiya
            var currentStreak = 0;
            for (int i = 0; i <= 365; i++)
            {
                var date = today.AddDays(-i).ToString("yyyy-MM-dd");
                var log = habit.Logs.FirstOrDefault(l => l.Date == date);
                
                if (log?.IsCompleted == true)
                {
                    currentStreak++;
                }
                else if (i > 0) // Aaj incomplete ho sakta hai
                {
                    break; // Streak toot gaya
                }
            }
            stats.CurrentStreak = currentStreak;

            // ===== 5. BEST STREAK =====
            // Ab tak ka sabse lamba streak
            
            // Pehle sab completed logs ko date wise sort karo
            var sortedLogs = habit.Logs
                .Where(l => l.IsCompleted)
                .OrderBy(l => l.Date)
                .ToList();
            
            var bestStreak = 0;
            var tempStreak = 0;
            DateTime? prevDate = null;

            foreach (var log in sortedLogs)
            {
                if (DateTime.TryParse(log.Date, out var logDate))
                {
                    // Check karo consecutive hai ya nahi
                    if (prevDate == null || (logDate - prevDate.Value).Days == 1)
                    {
                        // Consecutive hai - streak badhao
                        tempStreak++;
                    }
                    else
                    {
                        // Gap hai - streak reset karo
                        tempStreak = 1;
                    }
                    
                    // Best streak update karo
                    bestStreak = Math.Max(bestStreak, tempStreak);
                    prevDate = logDate;
                }
            }
            
            // Current streak bhi consider karo
            stats.BestStreak = Math.Max(bestStreak, currentStreak);

            return stats;
        }
    }
}
