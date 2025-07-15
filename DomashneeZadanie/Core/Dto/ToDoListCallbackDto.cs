using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; }

        public ToDoListCallbackDto(string action, Guid? toDoListId = null)
            : base(action)
        {
            ToDoListId = toDoListId;
        }

        public static new ToDoListCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new ToDoListCallbackDto(string.Empty, null);

            string[] parts = input.Split('|');
            string action = parts[0];

            Guid? toDoListId = null;
            if (parts.Length > 1 && parts[1] != "none" && Guid.TryParse(parts[1], out var parsedGuid))
                toDoListId = parsedGuid;

            return new ToDoListCallbackDto(action, toDoListId);
        }

        public override string ToString()
        {
            string idPart = ToDoListId.HasValue ? ToDoListId.Value.ToString() : "none";
            return $"{Action}|{idPart}";
        }
    }
}
