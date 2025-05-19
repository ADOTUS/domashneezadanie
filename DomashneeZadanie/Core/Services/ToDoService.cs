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
        private readonly int _maxTasks;
        private readonly int _maxNameLength;

        public ToDoService(IToDoRepository repository, int maxTasks, int maxNameLength)
        {
            _repository = repository;
            _maxTasks = maxTasks;
            _maxNameLength = maxNameLength;
        }

        public async Task <IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _repository.GetAllByUserId(userId, cancellationToken);
        }

        public async Task <IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _repository.GetActiveByUserId(userId, cancellationToken);
        }
        
        public async Task <ToDoItem> Add(ToDoUser user, string name, CancellationToken cancellationToken)
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

            ToDoItem newItem = new ToDoItem(user, name);
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
