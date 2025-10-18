using CrmBack.Core.Utils.Attributes;

namespace CrmBack.Core.Models.Entities;

[Table("status")]
public record StatusEntity(
    [Column(IsKey = true, IsUpdatable = false)] int status_id,
    [Column] string name,
    [Column(IsInsertable = false, IsUpdatable = false)] bool is_deleted
);
