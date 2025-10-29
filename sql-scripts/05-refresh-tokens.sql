CREATE TABLE refresh (
    refresh_id SERIAL PRIMARY KEY,
    usr_id INTEGER NOT NULL REFERENCES usr(usr_id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE
);