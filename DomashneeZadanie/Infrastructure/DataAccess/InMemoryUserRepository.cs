using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Infrastructure.DataAccess
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new List<ToDoUser>();

        public Task <ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            ToDoUser? getted = null;

            foreach (var user in _users)
            {
                if (user.UserId == userId)
                    { 
                    getted = user; break; 
                    }
            }
            return Task.FromResult(getted);
        }

        public Task <ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            ToDoUser? getted = null;

            foreach (var user in _users)
            {
                if (user.TelegramUserId == telegramUserId)
                {
                    getted = user; 
                    break;
                }
            }
            return Task.FromResult(getted);
        }

        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            await Task.Yield();
            _users.Add(user);
        }
        public Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<ToDoUser>>(_users.ToList());
        }
    }
}
