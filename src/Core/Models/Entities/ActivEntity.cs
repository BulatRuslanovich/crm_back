using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("activ")]
public class ActivEntity
{
    [Key]
    [Column("activ_id")]
    public int ActivId { get; set; }

    [Column("usr_id")]
    [Required]
    public int UsrId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("status_id")]
    [Required]
    public int StatusId { get; set; }

    [Column("visit_date", TypeName = "date")]
    [Required]
    public DateTime VisitDate { get; set; }

    [Column("start_time", TypeName = "time")]
    public TimeSpan StartTime { get; set; }

    [Column("end_time", TypeName = "time")]
    public TimeSpan EndTime { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("UsrId")]
    public virtual UserEntity User { get; set; } = null!;

    [ForeignKey("OrgId")]
    public virtual OrgEntity Organization { get; set; } = null!;

    [ForeignKey("StatusId")]
    public virtual StatusEntity Status { get; set; } = null!;
}
