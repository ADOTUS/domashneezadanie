using DomashneeZadanie.Core.DataAccess;
using DomashneeZadanie.Core.Entities;
using DomashneeZadanie.Infrastructure.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DomashneeZadanie.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            return new ToDoUser
            {
                UserId = model.UserId,
                TelegramUserId = model.TelegramUserId,
                TelegramUserName = model.TelegramUserName,
                RegisteredAt = model.RegisteredAt
            };
        }

        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            return new ToDoUserModel
            {
                UserId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                TelegramUserName = entity.TelegramUserName,
                RegisteredAt = entity.RegisteredAt
            };
        }

        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            return new ToDoItem
            {
                Id = model.Id,
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                State = (ToDoItemState)model.State,
                StateChangedAt = model.StateChangedAt,
                Deadline = model.Deadline,
                User = model.User != null ? MapFromModel(model.User) : null!,
                List = model.List != null ? MapFromModel(model.List) : null
            };
        }

        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            return new ToDoItemModel
            {
                Id = entity.Id,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                State = (int)entity.State,
                StateChangedAt = entity.StateChangedAt,
                Deadline = entity.Deadline,
                UserId = entity.User.UserId,
                ListId = entity.List?.Id
            };
        }

        public static ToDoList MapFromModel(ToDoListModel model)
        {
            return new ToDoList
            {
                Id = model.Id,
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                User = model.User != null ? MapFromModel(model.User) : null!
            };
        }

        public static ToDoListModel MapToModel(ToDoList entity)
        {
            return new ToDoListModel
            {
                Id = entity.Id,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                UserId = entity.User.UserId
            };
        }
        public static Notification MapFromModel(NotificationModel model)
        {
            return new Notification
            {
                Id = model.Id,
                User = new ToDoUser { UserId = model.UserId },
                Type = model.Type,
                Text = model.Text,
                ScheduledAt = model.ScheduledAt,
                IsNotified = model.IsNotified,
                NotifiedAt = model.NotifiedAt
            };
        }
        public static NotificationModel MapToModel(Notification entity)
        {
            return new NotificationModel
            {
                Id = entity.Id,
                UserId = entity.User.UserId,
                Type = entity.Type,
                Text = entity.Text,
                ScheduledAt = entity.ScheduledAt,
                IsNotified = entity.IsNotified,
                NotifiedAt = entity.NotifiedAt
            };
        }
    }

}
