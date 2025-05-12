using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie
{
    class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int lenghtTasks, string addTask) : base($"Максимальное количество символов в задаче - {lenghtTasks}. Вы ввели - {addTask}")
        { }
    }
}
