namespace CrmBack.Core.Models.Entities;

public record PolicyEntity(
    int policy_id = 0,
    string policy_name = "-",
    bool is_deleted = false
);