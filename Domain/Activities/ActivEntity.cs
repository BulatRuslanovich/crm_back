using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CrmBack.Domain.Common;
using CrmBack.Domain.Organizations;
using CrmBack.Domain.Users;

namespace CrmBack.Domain.Activities;

[Table("activ")]
public class ActivEntity : BaseEntity
{
    [Key]
    [Column("activ_id")]
    public int ActivId { get; set; }

    [Column("usr_id")]
    public int UsrId { get; set; }

    [Column("org_id")]
    public int OrgId { get; set; }

    [Column("status_id")]
    public int StatusId { get; set; }

    [Column("visit_date", TypeName = "date")]
    public DateTime VisitDate { get; set; }

    [Column("start_time", TypeName = "time")]
    public TimeSpan StartTime { get; set; }

    [Column("end_time", TypeName = "time")]
    public TimeSpan EndTime { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [ForeignKey("UsrId")]
    public virtual UserEntity User { get; set; } = null!;

    [ForeignKey("OrgId")]
    public virtual OrgEntity Organization { get; set; } = null!;

    [ForeignKey("StatusId")]
    public virtual StatusEntity Status { get; set; } = null!;
}
