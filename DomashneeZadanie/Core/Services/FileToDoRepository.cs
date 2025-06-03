using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using System.Text.Json; 

namespace DomashneeZadanie.Core.Services
{
    public class FileToDoRepository : IToDoRepository
    {
        private readonly string _baseFolder;
        private readonly string _indexFilePath;

        private Dictionary<Guid, Guid> _index = new();

        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public FileToDoRepository(string baseFolder)
        {
            if (string.IsNullOrWhiteSpace(baseFolder))
                throw new ArgumentException("Путь к базовой папке не должен быть пустым", nameof(baseFolder));

            _baseFolder = baseFolder;
            _indexFilePath = Path.Combine(_baseFolder, "index.json");

            if (!Directory.Exists(_baseFolder))
            {
                Directory.CreateDirectory(_baseFolder);
            }

            LoadOrCreateIndexAsync().GetAwaiter().GetResult();
        }

        private async Task LoadOrCreateIndexAsync()
        {
            if (File.Exists(_indexFilePath))
            {
                try
                {
                    using FileStream fs = File.OpenRead(_indexFilePath);
                    _index = await JsonSerializer.DeserializeAsync<Dictionary<Guid, Guid>>(fs) ?? new Dictionary<Guid, Guid>();
                }
                catch
                {
                    _index = await BuildIndexAsync();
                    await SaveIndexAsync();
                }
            }
            else
            {
                _index = await BuildIndexAsync();
                await SaveIndexAsync();
            }
        }
        private async Task<Dictionary<Guid, Guid>> BuildIndexAsync()
        {
            return await Task.Run(
                () =>
            {
                var index = new Dictionary<Guid, Guid>();

                if (!Directory.Exists(_baseFolder))
                    return index;

                var userFolders = Directory.GetDirectories(_baseFolder);

                foreach (var userFolder in userFolders)
                {
                    if (!Guid.TryParse(Path.GetFileName(userFolder), out Guid userId))
                        continue;

                    var files = Directory.GetFiles(userFolder, "*.json");
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        if (Guid.TryParse(fileName, out Guid itemId))
                        {
                            index[itemId] = userId;
                        }
                    }
                }

                return index;
            });
        }

        private async Task SaveIndexAsync()
        {
            using FileStream fs = File.Create(_indexFilePath);
            await JsonSerializer.SerializeAsync(fs, _index, _jsonOptions);
        }

        private string GetUserFolder(Guid userId)
        {
            string userFolder = Path.Combine(_baseFolder, userId.ToString());
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }
            return userFolder;
        }

        private string GetItemFilePath(Guid userId, Guid itemId)
        {
            return Path.Combine(GetUserFolder(userId), $"{itemId}.json");
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            string userFolder = GetUserFolder(userId);
            var result = new List<ToDoItem>();

            if (!Directory.Exists(userFolder))
                return result;

            var files = Directory.GetFiles(userFolder, "*.json");

            foreach (var file in files)
            {
           
                    using FileStream fs = File.OpenRead(file);
                    var item = await JsonSerializer.DeserializeAsync<ToDoItem>(fs, cancellationToken: cancellationToken);
                    if (item != null)
                    {
                        result.Add(item);
                    }
             
            }

            return result;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var all = await GetAllByUserId(userId, cancellationToken);
            var active = new List<ToDoItem>();

            foreach (var item in all)
            {
                if (item.State == ToDoItemState.Active)
                    active.Add(item);
            }

            return active;
        }

        public async Task<IReadOnlyList<ToDoItem>> Findd(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var all = await GetAllByUserId(userId, cancellationToken);
            var found = new List<ToDoItem>();

            foreach (var item in all)
            {
                if (predicate(item))
                    found.Add(item);
            }

            return found;
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            if (!_index.TryGetValue(id, out Guid userId))
                return null;

            string filePath = GetItemFilePath(userId, id);

            if (!File.Exists(filePath))
                return null;


                using FileStream fs = File.OpenRead(filePath);
                var item = await JsonSerializer.DeserializeAsync<ToDoItem>(fs, cancellationToken: cancellationToken);
                return item;
     
        }

        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.User == null)
                throw new ArgumentException("ToDoItem.User не может быть null", nameof(item));

            string filePath = GetItemFilePath(item.User.UserId, item.Id);

            using FileStream fs = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fs, item, _jsonOptions, cancellationToken);

            _index[item.Id] = item.User.UserId;
            await SaveIndexAsync();
        }

        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.User == null)
                throw new ArgumentException("ToDoItem.User не может быть null", nameof(item));

            string filePath = GetItemFilePath(item.User.UserId, item.Id);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Задача с Id={item.Id} не найдена для обновления.");

            using FileStream fs = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fs, item, _jsonOptions, cancellationToken);
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            if (!_index.TryGetValue(id, out Guid userId))
                return;

            string filePath = GetItemFilePath(userId, id);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _index.Remove(id);
            await SaveIndexAsync();
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            var all = await GetAllByUserId(userId, cancellationToken);

            foreach (var item in all)
            {
                if (item.Name == name)
                    return true;
            }

            return false;
        }

        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            var all = await GetAllByUserId(userId, cancellationToken);
            int count = 0;

            foreach (var item in all)
            {
                if (item.State == ToDoItemState.Active)
                    count++;
            }

            return count;
        }
    }
}