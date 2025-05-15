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

    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        var existingUser = _userRepository.GetUserByTelegramUserId(telegramUserId);
        if (existingUser != null)
            return existingUser;

        var newUser = new ToDoUser
        {
            UserId = Guid.NewGuid(),
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUserName,
            RegisteredAt = DateTime.Now
        };

        _userRepository.Add(newUser);
        return newUser;
    }
    public ToDoUser? GetUser(long telegramUserId)
    {
        return _userRepository.GetUserByTelegramUserId(telegramUserId);
    }
}