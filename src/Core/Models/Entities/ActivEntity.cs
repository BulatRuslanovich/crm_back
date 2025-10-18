namespace CrmBack.Core.Models.Entities;

public record ActivEntity(
    int activ_id,
    int usr_id,
    int org_id,
    int status_id,
    DateTime visit_date,
    TimeSpan start_time,
    TimeSpan end_time,
    string description,
    bool is_deleted
);