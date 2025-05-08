using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie
{
    public class ExitRequestedException : Exception
    {
        public ExitRequestedException() : base("Пользователь вышел из программы") { }
    }
}
