using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    public interface IHabitService
    {
        List<HabitModel> GetUserHabits(string userId);
        HabitModel? GetHabitById(int id, string userId);
        void AddHabit(HabitModel habit);
        bool UpdateHabit(HabitModel habit);
        bool DeleteHabit(int id, string userId);
        bool ToggleHabitLog(int id, string userId, string date);
        HabitStats GetHabitStats(HabitModel habit);
    }
}
