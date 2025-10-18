namespace CrmBack.Core.Models.Entities;

public record PolicyEntity(
    int policy_id,
    string policy_name,
    bool is_deleted
);