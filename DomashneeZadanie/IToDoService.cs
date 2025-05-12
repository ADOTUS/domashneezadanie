using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie
{
    public interface IToDoService
    {
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        ToDoItem Add(ToDoUser user, string name, int MaxTasks, int MaxNameLength);
        void MarkCompleted(Guid id);
        void Delete(Guid id);
    }
}
