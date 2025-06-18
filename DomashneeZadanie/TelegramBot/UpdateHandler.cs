using DomashneeZadanie.Core.Dto;
using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Scenarios;
using DomashneeZadanie.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
namespace DomashneeZadanie.TelegramBot
{
    public delegate void MessageEventHandler(string message);

    public class UpdateHandler : IUpdateHandler
    {
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateCompleted;
        private readonly ITelegramBotClient _botClient;

        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        public static bool sucscess = false; 
        private readonly IToDoReportService _reportService;
        private readonly IToDoListService _toDoListService;

        private readonly int _maxTasks;
        private readonly int _maxNameLength;

        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IScenarioContextRepository _contextRepository;
        public UpdateHandler(
            ITelegramBotClient botClient,
            IUserService userService,
            IToDoService todoService,
            IToDoReportService reportService,
            IToDoListService toDoListService,
            int maxTasks,
            int maxNameLength,
            IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository)

        {
            _botClient = botClient;
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
            _toDoListService = toDoListService;
            _maxTasks = maxTasks;
            _maxNameLength = maxNameLength;
            _scenarios = scenarios;
            _contextRepository = contextRepository;
        }
        private IScenario GetScenario(ScenarioType scenario)
        {
            foreach (var handler in _scenarios)
            {
                if (handler.CanHandle(scenario))
                    return handler;
            }

            throw new InvalidOperationException($"Сценарий '{scenario}' не найден.");
        }
        private async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);
            var result = await scenario.HandleMessageAsync(_botClient, context, update, ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(context.UserId, ct);
                await _botClient.SendMessage(update.Message.Chat, "Сценарий завершён.",
                    replyMarkup: GetKeyboard(true), cancellationToken: ct);
            }
            else
            {
                await _contextRepository.SetContext(context.UserId, context, ct);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"HandleError: {exception})");
            return Task.CompletedTask;
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery != null)
            {
                await OnCallbackQuery(update, update.CallbackQuery, cancellationToken);
            }

            if (update.Message == null || update.Message.From == null || string.IsNullOrWhiteSpace(update.Message.Text) || update.Message.Chat == null)
                return;

            long telegramUserId = update.Message.From.Id;

            var chat = update.Message.Chat;
            string messageText = update.Message.Text.Trim();

            if (messageText == "/cancel")
            {
                await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                await botClient.SendMessage(chat.Id,
                    "Сценарий отменён.",
                    replyMarkup: GetKeyboard(true), 
                    cancellationToken: cancellationToken);
                return;
            }

            var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
       
            if (context != null)
            {
                await ProcessScenario(context, update, cancellationToken);
                return;
            }
            string telegramUserName = update.Message.From.Username ?? "Unknown";
 
            OnHandleUpdateStarted?.Invoke(messageText);
            try
            {
                if (messageText.StartsWith("/remove"))
                {
                    await SwRemove(botClient, update, telegramUserId, messageText, cancellationToken);
                    return;
                }

                if (messageText.StartsWith("/complete"))
                {
                    await SwComplete(botClient, update, telegramUserId, messageText, cancellationToken);
                    return;
                }
                if (messageText.StartsWith("/find"))
                {
                    await SwFind(botClient, update, telegramUserId, messageText, cancellationToken);
                    return;
                }

                switch (messageText)
                {
                    case "/addtask":
                        {
                            var scenarioContext = new ScenarioContext(ScenarioType.AddTask)
                            {
                                UserId = update.Message.Chat.Id,
                                CurrentStep = null
                            };

                            await botClient.SendMessage(update.Message.Chat
                                                        , "Добавление задачи начато."
                                                        ,  replyMarkup: new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("/cancel") } }) { ResizeKeyboard = true }
                                                        ,  cancellationToken: cancellationToken);

                            await ProcessScenario(scenarioContext, update, cancellationToken);
                            return;
                        }

                    case "/start":
                        {
                            await SwStart(botClient, update, telegramUserId, telegramUserName, cancellationToken);
                            var isRegistered = await _userService.GetUser(telegramUserId, cancellationToken) != null;
                            break;
                        }
                    case "/show":
                        {
                           await SwShow(botClient, update, telegramUserId, cancellationToken);
                           break;
                        }
                    case "/help":
                        {
                            await SwHelp(botClient, update, cancellationToken);
                            break;
                        }
                    case "/info":
                        {
                            await SwInfo(botClient, update, cancellationToken);
                            break;
                        }

                    case "/report":
                        {
                            await SwReport(botClient, update, telegramUserId, cancellationToken);
                            break;
                        }

                    default:
                        await botClient.SendMessage(update.Message.Chat, "Неизвестная команда. Напишите /help.", cancellationToken: cancellationToken);
                        break;

                }
            }
            finally
            {
                OnHandleUpdateCompleted?.Invoke(messageText);
            }
        }
        private async Task SwReport(ITelegramBotClient botClient, Update update, long telegramUserId, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat == null)
            return;

            var chat = update.Message.Chat; 

            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(update.Message.Chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken: cancellationToken);
                return;
            }

            var stats = await _reportService.GetUserStats(user.UserId, cancellationToken);

            string message = $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}.\n" +
                             $"Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};";

            await botClient.SendMessage(chat, message, cancellationToken: cancellationToken);
        }
        private async Task SwFind(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat == null)
            return;
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken: cancellationToken);
                return;
            }

            string prefix = messageText.Substring("/find".Length).Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                await botClient.SendMessage(chat, "Укажите часть имени задачи после /find.", cancellationToken: cancellationToken);
                return;
            }

            var found = await _todoService.Find(user, prefix, cancellationToken: cancellationToken);
            if (found.Count == 0)
            {
                await botClient.SendMessage(chat, $"Задачи, начинающиеся на '{prefix}', не найдены.", cancellationToken: cancellationToken);
                return;
            }

            for (int i = 0; i < found.Count; i++)
            {
                var task = found[i];
                string msg = $"{i + 1}. {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}";
                await botClient.SendMessage(chat, msg, cancellationToken: cancellationToken);
            }
        }
        private async Task SwStart(ITelegramBotClient botClient, Update update, long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat == null)
                return;
            var chat = update.Message.Chat;
            ToDoUser? user = await _userService.RegisterUser(telegramUserId, telegramUserName, cancellationToken);
            await botClient.SendMessage(chat, $"Привет, {user?.TelegramUserName}!\nВы зарегистрированы.\nID: {user?.UserId}\nДата: {user?.RegisteredAt:dd.MM.yyyy HH:mm}", cancellationToken: cancellationToken);
        }
        private async Task SwShow(ITelegramBotClient botClient, Update update, long telegramUserId, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat == null)
                return;

            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.",
                        replyMarkup: GetKeyboard(false), cancellationToken: cancellationToken);
                return;
            }

            var lists = await _toDoListService.GetUserLists(user.UserId, cancellationToken);

            var inlineButtons = new List<List<InlineKeyboardButton>>();

            var noListCallback = new ToDoListCallbackDto();
            noListCallback.Action = "show";
            noListCallback.ToDoListId = null;
            string noListCallbackData = noListCallback.ToString();

            var noListButton = InlineKeyboardButton.WithCallbackData("📌 Без списка", noListCallbackData);
            inlineButtons.Add(new List<InlineKeyboardButton> { noListButton });

            for (int i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                var cb = new ToDoListCallbackDto();
                cb.Action = "show";
                cb.ToDoListId = list.Id;
                string cbData = cb.ToString();

                var button = InlineKeyboardButton.WithCallbackData(list.Name, cbData);
                inlineButtons.Add(new List<InlineKeyboardButton> { button });
            }

            var addButton = InlineKeyboardButton.WithCallbackData("🆕 Добавить", "addlist");
            inlineButtons.Add(new List<InlineKeyboardButton> { addButton });
             
            var deleteButton = InlineKeyboardButton.WithCallbackData("❌ Удалить", "deletelist");
            inlineButtons.Add(new List<InlineKeyboardButton> { deleteButton });

            var inlineKeyboard = new InlineKeyboardMarkup(inlineButtons);

            await botClient.SendMessage(chat.Id,
                "Выберите список:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        private async Task SwHelp(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat == null)
                return;
            var chat = update.Message.Chat;
            await botClient.SendMessage(chat, "Доступные команды:\n/start\n/addtask\n/cancel\n/complete\n/remove\n/show\n/showall\n/report\n/find\n/info\n/help", cancellationToken: cancellationToken);
        }
        private async Task SwInfo(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat == null)
                return;
            var chat = update.Message.Chat;
            await botClient.SendMessage(chat, "Версия программы 0.11 , дата обновления 14.05.2025", cancellationToken: cancellationToken);
        }
        
        private async Task SwRemove(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat == null)
                return;
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken: cancellationToken);
                return;
            }

            string input = messageText.Substring("/remove".Length).Trim();

            if (!int.TryParse(input, out int index))
            {
                await botClient.SendMessage(chat, "Введите номер задачи, например: /remove 1", cancellationToken: cancellationToken);
                return;
            }

            var activeTasks = await _todoService.GetActiveByUserId(user.UserId, cancellationToken);

            if (index < 1 || index > activeTasks.Count)
            {
                await botClient.SendMessage(chat, $"Некорректный номер задачи. Введите от 1 до {activeTasks.Count}.", cancellationToken: cancellationToken);
                return;
            }

            var taskToRemove = activeTasks[index - 1];
            await _todoService.Delete(taskToRemove.Id, cancellationToken);
            await botClient.SendMessage(chat, $"Задача '{taskToRemove.Name}' удалена.", cancellationToken: cancellationToken);
            return;
        }
        private async Task SwComplete(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText, CancellationToken cancellationToken)
        {
            if (update.Message?.Chat == null)
                return;
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken: cancellationToken);
                return;
            }

            string input = messageText.Substring("/complete".Length).Trim();
            var activeTasks = await _todoService.GetActiveByUserId(user.UserId, cancellationToken);

            Guid taskId = Guid.Parse(input);

            foreach (var task in activeTasks)
            {
                if (task.Id == taskId)
                {
                    await _todoService.MarkCompleted(task.Id, cancellationToken);
                    await botClient.SendMessage(update.Message.Chat, $"Задача \"{task.Name}\" помечена исполненной.", cancellationToken: cancellationToken);
                }
            }
            return;
        }
        private static ReplyMarkup GetKeyboard(bool isRegistered)
        {
            if (!isRegistered)
            {
                return new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { "/start" } })
                {
                    ResizeKeyboard = true
                };
            }

            return new ReplyKeyboardMarkup(
                    new[]
                    {
                    new KeyboardButton[] { "/addtask" },
                    new KeyboardButton[] { "/showall", "/show" },
                    new KeyboardButton[] { "/report" }
                    }
                                            )
            {
                ResizeKeyboard = true
            };
        }
        private async Task OnCallbackQuery(Update update, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.From == null)
                return;

            long telegramUserId = callbackQuery.From.Id;

            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                return;
            }
            var callbackDto = CallbackDto.FromString(callbackQuery.Data ?? string.Empty);

            switch (callbackDto.Action)
            {
                case "show":
                    {
                        var listCallbackDto = ToDoListCallbackDto.FromString(callbackQuery.Data ?? string.Empty);

                        IReadOnlyList<ToDoItem> tasks;

                        if (listCallbackDto.ToDoListId == null)
                        {
                            tasks = await _todoService.GetByUserIdAndList(user.UserId, null, cancellationToken);

                        }
                        else
                        {
                            tasks = await _todoService.GetByUserIdAndList(user.UserId, listCallbackDto.ToDoListId.Value, cancellationToken);
                        }

                        string message;

                        if (tasks.Count == 0)
                        {
                            message = "Задач в этом списке нет.";
                        }
                        else
                        {
                            message = "Задачи в выбранном списке:\n";
                            for (int i = 0; i < tasks.Count; i++)
                            {
                                var t = tasks[i];
                                message += $"{i + 1}. {t.Name}\n";
                            }
                        }

                        await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

                        await _botClient.SendMessage(callbackQuery.Message.Chat.Id,
                            message,
                            cancellationToken: cancellationToken);

                        break;
                    }

                default:
                    {
                        await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Неизвестная команда.", cancellationToken: cancellationToken);
                        break;
                    }
            }
        }
    }

}
