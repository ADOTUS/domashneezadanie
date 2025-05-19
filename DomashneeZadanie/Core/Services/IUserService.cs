using DomashneeZadanie.Core.Entities;

namespace DomashneeZadanie.Core.Services;

public interface IUserService
{
    Task<ToDoUser?> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken);
    Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken cancellationToken);
}