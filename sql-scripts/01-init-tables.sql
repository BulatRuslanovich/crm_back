CREATE TABLE usr
(
    usr_id        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    first_name    VARCHAR(100),
    middle_name   VARCHAR(100),
    last_name     VARCHAR(100),
    login         VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted    BOOLEAN   DEFAULT FALSE
);

CREATE TABLE policy
(
    policy_id   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    policy_name VARCHAR(100) NOT NULL UNIQUE,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted  BOOLEAN   DEFAULT FALSE
);

CREATE TABLE usr_policy
(
    usr_policy_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_id        INT NOT NULL REFERENCES usr (usr_id),
    policy_id     INT NOT NULL REFERENCES policy (policy_id),
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (usr_id, policy_id)
);

CREATE TABLE org
(
    org_id     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name       VARCHAR(255) NOT NULL,
    inn        VARCHAR(12) UNIQUE,
    latitude   DOUBLE PRECISION,
    longitude  DOUBLE PRECISION,
    address    TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN   DEFAULT FALSE
);

CREATE TABLE status
(
    status_id  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name       VARCHAR(50) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN   DEFAULT FALSE
);

CREATE TABLE activ
(
    activ_id    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_id      INT  NOT NULL REFERENCES usr (usr_id),
    org_id      INT  NOT NULL REFERENCES org (org_id),
    status_id   INT  NOT NULL REFERENCES status (status_id),
    visit_date  DATE NOT NULL,
    start_time  TIME,
    end_time    TIME,
    description TEXT,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted  BOOLEAN   DEFAULT FALSE
);


CREATE TABLE refresh
(
    refresh_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_id     INT          NOT NULL REFERENCES usr (usr_id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP    NOT NULL,
    created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN      DEFAULT FALSE
);


INSERT INTO status (name)
VALUES (1, 'Запланирован'),
       (2, 'Открыт'),
       (3, 'Сохранен'),
       (4, 'Закрыт');

INSERT INTO usr (first_name, middle_name, last_name, login, password_hash) VALUES
('Булат', 'Русланович', 'Бикмухаметов', 'bulat', '$2a$11$xi3EwTpQov3A9kYsM1UsveTjr1ZScZKQzjSBMQQXOOKbgPv6R5MjC');

INSERT INTO policy (policy_name) VALUES
(1, 'Admin'),
(2, 'Director'),
(3, 'Manager'),
(4, 'Representative');

INSERT INTO usr_policy (usr_id, policy_id) VALUES
(1, 1),
(1, 2),
(1, 3),
(1, 4);



