using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Services;
using DomashneeZadanie.Infrastructure.DataAccess.Models;
using LinqToDB; 

namespace DomashneeZadanie.Infrastructure.DataAccess
{
    public class NotificationService : INotificationService
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public NotificationService(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            var exists = await db.Notifications.AnyAsync(n => n.UserId == userId && n.Type == type, ct);

            if (exists)
                return false;

            await db.InsertAsync(new NotificationModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Text = text,
                ScheduledAt = scheduledAt,
                IsNotified = false,
                NotifiedAt = null
            }, token: ct);

            return true;
        }

        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();
            var items = await db.Notifications
                .Join(db.ToDoUsers,
                    n => n.UserId,
                    u => u.UserId,
                    (n, u) => new { Notification = n, User = u })
                .Where(x => !x.Notification.IsNotified && x.Notification.ScheduledAt <= scheduledBefore)
                .ToListAsync(ct);

            return items.Select(x =>
            {
                var entity = ModelMapper.MapFromModel(x.Notification);
                entity.User.TelegramUserId = x.User.TelegramUserId;
                entity.User.TelegramUserName = x.User.TelegramUserName;
                return entity;
            }).ToList();
        }

        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            await db.Notifications
                .Where(n => n.Id == notificationId)
                .Set(n => n.IsNotified, true)
                .Set(n => n.NotifiedAt, DateTime.UtcNow)
                .UpdateAsync(ct);
        }
    }
}
