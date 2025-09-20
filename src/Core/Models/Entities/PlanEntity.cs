namespace CrmBack.Core.Models.Entities;

// CREATE TABLE plan (
//     plan_id     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
//     usr_id      INT NOT NULL REFERENCES usr(usr_id),
//     org_id      INT NOT NULL REFERENCES org(org_id),
//     start_date  DATE NOT NULL,
//     end_date    DATE NOT NULL,
//     created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     created_by  VARCHAR(100),
//     updated_by  VARCHAR(100),
//     is_deleted  BOOLEAN DEFAULT FALSE,
//     CHECK (end_date >= start_date)
// );
public record PlanEntity(
    int plan_id,
    int usr_id,
    int org_id,
    int status_id,
    DateTime start_date,
    DateTime end_date,
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);
