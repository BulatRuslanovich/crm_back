
INSERT INTO usr (first_name, middle_name, last_name, login, password_hash) VALUES
('Булат', 'Русланович', 'Бикмухаметов', 'bulat', '$2a$11$xi3EwTpQov3A9kYsM1UsveTjr1ZScZKQzjSBMQQXOOKbgPv6R5MjC');

INSERT INTO policy (policy_name) VALUES
('Admin'),
('Director'),
('Manager'),
('Representative');

INSERT INTO usr_policy (usr_id, policy_id) VALUES
(1, 1);


