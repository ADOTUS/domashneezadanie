using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _repository;
        private readonly IToDoListRepository _listRepository;
        private readonly int _maxTasks;
        private readonly int _maxNameLength;

        public ToDoService(IToDoRepository repository, IToDoListRepository listRepository, int maxTasks, int maxNameLength)
        {
            _repository = repository;
            _listRepository = listRepository;
            _maxTasks = maxTasks;
            _maxNameLength = maxNameLength;
        }
        public async Task<ToDoList?> GetListByName(ToDoUser user, string name, CancellationToken ct)
        {
            var lists = await _listRepository.GetByUserId(user.UserId, ct);

            foreach (var list in lists)
            {
                if (list.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return list;
                }
            }

            return null;
        }
        public async Task <IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _repository.GetAllByUserId(userId, cancellationToken);
        }

        public async Task <IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _repository.GetActiveByUserId(userId, cancellationToken);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var allTasks = await _repository.GetAllByUserId(userId, ct);
            var result = new List<ToDoItem>();

            foreach (var task in allTasks)
            {
                if (listId == null)
                {
                    if (task.List == null)
                    {
                        result.Add(task);
                    }
                }
                else
                {
                    if (task.List != null && task.List.Id == listId.Value)
                    {
                        result.Add(task);
                    }
                }
            }

            return result;
        }
        public async Task <ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken cancellationToken)
        {
            int currentTaskCount = await _repository.CountActive(user.UserId, cancellationToken);


            if (currentTaskCount >= _maxTasks)
            {
                throw new ArgumentException($"Превышено количество задач. Максимум — {_maxTasks}.");
            }

            if (name.Length > _maxNameLength)
            {
                throw new ArgumentException($"Длина задачи превышает допустимую. Максимум — {_maxNameLength} символов.");
            }

            bool exists = await _repository.ExistsByName(user.UserId, name, cancellationToken);
            if (exists)
            {
                throw new DuplicateTaskException(name);
            }

            ToDoItem newItem = new ToDoItem(user, name, deadline, list);
            await _repository.Add(newItem, cancellationToken);
            return newItem;
        }

        public async Task MarkCompleted(Guid id, CancellationToken cancellationToken)
        {
            ToDoItem? item = await _repository.Get(id,cancellationToken);
            if (item != null)
            {
                item.State = ToDoItemState.Completed;
                item.StateChangedAt = DateTime.Now;
                await _repository.Update(item, cancellationToken);
            }
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            await _repository.Delete(id, cancellationToken);
        }
        public async Task <IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken cancellationToken)
        {
            return await _repository.Findd(user.UserId, new NamePrefixFind(namePrefix).IsMatch, cancellationToken);
        }
    }
}
