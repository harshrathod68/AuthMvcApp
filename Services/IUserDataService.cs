using MyApps.Models;

namespace MyApps.Services
{
    /// <summary>
    /// Interface for user data management service
    /// Defines CRUD operations for user data stored in JSON
    /// </summary>
    public interface IUserDataService
    {
        /// <summary>
        /// Gets all users from the JSON file
        /// </summary>
        /// <returns>List of all users</returns>
        List<UserDataModel> GetAllUsers();

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User if found, null otherwise</returns>
        UserDataModel? GetUserById(int id);

        /// <summary>
        /// Adds a new user to the JSON file
        /// </summary>
        /// <param name="user">User data to add</param>
        /// <returns>The created user with assigned ID</returns>
        UserDataModel AddUser(UserDataModel user);

        /// <summary>
        /// Updates an existing user in the JSON file
        /// </summary>
        /// <param name="user">Updated user data</param>
        /// <returns>True if successful, false otherwise</returns>
        bool UpdateUser(UserDataModel user);

        /// <summary>
        /// Deletes a user from the JSON file
        /// </summary>
        /// <param name="id">ID of user to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        bool DeleteUser(int id);

        /// <summary>
        /// Checks if an email already exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="excludeId">Optional ID to exclude from check (for updates)</param>
        /// <returns>True if email exists, false otherwise</returns>
        bool EmailExists(string email, int? excludeId = null);

        /// <summary>
        /// Gets the next available ID
        /// </summary>
        /// <returns>Next ID number</returns>
        int GetNextId();
    }
}
