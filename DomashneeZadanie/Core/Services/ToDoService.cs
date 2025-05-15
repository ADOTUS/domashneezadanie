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

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _repository.GetAllByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _repository.GetActiveByUserId(userId);
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            int currentTaskCount = _repository.CountActive(user.UserId);
            if (currentTaskCount >= _maxTasks)
            {
                throw new ArgumentException($"Превышено количество задач. Максимум — {_maxTasks}.");
            }

            if (name.Length > _maxNameLength)
            {
                throw new ArgumentException($"Длина задачи превышает допустимую. Максимум — {_maxNameLength} символов.");
            }

            bool exists = _repository.ExistsByName(user.UserId, name);
            if (exists)
            {
                throw new DuplicateTaskException(name);
            }

            ToDoItem newItem = new ToDoItem(user, name);
            _repository.Add(newItem);
            return newItem;
        }

        public void MarkCompleted(Guid id)
        {
            ToDoItem? item = _repository.Get(id);
            if (item != null)
            {
                item.State = ToDoItemState.Completed;
                item.StateChangedAt = DateTime.Now;
                _repository.Update(item);
            }
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _repository.Findd(user.UserId, new NamePrefixFind(namePrefix).IsMatch);
        }
    }
}
