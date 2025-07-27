namespace DomashneeZadanie.Core.Entities;

public class ToDoUser
{
    public Guid UserId { set; get; }
    public long TelegramUserId { set; get; }
    public string? TelegramUserName { set; get; }
    public DateTime RegisteredAt { set; get; }
    //public ToDoUser() { }
     
    //public ToDoUser(Guid userId, long telegramUserId, string? telegramUserName = null)
    //{
    //    UserId = userId;
    //    TelegramUserId = telegramUserId;
    //    TelegramUserName = telegramUserName;
    //    RegisteredAt = DateTime.UtcNow;
    //}
}
