using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DomashneeZadanie.Core.Services
{
    public static class UpdateExtensions
    {
        public static long GetChatId(this Update update)
        {
            return update.CallbackQuery?.Message?.Chat.Id
                ?? update.Message?.Chat.Id
                ?? throw new InvalidOperationException("Чат не найден");
        }
    }
}
