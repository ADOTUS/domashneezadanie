using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using Otus.ToDoList.ConsoleBot.Types;

namespace DomashneeZadanie.Core.Services;

public class UserService : IUserService
{
    //private readonly List<ToDoUser> _users = new();
    //public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    //{
    //    foreach (var user in _users)
    //    {
    //        if (user.TelegramUserId == telegramUserId)
    //        {
    //            return user;
    //        }
    //    }
    //    var newUser = new ToDoUser
    //    {
    //        UserId = Guid.NewGuid(),
    //        TelegramUserId = telegramUserId,
    //        TelegramUserName = telegramUserName,
    //        RegisteredAt = DateTime.UtcNow
    //    };
    //    _users.Add(newUser);
    //    return newUser;
    //}
    //public ToDoUser? GetUser(long telegramUserId)
    //{
    //    foreach (var user in _users)
    //    {
    //        if (user.TelegramUserId == telegramUserId)
    //        {
    //            return user;
    //        }
    //    }

    //    return null;
    //}
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