using DomashneeZadanie;
using DomashneeZadanie.Core.Services;
using DomashneeZadanie.Infrastructure.DataAccess;
using DomashneeZadanie.TelegramBot;
using Otus.ToDoList.ConsoleBot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneZadanie
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            int maxTasks = SetGlobalVar("Введите максимальное количество задач (1–10):", 1, 10);
            int maxNameLength = SetGlobalVar("Введите максимальную длину задачи (1–255):", 1, 255);


            var userRepository = new InMemoryUserRepository();
            var userService = new UserService(userRepository);

            var todoRepository = new InMemoryToDoRepository();
            var todoService = new ToDoService(todoRepository, maxTasks, maxNameLength);

            var reportService = new ToDoReportService(todoRepository);

            var botClient = new ConsoleBotClient();
            var handler = new UpdateHandler(userService, todoService , reportService, maxTasks, maxNameLength);

            using var cts = new CancellationTokenSource();

            handler.OnHandleUpdateStarted += HandleStarted;
            handler.OnHandleUpdateCompleted += HandleCompleted;

            try
            {
                Console.WriteLine("Нажми Ctrl+C для выхода");
                botClient.StartReceiving(handler, cts.Token);
                await Task.Delay(-1, cts.Token); 
            }
            finally
            {
                //понадобится в следующем дз, 9. В настоящий момент до сюда не дойти.
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
        private static int SetGlobalVar(string msg, int min, int max)
        {
            while (true)
            {
                Console.WriteLine(msg);
                string? input = Console.ReadLine();
                try
                {
                    int value = ParseAndValidateInt(input, min, max);
                    Console.WriteLine($"Вы ввели: {value}");
                    return value;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }
        private static int ParseAndValidateInt(string? input, int min, int max)
        {
            if (int.TryParse(input, out int value) && value >= min && value <= max)
            {
                return value;
            }
            throw new ArgumentException($"Введите целое число от {min} до {max}.");
        }
    }
}