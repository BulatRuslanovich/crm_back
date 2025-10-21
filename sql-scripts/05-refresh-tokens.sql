-- Добавляем таблицу для refresh токенов
CREATE TABLE refresh_tokens
(
    token_id     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usr_id       INT NOT NULL REFERENCES usr (usr_id) ON DELETE CASCADE,
    token_hash   VARCHAR(255) NOT NULL UNIQUE,
    expires_at   TIMESTAMP NOT NULL,
    created_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_revoked   BOOLEAN DEFAULT FALSE
);

-- Индекс для быстрого поиска по пользователю
CREATE INDEX idx_refresh_tokens_usr_id ON refresh_tokens(usr_id);
CREATE INDEX idx_refresh_tokens_token_hash ON refresh_tokens(token_hash);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);
