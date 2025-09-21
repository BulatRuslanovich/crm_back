namespace CrmBack.Core.Models.Entities;

public record ActivEntity(
    int activ_id = 0,
    int usr_id = 0,
    int org_id = 0,
    int status_id = 0,
    DateTime visit_date = default,
    TimeOnly start_time = default,
    TimeOnly end_time = default,
    string description = "-",
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);

