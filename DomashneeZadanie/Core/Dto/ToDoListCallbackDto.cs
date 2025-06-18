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

        public ToDoListCallbackDto()
        {
        }
        public ToDoListCallbackDto(string action, Guid? toDoListId = null)
            : base(action)
        {
            ToDoListId = toDoListId;
        }

        public static new ToDoListCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new ToDoListCallbackDto(string.Empty, null);
            }

            string[] parts = input.Split('|');

            string action = string.Empty;
            if (parts.Length > 0)
            {
                action = parts[0];
            }

            Guid? toDoListId = null;
            if (parts.Length > 1)
            {
                Guid parsedGuid;
                bool success = Guid.TryParse(parts[1], out parsedGuid);
                if (success)
                {
                    toDoListId = parsedGuid;
                }
            }

            return new ToDoListCallbackDto(action, toDoListId);
        }
        public override string ToString()
        {
            string baseString = base.ToString();
            string idString = ToDoListId.HasValue ? ToDoListId.Value.ToString() : string.Empty;
            return baseString + "|" + idString;
        }
    }
}
