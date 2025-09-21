namespace CrmBack.Core.Models.Entities;

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
