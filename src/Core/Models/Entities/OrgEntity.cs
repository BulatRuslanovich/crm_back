namespace CrmBack.Core.Models.Entities;

public record OrgEntity(
    int org_id = 0,
    string name = "-",
    string inn = "-",
    double latitude = 0,
    double longitude = 0,
    string address = "-",
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);

