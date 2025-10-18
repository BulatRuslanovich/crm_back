using CrmBack.Core.Utils.Attributes;

namespace CrmBack.Core.Models.Entities;

[Table("usr")]
public record UserEntity(
    [Column(IsKey = true, IsUpdatable = false)] int usr_id,
    [Column] string first_name,
    [Column] string middle_name,
    [Column] string last_name,
    [Column] string login,
    [Column] string password_hash,
    [Column(IsInsertable = false, IsUpdatable = false)] bool is_deleted
);
