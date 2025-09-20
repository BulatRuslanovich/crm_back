-- Пользователи системы
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
    created_by    VARCHAR(100),
    updated_by    VARCHAR(100),
    is_deleted    BOOLEAN   DEFAULT FALSE
);

-- Роли/политики
CREATE TABLE policy
(
    policy_id   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    policy_name VARCHAR(100) NOT NULL UNIQUE,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by  VARCHAR(100),
    updated_by  VARCHAR(100),
    is_deleted  BOOLEAN   DEFAULT FALSE
);

-- Связь многие-ко-многим между пользователями и политиками
CREATE TABLE usr_policy
(
    usr_policy_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_id        INT NOT NULL REFERENCES usr (usr_id),
    policy_id     INT NOT NULL REFERENCES policy (policy_id),
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by    VARCHAR(100),
    UNIQUE (usr_id, policy_id)
);

-- Организации
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
    created_by VARCHAR(100),
    updated_by VARCHAR(100),
    is_deleted BOOLEAN   DEFAULT FALSE
);

-- Статусы активностей
CREATE TABLE status
(
    status_id  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name       VARCHAR(50) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100),
    updated_by VARCHAR(100),
    is_deleted BOOLEAN   DEFAULT FALSE
);

-- Активности (визиты к организациям)
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
    created_by  VARCHAR(100),
    updated_by  VARCHAR(100),
    is_deleted  BOOLEAN   DEFAULT FALSE
);

-- Планы на период
CREATE TABLE plan
(
    plan_id    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_id     INT  NOT NULL REFERENCES usr (usr_id),
    org_id     INT  NOT NULL REFERENCES org (org_id),
    start_date DATE NOT NULL,
    end_date   DATE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100),
    updated_by VARCHAR(100),
    is_deleted BOOLEAN   DEFAULT FALSE,
    CHECK (end_date >= start_date)
);

-- Индексы для ускорения запросов
CREATE INDEX idx_usr_login ON usr (login);

-- Триггеры для автоматического обновления updated_at
CREATE OR REPLACE FUNCTION update_updated_at()
    RETURNS TRIGGER AS
$$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER usr_updated_at
    BEFORE UPDATE
    ON usr
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER policy_updated_at
    BEFORE UPDATE
    ON policy
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER org_updated_at
    BEFORE UPDATE
    ON org
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER status_updated_at
    BEFORE UPDATE
    ON status
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER activ_updated_at
    BEFORE UPDATE
    ON activ
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER plan_updated_at
    BEFORE UPDATE
    ON plan
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

INSERT INTO status (name, created_by, updated_by)
VALUES ('Запланирован', 'system', 'system'),
       ('Открыт', 'system', 'system'),
       ('Отменен', 'system', 'system'),
       ('Завершен', 'system', 'system');

