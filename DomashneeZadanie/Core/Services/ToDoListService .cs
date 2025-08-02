using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Services
{
    public class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository _listRepository;
        private const int MaxListNameLength = 10;

        public ToDoListService(IToDoListRepository listRepository)
        {
            _listRepository = listRepository ?? throw new ArgumentNullException(nameof(listRepository));
        }

        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название списка не может быть пустым.", nameof(name));

            if (name.Length > MaxListNameLength)
                throw new ArgumentException($"Максимальная длина названия списка — {MaxListNameLength} символов.");

            bool exists = await _listRepository.ExistsByName(user.UserId, name, ct);
            if (exists)
                throw new InvalidOperationException($"Список с именем \"{name}\" уже существует.");

            var list = new ToDoList
            {
                Id = Guid.NewGuid(),
                User = user,
                Name = name,
                CreatedAt = DateTime.UtcNow
            };

            await _listRepository.Add(list, ct);

            return list;
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            return _listRepository.Get(id, ct);
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            return _listRepository.Delete(id, ct);
        }

        public Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            return _listRepository.GetByUserId(userId, ct);
        }
    }
}
