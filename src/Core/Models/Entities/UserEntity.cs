namespace CrmBack.Core.Models.Entities;

public record UserEntity(
    int usr_id,
    string? first_name = null,
    string? middle_name = null,
    string? last_name = null,
    string? login = null,
    string? password_hash = null,
    DateTime? created_at = null,
    DateTime? updated_at = null,
    string? created_by = null,
    string? updated_by = null,
    bool? is_deleted = null
);