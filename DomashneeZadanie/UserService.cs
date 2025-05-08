using Otus.ToDoList.ConsoleBot.Types;

namespace DomashneeZadanie;
//ƒобавление класса сервиса UserService
//ƒобавить интерфейс IUserService
//interface IUserService
//{
//    ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
//    ToDoUser? GetUser(long telegramUserId);
//}
//—оздать класс UserService, который реализует интерфейс IUserService.«аполн€ть telegramUserId и telegramUserName нужно из значений Update.Message.From
//ƒобавить использование IUserService в UpdateHandler. ѕолучать IUserService нужно через конструктор
//ѕри команде /start нужно вызвать метод IUserService.RegisterUser.
//≈сли пользователь не зарегистрирован, то ему доступны только команды /help /info

public class UserService : IUserService
{
    //внутренний список всех зарегистрированных пользователей
    private readonly List<ToDoUser> _users = new();

    //метод регистрации пользовател€
    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        //проверка зарегистрирован ли пользоватль
        foreach (var user in _users)
        {
            if (user.TelegramUserId == telegramUserId)
            {
                // если пользователь найдн
                return user;
            }
        }
        // создание нового пользовател€ объекта
        var newUser = new ToDoUser
        {
            UserId = Guid.NewGuid(),
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUserName,
            RegisteredAt = DateTime.UtcNow
        };
        // добавлен€ем пользовател€ в список
        _users.Add(newUser);
        //возврат пользватл€
        return newUser;
    }

    // ћетод дл€ получени€ пользовател€ по TelegramUserId если зарегистрирова
    public ToDoUser? GetUser(long telegramUserId)
    {
        foreach (var user in _users)
        {
            if (user.TelegramUserId == telegramUserId)
            {
                return user; //найден
            }
        }

        return null; //не найден
    }
    
    
}