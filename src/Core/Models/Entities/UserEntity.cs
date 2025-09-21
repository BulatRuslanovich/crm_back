namespace CrmBack.Core.Models.Entities;

public record UserEntity(
    int usr_id = 0,
    string first_name = "-",
    string middle_name = "-",
    string last_name = "-",
    string login = "-",
    string password_hash = "-",
    DateTime created_at = default,
    DateTime updated_at = default,
    string created_by = "system",
    string updated_by = "system",
    bool is_deleted = false
);