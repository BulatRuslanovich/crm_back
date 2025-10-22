using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("org")]
public class OrgEntity
{
    [Key]
    [Column("org_id")]
    public int OrgId { get; set; }

    [Column("name")]
    [MaxLength(255)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("inn")]
    [MaxLength(12)]
    public string? Inn { get; set; } = string.Empty;

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<ActivEntity> Activities { get; set; } = [];
    public virtual ICollection<PlanEntity> Plans { get; set; } = [];
}
