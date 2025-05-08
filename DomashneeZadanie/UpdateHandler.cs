using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DomashneZadanie;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace DomashneeZadanie
{

    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;

        public UpdateHandler(IUserService userService, IToDoService todoService)
        {
            _userService = userService;
            _todoService = todoService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            if (update.Message == null || update.Message.From == null || string.IsNullOrWhiteSpace(update.Message.Text))
                return;

            long telegramUserId = update.Message.From.Id;
            string telegramUserName = update.Message.From.Username ?? "Unknown";
            string messageText = update.Message.Text.Trim();
            var chat = update.Message.Chat;

            if (messageText.StartsWith("/add"))
            {
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
                    ToDoItem task = _todoService.Add(user, name);
                    botClient.SendMessage(chat, $"Задача добавлена: {task.Name}");
                }
                catch (Exception ex)
                {
                    botClient.SendMessage(chat, $"Ошибка: {ex.Message}");
                }
                return;
            }

            if (messageText.StartsWith("/remove"))
            {
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
            if (messageText.StartsWith("/complete"))
            {
                var user = _userService.GetUser(telegramUserId);
                if (user == null)
                {
                    botClient.SendMessage(chat, "Вы не зарегистрированы. Напишите /start.");
                    return;
                }

                string input = messageText.Substring("/complete".Length).Trim();

                if (!int.TryParse(input, out int index))
                {
                    botClient.SendMessage(chat, "Введите номер задачи, например: /complete 2");
                    return;
                }

                var activeTasks = _todoService.GetActiveByUserId(user.UserId);

                if (index < 1 || index > activeTasks.Count)
                {
                    botClient.SendMessage(chat, $"Некорректный номер задачи. Введите от 1 до {activeTasks.Count}.");
                    return;
                }

                var taskToComplete = activeTasks[index - 1];
                _todoService.MarkCompleted(taskToComplete.Id);
                botClient.SendMessage(chat, $"Задача '{taskToComplete.Name}' помечена как выполненная.");
                return;
            }
            switch (messageText)
            {
                case "/start":
                    {
                        ToDoUser user = _userService.RegisterUser(telegramUserId, telegramUserName);
                        botClient.SendMessage(chat, $"Привет, {user.TelegramUserName}!\nВы зарегистрированы.\nID: {user.UserId}\nДата: {user.RegisteredAt:dd.MM.yyyy HH:mm}");
                        break;
                    }
                case "/show":
                    {
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
                        break;
                    }
                case "/help":
                    botClient.SendMessage(chat, "Доступные команды:\n/start\n/add [название]\n/complete\n/show\n/showall\n/remove [id]\n/info\n/help\n/exit");
                    break;

                case "/info":
                    var infoUser = _userService.GetUser(telegramUserId);
                    if (infoUser != null)
                    {
                        string info = $"Пользователь: {infoUser.TelegramUserName}\nID: {infoUser.UserId}\nЗарегистрирован: {infoUser.RegisteredAt:dd.MM.yyyy HH:mm}";
                        botClient.SendMessage(chat, info);
                    }
                    else
                    {
                        botClient.SendMessage(chat, "Вы не зарегистрированы.");
                    }
                    break;

                case "/showall":
                    {
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

                        break;
                    }
                case "/exit":
                    botClient.SendMessage(chat, "До свидания!");
                    throw new ExitRequestedException();

                default:
                    botClient.SendMessage(chat, "Неизвестная команда. Напишите /help.");
                    break;

            }
        }
    }
}
