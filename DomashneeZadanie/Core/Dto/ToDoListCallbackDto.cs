using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }
         

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

            string action = parts.Length > 0 ? parts[0] : string.Empty;
            Guid? toDoListId = null;

            if (parts.Length > 1 && Guid.TryParse(parts[1], out var parsedGuid))
                toDoListId = parsedGuid;

            return new ToDoListCallbackDto(action, toDoListId);
        }

        public override string ToString()
        {
            string? idPart = ToDoListId.HasValue ? ToDoListId.ToString() : string.Empty;
            return $"{Action}|{idPart}";
        }
    }
}
