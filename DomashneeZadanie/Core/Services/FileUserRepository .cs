using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Services
{
    public class FileUserRepository : IUserRepository
    {
        private readonly string _baseFolder;
        private readonly List<ToDoUser> _users = new();

        public FileUserRepository(string baseFolder)
        {
            if (string.IsNullOrWhiteSpace(baseFolder))
                throw new ArgumentException("Путь к базовой папке не должен быть пустым", nameof(baseFolder));

            _baseFolder = baseFolder;

            if (!Directory.Exists(_baseFolder))
            {
                Directory.CreateDirectory(_baseFolder);
            }

            LoadAllUsers();
        }

        private void LoadAllUsers()
        {
            var files = Directory.GetFiles(_baseFolder, "*.json");
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var user = JsonSerializer.Deserialize<ToDoUser>(json);
                    if (user != null)
                    {
                        _users.Add(user);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке файла {file}: {ex.Message}");
                }
            }
        }

        private string GetFilePath(Guid userId)
        {
            return Path.Combine(_baseFolder, $"{userId}.json");
        }

        public Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            foreach (var user in _users)
            {
                if (user.UserId == userId)
                {
                    return Task.FromResult<ToDoUser?>(user);
                }
            }
            return Task.FromResult<ToDoUser?>(null);
        }

        public Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            foreach (var user in _users)
            {
                if (user.TelegramUserId == telegramUserId)
                {
                    return Task.FromResult<ToDoUser?>(user);
                }
            }
            return Task.FromResult<ToDoUser?>(null);
        } 
        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _users.Add(user);

            string path = GetFilePath(user.UserId);
            string json = JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(path, json, cancellationToken);
        }
        public Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<ToDoUser>>(new List<ToDoUser>());
        }
    }
}
