using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

[Table("policy")]
public class PolicyEntity
{
    [Key]
    [Column("policy_id")]
    public int PolicyId { get; set; }

    [Column("policy_name")]
    [MaxLength(100)]
    [Required]
    public string PolicyName { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<UserPolicyEntity> UserPolicies { get; set; } = [];
}
