using CrmBack.Core.Utils.Attributes;

namespace CrmBack.Core.Models.Entities;

[Table("org")]
public record OrgEntity(
    [Column(IsKey = true, IsUpdatable = false)] int org_id,
    [Column] string name,
    [Column] string inn,
    [Column] double latitude,
    [Column] double longitude,
    [Column] string address,
    [Column(IsInsertable = false, IsUpdatable = false)] bool is_deleted
);
