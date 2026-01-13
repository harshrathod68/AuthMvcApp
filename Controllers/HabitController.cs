using Microsoft.AspNetCore.Mvc;
using AuthMvcApp.Services;
using AuthMvcApp.Models;

namespace AuthMvcApp.Controllers
{
    public class HabitController : Controller
    {
        private readonly IHabitService _habitService;

        public HabitController(IHabitService habitService)
        {
            _habitService = habitService;
        }

        public IActionResult Index(string view = "week")
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            var habits = _habitService.GetUserHabits(userId);
            
            var habitStats = new Dictionary<int, HabitStats>();
            foreach (var habit in habits)
            {
                habitStats[habit.Id] = _habitService.GetHabitStats(habit);
            }
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.HabitStats = habitStats;
            ViewBag.Today = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.WeekDays = GetWeekDays();
            ViewBag.CurrentView = view;
            ViewBag.CalendarData = GetCalendarData(habits);
            ViewBag.MonthProgress = GetMonthProgress(habits);
            ViewBag.YearProgress = GetYearProgress(habits);
            
            return View(habits);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(new HabitCreateModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HabitCreateModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (!ModelState.IsValid) return View(model);

            var habit = new HabitModel
            {
                UserId = HttpContext.Session.GetString("UserId")!,
                Name = model.Name,
                Description = model.Description,
                Icon = model.Icon,
                Color = model.Color,
                Category = model.Category
            };

            _habitService.AddHabit(habit);
            TempData["Success"] = "Habit created successfully!";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            var habit = _habitService.GetHabitById(id, userId);
            
            if (habit == null)
            {
                TempData["Error"] = "Habit not found";
                return RedirectToAction(nameof(Index));
            }

            var model = new HabitCreateModel
            {
                Name = habit.Name,
                Description = habit.Description,
                Icon = habit.Icon,
                Color = habit.Color,
                Category = habit.Category
            };

            ViewBag.HabitId = id;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, HabitCreateModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.HabitId = id;

            if (!ModelState.IsValid) return View(model);

            var habit = new HabitModel
            {
                Id = id,
                UserId = HttpContext.Session.GetString("UserId")!,
                Name = model.Name,
                Description = model.Description,
                Icon = model.Icon,
                Color = model.Color,
                Category = model.Category
            };

            if (_habitService.UpdateHabit(habit))
                TempData["Success"] = "Habit updated!";
            else
                TempData["Error"] = "Failed to update";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            
            if (_habitService.DeleteHabit(id, userId))
                TempData["Success"] = "Habit deleted!";
            else
                TempData["Error"] = "Failed to delete";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int id, string date)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userId = HttpContext.Session.GetString("UserId")!;
            _habitService.ToggleHabitLog(id, userId, date);

            return RedirectToAction(nameof(Index));
        }

        private List<(string Date, string Day)> GetWeekDays()
        {
            var days = new List<(string, string)>();
            var today = DateTime.Today;
            
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                days.Add((date.ToString("yyyy-MM-dd"), date.ToString("ddd")));
            }
            
            return days;
        }

        private CalendarData GetCalendarData(List<HabitModel> habits)
        {
            var today = DateTime.Today;
            var firstDay = new DateTime(today.Year, today.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            
            var calendar = new CalendarData
            {
                Year = today.Year,
                Month = today.Month,
                MonthName = today.ToString("MMMM yyyy"),
                FirstDayOfWeek = (int)firstDay.DayOfWeek,
                TotalDays = lastDay.Day,
                DayStatus = new Dictionary<int, int>()
            };

            for (int day = 1; day <= lastDay.Day; day++)
            {
                var date = new DateTime(today.Year, today.Month, day).ToString("yyyy-MM-dd");
                var totalHabits = habits.Count;
                var completedHabits = habits.Count(h => h.Logs.Any(l => l.Date == date && l.IsCompleted));
                
                // 0 = none, 1 = some, 2 = all
                if (totalHabits == 0) calendar.DayStatus[day] = 0;
                else if (completedHabits == 0) calendar.DayStatus[day] = 0;
                else if (completedHabits == totalHabits) calendar.DayStatus[day] = 2;
                else calendar.DayStatus[day] = 1;
            }

            return calendar;
        }

        private ProgressData GetMonthProgress(List<HabitModel> habits)
        {
            var today = DateTime.Today;
            var progress = new ProgressData { Labels = new(), Values = new() };
            
            for (int i = 29; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateStr = date.ToString("yyyy-MM-dd");
                var totalHabits = habits.Count;
                var completed = habits.Count(h => h.Logs.Any(l => l.Date == dateStr && l.IsCompleted));
                var rate = totalHabits > 0 ? (int)Math.Round((double)completed / totalHabits * 100) : 0;
                
                progress.Labels.Add(date.Day.ToString());
                progress.Values.Add(rate);
            }

            return progress;
        }

        private ProgressData GetYearProgress(List<HabitModel> habits)
        {
            var today = DateTime.Today;
            var progress = new ProgressData { Labels = new(), Values = new() };
            
            for (int i = 11; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
                var totalPossible = habits.Count * daysInMonth;
                var completed = 0;

                for (int d = 1; d <= daysInMonth; d++)
                {
                    var dateStr = new DateTime(month.Year, month.Month, d).ToString("yyyy-MM-dd");
                    completed += habits.Count(h => h.Logs.Any(l => l.Date == dateStr && l.IsCompleted));
                }

                var rate = totalPossible > 0 ? (int)Math.Round((double)completed / totalPossible * 100) : 0;
                progress.Labels.Add(month.ToString("MMM"));
                progress.Values.Add(rate);
            }

            return progress;
        }

        private bool IsAuthenticated() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
    }
}
