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

        public ToDoUser? GetUser(Guid userId)
        {
            foreach (var user in _users)
            {
                if (user.UserId == userId)
                    return user;
            }
            return null;
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            foreach (var user in _users)
            {
                if (user.TelegramUserId == telegramUserId)
                    return user;
            }
            return null;
        }

        public void Add(ToDoUser user)
        {
            _users.Add(user);
        }
    }
}
