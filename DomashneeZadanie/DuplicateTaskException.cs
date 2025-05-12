using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie
{
    class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string task) : base($"Задача ‘{task}’ уже существует в списке") { }
    }
}
