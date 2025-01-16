using System.Text.Json;
using MyPocketTrack.Components.Modals;

namespace MyPocketTrack.Services
{
    public class UserService
    {
        private readonly string _userFilePath;
        private User _currentUser;

        public UserService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "MyPocketTrack");
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);
            _userFilePath = Path.Combine(appFolder, "users.json");
            if (!File.Exists(_userFilePath))
                File.WriteAllText(_userFilePath, "[]");
        }

        private async Task<List<User>> GetUsersAsync()
        {
            try
            {
                string jsonString = await File.ReadAllTextAsync(_userFilePath);
                return JsonSerializer.Deserialize<List<User>>(jsonString) ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading users: {ex.Message}");
                return new List<User>();
            }
        }

        private async Task SaveUsersAsync(List<User> users)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_userFilePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            var users = await GetUsersAsync();

            // Check if username already exists
            if (users.Any(u => u.Username == user.Username))
            {
                return false;
            }

            users.Add(new User
            {
                Username = user.Username,
                Password = user.Password, // In production, hash the password
                CurrencyType = user.CurrencyType
            });

            await SaveUsersAsync(users);
            return true;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await ValidateUserAsync(username, password);
            if (user != null)
            {
                _currentUser = user;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public bool IsAuthenticated()
        {
            return _currentUser != null;
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public async Task<User> ValidateUserAsync(string username, string password)
        {
            var users = await GetUsersAsync();
            var user = users.FirstOrDefault(u =>
                u.Username == username &&
                u.Password == password);

            if (user != null)
            {
                _currentUser = user;
                return user;
            }
            return null;
        }
    }
}
