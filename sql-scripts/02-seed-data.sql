
INSERT INTO usr (first_name, middle_name, last_name, login, password_hash) VALUES
('Булат', 'Русланович', 'Бикмухаметов', 'bulat', '$2a$11$xi3EwTpQov3A9kYsM1UsveTjr1ZScZKQzjSBMQQXOOKbgPv6R5MjC');

-- 2. Вставка политик (policy) - ровно 4 как указано
INSERT INTO policy (policy_name) VALUES
('Админ'),
('Руководитель'),
('Менеджер'),
('Представитель');

INSERT INTO usr_policy (usr_id, policy_id) VALUES
(1, 1);


