using MyApps.Models;

namespace MyApps.Services
{
    /// <summary>
    /// Interface for role permission service
    /// </summary>
    public interface IRolePermissionService
    {
        /// <summary>
        /// Get all role permissions
        /// </summary>
        List<RolePermissionModel> GetAllRolePermissions();

        /// <summary>
        /// Get permissions for a specific role
        /// </summary>
        RolePermissionModel? GetRolePermission(string role);

        /// <summary>
        /// Update permissions for a role
        /// </summary>
        bool UpdateRolePermission(RolePermissionModel model);

        /// <summary>
        /// Check if a role has access to an app
        /// </summary>
        bool HasAccess(string role, string appName);

        /// <summary>
        /// Get all available app names
        /// </summary>
        List<string> GetAllAppNames();
    }
}
