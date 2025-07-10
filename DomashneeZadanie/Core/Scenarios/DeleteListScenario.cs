using DomashneeZadanie.Core.Dto;
using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace DomashneeZadanie.Core.Scenarios
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _listService;
        private readonly IToDoService _todoService;

        public DeleteListScenario(
            IUserService userService,
            IToDoListService listService,
            IToDoService todoService)
        {
            _userService = userService;
            _listService = listService;
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteList;

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Update update,
            CancellationToken ct)
        {
            var chatId = update.CallbackQuery?.Message?.Chat.Id ?? update.Message?.Chat.Id ?? 0;
            var userId = update.CallbackQuery?.From?.Id ?? update.Message?.From?.Id ?? 0;

            switch (context.CurrentStep)
            {
                case null:
                    {
                        var user = await _userService.GetUser(userId, ct);
                        if (user == null)
                        {
                            await bot.SendMessage(chatId, "Вы не зарегистрированы.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        context.Data ??= new Dictionary<string, object>();
                        context.Data["User"] = user;

                        var lists = await _listService.GetUserLists(user.UserId, ct);
                        if (lists.Count == 0)
                        {
                            await bot.SendMessage(chatId, "У вас нет списков для удаления.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        var keyboard = new List<List<InlineKeyboardButton>>();
                        foreach (var list in lists)
                        {
                            var callbackDto = new ToDoListCallbackDto("deletelist", list.Id);
                            keyboard.Add(new List<InlineKeyboardButton>
                                    {
                                        InlineKeyboardButton.WithCallbackData(list.Name, callbackDto.ToString())
                                    });
                        }

                        await bot.SendMessage(chatId, "Выберите список для удаления:",
                            replyMarkup: new InlineKeyboardMarkup(keyboard), cancellationToken: ct);

                        context.CurrentStep = "Approve";
                        return ScenarioResult.Transition;
                    }

                case "Approve":
                    {
                        if (update.CallbackQuery?.Data == null || context.Data == null)
                            return ScenarioResult.Completed;

                        var dto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                        var toDoList = await _listService.Get(dto.ToDoListId!.Value, ct);
                        if (toDoList == null)
                        {
                            await bot.SendMessage(chatId, "Список не найден.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        context.Data["ToDoList"] = toDoList;

                        var confirmationKeyboard = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅ Да", "yes"),
                            InlineKeyboardButton.WithCallbackData("❌ Нет", "no")
                        }
                    });

                        await bot.SendMessage(chatId,
                            $"Подтверждаете удаление списка \"{toDoList.Name}\" и всех его задач?",
                            replyMarkup: confirmationKeyboard,
                            cancellationToken: ct);

                        context.CurrentStep = "Delete";
                        return ScenarioResult.Transition;
                    }

                case "Delete":
                    {
                        if (update.CallbackQuery?.Data == null || context.Data == null)
                            return ScenarioResult.Completed;

                        if (!context.Data.TryGetValue("User", out var userObj) || userObj is not ToDoUser user)
                            return ScenarioResult.Completed;

                        if (!context.Data.TryGetValue("ToDoList", out var listObj) || listObj is not ToDoList toDoList)
                            return ScenarioResult.Completed;

                        await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);

                        if (update.CallbackQuery.Data == "yes")
                        {
                            var tasks = await _todoService.GetByUserIdAndList(user.UserId, toDoList.Id, ct);
                            foreach (var task in tasks)
                            {
                                await _todoService.Delete(task.Id, ct);
                            }

                            await _listService.Delete(toDoList.Id, ct);

                            await bot.SendMessage(chatId,
                                $"Список \"{toDoList.Name}\" и все его задачи удалены.", cancellationToken: ct);
                        }
                        else if (update.CallbackQuery.Data == "no")
                        {
                            await bot.SendMessage(chatId, "Удаление отменено.", cancellationToken: ct);
                        }

                        return ScenarioResult.Completed;
                    }

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг сценария.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}
