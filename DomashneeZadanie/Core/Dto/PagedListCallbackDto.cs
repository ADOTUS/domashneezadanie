using System;

namespace DomashneeZadanie.Core.Dto
{
    public class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public PagedListCallbackDto(string action, Guid? toDoListId, int page = 0)
            : base(action, toDoListId)
        {
            Page = page;
        }

        public static new PagedListCallbackDto FromString(string input)
        {
            var baseDto = ToDoListCallbackDto.FromString(input);
            var parts = input.Split('|');
            int page = 0;
            if (parts.Length > 2 && int.TryParse(parts[2], out int parsedPage))
                page = parsedPage;
            return new PagedListCallbackDto(baseDto.Action, baseDto.ToDoListId, page);
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{Page}";
        }
    }
}