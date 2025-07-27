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
    public class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var query = db.ToDoLists
                .LoadWith(l => l.User)
                .Where(l => l.UserId == userId);

            var items = await query.ToListAsync(ct);
            return items.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var list = await db.ToDoLists
                .LoadWith(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id, ct);

            return list == null ? null : ModelMapper.MapFromModel(list);
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            await db.InsertAsync(ModelMapper.MapToModel(list), token: ct);
        }

        public async Task Update(ToDoList list, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            await db.UpdateAsync(ModelMapper.MapToModel(list), token: ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            await db.ToDoLists
                .Where(l => l.Id == id)
                .DeleteAsync(ct);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            return await db.ToDoLists
                .AnyAsync(l => l.UserId == userId && l.Name == name, ct);
        }
    }

}
