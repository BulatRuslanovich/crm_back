using CrmBack.Core.Utils.Attributes;

namespace CrmBack.Core.Models.Entities;

[Table("plan")]
public record PlanEntity(
    [Column(IsKey = true, IsUpdatable = false)] int plan_id,
    [Column] int usr_id,
    [Column] int org_id,
    [Column] DateTime start_date,
    [Column] DateTime end_date,
    [Column(IsInsertable = false, IsUpdatable = false)] bool is_deleted
);
