using DomashneeZadanie.Core.Services;
using DomashneeZadanie.Core.Dto;
using DomashneeZadanie.Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DomashneeZadanie.Core.Scenarios
{
    public class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _todoService;

        public DeleteTaskScenario(IToDoService todoService)
        {
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteTask;

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Update update,
            CancellationToken ct)
        {
            var chatId = update.CallbackQuery?.Message?.Chat.Id ?? update.Message?.Chat.Id ?? 0;

            switch (context.CurrentStep)
            {
                case null:
                case "Approve":
                {
                    context.CurrentStep = "Delete";
                    if (!context.Data.TryGetValue("TaskId", out var taskIdObj) || taskIdObj is not Guid taskId)
                    {
                        await bot.SendMessage(chatId, "Задача не найдена.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    var task = await _todoService.Get(taskId, ct);
                    if (task == null)
                    {
                        await bot.SendMessage(chatId, "Задача не найдена.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅ Да", "yes"),
                            InlineKeyboardButton.WithCallbackData("❌ Нет", "no")
                        }
                    });

                    await bot.SendMessage(chatId,
                        $"Вы уверены, что хотите удалить задачу \"{task.Name}\"?",
                        replyMarkup: keyboard,
                        cancellationToken: ct);

                    return ScenarioResult.Transition;
                }

                case "Delete":
                {
                    if (update.CallbackQuery?.Data == null)
                        return ScenarioResult.Completed;

                    await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);

                    if (update.CallbackQuery.Data == "yes")
                    {
                        if (!context.Data.TryGetValue("TaskId", out var taskIdObj) || taskIdObj is not Guid taskId)
                        {
                            await bot.SendMessage(chatId, "Задача не найдена.", cancellationToken: ct);
                            return ScenarioResult.Completed;
                        }

                        await _todoService.Delete(taskId, ct);
                        await bot.SendMessage(chatId, "Задача удалена.", cancellationToken: ct);
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
