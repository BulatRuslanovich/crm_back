using CrmBack.Core.Attributes;

namespace CrmBack.Core.Models.Entities;

[Table("activ")]
public record ActivEntity(
    [Column(IsKey = true, IsUpdatable = false)] int activ_id,
    [Column] int usr_id,
    [Column] int org_id,
    [Column] int status_id,
    [Column] DateTime visit_date,
    [Column] TimeSpan start_time,
    [Column] TimeSpan end_time,
    [Column] string description,
    [Column(IsInsertable = false, IsUpdatable = false)] bool is_deleted
);