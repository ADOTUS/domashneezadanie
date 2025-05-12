using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _items = new();

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            List<ToDoItem> result = new List<ToDoItem>();
            foreach (var item in _items)
            {
                if (item.UserId == userId)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            List<ToDoItem> result = new List<ToDoItem>();
            foreach (var item in _items)
            {
                if (item.UserId == userId && item.State == ToDoItemState.Active)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public ToDoItem? Get(Guid id)
        {
            foreach (var item in _items)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }

        public void Add(ToDoItem item)
        {
            _items.Add(item);
        }

        public void Update(ToDoItem item)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == item.Id)
                {
                    _items[i] = item;
                    break;
                }
            }
        }

        public void Delete(Guid id)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == id)
                {
                    _items.RemoveAt(i);
                    break;
                }
            }
        }

        public bool ExistsByName(Guid userId, string name)
        {
            foreach (var item in _items)
            {
                if (item.UserId == userId && item.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public int CountActive(Guid userId)
        {
            int count = 0;
            foreach (var item in _items)
            {
                if (item.UserId == userId && item.State == ToDoItemState.Active)
                {
                    count++;
                }
            }
            return count;
        }
        public IReadOnlyList<ToDoItem> Findd(Guid userId, Func<ToDoItem, bool> predicate)
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
            return result;
        }
    }

}
