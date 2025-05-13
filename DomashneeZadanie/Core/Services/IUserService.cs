using DomashneeZadanie.Core.Entities;

namespace DomashneeZadanie.Core.Services;

public interface IUserService
{
    ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
    ToDoUser? GetUser(long telegramUserId);
}