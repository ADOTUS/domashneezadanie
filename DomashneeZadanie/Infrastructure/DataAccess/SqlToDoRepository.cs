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
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            var query = db.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId);

            var items = await query.ToListAsync(cancellationToken);
            return items.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            var query = db.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId && i.State == 0);

            var items = await query.ToListAsync(cancellationToken);
            return items.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var all = await GetAllByUserId(userId, cancellationToken);
            return all.Where(predicate).ToList();
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            var item = await db.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            return item == null ? null : ModelMapper.MapFromModel(item);
        }

        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            await db.InsertAsync(ModelMapper.MapToModel(item), token: cancellationToken);
        }

        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            await db.UpdateAsync(ModelMapper.MapToModel(item), token: cancellationToken);
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            await db.ToDoItems
                .Where(i => i.Id == id)
                .DeleteAsync(cancellationToken);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            return await db.ToDoItems
                .AnyAsync(i => i.UserId == userId && i.Name == name, cancellationToken);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            using var db = _factory.CreateDataContext();
            return await db.ToDoItems
                .CountAsync(i => i.UserId == userId && i.State == 0, cancellationToken);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(Guid userId, DateTime from, DateTime to, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            var items = await db.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId && i.State == (int)ToDoItemState.Active && i.Deadline >= from && i.Deadline < to)
                .ToListAsync(ct);

            return items.Select(ModelMapper.MapFromModel).ToList();
        }
    }
}
