namespace CrmBack.Core.Models.Entities;

public record UserEntity(
    int usr_id,
    string name,
    string login,
    DateTime created_at = default,
    DateTime updated_at = default,
    bool is_deleted = false
);