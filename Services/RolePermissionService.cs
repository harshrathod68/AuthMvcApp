using MyApps.Models;
using System.Text.Json;

namespace MyApps.Services
{
    /// <summary>
    /// Service for managing role-based app permissions
    /// </summary>
    public class RolePermissionService : IRolePermissionService
    {
        private readonly string _filePath = "Data/rolepermissions.json";
        private readonly ILogger<RolePermissionService> _logger;

        // All available apps in the system
        private readonly List<string> _allApps = new List<string>
        {
            "Weather",
            "Currency",
            "TimeZone",
            "Country",
            "News",
            "Notes",
            "Habits",
            "Translator",
            "Emergency",
            "TimeTrack",
            "SkillRoadmap",
            "PdfConverter"
        };

        public RolePermissionService(ILogger<RolePermissionService> logger)
        {
            _logger = logger;
            EnsureFileExists();
        }

        /// <summary>
        /// Ensure the permissions file exists
        /// </summary>
        private void EnsureFileExists()
        {
            if (!File.Exists(_filePath))
            {
                var defaultPermissions = new List<RolePermissionModel>
                {
                    new RolePermissionModel
                    {
                        Role = "Admin",
                        AllowedApps = new List<string>(_allApps)
                    },
                    new RolePermissionModel
                    {
                        Role = "Manager",
                        AllowedApps = new List<string> { "Weather", "Currency", "TimeZone", "Country", "News", "Notes", "Habits", "TimeTrack", "SkillRoadmap" }
                    },
                    new RolePermissionModel
                    {
                        Role = "User",
                        AllowedApps = new List<string> { "Weather", "Notes", "Habits", "SkillRoadmap" }
                    },
                    new RolePermissionModel
                    {
                        Role = "Moderator",
                        AllowedApps = new List<string> { "Weather", "Currency", "News", "Notes", "Habits", "Translator" }
                    },
                    new RolePermissionModel
                    {
                        Role = "Guest",
                        AllowedApps = new List<string> { "Weather", "Currency", "TimeZone" }
                    }
                };
                SavePermissions(defaultPermissions);
            }
        }

        /// <summary>
        /// Get all role permissions
        /// </summary>
        public List<RolePermissionModel> GetAllRolePermissions()
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<RolePermissionModel>>(json) ?? new List<RolePermissionModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading role permissions");
                return new List<RolePermissionModel>();
            }
        }

        /// <summary>
        /// Get permissions for a specific role
        /// </summary>
        public RolePermissionModel? GetRolePermission(string role)
        {
            var permissions = GetAllRolePermissions();
            return permissions.FirstOrDefault(p => p.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Update permissions for a role
        /// </summary>
        public bool UpdateRolePermission(RolePermissionModel model)
        {
            try
            {
                var permissions = GetAllRolePermissions();
                var existing = permissions.FirstOrDefault(p => p.Role.Equals(model.Role, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.AllowedApps = model.AllowedApps;
                }
                else
                {
                    permissions.Add(model);
                }

                SavePermissions(permissions);
                _logger.LogInformation("Updated permissions for role: {Role}", model.Role);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role permissions");
                return false;
            }
        }

        /// <summary>
        /// Check if a role has access to an app
        /// </summary>
        public bool HasAccess(string role, string appName)
        {
            var permission = GetRolePermission(role);
            if (permission == null) return false;

            return permission.AllowedApps.Contains(appName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get all available app names
        /// </summary>
        public List<string> GetAllAppNames()
        {
            return new List<string>(_allApps);
        }

        /// <summary>
        /// Save permissions to file
        /// </summary>
        private void SavePermissions(List<RolePermissionModel> permissions)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(permissions, options);
            File.WriteAllText(_filePath, json);
        }
    }
}
