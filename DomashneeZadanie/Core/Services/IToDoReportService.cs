using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Services
{
    public interface IToDoReportService
    {
        (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId);
    }
}
