using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Exceptions;
using DomashneeZadanie.Core.Scenarios;
using DomashneeZadanie.Core.Services;
using DomashneeZadanie.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DomashneeZadanie.Scenarios;
namespace DomashneZadanie
{
    internal static class Program
    {

        public static async Task Main(string[] args)
        {
            string? token = Environment.GetEnvironmentVariable("Telegram_TOKEN", EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Bot token not found. Please set the TELEGRAM_BOT_TOKEN environment variable.");
                return;
            }
            Console.WriteLine(token);


            int maxTasks = SetGlobalVar("Введите максимальное количество задач (1–10):", 1, 10, 0);
            int maxNameLength = SetGlobalVar("Введите максимальную длину задачи (1–255):", 1, 255, 1);

            var userRepository = new FileUserRepository("UserData");
            var userService = new UserService(userRepository);


            string baseFolder = "ToDoData";
            IToDoRepository todoRepository = new FileToDoRepository(baseFolder);

            var todoService = new ToDoService(todoRepository, maxTasks, maxNameLength);

            var reportService = new ToDoReportService(todoRepository);

            IToDoListRepository listRepository = new FileToDoListRepository("ListData");
            IToDoListService toDoListService = new ToDoListService(listRepository);

            var botClient = new TelegramBotClient(token);

            var scenarios = new List<IScenario>
                {
                    new AddTaskScenario(userService, todoService)
                };

            IScenarioContextRepository contextRepository = new InMemoryScenarioContextRepository();

       
            var handler = new UpdateHandler(botClient,userService, todoService, reportService, toDoListService, maxTasks, maxNameLength, scenarios, contextRepository);

            using var cts = new CancellationTokenSource();

            await SetBotCommands(botClient, cancellationToken: cts.Token); 

            handler.OnHandleUpdateStarted += HandleStarted; 
            handler.OnHandleUpdateCompleted += HandleCompleted;

            try
            {
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = [UpdateType.Message],
                    DropPendingUpdates = true
                }; 
                botClient.StartReceiving(updateHandler: handler, receiverOptions: receiverOptions, cancellationToken: cts.Token);

                Console.WriteLine("Бот запущен. Нажмите клавишу 'A' для выхода, любую другую — для информации о боте.");

                while (true)
                {
                    ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.A)
                    {
                        Console.WriteLine("\nЗавершение работы...");
                        cts.Cancel();
                        break;
                    }
                    else
                    {
                        var me = await botClient.GetMe();
                        Console.WriteLine($"\nИнформация о боте:");
                        Console.WriteLine($"Username: @{me.Username}");
                        Console.WriteLine($"Имя: {me.FirstName}");
                        Console.WriteLine($"ID: {me.Id}");
                    }
                }
            }
            finally
            {
                Console.WriteLine("Подписки на события удалены");
                handler.OnHandleUpdateStarted -= HandleStarted;
                handler.OnHandleUpdateCompleted -= HandleCompleted;

            }

            void HandleStarted(string message)
            {
                Console.WriteLine($"Началась обработка сообщения '{message}'");
            }

            void HandleCompleted(string message)
            {
                Console.WriteLine($"Закончилась обработка сообщения '{message}'");
            }

        }
        private static int SetGlobalVar(string msg, int min, int max, int varType)
        {
            while (true)
            {
                Console.WriteLine(msg);
                string? input = Console.ReadLine();
                try
                {
                    int value = ParseAndValidateInt(input, min, max, varType);
                    Console.WriteLine($"Вы ввели: {value}");
                    return value;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }
        private static int ParseAndValidateInt(string? input, int min, int max, int varType)
        {
            if (int.TryParse(input, out int value) && value >= min && value <= max)
            {
                return value;
            }
            else
            if (varType == 0)
                throw new TaskCountLimitException(max);
            else
                throw new TaskLengthLimitException(max);

        }
        public static async Task SetBotCommands(ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            var commands = new List<BotCommand>
                 {
                     new() { Command = "start",     Description = "Регистрация пользователя" },
                     new() { Command = "add",       Description = "Добавить задачу (/add TaskName)" },
                     new() { Command = "remove",    Description = "Удалить задачу (/remove TaskId)" },
                     new() { Command = "complete",  Description = "Завершить задачу (/complete TaskId)" },
                     new() { Command = "show",      Description = "Показать активные задачи" },
                     new() { Command = "showall",   Description = "Показать все задачи" },
                     new() { Command = "report",    Description = "Статистика задач" },
                     new() { Command = "find",      Description = "Поиск задач (/find keyword)" },
                     new() { Command = "help",      Description = "Список команд" },
                     new() { Command = "info",      Description = "Информация о боте" },
                 };

            await botClient.SetMyCommands(commands, cancellationToken: cancellationToken);
        }
    }
}