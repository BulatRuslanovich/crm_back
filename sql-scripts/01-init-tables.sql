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


CREATE INDEX idx_usr_login ON usr (login);
CREATE INDEX idx_usr_active ON usr (usr_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_usr_name_search ON usr (last_name, first_name) WHERE is_deleted = FALSE;
CREATE INDEX idx_usr_fts ON usr USING gin (
    (LOWER(first_name || ' ' || COALESCE(last_name, '') || ' ' || login)) gin_trgm_ops
) WHERE is_deleted = FALSE;


CREATE INDEX idx_org_active ON org (org_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_org_name_sort ON org (name) WHERE is_deleted = FALSE;
CREATE INDEX idx_org_fts ON org USING gin (
    (LOWER(COALESCE(name, '') || ' ' || COALESCE(inn, '') || ' ' || COALESCE(address, ''))) gin_trgm_ops
) WHERE is_deleted = FALSE;

CREATE INDEX idx_status_active ON status (status_id) WHERE is_deleted = FALSE;

CREATE INDEX idx_activ_usr_id ON activ (usr_id);
CREATE INDEX idx_activ_org_id ON activ (org_id);
CREATE INDEX idx_activ_status_id ON activ (status_id);
CREATE INDEX idx_activ_active ON activ (activ_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_activ_usr_visit_date ON activ (usr_id, visit_date DESC) WHERE is_deleted = FALSE;
CREATE INDEX idx_activ_created_at ON activ (created_at DESC) WHERE is_deleted = FALSE;

CREATE INDEX idx_plan_usr_id ON plan (usr_id);
CREATE INDEX idx_plan_org_id ON plan (org_id);
CREATE INDEX idx_plan_active ON plan (plan_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_plan_usr_org_check ON plan (usr_id, org_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_plan_start_date ON plan (start_date DESC) WHERE is_deleted = FALSE;
CREATE INDEX idx_plan_usr_org_join ON plan (usr_id, org_id, start_date DESC) WHERE is_deleted = FALSE;

CREATE INDEX idx_refresh_usr_id ON refresh (usr_id);
CREATE INDEX idx_refresh_token_hash ON refresh (token_hash);
CREATE INDEX idx_refresh_valid_tokens ON refresh (usr_id, expires_at DESC) 
    WHERE is_deleted = FALSE;
CREATE INDEX idx_refresh_expires_at ON refresh (expires_at) WHERE is_deleted = FALSE;

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


INSERT INTO status (name)
VALUES ('Запланирован'),
       ('Открыт'),
       ('Сохранен'),
       ('Закрыт');
       

