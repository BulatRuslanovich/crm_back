namespace CrmBack.Core.Models.Entities;

public record PlanEntity(
    int plan_id,
    int usr_id,
    int org_id,
    DateTime start_date,
    DateTime end_date,
    bool is_deleted
);