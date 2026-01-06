using AuthMvcApp.Models;

namespace AuthMvcApp.Services
{
    public interface IJsonDataService
    {
        List<UserModel> GetAllUsers();
        UserModel? GetUserByEmail(string email);
        UserModel? GetUserById(int id);
        void SaveUser(UserModel user);
        void UpdateUser(UserModel user);
        void DeleteUser(int id);
        bool EmailExists(string email);
        UserModel? ValidateUser(string email, string password);
        int GetNextId();
    }
}
