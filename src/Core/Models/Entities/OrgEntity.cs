namespace CrmBack.Core.Models.Entities;

// CREATE TABLE org (
//     org_id      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
//     name        VARCHAR(255) NOT NULL,
//     inn         VARCHAR(12) UNIQUE,
//     latitude    DOUBLE PRECISION,
//     longitude   DOUBLE PRECISION,
//     address     TEXT,
//     created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     created_by  VARCHAR(100),
//     updated_by  VARCHAR(100),
//     is_deleted  BOOLEAN DEFAULT FALSE
// );
public record OrgEntity(
    int org_id = 0,
    string name = "-",
    string inn = "-",
    double latitude = 0,
    double longitude = 0,
    string address = "-",
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);

