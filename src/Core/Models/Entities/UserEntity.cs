namespace CrmBack.Core.Models.Entities;

public record UserEntity(
    int usr_id,
    string first_name,
    string middle_name,
    string last_name,
    string login,
    string password_hash,
    bool is_deleted
);