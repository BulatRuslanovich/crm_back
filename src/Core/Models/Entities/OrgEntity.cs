namespace CrmBack.Core.Models.Entities;

public record OrgEntity(
    int org_id,
    string? name = null,
    string? inn = null,
    double? latitude = null,
    double? longitude = null,
    string? address = null,
    DateTime? created_at = null,
    DateTime? updated_at = null,
    string? created_by = null,
    string? updated_by = null,
    bool? is_deleted = null
);

