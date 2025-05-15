using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Exceptions
{
    class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int cntTasks) : base($"Максимальное количество добавяемых задач - {cntTasks}")
        { }
    }
}
