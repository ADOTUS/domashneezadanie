using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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
    public delegate void MessageEventHandler(string message);

    public class UpdateHandler : IUpdateHandler
    {
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateCompleted;

        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        public static bool sucscess = false;
        private readonly IToDoReportService _reportService;

        private readonly int _maxTasks;
        private readonly int _maxNameLength;

        public UpdateHandler(IUserService userService, IToDoService todoService, IToDoReportService reportService, int MaxTasks, int MaxNameLength)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
            _maxTasks = MaxTasks;
            _maxNameLength = MaxNameLength;
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"HandleError: {exception})");
            return Task.CompletedTask;
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null || update.Message.From == null || string.IsNullOrWhiteSpace(update.Message.Text))
                return;

            long telegramUserId = update.Message.From.Id;
            string telegramUserName = update.Message.From.Username ?? "Unknown";
            string messageText = update.Message.Text.Trim();
            var chat = update.Message.Chat;


            OnHandleUpdateStarted?.Invoke(messageText);
            try
            {
                if (messageText.StartsWith("/add"))
                {
                    await SwAdd(botClient, update, telegramUserId, messageText, _maxTasks, _maxNameLength, cancellationToken);
                    return;
                }

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
                    case "/start":
                        {
                            await SwStart(botClient, update, telegramUserId, telegramUserName, cancellationToken);
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

                    case "/showall":
                        {
                            await SwShowAll(botClient, update, telegramUserId, cancellationToken);
                            break;
                        }
                    case "/report":
                        {
                            await SwReport(botClient, update, telegramUserId, cancellationToken);
                            break;
                        }

                    default:
                        await botClient.SendMessage(update.Message.Chat, "Неизвестная команда. Напишите /help.", cancellationToken);
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
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(update.Message.Chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken);
                return;
            }

            var stats = await _reportService.GetUserStats(user.UserId, cancellationToken);

            string message = $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}.\n" +
                             $"Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};";

            await botClient.SendMessage(chat, message, cancellationToken);
        }
        private async Task SwFind(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken);
                return;
            }

            string prefix = messageText.Substring("/find".Length).Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                await botClient.SendMessage(chat, "Укажите часть имени задачи после /find.", cancellationToken);
                return;
            }

            var found = await _todoService.Find(user, prefix, cancellationToken);
            if (found.Count == 0)
            {
                await botClient.SendMessage(chat, $"Задачи, начинающиеся на '{prefix}', не найдены.", cancellationToken);
                return;
            }

            for (int i = 0; i < found.Count; i++)
            {
                var task = found[i];
                string msg = $"{i + 1}. {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}";
                await botClient.SendMessage(chat, msg, cancellationToken);
            }
        }
        private async Task SwStart(ITelegramBotClient botClient, Update update, long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            ToDoUser? user = await _userService.RegisterUser(telegramUserId, telegramUserName, cancellationToken);
            await botClient.SendMessage(chat, $"Привет, {user?.TelegramUserName}!\nВы зарегистрированы.\nID: {user?.UserId}\nДата: {user?.RegisteredAt:dd.MM.yyyy HH:mm}", cancellationToken);
        }
        private async Task SwShow(ITelegramBotClient botClient, Update update, long telegramUserId, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken);
                return;
            }

            var tasks = await _todoService.GetActiveByUserId(user.UserId, cancellationToken);
            if (tasks.Count == 0)
            {
                await botClient.SendMessage(chat, "Активных задач нет.", cancellationToken);
                return;
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                string msg = $"{i + 1}. {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}";
                await botClient.SendMessage(chat, msg, cancellationToken);
            }
        }
        private async Task SwShowAll(ITelegramBotClient botClient, Update update, long telegramUserId, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            ToDoUser? showUser = await _userService.GetUser(telegramUserId, cancellationToken);
            if (showUser == null)
            {
                await botClient.SendMessage(update.Message.Chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken);
                return;
            }

            var allTasks = await _todoService.GetAllByUserId(showUser.UserId, cancellationToken);
            if (allTasks.Count == 0)
            {
                await botClient.SendMessage(update.Message.Chat, "Задачи не найдены.", cancellationToken);
                return;
            }

            for (int i = 0; i < allTasks.Count; i++)
            {
                ToDoItem task = allTasks[i];
                string message = $"({task.State}) {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}";
                await botClient.SendMessage(update.Message.Chat, message, cancellationToken);
            }

        }
        private async Task SwHelp(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            await botClient.SendMessage(chat, "Доступные команды:\n/start\n/add\n/complete\n/remove\n/show\n/showall\n/report\n/find\n/info\n/help", cancellationToken);
        }
        private async Task SwInfo(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            await botClient.SendMessage(chat, "Версия программы 0.07 , дата создания 20.02.2025", cancellationToken);
        }
        private async Task SwAdd(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText, int MaxTasks, int MaxNameLength, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken);
                return;
            }

            string name = messageText.Substring("/add".Length).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                await botClient.SendMessage(chat, "Укажите описание задачи после команды /add.", cancellationToken);
                return;
            }

            try
            {
                ToDoItem task = await _todoService.Add(user, name, cancellationToken);
                await botClient.SendMessage(chat, $"Задача добавлена: {task.Name}", cancellationToken);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chat, $"Ошибка: {ex.Message}", cancellationToken);
            }
            return;
        }
        private async Task SwRemove(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken);
                return;
            }

            string input = messageText.Substring("/remove".Length).Trim();

            if (!int.TryParse(input, out int index))
            {
                await botClient.SendMessage(chat, "Введите номер задачи, например: /remove 1", cancellationToken);
                return;
            }

            var activeTasks = await _todoService.GetActiveByUserId(user.UserId, cancellationToken);

            if (index < 1 || index > activeTasks.Count)
            {
                await botClient.SendMessage(chat, $"Некорректный номер задачи. Введите от 1 до {activeTasks.Count}.", cancellationToken);
                return;
            }

            var taskToRemove = activeTasks[index - 1];
            await _todoService.Delete(taskToRemove.Id, cancellationToken);
            await botClient.SendMessage(chat, $"Задача '{taskToRemove.Name}' удалена.", cancellationToken);
            return;
        }
        private async Task SwComplete(ITelegramBotClient botClient, Update update, long telegramUserId, string messageText, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;
            var user = await _userService.GetUser(telegramUserId, cancellationToken);
            if (user == null)
            {
                await botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.", cancellationToken);
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
                    await botClient.SendMessage(update.Message.Chat, $"Задача \"{task.Name}\" помечена исполненной.", cancellationToken);
                }
            }
            return;
        }
    }
}
