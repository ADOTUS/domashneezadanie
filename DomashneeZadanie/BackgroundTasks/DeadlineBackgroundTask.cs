using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneeZadanie.BackgroundTasks
{
    public class DeadlineBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;

        public DeadlineBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository)
            : base(TimeSpan.FromMinutes(1), nameof(DeadlineBackgroundTask))
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var users = await _userRepository.GetUsers(ct);

            foreach (var user in users)
            {
                var yesterday = DateTime.UtcNow.AddDays(-1).Date;
                var today = DateTime.UtcNow.Date;

                var overdueTasks = await _toDoRepository.GetActiveWithDeadline(user.UserId, yesterday, today, ct);

                foreach (var task in overdueTasks)
                {
                    var type = $"Deadline_{task.Id}";
                    var text = $"Ой! Вы пропустили дедлайн по задаче: *{task.Name}*";

                    await _notificationService.ScheduleNotification(
                        user.UserId,
                        type,
                        text,
                        DateTime.UtcNow,
                        ct);
                }
            }
        }
    }
}
