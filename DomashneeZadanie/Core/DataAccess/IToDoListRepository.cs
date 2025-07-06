using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.DataAccess
{
    public interface IToDoListRepository
    {
        Task<ToDoList?> Get(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct);
        Task Add(ToDoList list, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct);
    }
}
