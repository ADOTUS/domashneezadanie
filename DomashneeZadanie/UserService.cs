using Otus.ToDoList.ConsoleBot.Types;

namespace DomashneeZadanie;
//���������� ������ ������� UserService
//�������� ��������� IUserService
//interface IUserService
//{
//    ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
//    ToDoUser? GetUser(long telegramUserId);
//}
//������� ����� UserService, ������� ��������� ��������� IUserService.��������� telegramUserId � telegramUserName ����� �� �������� Update.Message.From
//�������� ������������� IUserService � UpdateHandler. �������� IUserService ����� ����� �����������
//��� ������� /start ����� ������� ����� IUserService.RegisterUser.
//���� ������������ �� ���������������, �� ��� �������� ������ ������� /help /info

public class UserService : IUserService
{
    //���������� ������ ���� ������������������ �������������
    private readonly List<ToDoUser> _users = new();

    //����� ����������� ������������
    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        //�������� ��������������� �� �����������
        foreach (var user in _users)
        {
            if (user.TelegramUserId == telegramUserId)
            {
                // ���� ������������ �����
                return user;
            }
        }
        // �������� ������ ������������ �������
        var newUser = new ToDoUser
        {
            UserId = Guid.NewGuid(),
            TelegramUserId = telegramUserId,
            TelegramUserName = telegramUserName,
            RegisteredAt = DateTime.UtcNow
        };
        // ����������� ������������ � ������
        _users.Add(newUser);
        //������� ����������
        return newUser;
    }

    // ����� ��� ��������� ������������ �� TelegramUserId ���� ��������������
    public ToDoUser? GetUser(long telegramUserId)
    {
        foreach (var user in _users)
        {
            if (user.TelegramUserId == telegramUserId)
            {
                return user; //������
            }
        }

        return null; //�� ������
    }
    
    
}