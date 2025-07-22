--Удаление таблиц в прав пор
DROP TABLE IF EXISTS ToDoItem;
DROP TABLE IF EXISTS ToDoList;
DROP TABLE IF EXISTS ToDoUser;

--пользователи
CREATE TABLE ToDoUser (
    UserId UUID PRIMARY KEY,
    TelegramUserId BIGINT NOT NULL UNIQUE,
    TelegramUserName TEXT,
    RegisteredAt TIMESTAMP NOT NULL
);

--Списки
CREATE TABLE ToDoList (
    Id UUID PRIMARY KEY,
    Name TEXT NOT NULL,
    UserId UUID NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    CONSTRAINT FK_ToDoList_User FOREIGN KEY (UserId) REFERENCES ToDoUser(UserId) ON DELETE CASCADE
);

--Задачми
CREATE TABLE ToDoItem (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL,
    Name TEXT NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    State TEXT NOT NULL,
    StateChangedAt TIMESTAMP,
    Deadline TIMESTAMP NOT NULL,
    ListId UUID,
    CONSTRAINT FK_ToDoItem_User FOREIGN KEY (UserId) REFERENCES ToDoUser(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_ToDoItem_List FOREIGN KEY (ListId) REFERENCES ToDoList(Id) ON DELETE SET NULL
);

-- Индексы для внешних ключей
CREATE INDEX IDX_ToDoList_UserId ON ToDoList(UserId);
CREATE INDEX IDX_ToDoItem_UserId ON ToDoItem(UserId);
CREATE INDEX IDX_ToDoItem_ListId ON ToDoItem(ListId);

-- Уникальный индекс по TelegramUserId
CREATE UNIQUE INDEX IDX_ToDoUser_TelegramUserId ON ToDoUser(TelegramUserId);