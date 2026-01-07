using System.Text.Json;
using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    /// <summary>
    /// Service for managing user data with JSON file storage
    /// Implements CRUD operations for user management
    /// </summary>
    public class UserDataService : IUserDataService
    {
        private readonly string _filePath;
        private readonly ILogger<UserDataService> _logger;
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor - initializes file path and creates file if not exists
        /// </summary>
        /// <param name="environment">Web host environment for getting content root</param>
        /// <param name="logger">Logger for logging operations</param>
        public UserDataService(IWebHostEnvironment environment, ILogger<UserDataService> logger)
        {
            _logger = logger;
            
            // Set file path in Data folder
            var dataFolder = Path.Combine(environment.ContentRootPath, "Data");
            
            // Create Data folder if not exists
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
            
            _filePath = Path.Combine(dataFolder, "userdata.json");
            
            // Create file with empty array if not exists
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
                _logger.LogInformation("Created new userdata.json file");
            }
        }

        /// <summary>
        /// Gets all users from the JSON file
        /// </summary>
        /// <returns>List of all users</returns>
        public List<UserDataModel> GetAllUsers()
        {
            try
            {
                lock (_lock)
                {
                    var json = File.ReadAllText(_filePath);
                    var users = JsonSerializer.Deserialize<List<UserDataModel>>(json);
                    return users ?? new List<UserDataModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading users from JSON file");
                return new List<UserDataModel>();
            }
        }

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User if found, null otherwise</returns>
        public UserDataModel? GetUserById(int id)
        {
            try
            {
                var users = GetAllUsers();
                return users.FirstOrDefault(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Adds a new user to the JSON file
        /// </summary>
        /// <param name="user">User data to add</param>
        /// <returns>The created user with assigned ID</returns>
        public UserDataModel AddUser(UserDataModel user)
        {
            try
            {
                lock (_lock)
                {
                    var users = GetAllUsers();
                    
                    // Assign new ID
                    user.Id = GetNextId();
                    user.CreatedAt = DateTime.Now;
                    
                    users.Add(user);
                    SaveUsers(users);
                    
                    _logger.LogInformation("Added new user with ID: {Id}", user.Id);
                    return user;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing user in the JSON file
        /// </summary>
        /// <param name="user">Updated user data</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool UpdateUser(UserDataModel user)
        {
            try
            {
                lock (_lock)
                {
                    var users = GetAllUsers();
                    var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
                    
                    if (existingUser == null)
                    {
                        _logger.LogWarning("User not found for update: {Id}", user.Id);
                        return false;
                    }
                    
                    // Update properties
                    existingUser.Name = user.Name;
                    existingUser.Email = user.Email;
                    existingUser.City = user.City;
                    existingUser.UpdatedAt = DateTime.Now;
                    
                    // Update password only if provided
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        existingUser.Password = user.Password;
                    }
                    
                    SaveUsers(users);
                    
                    _logger.LogInformation("Updated user with ID: {Id}", user.Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {Id}", user.Id);
                return false;
            }
        }

        /// <summary>
        /// Deletes a user from the JSON file
        /// </summary>
        /// <param name="id">ID of user to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool DeleteUser(int id)
        {
            try
            {
                lock (_lock)
                {
                    var users = GetAllUsers();
                    var user = users.FirstOrDefault(u => u.Id == id);
                    
                    if (user == null)
                    {
                        _logger.LogWarning("User not found for deletion: {Id}", id);
                        return false;
                    }
                    
                    users.Remove(user);
                    SaveUsers(users);
                    
                    _logger.LogInformation("Deleted user with ID: {Id}", id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Checks if an email already exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="excludeId">Optional ID to exclude from check</param>
        /// <returns>True if email exists, false otherwise</returns>
        public bool EmailExists(string email, int? excludeId = null)
        {
            var users = GetAllUsers();
            return users.Any(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                u.Id != excludeId);
        }

        /// <summary>
        /// Gets the next available ID
        /// </summary>
        /// <returns>Next ID number starting from 1</returns>
        public int GetNextId()
        {
            var users = GetAllUsers();
            return users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;
        }

        /// <summary>
        /// Saves the user list to JSON file
        /// </summary>
        /// <param name="users">List of users to save</param>
        private void SaveUsers(List<UserDataModel> users)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            var json = JsonSerializer.Serialize(users, options);
            File.WriteAllText(_filePath, json);
        }
    }
}
