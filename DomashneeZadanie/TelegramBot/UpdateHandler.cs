using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Exceptions;
using DomashneeZadanie.Core.Services;
using DomashneZadanie;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace DomashneeZadanie.TelegramBot
{

    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        public int MaxTasks = 10;
        public int MaxNameLength = 255;
        public static bool sucscess = false;
        private readonly IToDoReportService _reportService;

        //public UpdateHandler(IUserService userService, IToDoService todoService)
        //{
        //    _userService = userService;
        //    _todoService = todoService;
        //}
        public UpdateHandler(IUserService userService, IToDoService todoService, IToDoReportService reportService)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
        }
        public void ParamsSetting()
        {
            while (!sucscess)
            {
                try

                {
                    CntTasksSet();
                    LenghtTasksSet();
                }
                catch (TaskCountLimitException ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
        }
        public void CntTasksSet()
        {
            Console.WriteLine("Введите максимальное количество задач для отслеживания:");
            int value = ParseAndValidateInt(Console.ReadLine(), 0, 10, MaxTasks, MaxNameLength);
            MaxTasks = value;
            Console.WriteLine($"Вы ввели: {MaxTasks}");
        }

        public void LenghtTasksSet()
        {
            Console.WriteLine("Введите максимальную длину задачи:");
            int value = ParseAndValidateInt(Console.ReadLine(), 0, 255, MaxTasks, MaxNameLength);
            MaxNameLength = value;
            Console.WriteLine($"Вы ввели: {MaxNameLength}");
        }
        public static int ParseAndValidateInt(string? str, int min, int max, int maxTasks, int maxNameLength)
        {

            if (int.TryParse(str, out int value) && value > min && value < max)
            {
                sucscess = true;
                return value;
            }
            else
            {
                maxTasks = 10;
                maxNameLength = 255;
                sucscess = false;
                throw new TaskCountLimitException(maxTasks, maxNameLength);

            }
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            ParamsSetting();

            if (update.Message == null || update.Message.From == null || string.IsNullOrWhiteSpace(update.Message.Text))
                return;

            long telegramUserId = update.Message.From.Id;
            string telegramUserName = update.Message.From.Username ?? "Unknown";
            string messageText = update.Message.Text.Trim();
            var chat = update.Message.Chat;



            if (messageText.StartsWith("/add"))
            {
                SwAdd(botClient, update, telegramUserId, messageText, MaxTasks, MaxNameLength);
                return;
            }

            if (messageText.StartsWith("/remove"))
            {
                SwRemove(botClient, update, telegramUserId, messageText);
                return;
            }

            if (messageText.StartsWith("/complete"))
            {
                SwComplete(botClient, update, telegramUserId, messageText);
                return;
            }
            if (messageText.StartsWith("/find"))
            {
                SwFind(botClient, update, telegramUserId, messageText);
                return;
            }

            switch (messageText)
            {
                case "/start":
                    {
                        SwStart(botClient, update, telegramUserId, telegramUserName);
                        break;
                    }
                case "/show":
                    {
                        SwShow(botClient, update, telegramUserId);
                        break;
                    }
                case "/help":
                    {
                        SwHelp(botClient, update);
                        break;
                    }
                case "/info":
                    {
                        SwInfo(botClient, update);
                        break;
                    }

                case "/showall":
                    {
                        SwShowAll(botClient, update, telegramUserId);
                        break;
                    }
                case "/report":
                    {
                        SwReport(botClient, update, telegramUserId);
                        break;
                    }

                default:
                    botClient.SendMessage(chat, "Неизвестная команда. Напишите /help.");
                    break;

            }
        }
        public void SwReport(ITelegramBotClient botClient, Update update, long telegramUserId)
        {
            var chat = update.Message.Chat;
            var user = _userService.GetUser(telegramUserId);
            if (user == null)
            {
                botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.");
                return;
            }

            var stats = _reportService.GetUserStats(user.UserId);

            string message = $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}.\n" +
                             $"Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};";

            botClient.SendMessage(chat, message);
        }
        public void SwFind(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText)
        {
            var chat = update.Message.Chat;
            var user = _userService.GetUser(telegramUserId);
            if (user == null)
            {
                botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.");
                return;
            }

            string prefix = messageText.Substring("/find".Length).Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                botClient.SendMessage(chat, "Укажите часть имени задачи после /find.");
                return;
            }

            var found = _todoService.Find(user, prefix);
            if (found.Count == 0)
            {
                botClient.SendMessage(chat, $"Задачи, начинающиеся на '{prefix}', не найдены.");
                return;
            }

            for (int i = 0; i < found.Count; i++)
            {
                var task = found[i];
                string msg = $"{i + 1}. {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}";
                botClient.SendMessage(chat, msg);
            }
        }
        public void SwStart(ITelegramBotClient botClient, Update update, long telegramUserId, string telegramUserName)
        {
            var chat = update.Message.Chat;
            ToDoUser user = _userService.RegisterUser(telegramUserId, telegramUserName);
            botClient.SendMessage(chat, $"Привет, {user.TelegramUserName}!\nВы зарегистрированы.\nID: {user.UserId}\nДата: {user.RegisteredAt:dd.MM.yyyy HH:mm}");
        }
        public void SwShow(ITelegramBotClient botClient, Update update, long telegramUserId)
        {
            var chat = update.Message.Chat;
            var user = _userService.GetUser(telegramUserId);
            if (user == null)
            {
                botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.");
                return;
            }

            var tasks = _todoService.GetActiveByUserId(user.UserId);
            if (tasks.Count == 0)
            {
                botClient.SendMessage(chat, "Активных задач нет.");
                return;
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                string msg = $"{i + 1}. {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}";
                botClient.SendMessage(chat, msg);
            }
        }
        public void SwShowAll(ITelegramBotClient botClient, Update update, long telegramUserId)
        {
            var chat = update.Message.Chat;
            ToDoUser? showUser = _userService.GetUser(telegramUserId);
            if (showUser == null)
            {
                botClient.SendMessage(update.Message.Chat, "Вы не зарегистрированы. Напишите /start.");
                return;
            }

            var allTasks = _todoService.GetAllByUserId(showUser.UserId);
            if (allTasks.Count == 0)
            {
                botClient.SendMessage(update.Message.Chat, "Задачи не найдены.");
                return;
            }

            for (int i = 0; i < allTasks.Count; i++)
            {
                ToDoItem task = allTasks[i];
                string message = $"({task.State}) {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}";
                botClient.SendMessage(update.Message.Chat, message);
            }

        }
        public void SwHelp(ITelegramBotClient botClient, Update update)
        {
            var chat = update.Message.Chat;
            botClient.SendMessage(chat, "Доступные команды:\n/start\n/add\n/complete\n/remove\n/show\n/showall\n/report\n/find\n/info\n/help");
        }
        public void SwInfo(ITelegramBotClient botClient, Update update)
        {
            var chat = update.Message.Chat;
            botClient.SendMessage(chat, "Версия программы 0.07 , дата создания 20.02.2025");
        }
        public void SwAdd(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText, int MaxTasks, int MaxNameLength)
        {
            var chat = update.Message.Chat;
            var user = _userService.GetUser(telegramUserId);
            if (user == null)
            {
                botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.");
                return;
            }

            string name = messageText.Substring("/add".Length).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                botClient.SendMessage(chat, "Укажите описание задачи после команды /add.");
                return;
            }

            try
            {
                ToDoItem task = _todoService.Add(user, name, MaxTasks, MaxNameLength);
                botClient.SendMessage(chat, $"Задача добавлена: {task.Name}");
            }
            catch (Exception ex)
            {
                botClient.SendMessage(chat, $"Ошибка: {ex.Message}");
            }
            return;
        }
        public void SwRemove(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText)
        {
            var chat = update.Message.Chat;
            var user = _userService.GetUser(telegramUserId);
            if (user == null)
            {
                botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.");
                return;
            }

            string input = messageText.Substring("/remove".Length).Trim();

            if (!int.TryParse(input, out int index))
            {
                botClient.SendMessage(chat, "Введите номер задачи, например: /remove 1");
                return;
            }

            var activeTasks = _todoService.GetActiveByUserId(user.UserId);

            if (index < 1 || index > activeTasks.Count)
            {
                botClient.SendMessage(chat, $"Некорректный номер задачи. Введите от 1 до {activeTasks.Count}.");
                return;
            }

            var taskToRemove = activeTasks[index - 1];
            _todoService.Delete(taskToRemove.Id);
            botClient.SendMessage(chat, $"Задача '{taskToRemove.Name}' удалена.");
            return;
        }
        public void SwComplete(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText)
        {
            var chat = update.Message.Chat;
            var user = _userService.GetUser(telegramUserId);
            if (user == null)
            {
                botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.");
                return;
            }

            string input = messageText.Substring("/complete".Length).Trim();
            var activeTasks = _todoService.GetActiveByUserId(user.UserId);

            Guid taskId = Guid.Parse(input);

            foreach (var task in activeTasks)
            {
                if (task.Id == taskId)
                {
                    _todoService.MarkCompleted(task.Id);
                    botClient.SendMessage(update.Message.Chat, $"Задача \"{task.Name}\" помечена исполненной.");
                }
            }
            return;
        }
    }
}
