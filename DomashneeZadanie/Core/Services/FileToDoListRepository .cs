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
    public class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _baseFolder;
        private readonly List<ToDoList> _lists = new();

        public FileToDoListRepository(string baseFolder)
        {
            if (string.IsNullOrWhiteSpace(baseFolder))
                throw new ArgumentException("Путь к базовой папке не должен быть пустым", nameof(baseFolder));

            _baseFolder = baseFolder;

            if (!Directory.Exists(_baseFolder))
            {
                Directory.CreateDirectory(_baseFolder);
            }

            LoadAllLists();
        }

        private void LoadAllLists()
        {
            string[] files = Directory.GetFiles(_baseFolder, "*.json");
            foreach (string file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    ToDoList? list = JsonSerializer.Deserialize<ToDoList>(json);
                    if (list != null)
                    {
                        _lists.Add(list);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке файла {file}: {ex.Message}");
                }
            }
        }

        private string GetFilePath(Guid listId)
        {
            return Path.Combine(_baseFolder, $"{listId}.json");
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            foreach (ToDoList list in _lists)
            {
                if (list.Id == id)
                {
                    return Task.FromResult<ToDoList?>(list);
                }
            }

            return Task.FromResult<ToDoList?>(null);
        }

        public Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            List<ToDoList> result = new List<ToDoList>();

            foreach (ToDoList list in _lists)
            {
                if (list.User != null && list.User.UserId == userId)
                {
                    result.Add(list);
                }
            }

            return Task.FromResult<IReadOnlyList<ToDoList>>(result.AsReadOnly());
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            _lists.Add(list);

            string path = GetFilePath(list.Id);
            string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(path, json, ct);
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            foreach (ToDoList list in _lists)
            {
                if (list.User != null && list.User.UserId == userId)
                {
                    if (!string.IsNullOrWhiteSpace(list.Name) &&
                        string.Equals(list.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult(true);
                    }
                }
            }

            return Task.FromResult(false);
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            ToDoList? toRemove = null;

            foreach (ToDoList list in _lists)
            {
                if (list.Id == id)
                {
                    toRemove = list;
                    break;
                }
            }

            if (toRemove != null)
            {
                _lists.Remove(toRemove);
                string path = GetFilePath(id);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            return Task.CompletedTask;
        }
    }
}
