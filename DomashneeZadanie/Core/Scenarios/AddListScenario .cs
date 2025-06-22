using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomashneeZadanie.Core.Scenarios
{
    public class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _todoListService;

        public AddListScenario(IUserService userService, IToDoListService todoListService)
        {
            _userService = userService;
            _todoListService = todoListService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddList;

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Update update,
            CancellationToken ct)
        {
            if (update.Message == null)
                return ScenarioResult.Completed;

            long chatId = update.Message.Chat.Id;

            string? currentStep = context.CurrentStep;

            if (currentStep == null)
            {
                long telegramUserId = 0;
                string? telegramUserName = null;

                if (update.Message != null && update.Message.From != null)
                {
                    telegramUserId = update.Message.From.Id;
                    telegramUserName = update.Message.From.Username;
                }

                ToDoUser? user = await _userService.GetUser(telegramUserId, ct);
                if (user == null)
                {
                    await bot.SendMessage(chatId, "Вы не зарегистрированы. Напишите /start.", cancellationToken: ct);
                    return ScenarioResult.Completed;
                }
                
                context.Data["User"] = user;
                context.CurrentStep = "Name";

                await bot.SendMessage(chatId, "Введите название списка:", cancellationToken: ct);
                return ScenarioResult.Transition;
            }

            if (currentStep == "Name")
            {
                string name = string.Empty;
                if (update.Message != null && update.Message.Text != null)
                {
                    name = update.Message.Text;
                }

                if (context.Data.ContainsKey("User"))
                {
                    object userObject = context.Data["User"];
                    ToDoUser? user = userObject as ToDoUser;

                    if (user != null)
                    {
                        await _todoListService.Add(user, name, ct);
                        await bot.SendMessage(chatId, $"Список \"{name}\" добавлен.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                }

                await bot.SendMessage(chatId, "Ошибка: пользователь не найден.", cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            await bot.SendMessage(chatId, "Неизвестный шаг сценария.", cancellationToken: ct);
            return ScenarioResult.Completed;
        }
    }
}
