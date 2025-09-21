namespace CrmBack.Core.Models.Entities;

public record ActivEntity(
    int activ_id,
    int? usr_id = null,
    int? org_id = null,
    int? status_id = null,
    DateTime? visit_date = null,
    TimeSpan? start_time = null,
    TimeSpan? end_time = null,
    string? description = null,
    DateTime? created_at = null,
    DateTime? updated_at = null,
    string? created_by = null,
    string? updated_by = null,
    bool? is_deleted = null
);

