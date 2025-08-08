using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.Infrastructure.DataAccess
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            var user = await db.ToDoUsers
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            return user == null ? null : ModelMapper.MapFromModel(user);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            var user = await db.ToDoUsers
                .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, cancellationToken);

            return user == null ? null : ModelMapper.MapFromModel(user);
        }

        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            await db.InsertAsync(ModelMapper.MapToModel(user), token: cancellationToken);
        }
        public async Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();

            var users = await db.ToDoUsers.ToListAsync(cancellationToken);

            return users.Select(ModelMapper.MapFromModel).ToList();
        }
    }
}
