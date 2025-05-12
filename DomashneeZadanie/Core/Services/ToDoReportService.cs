using DomashneeZadanie.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _repository;

        public ToDoReportService(IToDoRepository repository)
        {
            _repository = repository;
        }

        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var tasks = _repository.GetAllByUserId(userId);

            int total = 0;
            int completed = 0;
            int active = 0;

            for (int i = 0; i < tasks.Count; i++)
            {
                total++;

                if (tasks[i].State == ToDoItemState.Completed)
                {
                    completed++;
                }
                else if (tasks[i].State == ToDoItemState.Active)
                {
                    active++;
                }
            }

            return (total, completed, active, DateTime.Now);
        }
    }
}
