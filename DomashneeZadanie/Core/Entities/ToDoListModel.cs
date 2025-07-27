using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Entities
{
    [Table("ToDoList")]
    public class ToDoListModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; }
    }
}
