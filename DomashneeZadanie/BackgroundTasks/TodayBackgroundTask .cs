using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Services;
using System.Text;

namespace DomashneeZadanie.BackgroundTasks
{
    public class TodayBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;
        public TodayBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository) : base(TimeSpan.FromMinutes(1), nameof(TodayBackgroundTask))
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var users = await _userRepository.GetUsers(ct);
            if (users == null)
            {
                Console.WriteLine("вернул null");
            }
            else
            {
                Console.WriteLine($"пользователй: {users.Count}");
             
            }
            var today = DateTime.UtcNow.Date;

            foreach (var user in users)
            {
                var tasks = await _toDoRepository.GetActiveWithDeadline(user.UserId, today, today.AddDays(1).Date, ct);

                if (tasks.Count == 0)
                    continue;

                var textBuilder = new StringBuilder();
                textBuilder.AppendLine("*Ваши задачи на сегодня:*");

                foreach (var task in tasks)
                {
                    textBuilder.AppendLine($"• {task.Name} — до {task.Deadline:HH:mm}");
                }

                string type = $"Today_{DateOnly.FromDateTime(today)}";

                await _notificationService.ScheduleNotification(
                    user.UserId,
                    type,
                    textBuilder.ToString(),
                    scheduledAt: DateTime.UtcNow,
                    ct);
            }
        
        }
    }
}
