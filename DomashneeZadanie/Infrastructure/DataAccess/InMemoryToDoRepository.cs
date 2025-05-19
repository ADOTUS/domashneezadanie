using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DomashneeZadanie.Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _items = new();

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            List<ToDoItem> result = new List<ToDoItem>();
            foreach (var item in _items)
            {
                if (item.UserId == userId)
                {
                    result.Add(item);
                }
            }
            return Task.FromResult((IReadOnlyList<ToDoItem>)result);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            List<ToDoItem> result = new List<ToDoItem>();
            foreach (var item in _items)
            {
                if (item.UserId == userId && item.State == ToDoItemState.Active)
                {
                    result.Add(item);
                }
            }
            return Task.FromResult((IReadOnlyList<ToDoItem>)result);
        }

        public Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            ToDoItem? getted = null;

            foreach (var item in _items)
            {
                if (item.Id == id)
                {
                    getted = item;
                    break;
                }
            }
            return Task.FromResult(getted);
        }
        public Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            _items.Add(item);
            return Task.CompletedTask;
        }

        public Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == item.Id)
                {
                    _items[i] = item;
                    break;
                }
            }

            return Task.CompletedTask;
        }

        public Task Delete(Guid id, CancellationToken cancellationToken)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == id)
                {
                    _items.RemoveAt(i);
                    break;
                }
            }

            return Task.CompletedTask;
        }

        public Task <bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            foreach (var item in _items)
            {
                if (item.UserId == userId && item.Name == name)
                {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public Task<int>  CountActive(Guid userId, CancellationToken cancellationToken)
        {
            int count = 0;
            foreach (var item in _items)
            {
                if (item.UserId == userId && item.State == ToDoItemState.Active)
                {
                    count++;
                }
            }
            return Task.FromResult(count);
        }
        public Task <IReadOnlyList<ToDoItem>> Findd(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            List<ToDoItem> result = new List<ToDoItem>();

            foreach (var item in _items)
            {
                if (item.User != null && item.User.UserId == userId)
                {
                    if (predicate(item))
                    {
                        result.Add(item);
                    }
                }
            }
            return Task.FromResult<IReadOnlyList<ToDoItem>>(result);
        }
    }

}
