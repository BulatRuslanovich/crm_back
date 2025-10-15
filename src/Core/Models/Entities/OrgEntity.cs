namespace CrmBack.Core.Models.Entities;

public record OrgEntity(
    int org_id,
    string? name = null,
    string? inn = null,
    double? latitude = null,
    double? longitude = null,
    string? address = null,
    bool? is_deleted = null
);