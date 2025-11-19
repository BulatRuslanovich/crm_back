CREATE TABLE usr
(
    usr_id        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_firstname    VARCHAR(100),
    usr_lastname     VARCHAR(100),
    usr_login         VARCHAR(100) NOT NULL UNIQUE,
    usr_pass VARCHAR(255) NOT NULL,
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

CREATE TABLE usrpolicy
(
    usrpolicy_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_id        INT NOT NULL REFERENCES usr (usr_id),
    policy_id     INT NOT NULL REFERENCES policy (policy_id),
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (usr_id, policy_id)
);

CREATE TABLE org
(
    org_id     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    org_name       VARCHAR(255) NOT NULL,
    org_inn        VARCHAR(12) UNIQUE,
    org_latitude   DOUBLE PRECISION,
    org_longitude  DOUBLE PRECISION,
    org_address    TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN   DEFAULT FALSE
);

CREATE TABLE status
(
    status_id  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    status_name       VARCHAR(50) NOT NULL UNIQUE,
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
    activ_date  DATE NOT NULL,
    activ_starttime  TIME,
    activ_endtime    TIME,
    activ_description TEXT,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted  BOOLEAN   DEFAULT FALSE
);


CREATE TABLE refresh
(
    refresh_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_id     INT          NOT NULL REFERENCES usr (usr_id) ON DELETE CASCADE,
    refresh_token_hash VARCHAR(255) NOT NULL UNIQUE,
    refresh_expires_at TIMESTAMP    NOT NULL,
    created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN      DEFAULT FALSE
);


INSERT INTO status (status_name)
VALUES ('Запланирован'),
       ('Открыт'),
       ('Сохранен'),
       ('Закрыт');

INSERT INTO usr (usr_firstname, usr_lastname, usr_login, usr_pass) VALUES
('Булат', 'Бикмухаметов', 'bulat', '$2a$11$xi3EwTpQov3A9kYsM1UsveTjr1ZScZKQzjSBMQQXOOKbgPv6R5MjC');

INSERT INTO policy (policy_name) VALUES
('Admin'),
('Director'),
('Manager'),
('Representative');

INSERT INTO usrpolicy (usr_id, policy_id) VALUES
(1, 1),
(1, 2),
(1, 3),
(1, 4);



