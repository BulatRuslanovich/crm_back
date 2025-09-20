namespace CrmBack.Core.Models.Entities;

// CREATE TABLE policy (
//     policy_id   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
//     policy_name VARCHAR(100) NOT NULL UNIQUE,
//     created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     created_by  VARCHAR(100),
//     updated_by  VARCHAR(100),
//     is_deleted  BOOLEAN DEFAULT FALSE
// );
public record PolicyEntity(
    int policy_id,
    string policy_name,
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);

