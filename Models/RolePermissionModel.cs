namespace MyApps.Models
{
    /// <summary>
    /// Model for role-based app permissions
    /// </summary>
    public class RolePermissionModel
    {
        public string Role { get; set; } = string.Empty;
        public List<string> AllowedApps { get; set; } = new List<string>();
    }

    /// <summary>
    /// Model for managing role permissions
    /// </summary>
    public class ManageRolePermissionModel
    {
        public string Role { get; set; } = string.Empty;
        public Dictionary<string, bool> Apps { get; set; } = new Dictionary<string, bool>();
    }
}
