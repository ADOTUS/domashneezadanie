-- Получить все задачи пользователя
SELECT * FROM "todoitem" it
JOIN "todouser" usr ON usr."userid" = it."userid";

-- Получить активные задачи пользователя (State = 'Active')
SELECT * FROM "todoitem" it
JOIN "todouser" usr ON usr."userid" = it."userid" AND it."state" = 'Active';

-- Поиск задач по имени 
SELECT * FROM "todoitem" it
JOIN "todouser" usr ON usr."userid" = it."userid" AND "name" ILIKE '%б%';

-- Получить задачу по Id
SELECT * FROM "todoitem" it
WHERE it."id" = '20000000-0000-0000-0000-000000000001';

-- Проверка существования задачи с именем
SELECT EXISTS (
    SELECT 1 FROM "todoitem" it
    WHERE it."userid" = '00000000-0000-0000-0000-000000000001' AND it."name" = '1'
);

-- Подсчитать количество активных задач пользователя
SELECT COUNT(*) FROM "todoitem"
WHERE "userid" = '00000000-0000-0000-0000-000000000001' AND "state" = 'Active';