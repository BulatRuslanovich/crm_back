namespace CrmBack.Core.Models.Entities;

// CREATE TABLE usr (
//     usr_id        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
//     first_name    VARCHAR(100),
//     middle_name   VARCHAR(100),
//     last_name     VARCHAR(100),
//     login         VARCHAR(100) NOT NULL UNIQUE,
//     password_hash VARCHAR(255) NOT NULL,
//     created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     created_by    VARCHAR(100),
//     updated_by    VARCHAR(100),
//     is_deleted    BOOLEAN DEFAULT FALSE
// );

public record UserEntity(
    int usr_id = 0,
    string first_name = "-",
    string middle_name = "-",
    string last_name = "-",
    string login = "-",
    string password_hash = "-",
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);