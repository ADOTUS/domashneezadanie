using DomashneeZadanie.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DomashneeZadanie.BackgroundTasks
{
    public class NotificationBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly ITelegramBotClient _bot;

        public NotificationBackgroundTask(
            INotificationService notificationService,
            ITelegramBotClient bot)
            : base(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
        {
            _notificationService = notificationService;
            _bot = bot;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);

            foreach (var notification in notifications)
            {
                await _bot.SendMessage(
                    chatId: notification.User.TelegramUserId,
                    text: notification.Text,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);

                await _notificationService.MarkNotified(notification.Id, ct);
            }
        }
    }
}
