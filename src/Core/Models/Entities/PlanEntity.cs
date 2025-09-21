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
    int plan_id = 0,
    int usr_id = 0,
    int org_id = 0,
    DateTime start_date = default,
    DateTime end_date = default,
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);
