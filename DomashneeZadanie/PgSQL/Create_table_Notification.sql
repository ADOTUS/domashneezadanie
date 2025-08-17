CREATE TABLE "Notification" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Type" TEXT NOT NULL,
    "Text" TEXT NOT NULL,
    "ScheduledAt" TIMESTAMP NOT NULL,
    "IsNotified" BOOLEAN NOT NULL,
    "NotifiedAt" TIMESTAMP,
    FOREIGN KEY ("UserId") REFERENCES "ToDoUser"("UserId") ON DELETE CASCADE
);

CREATE INDEX "IX_Notification_UserId" ON "Notification" ("UserId");