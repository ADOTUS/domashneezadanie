using DomashneeZadanie.Core.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace DomashneeZadanie.BackgroundTasks
{
    public class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan _resetScenarioTimeout;
        private readonly IScenarioContextRepository _scenarioRepository;
        private readonly ITelegramBotClient _bot;

        public ResetScenarioBackgroundTask(
            TimeSpan resetScenarioTimeout,
            IScenarioContextRepository scenarioRepository,
            ITelegramBotClient bot)
            : base(TimeSpan.FromMinutes(1), nameof(ResetScenarioBackgroundTask))
        {
            _resetScenarioTimeout = resetScenarioTimeout;
            _scenarioRepository = scenarioRepository;
            _bot = bot;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var contexts = await _scenarioRepository.GetContexts(ct);
            var now = DateTime.UtcNow;

            foreach (var context in contexts)
            {
                var createdAt = context.CreatedAt;

                if (now - createdAt >= _resetScenarioTimeout)
                {
                    await _scenarioRepository.ResetContext(context.UserId, ct);

                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[] { "/addtask", "/show", "/report" }
                })
                    {
                        ResizeKeyboard = true
                    };

                    await _bot.SendMessage(
                        chatId: context.UserId,
                        text: $"Сценарий отменен, так как нет активности в течение {_resetScenarioTimeout}",
                        replyMarkup: keyboard,
                        cancellationToken: ct);
                }
            }
        }
    }
}
