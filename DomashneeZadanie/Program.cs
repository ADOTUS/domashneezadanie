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
        public static void Main(string[] args)
        {
            var userRepository = new InMemoryUserRepository();
            var userService = new UserService(userRepository);

            var todoRepository = new InMemoryToDoRepository();
            var todoService = new ToDoService(todoRepository);

            var reportService = new ToDoReportService(todoRepository);

            var botClient = new ConsoleBotClient();
            var handler = new UpdateHandler(userService, todoService , reportService);

            botClient.StartReceiving(handler);
        }
    }
}