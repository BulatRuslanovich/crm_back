using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("status")]
public class StatusEntity
{
    [Key]
    [Column("status_id")]
    public int StatusId { get; set; }

    [Column("name")]
    [MaxLength(50)]
    [Required]
    public string Name { get; set; } = string.Empty;

    // [Column("created_at")]
    // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // [Column("updated_at")]
    // public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // [Column("is_deleted")]
    // public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<ActivEntity> Activities { get; set; } = [];
}
