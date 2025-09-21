namespace CrmBack.Core.Models.Entities;

public record StatusEntity(
    int status_id = 0,
    string name = "-",
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);

