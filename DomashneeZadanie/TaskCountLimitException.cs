using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie
{
    class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int cntTasks, int lengthTask) : base($"Максимальное количество добавяемых задач - {cntTasks}. Максимальная длина задачи - {lengthTask}")
        { }
        public TaskCountLimitException(int cntTasks) : base($"Максимальное количество добавяемых задач - {cntTasks}")
        { }
        //public TaskCountLimitException(int cntTasks, string addTask) : base($"Максимальное количество добавленных задач - {cntTasks}, элемент {addTask} в список не добавлен")
        //{ }
    }
}
