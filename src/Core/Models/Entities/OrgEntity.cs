namespace CrmBack.Core.Models.Entities;

public record OrgEntity(
    int org_id,
    string name,
    string inn,
    double latitude,
    double longitude,
    string address,
    bool is_deleted
);