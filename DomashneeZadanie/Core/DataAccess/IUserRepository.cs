using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.DataAccess
{
    public interface IUserRepository
    {
        Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken);
        Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken);
        Task Add(ToDoUser user, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken cancellationToken);
    }
}
