using CrmBack.Core.Attributes;

namespace CrmBack.Core.Models.Entities;

[Table("policy")]
public record PolicyEntity(
    [Column(IsKey = true, IsUpdatable = false)] int policy_id,
    [Column] string policy_name,
    [Column(IsInsertable = false, IsUpdatable = false)] bool is_deleted
);