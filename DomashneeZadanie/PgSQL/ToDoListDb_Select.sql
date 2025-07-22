Вариант1. Без кавычек.
-- Получить все задачи пользователя
SELECT * FROM ToDoItem
WHERE UserId = '...';

-- Получить активные задачи пользователя (State = 'Active')
SELECT * FROM ToDoItem
WHERE UserId = '...' AND State = 'Active';

-- Поиск задач по имени
SELECT * FROM ToDoItem
WHERE UserId = '...' AND Name ILIKE '%маска%';

-- Получить задачу по Id
SELECT * FROM ToDoItem
WHERE Id = '...';

-- Проверка существования задачи с именем
SELECT EXISTS (
    SELECT 1 FROM ToDoItem
    WHERE UserId = '...' AND Name = '...'
);

-- Подсчитать количество активных задач пользователя
SELECT COUNT(*) FROM ToDoItem
WHERE UserId = '...' AND State = 0;




Вариант2. С кавычками. Постгри не выполняет так. Вероятно я не так понял задачу. Вот то, что не работает, но по тз:

-- Получить все задачи пользователя
SELECT * FROM "ToDoItem"
WHERE "UserId" = '...';

-- Получить активные задачи пользователя (State = 'Active')
SELECT * FROM "ToDoItem"
WHERE "UserId" = '...' AND "State" = 0;

-- Поиск задач по имени 
SELECT * FROM "ToDoItem"
WHERE "UserId" = '...' AND "Name" ILIKE '%поиск%';

-- Получить задачу по Id
SELECT * FROM "ToDoItem"
WHERE "Id" = '...';

-- Проверка существования задачи с именем
SELECT EXISTS (
    SELECT 1 FROM "ToDoItem"
    WHERE "UserId" = '...' AND "Name" = '...'
);

-- Подсчитать количество активных задач пользователя
SELECT COUNT(*) FROM "ToDoItem"
WHERE "UserId" = '...' AND "State" = 0;