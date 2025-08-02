using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.DataAccess
{
    public interface IToDoRepository
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken);
        Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken);
        Task Add(ToDoItem item, CancellationToken cancellationToken);
        Task Update(ToDoItem item, CancellationToken cancellationToken);
        Task Delete(Guid id, CancellationToken cancellationToken);
        Task <bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken);
        Task <int> CountActive(Guid userId, CancellationToken cancellationToken);
    }
}
