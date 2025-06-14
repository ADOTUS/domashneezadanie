using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Scenarios;
using DomashneeZadanie.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomashneeZadanie.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;

        public AddTaskScenario(IUserService userService, IToDoService todoService)
        {
            _userService = userService;
            _todoService = todoService;
        }
        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }
        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Update update,
            CancellationToken ct)
        {
            var chatId = update.Message.Chat.Id;

            switch (context.CurrentStep)
            {
                case null:
                    {
                        var user = await _userService.GetUser(update.Message.From.Id, ct);
                        if (user == null)
                        {
                            await bot.SendMessage(chatId,
                                "Вы не зарегистрированы. Пожалуйста, зарегистрируйтесь.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        if (context.Data == null)
                            context.Data = new Dictionary<string, object>();

                        context.Data["User"] = user;

                        await bot.SendMessage(chatId, "Введите название задачи:", cancellationToken: ct);

                        context.CurrentStep = "Name";

                        return ScenarioResult.Transition;
                    }

                case "Name":
                    {
                        if (context.Data == null ||
                            !context.Data.TryGetValue("User", out var userObj) ||
                            userObj is not ToDoUser user)
                        {
                            await bot.SendMessage(chatId,
                                "Произошла ошибка. Попробуйте снова.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        var taskName = update.Message.Text?.Trim();
                        if (string.IsNullOrWhiteSpace(taskName))
                        {
                            await bot.SendMessage(chatId,
                                "Название задачи не может быть пустым. Введите название задачи:", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }
                        context.Data["TaskName"] = taskName;

                        await bot.SendMessage(chatId, "Введите дедлайн задачи в формате dd.MM.yyyy:", cancellationToken: ct);
                        context.CurrentStep = "Deadline";
                        return ScenarioResult.Transition;
                        
                    }
                case "Deadline":
                    {
                        if (context.Data == null
                            || !context.Data.TryGetValue("User", out var userObj)
                            || userObj is not ToDoUser user
                            || !context.Data.TryGetValue("TaskName", out var taskNameObj)
                            || taskNameObj is not string taskName)
                        {
                            await bot.SendMessage(chatId, "Произошла ошибка. Попробуйте снова.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        var deadlineText = update.Message.Text?.Trim();

                        if (!DateTime.TryParseExact(deadlineText, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var deadline))
                        {
                            await bot.SendMessage(chatId,
                                "Некорректный формат даты. Пожалуйста, введите дату в формате dd.MM.yyyy:", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }

                        try
                        {
                            await _todoService.Add(user, taskName, deadline, ct);

                            await bot.SendMessage(chatId,
                                $"Задача \"{taskName}\" успешно добавлена с дедлайном {deadline:dd.MM.yyyy}.", cancellationToken: ct);
                        }
                        catch (Exception ex)
                        {
                            await bot.SendMessage(chatId,
                                $"Ошибка при добавлении задачи: {ex.Message}", cancellationToken: ct);
                        }

                        return ScenarioResult.Completed;
                    }
                default:
                    {
                        await bot.SendMessage(chatId,
                            "Неизвестный шаг сценария. Попробуйте снова.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
            }
        }
    }
}