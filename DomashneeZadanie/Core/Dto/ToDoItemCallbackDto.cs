using System;

namespace DomashneeZadanie.Core.Dto
{
    public class ToDoItemCallbackDto : CallbackDto
    {
        public Guid? ToDoItemId { get; }

        public ToDoItemCallbackDto(string action, Guid? toDoItemId = null)
            : base(action)
        {
            ToDoItemId = toDoItemId;
        }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new ToDoItemCallbackDto(string.Empty, null);

            string[] parts = input.Split('|');
            string action = parts[0];

            Guid? toDoItemId = null;
            if (parts.Length > 1 && parts[1] != "none" && Guid.TryParse(parts[1], out var parsedGuid))
                toDoItemId = parsedGuid;

            return new ToDoItemCallbackDto(action, toDoItemId);
        }

        public override string ToString()
        {
            string idPart = ToDoItemId.HasValue ? ToDoItemId.Value.ToString() : "none";
            return $"{Action}|{idPart}";
        }
    }
}