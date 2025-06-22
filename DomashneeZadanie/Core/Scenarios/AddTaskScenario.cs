using DomashneeZadanie.Core.Dto;
using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Scenarios;
using DomashneeZadanie.Core.Services;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DomashneeZadanie.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoListService _toDoListService;

        public AddTaskScenario(IUserService userService, IToDoService todoService, IToDoListService toDoListService)
        {
            _userService = userService;
            _todoService = todoService;
            _toDoListService = toDoListService;
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
            //long? chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;

            long? chatId = null;
            if (update.Message != null && update.Message.Chat != null)
            {
                chatId = update.Message.Chat.Id;
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.Message != null)
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
            }

            //long? userIdNullable = update.Message?.From.Id ?? update.CallbackQuery?.From.Id;

            long? userIdNullable = null;

            if (update.Message != null && update.Message.From != null)
            {
                userIdNullable = update.Message.From.Id;
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.From != null)
            {
                userIdNullable = update.CallbackQuery.From.Id;
            }

            //long? chatIdNullable = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;

            long? chatIdNullable = null;
            if (update.Message != null && update.Message.Chat != null)
            {
                chatIdNullable = update.Message.Chat.Id;
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.Message != null)
            {
                chatIdNullable = update.CallbackQuery.Message.Chat.Id;
            }


            if (chatIdNullable == null || userIdNullable == null)
            {
                return ScenarioResult.Completed;
            }

            long userId = userIdNullable.Value;

            if (chatId == 0 || userId == 0)
            {
                return ScenarioResult.Completed;
            }

            if (context.Data == null)
                context.Data = new Dictionary<string, object>();

            if (chatId == null)
            {
                return ScenarioResult.Completed;
            }

            switch (context.CurrentStep)
            {
                case null:
                    {
                        var user = await _userService.GetUser(userId, ct);
                        if (user == null)
                        {
                            await bot.SendMessage(chatId, "Вы не зарегистрированы. Пожалуйста, зарегистрируйтесь.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        context.Data["User"] = user;

                        await bot.SendMessage(chatId, "Введите название задачи:", cancellationToken: ct);
                        context.CurrentStep = "Name";
                        return ScenarioResult.Transition;
                    }

                case "Name":
                    {
                        var taskName = update.Message?.Text?.Trim();
                        if (string.IsNullOrWhiteSpace(taskName))
                        {
                            await bot.SendMessage(chatId, "Название задачи не может быть пустым. Введите снова:", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }

                        context.Data["TaskName"] = taskName;

                        await bot.SendMessage(chatId, "Введите дедлайн задачи в формате dd.MM.yyyy:", cancellationToken: ct);
                        context.CurrentStep = "Deadline";
                        return ScenarioResult.Transition;
                    }

                case "Deadline":
                    {
                        var deadlineText = update.Message?.Text?.Trim();
                        if (!DateTime.TryParseExact(deadlineText, "dd.MM.yyyy", null,
                                System.Globalization.DateTimeStyles.None, out var deadline))
                        {
                            await bot.SendMessage(chatId, "Некорректный формат даты. Пожалуйста, введите в формате dd.MM.yyyy:", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }

                        context.Data["Deadline"] = deadline;

                        var user = (ToDoUser)context.Data["User"];
                        var lists = await _toDoListService.GetUserLists(user.UserId, ct);

                        var buttons = new List<List<InlineKeyboardButton>>();

                        var noneDto = new ToDoListCallbackDto("addtask", null);

                        buttons.Add(new List<InlineKeyboardButton>
                                        {
                                            InlineKeyboardButton.WithCallbackData("📌 Без списка", noneDto.ToString())
                                        }
                                    );

                        foreach (var list in lists)
                        {
                            var dto = new ToDoListCallbackDto("addtask", list.Id);

                            buttons.Add(new List<InlineKeyboardButton>
                                             {
                                                 InlineKeyboardButton.WithCallbackData(list.Name, dto.ToString())
                                             }
                                        );
                        }

                        var keyboard = new InlineKeyboardMarkup(buttons);

                        await bot.SendMessage(chatId, "Выберите список для задачи:", replyMarkup: keyboard, cancellationToken: ct);

                        context.CurrentStep = "ChooseList";

                        return ScenarioResult.Transition;
                    }

                case "ChooseList":
                    {
                        if (update.CallbackQuery == null)
                        {
                            await bot.SendMessage(chatId, "Пожалуйста, выберите список, нажав на кнопку.", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }

                        var data = update.CallbackQuery.Data;

                        if (string.IsNullOrWhiteSpace(data))
                        {
                            await bot.SendMessage(chatId, "Неверные данные выбора. Попробуйте снова.", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }

                        CallbackDto baseDto = CallbackDto.FromString(data);
                        if (baseDto.Action != "addtask")
                        {
                            await bot.SendMessage(chatId, "Неверная кнопка. Попробуйте снова.", cancellationToken: ct);
                            return ScenarioResult.Transition;
                        }

                        ToDoListCallbackDto dto = ToDoListCallbackDto.FromString(data);

                        var user = (ToDoUser)context.Data["User"];
                        var taskName = (string)context.Data["TaskName"];
                        var deadline = (DateTime)context.Data["Deadline"];

                        ToDoList? list = null;

                        if (dto.ToDoListId.HasValue)
                        {
                            list = await _toDoListService.Get(dto.ToDoListId.Value, ct);
                            if (list == null)
                            {
                                await bot.SendMessage(chatId, "Выбранный список не найден. Попробуйте заново.", cancellationToken: ct);
                                return ScenarioResult.Transition;
                            }
                        }

                        try
                        {
                            await _todoService.Add(user, taskName, deadline, list, ct);
                            await bot.SendMessage(chatId,
                                $"Задача \"{taskName}\" успешно добавлена в список \"{list?.Name ?? "Без списка"}\" с дедлайном {deadline:dd.MM.yyyy}.",
                                cancellationToken: ct);
                        }
                        catch (Exception ex)
                        {
                            await bot.SendMessage(chatId, $"Ошибка при добавлении задачи: {ex.Message}", cancellationToken: ct);
                        }

                        return ScenarioResult.Completed;
                    }
                default:
                    {
                        await bot.SendMessage(chatId, "Неизвестный шаг сценария. Попробуйте снова.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
            }
        }
    }

}