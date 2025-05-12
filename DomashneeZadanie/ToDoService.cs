using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie
{
    public class ToDoService : IToDoService
    {
        private static readonly List<ToDoItem> _tasks = new List<ToDoItem>();
        //private const int MaxTasks = 2;
        //private const int MaxNameLength = 4;
        
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            List<ToDoItem> result = new List<ToDoItem>();
            for (int i = 0; i < _tasks.Count; i++)
            {
                if (_tasks[i].User.UserId == userId)
                {
                    result.Add(_tasks[i]);
                }
            }
            return result;
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            List<ToDoItem> result = new List<ToDoItem>();
            for (int i = 0; i < _tasks.Count; i++)
            {
                if (_tasks[i].User.UserId == userId && _tasks[i].State == ToDoItemState.Active)
                {
                    result.Add(_tasks[i]);
                }
            }
            return result;
        }

        public ToDoItem Add(ToDoUser user, string name, int MaxTasks, int MaxNameLength)
        {
            int count = 0;
            for (int i = 0; i < _tasks.Count; i++)
            {
                if (_tasks[i].User.UserId == user.UserId)
                {
                    count++;
                }
            }

            if (count >= MaxTasks)
            {
                //throw new InvalidOperationException("Превышено максимальное количество задач (10).");
                throw new TaskCountLimitException(MaxTasks);
            }

            if (name.Length > MaxNameLength)
            {
                //throw new ArgumentException("Имя задачи слишком длинное.");
                throw new TaskLengthLimitException(MaxNameLength, name);
            }

            // Проверка на дубликаты
            for (int i = 0; i < _tasks.Count; i++)
            {
                if (_tasks[i].User.UserId == user.UserId && _tasks[i].Name == name)
                {
                    //throw new InvalidOperationException("Такая задача уже существует.");
                    throw new DuplicateTaskException(name);
                }
            }

            ToDoItem newItem = new ToDoItem(user, name);
            _tasks.Add(newItem);
            return newItem;
        }

        public void MarkCompleted(Guid id)
        {
            for (int i = 0; i < _tasks.Count; i++)
            {
                if (_tasks[i].Id == id)
                {
                    _tasks[i].State = ToDoItemState.Completed;
                    _tasks[i].StateChangedAt = DateTime.Now;
                    return;
                }
            }
        }

        public void Delete(Guid id)
        {
            for (int i = 0; i < _tasks.Count; i++)
            {
                if (_tasks[i].Id == id)
                {
                    _tasks.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
