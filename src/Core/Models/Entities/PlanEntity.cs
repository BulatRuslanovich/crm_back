namespace CrmBack.Core.Models.Entities;

public record PlanEntity(
    int plan_id = 0,
    int usr_id = 0,
    int org_id = 0,
    DateTime start_date = default,
    DateTime end_date = default,
    bool is_deleted = false
);