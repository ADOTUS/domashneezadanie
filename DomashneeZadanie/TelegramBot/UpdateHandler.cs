using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Dto;
using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Scenarios;
using DomashneeZadanie.Core.Services;
using System;
using System.Numerics;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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
        private readonly IToDoReportService _reportService;
        private readonly IToDoListService _toDoListService;
        private readonly int _maxTasks;
        private readonly int _maxNameLength;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IScenarioContextRepository _contextRepository;
        private static int _pageSize = 5;

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
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"HandleError: {exception})");
            return Task.CompletedTask;
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine($"CallbackQuery test: {update.CallbackQuery?.Data}");
            if (update.CallbackQuery != null)
            {
                await HandleCallbackQuery(update.CallbackQuery, cancellationToken);
                return;
            }

            if (update.Message?.From == null || string.IsNullOrWhiteSpace(update.Message.Text))
                return;

            string text = update.Message.Text.Trim();
            long userId = update.Message.From.Id;

            if (text == "/cancel")
            {
                await _contextRepository.ResetContext(userId, cancellationToken);
                await _botClient.SendMessage(update.Message.Chat.Id, "Сценарий отменен", replyMarkup: GetRegisteredKb(), cancellationToken: cancellationToken);
                return;
            }

            var context = await _contextRepository.GetContext(userId, cancellationToken);
            if (context != null)
            {
                await ProcessScenario(context, update, cancellationToken);
                return;
            }

            OnHandleUpdateStarted?.Invoke(text);
            try
            {
                await HandleCommand(update, text, userId, cancellationToken);
            }
            finally
            {
                OnHandleUpdateCompleted?.Invoke(text);
            }

            if (context == null && text != "/cancel" && text != "/addtask")
            {
                var user = await _userService.GetUser(userId, cancellationToken);
                bool registered = user != null;
                await _botClient.SendMessage(update.Message.Chat.Id, "Выберите команду:", replyMarkup: GetRegisteredKb(), cancellationToken: cancellationToken);
            }
        }

        private async Task HandleCommand(Update update, string command, long userId, CancellationToken ct)
        {
            if (update.Message?.From == null || string.IsNullOrWhiteSpace(update.Message.Text))
                return;

            var chatId = update.Message.Chat.Id;
            var user = await _userService.GetUser(userId, ct);

            if (user == null && command != "/start" )
            {
                await _botClient.SendMessage(chatId, "Вы не зарегистрированы. Напишите /start.", cancellationToken: ct);
                return;
            }

            switch (command)
            {

                case "/start":
                    var name = update.Message.From.Username ?? "Unknown";
                    user = await _userService.RegisterUser(userId, name, ct);
                    await _botClient.SendMessage(chatId, $"Привет, {user?.TelegramUserName}! Вы зарегистрированы.", cancellationToken: ct);
                    break;

                case "/show":
                    if (user == null)
                    {
                        await _botClient.SendMessage(chatId, "Вы не зарегистрированы. Напишите /start.", cancellationToken: ct);
                    }
                    else
                    {
                        await SwShow(chatId, user, ct);
                    }
                    break;

                case "/addtask":
                    await StartScenario(userId, ScenarioType.AddTask, ct);
                    break;

                case "🆕Добавить":
                    await StartScenario(userId, ScenarioType.AddList, ct);
                    break;

                case "/report":
                    
                    if (user == null)
                    {
                        await _botClient.SendMessage(chatId, "Вы не зарегистрированы. Напишите /start.", cancellationToken: ct);
                    }
                    else
                    {
                        await SwReport(chatId, user, ct);
                    }
                    break;

                case "/help":
                    await SwHelp(_botClient, update, ct);
                    break;

                case "/info":
                    await SwInfo(_botClient, update, ct);
                    break;

                case string c when c.StartsWith("/find"):
#pragma warning disable CS8604 // 116 строка выполняет проверку
                    await SwFind(_botClient, update, user, command, ct);
#pragma warning restore CS8604 // 116 строка выполняет проверку
                    break;

                default:
                    await _botClient.SendMessage(chatId, "Неизвестная команда. Напишите /help.", cancellationToken: ct);
                    break;
            }
        }
        private async Task SwHelp(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null || update.Message.Chat == null)
                return;

            ChatId chat = update.Message.Chat;

            string helpText = "Доступные команды:\n" +
                              "/start\n" +
                              "/addtask\n" +
                              "/cancel\n" +
                              "/show\n" +
                              "/show -> add new list\n" +
                              "/show -> remove list\n" +
                              "/show -> mark completed\n" +
                              "/show -> remove\n" +
                              "/report\n" +
                              "/find\n" +
                              "/info\n" +
                              "/help";

            await botClient.SendMessage(chat, helpText, cancellationToken: cancellationToken);
        }

        private async Task SwInfo(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null || update.Message.Chat == null)
                return;

            ChatId chat = update.Message.Chat;

            string infoText = "Версия программы 0.13 , дата обновления 06.07.2025";

            await botClient.SendMessage(chat, infoText, cancellationToken: cancellationToken);
        }
 
        private async Task SwReport(long chatId, ToDoUser user, CancellationToken ct)
        {
           var stats = await _reportService.GetUserStats(user.UserId, ct);

            string message = $"📊 Статистика задач на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}:\n" +
                             $"— Всего: {stats.total}\n" +
                             $"— Завершено: {stats.completed}\n" +
                             $"— Активных: {stats.active}";

            await _botClient.SendMessage(chatId, message, cancellationToken: ct);
        }
        private async Task SwFind(ITelegramBotClient bot, Update update, ToDoUser user, string messageText, CancellationToken ct)
        {
            var chat = update.Message?.Chat;
            if (chat == null)
                return;

            if (user == null)
            {
                await bot.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken: ct);
                return;
            }

            var prefix = messageText["/find".Length..].Trim();
            if (string.IsNullOrWhiteSpace(prefix))
            {
                await bot.SendMessage(chat, "Укажите часть имени задачи после /find.", cancellationToken: ct);
                return;
            }

            var tasks = await _todoService.Find(user, prefix, ct);
            if (tasks.Count == 0)
            {
                await bot.SendMessage(chat, $"Задачи, начинающиеся на '{prefix}', не найдены.", cancellationToken: ct);
                return;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];
                sb.AppendLine($"{i + 1}. {t.Name} — {t.CreatedAt:dd.MM.yyyy HH:mm:ss} — ID: {t.Id}");
            }

            await bot.SendMessage(chat, sb.ToString(), cancellationToken: ct);
        }
        private async Task SwShow(long chatId, ToDoUser user, CancellationToken ct)
        {
            if (user == null)
            {
                await _botClient.SendMessage(chatId, "Вы не зарегистрированы. Напишите /start.", replyMarkup: GetUnregisteredKb(), cancellationToken: ct);
                return;
            }

            var lists = await _toDoListService.GetUserLists(user.UserId, ct);
            var buttons = new List<List<InlineKeyboardButton>>
        {
            new() { InlineKeyboardButton.WithCallbackData("📌 Без списка", new ToDoListCallbackDto("show", null).ToString()) }
        };

            foreach (var list in lists)
            {
                buttons.Add(new List<InlineKeyboardButton>
                                {
                                    InlineKeyboardButton.WithCallbackData($"📄 {list.Name}", new ToDoListCallbackDto("show", list.Id).ToString()),
                                    InlineKeyboardButton.WithCallbackData("❌ Удалить", new ToDoListCallbackDto("deletelist", list.Id).ToString())
                                }
                            );
            }

            buttons.Add(new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("🆕 Добавить", "addlist")
        });

            var keyboard = new InlineKeyboardMarkup(buttons);
            await _botClient.SendMessage(chatId, "Выберите список:", replyMarkup: keyboard, cancellationToken: ct);
        }
        private async Task HandleCallbackQuery(CallbackQuery callback, CancellationToken ct)
        {
            if (callback.Message == null)
                return;
            var userId = callback.From.Id;
            var user = await _userService.GetUser(userId, ct);
             
            if (user == null)
            {
                await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                return;
            }
            await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);

            var context = await _contextRepository.GetContext(userId, ct);

            if (callback.Data == null)
            {
                await _botClient.SendMessage(callback.Message.Chat.Id, "Пустые данные callback.", cancellationToken: ct);
                return;
            }

            if (callback.Data == "/cancel")
            {
                await _contextRepository.ResetContext(userId, ct);
                await _botClient.SendMessage(callback.Message.Chat.Id, "Сценарий отменен", replyMarkup: GetRegisteredKb(), cancellationToken: ct);
                await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                return;
            }

            if (callback.Data.StartsWith("addtask|list|"))
                {
                    var fakeUpdate = new Update { CallbackQuery = callback };
                    await ProcessScenario(context, fakeUpdate, ct);
                    return;
                }


            if (user == null)
            {
                await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                return;
            }

            if (callback.Data == "yes" || callback.Data == "no")
            {
                if (context == null)
                {
                    await _botClient.SendMessage(callback.Message.Chat.Id, "Нет активного сценария.", cancellationToken: ct);
                    return;
                }

                if (context.CurrentScenario == ScenarioType.DeleteList ||
                    context.CurrentScenario == ScenarioType.DeleteTask)
                {
                    var fakeUpdate = new Update { CallbackQuery = callback };
                    await ProcessScenario(context, fakeUpdate, ct);
                    return;
                }

                await _botClient.SendMessage(callback.Message.Chat.Id, "Нет активного подходящего сценария для обработки выбора.", cancellationToken: ct);
                return;
            }

            var baseDto = CallbackDto.FromString(callback.Data ?? "");

            if (baseDto.Action == "deletelist")
            {
                var dto = ToDoListCallbackDto.FromString(callback.Data ?? "");
                if (dto.ToDoListId.HasValue)
                {
                    var list = await _toDoListService.Get(dto.ToDoListId.Value, ct);
                    if (list == null)
                    {
                        await _botClient.SendMessage(callback.Message.Chat.Id, "Список не найден.", cancellationToken: ct);
                        return;
                    }

                    context = new ScenarioContext(ScenarioType.DeleteList)
                    {
                        UserId = userId,
                        CurrentScenario = ScenarioType.DeleteList,
                        CurrentStep = "Approve",
                        Data = new Dictionary<string, object>
                        {
                            ["User"] = user,
                            ["ToDoList"] = list
                        }
                    };

                    await _contextRepository.SetContext(userId, context, ct);

                    var fakeUpdate = new Update { CallbackQuery = callback };
                    await ProcessScenario(context, fakeUpdate, ct);
                }
            }
            else if (baseDto.Action == "addlist")
            {
                await StartScenario(userId, ScenarioType.AddList, ct);
            }
            else if (baseDto.Action == "show")
            {
                var listDto = PagedListCallbackDto.FromString(callback.Data ?? "");
                var userObj = await _userService.GetUser(userId, ct);
                if (userObj == null)
                {
                    await _botClient.SendMessage(callback.Message.Chat.Id, "Вы не зарегистрированы. Напишите /start.", cancellationToken: ct);
                    return;
                }

                IReadOnlyList<ToDoItem> allTasks;
                if (listDto.ToDoListId.HasValue)
                    allTasks = await _todoService.GetByUserIdAndList(userObj.UserId, listDto.ToDoListId, ct);
                else
                    allTasks = await _todoService.GetAllByUserId(userObj.UserId, ct);

                var activeTasks = allTasks.Where(t => t.State != ToDoItemState.Completed).ToList();

                string text;
                var buttons = new List<List<InlineKeyboardButton>>();
                if (activeTasks.Count == 0)
                {
                    text = "В списке нет задач.";
                }
                else
                {
                    text = "Активные задачи:";
                    int totalPages = (int)Math.Ceiling((double)activeTasks.Count / _pageSize);
                    var pageItems = activeTasks.Skip(listDto.Page * _pageSize).Take(_pageSize).ToList();

                    foreach (var task in pageItems)
                    {
                        var dto = new ToDoItemCallbackDto("showtask", task.Id);
                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData($"⬜ {task.Name}", dto.ToString())
                        });
                    }

                    var navButtons = new List<InlineKeyboardButton>();
                    if (listDto.Page > 0)
                    {
                        var prevDto = new PagedListCallbackDto("show", listDto.ToDoListId, listDto.Page - 1);
                        navButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️", prevDto.ToString()));
                    }
                    if (listDto.Page < totalPages - 1)
                    {
                        var nextDto = new PagedListCallbackDto("show", listDto.ToDoListId, listDto.Page + 1);
                        navButtons.Add(InlineKeyboardButton.WithCallbackData("➡️", nextDto.ToString()));
                    }
                    if (navButtons.Count > 0)
                        buttons.Add(navButtons);
                }

                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(
                        "☑️Посмотреть выполненные",
                        new PagedListCallbackDto("show_completed", listDto.ToDoListId, 0).ToString())
                });

                var keyboard = new InlineKeyboardMarkup(buttons);

                if (callback.Message != null)
                {
                    await _botClient.EditMessageText(
                        chatId: callback.Message.Chat.Id,
                        messageId: callback.Message.MessageId,
                        text: text,
                        replyMarkup: keyboard,
                        cancellationToken: ct);
                }
                return;
            }
            else if (baseDto.Action == "show_completed")
            {
                var listDto = PagedListCallbackDto.FromString(callback.Data ?? "");
                var userObj = await _userService.GetUser(userId, ct);
                if (userObj == null)
                {
                    await _botClient.SendMessage(callback.Message.Chat.Id, "Вы не зарегистрированы. Напишите /start.", cancellationToken: ct);
                    return;
                }

                IReadOnlyList<ToDoItem> allTasks;
                if (listDto.ToDoListId.HasValue)
                    allTasks = await _todoService.GetByUserIdAndList(userObj.UserId, listDto.ToDoListId, ct);
                else
                    allTasks = await _todoService.GetAllByUserId(userObj.UserId, ct);

                var completedTasks = allTasks.Where(t => t.State == ToDoItemState.Completed).ToList();

                string text;
                var buttons = new List<List<InlineKeyboardButton>>();
                if (completedTasks.Count == 0)
                {
                    text = "Задач нет";
                }
                else
                {
                    text = "Выполненные задачи:";
                    int totalPages = (int)Math.Ceiling((double)completedTasks.Count / _pageSize);
                    var pageItems = completedTasks.Skip(listDto.Page * _pageSize).Take(_pageSize).ToList();

                    foreach (var task in pageItems)
                    {
                        var dto = new ToDoItemCallbackDto("showtask", task.Id);
                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData($"✅ {task.Name}", dto.ToString())
                        });
                    }

                    var navButtons = new List<InlineKeyboardButton>();
                    if (listDto.Page > 0)
                    {
                        var prevDto = new PagedListCallbackDto("show_completed", listDto.ToDoListId, listDto.Page - 1);
                        navButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️", prevDto.ToString()));
                    }
                    if (listDto.Page < totalPages - 1)
                    {
                        var nextDto = new PagedListCallbackDto("show_completed", listDto.ToDoListId, listDto.Page + 1);
                        navButtons.Add(InlineKeyboardButton.WithCallbackData("➡️", nextDto.ToString()));
                    }
                    if (navButtons.Count > 0)
                        buttons.Add(navButtons);
                }

                var keyboard = new InlineKeyboardMarkup(buttons);

                if (callback.Message != null)
                {
                    await _botClient.EditMessageText(
                        chatId: callback.Message.Chat.Id,
                        messageId: callback.Message.MessageId,
                        text: text,
                        replyMarkup: keyboard,
                        cancellationToken: ct);
                }
                return;
            }
            else if (baseDto.Action == "addtask")
            {
                if (context != null && context.CurrentScenario == ScenarioType.AddTask)
                {
                    var fakeUpdate = new Update { CallbackQuery = callback };
                    await ProcessScenario(context, fakeUpdate, ct);
                }
                else
                {
                    await _botClient.SendMessage(callback.Message.Chat.Id, "Нет активного сценария добавления задачи.", cancellationToken: ct);
                }
            }
            else if (baseDto.Action == "showtask")
            {
                var dto = ToDoItemCallbackDto.FromString(callback.Data ?? "");
                if (dto.ToDoItemId.HasValue)
                {
                    var task = await _todoService.Get(dto.ToDoItemId.Value, ct);
                    if (task == null)
                    {
                        await _botClient.SendMessage(callback.Message.Chat.Id, "Задача не найдена.", cancellationToken: ct);
                        return;
                    }

                    var info = $"Задача: {task.Name}\n" +
                               $"Статус: {(task.State == ToDoItemState.Completed ? "✅ Выполнена" : "⬜ Активна")}\n" +
                               $"Создана: {task.CreatedAt:dd.MM.yyyy HH:mm}\n" +
                               $"Дедлайн: {task.Deadline:dd.MM.yyyy}\n" +
                               $"ID: {task.Id}";

                    var completeDto = new ToDoItemCallbackDto("completetask", task.Id);
                    var deleteDto = new ToDoItemCallbackDto("deletetask", task.Id);

                    var buttons = new List<List<InlineKeyboardButton>>
                    {
                        new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("✅ Выполнить", completeDto.ToString()),
                            InlineKeyboardButton.WithCallbackData("❌ Удалить", deleteDto.ToString())
                        }
                    };

                    var keyboard = new InlineKeyboardMarkup(buttons);

                    await _botClient.SendMessage(callback.Message.Chat.Id, info, replyMarkup: keyboard, cancellationToken: ct);
                }
                return;
            }
            else if (baseDto.Action == "completetask")
            {
                var dto = ToDoItemCallbackDto.FromString(callback.Data ?? "");
                if (dto.ToDoItemId.HasValue)
                {
                    await _todoService.MarkCompleted(dto.ToDoItemId.Value, ct);
                    await _botClient.SendMessage(callback.Message.Chat.Id, "Задача помечена как выполненная.", cancellationToken: ct);
                }
                return;
            }
            else if (baseDto.Action == "deletetask")
            {
                var dto = ToDoItemCallbackDto.FromString(callback.Data ?? "");
                if (dto.ToDoItemId.HasValue)
                {
                    var task = await _todoService.Get(dto.ToDoItemId.Value, ct);
                    if (task == null )
                    {
                        await _botClient.SendMessage(callback.Message.Chat.Id, "Задача не найдена.", cancellationToken: ct);
                        return;
                    }

                    context = new ScenarioContext(ScenarioType.DeleteTask)
                    {
                        UserId = userId,
                        CurrentStep = "Approve",
                        CurrentScenario = ScenarioType.DeleteTask,
                        Data = new Dictionary<string, object>
                        {
                            ["TaskId"] = task.Id
                        }
                    };

                    await _contextRepository.SetContext(userId, context, ct);

                    var fakeUpdate = new Update { CallbackQuery = callback };
                    await ProcessScenario(context, fakeUpdate, ct);
                }
                return;
            }
            else
            {
                await _botClient.SendMessage(callback.Message.Chat.Id, "Неизвестная команда.", cancellationToken: ct);
            }
        }
        private async Task StartScenario(long userId, ScenarioType scenario, CancellationToken ct)
        {
            var context = new ScenarioContext(scenario)
            {
                UserId = userId,
                CurrentScenario = scenario
            };

            await _contextRepository.SetContext(userId, context, ct);
            await _botClient.SendMessage(userId, "Начался сценарий", replyMarkup: GetCancelKb(), cancellationToken: ct);

            var fakeUpdate = new Update
            {
                Message = new Message
                {
                    Chat = new Chat { Id = userId },
                    From = new User { Id = userId },
                    Text = string.Empty
                }
            };

            await ProcessScenario(context, fakeUpdate, ct);
        }
        private async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = _scenarios.FirstOrDefault(s => s.CanHandle(context.CurrentScenario))
                ?? throw new InvalidOperationException($"Сценарий {context.CurrentScenario} не найден");

            var result = await scenario.HandleMessageAsync(_botClient, context, update, ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(context.UserId, ct);
                await _botClient.SendMessage(update.GetChatId(), "Закончился сценарий.", replyMarkup: GetRegisteredKb(), cancellationToken: ct);
            }
            else
            {
                await _contextRepository.SetContext(context.UserId, context, ct);
            }
        }

        private static ReplyKeyboardMarkup GetRegisteredKb()
        {
            return new ReplyKeyboardMarkup(new[]
                {
            new[] { new KeyboardButton("/addtask") },
            new[] { new KeyboardButton("/show") },
            new[] { new KeyboardButton("/report") }
        })
            { ResizeKeyboard = true };
        }

        private static ReplyKeyboardMarkup GetUnregisteredKb()
        {
            return new ReplyKeyboardMarkup(new[]
                {
            new[] { new KeyboardButton("/start") }
        })
            { ResizeKeyboard = true };
        }

        private static ReplyKeyboardMarkup GetCancelKb()
        {
            return new ReplyKeyboardMarkup(new[]
                {
            new[] { new KeyboardButton("/cancel") }
        })
            { ResizeKeyboard = true };
        }


    }

}
