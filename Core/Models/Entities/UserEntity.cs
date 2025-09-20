namespace CrmBack.Core.Models.Entities;

public record UserEntity(
    int usr_id,
    string name,
    string login,
    DateTime created_at,
    DateTime updated_at,
    bool is_deleted
);