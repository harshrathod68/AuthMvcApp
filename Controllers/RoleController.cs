using Microsoft.AspNetCore.Mvc;
using MyApps.Models;
using MyApps.Services;

namespace MyApps.Controllers
{
    /// <summary>
    /// Controller for managing role-based app permissions (Admin only)
    /// </summary>
    public class RoleController : Controller
    {
        private readonly IRolePermissionService _rolePermissionService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRolePermissionService rolePermissionService, ILogger<RoleController> logger)
        {
            _rolePermissionService = rolePermissionService;
            _logger = logger;
        }

        /// <summary>
        /// Display role management page (Admin only)
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied! Only Admin users can manage roles.";
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = userRole;

            return View();
        }

        /// <summary>
        /// Get role permissions for editing
        /// </summary>
        [HttpGet]
        public IActionResult Edit(string role)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied!";
                return RedirectToAction("Index", "Dashboard");
            }

            var permission = _rolePermissionService.GetRolePermission(role);
            if (permission == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction("Index");
            }

            // Create model with all apps and their selection status
            var allApps = _rolePermissionService.GetAllAppNames();
            var model = new ManageRolePermissionModel
            {
                Role = role,
                Apps = allApps.ToDictionary(
                    app => app,
                    app => permission.AllowedApps.Contains(app, StringComparer.OrdinalIgnoreCase)
                )
            };

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = userRole;

            return View(model);
        }

        /// <summary>
        /// Update role permissions
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string role, List<string> selectedApps)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied!";
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                var permission = new RolePermissionModel
                {
                    Role = role,
                    AllowedApps = selectedApps ?? new List<string>()
                };

                var result = _rolePermissionService.UpdateRolePermission(permission);

                if (result)
                {
                    TempData["Success"] = $"Permissions updated successfully for {role} role!";
                    _logger.LogInformation("Admin updated permissions for role: {Role}", role);
                }
                else
                {
                    TempData["Error"] = "Failed to update permissions.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role permissions");
                TempData["Error"] = "An error occurred while updating permissions.";
            }

            return RedirectToAction("Index");
        }

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }
    }
}
