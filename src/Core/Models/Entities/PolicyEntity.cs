namespace CrmBack.Core.Models.Entities;

public record PolicyEntity(
    int policy_id = 0,
    string policy_name = "-",
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);

