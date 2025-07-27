--Удаление таблиц в прав пор
DROP TABLE IF EXISTS "ToDoItem";
DROP TABLE IF EXISTS "ToDoList";
DROP TABLE IF EXISTS "ToDoUser";

--пользователи
CREATE TABLE "ToDoUser" (
    "UserId" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL UNIQUE,
    "TelegramUserName" TEXT,
    "RegisteredAt" TIMESTAMP NOT NULL
);

CREATE TABLE "ToDoList" (
    "Id" UUID PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "UserId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    FOREIGN KEY ("UserId") REFERENCES "ToDoUser" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "ToDoItem" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Name" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "State" INT NOT NULL,
    "StateChangedAt" TIMESTAMP,
    "Deadline" TIMESTAMP NOT NULL,
    "ListId" UUID,
    FOREIGN KEY ("UserId") REFERENCES "ToDoUser" ("UserId") ON DELETE CASCADE,
    FOREIGN KEY ("ListId") REFERENCES "ToDoList" ("Id") ON DELETE SET NULL
);

CREATE UNIQUE INDEX "IX_ToDoUser_TelegramUserId" ON "ToDoUser" ("TelegramUserId");

CREATE INDEX "IX_ToDoList_UserId" ON "ToDoList" ("UserId");
CREATE INDEX "IX_ToDoItem_UserId" ON "ToDoItem" ("UserId");
CREATE INDEX "IX_ToDoItem_ListId" ON "ToDoItem" ("ListId");