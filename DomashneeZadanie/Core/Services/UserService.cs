using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using Otus.ToDoList.ConsoleBot.Types;

namespace DomashneeZadanie.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ToDoUser?> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
        if (existingUser != null)
            return existingUser;

        var newUser = new ToDoUser
        {
            UserId = Guid.NewGuid(),
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUserName,
            RegisteredAt = DateTime.Now
        };

        await _userRepository.Add(newUser,cancellationToken);
        return newUser;
    }
    public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
    }
}