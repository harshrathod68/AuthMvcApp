using Microsoft.AspNetCore.Mvc;
using MyApps.Models;
using MyApps.Services;

namespace MyApps.Controllers
{
    /// <summary>
    /// Controller for Skill Mastery Tracker
    /// Complete skill tracking with daily tasks, progress, charts, and motivation
    /// </summary>
    public class SkillRoadmapController : Controller
    {
        private readonly ISkillTrackerService _skillService;
        private readonly ILogger<SkillRoadmapController> _logger;

        public SkillRoadmapController(ISkillTrackerService skillService, ILogger<SkillRoadmapController> logger)
        {
            _skillService = skillService;
            _logger = logger;
        }

        #region My Skills

        /// <summary>
        /// Display all skills for current user
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
            var skills = _skillService.GetAllSkills(userEmail);
            var stats = _skillService.GetOverallStats(userEmail);

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.Stats = stats;
            return View(skills);
        }

        #endregion

        #region Create Skill

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(new CreateSkillMasteryModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateSkillMasteryModel model)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
                
                var skill = new SkillMasteryModel
                {
                    SkillName = model.SkillName,
                    TotalDays = model.TotalDays,
                    DailyMinutes = model.DailyMinutes,
                    StartDate = model.StartDate,
                    GoalLevel = model.GoalLevel,
                    UserEmail = userEmail
                };

                var id = _skillService.CreateSkill(skill);

                if (id > 0)
                {
                    TempData["Success"] = "Skill created successfully! Start your learning journey today!";
                    return RedirectToAction("Dashboard", new { id });
                }
                else
                {
                    TempData["Error"] = "Failed to create skill.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating skill");
                TempData["Error"] = "An error occurred while creating skill.";
                return View(model);
            }
        }

        #endregion

        #region Dashboard

        [HttpGet]
        public IActionResult Dashboard(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var dashboard = _skillService.GetDashboard(id);
            
            if (dashboard.Skill == null || dashboard.Skill.Id == 0)
            {
                TempData["Error"] = "Skill not found.";
                return RedirectToAction("Index");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.MotivationalMessage = _skillService.GetMotivationalMessage(dashboard.Skill.CurrentStreak);
            ViewBag.DailyReminder = _skillService.GetDailyReminder(dashboard.Skill);

            return View(dashboard);
        }

        #endregion

        #region Daily Tasks

        [HttpGet]
        public IActionResult Tasks(int id, DateTime? date)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var skill = _skillService.GetSkillById(id);
            if (skill == null)
            {
                TempData["Error"] = "Skill not found.";
                return RedirectToAction("Index");
            }

            var selectedDate = date ?? DateTime.Today;
            var progress = _skillService.GetProgressForDate(id, selectedDate);

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.Skill = skill;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.Progress = progress;

            return View();
        }

        [HttpPost]
        public IActionResult MarkComplete(int skillId, DateTime date, int minutesSpent, string? notes)
        {
            if (!IsAuthenticated())
                return Json(new { success = false, message = "Not authenticated" });

            try
            {
                var result = _skillService.MarkDayComplete(skillId, date, minutesSpent, notes);
                
                if (result)
                {
                    var skill = _skillService.GetSkillById(skillId);
                    var message = _skillService.GetMotivationalMessage(skill?.CurrentStreak ?? 0);
                    
                    return Json(new { 
                        success = true, 
                        message = message,
                        streak = skill?.CurrentStreak ?? 0,
                        progress = skill?.ProgressPercentage ?? 0
                    });
                }
                
                return Json(new { success = false, message = "Failed to mark complete" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking complete");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        #endregion

        #region Edit & Delete

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var skill = _skillService.GetSkillById(id);
            if (skill == null)
            {
                TempData["Error"] = "Skill not found.";
                return RedirectToAction("Index");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SkillMasteryModel skill)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (!ModelState.IsValid)
                return View(skill);

            try
            {
                var result = _skillService.UpdateSkill(skill);
                
                if (result)
                {
                    TempData["Success"] = "Skill updated successfully!";
                    return RedirectToAction("Dashboard", new { id = skill.Id });
                }
                else
                {
                    TempData["Error"] = "Failed to update skill.";
                    return View(skill);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill");
                TempData["Error"] = "An error occurred while updating skill.";
                return View(skill);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            try
            {
                var result = _skillService.DeleteSkill(id);
                
                if (result)
                {
                    TempData["Success"] = "Skill deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete skill.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skill");
                TempData["Error"] = "An error occurred while deleting skill.";
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Chart Data API

        [HttpGet]
        public IActionResult GetChartData(int id, string type = "progress")
        {
            if (!IsAuthenticated())
                return Json(new { success = false });

            try
            {
                var chartData = type.ToLower() switch
                {
                    "weekly" => _skillService.GetWeeklyChartData(id),
                    _ => _skillService.GetProgressChartData(id)
                };

                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart data");
                return Json(new { success = false });
            }
        }

        #endregion

        #region Helper Methods

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        #endregion
    }
}
