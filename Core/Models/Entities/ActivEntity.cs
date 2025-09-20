namespace CrmBack.Core.Models.Entities;

// CREATE TABLE activ (
//     activ_id     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
//     usr_id       INT NOT NULL REFERENCES usr(usr_id),
//     org_id       INT NOT NULL REFERENCES org(org_id),
//     status_id    INT NOT NULL REFERENCES status(status_id),
//     visit_date   DATE NOT NULL,
//     start_time   TIME,
//     end_time     TIME,
//     description  TEXT,
//     created_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     updated_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
//     created_by   VARCHAR(100),
//     updated_by   VARCHAR(100),
//     is_deleted   BOOLEAN DEFAULT FALSE
// );
public record ActivEntity(
    int activ_id,
    int usr_id,
    int org_id,
    int status_id,
    DateTime visit_date,
    TimeOnly start_time,
    TimeOnly end_time,
    string description,
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);

