using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("plan")]
public class PlanEntity
{
    [Key]
    [Column("plan_id")]
    public int PlanId { get; set; }

    [Column("usr_id")]
    [Required]
    public int UsrId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("start_date", TypeName = "date")]
    [Required]
    public DateTime StartDate { get; set; }

    [Column("end_date", TypeName = "date")]
    [Required]
    public DateTime EndDate { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("UsrId")]
    public virtual UserEntity User { get; set; } = null!;

    [ForeignKey("OrgId")]
    public virtual OrgEntity Organization { get; set; } = null!;
}
