using DomashneeZadanie.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Entities
{
    public class ToDoItem
    {
        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public DateTime Deadline { get; set; }
        public ToDoList? List { get; set; }
        //public ToDoItem(ToDoUser user, string name, DateTime deadline, ToDoList? list = null)
        //{
        //    Id = Guid.NewGuid();
        //    User = user;
        //    Name = name;
        //    CreatedAt = DateTime.UtcNow;
        //    State = ToDoItemState.Active;
        //    StateChangedAt = null;
        //    Deadline = deadline;
        //    List = list;
        //}

    }
}
