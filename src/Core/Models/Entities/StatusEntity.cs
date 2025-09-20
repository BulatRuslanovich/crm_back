namespace CrmBack.Core.Models.Entities;

// CREATE TABLE status (
//     status_id   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
//     name        VARCHAR(50) NOT NULL UNIQUE,
//     created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     created_by  VARCHAR(100),
//     updated_by  VARCHAR(100),
//     is_deleted  BOOLEAN DEFAULT FALSE
// );
public record StatusEntity(
    int status_id,
    string name,
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);

