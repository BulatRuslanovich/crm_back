namespace CrmBack.Core.Models.Entities;

public record UserEntity(
    int usr_id,
    string? first_name = null,
    string? middle_name = null,
    string? last_name = null,
    string login = "",
    string? password_hash = null,
    bool? is_deleted = null
);