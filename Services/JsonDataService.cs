using System.Text.Json;
using MyApps.Models;

namespace MyApps.Services
{
    public class JsonDataService : IJsonDataService
    {
        private readonly string _usersFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDataService(IWebHostEnvironment env)
        {
            var dataPath = Path.Combine(env.ContentRootPath, "Data");
            _usersFilePath = Path.Combine(dataPath, "users.json");
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            if (!File.Exists(_usersFilePath))
                File.WriteAllText(_usersFilePath, "[]");
        }

        public List<UserModel> GetAllUsers()
        {
            var json = File.ReadAllText(_usersFilePath);
            return JsonSerializer.Deserialize<List<UserModel>>(json, _jsonOptions) ?? new List<UserModel>();
        }

        public UserModel? GetUserByEmail(string email)
        {
            return GetAllUsers().FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public UserModel? GetUserById(int id)
        {
            return GetAllUsers().FirstOrDefault(u => u.Id == id);
        }

        public int GetNextId()
        {
            var users = GetAllUsers();
            return users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;
        }

        public void SaveUser(UserModel user)
        {
            var users = GetAllUsers();
            user.Id = GetNextId();
            users.Add(user);
            SaveAll(users);
        }

        public void UpdateUser(UserModel user)
        {
            var users = GetAllUsers();
            var index = users.FindIndex(u => u.Id == user.Id);
            if (index >= 0)
            {
                users[index] = user;
                SaveAll(users);
            }
        }

        public void DeleteUser(int id)
        {
            var users = GetAllUsers();
            users.RemoveAll(u => u.Id == id);
            SaveAll(users);
        }

        public bool EmailExists(string email)
        {
            return GetAllUsers().Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public UserModel? ValidateUser(string email, string password)
        {
            return GetAllUsers().FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                u.Password == password && 
                u.IsVerified);
        }

        private void SaveAll(List<UserModel> users)
        {
            File.WriteAllText(_usersFilePath, JsonSerializer.Serialize(users, _jsonOptions));
        }
    }
}
