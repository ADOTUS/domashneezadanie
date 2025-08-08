using DomashneeZadanie.BackgroundTasks;
using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Exceptions;
using DomashneeZadanie.Core.Scenarios;
using DomashneeZadanie.Core.Services;
using DomashneeZadanie.Infrastructure.DataAccess;
using DomashneeZadanie.Scenarios;
using DomashneeZadanie.TelegramBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace DomashneZadanie
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            int maxTasks = SetGlobalVar("Введите максимальное количество задач (1–10):", 1, 10, 0);
            int maxNameLength = SetGlobalVar("Введите максимальную длину задачи (1–255):", 1, 255, 1);

            var builder = Host.CreateApplicationBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("ToDoList");
            Console.WriteLine($"DEBUG Строка подключения: {connectionString}");

            builder.Services.AddSingleton<IDataContextFactory<ToDoDataContext>>(new DataContextFactory(connectionString!));
            builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
            builder.Services.AddScoped<IToDoRepository, SqlToDoRepository>();
            builder.Services.AddScoped<IToDoListRepository, SqlToDoListRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IToDoService>(provider =>
            {
                var todoRepo = provider.GetRequiredService<IToDoRepository>();
                var listRepo = provider.GetRequiredService<IToDoListRepository>();
                return new ToDoService(todoRepo, listRepo, maxTasks, maxNameLength);
            });
            builder.Services.AddScoped<IToDoListService, ToDoListService>();
            builder.Services.AddScoped<IToDoReportService, ToDoReportService>();

            builder.Services.AddScoped<INotificationService, NotificationService>();

            builder.Services.AddSingleton<IScenarioContextRepository, InMemoryScenarioContextRepository>();

            string? token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Bot token not found. Please set the TELEGRAM_BOT_TOKEN environment variable.");
                return;
            }

            builder.Services.AddScoped<UpdateHandler>(provider =>
            {
                var botClient = new TelegramBotClient(token);
                var userService = provider.GetRequiredService<IUserService>();
                var todoService = provider.GetRequiredService<IToDoService>();
                var reportService = provider.GetRequiredService<IToDoReportService>();
                var listService = provider.GetRequiredService<IToDoListService>();
                var contextRepository = provider.GetRequiredService<IScenarioContextRepository>();

                var scenarios = new List<IScenario>
                    {
                        new AddTaskScenario(userService, todoService, listService),
                        new AddListScenario(userService, listService),
                        new DeleteListScenario(userService, listService, todoService),
                        new DeleteTaskScenario(todoService)
                    };

                return new UpdateHandler(
                    botClient,
                    userService,
                    todoService,
                    reportService,
                    listService,
                    maxTasks,
                    maxNameLength,
                    scenarios,
                    contextRepository);
            });

            using var host = builder.Build();

            var handler = host.Services.GetRequiredService<UpdateHandler>();
            var botClient = new TelegramBotClient(token);
            using var cts = new CancellationTokenSource();

            var contextRepo = host.Services.GetRequiredService<IScenarioContextRepository>();
            var notificationService = host.Services.GetRequiredService<INotificationService>();
            var todoRepo = host.Services.GetRequiredService<IToDoRepository>();
            var userRepo = host.Services.GetRequiredService<IUserRepository>();

            var runner = new BackgroundTaskRunner();

            var resetTimeout = TimeSpan.FromMinutes(1);

            runner.AddTask(new ResetScenarioBackgroundTask(
                resetTimeout,
                contextRepo,
                botClient));
            runner.AddTask(new NotificationBackgroundTask(notificationService, botClient));

            runner.AddTask(new DeadlineBackgroundTask(notificationService, userRepo, todoRepo));
            runner.AddTask(new TodayBackgroundTask(notificationService, userRepo, todoRepo));

            runner.StartTasks(cts.Token);


            await SetBotCommands(botClient, cancellationToken: cts.Token);

            handler.OnHandleUpdateStarted += HandleStarted;
            handler.OnHandleUpdateCompleted += HandleCompleted;

            try
            {
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
                    DropPendingUpdates = true
                };

                botClient.StartReceiving(handler, receiverOptions, cancellationToken: cts.Token);

                Console.WriteLine("Бот запущен. Нажмите клавишу 'A' для выхода, любую другую — для информации о боте.");

                while (true)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.A)
                    {
                        Console.WriteLine("\nЗавершение работы и фоновых задач");
                        await runner.StopTasks(CancellationToken.None);
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
                handler.OnHandleUpdateStarted -= HandleStarted;
                handler.OnHandleUpdateCompleted -= HandleCompleted;
            }

            void HandleStarted(string message) =>
                Console.WriteLine($"Началась обработка сообщения '{message}'");

            void HandleCompleted(string message) =>
                Console.WriteLine($"Закончилась обработка сообщения '{message}'");
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
                     new() { Command = "addtask",   Description = "Добавить задачу (/addtask TaskName)" },
                     new() { Command = "show",      Description = "Работа с списками, задачами" },
                     new() { Command = "report",    Description = "Статистика задач" },
                     new() { Command = "find",      Description = "Поиск задач (/find keyword)" },
                     new() { Command = "help",      Description = "Список команд" },
                     new() { Command = "info",      Description = "Информация о боте" },
                 };

            await botClient.SetMyCommands(commands, cancellationToken: cancellationToken);
        }
    }
}