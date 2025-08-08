using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Services;

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
            var today = DateTime.UtcNow.Date;

            foreach (var user in users)
            {

                var overdueTasks = await _toDoRepository.GetActiveWithDeadline(user.UserId, today.AddDays(-1), today, ct);

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
